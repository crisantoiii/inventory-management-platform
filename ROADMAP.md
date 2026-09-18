# Roadmap

## Overview

This roadmap outlines the planned evolution of the Inventory Management Platform.

The project is developed incrementally, with each completed module validating the architecture before expanding into additional business domains.

v0.1  Products                 ✅
v0.2  Categories               ✅
v0.3  Suppliers                ✅
v0.4  Customers / Units        ✅
v0.5  Shared Infrastructure    ✅
v0.6  Inventory Transactions   ✅
v0.7  Dashboard                ✅
v0.8  Identity & Users         ✅
v0.9  Architecture Sprint      ✅
v1.0  Purchasing Application   ✅
v1.1  Purchasing Presentation  ✅
v1.2  Reporting                ✅
v1.3  Account Management       ✅
v1.4  Additional Reporting     ✅
v1.5  Purchasing Enhancements  ✅
v1.6  Dynamic Capability Auth  ✅
(Sprint 11 Automated Testing ✅ — non-release; Sprint 12 Authorization Refinement ✅ — non-release; Sprint 13 Purchasing Workflow Test Automation ✅ — non-release; Sprint 14 Purchase Order Cancellation and Draft Item Editing ✅ — non-release; Sprint 15 Purchase Order Workflow Error Handling and UX Hardening ✅ — non-release; Sprint 16 Purchase Order POST Round-Trip State and Create Failure Presentation Corrections ✅ — non-release)

---

# Current Development Strategy

The project has completed its architectural foundation and the first end-to-end Purchasing vertical slice.

Future development will prioritize expanding business capabilities while preserving the validated architecture.

Focus Areas:

- Business workflows
- Domain modeling
- Enterprise features
- Reporting
- APIs
- Incremental vertical slices

Each major business capability should be implemented from Domain and Application logic through a usable Presentation workflow before being considered complete.

### Sprint 7 Status

Sprint 7 Additional Reporting is complete, verified, and documented.

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

Sprint 8 Purchasing Enhancements is complete and closed. P0, P1, P2, P3, P4, P5, P6, and P7 are complete and runtime/browser verified. P7 completed the integrated Purchasing regression pass and corrected one in-scope pagination state-preservation defect.

---

# Sprint 8 - Purchasing Enhancements

## Completed

- P0 - Actual Purchasing Source/Documentation Baseline
- P1 - Multiple Purchase Order Item Management
- P2 - Purchase Order Search
- P3 - Purchase Order Filtering
- P4 - Purchase Order Sorting
- P5 - Purchase Order Pagination
- P6 - Inventory Synchronization During Receiving - Complete and verified
- P7 - Integrated Purchasing Verification - Complete and verified

### P3 - Purchase Order Filtering

Verified filters:
- From Date
- To Date
- Purchase Order Status

Verified:
- Individual filters
- Combined filters
- Search + filter interaction
- Empty-result behavior
- Applicable filter-state preservation
- Existing authorization behavior
- No unrelated Purchasing behavior changed

Runtime/browser verification was completed successfully by the project owner.

## Final Sprint 8 State

- D1 - Documentation Synchronization - Complete
- D2 - Design Decision Synchronization - Complete
- D3 - Final Sprint 8 Retrospective - Complete
- D4 - Final Documentation Validation - Complete

The Sprint 8 final save point has been established. No new feature work begins from this roadmap state; the next activity is Next Sprint Planning.

## Sprint 9 - ASP.NET Core Code Quality & Consistency

**Status:** T03-T13 complete; final documentation and architecture validation

Sprint 9 is a bounded consistency workstream rather than a feature-module release.

Completed implementation scope:

- Razor `asp-for` normalization for Purchase History and Supplier Purchase Analysis.
- Purchase Order sorting/pagination navigation normalization to `asp-route-*`.
- Request-binding consolidation for seven core list PageModels.
- Removal of redundant inherited repository interface declarations.
- Repository query-signature validation without collapsing meaningful feature-specific filters.
- Seven `PageNum` pagination-link corrections.
- Purchase Order Details pagination context preservation.
- Purchase Order Status option de-duplication and filter label/control association corrections.

