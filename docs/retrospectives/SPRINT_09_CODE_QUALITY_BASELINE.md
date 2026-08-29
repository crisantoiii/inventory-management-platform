# InventoryPlatform - Sprint 9 Code Quality Baseline

**Task:** T01 - Repository & Documentation Baseline  
**Date:** 2026-08-22  
**Scope:** Inspection only - no code refactoring

## 1. Governing Baseline

Sprint 9 is limited to controlled ASP.NET Core/Razor code-quality and consistency work. The governing rules require validating the existing architecture before changing anything, preserving model binding/validation/accessibility/behavior, applying the Rule-of-Three, and avoiding unrelated redesign.

## 2. Repository Inventory

The supplied repository contains:

- 5 application projects: Web, Application, Domain, Infrastructure, Shared
- 74 `.cshtml` files
- 68 `.cshtml.cs` files in the supplied source tree, including Identity area pages
- 65 Web PageModels under `InventoryPlatform.Web/Pages`
- 69 Razor Pages under `InventoryPlatform.Web/Pages`
- 11 application DTO types
- 62 request/query-named application types
- 32 repository/interface/implementation files matching `*Repository`
- No test project or test source files were found in the supplied repository

## 3. Razor Binding and Navigation Inventory

### `asp-for`

There are **218** `asp-for` usages across the Razor Pages inspected.

The established CRUD/create/edit pages generally use tag-helper binding. Purchasing and several reporting pages also use `asp-for`.

### `asp-route-*`

There are **655** `asp-route-*` usages.

Most listing, sorting, pagination, details, activation/deactivation, and report-navigation links use route tag helpers consistently.

### Manual and programmatic URLs

The source contains two distinct application-navigation patterns that should be tracked separately:

**Hard-coded application URLs**
1. `InventoryPlatform.Web/Pages/Dashboard/Index.cshtml`
   - `/Dashboard`
   - `/InventoryTransactions/Index`

**Programmatic Razor URL generation**
2. `InventoryPlatform.Web/Pages/Purchasing/PurchaseOrders/Index.cshtml.cs`
   - `GetSortUrl(...)` uses `Url.Page(...)`
   - `GetPageUrl(...)` uses `Url.Page(...)`

The Purchase Order implementation therefore uses programmatic URL generation for both sorting and pagination, while the surrounding listing pages predominantly express route state with `asp-route-*`.

The `_Layout.cshtml` `href="#"` dropdown trigger is intentionally **not** classified as an application-navigation irregularity; it is a UI/JavaScript trigger rather than a page URL.

## 4. GET Binding Inventory

### `[FromQuery]`

Ten `[FromQuery]` usages were found:

- `Pages/Products/Index.cshtml.cs`
- `Pages/Categories/Index.cshtml.cs`
- `Pages/Suppliers/Index.cshtml.cs`
- `Pages/Customers/Index.cshtml.cs`
- `Pages/Units/Index.cshtml.cs`
- `Pages/InventoryTransactions/Index.cshtml.cs`
- `Pages/Administrator/Users/Index.cshtml.cs`
- `Pages/Dashboard/Index.cshtml.cs`
- `Pages/Reports/SupplierPurchaseAnalysis/Index.cshtml.cs` - two handler parameters

The first eight pages bind an application request/query object from the query string. The Supplier Purchase Analysis page additionally uses `[FromQuery(Name = ...)]` directly on handler parameters.

The README explicitly says not to use `[FromQuery]` merely as a style rule; therefore these are **review candidates**, not automatic defects.

### `[BindProperty]`

There are **74** `BindProperty` attributes in Web PageModels, of which **40** use `SupportsGet = true`.

The main established CRUD pages use `[FromQuery]` request-object binding, while Purchasing and several Reporting pages use PageModel properties with `[BindProperty(SupportsGet = true)]`.

## 5. PageModel GET-Binding Patterns

The supplied code contains three materially different GET-binding styles for list/report pages:

### Pattern A - Application Request object bound from query

Used by:

- Products
- Categories
- Suppliers
- Customers
- Units
- Inventory Transactions
- Administrator Users
- Dashboard

