# Changelog

## [Sprint 16] - Purchase Order POST Round-Trip State and Create Failure Presentation Corrections

### Changed

- Corrected exactly six Purchase Order Details/Edit hidden `Descending` inputs to render explicit `"true"`/`"false"` values.
- Added a narrow `DomainException` boundary around Create handler execution so expected business failures render the canonical message inline after item/dropdown restoration; Result failures, success PRG, authorization, NotFound, and unexpected exceptions retain their existing semantics.

### Verified

- 12/12 true/false round-trip cases passed across Submit, Approve, Cancel, Receive, UpdateItem, and RemoveItem.
- Duplicate-product, zero-quantity, and negative-cost Create failures rendered inline and produced no persistence; successful Create and workflow regression passed.
- Authorization checks passed. Automated suites: 346 UnitTests + 92 IntegrationTests + 39 Web.Tests = 477 passed, 0 failed, 0 skipped.
- Normal build: 0 warnings/0 errors. Non-incremental build: 28 pre-existing warnings/0 errors; no new warning or defect.

### Deferred / Release Classification

- Candidates C (Create FluentValidation invocation), D1 (EditStatus behavior), D4 (HTTP/CI/automated SQL infrastructure), and E (shared POST failure-render extraction) remain deferred. Candidate E was re-evaluated under the Rule of Three and reserved for a dedicated design/refactoring task because page restoration responsibilities differ.
- Non-release sprint: no version bump, tag, GitHub release, schema/migration, authorization/seed, package, or configuration change. v1.6.0 remains the release baseline.

## [Sprint 15] - Purchase Order Workflow Error Handling and UX Hardening

### Summary

Hardened the Purchase Order Details presentation boundary so expected Domain business-rule failures on the four POST workflows render as user-facing inline validation feedback instead of propagating to the Development exception page, following the Sprint 14 Edit-page precedent. Not a release sprint — no version has been assigned; v1.6.0 remains the current release baseline.

### Changed

- All four Purchase Order Details POST handlers (Submit, Approve, Receive, Cancel) now catch expected `DomainException` and route through a single private re-render helper (`RenderDomainFailureAsync`): canonical Domain message added to model-level `ModelState`, Purchase Order reloaded via `GetPurchaseOrderHandler`, successful reload returns `Page()`, helper reload failure returns `NotFound()`
- Authorization remains before try/catch and unchanged (`Forbid()` semantics intact); unexpected exceptions remain outside the narrow catch; existing Application Result failures keep their pre-existing ModelState + reload + Page() semantics
- `PurchaseOrder` reload on failure uses `GetPurchaseOrderHandler`; no Web-layer persistence was added

### Verification

- Automated baseline: 477 passed tests (346 UnitTests, 92 IntegrationTests, 39 Web.Tests; 0 failed, 0 skipped); normal build 0 warnings / 0 errors; full non-incremental build 28 pre-existing warnings / 0 errors (no Sprint 15 warning regression)
- Manual application/browser verification against the configured SQL Server database: all expected Domain failure scenarios render inline with no HTTP 500 (Submit empty/non-Draft, Approve invalid state, all four Receive failures, Cancel invalid state); rejected operations left persisted state unchanged (PurchaseOrders, PurchaseOrderItems, Products.QuantityOnHand, InventoryTransactions); authorization denial remained distinct from validation feedback; GET and crafted POST of a missing Purchase Order returned HTTP 404 via the existing NotFound path; success-path regression passed; persistence confirmed across application restart; migration history matches source with no pending model changes

### Known/Deferred (recorded, not remediated)

- `Descending=True` is not reliably preserved through the hidden-input POST round-trip (Razor boolean-attribute rendering on `value="@Model.Descending"`); exists on Details and Edit pages, predates Sprint 15/T02, does not block the sprint objective — deferred to a future approved task
- Antiforgery `SecurePolicy = Always` is incompatible with plain-HTTP serving; the `https` launch profile is the intended development configuration (record-only observation)
- Create-page duplicate-product `DomainException` presentation and Create FluentValidation production invocation remain deferred (unchanged from planning)

---

## [Sprint 14] - Purchase Order Cancellation and Draft Item Editing

### Summary

Completed the Purchase Order lifecycle by making the existing `Cancelled` state reachable through the full stack and exposing Draft item editing through the Application and Web layers, following the established Clean Architecture, capability-based authorization, and rich-domain conventions. Not a release sprint — no version has been assigned; v1.6.0 remains the current release baseline.

### Added

- Purchase Order cancellation for Draft and Submitted states (`Draft -> Cancelled`, `Submitted -> Cancelled`); cancellation is Domain-rejected for Approved, Receiving, Completed, and already-Cancelled orders
- `Cancelled` as a terminal state: a cancelled Purchase Order cannot Submit, Approve, Receive, edit items, remove items, or reopen
- `CancelPurchaseOrderRequest` / `CancelPurchaseOrderResponse` / `CancelPurchaseOrderHandler` (Application cancellation workflow)
- Dedicated Draft item edit page: `Pages/Purchasing/PurchaseOrders/Edit.cshtml` / `Edit.cshtml.cs` (Quantity/UnitCost update and item removal; `ProductId` is the mutation key; Product replacement is not supported; only Draft orders can be edited; removing the final item is allowed and an empty Draft may temporarily exist — `Submit()` still rejects empty Purchase Orders)
- Cancellation workflow on the Purchase Order Details page (status-gated visibility, confirmation UX, PRG redirect)
- `PurchaseOrder.Edit` and `PurchaseOrder.Cancel` capabilities: catalog/seed additions (39 → 41 capabilities) plus `EditPolicy` / `CancelPolicy` constants and capability-policy registration; group assignment follows the existing filter-derived seed rules (Administrator/InventoryManager/Viewer)
- Draft-only "Edit Items" entry on Details and a `ModelOnly` validation summary restoring render visibility for Application/Domain error feedback
- `PurchaseOrderLifecyclePersistenceTests` (5 IntegrationTests, EF Core InMemory, fresh-context isolation): Cancelled status, item update, item removal, and final-item removal survive fresh-context reload
- `PurchaseOrderCapabilityPolicyRegistrationTests` (6 Web.Tests): Edit/Cancel capability constants, policy naming convention, and real `AddWeb` policy registration
- Application handler tests: 7 cancellation tests (T03) and 14 UpdateItem/RemoveItem tests (T04)

### Changed