Verified conventions:

- `PageNum` is the canonical Razor/UI paging property and query parameter.
- `asp-for` is preferred where appropriate for Razor form binding and labels.
- `asp-route-*` is preferred for direct Razor navigation and query state.
- Application Request -> `PagedQuery` / repository boundaries remain when they have distinct responsibilities.
- Rule-of-Three is applied before introducing reusable helpers or abstractions.

Verification limitation:

- Source-level verification is complete through T13.
- The supplied environment has no `dotnet` CLI, so no successful Sprint 9 build or runtime/browser verification is claimed.
- No automated test project/source is present in the repository.

Sprint 9 did not introduce unrelated business capabilities or structural architectural redesign.

## Sprint 11 - Automated Testing & Test Automation

**Status:** Complete

Sprint 11 established the project's first automated testing foundation and implemented risk-based automated coverage for the highest-value Domain, Application authorization, authorization seeding, and authorization repository behaviors.

### Completed

- T01: Test Infrastructure Foundation (xUnit, EF Core InMemory)
- T02: PurchaseOrder Domain Tests (101 tests)
- T03: Product Domain Tests (56 tests)
- T04: Authorization Domain Tests (46 tests)
- T05: CapabilityAuthorizationService Tests (15 tests)
- T07: AuthorizationSeeder Integration Tests (24 tests)
- T08: Authorization Repository Integration Tests (36 tests)
- T09: CI / Automated Test Execution (provider-neutral baseline)
- T10: Test Conventions and Sprint Documentation
- T11: Documentation Synchronization and Sprint Closure

### Deferred

- T06: Authorization Handler Tests → Sprint 12

### Final Test Baseline (Sprint 11 historical baseline; superseded by the Sprint 12 total below)

```text
UnitTests:        219 passed
IntegrationTests:  61 passed
Total:            280 passed, 0 failed
Build:            0 errors, 0 warnings
```

### Sprint 12 Direction

Sprint 12 should focus on:
- Web Authorization Handler Tests (CapabilityAuthorizationHandler, MultiCapabilityAuthorizationHandler)
- WebApplicationFactory integration tests
- Razor Page authorization integration tests
- CI provider establishment (if repository hosting is confirmed)

## Sprint 12 - Authorization Refinement

**Status:** Complete

Sprint 12 hardened the capability-based authorization model through automated Web authorization-handler testing and remediation of two confirmed authorization-boundary defects, without changing the authorization architecture.

### Completed

- T01: `InventoryPlatform.Web.Tests` project foundation (references Web only)
- T02: `FakeCapabilityAuthorizationService` hand-written test double (no mocking framework)
- T03: `CapabilityAuthorizationHandlerTests` (8 tests)
- T04: `MultiCapabilityAuthorizationHandlerTests` (12 tests, OR semantics with short-circuit)
- T05: Categories/Edit remediation — `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]`
- T06: Suppliers/Create remediation — `ViewInventory` replaced with `InventoryManagement`
- T08: Integrated verification — passed (no authorization regression)
- T09: Documentation synchronization and Sprint 12 closure

### Blocked/Deferred

- T07: EditStatus `User.IsInRole(InventoryManager)` cleanup — the remaining occurrence (line 61) is a reachable, behavior-affecting self-deactivation guard for supported multi-role users, NOT dead code. Removal would change observable behavior and requires an explicit behavioral decision.

### Final Test Baseline

```text
UnitTests:        219 passed
IntegrationTests:  61 passed
Web.Tests:         33 passed
Total:            313 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings)
```

## Sprint 13 - Purchasing Workflow Test Automation

**Status:** Complete