Example architecture:

`Razor Page -> [FromQuery] Request/Query -> Application Handler`

The CRUD request types inherit `PagedRequest`.

### Pattern B - PageModel properties bound individually

Used by:

- Purchasing Purchase Orders
- Reports/InventoryMovement
- Reports/ProductReports
- Reports/StockMovement
- Reports/LowStock
- Purchasing Purchase Order Details (for preserved list/filter context)

These pages then construct a `PagedQuery` and/or Application Request.

### Pattern C - Handler parameters bound directly

Used by:

- Reports/PurchaseHistory
- Reports/SupplierPurchaseAnalysis

These handlers receive `search`, dates, status, sorting, and paging parameters directly. The PageModels then construct the Application Request.

This is the clearest Sprint 9 consistency hotspot because the same conceptual HTTP query state is represented differently across adjacent reporting features.

## 6. Query / Filter / Sort / Paging Duplication

### Repeated PageModel state

The following concepts are repeated directly as PageModel properties:

- `Search` - 8 PageModels
- `FromDate` - 6 PageModels
- `ToDate` - 6 PageModels
- `SortBy` - 8 PageModels
- `Descending` - 8 PageModels
- `PageSize` - 5 PageModels
- `PageNum` - 4 PageModels
- `Status` - 5 PageModels

These repetitions are not automatically invalid. They need to be evaluated against the existing Razor boundary and the project's Rule-of-Three.

### `PagedRequest` vs `PagedQuery`

The shared project contains both:

- `InventoryPlatform.Shared.Paging.PagedRequest`
- `InventoryPlatform.Shared.Paging.PagedQuery`

`PagedRequest` is used by application request types such as `GetProductsRequest`, `GetCustomersRequest`, `GetPurchaseOrdersRequest`, `GetUsersRequest`, etc.

Handlers for those features commonly construct a separate `PagedQuery` before calling the repository.

Other reporting request types instead carry a `PagedQuery` directly, for example:

- `GetLowStockRequest`
- `GetProductReportsRequest`
- `GetInventoryMovementRequest`
- `GetStockMovementRequest`

This means the repository-facing paging representation is already established, but the Application request boundary is inconsistent. The README explicitly permits meaningful boundaries such as `HTTP Request -> Application Request -> Repository Query`; therefore this is a **review candidate for consistency**, not proof that one representation must be removed.

### Paging parameter names

The reporting pages currently use multiple names for the same conceptual page number:

- `Page`
- `PageNum`
- `page`
- `currentPage`

For Sprint 9, the intended **UI/Razor convention is `PageNum`**.

Rationale: `Page` is already a meaningful ASP.NET Core/Razor Page boundary concept, and using `Page` as a UI/PageModel property can create ambiguity/conflict with the Razor Page `Page` property/context. `PageNum` makes the UI-layer paging concept explicit and avoids that conflict while remaining clear.

The current source still contains:

- Inventory Movement: `PageNum`
- Product Reports: `PageNum`
- Stock Movement: `Page`
- Low Stock: both `PageNum` and `Page` appear in the route surface
- Purchase History: `page`
- Supplier Purchase Analysis: `currentPage`

Therefore the Sprint 9 consistency target is to standardize the **Razor/UI-facing paging representation on `PageNum`**, while preserving whatever Application-layer property names are architecturally appropriate.

This is specifically a UI-boundary naming decision. It does **not** imply that every backend/Application property must be renamed to `PageNum`.

## 7. Reporting Consistency Findings

Seven report PageModels were inspected.

- Inventory Valuation is a non-paginated report.
- Inventory Movement uses `PagedQuery` request wrapping.
- Product Reports uses `PagedQuery` request wrapping.
- Stock Movement uses `PagedQuery` request wrapping.
- Low Stock uses `PagedQuery` request wrapping.
- Purchase History uses a `PagedRequest`-derived Application Request.
- Supplier Purchase Analysis uses a `PagedRequest`-derived Application Request.

The last two also use direct handler parameters for HTTP values, while the other paginated reports use PageModel properties.