- Purchase Order web workflow now exposes cancellation and Draft item editing alongside the existing Submit/Approve/Receive actions; Details POST handlers remain programmatic-authorization and Domain-error propagation per the pre-existing convention
- Automated baseline: 432 → 477 passed tests (346 UnitTests, 92 IntegrationTests, 39 Web.Tests; 0 failed, 0 skipped); full non-incremental build remains 28 warnings / 0 errors (no new Sprint 14 warnings)
- Manual application/provider verification against the configured SQL Server database confirmed cancellation, Draft editing, authorization enforcement (authorized and denied personas, crafted requests), and persistence across application restart; manual browser/provider verification remains manual and is not automated end-to-end testing

### Known/Deferred (recorded, not remediated)

- Existing Details POST handlers (Submit/Approve/Receive/Cancel) follow the pre-existing convention where some Domain failures propagate as an uncaught `DomainException` and render through the Development developer-exception page instead of inline validation; the Sprint 14 Edit page handles equivalent failures inline. Deferred follow-up only — not remediated in Sprint 14
- No schema migration, EF mapping change, or data backfill was required; Sprint 14 uses the existing Purchase Order model and the existing `Cancelled = 6` status

---

## [Sprint 13] - Purchasing Workflow Test Automation

### Summary

Expanded automated coverage of the purchasing workflow — the platform's most business-critical process — across the Application layer (handlers, validators, error contracts) and the `PurchaseOrderRepository` persistence layer, without any production code change. Not a release sprint — v1.6.0 remains the current release baseline.

### Added

- Shared Purchasing test support: `FakePurchaseOrderRepository`, `FakeUnitOfWork` (per-test instance-scoped interaction recording — no static test state), `PurchasingTestData`, `EntityIdHelper`
- `CreatePurchaseOrderHandler` tests (15): supplier missing/inactive, product missing/inactive, duplicate-product `DomainException` propagation, successful Draft creation with aggregate/item mapping, add-before-save ordering
- `CreatePurchaseOrderValidator` (14 discovered cases) and `CreatePurchaseOrderItemValidator` (13 discovered cases) tests: parent rules, per-item child validation, quantity/unit-cost boundaries
- `PurchaseOrderErrors` contract tests (10→11 discovered cases across T02/T03, including the parameterized variants)
- `SubmitPurchaseOrderHandler` (6) and `ApprovePurchaseOrderHandler` (6) tests: not-found failures, valid transitions, response mapping, load-before-save ordering, invalid-state `DomainException` propagation with no save
- `ReceivePurchaseOrderHandler` tests (14): PO/product missing, invalid states, missing PO item, zero/negative/over/cumulative-over receiving, partial and full completion, stock increase via the separately loaded `IProductRepository` product, `StockIn` inventory-transaction creation, observable ordering across participating fakes
- `GetPurchaseOrderHandler` (7) and `GetPurchaseOrdersHandler` (9) query-handler tests: DTO/header/item mapping, received/remaining quantities, paging/search/sort/date/status request pass-through as actually implemented, `PageNum`/`PageSize` clamping, empty pages and paging metadata (fake-based)
- `PurchaseOrderRepositoryTests` (24 IntegrationTests, EF Core InMemory): `GetByIdAsync` Includes/`ThenInclude` verified from fresh contexts (relationship fix-up cannot mask a missing Include), search numeric/name branches, inclusive date boundaries, status filtering, all supported sorts plus the default fallback, paging/metadata/`TotalCount`, `AsNoTracking` on `GetPagedAsync`, and a persistence round-trip proving a repository-loaded aggregate mutation survives save and a further fresh-context reload

### Changed

- No production code, test-support behavior, package, database/migration/seed, CI, or authorization changes. Test suites only.

### Verified

- Build: SUCCESS (0 errors; full non-incremental rebuild reports the unchanged baseline of 28 pre-existing warnings — none from test projects)
- UnitTests: 314 passed
- IntegrationTests: 85 passed
- Web.Tests: 33 passed
- Total: 432 passed, 0 failed, 0 skipped (314 + 85 + 33; integrated verification T07)
- The 313-test pre-sprint baseline was preserved throughout; every task gate grew the suite arithmetically (313 → 365 → 378 → 392 → 408 → 432)

### Known/Deferred (recorded, not remediated)

- `GetPurchaseOrdersHandler` does not copy `PagedRequest.Status` into `PagedQuery` (T05 recorded finding; current-source behavior, tested as-is)
- Repository integration tests use EF Core InMemory: they provide repository wiring/query-shape regression coverage only and do NOT prove SQL Server SQL translation, collation, relational FK/unique constraints, transaction semantics, provider-specific date/string behavior, or query-plan/performance characteristics
- Sprint 12 EditStatus self-deactivation guard remains deferred (behavioral decision pending); untouched by Sprint 13
- WebApplicationFactory integration testing and CI provider establishment remain deferred

---

## [Sprint 12] - Authorization Refinement

### Summary

Hardened the capability-based authorization model through automated Web authorization-handler testing and remediation of two confirmed authorization-boundary defects, without changing the authorization architecture. Not a release sprint — v1.6.0 remains the current release baseline.

### Added

- Third test project: `InventoryPlatform.Web.Tests` (references `InventoryPlatform.Web` only; no test-project cross-references)
- `FakeCapabilityAuthorizationService` — hand-written fake implementing `ICapabilityAuthorizationService` (no mocking framework)
- `CapabilityAuthorizationHandlerTests` — 8 tests covering authentication gate, NameIdentifier extraction/parsing, service delegation, and succeed/do-not-succeed outcomes
- `MultiCapabilityAuthorizationHandlerTests` — 12 tests covering OR semantics with short-circuit, denial when all capabilities are denied, correct user-ID usage, gate/claim edge cases, and requirement constructor validation
- Testing conventions updated for the three-project test architecture

### Changed

- `Categories/Edit.cshtml.cs` now requires `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]` (previously unprotected)
- `Suppliers/Create.cshtml.cs` now requires `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]` (previously `ViewInventory`; 0 `ViewInventory` occurrences remain)

### Verified

- Build: SUCCESS (0 errors; full rebuild reports 28 pre-existing warnings — none introduced by Sprint 12)
- UnitTests: 219 passed
- IntegrationTests: 61 passed
- Web.Tests: 33 passed
- Total: 313 passed, 0 failed, 0 skipped
- Integrated authorization verification (T08): no authorization regression; `RequireRole` = 0; `[Authorize(Roles = ...)]` = 0; capability-policy registrations intact

### Known/Deferred

