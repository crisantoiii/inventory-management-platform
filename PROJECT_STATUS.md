## Overall Progress

█████████████████░░░ 90%

Foundation
████████████████████ 100%

Identity
████████████████████ 100%

Master Data
████████████████████ 100%

Inventory
████████████████████ 100%

Purchase Orders
████████████████████ 100%

Reporting
████████████████████ 100%

Account Management
████████████████████ 100%


# Project Status

**Current Version:** v1.6.0 - Dynamic Capability-Based Authorization

**Latest Release:** v1.6.0 — Sprint 10 Dynamic Capability-Based Authorization

**Project Status:** Sprint 20 Provider-Neutral Continuous Verification — Complete/Closed. Every IntegrationTest carries exactly one explicit `TestTier` — `ProviderNeutral` or `SqlServerRelational` — enforced by a fail-safe classification audit that fails closed on missing, duplicate/multiple, or unknown tiers and locks the 14-identity SQL-provider-bound inventory by fully qualified name; classification follows actual runtime provider dependency (tests that connect or attempt to connect to SQL Server/LocalDB are `SqlServerRelational`), not folder or namespace. Verification runs through one shared command authority, `scripts/verify-provider-neutral.ps1` (repository-local `dotnet-ef 10.0.10` tool restore and resolution evidence, solution restore, normal Release build, UnitTests, Web.Tests, affirmative `TestTier=ProviderNeutral` IntegrationTests, EF `migrations has-pending-model-changes` with no pending model changes, distinct TRX files under `artifacts/verification/`, explicit executed/excluded tier summary, strict exit-code propagation), and the GitHub Actions workflow `.github/workflows/provider-neutral-verification.yml` (pull requests targeting `main`, pushes to `main`, `workflow_dispatch`; `windows-latest`; .NET 10.0.x; `contents: read`; no secrets, no database/browser setup) delegates entirely to that script and uploads the `provider-neutral-verification-results` TRX artifact with `if: always()`. The hosted job completed successfully on a clean runner (three-file TRX artifact; artifact ID `10926782773`, 111464 bytes, SHA256 `38a540e304f33bd77cfbf9d6f4e21086dbf250ee60e0e8eb7320d108ddde739f`); detailed hosted test counts remain supported by the accepted local/integrated verification record. T03 verification empirically exposed one misclassified LocalDB connection probe; under the runtime-dependency rule it was reclassified to `SqlServerRelational` and the lock updated intentionally (relational inventory 13 → 14, ProviderNeutral 127 → 126); the final provider-neutral gate makes zero LocalDB/SQL Server provider contact (instance stopped before and after gated runs, last-start unchanged). **Provider-neutral verification baseline: 535 passing** (346 UnitTests, 63 Web.Tests, 126 ProviderNeutral IntegrationTests; 0 failed, 0 skipped); the 14 `SqlServerRelational` tests are discovered and locked but intentionally not executed in the gate — Sprint 20 makes no SQL relational-pass claim (Sprint 18's 133/133 relational pass at Sprint 19 T05 is historical evidence). Normal Release build is 0 warnings/0 errors; non-incremental build retains 28 unchanged historical warnings/0 errors; migration chain unchanged at 10 through `20260831141400_CreateAuthorizationSchema`. No production behavior, source, schema, migration, package, project, startup, or configuration changed. Technical/non-release — v1.6.0 remains the latest release baseline.

**Last Updated:** September 2026

# Latest Release

## v1.6.0 - Dynamic Capability-Based Authorization

Released: September 2026

Completed:

- Dynamic capability-based authorization model
- 39 capabilities, 3 groups, 3 user assignments
- 50 page-level [Authorize(Policy)] migrations
- 21 Razor UI authorization migrations
- Administration UI (14 pages)
- Database remediation and runtime reverification
- Formal closure gate PASS (Phase 26)

Previous Release:

## v1.5.0 - Sprint 8 Purchasing Enhancements

Released: August 2026

Completed:

- Multiple Purchase Order Item Management
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination
- Inventory Synchronization During Receiving
- Integrated Purchasing Verification
- D1-D4 Sprint 8 documentation and closure

Verified:

- Complete Purchasing workflow from creation through receiving
- Search, date/status filtering, sorting, and pagination
- Inventory synchronization and StockIn transaction creation
- Existing authorization boundaries
- Relevant empty-result and failure/recovery behavior
- Pagination state preservation after the P7 correction

---

## v1.4.0 - Additional Reporting & Exports (Previous Release)

Released: August 2026

Completed:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel Export
- PDF Export
- Final project-wide verification

Verified:

- Authentication, Account Management, and 2FA
- Management and inventory operations regression
- Reporting filters, sorting, pagination, navigation, and no-result behavior
- All seven Excel exports
- All seven PDF exports
- Empty database behavior
- Explicit query failure and database recovery
- Existing authorization boundaries
- `dotnet restore` and `dotnet build`

---

# Milestones

- ✅ v0.1.0 - Product Management
- ✅ v0.2.0 - Category Management
- ✅ v0.3.0 - Supplier Management
- ✅ v0.4.0 - Customer & Unit Management
- ✅ v0.5.0 - Shared Infrastructure
- ✅ v0.6.0 - Inventory Transactions
- ✅ v0.7.0 - Dashboard
- ✅ v0.8.0 - Authentication & User Management
- ✅ v0.9.0 - Architecture Sprint 1
- ✅ v1.0.0 - Purchasing Application Layer
- ✅ v1.1.0 - Purchasing Presentation Layer
- ✅ v1.2.0 - Reporting: Inventory Valuation
- ✅ v1.3.0 - Account Management
- ✅ v1.4.0 - Additional Reporting & Exports
- ✅ v1.5.0 - Sprint 8 Purchasing Enhancements
- ✅ v1.6.0 - Sprint 10 Dynamic Capability-Based Authorization

---

# Overall Completion

- **Completed Modules:** 11
- **Architecture Status:** Validated
- **Current Milestone:** Sprint 20 — Provider-Neutral Continuous Verification (Complete/Closed; technical/non-release — v1.6.0 remains the current release baseline)
- **Next Milestone:** Separate Sprint Planning session pending; no Sprint 21 scope is selected; Sprint 20 carry-forward items remain unassigned
- **Automated Tests:** provider-neutral gate 535 passing (346 UnitTests, 63 Web.Tests, 126 ProviderNeutral IntegrationTests; 0 failed, 0 skipped); 14 SqlServerRelational tests discovered and locked, intentionally not executed in the gate (full local suite remains 535 + 14)

---

# Sprint 9 - ASP.NET Core Code Quality & Consistency

**Status:** T03-T13 complete; final documentation and architecture validation

Sprint 9 is a controlled code-quality and consistency workstream. The verified implementation changes are limited to Razor form binding, Razor route/query navigation, request-binding consolidation, repository contract redundancy cleanup, pagination consistency, and Purchase Order presentation/state corrections.

### Verified implementation

- Purchase History and Supplier Purchase Analysis use `asp-for` for report filters.
- Purchase Order sorting and pagination use `asp-route-*`; the former server-side URL helpers are removed.
- Seven core list PageModels bind their complete Application Request from query state without duplicate `PageNum` handler parameters.
- `IInventoryTransactionRepository` no longer redeclares inherited `AddAsync(...)` or `GetByIdAsync(...)`.
- Seven list pages use `asp-route-PageNum` for pagination and no longer use the identified manual `?Page=...` pagination pattern.
- Purchase Order Details preserves `PageNum` and `PageSize` together with existing search/filter/sort state.
- Purchase Order Status options are rendered once and filter labels have explicit matching control IDs.

### Verification

Source-level verification was completed through T13. The supplied environment does not contain the `dotnet` CLI, so no successful build or runtime/browser verification is claimed for Sprint 9. No automated test project/source is present in the repository.

### Scope boundary

No unrelated business capability or structural architectural redesign was introduced by the reviewed Sprint 9 work. Dynamic Capability-Based Authorization remains outside Sprint 9 and is not implemented.

---

# Sprint 10 - Dynamic Capability-Based Authorization (Complete)

**Status:** Complete. v1.6.0 released. Formal closure gate PASS (Phase 26).

Sprint 10 introduces a dynamic, database-backed capability-based authorization model while preserving ASP.NET Core Identity authentication and maintaining backward compatibility.

### Completed Sprint 10 Tasks

- T01 — Authorization Model & Architecture Baseline
- T02 — Capability and Group Domain Model
- T03 — Application Authorization Abstractions
- T04 — Authorization Persistence & EF Core Configuration
- T05 — Capability/Group Seed Data & Identity Compatibility Mapping
- T06 — Database Migration
- T07 — Capability Authorization Service
- T08 — ASP.NET Core Capability Authorization Handler
- T09 — Purchasing Dynamic Authorization Integration
- T10 — Existing Authorization Boundary Migration
- T11 — Authorization Administration
- T12 — Razor Navigation & UI Capability Visibility
- T13 — Integrated Authorization Verification (runtime verified)
- T14 — Documentation Synchronization (complete)

### T13 - Integrated Authorization Verification

**Status:** Complete

Performed comprehensive runtime verification of the Sprint 10 authorization implementation using the actual running application.

**Verification Results:**
- Build: SUCCESS (0 errors, 0 warnings)
- Authentication: All 3 seeded users login successfully
- Unauthenticated access: All protected pages redirect to login (302)
- Administrator access: All admin pages accessible (200)
- Manager access: All management pages accessible (200)
- Viewer access: View pages accessible (200), management pages correctly denied (302 → AccessDenied)
- Purchasing: All per-action capabilities work correctly
- Reports: Accessible to all authenticated users (by design)
- AccessDenied page: Renders correctly
- Database: 39 capabilities, 3 groups, 3 assignments verified

**Key Findings:**
- InventoryManager group includes Administration.Access (seed data issue — Medium severity)
- InventoryManagement OR-composite grants broad access via single capability (Medium severity)
- Categories/Edit missing [Authorize] attribute (Medium severity — pre-existing gap)
- Viewer has User.View capability (Low severity)
- Reports unrestricted (Low severity — design decision pending)

### T10 - Existing Authorization Boundary Migration

**Status:** Complete

Migrated the three remaining static role-based authorization policies (`Administrator`, `InventoryManagement`, `ViewInventory`) to capability-backed equivalents while preserving effective access behavior.

Implemented changes:

- Added `Administration.Access` capability to the seed data catalog (assigned to Administrator group via `CapabilityCatalog.All`)
- Created `MultiCapabilityRequirement` and `MultiCapabilityAuthorizationHandler` for OR-composite capability authorization
- Added OR-composite `AddCapabilityPolicy` overload to `CapabilityAuthorizationExtensions`
- Replaced role-based policy registrations with capability-backed equivalents using same policy names
- Replaced folder-level role conventions (`/Administration`, `/Inventory`) with policy-name references
- Registered `MultiCapabilityAuthorizationHandler` in DI

Configuration-level policy equivalence established through group-capability analysis; runtime authorization behavior not verified due to environment limitations.

### T10 Files Changed

- `Infrastructure/Identity/AuthorizationSeeder.cs` — Added `Administration.Access` capability
- `Web/Authorization/MultiCapabilityRequirement.cs` — NEW: OR-composite requirement
- `Web/Authorization/MultiCapabilityAuthorizationHandler.cs` — NEW: OR-composite handler
- `Web/Authorization/CapabilityAuthorizationExtensions.cs` — Added OR-composite overload
- `Web/Authorization/AuthorizationPolicies.cs` — Added capability constants
- `Web/Extensions/ServiceCollectionExtensions.cs` — Replaced role-based policies + folder conventions

### T10 Build Verification

- Build: SUCCESS — 0 errors, 26 pre-existing warnings
- No automated test project exists in the repository
- Runtime/browser verification not performed (environment limitation)
- No database migration required (seed data only)

### Authorization Equivalence (Configuration-Level)

- `Administrator` policy: `Administration.Access` capability (only Administrator group has it) — equivalent to `RequireRole("Administrator")`
- `InventoryManagement` policy: OR-composite of 9 capabilities (Product.Create/Edit, Category.Create, Supplier.Create/Edit, Customer.Create/Edit, Unit.Edit, InventoryTransaction.Create) — equivalent to `RequireRole("Administrator", "InventoryManager")`
- `ViewInventory` policy: OR-composite of 7 view capabilities (Dashboard.View, Product.View, Category.View, Unit.View, Customer.View, Supplier.View, InventoryTransaction.View) — equivalent to `RequireRole("Administrator", "InventoryManager", "Viewer")`

### T10 Preservation

- All 43 page-level `[Authorize(Policy = ...)]` attributes: UNCHANGED (reference same policy names)
- All 46 Razor view `User.IsInRole` checks: UNCHANGED (deferred to T12)
- EditStatus.cshtml.cs: UNCHANGED
- PurchaseOrder authorization (T09): UNCHANGED
- Single-capability `CapabilityRequirement`/`CapabilityAuthorizationHandler`: UNCHANGED

### Deferred from T10

- Razor `User.IsInRole` UI visibility checks (45 occurrences in 14 `.cshtml` files) — T12 COMPLETE
- Categories/Edit.cshtml.cs missing `[Authorize]` — pre-existing, separate task
- Suppliers/Create.cshtml.cs using overly broad `ViewInventory` policy — pre-existing, separate task

### T11 - Authorization Administration

**Status:** Complete

Added the minimum administration surface for managing dynamic authorization: group CRUD, capability assignment to groups, user assignment to groups, and a read-only capability catalog.

Implemented changes:

- Added `GetWithCapabilitiesAndUsersAsync` and `GetAllWithDetailsAsync` to `IAuthorizationGroupRepository`
- Added `GetAllUsersAsync` to `IIdentityService`
- Created `AuthorizationGroupErrors` error constants
- Created `CapabilityOption` DTO for checkbox UI
- Created 10 Application feature handlers (GetAuthorizationGroups, GetAuthorizationGroup, CreateAuthorizationGroup, UpdateAuthorizationGroup, DeleteAuthorizationGroup, ManageGroupCapabilities, ManageGroupUsers, GetCapabilities, GetAllUsers)
- Implemented repository and identity service methods
- Registered all handlers in Application DI
- Created 7 Razor Pages under Administrator (Groups/Index, Create, Edit, Details, EditCapabilities, EditUsers; Capabilities/Index)
- Updated navigation layout with Groups and Capabilities links

### T11 Files Changed

- `Application/Interfaces/Authorization/IAuthorizationGroupRepository.cs` — Added 2 methods
- `Application/Interfaces/Identity/IIdentityService.cs` — Added 1 method
- `Application/DependencyInjection/ServiceCollectionExtensions.cs` — Registered 9 new handlers
- `Infrastructure/Persistence/Repositories/AuthorizationGroupRepository.cs` — Implemented 2 new methods
- `Infrastructure/Identity/IdentityService.cs` — Implemented GetAllUsersAsync
- `Web/Pages/Shared/_Layout.cshtml` — Added admin navigation links

### T11 Files Added

- `Application/Features/AuthorizationGroups/AuthorizationGroupErrors.cs`
- `Application/Features/AuthorizationGroups/GetAuthorizationGroups/` (3 files)
- `Application/Features/AuthorizationGroups/GetAuthorizationGroup/` (3 files)
- `Application/Features/AuthorizationGroups/CreateAuthorizationGroup/` (4 files)
- `Application/Features/AuthorizationGroups/UpdateAuthorizationGroup/` (3 files)
- `Application/Features/AuthorizationGroups/DeleteAuthorizationGroup/` (2 files)
- `Application/Features/AuthorizationGroups/ManageGroupCapabilities/` (2 files)
- `Application/Features/AuthorizationGroups/ManageGroupUsers/` (2 files)
- `Application/Features/Capabilities/GetCapabilities/` (2 files)
- `Application/Features/Users/GetAllUsers/` (2 files)
- `Application/DTOs/Authorization/CapabilityOption.cs`
- `Web/Pages/Administrator/Groups/` (12 files)
- `Web/Pages/Administrator/Capabilities/` (2 files)

### T11 Build Verification

- Build: SUCCESS — 0 errors, 20 pre-existing warnings
- No automated test project exists in the repository
- Runtime/browser verification deferred to T13
- No database migration required

### T11 Security

- All admin pages secured with `[Authorize(Policy = AuthorizationPolicies.Administrator)]`
- Delete safety: refuses group deletion when users are assigned
- Seed-based recovery: application restart restores Administrator Group access if lockout occurs
- Administrator lockout is HIGH impact / LOW probability with automatic recovery on restart

### Sprint 10 Closure

- T15 — Sprint 10 Final Verification, Retrospective & Save Point — Complete

---

# Sprint 12 - Authorization Refinement (Complete)

**Status:** Complete. T01–T06, T08, and T09 complete; T07 Blocked/Deferred. Documentation-only closure performed (T09). Not a release sprint — v1.6.0 remains the current release baseline.

Sprint 12 hardened the capability-based authorization model through automated handler testing and remediation of two confirmed authorization-boundary defects, without changing the authorization architecture.

### Completed Sprint 12 Tasks

- T01 — Web.Tests project foundation (`InventoryPlatform.Web.Tests`, references Web only) — Complete
- T02 — `FakeCapabilityAuthorizationService` hand-written test double (no mocking framework) — Complete
- T03 — `CapabilityAuthorizationHandlerTests` (8 tests) — Complete
- T04 — `MultiCapabilityAuthorizationHandlerTests` (12 tests, OR semantics with short-circuit) — Complete
- T05 — Categories/Edit remediation: `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]` — Complete
- T06 — Suppliers/Create remediation: `ViewInventory` replaced with `InventoryManagement` (0 `ViewInventory` occurrences) — Complete
- T07 — EditStatus `IsInRole` cleanup — **BLOCKED/DEFERRED**: the remaining `User.IsInRole(InventoryManager)` guard (line 61) is reachable for supported multi-role users and behavior-affecting (self-deactivation prevention); removing it would change observable behavior. It is NOT dead code.
- T08 — Integrated verification — Complete (passed, with one documented non-state-changing process deviation: a read-only Git-status probe that failed because the workspace is not a Git repository)
- T09 — Documentation synchronization and Sprint 12 closure — Complete

### Final Verified Baseline

```text
UnitTests:        219 passed
IntegrationTests:  61 passed
Web.Tests:         33 passed
Total:            313 passed, 0 failed, 0 skipped
Build:             0 errors
Full rebuild:      28 pre-existing warnings (no warnings introduced by Sprint 12)
```

### Authorization State (Verified)

- Categories/Edit and Suppliers/Create require `AuthorizationPolicies.InventoryManagement`
- CapabilityAuthorizationHandler and MultiCapabilityAuthorizationHandler tests pass
- `RequireRole`: 0 — `[Authorize(Roles = ...)]`: 0 — `User.IsInRole` in Web: 1 (EditStatus line 61, reachable)
- No authorization regression discovered; capability-based authorization preserved

### Known Deferred Item

- T07: the EditStatus `IsInRole` guard requires an explicit behavioral decision (unconditional self-deactivation guard vs. removal vs. keep). See `docs/retrospectives/SPRINT_12_AUTHORIZATION_REFINEMENT.md` Section 24.

---


# Architecture Validation

Architecture Sprint 1 has been completed.

Result

Application Layer

- Passed

Infrastructure Layer

- Passed

Web Layer

- Passed

Overall Assessment

The architecture has been validated and is considered stable for future business expansion.

---

# Overall Progress

| Module | Status | Progress |
|---------|--------|---------:|
| Product Management | ✅ Complete | 100% |
| Category Management | ✅ Complete | 100% |
| Supplier Management | ✅ Complete | 100% |
| Customer Management | ✅ Complete | 100% |
| Unit Management | ✅ Complete | 100% |
| Inventory Transactions | ✅ Complete | 100% |
| Dashboard | ✅ Complete | 100% |
| Authentication & Authorization |  ✅ Complete | 100% |
| User Management | ✅ Complete | 100% |
| Purchasing | ✅ Core Workflow + Sprint 8 Purchasing Enhancements | 100% |
| Reporting | ✅ Additional Reporting Complete | 100% |
| Account Management | ✅ Complete | 100% |

---

# Completed Features

## Product Management

- ✅ Product Listing
- ✅ Product Details
- ✅ Create Product
- ✅ Edit Product
- ✅ Activate Product
- ✅ Deactivate Product
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Category Management

- ✅ Category Listing
- ✅ Category Details
- ✅ Create Category
- ✅ Edit Category
- ✅ Activate Category
- ✅ Deactivate Category
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications
  
## Supplier Management

- ✅ Supplier Listing
- ✅ Supplier Details
- ✅ Create Supplier
- ✅ Edit Supplier
- ✅ Activate Supplier
- ✅ Deactivate Supplier
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Customer Management

- ✅ Customer Listing
- ✅ Customer Details
- ✅ Create Customer
- ✅ Edit Customer
- ✅ Activate Customer
- ✅ Deactivate Customer
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Unit Management

- ✅ Unit Listing
- ✅ Unit Details
- ✅ Create Unit
- ✅ Edit Unit
- ✅ Activate Unit
- ✅ Deactivate Unit
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Inventory Transactions

- ✅ Inventory Transaction Listing
- ✅ Transaction Details
- ✅ Create Inventory Transaction
- ✅ Stock In
- ✅ Stock Out
- ✅ Stock Adjustment
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Product Quantity Updates
- ✅ Immutable Transaction History

## Dashboard

- ✅ Dashboard Overview
- ✅ Inventory Statistics
- ✅ Inventory Value Summary
- ✅ Recent Inventory Transactions
- ✅ Low Stock Products
- ✅ Dashboard Refresh
- ✅ Responsive Layout
- ✅ Empty State Handling

## Authentication

- ✅ Login
- ✅ Logout
- ✅ Cookie Authentication
- ✅ ASP.NET Core Identity
- ✅ Role-based Authorization
- ✅ Policy-based Authorization
- ✅ Dynamic Capability-Based Authorization
- ✅ Web Authorization Handler Tests (Sprint 12 — CapabilityAuthorizationHandler, MultiCapabilityAuthorizationHandler)

## User Management

- ✅ User Listing
- ✅ User Details
- ✅ Create User
- ✅ Edit User
- ✅ Assign Roles
- ✅ Activate User
- ✅ Deactivate User
- ✅ Reset Password
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering

## Account Management

### Profile

- ✅ User Profile
- ✅ Self-Service Account Management

### Password Management

- ✅ Change Password
- ✅ Forgot Password
- ✅ Reset Password
- ✅ Force Password Change

### Email

- ✅ Email Verification

### Two-Factor Authentication

- ✅ Two-Factor Authentication
- ✅ 2FA Setup
- ✅ 2FA Verification
- ✅ 2FA Login Challenge
- ✅ Recovery Codes
- ✅ Recovery Code Regeneration
- ✅ Recovery Code Invalidation
- ✅ 2FA Disable

## Purchasing

### Application Layer

- ✅ Create Purchase Order
- ✅ Get Purchase Order
- ✅ Get Purchase Orders
- ✅ Submit Purchase Order
- ✅ Approve Purchase Order
- ✅ Receive Purchase Order
- ✅ Rich Domain Workflow
- ✅ CQRS-style Application Layer

### Presentation Layer

- ✅ Purchase Order Listing
- ✅ Create Purchase Order
- ✅ Purchase Order Details
- ✅ Submit Purchase Order
- ✅ Approve Purchase Order
- ✅ Partial Purchase Order Receiving
- ✅ Final Purchase Order Receiving
- ✅ Completed Purchase Order State
- ✅ Supplier Selection
- ✅ Product Selection
- ✅ Ordered Quantity Display
- ✅ Received Quantity Display
- ✅ Remaining Quantity Display
- ✅ Calculated Purchase Order Total
- ✅ Multiple Purchase Order Item Creation
- ✅ Dynamic Item Add/Remove in Create UI
- ✅ Client-side Validation
- ✅ Domain Validation
- ✅ Success Messages
- ✅ Query Failure Feedback

## Reporting

### Inventory Valuation

- ✅ Inventory Valuation Report
- ✅ Inventory Valuation Read Model
- ✅ Inventory Valuation Application Handler
- ✅ Inventory Valuation Persistence Abstraction
- ✅ Inventory Valuation Repository
- ✅ Read-only EF Core Projection
- ✅ Product-level Inventory Valuation
- ✅ Category Projection
- ✅ Quantity On Hand Display
- ✅ Cost Price Display
- ✅ Inventory Value Display
- ✅ Total Inventory Value
- ✅ Inventory Valuation Navigation
- ✅ Dashboard/Report Value Consistency
- ✅ Browser Verification

### Purchase History

- ✅ Purchase History Report
- ✅ Purchase History Read Model
- ✅ Purchase History Application Handler
- ✅ Purchase History Persistence Abstraction
- ✅ Purchase History Repository
- ✅ Supplier Information
- ✅ Purchase Order Status
- ✅ Total Amount
- ✅ Total Quantity
- ✅ Received Quantity
- ✅ Remaining Quantity
- ✅ Server-side Search
- ✅ From/To Date Filtering
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Browser Verification

### Supplier Purchase Analysis

- ✅ Supplier Purchase Analysis Report
- ✅ Supplier Purchase Analysis Read Model
- ✅ Supplier Purchase Analysis Application Handler
- ✅ Supplier Purchase Analysis Persistence Abstraction
- ✅ Supplier Purchase Analysis Repository
- ✅ Supplier Aggregation
- ✅ Purchase Period Display
- ✅ Purchase Order Count
- ✅ Ordered Quantity
- ✅ Received Quantity
- ✅ Remaining Quantity
- ✅ Total Amount
- ✅ Server-side Supplier Search
- ✅ From/To Date Filtering
- ✅ Status Filtering
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Pagination State Preservation
- ✅ No-result Behavior
- ✅ Browser Verification

### Stock Movement

- ✅ Stock Movement Report
- ✅ Stock Movement Read Model
- ✅ Stock Movement Application Handler
- ✅ Stock Movement Persistence Abstraction
- ✅ Stock Movement Repository
- ✅ Product Information
- ✅ SKU Information
- ✅ Movement Type
- ✅ Quantity Display
- ✅ Reference Number
- ✅ Remarks
- ✅ Server-side Search
- ✅ From/To Date Filtering
- ✅ Movement Type Filtering
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Stock Movement Navigation
- ✅ Browser Verification

### Low Stock

- ✅ Low Stock Report
- ✅ Low Stock Read Model
- ✅ Low Stock Application Handler
- ✅ Low Stock Persistence Abstraction
- ✅ Low Stock Repository
- ✅ Product information
- ✅ SKU information
- ✅ Category information
- ✅ Quantity On Hand
- ✅ Low-stock threshold based on the existing Dashboard rule: QuantityOnHand <= 10
- ✅ Server-side Product/SKU search
- ✅ Server-side sorting
- ✅ Server-side pagination
- ✅ Pagination state preservation
- ✅ Reset behavior
- ✅ Combined search/sorting/pagination
- ✅ Low Stock navigation
- ✅ Browser verification

### Inventory Movement

- ✅ Inventory Movement Report
- ✅ Inventory Movement Read Model
- ✅ Inventory Movement Application Handler
- ✅ Inventory Movement Persistence Abstraction
- ✅ Inventory Movement Repository
- ✅ Product Information
- ✅ SKU Information
- ✅ Opening Quantity
- ✅ Stock In Quantity
- ✅ Stock Out Quantity
- ✅ Adjustment Quantity
- ✅ Closing Quantity
- ✅ Server-side Product/SKU Search
- ✅ From/To Date Filtering
- ✅ Reporting Period Display
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Pagination State Preservation
- ✅ Page-size Changes
- ✅ Reset Behavior
- ✅ Combined Search and Date Filtering
- ✅ Boundary / No-result Behavior
- ✅ Inventory Movement Navigation
- ✅ Browser Verification

### Product Reports

- ✅ Product Reports
- ✅ Product Report Read Model
- ✅ Product Report Application Handler
- ✅ Product Report Persistence Abstraction
- ✅ Product Report Repository
- ✅ Product Information
- ✅ SKU Information
- ✅ Category Information
- ✅ Unit Information
- ✅ Quantity On Hand
- ✅ Cost Price
- ✅ Selling Price
- ✅ Product Status
- ✅ Active / Inactive / All Products Filtering
- ✅ Server-side Product/SKU/Category/Unit Search
- ✅ Server-side Sorting
- ✅ Server-side Pagination
- ✅ Pagination State Preservation
- ✅ Page-size Changes
- ✅ Reset Behavior
- ✅ Combined Search and Status Filtering
- ✅ Boundary / No-result Behavior
- ✅ Product Reports Navigation
- ✅ Browser Verification

### Reporting - Completed Export Work

- ✅ Excel export
- ✅ PDF export

### Reporting - Final Verification

- ✅ Final project-wide verification
- ✅ Empty database behavior verification
- ✅ Explicit query-failure testing
- ✅ Authorization regression verification
- ✅ Final build verification

### Reporting - Sprint 7 Outcome

Sprint 7 Additional Reporting is complete and fully verified.

Completed reporting capabilities:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel Export
- PDF Export

Final verification covered normal application regression, reporting workflows, export behavior, empty-database behavior, explicit query failure and recovery, authorization boundaries, and final solution build verification.

---

# Shared Infrastructure

## Implemented

- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ Shared Paging Infrastructure
- ✅ Shared Filtering Infrastructure
- ✅ Shared Sorting Infrastructure
- ✅ Entity Framework Core
- ✅ Dependency Injection
- ✅ Razor Pages
- ✅ Domain-driven Inventory Updates
- ✅ Inventory Transaction Workflow
- ✅ Dashboard Read Models
- ✅ ASP.NET Core Identity Integration
- ✅ Identity Service Abstraction
- ✅ Shared Result Pattern
- ✅ Shared User Sorting Infrastructure
- ✅ Vertical Slice Architecture
- ✅ Workflow-oriented Application Handlers
- ✅ Rich Domain Model

---

# Current Focus

Sprint 20 is complete and closed. The repository now has explicit, fail-safe IntegrationTest tier classification (every IntegrationTest resolves to exactly one supported `TestTier` — `ProviderNeutral` or `SqlServerRelational` — with classification following actual runtime provider dependency, not folder placement), a locked 14-identity SQL-provider-bound inventory, one shared local/CI verification command (`scripts/verify-provider-neutral.ps1`), and a GitHub Actions provider-neutral gate (`.github/workflows/provider-neutral-verification.yml`) whose hosted job completed successfully on a clean runner with a three-file TRX artifact. The provider-neutral gate makes zero LocalDB/SQL Server provider contact (instance stopped before and after gated runs, last-start unchanged). The provider-neutral verification baseline is 535 passing (346 UnitTests, 63 Web.Tests, 126 ProviderNeutral IntegrationTests; 0 failed, 0 skipped); the 14 `SqlServerRelational` tests are discovered and locked but intentionally not executed in the gate — Sprint 20 makes no SQL relational-pass claim. Normal Release build is 0 warnings/0 errors (non-incremental: 28 unchanged historical warnings/0 errors); EF has no pending model changes; 10 migrations, latest `20260831141400_CreateAuthorizationSchema`. Sprint 20 made zero production changes and closed the Sprint 19 carry-forward item C19-02 (provider-neutral continuous verification). Remaining carry-forward: C19-01, C19-03 through C19-12 (renumbered C20-01 through C20-11 in Sprint 20 records, with C19-02 closed), including the recorded observation that the seeded Viewer has broad `PurchaseOrder.*`, including `PurchaseOrder.Create` (no remediation commitment); no Sprint 21 scope is selected.

Sprint 19 (historical record) was complete and closed before Sprint 20. The Sprint 17 database-safe HTTP/Razor integration foundation covers the real `/Purchasing/PurchaseOrders/Create` pipeline through five accepted HTTP behaviors (H1-H5): anonymous-GET login challenge with semantic ReturnUrl; authorized manager access for seeded `manager@inventory.local`; real `PurchaseOrder.Create` capability denial of the test-only `purchaseorder-denied@inventory.test` user with no authorization-group assignment (the seeded Viewer is NOT the H3 identity because it already possesses `PurchaseOrder.Create`; no production seed or authorization bypass was added); valid rendered-antiforgery indexed POST → real PageModel/Application handler → immediate 302 PRG → same-factory persistence of a Draft Purchase Order plus one item; and duplicate-product Domain-failure redisplay with the canonical message `The product already exists in this purchase order.`, full state restoration, and no mutation. At the Sprint 19 T05 integrated verification, 542 automated tests passed (346 UnitTests, 133 IntegrationTests, 63 Web.Tests), including 41 relational tests that were freshly re-executed against available LocalDB; builds retained the 0-warning normal and 28-unchanged-warning non-incremental baselines; EF had no pending model changes. There are 10 migrations, latest `20260831141400_CreateAuthorizationSchema`. H1-H5 are EF Core InMemory HTTP-host evidence, not SQL Server relational evidence — the Sprint 18 relational tier (537/537 at its closure) remains the relational architecture authority. Sprint 19 made zero production changes.

Completed in Sprint 8:

- P0 - Actual Purchasing Source/Documentation Baseline
- P1 - Multiple Purchase Order Item Management
- P2 - Purchase Order Search
- P3 - Purchase Order Filtering
- P4 - Purchase Order Sorting
- P5 - Purchase Order Pagination
- P6 - Inventory Synchronization During Receiving
- P7 - Integrated Purchasing Verification

P1, P2, P3, P4, P5, P6, and P7 were runtime/browser verified successfully. The Purchase Order workflow now supports multiple item rows, search, filtering, sorting, pagination, and inventory synchronization while preserving the existing Purchasing architecture and downstream workflow.

# D1 - Documentation Synchronization

**Status: Complete**

Current-state documentation was synchronized with the verified Sprint 8 Purchasing behavior through P7.

Validated documentation updates include:
- Sprint 8 P0-P7 completion state
- P7 integrated verification scope and result
- Pagination date-filter state preservation correction
- Current Purchasing scope and future priority boundaries
- D4 final documentation validation and sprint-closure readiness

No implementation behavior was changed as part of D1.

# Known Limitations

Current development scope does not yet include:

Business Modules

- Sales Orders

Platform

- Audit Logging
- File Uploads

---

# Overall Assessment

The Dashboard, Product, Category, Supplier, Customer, Unit, Inventory Transaction, and Account Management modules are feature-complete for their current planned scope.

The architecture has been validated through master data modules, transactional workflows, dashboard reporting, authentication, comprehensive user management, and the Purchasing workflow.

Architecture Sprint 1 validated the solution across the Application, Infrastructure, and Web layers and concluded that no architectural redesign was required.

Sprint 3 extended this validation into workflow-driven business processes through the Purchasing Application layer.

Sprint 4 extended the Purchasing workflow into the Presentation layer, providing a usable Razor Pages interface for:

- Purchase Order listing
- Purchase Order creation
- Purchase Order details
- Submission
- Approval
- Partial receiving
- Final receiving

The complete Purchase Order lifecycle was verified through actual browser interactions and persisted database records:

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

The successful implementation confirms that the existing architecture supports a complete workflow-driven vertical slice across:

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

without structural redesign.

The Purchasing module is complete for the current roadmap scope. Future enhancements remain subject to the next Sprint Planning process.

Sprint 5 introduced the first Reporting vertical slice through Inventory Valuation.

The Inventory Valuation report provides a read-only browser-accessible view backed by actual persisted database data.

Sprint 6 extended the Reporting architecture through the Purchase
History vertical slice.

The Purchase History report provides a read-only view of historical
Purchase Orders backed by persisted database data and supports:

- Server-side search
- From/To date filtering
- Server-side pagination
- Server-side sorting

The Purchase History implementation confirmed that the read-oriented
Reporting architecture established by Inventory Valuation can be
reused for additional reporting domains without structural
redesign.

The platform is currently continuing development of Additional
Reporting capabilities.

Inventory valuation is calculated using:

```text
QuantityOnHand × CostPrice
```

The report total was verified against the existing Dashboard Inventory Value.

The Reporting implementation follows a read-oriented architecture:

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

The implementation demonstrates that the existing architecture supports read-oriented Reporting capabilities alongside transactional business workflows without requiring structural redesign.

The Reporting module remains partially complete, with additional reports and export capabilities planned for future work.

The architecture has now been validated through:

- Master data modules
- Inventory transactions
- Dashboard reporting
- Authentication
- Administrative user management
- Purchasing workflows
- Read-oriented Reporting
- Self-service Account Management
- Two-factor authentication


## Current Focus

### Sprint 12 — Authorization Refinement

Sprint 12 Authorization Refinement is complete. 313 automated tests established (219 UnitTests, 61 IntegrationTests, 33 Web.Tests). T07 (EditStatus `IsInRole` cleanup) remains blocked/deferred pending an explicit behavioral decision. The next activity is a separate Sprint Planning session.

### Sprint 10 — Dynamic Capability-Based Authorization

Sprint 10 Dynamic Capability-Based Authorization is complete. T15 (Final Verification, Retrospective & Save Point) is complete. v1.6.0 released.

### Completed Sprint 8 Scope

- P0 - Actual Purchasing Source/Documentation Baseline
- P1 - Multiple Purchase Order Item Management
- P2 - Purchase Order Search
- P3 - Purchase Order Filtering
- P4 - Purchase Order Sorting
- P5 - Purchase Order Pagination
- P6 - Inventory Synchronization During Receiving
- P7 - Integrated Purchasing Verification
- D1 - Documentation Synchronization
- D2 - Design Decision Synchronization
- D3 - Final Sprint 8 Retrospective
- D4 - Final Documentation Validation

### Final Project-wide Verification

- [X] Empty database behavior verification
- [X] Explicit query-failure testing
- [X] Authorization regression verification
- [X] Final build verification




# Sprint 7 Final Verification

Sprint 7 Additional Reporting has completed final project-wide verification successfully.

Verified:

- All seven reporting pages
- All seven Excel exports
- All seven PDF exports
- Filtering, sorting, pagination, and navigation
- Full filtered dataset exports
- Multi-page PDF output
- Inventory Valuation Total Inventory Value
- Empty database behavior
- Explicit query failure and database recovery
- Existing authorization and Access Denied behavior
- Authentication, Account Management, 2FA, management, and operations regression
- `dotnet restore`
- `dotnet build`

No in-scope implementation defects were discovered during final verification.

Dynamic Capability-Based Authorization was not implemented as part of Sprint 7.

## Sprint 8 - P2 Purchase Order Search

**Status: Complete and verified**

P2 implements server-side Purchase Order search using the existing Purchase Order listing/query architecture.

Verified behavior:
- Search by Purchase Order ID.
- Search by Supplier Name.
- Empty or whitespace-only search returns the normal unfiltered list.
- No-match searches return the correct empty result state.
- Search state is preserved through the applicable Purchase Order navigation.
- Existing authorization behavior remains intact.
- Existing Purchase Order list behavior outside search remains unchanged.

The project owner completed runtime/browser verification successfully after implementation.

No P3-P6 functionality was implemented as part of P2.

---

# Sprint 8 - P3 Purchase Order Filtering

**Status: Complete and verified**

P3 extends the existing Purchase Order listing with server-side filtering while preserving the P2 search behavior and established Purchasing architecture.

Implemented filters:
- From Date
- To Date
- Purchase Order Status

Verified behavior:
- Each confirmed filter works correctly.
- Multiple filters can be combined.
- Search and filtering work together.
- Empty-result combinations display the existing no-results behavior correctly.
- Filter state is preserved where applicable through the existing Purchase Order navigation.
- Existing authorization behavior remains intact.
- No unrelated Purchase Order behavior was changed.

Runtime/browser verification was completed successfully by the project owner after the P3 implementation.

# Sprint 8 - P4 Purchase Order Sorting

**Status: Complete and verified**

P4 adds server-side Purchase Order sorting using the established shared sorting conventions and the existing Purchase Order listing/query architecture.

Confirmed sort fields:
- Purchase Order ID
- Supplier
- Order Date
- Status
- Total Amount

Verified behavior:
- Ascending sorting works for all supported fields.
- Descending sorting works for all supported fields.
- Sorting integrates with the existing Purchase Order search and filters.
- Sorting state is preserved through applicable Purchase Order navigation and workflow actions.
- Sorting remains server-side in the repository query.
- Existing authorization behavior remains intact.
- No unrelated Purchase Order behavior was changed.
- Purchase Order pagination was not introduced as part of P4.

Runtime/browser verification was completed successfully by the project owner after the P4 implementation.

Next task: **P6 - Inventory Synchronization During Receiving**.



---

# Sprint 8 - P7 Integrated Purchasing Verification

**Status:** Complete and verified

P7 performed the integrated Purchasing regression verification across the complete workflow:

- Create Purchase Order
- Multiple Purchase Order items
- Purchase Order listing
- Search
- From/To date filtering
- Status filtering
- Sorting
- Pagination
- Details
- Submit
- Approve
- Receive
- Inventory synchronization
- Authorization
- Empty-result behavior
- Relevant failure/recovery behavior

During integrated verification, one in-scope regression was identified in Purchase Order pagination: pagination links did not preserve the active `FromDate` and `ToDate` values. The defect was corrected in the Purchase Order listing Razor Page. Pagination now preserves the active search, status, date-filter, page-size, and sorting state through Previous, numbered-page, and Next navigation.

The corrected implementation was runtime/browser tested by the project owner and confirmed working.

No Dynamic Capability-Based Authorization implementation was introduced, and no unrelated Purchasing feature was added.

The P7 implementation fix is intentionally kept separate from documentation changes. Documentation is synchronized in the dedicated P7 documentation update.

**P7 result:** Complete and verified.

D1 - Documentation Synchronization, D2 - Design Decision Synchronization, D3 - Final Sprint 8 Retrospective, and D4 - Final Documentation Validation are complete. The Sprint 8 final save point has been established. The current development activity is Sprint 9 ASP.NET Core Code Quality & Consistency.
