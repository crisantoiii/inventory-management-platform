# InventoryPlatform - Sprint 8 Planning Baseline

**Sprint:** Sprint 8 - Purchasing Enhancements  
**Repository/Branch:** `feature/purchasing_enhancements`  
**Baseline Date:** 2026-08-19  
**Status:** P0-P4 complete and runtime/browser verified; P5 - Purchase Order Pagination is next

---

## 1. Sprint 7 Closure

Sprint 7 - Additional Reporting & Exports is complete, verified, and documented.

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

Final project-wide verification covered:

- Authentication
- Account Management and 2FA
- Product, Category, Supplier, and Customer management
- Purchase Orders
- Inventory operations
- All seven reporting pages
- Reporting filters, sorting, pagination, navigation, and no-result behavior
- All seven Excel exports
- All seven PDF exports
- Full filtered export behavior
- Inventory Valuation Total Inventory Value
- Empty database behavior
- Explicit query failure and database recovery
- Existing authorization boundaries
- Final `dotnet restore` and `dotnet build`

The final Sprint 7 verification was performed on `feature/additional-reporting`, with a clean working tree after temporary verification configuration was restored.

Sprint 7 also confirmed that Dynamic Capability-Based Authorization was not implemented and remains a future architecture direction.

### Sprint 7 Lessons to Carry Forward

1. Reuse established architecture instead of introducing feature-specific parallel patterns.
2. Keep business behavior in Domain entities and aggregates.
3. Keep Application handlers focused on orchestration.
4. Keep Razor PageModels thin.
5. Keep persistence concerns behind repository/application abstractions.
6. Use dedicated read models for read-oriented reporting.
7. Keep filtering, sorting, and pagination server-side.
8. Validate EF Core queries against actual translation and runtime behavior.
9. Prefer complete vertical slices over isolated technical changes.
10. Perform browser/manual verification before declaring a workflow complete.
11. Keep implementation commits separate from documentation commits.
12. Synchronize documentation only after behavior has been verified.

---

## 2. Architecture and Patterns to Preserve

The validated solution structure is:

```text
InventoryPlatform
|
+-- InventoryPlatform.Web
+-- InventoryPlatform.Application
+-- InventoryPlatform.Domain
+-- InventoryPlatform.Infrastructure
+-- InventoryPlatform.Shared
```

The established workflow-oriented Purchasing architecture is:

```text
Razor Page
     |
     v
Application Handler
     |
     v
PurchaseOrder Aggregate
     |
     v
Repository / Unit of Work
     |
     v
Entity Framework Core
     |
     v
SQL Server
```

The aggregate remains responsible for business rules and state transitions.

The established Purchasing workflow is:

```text
Draft
  |
  v
Submitted
  |
  v
Approved
  |
  v
Receiving
  |
  v
Completed
```

The Purchasing vertical slice already supports:

- Purchase Order creation
- Purchase Order retrieval
- Purchase Order listing
- Submission
- Approval
- Partial receiving
- Final receiving
- Completed state
- Supplier selection
- Product selection
- Ordered quantity display
- Received quantity display
- Remaining quantity display
- Purchase Order total calculation
- Client-side validation
- Domain validation
- Success feedback
- Query-failure feedback

Shared infrastructure to preserve includes:

- `PagedRequest`
- `PagedQuery`
- `PagedResult<T>`
- Shared filtering infrastructure
- Shared sorting infrastructure
- `Result`
- `Result<T>`
- Dependency injection conventions
- Repository pattern
- Unit of Work
- Feature-first organization
- Vertical Slice Architecture
- Rich Domain Model
- Thin PageModels
- Thin Application handlers
- Request/Response/Handler patterns

No structural architectural redesign is planned for Purchasing Enhancements unless actual source inspection proves that an existing boundary cannot support the required behavior.

---

## 3. Sprint 8 Objective

Sprint 8 begins with **Purchasing Enhancements** as its exclusive implementation focus.

The objective is to evolve the existing Purchasing vertical slice from its current core workflow into a more complete operational Purchasing capability while preserving the validated architecture.

The sprint must:

