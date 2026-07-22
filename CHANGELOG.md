# Changelog

All notable changes to this project will be documented in this file.

## [v0.4.0] - 2026-07-22

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

### Changed

- Refactored product repository to support reusable filtering, sorting and paging.
- Refactored product listing to use server-side search.
- Refactored product queries to use reusable paging models.
- Implemented Customer module using the established Product, Category, and Supplier architecture.

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