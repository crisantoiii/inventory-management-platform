# Sprint 7 - Additional Reporting

## Milestone

**Additional Reporting**

## Objective

Extend the Reporting capabilities established by the initial
Inventory Valuation vertical slice into a broader set of operational
and analytical inventory reports.

## Scope

### Completed

- [X] Inventory Valuation
- [X] Purchase History
- [X] Supplier Purchase Analysis
- [X] Stock Movement
- [X] Low Stock Report
- [X] Inventory Movement Report
- [X] Product Reports

### Completed Sprint Scope

- [X] Excel Export
- [X] PDF Export
- [X] Final Project-wide Verification

### Deferred Validation — Completed

- [X] Empty database behavior verification
- [X] Explicit query-failure testing
- [X] Authorization regression

These validation items were completed during final project-wide verification.

## Reporting Architecture

All Sprint 7 reports continue to use the established read-oriented
Reporting architecture:

```text
Presentation
     ↓
Application Handler
     ↓
Read Model / DTO
     ↓
Repository Abstraction
     ↓
Infrastructure Repository
     ↓
EF Core Query
     ↓
SQL Server
```

Reporting remains read-only.

The implementation continues to favor:

- Dedicated DTO/read models
- Thin Application handlers
- Repository abstractions
- Server-side filtering
- Server-side sorting
- Server-side pagination
- EF Core projections
- Database-side calculations and aggregation
- AsNoTracking() for read-only queries
- Shared paging infrastructure

No generic reporting framework or parallel architecture was
introduced.

## Product Reports

Product Reports extends the read-oriented Reporting architecture to
current Product state.

The report provides:

- Product
- SKU
- Category
- Unit
- Quantity On Hand
- Cost Price
- Selling Price
- Status

The report supports:

- Active / Inactive / All Products filtering
- Product/SKU/Category/Unit search
- Server-side sorting
- Server-side pagination
- Page-size changes
- Reset behavior
- Combined search and status filtering
- Boundary/no-result handling

Product Reports uses a dedicated read model, application handler,
repository abstraction, Infrastructure repository, and Razor Page.

The query remains read-only and database-side using `AsNoTracking()`.
No Domain entity, database schema, or migration changes were required.

## Product Reports Verification

Product Reports was built successfully and verified through actual
browser workflows.

Validated:

- Page loading
- Navigation
- Product/SKU/Category/Unit search
- Active / Inactive / All Products filtering
- Server-side sorting
- Server-side pagination
- Pagination state preservation
- Page-size changes
- Reset behavior
- Combined search and status filtering
- Boundary/no-result behavior

All implemented Product Reports test cases were confirmed through
manual verification.

## Excel Export

Excel Export extends the completed Reporting pages with downloadable `.xlsx` output while reusing the established read-oriented Reporting queries and DTOs.

The export supports the completed reports:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports

The export preserves active report filters and sorting. It exports the full filtered result set rather than only the current paginated page.

Excel generation is isolated in the Web layer using a focused Excel report writer. No Domain entity, database schema, or migration changes were required, and no generic reporting framework was introduced.

Inventory Valuation also includes the Total Inventory Value summary in the exported workbook.

## Excel Export Verification

Excel Export was built successfully and verified through browser/manual workflows.

Validated:

- Export action availability
- Workbook generation
- Report-specific columns and values
- Filter preservation
- Sorting preservation
- Full filtered result export without UI pagination limits
- Inventory Valuation Total Inventory Value

## Inventory Movement

Inventory Movement extends Reporting from transaction-level movement
history into product-level movement analysis.

The report provides:

- Product
- SKU
- Opening Quantity
- Stock In
- Stock Out
- Adjustment
- Closing Quantity

The report supports:

- Product/SKU search
- From/To date filtering
- Reporting Period display
- Server-side sorting
- Server-side pagination
- Page-size changes
- Reset behavior
- Combined filtering
- Boundary/no-result handling

Stock Movement remains the transaction-level report, while Inventory Movement provides product-level aggregation for a selected reporting period.

## PDF Export

PDF Export extends the completed Reporting pages with downloadable `.pdf` output while reusing the established read-oriented Reporting queries and DTOs.

The export supports the completed reports:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports

The export preserves active report filters and sorting. It exports the full filtered result set rather than only the current paginated page.

QuestPDF is used as the focused Web-layer PDF generation component. No generic export framework, Domain entity, database table, or migration was introduced.

Inventory Valuation also includes the Total Inventory Value summary in the generated PDF.

## PDF Export Verification

PDF Export was built and verified through browser/manual workflows.

Validated:

- Export action availability
- PDF generation
- Report-specific columns and values
- Preservation of active filters
- Preservation of active sorting
- Full filtered result export without UI pagination limits
- Inventory Valuation Total Inventory Value summary

## Verification

Inventory Movement was verified through actual browser workflows.

Validated:

- Page loading
- Navigation
- Product/SKU search
- From/To date filtering
- Reporting Period display
- Combined search and date filtering
- Sorting
- Reset
- Pagination
- Page-size changes
- Pagination with active filters
- Boundary/no-result behavior
- Aggregated report values

## Architecture Outcome

Sprint 7 continues to validate that the existing architecture can support multiple read-oriented reporting scenarios without requiring structural redesign.

The reporting implementation remains independent of the future Dynamic Capability-Based Authorization architecture.

## Current Sprint Position

Sprint 7 Additional Reporting is complete and has passed final project-wide verification.

Final verification included application regression, reporting verification, export verification, empty-database behavior, explicit query-failure behavior and database recovery, authorization regression, and final build verification.

## Sprint Outcome

Sprint 7 Additional Reporting is complete.

The following reporting capabilities and exports are implemented and verified:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel Export
- PDF Export

Final project-wide verification passed, including application regression, reporting verification, export verification, empty-database behavior, explicit query-failure behavior, database recovery, authorization regression, and final build verification.