- EditStatus `User.IsInRole(InventoryManager)` self-deactivation guard (`Pages/Administrator/Users/EditStatus.cshtml.cs`, line 61): Sprint 12 T07 cleanup was blocked after source inspection showed the guard is reachable for supported multi-role users and behavior-affecting — it is NOT dead code. Removal would change observable behavior and requires an explicit behavioral decision. This is NOT a completed fix.
- WebApplicationFactory integration tests, Razor Page authorization integration tests, and CI provider establishment remain deferred.

---

## [Sprint 11] - Automated Testing & Test Automation

### Summary

Established the project's first automated testing foundation and implemented risk-based automated coverage for the highest-value Domain, Application authorization, authorization seeding, and authorization repository behaviors.

### Added

- Two test projects: InventoryPlatform.UnitTests and InventoryPlatform.IntegrationTests
- xUnit test framework with hand-written fakes (no mocking framework)
- EF Core InMemory for integration tests
- 280 automated tests covering Domain, Application, and Infrastructure
- Testing conventions documentation (docs/TESTING_CONVENTIONS.md)
- Provider-neutral CI readiness baseline

### Test Coverage

**UnitTests (219 tests):**
- PurchaseOrder domain workflow (62 tests)
- PurchaseOrderItem behavior (39 tests)
- Product domain behavior (56 tests)
- Capability domain behavior (15 tests)
- AuthorizationGroup domain behavior (31 tests)
- CapabilityAuthorizationService behavior (15 tests)

**IntegrationTests (61 tests):**
- AuthorizationSeeder behavior (24 tests)
- CapabilityRepository behavior (12 tests)
- AuthorizationGroupRepository behavior (24 tests)

### Test Architecture

```text
InventoryPlatform.UnitTests
    -> InventoryPlatform.Domain
    -> InventoryPlatform.Application
    -> InventoryPlatform.Shared

InventoryPlatform.IntegrationTests
    -> InventoryPlatform.Domain
    -> InventoryPlatform.Application
    -> InventoryPlatform.Infrastructure
    -> InventoryPlatform.Shared
```

Neither test project references InventoryPlatform.Web.

### Verified

- Build: SUCCESS (0 errors, 0 warnings)
- UnitTests: 219 passed
- IntegrationTests: 61 passed
- Total: 280 passed, 0 failed
- Authorization seed baseline: 39 capabilities, 39 Administrator, 21 InventoryManager, 13 Viewer, 73 relationships

### Deferred

- T06: Authorization Handler Tests → Sprint 12
- WebApplicationFactory integration tests → Sprint 12
- Razor Page authorization integration tests → Sprint 12
- CI provider establishment → Sprint 12 (if repository hosting is confirmed)

### Authorization Seed Baseline (Source-Confirmed)

```text
Capabilities:                    39
Administrator capabilities:      39
InventoryManager capabilities:   21
Viewer capabilities:            13
Total group-capability relationships: 73
```

**Note:** Viewer receives 13 capabilities because `User.View` matches the Viewer filter's `EndsWith(".View")` predicate. Earlier planning documentation stated 12/72; current source and tests are authoritative.

---

## [v1.6.0] - Sprint 10 Dynamic Capability-Based Authorization

### Summary

Introduced a dynamic, database-backed capability-based authorization model while preserving ASP.NET Core Identity authentication and maintaining backward compatibility.

### Added

- Domain entities: Capability, AuthorizationGroup, AuthorizationGroupCapability, UserAuthorizationGroup
- Application interfaces: ICapabilityAuthorizationService, ICapabilityRepository, IAuthorizationGroupRepository
- Application service: CapabilityAuthorizationService
- Web authorization: CapabilityRequirement, MultiCapabilityRequirement, CapabilityAuthorizationHandler, MultiCapabilityAuthorizationHandler
- AuthorizationPolicies with 39 capability constants
- EF Core configurations for 4 authorization tables
- Repositories: CapabilityRepository, AuthorizationGroupRepository
- Migration: CreateAuthorizationSchema
- Seeder: AuthorizationSeeder with CapabilityCatalog (39 capabilities, 3 groups)
- Administration pages: Groups CRUD, EditCapabilities, EditUsers, Users CRUD, EditRoles, EditStatus, ResetPassword, Capabilities Index (14 pages)
- 10 Application feature handlers for Group/Capability management
- GetAllUsersHandler for user listing in group assignment
- CapabilityAuthorizationExtensions for policy registration
- ApplicationUserClaimsPrincipalFactory for MustChangePassword claim

### Changed

- All 50 page-level [Authorize(Policy)] attributes migrated to capability-backed policies
- 21 Razor UI authorization checks via IAuthorizationService.AuthorizeAsync
- Three static role-based policies replaced with capability-backed equivalents (same policy names)
- _ViewImports.cshtml updated with @using InventoryPlatform.Web.Authorization

### Security

- Default deny enforced: all handler failure paths return without context.Succeed()
- No fallback role authorization exists in the codebase
- UI visibility independently enforced via server-side [Authorize] on every page
- AccessDenied page renders correctly for denied authorization

### Verified

- Build: SUCCESS (0 errors, 0 warnings)
- Authentication: All 3 seeded users login successfully
- Unauthenticated access: All protected pages redirect to login (302)
- Administrator access: All admin pages accessible (200)
- Manager access: All management pages accessible (200), Administrator pages correctly denied (302)
- Viewer access: View pages accessible (200), create/edit pages correctly denied (302)
- Purchasing per-action capabilities: View, Create, Submit, Approve, Receive all verified
- Reports: Accessible to all authenticated users (by design)
- Database: 39 capabilities, 3 groups, 3 assignments verified
- Seed data: Additive-only restoration verified in source

### Known Findings (Deferred)

- DF3: Categories/Edit missing [Authorize] attribute (DEFERRED - PRE-EXISTING)
- Viewer has User.View capability (seed filter includes all *.View; by design)
- Reports unrestricted (design decision pending)

### Resolved Findings

- DF1: InventoryManager group Administration.Access — RESOLVED. Stale DB relationship removed via authorized database remediation (Phase 25). Runtime reverification confirmed 0 rows. All Administrator pages correctly denied for Manager (302).
- DF2: Viewer Supplier.Create — RESOLVED. Stale DB relationship removed via authorized database remediation (Phase 25). Runtime reverification confirmed 0 rows. Create/edit pages correctly denied for Viewer (302).

---

## [Unreleased]

### Sprint 10 T10 - Existing Authorization Boundary Migration

**Status:** Complete

#### Changed

