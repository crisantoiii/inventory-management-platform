# Architecture Review

## Date

August 2026

## Scope

- Application Layer
- Infrastructure Layer
- Web Layer
- Purchasing Application Layer
- Purchasing Presentation Layer
- Account Management
- Identity and Authentication
- Two-Factor Authentication

## Findings

### Application

- Architecture validated.
- No major refactoring required.
- Purchasing Application handlers integrate cleanly with the existing Application architecture.
- Rich Domain Model successfully supports workflow-driven business processes.
- Account Management handlers integrate cleanly with the existing Application architecture.
- Existing Identity abstractions successfully support self-service account workflows.
- Account Management was implemented without introducing a separate application architecture.

### Infrastructure

- Minor improvements to IdentityService error handling.
- PurchaseOrderRepository successfully integrates with the existing repository and Unit of Work infrastructure.
- Purchase Order item loading was required to support calculated aggregate totals in list queries.
- Existing Identity infrastructure successfully supports Account Management workflows.
- ASP.NET Core Identity remains encapsulated behind the existing Identity service abstraction.
- Two-Factor Authentication uses the existing Identity infrastructure rather than introducing custom authentication infrastructure.
- Recovery-code management remains within the established Identity workflow.
- Overall architecture approved.

### Web

- Consistent CRUD implementation.
- Purchasing Presentation layer successfully integrated with Application handlers.
- Razor Pages support workflow-oriented business actions without directly accessing persistence infrastructure.
- Account Management Razor Pages integrate with Application handlers while preserving the existing Presentation boundaries.
- Account Management workflows do not directly access Identity framework infrastructure.
- Two-Factor Authentication setup and authentication challenge flows were successfully integrated into the existing Razor Pages authentication flow.
- Opportunity for small shared Razor partials remains.

## Purchasing Workflow Validation

The Purchasing workflow was successfully validated through the Presentation layer:

```text
Draft
  ↓ Submit
Submitted
  ↓ Approve
Approved
  ↓ Receive partial quantity
Receiving
  ↓ Receive remaining quantity
Completed
```

The workflow was verified through actual browser interactions and persisted database records.

## Account Management Validation

The Account Management vertical slice was successfully validated through actual browser workflows.

The implementation supports:

- User Profile
- Update Profile
- Phone Number Update
- Blank Phone Handling
- Change Password
- Forgot Password
- Reset Password
- Force Password Change
- Email Verification
- Two-Factor Authentication
- Recovery Code Authentication
- Recovery Code Regeneration
- Recovery Code Invalidation
- Disable 2FA

The resulting architecture remains:

```text
Razor Page
     ↓
Application Handler
     ↓
Identity Service Abstraction
     ↓
ASP.NET Core Identity
```

The implementation preserves the separation between:

```text
Administrative User Management
        ↓
Manage users

Account Management
        ↓
Manage authenticated user's own account

Authentication
        ↓
Authenticate the user
```

## Two-Factor Authentication Validation

Two-factor authentication was validated through:

- 2FA setup
- Authenticator-code verification
- 2FA login challenge
- Recovery-code login
- Recovery-code regeneration
- Recovery-code invalidation
- 2FA disablement

The implementation separates account security configuration from authentication enforcement.

Account Management is responsible for configuring and managing 2FA, while the authentication flow is responsible for enforcing the second-factor challenge during login.

No custom authentication infrastructure was introduced.

### Decisions

- No generic CRUD framework.
- No AutoMapper.
- No MediatR.
- Keep explicit repositories.
- Keep explicit handlers.
- Continue feature-first organization.
- Keep workflow business rules inside Domain aggregates.
- Use Application handlers as the boundary between Presentation and Domain workflows.
- Keep Presentation validation separate from Domain business-rule validation.
- Keep administrative User Management separate from self-service Account Management.
- Keep ASP.NET Core Identity behind the existing Identity service abstraction.
- Keep Account Management security configuration separate from authentication enforcement.
- Use the existing Identity infrastructure for Two-Factor Authentication rather than introducing custom authentication infrastructure.

### Overall Assessment

The architecture is validated for continued expansion across both workflow-driven business modules and security-sensitive self-service capabilities.

Sprint 3 validated the architecture through the Purchasing Application layer.

Sprint 4 extended that validation into the Presentation layer and verified the complete Purchase Order lifecycle through actual browser interactions and persisted database records.

Sprint 5 further validated the architecture through the dedicated Inventory Valuation reporting workflow.

Sprint 6 extended the validation into self-service Account Management, Email Verification, and Two-Factor Authentication.

The review confirmed that:

- Existing Application handlers remain appropriate for both business workflows and account workflows.
- Existing Identity abstractions successfully encapsulate ASP.NET Core Identity.
- Administrative User Management and self-service Account Management remain clearly separated.
- Two-Factor Authentication can be integrated using the existing Identity infrastructure.
- Presentation workflows continue to respect Application and Infrastructure boundaries.
- No structural architectural redesign was required.