This produces duplicated request construction and multiple naming/binding conventions within the Reporting vertical slice.

### PageModel thinness / repeated request construction

Purchase History and Supplier Purchase Analysis also contain repeated request-construction logic across page and export handlers. The PageModels parse query/status values and construct Application Requests for multiple handlers.

This is a **review candidate**, not an instruction to introduce a helper automatically. Any extraction must first demonstrate a repeated responsibility under the project's Rule-of-Three and preserve the existing Razor -> Application boundary.

## 8. DTOs and Mapping

There are **11 DTO types** in `InventoryPlatform.Application/DTOs`:

### Dashboard

- `DashboardDto`
- `DashboardStatisticsDto`
- `LowStockProductDto`
- `RecentTransactionDto`

### Reporting

- `InventoryMovementDto`
- `InventoryValuationDto`
- `LowStockDto`
- `ProductReportDto`
- `PurchaseHistoryDto`
- `StockMovementDto`
- `SupplierPurchaseAnalysisDto`

Reporting DTO projections are performed directly in Infrastructure repositories with LINQ `Select(... new ...Dto(...))`.

No separate mapping profile or mapper abstraction was identified in the supplied source tree.

The existing architecture documentation explicitly describes Dashboard and Inventory Valuation as read-only DTO projections, so these direct projections are consistent with the documented read-model pattern and are not treated as irregularities.

## 9. Repository Signature Inventory

The persistence abstraction contains both generic CRUD repositories and feature-specific read repositories.

### Generic repository pattern

`IRepository<TEntity>` provides:

- `GetByIdAsync`
- `GetAllAsync`
- `FindAsync`
- `AddAsync`
- `Update`
- `Remove`
- `ExistsAsync`

### Entity list repositories

These use `GetPagedAsync(PagedQuery, ...)`:

- `IProductRepository`
- `ICategoryRepository`
- `ISupplierRepository`
- `ICustomerRepository`
- `IUnitRepository`
- `IInventoryTransactionRepository`

### Purchasing

`IPurchaseOrderRepository` uses the feature-specific name:

`GetPurchaseOrdersAsync(PagedQuery, DateOnly?, DateOnly?, PurchaseOrderStatus?, ...)`

### Reporting

Feature-specific repository method names include:

- `GetInventoryValuationAsync`
- `GetPurchaseHistoryAsync`
- `GetSupplierPurchaseAnalysisAsync`
- `GetStockMovementAsync`
- `GetLowStockAsync`
- `GetInventoryMovementAsync`
- `GetProductReportsAsync`

The naming difference is understandable for read-model/report repositories, but `IPurchaseOrderRepository` is an entity repository and is therefore a lower-priority naming-consistency candidate against the `GetPagedAsync` convention.

## 10. Repository Irregularities

### Duplicate inherited member declaration

`IInventoryTransactionRepository` redeclares:

`AddAsync(InventoryTransaction transaction, CancellationToken ...)`

even though it already inherits `AddAsync(TEntity entity, CancellationToken ...)` from `IRepository<InventoryTransaction>`.

This is a concrete redundancy candidate.

### Minor naming/style inconsistency

`IUnitRepository.ExistsBySymbolAsync` declares its parameter as `Symbol` with an uppercase initial character, unlike the surrounding parameter naming convention.

### Namespace/import cleanliness

Some PageModels contain unused or stale-looking imports. A concrete example is Dashboard:

- `System.Net.WebRequestMethods`
- `InventoryPlatform.Application.Features.Customers.GetCustomers`

These are unrelated to the visible Dashboard implementation.

These are low-priority cleanup candidates unless confirmed by compiler/analyzer output.

## 11. Other Concrete Razor/PageModel Irregularities

### Purchase History manual form binding

`Pages/Reports/PurchaseHistory/Index.cshtml` manually supplies:

- `name`
- `value`
- `selected`

for search, dates, and status rather than using the established `asp-for` pattern.

### Supplier Purchase Analysis manual form binding

`Pages/Reports/SupplierPurchaseAnalysis/Index.cshtml` similarly manually supplies:

- `name`
- `value`
- `selected`

for search, dates, and status.

These two pages are the clearest candidates for aligning with the Sprint 9 `asp-for` convention, subject to preserving their current query names and behavior.

### Purchase Order sorting URL generation

`Pages/Purchasing/PurchaseOrders/Index.cshtml` uses `GetSortUrl()` and `Url.Page(...)` for five sorting links rather than expressing the route values with `asp-route-*`.

### Dashboard manual navigation

`Pages/Dashboard/Index.cshtml` contains two hard-coded application URLs instead of the route/page-tag-helper pattern used elsewhere.

## 12. Behavior-Preservation Risks

Any future Sprint 9 refactoring must preserve:

- Existing query-string parameter compatibility where intentionally retained.
- Date input formatting and parsing, especially `yyyy-MM-dd` values used by HTML date inputs.
- Enum/status selection behavior on report filters.
- Export filters, sorting, and paging state.
- Purchase Order pagination/filter preservation, which is historically regression-sensitive.
- Accessibility and validation behavior of Razor forms.
- The established Razor -> Application -> Infrastructure boundary.

In particular, changing `Page`, `page`, or `currentPage` to `PageNum` should be treated as an intentional UI/query-contract change and verified for all links, forms, pagination, sorting, and export handlers that participate in the same page.

## 13. Tests

No test project, test `.cs`, or test source directory was found in the supplied repository.

Therefore this baseline cannot identify existing automated tests covering:

- Razor Page binding
- route generation
- request construction
- paging/filtering/sorting consistency
- report request boundaries
- repository signatures

This is an important baseline gap, but T01 does not add tests.

## 14. Prioritized Irregularity List

### P1 - Inconsistent HTTP query binding strategy across list/report pages

**Evidence:**
- `[FromQuery]` request-object binding on the core CRUD pages
- `[BindProperty(SupportsGet = true)]` on Purchasing and several Reports
- direct handler parameters on Purchase History and Supplier Purchase Analysis

**Assessment:** strongest Sprint 9 consistency target. Do not blindly normalize all pages; select a pattern based on the existing Razor/Application boundary.

### P1 - Manual form binding in two reporting pages

**Files:**
- `Pages/Reports/PurchaseHistory/Index.cshtml`
- `Pages/Reports/SupplierPurchaseAnalysis/Index.cshtml`

**Assessment:** concrete `asp-for` consistency candidates. Preserve existing parameter names, date formatting, enum selection, and export behavior during any future refactor.

### P1 - Standardize UI/Razor paging parameter naming on `PageNum`

**Current source:** `Page`, `PageNum`, `page`, and `currentPage` are all used for the same conceptual paging value.

**Sprint 9 target:** use `PageNum` for the UI/Razor-facing paging property and query parameter.

**Important boundary:** this is a UI-layer convention. It does not require renaming Application/backend properties unless separately justified.

### P1 - Programmatic/manual application navigation

**Files:**
- `Pages/Purchasing/PurchaseOrders/Index.cshtml.cs`
- `Pages/Dashboard/Index.cshtml`

**Assessment:** review `Url.Page(...)` sorting/pagination generation and hard-coded Dashboard URLs for alignment with `asp-route-*` / Razor page tag-helper navigation.

### P1/P2 - Repeated report request construction and status parsing

**Primary area:**
- Purchase History
- Supplier Purchase Analysis

**Assessment:** these PageModels repeat query/status parsing and Application Request construction across page/export handlers. Review for a genuine Rule-of-Three abstraction before introducing helpers.

### P2 - `PagedRequest` -> `PagedQuery` duplication is unevenly applied

**Evidence:** some handlers map `PagedRequest` fields into a new `PagedQuery`; several reporting requests carry `PagedQuery` directly.

**Assessment:** review against the meaningful-boundary rule. Do not remove a representation solely for visual consistency.

### P2 - Duplicate inherited `AddAsync` declaration

**File:**
`InventoryPlatform.Application/Interfaces/Persistence/IInventoryTransactionRepository.cs`

**Assessment:** concrete redundancy with no distinct responsibility visible in the interface.

