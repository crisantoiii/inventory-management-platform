# Changelog

All notable changes to this project will be documented in this file.

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