Sprint 13 extended the established risk-based automated testing program to the Purchasing workflow with layered automated coverage: the Purchasing Application layer (six handlers, two Create validators, the `PurchaseOrderErrors` contract) as primary scope, plus `PurchaseOrderRepository` integration verification as supporting scope. No production code was changed by the sprint.

### Completed

- T01: Purchasing Test Support Foundation (`FakePurchaseOrderRepository`, `FakeUnitOfWork`, `PurchasingTestData`, `EntityIdHelper`)
- T02: CreatePurchaseOrderHandler + Validator Tests (52 discovered cases)
- T03: Workflow Transition Handler Tests — Submit/Approve (13 discovered cases)
- T04: ReceivePurchaseOrderHandler Tests (14 tests)
- T05: Purchase Order Query Handler Tests (16 tests)
- T06: PurchaseOrderRepository Integration Tests (24 tests, EF Core InMemory with fresh-context isolation)
- T07: Integrated Verification — 432 passed / 0 failed / 0 skipped (314 UnitTests, 85 IntegrationTests, 33 Web.Tests); full rebuild 28 warnings / 0 errors, baseline preserved
- T08: Documentation Synchronization & Sprint 13 Closure

### Final Test Baseline

```text
UnitTests:        314 passed
IntegrationTests:  85 passed
Web.Tests:         33 passed
Total:            432 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings)
```

### Boundaries Held

- No production changes, no new packages, no database/migration/seed changes, no CI changes, no WebApplicationFactory
- Repository integration tests use EF Core InMemory: repository wiring/query-shape regression coverage only — not SQL Server integration testing
- Sprint 12 EditStatus item and the T05 `PagedRequest.Status` pass-through finding remain deferred, untouched

## Sprint 15 - Purchase Order Workflow Error Handling and UX Hardening

**Status:** Complete/Closed

Sprint 15 hardened the Purchase Order Details presentation boundary: all four POST workflows (Submit, Approve, Receive, Cancel) now catch expected `DomainException` failures and render the canonical Domain message as inline validation feedback (ModelState + Purchase Order reload via `GetPurchaseOrderHandler` + `Page()`, through one private local helper; helper reload failure returns `NotFound()`), instead of propagating to the Development exception page. Authorization remains before try/catch and unchanged; existing Application Result failures keep their ModelState + reload + Page() semantics; unexpected exceptions propagate. Rejected operations persist no state changes (SQL before/after and restart evidence in T04); successful paths are unchanged.

### Completed

- T01: Contract Verification and Design Lock
- T02: Purchase Order Details Workflow Error Handling (`Details.cshtml.cs` only)
- T03: Regression and Coverage Verification (all suites green; Result/NotFound semantics documented correctly)
- T04: Integrated and Manual Verification (real-browser failure/success/authorization/NotFound scenarios with SQL before/after, restart persistence check, migration-history check)
- T05: Documentation Synchronization and Sprint Closure

### Final Test Baseline

```text
UnitTests:        346 passed
IntegrationTests:  92 passed
Web.Tests:         39 passed
Total:            477 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings)
```

### Boundaries Held

- No production source changed outside `Details.cshtml.cs` (T02); no test, migration/schema, authorization/seed, package, or project changes
- No artificial PageModel test seam introduced
- `Descending=True` hidden-field POST round-trip loss (Details and Edit pages) is a pre-existing markup issue, deferred to a future approved task — representative navigation/query state was preserved except for this issue
- No version assigned, no tag created, no release published (non-release technical-hardening sprint)

## Sprint 14 - Purchase Order Cancellation and Draft Item Editing

**Status:** Complete

Sprint 14 completed the Purchase Order lifecycle: authorized cancellation is now available from Draft and Submitted states through the full stack (Domain aggregate `Cancel()`, Application cancellation workflow, `PurchaseOrder.Cancel` capability, Razor Pages cancellation workflow on Details), and Draft Purchase Orders support item editing (Quantity/UnitCost) and item removal through the dedicated `Pages/Purchasing/PurchaseOrders/Edit.cshtml` surface backed by `PurchaseOrder.Edit`. `Cancelled` is terminal. Cancellation is forbidden from Approved, Receiving, Completed, and Cancelled states; item mutation is keyed by `ProductId` and only Draft orders can be edited; removing the final item is allowed while empty Purchase Orders still cannot be submitted.