Sprint 8 P7 further validated the Purchasing vertical slice through integrated regression testing of Create, multi-item creation, listing, Search, Filtering, Sorting, Pagination, Details, Submit, Approve, Receive, inventory synchronization, authorization, empty-result behavior, and relevant failure/recovery behavior. The verification found and corrected a Presentation-layer pagination state-preservation defect without requiring architectural redesign.

Sprint 7 further validated the architecture through seven read-oriented reporting workflows and Web-layer Excel/PDF export writers. The reporting and export implementation reused the established Application, DTO/read-model, repository, and EF Core patterns without structural redesign.

The current architecture is suitable for continued expansion into:

- Purchasing enhancements
- Sales
- Additional Reporting enhancements
- API modules
- Additional account security capabilities

while preserving the existing Clean Architecture, Vertical Slice Architecture, Rich Domain Model, repository, Identity abstraction, and Application handler patterns.


---

# Sprint 9 Architecture Review Update

**Date:** 2026-08-27  
**Status:** Complete - source-level validation

Sprint 9 revalidated the existing architecture against the T03-T10 consistency changes.

## Findings

- Razor form normalization remained within the Presentation layer.
- `asp-route-*` navigation changes remained within the Razor navigation boundary.
- Application Request binding consolidation removed accidental HTTP-boundary duplication without collapsing Application or repository responsibilities.
- `PagedRequest` -> `PagedQuery` mapping remains a meaningful boundary where the types serve different responsibilities.
- Repository-specific report filters remain feature-specific responsibilities rather than duplicate paging/query fields.
- Direct DTO projection remains an established Infrastructure read-model pattern.
- The Rule-of-Three did not justify a generic report request helper or broad abstraction.
- Redundant inherited repository declarations were removed without changing repository responsibilities.
- No structural architectural redesign was introduced.

## Verification limitation

The supplied environment does not contain the `dotnet` CLI, and no automated test project/source is present. The Sprint 9 architecture review therefore claims source-level validation only and does not claim a successful build or runtime/browser verification.

---

# Sprint 9 T13 Final Documentation & Architecture Validation

**Date:** 2026-08-28  
**Status:** Complete - source/documentation validation

T13 performed the final Sprint 9 consistency gate against the governing README, the current repository source, and the T03-T12 documentation/verification records available in the supplied repository snapshot.

## Final validation result

- Sprint 8 is consistently documented as closed and released as `v1.5.0`.
- Sprint 9 remains limited to ASP.NET Core/Razor code-quality and consistency work; no new business capability is documented as part of Sprint 9.
- `PageNum` is consistently represented in the current shared `PagedRequest`, `PagedQuery`, and Razor/UI paging implementations.
- Dynamic Capability-Based Authorization is outside Sprint 9 and is not present as a dynamic capability implementation in the inspected source. Existing static authorization policies remain separate.
- The reviewed implementation preserves the existing Clean Architecture and feature-first project boundaries.
- Rule-of-Three decisions remain documented; deferred candidates are not represented as completed work.
- Verification claims remain source-level only for Sprint 9 because the supplied environment does not contain the `dotnet` CLI/runtime and no automated test project/source is present.
- No evidence of unrelated business capability or structural architectural redesign was identified in the reviewed source/documentation scope.

## Documentation correction

One actual documentation inconsistency was corrected during T13: `CODE_STYLE.md` incorrectly described `PagedQuery.Page` as the infrastructure paging property, while the current source defines `PagedQuery.PageNum`. The guidance now reflects the source-backed `PageNum` contract without collapsing the distinct Application Request -> `PagedQuery` responsibility boundary.

---

# Sprint 10 — Dynamic Capability-Based Authorization

**Date:** September 2026  
**Status:** PASS WITH FINDINGS — source-verified architecture, runtime-verified behavior (T13)

## Scope

Sprint 10 implemented a dynamic, database-backed capability-based authorization model across all four architecture layers.

## Domain Layer — PASS

4 entities in Domain/Entities with no external framework dependencies:
- `Capability` (Name, IsEnabled, GroupCapabilities)
- `AuthorizationGroup` (Name, Capabilities, UserGroups)
- `AuthorizationGroupCapability` (join entity)
- `UserAuthorizationGroup` (join entity)

Rich domain behavior: AddCapability, RemoveCapability, AssignUser, RemoveUser, Enable, Disable.

No boundary violations: no ASP.NET Core, Identity, ClaimsPrincipal, UserManager, Razor, or HTTP dependencies.

## Application Layer — PASS

- Abstractions: ICapabilityAuthorizationService, ICapabilityRepository, IAuthorizationGroupRepository
- Service: CapabilityAuthorizationService (union semantics, IsEnabled check)
- 10 feature handlers for Group/Capability management
- No unnecessary coupling to ASP.NET Core authorization framework types

## Infrastructure Layer — PASS

- EF Core configurations for 4 tables with correct keys, indexes, unique constraints, cascade delete
- Repositories: AuthorizationGroupRepository, CapabilityRepository
- Seed data: AuthorizationSeeder (39 capabilities, 3 groups), IdentitySeeder (user-to-group assignment)
- Identity integration: ApplicationUser derives from IdentityUser<Guid> — clean