- Added `Administration.Access` capability to the seed data catalog.
- Created `MultiCapabilityRequirement` and `MultiCapabilityAuthorizationHandler` for OR-composite capability authorization.
- Added OR-composite `AddCapabilityPolicy` overload to `CapabilityAuthorizationExtensions`.
- Replaced `Administrator` role-based policy with single-capability `Administration.Access` policy.
- Replaced `InventoryManagement` role-based policy with OR-composite of 9 capabilities.
- Replaced `ViewInventory` role-based policy with OR-composite of 7 view capabilities.
- Replaced `/Administration` and `/Inventory` folder-level role conventions with policy-name references.
- Registered `MultiCapabilityAuthorizationHandler` in DI alongside existing `CapabilityAuthorizationHandler`.

#### Verified

- Build: SUCCESS — 0 errors, 26 pre-existing warnings
- Configuration-level policy equivalence established through group-capability analysis; runtime authorization behavior not verified due to environment limitations.
- All 43 page-level `[Authorize(Policy = ...)]` attributes unchanged (reference same policy names).
- All 46 Razor view `User.IsInRole` checks unchanged (deferred to T12).
- PurchaseOrder authorization (T09) unchanged.
- No database migration required.



### Sprint 10 T12 - Razor Navigation & Capability Visibility

**Status:** Complete

#### Changed

- Replaced 45 `User.IsInRole(...)` role-based UI visibility checks across 14 Razor `.cshtml` files with capability-backed `IAuthorizationService.AuthorizeAsync(...)` calls.
- Replaced admin navigation role check in `_Layout.cshtml` with capability-backed check.
- Added `@using InventoryPlatform.Web.Authorization` to `_ViewImports.cshtml` for global access to `AuthorizationPolicies`.
- Added per-file `@using Microsoft.AspNetCore.Authorization` for `IAuthorizationService` injection.
- Removed `@using InventoryPlatform.Infrastructure.Identity` from all 14 affected files (no longer needed).
- Pre-computed authorization boolean variables (`canManage`, `canAdmin`) in each affected Razor file to minimize per-request authorization evaluations.

#### Verified

- Build: SUCCESS — 0 errors, 20 pre-existing warnings.
- Source verification: All 6 searches confirm correct migration.
- 0 `User.IsInRole(...)` remain in targeted `.cshtml` files.
- 1 dead-code `IsInRole` in `EditStatus.cshtml.cs` documented but unchanged.
- All page-level `[Authorize(Policy = ...)]` attributes unchanged.
- No Domain, Application, or Infrastructure files modified.
- No database migration required.

#### Deferred

- `Categories/Edit.cshtml.cs` missing `[Authorize]` — pre-existing, separate task.
- `Suppliers/Create.cshtml.cs` using overly broad `ViewInventory` policy — pre-existing, separate task.
- `Units/Create.cshtml.cs` using `Administrator` policy — pre-existing, separate task.
- `EditStatus.cshtml.cs` line 61 dead-code `IsInRole`.

### Sprint 9 - ASP.NET Core Code Quality & Consistency

**Status:** T03-T13 implementation/documentation work complete; final validation completed

#### Changed

- Normalized report filter forms in Purchase History and Supplier Purchase Analysis to use `asp-for`.
- Replaced Purchase Order sorting/pagination URL helper generation with Razor `asp-route-*` navigation.
- Consolidated duplicate HTTP query binding on seven core list PageModels by binding the complete Application Request once.
- Removed redundant inherited `AddAsync(...)` and `GetByIdAsync(...)` declarations from `IInventoryTransactionRepository`.
- Corrected seven list-page pagination links to use `asp-route-PageNum`.
- Preserved Purchase Order Details `PageNum` and `PageSize` state through Back and workflow redirects.
- Removed duplicate Purchase Order Status option rendering and added explicit filter control IDs for label association.

#### Verified Conventions

- `PageNum` is the canonical Razor/UI paging property and query parameter.
- `asp-for` is preferred for appropriate Razor form binding and labels.
- `asp-route-*` is preferred for direct Razor navigation and query state.
- Meaningful HTTP Request -> Application Request -> Repository Query boundaries are preserved.
- Rule-of-Three governs shared helper/abstraction extraction.

#### Verification

- Source-level verification completed through T13.
- No automated test project/source is present.
- The supplied verification environment does not contain the `dotnet` CLI; therefore no successful build or runtime/browser result is claimed for Sprint 9.
- No unrelated business capability or structural architectural redesign was introduced in the reviewed Sprint 9 scope.

### Next Locked Feature Priority

Dynamic Capability-Based Authorization remains outside Sprint 9 and is not implemented.


## [v1.5.0] - 2026-08-21

### Release Summary

Sprint 8 Purchasing Enhancements is complete, verified, documented, and closed. This release includes the complete P0-P7 Purchasing enhancement sequence and D1-D4 documentation and closure work.

### Purchasing Enhancements

**Status: Complete and Closed**

Sprint 8 delivered and verified the complete Purchasing enhancement scope P0-P7. D1-D4 documentation and closure tasks are complete. No future-priority feature was implemented during Sprint 8.

#### P1 — Multiple Purchase Order Item Management

- Extended the Purchase Order Create UI to support multiple item rows.
- Added dynamic item-row add/remove behavior.
- Preserved the existing Product, Quantity, and Unit Cost model binding.
- Preserved the existing Application handler and `PurchaseOrder.AddItem()` domain operation.
- Preserved the existing Purchase Order persistence model; no database migration was required.
- Runtime/browser verification confirmed multi-item Purchase Order creation and the existing downstream Purchasing workflow.

P2 - Purchase Order Search is complete and verified.

#### P3 - Purchase Order Filtering

- Added server-side Purchase Order From Date filtering.
- Added server-side Purchase Order To Date filtering.
- Added server-side Purchase Order Status filtering.
- Preserved existing Purchase Order Search behavior.
- Enabled combined Search + filter behavior.
- Preserved applicable filter state through existing Purchase Order navigation.
- Preserved existing empty-result behavior.
- Preserved existing authorization boundaries.
- Runtime/browser verification was completed successfully by the project owner.

#### P4 - Purchase Order Sorting

- Added server-side Purchase Order sorting using the established shared sorting conventions.
- Added supported sorting for Purchase Order ID, Supplier, Order Date, Status, and Total Amount.
- Added ascending and descending sorting behavior.
- Preserved existing Purchase Order Search and Filtering behavior when sorting is applied.
- Preserved applicable sorting state through Purchase Order navigation and workflow actions.
- Runtime/browser verification was completed successfully by the project owner.

#### P5 - Purchase Order Pagination

