# InventoryPlatform - Sprint 9 Code Quality Conventions

**Task:** T02 - Convention & Refactoring Rules Validation  
**Date:** 2026-08-22  
**Repository baseline:** `feature/code_quality_consistency`  
**Scope:** Evidence-based convention validation and documentation only - no code refactoring

## 1. Governing Basis

Sprint 9 is limited to controlled ASP.NET Core/Razor code-quality and consistency work.

T02 was validated against:

- `README.md` - Sprint 9 governing rulebook.
- T01 baseline - `docs/retrospectives/SPRINT_09_CODE_QUALITY_BASELINE.md`.
- The supplied current repository/source ZIP.

The architecture remains:

```text
Razor Page / PageModel
        ↓
Application Handler
        ↓
Application Persistence Abstraction
        ↓
Infrastructure Repository
```

The existing Clean Architecture, feature-first organization, explicit handlers/repositories, and Application request boundaries remain valid.

T02 does not authorize broad normalization. A convention is locked only where the source evidence and architecture justify it.

## 2. Evidence Baseline

Static inspection of the supplied source confirms the T01 inventory:

- `218` `asp-for` usages.
- `655` `asp-route-*` usages.
- `10` `[FromQuery]` usages.
- `74` `[BindProperty]` attributes.
- `40` `[BindProperty(SupportsGet = true)]` attributes.

T01 also identified:

- Manual form binding in Purchase History and Supplier Purchase Analysis.
- Programmatic `Url.Page(...)` navigation in Purchase Orders.
- Hard-coded application URLs in Dashboard.
- Multiple GET-binding patterns across CRUD, Purchasing, and Reporting.
- Both `PagedRequest` and `PagedQuery` in the existing paging architecture.
- Direct DTO projections in Infrastructure repositories.
- Repeated request construction in report page/export handlers.
- No automated test project/source in the supplied repository.

These findings are evidence for targeted review, not blanket refactoring instructions.

## 3. Locked Razor Form Convention

### 3.1 `asp-for`

Use `asp-for` for Razor form fields and labels where the existing PageModel property is the appropriate binding boundary.

Preferred pattern:

```html
<label asp-for="Search"></label>
<input asp-for="Search" />
```

For selects and validation, use the corresponding tag helpers where they preserve the existing model, validation, formatting, and accessibility behavior.

### 3.2 Manual `name` / `value` / `selected`

Manual form binding is not the preferred pattern when an equivalent `asp-for` representation can preserve the existing behavior.

The two concrete T01 candidates are:

- `Pages/Reports/PurchaseHistory/Index.cshtml`
- `Pages/Reports/SupplierPurchaseAnalysis/Index.cshtml`

Future refactoring of these pages must preserve:

- Existing query-string parameter names where intentionally retained.
- `yyyy-MM-dd` date input behavior.
- Enum/status selection behavior.
- Validation and accessibility.
- Export filters and state.

Do not change binding merely to make markup visually uniform if the manual representation carries a distinct responsibility.

## 4. Locked Razor Navigation Convention

### 4.1 `asp-route-*`

For Razor-facing page navigation, sorting, filtering, pagination, and other route/query values, prefer `asp-route-*` when the link is represented directly in Razor markup.

Example:

```html
<a asp-page="./Index"
   asp-route-PageNum="@Model.PageNum"
   asp-route-SortBy="@Model.SortBy">
    Next
</a>
```

This makes route state explicit at the Razor boundary and avoids unnecessary manual URL construction.

### 4.2 `Url.Page(...)`

`Url.Page(...)` is not prohibited.

It remains appropriate when URL generation is genuinely performed in PageModel/server-side code and the generated URL is required by that boundary.

However, T01 identified:

- `Pages/Purchasing/PurchaseOrders/Index.cshtml.cs`
  - `GetSortUrl(...)`
  - `GetPageUrl(...)`

These are review candidates because equivalent navigation is commonly expressed with `asp-route-*` in surrounding Razor pages.

No conversion is authorized by T02 itself.

### 4.3 Hard-coded application URLs

Hard-coded application page URLs are not the preferred navigation convention.

T01 identified:

- `Pages/Dashboard/Index.cshtml`
  - `/Dashboard`
  - `/InventoryTransactions/Index`

Future changes should prefer Razor page/tag-helper navigation where that preserves the existing destination and behavior.