### P2 - Purchase Order repository naming differs from established entity-list repository naming

**File:**
`InventoryPlatform.Application/Interfaces/Persistence/IPurchaseOrderRepository.cs`

**Assessment:** `GetPurchaseOrdersAsync` differs from the `GetPagedAsync` convention used by the other entity repositories. Review only if the name does not carry a meaningful responsibility.

### P2 - No automated test project/source

**Assessment:** verification coverage is not represented in source as automated tests. T01 records the gap only; no tests are added in T01.

### P3 - Minor code cleanliness inconsistencies

Examples:

- `Dashboard.IndexModel.DashbordDTO` appears misspelled.
- Dashboard has unrelated imports.
- `IUnitRepository.ExistsBySymbolAsync` uses `Symbol` instead of `symbol`.

These are low-risk cleanup candidates and should not drive unrelated redesign.

### Explicit non-targets

The following are **not** Sprint 9 defects merely because they differ in representation:

- The existence of both `PagedRequest` and `PagedQuery`.
- Direct DTO projection in Infrastructure repositories.
- Feature-specific report repository method names.
- The use of `[FromQuery]` itself.
- The use of `[BindProperty]` itself.
- Repeated PageModel property names where they represent legitimate HTTP state.

Each must be evaluated for actual architectural redundancy before change.

## 15. Architecture Validation

The observed implementation remains consistent with the documented architecture at the major boundary level:

```text
Razor Page / PageModel
        ↓
Application Handler
        ↓
Application Persistence Abstraction
        ↓
Infrastructure Repository
```

The supplied architecture documentation explicitly permits request/query transformations where they represent distinct responsibilities. Therefore this baseline does **not** conclude that all request types, PageModels, or `PagedQuery` instances should be collapsed.

The most defensible Sprint 9 work is consistency-focused and localized to the concrete irregularities above.

## 16. T01 Execution Status

- Documentation reviewed: **Complete**
- Actual source inspected: **Complete**
- Exact files identified: **Complete**
- Architecture/design assumptions compared with source: **Complete**
- Code refactoring: **None**
- Automated tests: **None present in repository**
- Build verification: **Not performed successfully** - the execution environment used for this inspection does not have the `dotnet` CLI installed.
- Static/source inventory verification: **Complete**
- T01 baseline document: **Created**

## 16. Recommended Documentation Commit

`docs: establish sprint 9 code quality baseline`

No code commit is applicable to T01.


## 17. Sprint 9 Workstream Grouping

The baseline findings should be addressed through controlled workstreams rather than a broad normalization:

### Group A - Razor binding consistency
- `asp-for`
- `[BindProperty]`
- direct handler parameters
- request-model binding

### Group B - Navigation consistency
- `asp-route-*`
- `Url.Page(...)`
- hard-coded application page URLs
- sorting/pagination URL generation

### Group C - Request/query consistency
- `PageNum` as the UI paging convention
- `PagedRequest`
- `PagedQuery`
- repeated request construction

### Group D - Small code-quality cleanup
- duplicate inherited repository member
- unused imports
- naming typo
- parameter capitalization

Each future task should remain bounded to its assigned workstream and must follow the Sprint 9 stop rule.


## 18. T09 Automated Regression Verification Update

T09 reviewed the repository for automated verification and performed source-level regression checks after the Sprint 9 consistency changes.

### T09 findings

- No automated test project/source is present in the repository.
- A solution build could not be executed because the verification environment does not contain the `dotnet` CLI.
- Seven Razor list pages contained manual `?Page=...` pagination URLs despite the Sprint 9 `PageNum` convention.
- The pagination links were corrected to use Razor route tag helpers with `asp-route-PageNum` while preserving search, status, sort, and descending state.

Affected pages:

- Products
- Categories
- Suppliers
- Customers
- Units
- Inventory Transactions
- Administrator Users

### T09 verification limitation

No successful build, test, browser, runtime model-binding, repository-query, export, or authentication/authorization verification is claimed. The repository source was inspected and the pagination correction was statically verified.

**T09 result: PASS with environment limitation.**
