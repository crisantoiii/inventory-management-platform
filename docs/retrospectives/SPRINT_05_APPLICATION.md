# Sprint 5 — Reporting

## Sprint Goal

Implement the first Reporting vertical slice by introducing a dedicated Inventory Valuation report.

The report will build upon the existing Dashboard read-model pattern while keeping Reporting concerns separate from transactional Domain workflows.

The goal is to validate that the architecture supports read-oriented business capabilities without introducing unnecessary abstractions.

---

# Scope

## In Scope

### Inventory Valuation Report

The report will provide a read-only view of current inventory valuation.

The existing Dashboard defines Inventory Value as:

```text
Inventory Value
= Σ (QuantityOnHand × CostPrice)
```

Sprint 5 will expose this calculation at the Product level so that users can understand how the total inventory value is derived.

### Report Data

The report will provide:

- Product
- Category
- Quantity On Hand
- Cost Price
- Inventory Value

Where:
```text
Inventory Value
= Quantity On Hand × Cost Price
```

### Application Layer

Implement:

- Inventory Valuation request
- Inventory Valuation response/read model
- Inventory Valuation handler
- Dedicated persistence abstraction

### Infrastructure Layer

Implement:

- Read-only inventory valuation query
- EF Core projection
- AsNoTracking() query
- Product and Category data projection

### Presentation Layer

Implement:

- Inventory Valuation Razor Page
- Report table
- Total inventory valuation
- Appropriate empty-state handling
- Query failure feedback

---

## Architecture

The report will follow the established read-oriented architecture:

```text
Razor Page
     ↓
Application Handler
     ↓
Read Model
     ↓
IInventoryValuationRepository
     ↓
InventoryValuationRepository
     ↓
EF Core Projection
     ↓
SQL Server
```

The Reporting feature must not access DbContext directly from the Presentation layer.

The report must not mutate Domain entities.

---

## Read Model

The initial report read model is expected to contain:

```text
InventoryValuationDto

- ProductId
- ProductName
- CategoryName
- QuantityOnHand
- CostPrice
- InventoryValue
```

The final property names should follow the project's existing naming conventions when implementation begins.

---

## Application Layer

Introduce a dedicated feature for retrieving the Inventory Valuation report.

Expected structure:

```text
Application
└── Features
    └── Reporting
        └── GetInventoryValuation
            ├── GetInventoryValuationRequest
            └── GetInventoryValuationHandler
```

The handler should remain thin.

Its responsibility is to:

1. Receive the request.
2. Call the reporting persistence abstraction.
3. Return the read model through the existing Result<T> pattern.

Business calculations should not be moved into the handler when they can be represented directly by the read query.

---

## Presentation Layer

Add a dedicated Inventory Valuation report page.

Expected flow:

```text
User
 ↓
Inventory Valuation Page
 ↓
GetInventoryValuationHandler
 ↓
Inventory Valuation Read Query
 ↓
Database
```

The page should display:

| Product | Category | Quantity On Hand | Cost Price | Inventory Value |
| :--- | :--- | :--- | :--- | :--- |
| Product A | Category A | 10 | 50.00 | 500.00 |
| Product B | Category B | 5 | 100.00 | 500.00 |
| **Total** | | | | **1,000.00** |

The actual data will come from the database.

No seed data will be introduced solely to support the report.

---

## Validation and Error Handling

The report is read-only and therefore does not require Domain mutation validation.

Presentation-level concerns include:

- Empty report handling
- Query failure feedback
- Appropriate validation/error summary

Application query failures should follow the existing `Result<T>` contract.

The implementation should avoid silently treating a failed query as an empty report.

---

## Empty Database Behavior

The report must remain usable when there are no Products.

Expected behavior:

```text
Inventory Valuation

No inventory records are available.
```

or the project's established empty-state convention.

The page should not require seed data simply to render successfully.

---

## Testing Strategy

The report will be verified using actual persisted database data.

### Scenario 1 — Empty Database

Verify:

- Page loads
- No exception occurs
- Empty state is displayed
- Total is zero or appropriately represented

### Scenario 2 — Single Product

Verify:

```text
QuantityOnHand × CostPrice
```

produces the expected inventory value.

### Scenario 3 — Multiple Products

Verify:

```text
Total Inventory Value
=
Σ Product Inventory Value
```