### Completed

- T01: Sprint 14 Contract Verification and Implementation Readiness
- T02: Domain Cancellation Transition
- T03: Application Cancellation Workflow
- T04: Application Draft Item Editing Workflow
- T05: Purchase Order Edit/Cancel Authorization (`PurchaseOrder.Edit`, `PurchaseOrder.Cancel` — catalog 39 → 41)
- T06: Purchase Order Persistence Integration Coverage (EF Core InMemory, fresh-context isolation)
- T07: Web Cancellation Workflow
- T08: Web Draft Item Edit Workflow (dedicated Edit page)
- T09: Integrated and Manual Verification — automated suites re-executed plus mandatory manual browser verification against SQL Server
- T10: Documentation Synchronization and Sprint 14 Closure

### Final Test Baseline

```text
UnitTests:        346 passed
IntegrationTests:  92 passed
Web.Tests:         39 passed
Total:            477 passed, 0 failed, 0 skipped
Build:             0 errors (full rebuild: 28 pre-existing warnings; no Sprint 14 warning regression)
```

### Boundaries Held

- No schema migration, no EF mapping change, no data backfill — the existing `Cancelled = 6` status is used
- Capability authorization remains additive within the existing dynamic capability model (not role-only authorization)
- Manual browser/provider verification against SQL Server remains manual — it is not automated end-to-end or SQL Server integration testing
- No version assigned, no tag created, no release published

## Sprint 16 - Purchase Order POST Round-Trip State and Create Failure Presentation Corrections

**Status:** Complete/Closed — non-release sprint; v1.6.0 remains the release baseline.

Sprint 16 corrected the six Purchase Order Details/Edit hidden `Descending` values and added narrow inline handling for expected Create `DomainException` failures. A 12-case true/false runtime matrix passed for Submit, Approve, Cancel, Receive, UpdateItem, and RemoveItem. Duplicate-product, zero-quantity, and negative-cost Create failures rendered inline without persistence; the successful workflow and authorization checks also passed. The final automated baseline is 477/0/0 (346 UnitTests, 92 IntegrationTests, 39 Web.Tests), with 0/0 normal-build diagnostics and 28 pre-existing warnings/0 errors non-incrementally. Candidates C, D1, D4, and E remain deferred.

## Next Sprint Planning

Sprint 11 through Sprint 16 are complete. Sprint 16 achieved its round-trip and Create-presentation objectives with all verification gates green and all scope boundaries held. The next activity is a separate Sprint Planning session; no next-sprint scope has been defined.

## D1 - Documentation Synchronization

**Status: Complete**

Current-state documentation now reflects the verified Sprint 8 Purchasing sequence through P7, including the integrated verification result and the corrected pagination date-filter state preservation.

No implementation behavior was changed during D1.

## Later
- Additional Purchasing User Experience Improvements

Dynamic Capability-Based Authorization remains outside the completed Purchasing implementation scope. It is the next locked priority after Sprint 8 closure and must not be started automatically as part of this handoff.

---

# Current Release

## Version 1.5.0 - Sprint 8 Purchasing Enhancements

### Completed

- Multiple Purchase Order Item Management
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination
- Inventory Synchronization During Receiving
- Integrated Purchasing Verification
- D1-D4 Sprint 8 documentation and closure

### Verification

- Complete Purchasing workflow from creation through receiving
- Search, date/status filtering, sorting, and pagination
- Inventory synchronization and StockIn transaction creation
- Existing authorization boundaries
- Relevant empty-result and failure/recovery behavior
- Pagination state preservation after the P7 correction

Sprint 8 is closed. Sprint 9 is the current code-quality workstream and does not change the release version.

---

# Historical Release