- Added server-side Purchase Order pagination using the existing `PageNum` / `PageSize` conventions.
- Preserved search, filtering, sorting, and page-size state.
- Browser verification confirmed page-index/result-set behavior with `PageSize=1`.

#### P6 - Inventory Synchronization During Receiving

- Synchronized Purchase Order receiving with Product `QuantityOnHand`.
- Added a `StockIn` InventoryTransaction for each valid receiving operation using the Purchase Order reference.
- Preserved the existing Purchase Order Domain receiving invariants and workflow.
- Persisted Purchase Order state, Product stock, and inventory transaction through the same Unit of Work save boundary.
- Source-level verification confirms repeated receiving cannot exceed the ordered quantity because the existing Domain invariant remains authoritative.
- Runtime/browser verification was completed successfully by the project owner, including valid full and partial receiving, inventory quantity updates, StockIn transaction creation, invalid/over-receiving rejection, repeated receiving safety, authorization preservation, and receiving workflow preservation.

#### P7 — Integrated Purchasing Verification

- Completed integrated regression verification across Purchase Order creation, multiple items, listing, search, date/status filtering, sorting, pagination, details, Submit, Approve, Receive, and inventory synchronization.
- Verified existing authorization boundaries, empty-result behavior, and relevant failure/recovery behavior.
- Corrected an in-scope pagination regression so `FromDate` and `ToDate` remain preserved during pagination.
- Runtime/browser re-verification confirmed the corrected pagination behavior.
- No Dynamic Capability-Based Authorization or unrelated Purchasing feature was introduced.

#### D1 — Documentation Synchronization

- Synchronized current-state documentation with verified Sprint 8 P0-P7 behavior.
- Confirmed the documentation boundary between the released v1.4.0 reporting work and the completed v1.5.0 Purchasing Enhancements.

#### D2 — Design Decision Synchronization

- Recorded the validated Application-layer coordination / Domain-owned invariant decision for Purchase Order receiving and inventory synchronization.

#### D3 — Final Sprint 8 Retrospective

- Recorded the final Sprint 8 outcome, deviations, lessons learned, limitations, and locked future priority boundaries.

#### D4 — Final Documentation Validation

- Completed the final consistency audit against the available source snapshot, verified behavior recorded by P7, Sprint 8 planning baseline, D2 design decision record, and final retrospective.
- Corrected stale D1/D2 next-task references in current-state documentation.
- Confirmed Sprint 7 remains historical/released at v1.4.0 and Sprint 8 Purchasing Enhancements are released as v1.5.0.
- Confirmed no future-priority feature is marked complete.
- Sprint 8 final save point and sprint closure are complete.

## [v1.4.0] - 2026-08-18

### Release Summary

Sprint 7 Additional Reporting and Exports is complete, verified, and released.

### Additional Reporting

Sprint 7 Additional Reporting is complete and has passed final project-wide verification.

#### Completed Reporting

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports

#### Completed Exports

- Excel Export for all seven completed reports
- PDF Export for all seven completed reports using QuestPDF

#### Final Verification

- Authentication, Account Management, and 2FA regression
- Product, Category, Supplier, and Customer management regression
- Purchase Orders and Inventory operations regression
- Reporting filters, sorting, pagination, navigation, and no-result behavior
- Full filtered dataset export verification
- Multi-page PDF verification
- Inventory Valuation Total Inventory Value verification
- Empty database verification
- Explicit query-failure and database recovery verification
- Existing authorization and Access Denied behavior verification
- `dotnet restore` and `dotnet build` verification

No Dynamic Capability-Based Authorization implementation was introduced during Sprint 7.

## [v1.3.0] - 2026-08-13

### Release Summary

This release completes the Account Management vertical slice, providing authenticated users with self-service account management capabilities while preserving the separation between administrative User Management and authentication workflows.

The implementation extends the existing ASP.NET Core Identity integration through the established Identity abstraction, Application handler patterns, and Razor Pages architecture without requiring structural architectural redesign.


### Changed

#### P5 - Purchase Order Pagination

- Added server-side pagination to the Purchase Order listing.
- Reused the existing shared paging infrastructure and `PageNum` / `PageSize` conventions.
- Preserved search, Purchase Order filtering, sorting, and page-size state across pagination navigation.
- Added Previous, numbered-page, and Next navigation with first/last page boundary handling.
- Verified page navigation with `PageSize=1`, including page 5 and active-page/result changes.
- No P6 functionality was introduced.


#### Additional Reporting

- Completed Purchase History reporting.
- Added server-side Purchase History search.
- Added From/To date filtering.
- Added server-side pagination.
- Added server-side sorting.

### Development State

- Sprint 8 Purchasing Enhancements P0-P7 are complete and verified.
- D1 Documentation Synchronization is complete.
- D2 Design Decision Synchronization is complete.
- D3 Final Sprint 8 Retrospective is complete.
- D4 Final Documentation Validation is complete.
- Sprint 8 is ready for its final save point / sprint closure.

---

### Added

#### Account Management

- User Profile
- Update Profile
- Phone Number Update
- Change Password
- Forgot Password
- Reset Password
- Force Password Change

#### Email Verification

- Request Email Verification
- Email Verification
- Email Confirmation
- Email verification status in the user profile

#### Two-Factor Authentication

- Two-Factor Authentication setup
- Authenticator-based TOTP verification
- 2FA login challenge
- Recovery codes
- Recovery code authentication
- Recovery code regeneration
- Recovery code invalidation
- 2FA disablement

#### Account Management Navigation

- Account Management navigation entry
- Two-Factor Authentication navigation entry

---

### Changed

- Separated self-service Account Management from administrative User Management.
- Updated new user creation so email confirmation defaults to unverified.
- Extended the existing Identity service abstraction to support account-management workflows.
- Integrated email verification with the existing development email service.
- Integrated Two-Factor Authentication with the existing ASP.NET Core Identity infrastructure.
- Added authentication challenge handling for users with 2FA enabled.
- Preserved existing Application handler and Razor Pages patterns.

---

### Improved

- Added self-service profile management for authenticated users.
- Added password management and recovery workflows.
- Added email ownership verification.
- Added authenticator-based two-factor authentication.
- Added recovery-code authentication for users without access to their authenticator.
- Added recovery-code regeneration with invalidation of previously generated codes.
- Added account security management without exposing administrative User Management functionality.
- Preserved Clean Architecture and feature-first organization.
- Preserved the separation between Account Management configuration and authentication enforcement.

---

### Validated

#### Profile and Password Management

- Profile display and update
- Phone number update
- Blank phone number handling
- Password change
- Forgot password
- Password reset
- Forced password change

