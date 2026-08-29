# Code Style

## Philosophy

- Readability over cleverness
- Consistency over personal preference
- Reuse before duplication
- Follow existing project patterns

---

## Project Structure

InventoryPlatform.Web
InventoryPlatform.Application
InventoryPlatform.Domain
InventoryPlatform.Infrastructure
InventoryPlatform.Shared

Describe what belongs in each layer.

---

## Naming Conventions

Classes
Interfaces
Records
Enums
Constants
Methods
Variables

Examples:

ProductRepository
IProductRepository
GetProductsHandler
ProductSortFields

---

## Feature Organization

Each module should contain:

Features/
    GetProducts/
    GetProduct/
    CreateProduct/
    UpdateProduct/
    ActivateProduct/
    DeactivateProduct/

---

## Handler Guidelines

- One handler per use case
- Keep handlers focused
- Return Result<T>
- Avoid business logic duplication

---

## Repository Guidelines

- Query only
- No business rules
- Async methods
- CancellationToken support

---

## Entity Guidelines

- Business rules belong here
- Avoid anemic models where practical
- No infrastructure dependencies

---

## Razor Pages

- One responsibility per page
- Keep PageModels thin
- Delegate work to handlers
- Prefer `asp-page` and `asp-route-*` tag helpers for application navigation and query-string state.
- Preserve the established query parameter contract when generating navigation links.
- For Purchase Order pagination, use the established `PageNum` and `PageSize` parameters.
- Reset pagination to `PageNum=1` when changing Purchase Order sorting while preserving active filters and page size.
- Use explicit `yyyy-MM-dd` formatting for `DateOnly` query route values when preserving date-filter state.


---

## Shared Infrastructure

When to place code into Shared.

General rule:

Extract only after it has been proven reusable.

Examples:

- Paging
- Sorting
- Filtering
- Result Pattern

---

## Error Handling

Use Result / Result<T>

Avoid exceptions for expected validation failures.

---

## General Formatting

- File-scoped namespaces
- Expression-bodied members where appropriate
- Meaningful method names
- Consistent spacing

### Razor Page application requests

When a Razor Page's query parameters already correspond to an Application request, prefer binding the PageModel directly to that Application request instead of maintaining a duplicate Web-only request model. If the external query-string contract differs from the Application request property names, keep the Application request clean and perform the HTTP-to-Application name translation at the Web binding boundary. Preserve established public query parameter contracts such as `PageNum`.

### Razor/UI paging convention

Use `PageNum` as the Razor-facing paging property and query parameter across paginated list/report pages. Keep `PageNum` in shared `PagedRequest` and `PagedQuery` contracts where those existing types already use it; do not rename backend properties solely for UI consistency.

When an Application request is already the correct HTTP-bound request shape, prefer binding that request directly. Keep a separate PageModel/query representation when the Application request intentionally contains a repository-facing `PagedQuery` or otherwise represents a distinct Application boundary.


### Direct Application-request binding

Do not introduce a custom model binder solely to translate a small number of Razor query-string names to Application request property names. Prefer direct binding to the Application request and perform minimal, explicit translation at the Razor Page handler boundary when the established public URL contract must be preserved (for example, `Status` to `PurchaseOrderStatus`).

### Paging request contract

`PagedRequest.PageNum` and the existing `PagedQuery.PageNum` are the paging properties used by the current shared request/query contracts. `PageNum` is also the canonical Razor/UI paging property and query parameter. Keep the Application Request -> `PagedQuery` boundary where the two types have distinct responsibilities.

Razor Pages that bind an Application request directly must emit top-level query parameter names (`Search`, `FromDate`, `ToDate`, `PageNum`, `PageSize`, etc.) rather than `Request.*`. Do not introduce a custom model binder solely for this translation.

