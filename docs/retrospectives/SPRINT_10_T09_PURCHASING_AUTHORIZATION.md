# Sprint 10 T09 - Purchasing Dynamic Authorization Integration Result

## 1. Task

```
T09 - Purchasing Dynamic Authorization Integration
STATUS: COMPLETE
```

## 2. Source Findings

The supplied repository/source ZIP was inspected directly before implementation.

**T01–T08 infrastructure confirmed present:**
- Domain entities: Capability, AuthorizationGroup, AuthorizationGroupCapability, UserAuthorizationGroup
- Application abstractions: ICapabilityAuthorizationService, ICapabilityRepository, IAuthorizationGroupRepository
- Application service: CapabilityAuthorizationService (union semantics, IsEnabled check)
- EF Core configurations: All 4 authorization tables with proper keys, indexes, unique constraints, cascade delete
- Migration: CreateAuthorizationSchema exists
- Seed data: AuthorizationSeeder seeds capabilities (including 5 PurchaseOrder capabilities) and groups with capability assignments
- Web authorization chain: CapabilityRequirement → CapabilityAuthorizationHandler → ICapabilityAuthorizationService
- DI registrations: All authorization repositories, service, and handler registered

**Critical T05 gap identified:**
`IdentitySeeder.SeedAsync` created capabilities and groups but never assigned the default users (admin, manager, viewer) to their corresponding AuthorizationGroups. Without user→group assignments, `GetForUserAsync` returned empty and every `HasCapabilityAsync` call returned false. The entire T01–T08 authorization infrastructure was non-functional for all users.

**Purchasing authorization baseline:**
All 3 Purchasing PageModels (Index, Create, Details) had no `[Authorize]` attributes. They relied solely on the global `AuthorizeFolder("/")` for authenticated-access protection.

**Actual Purchasing application actions (confirmed from source):**
- View listing (Index GET) — GetPurchaseOrdersHandler
- View details (Details GET) — GetPurchaseOrderHandler
- Create PO (Create GET + POST) — CreatePurchaseOrderHandler
- Submit PO (Details OnPostSubmit) — SubmitPurchaseOrderHandler
- Approve PO (Details OnPostApprove) — ApprovePurchaseOrderHandler
- Receive items (Details OnPostReceive) — ReceivePurchaseOrderHandler

**Confirmed non-existent actions:**
- PurchaseOrder.Edit — not in source, not in seed catalog
- PurchaseOrder.Reject — not in source, not in seed catalog

## 3. Architecture Decision

T09 applies capability-backed authorization to Purchasing using the existing ASP.NET Core authorization infrastructure from T01–T08. No second authorization mechanism is introduced.

The authorization flow for page-level access:

```
[Authorize(Policy = "Capability:PurchaseOrder.View")]
    → Policy resolves to CapabilityRequirement("PurchaseOrder.View")
    → CapabilityAuthorizationHandler.HandleRequirementAsync
    → ICapabilityAuthorizationService.HasCapabilityAsync
    → ICapabilityRepository.GetByNameAsync + IAuthorizationGroupRepository.GetForUserAsync
    → Capability exists + IsEnabled + user's groups contain it → Succeed
```

The authorization flow for handler-level checks (Details POST handlers):

```
IAuthorizationService.AuthorizeAsync(User, null, "Capability:PurchaseOrder.Submit")
    → Same policy resolution chain as above
    → On failure: return Forbid(), Application handler never invoked
```

Class-level `[Authorize]` protects page access. Handler-level `IAuthorizationService` checks protect specific POST operations. No redundant handler-level View checks are added because the class-level View policy already protects the page.

## 4. Exact Files Changed

### File 1: `src/InventoryPlatform/InventoryPlatform.Infrastructure/Identity/IdentitySeeder.cs`
- **Purpose**: T05 implementation-gap correction — assign default users to AuthorizationGroups
- **Change**: Added `AssignUsersToGroupsAsync` method that looks up each default user by email, looks up the corresponding group by name, verifies the relationship does not already exist in the database, calls `group.AssignUser(user.Id)`, and persists via `SaveChangesAsync`. Idempotent — re-running does not create duplicates.

### File 2: `src/InventoryPlatform/InventoryPlatform.Web/Authorization/AuthorizationPolicies.cs`
- **Purpose**: Define compile-time capability and policy-name constants for Purchasing
- **Change**: Added nested `PurchaseOrder` class with 5 capability constants (View, Create, Submit, Approve, Receive) and 5 compile-time policy-name constants (ViewPolicy, CreatePolicy, SubmitPolicy, ApprovePolicy, ReceivePolicy). Existing `ForCapability()` method preserved.