## Web Layer — PASS

- Authorization handlers: CapabilityAuthorizationHandler, MultiCapabilityAuthorizationHandler
- Requirements: CapabilityRequirement, MultiCapabilityRequirement
- Policy registration: CapabilityAuthorizationExtensions, ServiceCollectionExtensions
- 43 page-level [Authorize(Policy)] attributes using capability-backed policies
- 45 Razor User.IsInRole checks migrated to IAuthorizationService.AuthorizeAsync
- 7 admin Razor Pages under /Administrator/Groups and /Administrator/Capabilities

## Identity Compatibility — PASS WITH FINDINGS

- Identity roles (Administrator, InventoryManager, Viewer) retained
- Group names match role names — mapping is name-based
- Users assigned to groups matching their roles — effective equivalence for seeded users
- Non-seeded users: capability model is authoritative (intentional)
- Finding (DF1): InventoryManager group includes Administration.Access — DESIGN FINDING

## Dynamic Capability Flow — PASS (SOURCE VERIFIED)

```
[Authorize(Policy)] / IAuthorizationService.AuthorizeAsync()
    ↓
CapabilityRequirement / MultiCapabilityRequirement
    ↓
CapabilityAuthorizationHandler / MultiCapabilityAuthorizationHandler
    ↓
ICapabilityAuthorizationService.HasCapabilityAsync(userId, capabilityName)
    ↓  ← queries database on every call
ICapabilityRepository.GetByNameAsync → exists + IsEnabled?
    ↓
IAuthorizationGroupRepository.GetForUserAsync → user's groups with capabilities
    ↓
Union: groups.Any(g => g.Capabilities.Any(...))
    ↓
context.Succeed() on match / return on failure (default deny)
```

## Default Deny — PASS (SOURCE VERIFIED)

All handler failure paths return without calling context.Succeed(). Zero fallback role authorization in the codebase.

## Server-Side Authorization — PASS (SOURCE VERIFIED)

43 page-level [Authorize(Policy)] attributes. Handler-level IAuthorizationService checks on Purchasing POST actions. Razor visibility is independent presentation concern.

## Policy Migration — PASS (SOURCE VERIFIED)

- Administrator policy: single cap Administration.Access (only Administrator group)
- InventoryManagement policy: OR-composite of 9 capabilities
- ViewInventory policy: OR-composite of 7 view capabilities
- All 43 page-level [Authorize] attributes preserved unchanged (same policy names)

## Razor UI Visibility — PASS (SOURCE VERIFIED)

45 User.IsInRole checks migrated to IAuthorizationService.AuthorizeAsync with pre-computed boolean variables. Zero IdentityConstants.Roles references remain in .cshtml files.

## Database Model — PASS (SOURCE VERIFIED from migration)

```
AuthorizationGroups (Id PK, Name UNIQUE)
Capabilities (Id PK, Name UNIQUE, IsEnabled)
AuthorizationGroupCapabilities (Id PK, AuthorizationGroupId FK CASCADE, CapabilityId FK CASCADE, UNIQUE composite)
UserAuthorizationGroups (Id PK, UserId FK CASCADE, AuthorizationGroupId FK CASCADE, UNIQUE composite)
```

All non-nullable FKs. Correct cascade delete. Unique composite indexes prevent duplicates.

## Active-Session Behavior

- Capability data is queried per authorization request (no caching) — SOURCE VERIFIED
- Active-session capability change behavior — NOT RUNTIME VERIFIED (T13 did not test this scenario)

## Known Findings

| # | Finding | Severity | Classification |
|---|---------|----------|---------------|
| DF1 | InventoryManager group includes Administration.Access | Medium | DESIGN FINDING — seed data |
| DF2 | InventoryManagement OR-composite grants broad access | Medium | DESIGN FINDING — policy design |
| DF3 | Categories/Edit missing [Authorize] attribute | Medium | PRE-EXISTING GAP |
| DF4 | Viewer has User.View capability | Low | DESIGN FINDING — seed filter |
| DF5 | Suppliers/Create uses ViewInventory policy | Low | PRE-EXISTING POLICY CHOICE |
| DF6 | Dead /Inventory folder convention | Low | DEAD CODE |
| DF7 | Account lockout not restored by seed | Medium | PRE-EXISTING BEHAVIOR |
| DF8 | Reports unrestricted | Low | DESIGN DECISION PENDING |

All findings are documented in the ENGINEERING_JOURNAL T13 entry and deferred to post-Sprint 10.

## Overall Assessment

The Dynamic Capability-Based Authorization architecture is structurally sound and correctly implemented across all four layers. The authorization flow matches the intended design from DD-032. Identity compatibility is preserved. Server-side authorization is enforced. Default deny works correctly.

Findings are seed data and policy design issues, not code bugs. The authorization infrastructure (handlers, services, repositories, policies) works correctly.

Sprint 9 is complete at the source/documentation level, with the runtime verification limitation explicitly retained.