The `_Layout.cshtml` `href="#"` dropdown trigger remains a UI/JavaScript trigger and is not an application-navigation defect.

## 5. Locked GET-Binding Rule

There is no single binding mechanism mandated for every Razor Page.

The correct binding mechanism depends on the actual Presentation boundary.

### 5.1 `[FromQuery]`

`[FromQuery]` is appropriate when a PageModel handler intentionally receives a request/query object from HTTP query-string state.

It is **not** a Sprint 9 style rule to add `[FromQuery]` everywhere.

T01 found ten usages. The eight request-object cases on core list/report pages represent an established pattern. Supplier Purchase Analysis additionally uses `[FromQuery(Name = ...)]` on handler parameters.

These usages are review candidates only where the boundary or behavior is actually improved.

### 5.2 `[BindProperty]`

`[BindProperty]` remains valid where PageModel state is the intended Razor boundary.

`SupportsGet = true` is valid where the property represents GET-bound page state.

T01 found 74 `BindProperty` attributes, including 40 with `SupportsGet = true`.

Do not replace `BindProperty` solely because another page uses `[FromQuery]`.

### 5.3 Handler-parameter binding

Direct handler parameters are valid when they provide a clear and appropriate Razor Page HTTP boundary.

T01 identified direct handler-parameter binding in:

- Purchase History.
- Supplier Purchase Analysis.

These pages are consistency review candidates because neighboring report pages use different representations. They are not automatically incorrect.

### 5.4 Binding decision rule

For any future Sprint 9 change:

1. Identify the actual HTTP/Razor boundary.
2. Preserve the existing request contract unless an intentional convention change is required.
3. Choose the binding pattern that best represents that boundary.
4. Preserve validation, accessibility, query names, and behavior.
5. Do not normalize binding patterns merely for visual consistency.

## 6. Locked UI Paging Convention: `PageNum`

The standard Razor/UI-facing paging property and query parameter is:

**`PageNum`**

Rationale:

- `Page` is already a meaningful ASP.NET Core/Razor Page boundary concept.
- A PageModel property named `Page` can create ambiguity with the Razor Page `Page` property/context.
- `PageNum` makes the UI paging concept explicit.

T01 found these UI-facing variants:

- `Page`
- `PageNum`
- `page`
- `currentPage`

The Sprint 9 convention is therefore:

```text
Razor/UI property: PageNum
Razor/UI query parameter: PageNum
```

This is a **UI/Razor convention only**.

Do **not** automatically rename:

- Application request properties.
- Application handler parameters.
- `PagedRequest` properties.
- `PagedQuery` properties.
- Repository parameters.

A cross-layer rename requires separate architectural justification and explicit verification of all affected contracts.

Any intentional `PageNum` change must verify:

- links
- forms
- pagination
- sorting
- filtering
- exports
- query-string compatibility where applicable
- Purchase Order pagination/filter preservation

## 7. Application Request Boundary

The Application request boundary remains valid.

Meaningful transformations such as:

```text
HTTP Request
    ↓
Application Request
    ↓
Repository Query
```

are allowed and should not be collapsed merely because the types contain overlapping properties.

An Application Request has a distinct responsibility when it represents the use-case input contract independently from the persistence/query representation.

Therefore:

- Keep explicit Application Requests where they define a use-case boundary.
- Keep `PagedRequest` and `PagedQuery` when they serve distinct responsibilities.
- Do not replace Application Requests with repository queries merely to reduce type count.
- Do not introduce a new request abstraction without evidence.

## 8. `PagedRequest` vs `PagedQuery`

Both existing types remain valid.

T01 confirmed:

- Many application requests inherit `PagedRequest`.
- Several reporting requests carry `PagedQuery` directly.
- Some handlers transform `PagedRequest` data into a `PagedQuery` before repository access.

This is an architectural consistency review area, not a confirmed defect.

### Locked rule

Do not collapse `PagedRequest` and `PagedQuery` unless source inspection proves that the two types have no distinct responsibility in the affected workflow.

Visual similarity or duplicated paging properties alone is insufficient evidence.

## 9. Mapping and Projection Boundary

The existing direct projection pattern remains valid.

Reporting repositories use LINQ projections such as:

```csharp
Select(... new ...Dto(...))
```

No mapper profile or mapping framework is required.

The architecture documentation already establishes read-only DTO projections for reporting/dashboard scenarios.