### File 3: `src/InventoryPlatform/InventoryPlatform.Web/Extensions/ServiceCollectionExtensions.cs`
- **Purpose**: Register 5 capability authorization policies
- **Change**: Added 5 `AddCapabilityPolicy` calls in the `AddAuthorization` block for PurchaseOrder.View, Create, Submit, Approve, Receive. Existing role-based policies untouched.

### File 4: `src/InventoryPlatform/InventoryPlatform.Web/Pages/Purchasing/PurchaseOrders/Index.cshtml.cs`
- **Purpose**: Server-side capability authorization for PO listing
- **Change**: Added `[Authorize(Policy = AuthorizationPolicies.PurchaseOrder.ViewPolicy)]` at class level. Added using directives for Authorization and Authorization namespaces.

### File 5: `src/InventoryPlatform/InventoryPlatform.Web/Pages/Purchasing/PurchaseOrders/Create.cshtml.cs`
- **Purpose**: Server-side capability authorization for PO creation
- **Change**: Added `[Authorize(Policy = AuthorizationPolicies.PurchaseOrder.CreatePolicy)]` at class level. Protects both OnGet and OnPost handlers. Added using directives.

### File 6: `src/InventoryPlatform/InventoryPlatform.Web/Pages/Purchasing/PurchaseOrders/Details.cshtml.cs`
- **Purpose**: Server-side capability authorization for PO details and workflow actions
- **Change**: Added class-level `[Authorize(Policy = AuthorizationPolicies.PurchaseOrder.ViewPolicy)]`. Injected `IAuthorizationService`. Added capability checks via `IAuthorizationService.AuthorizeAsync` in OnPostSubmitAsync (Submit), OnPostApproveAsync (Approve), and OnPostReceiveAsync (Receive). Each returns `Forbid()` on failure before the Application handler is invoked. No redundant handler-level View checks.

## 5. Implementation

### T05 Gap Correction: User→Group Assignment

Modified `IdentitySeeder.SeedAsync` to call a new `AssignUsersToGroupsAsync` method after `AuthorizationSeeder.SeedAsync`. This method:
1. Iterates over the 3 default user-to-group mappings (reusing `IdentityConstants.Roles` names)
2. Looks up each user by email via `UserManager<ApplicationUser>.FindByEmailAsync`
3. Looks up each group by name via `context.AuthorizationGroups.SingleOrDefaultAsync`
4. Queries `context.UserAuthorizationGroups.AnyAsync` for existing relationship (database-level check)
5. If not present, calls `group.AssignUser(user.Id)` (domain method with in-memory idempotency guard)
6. Saves only if changes were made

### Capability Policy Registration

Used existing `AddCapabilityPolicy` extension to register 5 policies in `ServiceCollectionExtensions.AddAuthorization`. Policy names follow the existing `ForCapability()` convention: `"Capability:PurchaseOrder.{Action}"`.

### Page-Level Authorization

- **Index**: `[Authorize(Policy = "Capability:PurchaseOrder.View")]`
- **Create**: `[Authorize(Policy = "Capability:PurchaseOrder.Create")]` — protects both GET and POST
- **Details**: `[Authorize(Policy = "Capability:PurchaseOrder.View")]` — class-level for all handlers

### Handler-Level Authorization (Details page)

Each POST handler checks its specific capability via `IAuthorizationService.AuthorizeAsync(User, null, ForCapability(...))` before calling the Application handler. Failure returns `Forbid()`. The Application handler is never invoked when authorization fails.

## 6. Authorization Mapping

```
Capability             → Action          → Component                      → Enforcement Point
──────────────────────────────────────────────────────────────────────────────────────────────────
PurchaseOrder.View     → View listing    → Index.cshtml.cs OnGet          → [Authorize] class-level
PurchaseOrder.View     → View details    → Details.cshtml.cs OnGet        → [Authorize] class-level
PurchaseOrder.Create   → Create PO       → Create.cshtml.cs OnGet/OnPost  → [Authorize] class-level
PurchaseOrder.Submit   → Submit PO       → Details.cshtml.cs OnPostSubmit → IAuthorizationService in handler
PurchaseOrder.Approve  → Approve PO      → Details.cshtml.cs OnPostApprove → IAuthorizationService in handler
PurchaseOrder.Receive  → Receive items   → Details.cshtml.cs OnPostReceive → IAuthorizationService in handler
```

## 7. Verification