#### Email Verification

- Verification request
- Verification token generation
- Email confirmation
- Already-verified handling
- Verification state displayed in Profile

#### Two-Factor Authentication

- 2FA setup
- Authenticator-code verification
- 2FA login challenge
- Recovery-code login
- Recovery-code regeneration
- Recovery-code invalidation
- 2FA disablement

#### Regression Validation

- Existing administrative User Management workflows
- Authentication and authorization behavior
- Account Management navigation
- Solution build
- Browser-based workflows

---

### Technical Findings

- Existing ASP.NET Core Identity infrastructure was sufficient for Two-Factor Authentication without introducing custom authentication infrastructure.
- The existing Identity abstraction successfully supports both administrative User Management and self-service Account Management.
- Account Management and authentication enforcement remain separate workflows.
- Recovery-code lifecycle management requires explicit handling of generation, single-use authentication, regeneration, and invalidation.
- The completed Account Management vertical slice required no structural architectural redesign.

---

### Documentation

- Updated `PROJECT_STATUS.md`
- Updated `FEATURES.md`
- Updated `ROADMAP.md`
- Updated `README.md`
- Updated `DESIGN_DECISIONS.md`
- Updated `ENGINEERING_JOURNAL.md`
- Updated `ARCHITECTURE_REVIEW.md`
- Updated Sprint 6 Account Management documentation

---

### Outcome

Account Management is now a completed v1.3.0 milestone.

The platform now provides authenticated users with self-service profile, password, email verification, and two-factor authentication capabilities while preserving the existing Clean Architecture, Vertical Slice Architecture, Identity abstraction, Application handler patterns, and Razor Pages workflows.

The release was validated through repeated solution builds and actual browser workflows without requiring structural architectural redesign.

---

## [v1.2.0] - 2026-08-09

### Release Summary

This release introduces the first Reporting vertical slice through the Inventory Valuation report.

The report provides a read-only view of current inventory valuation using the existing inventory valuation definition:

```text
Inventory Value
= Σ (QuantityOnHand × CostPrice)
```

The implementation extends the existing Dashboard read-model approach into a dedicated Reporting feature without requiring architectural redesign.

---

## Added

### Inventory Valuation Report

- Inventory Valuation Razor Page
- Inventory Valuation read model
- Inventory Valuation application request
- Inventory Valuation application handler
- Inventory Valuation persistence abstraction
- Inventory Valuation repository
- Product-level inventory valuation
- Category information
- Quantity On Hand display
- Cost Price display
- Inventory Value display
- Total Inventory Value
- Inventory Valuation navigation entry

### Reporting Read Model

- InventoryValuationDto
- Read-only EF Core projection
- AsNoTracking() query

---

## Changed

- Extended the Application layer with the Reporting feature.
- Added `IInventoryValuationRepository` following the existing repository pattern.
- Added `InventoryValuationRepository` to the Infrastructure layer.
- Added Inventory Valuation presentation under Razor Pages.
- Added Inventory Valuation to the Operations navigation.
- Reused the existing Dashboard inventory valuation definition.

---

## Improved
- Added a dedicated browser-accessible Inventory Valuation report.
- Preserved separation between Presentation, Application, Domain, and Infrastructure layers.
- Kept Reporting read-only and separate from transactional Domain workflows.
- Used EF Core projection to retrieve only the data required by the report.
- Kept inventory valuation calculation database-side.
- Reused the existing Result<T> application pattern.

---

## Validated

### Inventory Valuation

```text
QuantityOnHand × CostPrice
```

Each product's inventory value was verified against the expected calculation.

### Total Inventory Value

```text
Total Inventory Value
=
Σ Product Inventory Value
```

The report total was verified against the existing Dashboard Inventory Value.

### Browser Verification

- Inventory Valuation navigation
- Inventory Valuation page loading
- Product data retrieval
- Category projection
- Quantity On Hand display
- Cost Price display
- Individual Inventory Value calculation
- Total Inventory Value calculation
- Dashboard/report total consistency
- Existing application functionality

### EF Core Query Translation

The initial query attempted to order the projected DTO:

```text
Projection
    ↓
OrderBy(DTO.ProductName)
```

EF Core could not translate this expression.

The query was changed to:

```text
Products
    ↓
OrderBy(Product.Name)
    ↓
Projection
    ↓
InventoryValuationDto
```

This kept the query fully database-side without introducing client-side evaluation.

---

## Technical Findings

The following items remain intentionally deferred:

- Empty database behavior verification
- Explicit query-failure testing for the Reporting feature
- Excel export
- PDF export
- Purchase History report
- Supplier Purchase Analysis
- Stock Movement report
- Low Stock report
- Inventory Movement report
- Advanced report filtering
- Advanced report sorting
- Report scheduling
- Generic reporting framework

These are not considered completed features of this release.

---

## Documentation

- Added SPRINT_05_APPLICATION.md
- Updated Reporting architecture documentation
- Updated project status documentation
- Updated roadmap documentation
- Updated feature documentation
- Updated engineering journal
- Updated design decisions where required

---

## Outcome

The Reporting module now has its first usable vertical slice:

```text
Presentation
     ↓
Application
     ↓
Read Model
     ↓
Infrastructure
     ↓
Database
```

Inventory Valuation is available as a browser-accessible report backed by actual persisted database data.

The implementation validates that the existing architecture can support read-oriented Reporting capabilities alongside the transactional workflows introduced by the Purchasing module.

The first Reporting slice was implemented without requiring structural architectural redesign.

---

## [v1.1.0] - 2026-08-08

### Release Summary

This release completes the Presentation layer for the Purchasing module, connecting the existing Purchasing Application use cases to a usable Razor Pages workflow.

The release delivers a complete Purchase Order vertical slice from creation through submission, approval, partial receiving, and final completion using actual persisted database records.

The implementation preserves the existing Clean Architecture, Rich Domain Model, Vertical Slice Architecture, and business-oriented Application handler patterns established in previous sprints.

---

### Added

#### Purchasing Presentation Layer

- Purchase Order Index page
- Create Purchase Order page
- Purchase Order Details page
- Supplier selection
- Product selection
- Purchase Order item input
- Expected delivery date
- Purchase Order remarks
- Purchase Order status display
- Ordered quantity display
- Received quantity display
- Remaining quantity display
- Calculated Purchase Order total display

#### Purchase Order Workflow

- Submit Purchase Order action
- Approve Purchase Order action
- Receive Purchase Order action
- Partial Purchase Order receiving
- Final receiving and Completed state
- Fully Received item indication