### Locked rule

Keep projection close to the persistence/query boundary when the repository is responsible for shaping a read model.

Do not introduce:

- AutoMapper.
- Generic mapping helpers.
- Mapping profiles.

unless a separate architectural decision establishes a real repeated responsibility that cannot be handled clearly by the existing explicit projections.

Direct DTO projection is therefore an explicit **non-target** of Sprint 9 consistency work.

## 10. Rule-of-Three

Sprint 9 uses the project's Rule-of-Three:

> Introduce reusable abstractions only after demonstrating value across multiple independent implementations.

Three similar lines of code do not automatically require a helper. The repeated responsibility must itself be meaningful and independently reusable.

### Acceptable repetition

Repetition is acceptable when it represents:

- Different architectural boundaries.
- Different HTTP contracts.
- Different use-case requests.
- Different repository/query responsibilities.
- Feature-specific business behavior.
- Explicit read-model projections.

### Refactor only when

A candidate abstraction:

1. Represents the same responsibility.
2. Appears across at least three independent implementations or has equivalent evidence of established reuse.
3. Reduces meaningful duplication without hiding architectural boundaries.
4. Preserves existing behavior and contracts.
5. Improves maintainability rather than merely reducing line count.

## 11. Report Request Construction

Purchase History and Supplier Purchase Analysis repeat request construction and query/status parsing across page and export handlers.

This is a valid review target.

It is **not** authorization to introduce a helper automatically.

A future extraction must demonstrate that the repeated logic is one responsibility and satisfies the Rule-of-Three. The extraction must also preserve the Razor -> Application boundary and all export/query state.

## 12. Explicit Non-Targets

The following are not Sprint 9 defects merely because they differ from another implementation:

- `[FromQuery]` usage by itself.
- `[BindProperty]` usage by itself.
- Direct handler-parameter binding by itself.
- The existence of both `PagedRequest` and `PagedQuery`.
- An Application Request mapping into a repository query.
- Direct Infrastructure DTO projection.
- Feature-specific report repository method names.
- Repeated PageModel properties when they represent legitimate HTTP state.
- Explicit handlers and repositories.
- Feature-first organization.
- Existing Clean Architecture boundaries.
- UI/JavaScript triggers that are not application navigation.

The following are also outside T02:

- Sales.
- Dynamic Capability-Based Authorization.
- Audit/Activity Logging.
- Bulk Import/Export.
- Barcode/QR.
- Broad architectural redesign.
- Generic CRUD frameworks.
- New test infrastructure.

## 13. Non-Target vs Future Candidate Summary

| Area | T02 decision |
|---|---|
| `asp-for` | Preferred for appropriate Razor form binding |
| `asp-route-*` | Preferred for direct Razor navigation/route state |
| `Url.Page(...)` | Allowed; review only where equivalent Razor navigation is clearer |
| `[FromQuery]` | Valid where boundary is appropriate; not a blanket rule |
| `[BindProperty]` | Valid where PageModel state is the appropriate boundary |
| Direct handler parameters | Valid where they form an appropriate HTTP boundary |
| UI paging | **Standardize on `PageNum`** |
| Application/backend paging names | Preserve unless separately justified |
| `PagedRequest` + `PagedQuery` | Keep unless distinct responsibility is disproven |
| Application Request mapping | Keep when it represents a meaningful boundary |
| DTO projections | Keep explicit projections |
| Mapping framework | Not required |
| Helpers/abstractions | Apply Rule-of-Three |
| Manual form binding | Review where `asp-for` can preserve behavior |
| Hard-coded page URLs | Review for Razor/page-helper navigation |
| Purchase Order URL helpers | Review, but not automatically replace |
| Broad normalization | Prohibited |

## 14. Verification

### Documentation review

Complete against:

- Sprint 9 `README.md`.
- T01 baseline.
- Existing architecture/design documentation.

### Actual source inspection

Complete against the supplied repository ZIP.

Confirmed source inventory:

- `218` `asp-for` usages.
- `655` `asp-route-*` usages.
- `10` `[FromQuery]` usages.
- `74` `[BindProperty]` attributes.
- `40` `SupportsGet = true` bindings.

Concrete T01 candidates were cross-checked against the source without applying code changes.

### Automated verification

No automated test project/source exists in the supplied repository.

### Build verification

