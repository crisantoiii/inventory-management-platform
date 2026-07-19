# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

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

#### Shared Infrastructure
- Reusable paging infrastructure
  - PagedRequest
  - PagedQuery
  - PagedResult<T>
- Shared product status filter enum
- Shared product sort field definitions

### Changed

- Refactored product repository to support reusable filtering, sorting and paging.
- Refactored product listing to use server-side search.
- Refactored product queries to use reusable paging models.

### Improved

- Product listing preserves filter state across pagination.
- Product listing preserves sorting state.
- Product activation workflow.
- Product deactivation workflow.