## Version 1.4.0 – Additional Reporting & Exports

### Completed

#### Profile

- User Profile
- Update Profile
- Self-Service Account Management

#### Password Management

- Change Password
- Forgot Password
- Reset Password
- Force Password Change

#### Email Verification

- Email Verification
- Verification Request
- Email Confirmation

#### Two-Factor Authentication

- 2FA Setup
- TOTP Verification
- 2FA Login Challenge
- Recovery Codes
- Recovery Code Login
- Recovery Code Regeneration
- Recovery Code Invalidation
- Disable 2FA

### Result

The Account Management vertical slice is now complete.

The implementation provides authenticated users with self-service
account management capabilities while preserving the existing
Clean Architecture, Vertical Slice Architecture, Application
handler patterns, Identity abstraction, and Razor Pages workflows.

The completed functionality includes:

- Self-service user profile management
- Password management
- Email verification
- Two-factor authentication
- Authenticator-based TOTP verification
- Recovery-code authentication
- Recovery-code regeneration
- Recovery-code invalidation
- 2FA disablement

The implementation was verified through actual browser workflows
and completed without requiring structural architectural redesign.

---

# Phase 1 — Foundation ✅

---

# Phase 2 — Inventory Core ✅

---

# Phase 3 — Identity & User Management ✅

---

# Phase 4 – Architecture Sprint 1

Objectives

- Review overall solution architecture
- Apply Rule of Three refactoring where justified
- Improve shared UI components
- Standardize Razor Page patterns
- Review dependency registration
- Update project documentation
- Prepare foundation for Purchasing

---

# Phase 5 — Purchasing Module

Status: ✅ Complete

Completed:

- Purchase Orders
- Purchase Order Items
- Purchase Approval Workflow
- Goods Receiving
- Partial Receiving
- Purchase Order Completion
- Purchasing Presentation Layer
- End-to-End Workflow Validation
- Inventory Integration
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination

---

# Phase 6 — Reporting

Status: ✅ Complete

### Completed

- Inventory Valuation
- Purchase History
- Purchase History Search
- Purchase History Date Filtering
- Purchase History Pagination
- Purchase History Sorting
- Supplier Purchase Analysis
- Supplier Purchase Analysis Search
- Supplier Purchase Analysis Date Filtering
- Supplier Purchase Analysis Status Filtering
- Supplier Purchase Analysis Pagination
- Supplier Purchase Analysis Sorting
- Supplier Purchase Analysis Purchase Period
- Stock Movement
- Stock Movement Search
- Stock Movement Date Filtering
- Stock Movement Movement Type Filtering
- Stock Movement Pagination
- Stock Movement Sorting
- Low Stock Report
- Low Stock Search
- Low Stock Pagination
- Low Stock Sorting
- Inventory Movement Report
- Inventory Movement Search
- Inventory Movement Date Filtering
- Inventory Movement Reporting Period
- Inventory Movement Pagination
- Inventory Movement Sorting
- Product Reports
- Product Reports Search
- Product Reports Status Filtering
- Product Reports Pagination
- Product Reports Sorting
- Excel Export
- PDF Export

### Export Options

- Excel
- PDF

### Final Verification

- Empty database behavior verification
- Explicit query-failure testing
- Authorization regression
- Final build verification

### Additional Reporting

Additional Reporting was completed as Sprint 7 within the broader Phase 6 Reporting roadmap.

Completed Sprint 7 scope:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel export
- PDF export
- Final project-wide verification

---

# Phase 7 — Dynamic Capability-Based Authorization

Status: ✅ Complete

**Implementation status:** T01-T14 complete. T15 (Final Verification & Retrospective) complete. v1.6.0 released.

### Objective

Evolve the existing Identity-based authorization model into a
dynamic capability-based authorization model.

### Model

```text
User
  ↓
Group
  ↓
Capabilities
  ↓
Application Action
  ↓
Domain State Validation
```

### Capabilities