### Scenario 4 — Different Categories

Verify Category information is projected correctly.

### Scenario 5 — Query Failure

Verify that an Application/read-query failure is surfaced rather than presented as an empty successful report.

---

## Architectural Constraints

Sprint 5 must preserve the following:

- Clean Architecture boundaries
- Feature-first organization
- Thin Application handlers
- Read-oriented DTO projections
- Explicit persistence abstractions
- No direct DbContext access from Web
- No generic reporting framework
- No unnecessary abstraction
- No mutation of Domain entities from Reporting

Reporting should remain separate from transactional workflow logic.

---

## Deferred Features

The following are intentionally outside the first Inventory Valuation vertical slice:

- Excel export
- PDF export
- Purchase History report
- Supplier Purchase Analysis
- Stock Movement report
- Low Stock report
- Inventory Movement report
- Advanced report filtering
- Advanced report sorting
- Report scheduling
- Generic reporting framework

These may be considered after the first reporting pattern has been validated.

---

## Acceptance Criteria

Sprint 5 Inventory Valuation is considered complete when:

- [x] Inventory Valuation read model is implemented.
- [x] Application request and handler are implemented.
- [x] Persistence abstraction is implemented.
- [x] EF Core read projection is implemented.
- [x] Inventory valuation uses `QuantityOnHand` × `CostPrice`.
- [x] Inventory Valuation Razor Page is implemented.
- [x] Total inventory value is displayed.
- [ ] Empty database behavior is verified.
- [x] Multiple-product calculation is verified.
- [x] Category projection is verified.
- [ ] Query failures are surfaced appropriately.
- [x] No Domain entities are mutated by the report.
- [x] No Presentation-to-DbContext dependency exists.
- [x] Existing functionality remains working.
- [x] Solution builds successfully.
- [x] Browser report is verified.
- [ ] Sprint documentation is updated.
- [ ] Final documentation is consolidated into one documentation commit.

---

## Implementation Completed

### Inventory Valuation

Implemented the first Reporting vertical slice with:

- `InventoryValuationDto`
- `GetInventoryValuationRequest`
- `GetInventoryValuationHandler`
- `IInventoryValuationRepository`
- `InventoryValuationRepository`
- Inventory Valuation Razor Page
- Navigation entry under Operations

The repository uses a read-only EF Core projection with `AsNoTracking()`.

Inventory valuation is calculated as:

```text
QuantityOnHand × CostPrice
```

The report total was verified against the existing Dashboard Inventory Value.

## EF Core Query Adjustment

The initial query attempted to order the projected DTO:

```text
Projection
    ↓
OrderBy(DTO.ProductName)
```

This could not be translated by EF Core.

The query was changed to order the Product entity before projection:

```text
Products
    ↓
OrderBy(Product.Name)
    ↓
Projection
    ↓
InventoryValuationDto
```

This kept the query fully database-side without using client evaluation.

## Browser Verification

Verified that:

- Inventory Valuation is accessible from the application navigation.
- Product data loads from the database.
- Category information is displayed.
- Quantity On Hand is displayed.
- Cost Price is displayed.
- Individual Inventory Value is calculated correctly.
- Total Inventory Value is calculated correctly.
- Report total matches the Dashboard Inventory Value.

---

## Expected Architecture Outcome

Sprint 5 should validate that the existing architecture supports both:

```text
Transactional Workflows

Presentation
     ↓
Application
     ↓
Domain
     ↓
Infrastructure
```

and:

```text
Read-Oriented Reporting

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

The goal is to support reporting without forcing read-only queries through Domain aggregates or transactional workflow patterns.

---

# Sprint 5 Success Criteria

The first Reporting vertical slice is functionally complete when Inventory Valuation is available as a usable browser-accessible report backed by actual database data and implemented without architectural redesign.

Remaining validation items are limited to empty-database behavior and explicit query-failure testing.

## One deliberate decision

I would **not include Excel/PDF implementation in the first slice**.

They remain on the roadmap, but adding export immediately would make the first Reporting feature larger than necessary.

Our sequence should be:

```text
Inventory Valuation
       ↓
Validate reporting architecture
       ↓
Then add export
       ↓
Then additional reports
```

This follows the same principle we used with Purchasing: **build a complete, usable vertical slice first, then expand**.