- Extend existing Purchasing behavior rather than replace it.
- Reuse the existing Purchase Order aggregate and workflow where appropriate.
- Improve Purchase Order item management.
- Add operational Purchase Order discovery capabilities.
- Integrate receiving with inventory where required by the verified business rules.
- Preserve domain validation and transactional integrity.
- Maintain consistent Razor Pages behavior.
- Preserve existing authorization until the dedicated authorization phase.

---

## 4. Purchasing Business and Portfolio Value

Purchasing is already the platform's first workflow-driven business module. Enhancing it increases the practical value of the platform by connecting:

```text
Supplier
   |
   v
Purchase Order
   |
   v
Purchase Order Items
   |
   v
Approval
   |
   v
Receiving
   |
   v
Inventory
```

The enhancements are intended to make Purchasing operationally useful beyond the current core demonstration workflow.

Portfolio value includes demonstrating:

- Rich domain modeling
- Aggregate-based workflow management
- Multi-item transactional behavior
- Search/filter/sort/pagination patterns
- Inventory integration
- Transactional consistency
- End-to-end business workflow validation
- Reuse of shared infrastructure
- Controlled incremental architecture evolution

---

## 5. Locked Sprint Priority Order

The Sprint 8 priority order is locked as:

1. Purchasing Enhancements
2. Dynamic Capability-Based Authorization
3. Sales Module
4. Audit / Activity Logging
5. Bulk Import / Export
6. Barcode / QR

Only priority 1 is active during the Purchasing Enhancements work.

Dynamic Capability-Based Authorization must not be implemented as part of Purchasing Enhancements.

---

## 6. Repository and Branch Strategy

Sprint 8 Purchasing Enhancements uses:

```text
feature/purchasing_enhancements
```

The Sprint 7 branch:

```text
feature/additional-reporting
```

remains the historical Sprint 7 baseline.

Sprint 7 implementation history must not be modified as part of Sprint 8.

`main` must not be used as the active development branch.

The first Sprint 8 implementation work must begin only after the actual repository and branch state have been inspected and confirmed.

Repository creation, if required by the Sprint 8 environment, is part of Sprint 8 initial setup and must preserve the documented architecture and history baseline.

---

## 7. Purchasing Scope

The current source/documentation establishes the following Purchasing enhancement areas:

### In Scope

- Multiple Purchase Order Item Management
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination
- Inventory Integration During Receiving
- Additional Purchasing User Experience Improvements

These areas extend the already implemented Purchase Order lifecycle.

### Explicitly Preserved Existing Behavior

- Draft -> Submitted -> Approved -> Receiving -> Completed workflow
- Purchase Order aggregate ownership of business state
- Domain validation as the authoritative business-rule boundary
- Existing repository and Unit of Work patterns
- Existing Razor Pages architecture
- Existing Identity authorization model until the authorization phase

---

## 8. Explicit Non-Scope

The following are outside Purchasing Enhancements:

- Dynamic Capability-Based Authorization implementation
- Sales Module
- Audit / Activity Logging
- Bulk Import / Export as a platform-wide feature
- Barcode / QR implementation
- REST API
- Blazor
- Mobile application
- Inventory forecasting
- Unrelated reporting changes
- Architectural redesign without evidence requiring it
- Refactoring solely for stylistic preference
- Unplanned schema changes without an approved business requirement
- Changes to Sprint 7 reporting behavior unless required by a verified regression

Dynamic Capability-Based Authorization remains a separate future implementation phase.

---

## 9. Task Sequence P0-P7

> **Sprint 8 task sequence revision:** The originally drafted P4-P7 labels were superseded by the approved task prompts. The current sequence places **P4 - Purchase Order Sorting** and **P5 - Purchase Order Pagination** before inventory integration and later regression/completion work.

The task labels below are established by this planning baseline because no prior Sprint 8 P0-P7 task definition was found in the available project documentation.

### P0 - Actual Purchasing Source / Documentation Baseline

Inspect and document the actual repository state before implementation.

Required:

- Confirm repository
- Confirm branch
- Confirm working-tree state
- Review Purchasing Domain entities
- Review Purchase Order aggregate behavior
- Review Purchase Order item behavior
- Review Application handlers
- Review repository interfaces and implementations
- Review EF Core configurations
- Review migrations
- Review Purchasing Razor Pages
- Review shared paging/filtering/sorting infrastructure
- Review current authorization boundaries
- Review relevant documentation
- Identify exact files affected by each planned enhancement

No code changes are permitted during P0 unless required to establish the baseline and explicitly approved.

### P1 - Multiple Purchase Order Item Management

Improve Purchase Order item management using the actual existing domain and presentation patterns.

**Current implementation status:** Source implementation completed for multi-item Purchase Order creation in the Create UI. Runtime/browser verification completed successfully.

Primary concern:

- Support multi-item Purchase Order creation without bypassing the PurchaseOrder aggregate.

### P2 - Purchase Order Search

**Status: Complete and verified**

P2 implements server-side Purchase Order search using the actual Purchase Order listing/query architecture.

Verified behavior:
- Purchase Order ID search.
- Supplier Name search.
- Empty/whitespace search handling.
- No-result handling.
- Search-state preservation through applicable navigation.
- Existing authorization remains intact.
- No unrelated Purchase Order list behavior changed.

Runtime/browser verification was completed successfully by the project owner.

The next task is **P5 - Purchase Order Pagination**.

### P3 - Purchase Order Filtering

**Status: Complete and verified**

P3 extends Purchase Order listing with confirmed server-side filters:

- From Date
- To Date
- Purchase Order Status

Acceptance/verification result:
- Confirmed filters work correctly.
- Multiple filters work together.
- Existing P2 search integrates with filtering.
- Empty results behave correctly.
- Applicable filter state is preserved.
- Existing authorization behavior remains intact.
- No unrelated behavior was changed.

Runtime/browser verification was completed successfully by the project owner.

### P4 - Purchase Order Sorting

**Status: Complete and verified**

Extend Purchase Order listing with server-side sorting using the established shared sorting patterns.

Confirmed sort fields:
- Purchase Order ID
- Supplier
- Order Date
- Status
- Total Amount

Ascending and descending sorting were runtime/browser verified. Sorting integrates with the existing P2 Search and P3 Filtering behavior.

### P5 - Purchase Order Pagination

**Next task**

Extend Purchase Order listing with server-side pagination using the established shared paging infrastructure while preserving Search, Filtering, and Sorting behavior.



## 23. P3 - Purchase Order Filtering

P3 is complete and runtime/browser verified.

The Purchase Order listing now supports the confirmed server-side filters:

- From Date
- To Date
- Purchase Order Status

The implementation preserves P2 search and allows search and filtering to operate together.

### P3 Acceptance Result

**Status: COMPLETE**

Verified by the project owner through runtime/browser testing.

Verified:
- Individual filters
- Combined filters
- Search + filter interaction
- Empty-result behavior
- Applicable filter-state preservation
- Existing authorization boundaries
- No unrelated Purchase Order behavior changes

P4 - Purchase Order Sorting is complete and runtime/browser verified.

P5 - Purchase Order Pagination is the next task.

### P3 Documentation Audit

The following current-state documentation was synchronized with the verified P3 behavior:

- `PROJECT_STATUS.md`
- `docs/FEATURES.md`
- `ROADMAP.md`
- `README.md`
- `CHANGELOG.md`
- `docs/ENGINEERING_JOURNAL.md`
- `docs/retrospectives/SPRINT_08_PLANNING_BASELINE.md`

Historical Sprint 7 documentation remains unchanged.

No Dynamic Capability-Based Authorization documentation was marked as implemented.


## 24. P4 - Purchase Order Sorting

P4 is complete and runtime/browser verified.

The Purchase Order listing supports server-side sorting for:

- Purchase Order ID
- Supplier
- Order Date
- Status
- Total Amount

Both ascending and descending directions were verified. Sorting integrates with the existing Search and Filtering behavior and preserves applicable state through Purchase Order navigation and workflow actions.

Current-state documentation was synchronized after verification. Historical Sprint 7 documentation remains unchanged.

P5 - Purchase Order Pagination is the next task.