Capabilities represent atomic application functionality,
actions, or permissions.

Examples:

- PurchaseOrder.View
- PurchaseOrder.Create
- PurchaseOrder.Edit
- PurchaseOrder.Submit
- PurchaseOrder.Approve
- PurchaseOrder.Reject
- PurchaseOrder.Receive

### Groups

Groups compose reusable capabilities into business
responsibilities.

Examples:

- PO Account
- IT Account
- Inventory Manager
- Viewer
- Administrator

### Implementation Goals

- Define capability catalog
- Define groups
- Map groups to capabilities
- Assign groups to users
- Introduce capability authorization
- Preserve existing Identity infrastructure where appropriate
- Preserve Domain state validation
- Apply authorization to Purchasing workflow
- Apply authorization to Reporting
- Validate UI and server-side authorization behavior

### Implementation Summary

- T01-T12: Full implementation (domain model, application abstractions, persistence, handlers, policies, admin UI, UI visibility)
- T13: Runtime verification complete (build success, authorization verified for all 3 seeded users)
- T14: Documentation synchronization complete
- T15: Final verification, retrospective, and save point — Complete

### Deferred Findings from T13

- InventoryManager group includes Administration.Access (seed data issue)
- InventoryManagement OR-composite grants broad access via single capability
- Categories/Edit missing [Authorize] attribute (pre-existing gap)
- Reports unrestricted (design decision pending)

### Sequencing

Sprint 10 implementation is complete. T15 closure gate is BLOCKED due to P1 findings requiring database remediation. Documentation and retrospective remain.

---

# Phase 8 — Advanced Features

Status: ⏳ Planned

Planned:

- Audit Trail
- Activity Logs
- File Uploads
- Barcode Scanner Integration
- Product Images
- QR Code Support
- Email Notifications
- Bulk Import
- Bulk Export

---

# Long-Term Goals

Future enhancements may include:

## Business Modules

- Purchasing Enhancements
- Sales
- Warehouse
- Inventory Transfers
- Cycle Counts
- Returns
- Stock Adjustments Approval

## Integrations

- REST API
- Barcode Scanner Integration

## Client Applications

- Mobile Application

## Intelligence

- Inventory Forecasting

---

# Planned Releases

| Version | Milestone |
|---------|-----------|
| v0.9.0 | Architecture Sprint 1 ✅ |
| v1.0.0 | Purchasing Application Layer ✅ |
| v1.1.0 | Purchasing Presentation Layer ✅ |
| v1.2.0 | Reporting — Inventory Valuation ✅ |
| v1.3.0 | Account Management ✅ |
| v1.4.0 | Additional Reporting & Exports — Released |
| v1.5.0 | Sprint 8 Purchasing Enhancements — Released |
| v1.6.0 | Dynamic Capability-Based Authorization ✅ |
| v2.0.0 | REST API & Blazor ⏳ |

---

# Guiding Principles

Each new module should:

- Reuse the shared paging infrastructure.
- Reuse the shared filtering infrastructure.
- Reuse the shared sorting infrastructure.
- Follow Clean Architecture.
- Maintain consistent UI behavior.
- Prefer composition over duplication.
- Reuse established application handler patterns.
- Maintain consistent Razor Pages workflows.
- Keep business rules inside domain entities.
- Favor consistency over premature abstraction.
- Prefer DTO projections for read-only reporting features.
- Encapsulate framework-specific implementations behind application abstractions.
- Apply the Rule of Three before introducing shared abstractions.
- Prefer complete vertical slices over isolated technical implementations.
- Validate new workflows through real application usage before considering the feature complete.
- Validate read-oriented queries against actual EF Core translation before introducing client-side evaluation.

The architecture should evolve through reuse rather than introducing module-specific implementations whenever possible.

## Sprint 7 Final Verification

- [x] Final Project-wide Verification

### P2 - Purchase Order Search - Complete

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

D1 documentation synchronization was completed after the Sprint 8 P7 verification.