### Source Verification — VERIFIED
- All 6 changed files inspected after implementation
- `[Authorize]` attributes correctly reference compile-time policy-name constants
- `ForCapability()` used correctly in `AddCapabilityPolicy` registration (runtime context) and in `IAuthorizationService.AuthorizeAsync` calls (runtime context)
- Handler-level checks occur before Application handler invocation
- `Forbid()` returned on authorization failure
- User→group assignment queries database for existing relationships (idempotent)
- Existing `ForCapability()` method preserved
- No existing role-based policies modified
- No UI visibility changes made
- No migration created
- 5 capability names and 5 policy names confirmed unambiguous and matching seed catalog

### Build Verification — VERIFIED (post-implementation)
- **Baseline (pre-implementation)**: 0 errors, 28 pre-existing warnings
- **Post-implementation**: 0 errors, 26 pre-existing warnings
- T09 introduced zero new errors and zero new warnings

### Automated Test Verification — NOT APPLICABLE
- No automated test project exists in the repository.

### Database Verification — NOT VERIFIED
- Reason: No running SQL Server instance available in this environment.
- The `CreateAuthorizationSchema` migration already includes the authorization tables.
- Seed data modifications execute on application startup via `IdentitySeeder.SeedAsync`.
- Idempotent design verified through source inspection: database query for existing UserAuthorizationGroup records before insertion.

### Browser Verification — NOT VERIFIED
- Reason: No running application instance available in this environment.
- Full browser verification must be performed by the developer including: authorized access, unauthorized access, direct URL access, action buttons, multiple groups, capability removal, and administrator access.

### Server-Side Authorization Verification — NOT VERIFIED
- Reason: No running application instance available in this environment.
- Source inspection confirms the authorization chain is correctly wired. Runtime verification that unauthorized requests return AccessDenied/Forbid() must be confirmed by the developer.

### UI Verification — NOT VERIFIED
- Reason: T09 does not modify button/navigation visibility. UI capability visibility is exclusively T12 scope.
- Action button verification for T09 means confirming unauthorized operations are rejected server-side even if existing UI controls remain visible. Requires runtime testing.

## 8. Regression

No regression identified from source/build verification; runtime regression remains unverified until runtime testing is performed.

**Source-level analysis:**
- Existing `[Authorize]` attributes on Products, Categories, Suppliers, Customers, Units, InventoryTransactions, Dashboard, Administrator, Account, and Reports pages: NOT MODIFIED
- Existing role-based policies (Administrator, InventoryManagement, ViewInventory): NOT MODIFIED
- Global `AuthorizeFolder("/")` and anonymous-page exceptions: NOT MODIFIED
- ASP.NET Core Identity authentication: NOT MODIFIED
- Domain business rules (PurchaseOrder.Submit, Approve, Receive): NOT MODIFIED — remain authoritative for business state
- `ForCapability()` method: PRESERVED — remains compatible
- CapabilityAuthorizationHandler: NOT MODIFIED
- ICapabilityAuthorizationService: NOT MODIFIED

**Runtime regression**: Cannot be verified without a running application. Developer must perform runtime regression testing.

## 9. Documentation

### Updated
- `docs/retrospectives/SPRINT_10_T09_PURCHASING_AUTHORIZATION.md` — this document. Records actual implementation and actual verification status only.

### Not Updated
- `README.md` — T14 scope
- `PROJECT_STATUS.md` — T14 scope
- `FEATURES.md` — T14 scope
- `ROADMAP.md` — T14 scope
- `DESIGN_DECISIONS.md` — T14 scope
- `ENGINEERING_JOURNAL.md` — T14 scope

## 10. Commit Recommendations

### Implementation
```
feat(auth): add capability-backed authorization to Purchasing pages
```

### Documentation
```
docs(auth): document T09 Purchasing dynamic authorization integration
```

## 11. Deferred Work

- **UI visibility per capability** — Exclusively T12 scope. T09 did not change any button, navigation link, or menu visibility.
- **Role-based policy migration to capabilities** — T10 scope. Existing Administrator, InventoryManagement, ViewInventory policies remain untouched.
- **Authorization administration pages** — T11 scope.
- **Non-Purchasing module authorization** — T10+ scope.
- **PurchaseOrder.Edit and PurchaseOrder.Reject** — These actions do not exist. No capabilities created.
- **Active session capability refresh** — Not addressed in T09.
- **Runtime/browser/server-side authorization verification** — Requires running environment. Developer must perform.
- **Automated tests** — No test project exists. Future consideration.

## 12. Next Task

T10 — Existing Authorization Boundary Migration

## 13. STOP

T09 is complete. Do not start T10.