#### Presentation Validation and Feedback

- Client-side Receive quantity validation
- Validation summaries
- Success messages using `TempData`
- Index query failure feedback
- Supplier query failure feedback
- Product query failure feedback

---

### Changed

- Registered Purchasing Application handlers required by the Presentation layer.
- Connected Razor PageModels to the existing Purchasing Application use cases through dependency injection.
- Updated Purchase Order repository queries to load Purchase Order items required for calculated totals.
- Improved Purchase Order Index total calculation.
- Added Presentation-layer handling for Application query failures.
- Added Supplier and Product lookup failure handling during Purchase Order creation.
- Added success feedback after successful Purchase Order operations.

---

### Improved

- Completed the Purchasing workflow as a browser-accessible vertical slice.
- Preserved the separation between Presentation, Application, Domain, and Infrastructure layers.
- Kept business workflow rules inside the `PurchaseOrder` aggregate.
- Improved Purchase Order receiving usability through item-level quantity and remaining-quantity display.
- Improved user feedback for successful and failed operations.
- Validated client-side and Domain-level receiving rules.
- Reused existing Application handlers, Repository, Unit of Work, and Result pattern infrastructure.

---

### Validated

#### Purchase Order Lifecycle

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

### End-to-End Verification

- Purchase Order creation
- Purchase Order listing
- Purchase Order details
- Purchase Order submission
- Purchase Order approval
- Partial receiving
- Final receiving
- Completed status
- Remaining quantity calculation
- Fully Received state
- Calculated Purchase Order total
- Client-side validation
- Domain validation
- Success feedback
- Query failure feedback

The workflow was verified using actual database records rather than seed data.

### Technical Findings

The following items were identified during Sprint 4 but intentionally deferred:

- Cross-cutting DomainException-to-Result/error handling strategy
- Inventory synchronization during Purchase Order receiving
- Multiple Purchase Order item management in the Create UI
- Purchase Order search, filtering, sorting, and pagination
- Additional Product identification information such as SKU in selection controls

These are documented as technical debt or future enhancements and are not considered completed features of this release.

### Documentation
- Added SPRINT_04_APPLICATION.md
- Updated ARCHITECTURE.md
- Updated DESIGN_DECISIONS.md
- Updated ENGINEERING_JOURNAL.md
- Updated FEATURES.md
- Updated PROJECT_STATUS.md
- Updated README.md

### Outcome

The Purchasing module is now a complete browser-accessible vertical slice spanning:

```text
Presentation
     ↓
Application
     ↓
Domain
     ↓
Infrastructure
     ↓
Database
```

The release demonstrates that the architecture can support a workflow-driven business module from Domain and Application implementation through a usable Presentation layer without requiring structural redesign.

The Purchasing module has progressed from an Application-layer capability in v1.0.0 to an end-to-end browser-accessible workflow in v1.1.0.

## [v1.0.0] - 2026-08-07

### Release Summary

This release introduces the Application layer for the Purchasing module, extending the platform beyond CRUD-oriented business modules into workflow-driven business processes.

The Purchasing module demonstrates the successful application of the Rich Domain Model, Vertical Slice Architecture, and business-oriented application handlers without requiring changes to the existing Clean Architecture.

---

### Added

#### Purchasing Module

- Create Purchase Order
- Get Purchase Order
- Get Purchase Orders
- Submit Purchase Order
- Approve Purchase Order
- Receive Purchase Order

#### Application Layer

- Purchase Order command handlers
- Purchase Order query handlers
- Request / Response models
- Purchasing error definitions
- Repository integration for Purchasing workflows

---

### Changed

- Extended the Application layer with workflow-oriented business capabilities.
- Introduced dedicated read models for Purchasing queries.
- Standardized Purchasing handlers using the established Request / Response / Handler pattern.
- Expanded the Domain-driven workflow through the Application layer without modifying architectural boundaries.

---

### Improved

- Preserved thin Application handlers by delegating business behavior to the PurchaseOrder aggregate.
- Maintained consistent Vertical Slice Architecture across all Purchasing features.
- Reused existing Repository, Unit of Work, and Result pattern infrastructure.
- Validated the architecture's ability to support workflow-driven business modules.

---

### Documentation

- Added SPRINT_03_APPLICATION.md
- Updated ARCHITECTURE
- Updated DESIGN_DECISIONS
- Updated ENGINEERING_JOURNAL
- Updated FEATURES
- Updated PROJECT_STATUS

---

### Outcome

The Purchasing module became the first workflow-oriented business module within the platform.

This release validates that the existing architecture successfully scales from CRUD-based modules to aggregate-driven business workflows while preserving Clean Architecture principles.

---

## [v0.9.0] - 2026-08-04

### Release Summary

This release completes **Architecture Sprint 1**, a comprehensive review of the Inventory Management Platform architecture across the Application, Infrastructure, and Web layers.

The objective of this milestone was to validate the existing Clean Architecture implementation before introducing larger business workflows. The review confirmed that the architecture scales successfully and does not require structural redesign prior to implementing the Purchasing module.

---

### Reviewed

#### Application Layer

- Reviewed feature-first organization
- Reviewed request and response models
- Reviewed validators
- Reviewed Create, Get, Update, Activate, and Deactivate handlers
- Reviewed Identity handlers
- Validated handler consistency and application boundaries

#### Infrastructure Layer

- Reviewed dependency injection
- Reviewed ApplicationDbContext
- Reviewed generic repository
- Reviewed feature repositories
- Reviewed IdentityService
- Reviewed Unit of Work
- Reviewed Entity Framework Core configurations

#### Web Layer

- Reviewed shared layout
- Reviewed navigation
- Reviewed Users module
- Reviewed Products module
- Reviewed Categories module
- Reviewed shared Razor Pages patterns

---

### Changed

- Improved naming consistency across the solution
- Improved IdentityService result handling
- Improved handler consistency
- Standardized architecture documentation
- Updated project documentation for Architecture Sprint 1

---

### Validated

- Clean Architecture
- Feature-first organization
- Repository Pattern
- Unit of Work
- Result Pattern
- ASP.NET Core Identity isolation
- Razor Pages architecture
- Shared paging, filtering, and sorting infrastructure

---

### Documentation

- Updated README
- Updated PROJECT_STATUS
- Updated ROADMAP
- Updated ARCHITECTURE
- Updated ENGINEERING_JOURNAL
- Updated DESIGN_DECISIONS
- Updated FEATURES

---

### Outcome