No build, browser, migration, or test result is claimed.

## 15. T02 Execution Status

- Documentation review: **Complete**
- T01 evidence review: **Complete**
- Actual source inspection: **Complete**
- Exact candidate files identified: **Complete**
- Architecture validation: **Complete**
- Evidence-based conventions locked: **Complete**
- Code changes: **None**
- Documentation change: **Complete**
- Automated tests: **Not available in repository**

## 16. Recommended Documentation Commit

```text
docs: define sprint 9 code quality conventions
```

No code commit is applicable to T02.

## 17. Stop Rule

T02 defines and locks the evidence-based Sprint 9 conventions.

Later Sprint 9 tasks must implement only their assigned workstream, re-inspect the affected source, preserve the locked boundaries above, perform actual verification, and stop.

## 18. T03 Implementation - Razor Form Binding Normalization

T03 applied the locked Razor form convention to the two concrete reporting candidates identified by T01/T02:

- `Pages/Reports/PurchaseHistory/Index.cshtml`
- `Pages/Reports/SupplierPurchaseAnalysis/Index.cshtml`

The report filter forms use `asp-for` for the existing PageModel properties:

- `Search`
- `FromDate`
- `ToDate`
- `Status`

Date inputs retain the HTML date-input format requirement through the existing `yyyy-MM-dd` formatting. Status selection retains the existing Purchase Order status values and the `All` option.

The change is limited to the Razor form representation. Existing report request properties and report behavior remain the responsibility of the existing PageModel/Application boundary.

## 19. T04 Implementation - Razor Route & Query Navigation Normalization

T04 reviewed the concrete navigation candidates identified by T01/T02.

The current source confirms that Purchase Order sorting and pagination navigation are represented directly in Razor using `asp-page` and `asp-route-*` values.

The previous server-side URL helper methods:

- `GetSortUrl(...)`
- `GetPageUrl(...)`

are no longer present in `Pages/Purchasing/PurchaseOrders/Index.cshtml.cs`.

The Purchase Order Razor page now carries the active query state through explicit route values, including:

- `Search`
- `FromDate`
- `ToDate`
- `Status`
- `SortBy`
- `Descending`
- `PageNum`
- `PageSize`

The established UI paging convention remains `PageNum`.

### T04 scope boundary

The current source still contains a hard-coded Dashboard refresh URL:

```html
<a href="/Dashboard" ...>
```

Therefore the Dashboard hard-coded URL candidate identified by T01 remains present in the current source and is not recorded here as successfully normalized.

The `_Layout.cshtml` `href="#"` UI trigger remains outside application-navigation normalization.

## 20. T04 Verification

Source verification confirms:

- Purchase Order sorting navigation uses `asp-route-*`.
- Purchase Order pagination navigation uses `asp-route-*`.
- Purchase Order navigation preserves the active search, date, status, sorting, page-size, and `PageNum` state.
- The previous `GetSortUrl(...)` and `GetPageUrl(...)` methods are absent from the current Purchase Order PageModel.
- Dashboard still contains the identified `/Dashboard` hard-coded URL.

No build, browser, or automated-test result is claimed in this documentation unless actually performed.

## 21. T05 Request Binding Consolidation

T05 reviewed PageModel handler parameters, `[BindProperty]`, `[FromQuery]`, query binding, and Application Request classes.

The consolidation was limited to accidental duplication where an Application Request already represented the complete HTTP GET input.

The following list PageModels now bind the complete Application Request from query state and pass that request directly to the Application handler:

- `Pages/Products/Index.cshtml.cs`
- `Pages/Categories/Index.cshtml.cs`
- `Pages/Suppliers/Index.cshtml.cs`
- `Pages/Customers/Index.cshtml.cs`
- `Pages/Units/Index.cshtml.cs`
- `Pages/InventoryTransactions/Index.cshtml.cs`
- `Pages/Administrator/Users/Index.cshtml.cs`

The affected Application Requests inherit the existing `PagedRequest`, whose paging property remains `PageNum`.

### Consolidated boundary

The affected flow is:

```text
HTTP query
    ↓
[FromQuery] Application Request
    ↓
Application Handler
```

The previous duplication of binding `PageNum` separately as a handler parameter and then copying it into the same request was removed.

The Application Request remains the use-case contract.

### T05 non-targets

T05 did not:

- impose `[FromQuery]` universally
- convert all `[BindProperty]` usage
- convert all handler parameters into Request models
- rename Application/backend paging properties
- collapse `PagedRequest` and `PagedQuery`
- alter intentional external-to-Application property mappings
- normalize binding solely for stylistic consistency

Purchase Order `Status` handling remains a distinct mapping because the external query name and Application Request property have different responsibilities.

## 22. T05 Verification

Source verification confirms that the seven affected list PageModels use the request object as the query-bound input and no longer bind `PageNum` again as a separate handler parameter.

The existing request models retain their defaults and existing paging inheritance.

The consolidation preserves the existing request boundary for:

- filtering
- sorting
- pagination
- query-string state

The T05 verification requirement also covers exports and query compatibility. No claim of browser/export execution is made here unless actually performed.

No automated test project/source is present in the supplied repository.

No build, migration, browser, or automated-test result is claimed from the documentation/source inspection environment.

## 23. Cumulative Sprint 9 Documentation Status

The conventions document now records the evidence-based baseline and the completed/observed implementation history through T05.

The locked architectural rules remain:

1. Prefer `asp-for` for appropriate Razor form binding.
2. Prefer `asp-route-*` for direct Razor navigation and query state.
3. Do not impose `[FromQuery]` universally.
4. Do not convert all `[BindProperty]` usage.
5. Do not convert all handler parameters into Request models.
6. Use `PageNum` for the Razor/UI paging boundary.
7. Preserve distinct Application Request and persistence/query responsibilities.
8. Apply the Rule-of-Three before introducing shared abstractions.
9. Preserve filtering, sorting, pagination, exports, validation, accessibility, and query contracts.
10. Do not expand Sprint 9 into unrelated architectural redesign.

The current source remains the source of truth for subsequent Sprint 9 tasks.

## 24. T05 Commit Messages

Code:

```text
refactor(web): consolidate request binding
```

Documentation:

```text
docs: document request binding conventions
```

Documentation and code changes remain separate commits according to the Sprint 9 workflow.

## 25. Stop Rule

T05 is limited to request-binding consolidation.

Subsequent Sprint 9 tasks must begin from their own task prompt and current repository/source ZIP, re-inspect the relevant documentation and source, preserve the locked conventions above, perform only their assigned scope, report actual verification, and stop.
## 26. T06 Application Request Redundancy Review

T06 reviewed Application Request/Response/Handler patterns for concrete redundancy identified by the Sprint 9 baseline and verified against the current source.

### Verified redundancy removed

`IInventoryTransactionRepository` inherited `AddAsync` from `IRepository<InventoryTransaction>` but redeclared the same method signature. The inherited contract already provides the required repository capability, so the interface declaration was redundant. The duplicate declaration was removed.

The concrete `InventoryTransactionRepository.AddAsync` implementation remains because it provides the inherited repository implementation and does not duplicate an interface contract.

### Application request construction assessment

T05 already consolidated accidental HTTP-boundary duplication by binding the complete Application Request where that request represented the complete GET input. T06 reviewed the remaining Application request construction exposed by T05.

Purchase History and Supplier Purchase Analysis currently have similarly shaped but feature-specific Application Request types. Their matching properties are not sufficient evidence of redundancy: each request is the independent use-case input contract for a different Application handler/repository workflow. They therefore remain separate.

The handlers also map the paging/search/sort portion of those requests into `PagedQuery`. This remains a meaningful Application Request -> repository query boundary and was not collapsed. `PagedRequest` and `PagedQuery` remain distinct for the same reason.

The repeated page/export request construction and status parsing in the two report PageModels was reviewed but not extracted. The task does not provide sufficient evidence that a new shared abstraction would represent a single architectural responsibility rather than hide feature-specific HTTP/Application behavior.

### T06 decision

Only the verified inherited `AddAsync` redeclaration was removed. No Application Request types, `PagedRequest`, `PagedQuery`, handlers, or report request construction were consolidated merely for structural similarity.

### T06 Commit Messages

Code:

```text
refactor(application): reduce verified request redundancy
```

Documentation:

```text
docs: document application request conventions
```

Documentation and code changes remain separate commits according to the Sprint 9 workflow.

### T06 Stop Rule

T06 is limited to verified Application Request/Handler/repository contract redundancy. No broad request-model redesign, paging abstraction consolidation, or unrelated refactoring is authorized by this task.