Architecture Sprint 1 concluded that the existing architecture remains stable, maintainable, and suitable for future expansion.

The project is now ready to begin implementation of the **Purchasing Module (v1.0.0)**.

All notable changes to this project will be documented in this file.

---

## [v0.8.0] - 2026-08-01

### Release Summary

This release introduces a complete Identity and User Management subsystem built on ASP.NET Core Identity. Authentication, authorization, and administrative user management are now fully integrated into the existing Clean Architecture while preserving separation of concerns through the `IIdentityService` abstraction.

### Added

#### Authentication

- ASP.NET Core Identity integration
- Cookie authentication
- Login page
- Logout functionality
- Role-based authorization
- Policy-based authorization
- Identity service abstraction

#### User Management

- User listing page
- User details page
- Create user page
- Edit user page
- User role management
- User activation
- User deactivation
- Password reset
- User search
- Server-side pagination
- Server-side sorting
- Status filtering

#### Identity Infrastructure

- ApplicationUser entity
- Identity database integration
- Identity service implementation
- Role initialization
- Authorization policies
- Identity dependency registration

### Changed

- Extended the Clean Architecture to support ASP.NET Core Identity.
- Added `IIdentityService` abstraction to isolate Identity framework APIs.
- Updated the Infrastructure layer to encapsulate user and role management.
- Added administrator-only Razor Pages for user administration.
- Improved project documentation to include authentication, identity, and user management architecture.

### Improved

- Standardized user management workflows with existing application patterns.
- Reused shared paging, filtering, sorting, and Result pattern infrastructure.
- Improved security by encapsulating framework-specific functionality behind application abstractions.
- Maintained consistent feature-first organization across Identity and business modules.
- Preserved Clean Architecture boundaries while integrating authentication and authorization.

---

## [v0.7.0] - 2026-07-27

### Added

#### Dashboard Module

- Dashboard overview page
- Dashboard statistics cards
- Recent inventory transactions widget
- Low stock products widget
- Inventory value summary
- Dashboard refresh action

#### Dashboard Statistics

- Total Products
- Active Products
- Inactive Products
- Low Stock Products
- Out of Stock Products
- Total Inventory Value

#### Dashboard Reporting

- Recent inventory transactions
- Low stock product monitoring
- Read-only dashboard projections
- Empty state handling for dashboard widgets

### Changed

- Added Dashboard feature to the application navigation.
- Extended the application layer with dashboard queries and handlers.
- Added dashboard repository for read-only reporting.
- Introduced dashboard DTOs for statistics and widget data.
- Improved project documentation to reflect the completed Dashboard module.

### Improved

- Added responsive dashboard layout using Bootstrap cards.
- Improved visibility of inventory metrics through KPI cards.
- Enhanced dashboard usability with transaction badges and low stock indicators.
- Formatted inventory value for improved readability.
- Added user-friendly empty state messages when dashboard widgets contain no data.

---

## [v0.6.0] - 2026-07-25

### Added

#### Product Module
- Product search
- Server-side pagination
- Server-side sorting
- Product status filtering
- Product activation
- Product deactivation
- Product details page
- Product create page
- Product edit page
- Product barcode support
- Product category relationship
- Product unit relationship
- Product quantity tracking

#### Category Module
- Category search
- Server-side pagination
- Server-side sorting
- Category status filtering
- Category activation
- Category deactivation
- Category details page
- Category create page
- Category edit page

#### Supplier Module
- Supplier search
- Server-side pagination
- Server-side sorting
- Supplier status filtering
- Supplier activation
- Supplier deactivation
- Supplier details page
- Supplier create page
- Supplier edit page

#### Customer Module
- Customer search
- Server-side pagination
- Server-side sorting
- Customer status filtering
- Customer activation
- Customer deactivation
- Customer details page
- Customer create page
- Customer edit page

#### Unit Module
- Unit search
- Server-side pagination
- Server-side sorting
- Unit status filtering
- Unit activation
- Unit deactivation
- Unit details page
- Unit create page
- Unit edit page

#### Inventory Transactions Module
- Inventory transaction management
- Inventory transaction details page
- Inventory transaction create page
- Inventory transaction listing page
- Stock In transactions
- Stock Out transactions
- Stock Adjustment transactions
- Product selection dropdown
- Transaction type selection
- Transaction reference number
- Transaction remarks
- Transaction date tracking

#### Inventory Workflow
- Automatic Quantity On Hand updates
- Immutable inventory transaction history
- Inventory movement audit trail
- Product inventory validation

#### Shared Infrastructure
- Reusable paging infrastructure
  - PagedRequest
  - PagedQuery
  - PagedResult<T>
- Shared status filter enum
- Shared product sort field definitions
- Shared category sort field definitions
- Shared supplier sort field definitions
- Shared customer sort field definitions
- Shared unit sort field definitions
- Shared inventory transaction sort field definitions

### Changed

- Refactored product repository to support reusable filtering, sorting and paging.
- Refactored product listing to use server-side search.
- Refactored product queries to use reusable paging models.
- Implemented Customer module using the established Product, Category, and Supplier architecture.
- Product now references Category
- Product now references Unit
- Product no longer stores Unit as string
- Product pages updated to use dropdowns
- Product repository updated to load Category and Unit
- Product inventory is now maintained through inventory transactions.
- Product stock updates are handled through domain methods (`IncreaseStock`, `DecreaseStock`, and `AdjustStock`).
- Inventory movements are persisted as historical records instead of directly modifying product quantities.


### Improved

- Product listing preserves filter state across pagination.
- Product listing preserves sorting state.
- Product activation workflow.
- Product deactivation workflow.
- Category listing preserves filter state across pagination.
- Category listing preserves sorting state.
- Category activation workflow.
- Category deactivation workflow.
- Supplier listing preserves filter state across pagination.
- Supplier listing preserves sorting state.
- Supplier activation workflow.
- Supplier deactivation workflow.
- Customer listing preserves filter state across pagination.
- Customer listing preserves sorting state.
- Customer activation workflow.
- Customer deactivation workflow.
- Unit listing preserves filter state across pagination.
- Unit listing preserves sorting state.
- Unit activation workflow.
- Unit deactivation workflow.
- Stronger inventory domain model.
- Foundation prepared for Inventory Transactions.
- Added server-side search for inventory transactions.
- Added server-side sorting for inventory transactions.
- Added server-side pagination for inventory transactions.
- Added Bootstrap badges for transaction types.
- Improved quantity display using positive and negative values.
- Added success notifications after transaction creation.
- Improved inventory transaction user experience with consistent Razor Pages UI.
