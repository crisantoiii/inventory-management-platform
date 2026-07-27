# Design Decisions

## Overview

This document records significant architectural and engineering decisions made during the development of the Inventory Management Platform.

The goal is to capture **why** a decision was made, what alternatives were considered, and the expected long-term benefits.

---

# DD-001 — Clean Architecture

## Decision

The solution follows a layered Clean Architecture.

```
Web
↓

Application
↓

Domain

Infrastructure
```

## Rationale

Business logic should remain independent of UI and persistence technologies.

This allows:

- Better maintainability
- Easier testing
- Lower coupling
- Easier future expansion

## Alternatives Considered

Traditional three-layer architecture.

## Outcome

Accepted.

---

# DD-002 — Razor Pages

## Decision

Use Razor Pages instead of ASP.NET MVC.

## Rationale

The application is primarily business CRUD with page-oriented workflows.

Razor Pages provide:

- Simpler folder organization
- Better feature locality
- Less boilerplate
- Faster development

## Alternatives

ASP.NET MVC

## Outcome

Accepted.

---

# DD-003 — Repository Pattern

## Decision

Repositories abstract Entity Framework Core from application logic.

## Rationale

Repositories centralize persistence concerns and keep handlers focused on business workflows.

Current implementation:

- ProductRepository
- CategoryRepository
- SupplierRepository
- CustomerRepository
- UnitRepository
- InventoryTransactionRepository
- DashboardRepository

## Outcome

Accepted.

---

# DD-004 — Result Pattern

## Decision

Application handlers return Result or Result<T>.

## Rationale

Standardizes success and failure responses.

Benefits:

- Consistent error handling
- Cleaner handlers
- Easier validation

## Outcome

Accepted.

---

# DD-005 — Shared Paging Infrastructure

## Decision

Create reusable paging classes in the Shared project.

Components:

- PagedRequest
- PagedQuery
- PagedResult<T>

## Rationale

Initially paging was implemented specifically for Products.

After validating the approach, paging was extracted into reusable infrastructure.

Benefits:

- Consistent paging behavior
- Reduced duplication
- Faster implementation of new modules
- Proven reusable across Product, Category, Supplier, Customer, Unit, and Inventory Transaction modules.

## Outcome

Accepted.

---

# DD-006 — Shared Filtering Infrastructure

## Decision

Introduce reusable filtering types.

Current implementation:

- Shared status filtering infrastructure

## Rationale

The Product, Category, Supplier, Customer, Unit, and Inventory Transaction modules all use the same filtering approach.

## Outcome

Accepted.

---

# DD-007 — Shared Sorting Infrastructure

## Decision

Move module sort field definitions into InventoryPlatform.Shared.

## Rationale

Sorting definitions are used by:

- Web
- Application
- Infrastructure

Current implementation:

- ProductSortFields
- CategorySortFields
- SupplierSortFields
- CustomerSortFields
- UnitSortFields
- InventoryTransactionSortFields

Keeping them in Shared prevents unnecessary project dependencies and improves reuse.

## Alternatives

Keeping ProductSortFields in the Application project.

## Outcome

Shared project chosen.

---

# DD-008 — Soft Delete

## Decision

Business entities are deactivated instead of permanently deleted.

## Rationale

Inventory systems should preserve historical data.

Inactive records can later be:

- Restored
- Reported
- Audited

This approach better reflects real-world business requirements.

## Outcome

Accepted.

---

# DD-009 — Server-side Processing

## Decision

Searching, sorting, filtering, and paging are performed in SQL rather than in memory.

## Rationale

Benefits include:

- Better scalability
- Reduced memory usage
- Faster response times
- Smaller data transfers

## Outcome

Accepted.

---

# DD-010 — Consistent Module Architecture

## Decision

All business modules follow the same architectural structure. 
The architecture has now been successfully validated across seven independent business modules without requiring structural changes, demonstrating that the design scales consistently as new features are introduced.

Each module implements:

- Domain Entity
- Repository Interface
- Repository Implementation
- CQRS-style Application Handlers
- Razor Pages
- Shared Paging
- Shared Filtering
- Shared Sorting
- Result Pattern

## Rationale

Maintaining a consistent implementation pattern across modules improves readability, reduces onboarding time, simplifies maintenance, and enables new features to be developed with minimal duplication.

## Outcome

Accepted.

---

# DD-011 — Normalize Product Relationships

## Decision

Replace free-text Unit with Unit entity.
Add Product → Category relationship.

## Rationale

Improves consistency by replacing free-text values with normalized relationships, supports inventory transactions, and avoids duplicated unit values.
Supports inventory transactions by enforcing consistent relationships between products, categories, and units while avoiding duplicated values.

## Outcome

Accepted.

---

# DD-012 — Immutable Inventory Transactions

## Decision

Inventory transactions are immutable and cannot be edited or deleted.

Inventory corrections are performed by creating adjustment transactions.

## Rationale

Inventory transactions represent historical business events.

Allowing edits or deletions would compromise inventory history, auditability, and stock traceability.

Instead, every inventory movement is preserved as a permanent record.

Benefits include:

- Complete audit trail
- Historical accuracy
- Easier troubleshooting
- Improved reporting
- Better support for future audit logging

## Alternatives Considered

Allow editing or deleting transactions while recalculating product quantities.

This approach was rejected because it increases complexity and risks inconsistent inventory history.

## Outcome

Accepted.

---

# DD-013 — Product as the Inventory Aggregate Root

## Decision

The Product entity is responsible for maintaining its inventory quantity through domain methods.

Methods include:

- IncreaseStock()
- DecreaseStock()
- AdjustStock()

## Rationale

Centralizing inventory behavior inside the Product entity keeps business rules in the Domain layer and prevents inventory logic from being duplicated across application handlers.

## Outcome

Accepted.

---

# DD-014 — Read-only Dashboard Projections

Version Introduced: v0.7.0

## Decision

Implement the Dashboard using read-only DTO projections instead of exposing domain entities directly.

## Rationale

The Dashboard is a reporting feature that aggregates information from multiple sources without modifying business data.

Using dedicated DTO projections:

- Keeps reporting concerns separate from transactional workflows.
- Avoids loading unnecessary entity graphs.
- Improves query performance.
- Reduces coupling between the UI and domain model.
- Allows the Dashboard to evolve independently of domain entities.

## Alternatives Considered

Reuse existing domain entities directly for reporting.

This approach was rejected because reporting requirements differ from transactional workflows and would unnecessarily expose domain models to presentation concerns.

## Outcome

Accepted.

---

# Future Decisions

This document will continue to evolve as the project grows.

Examples:



- Purchase order workflow
- Concurrency handling
- Authentication strategy
- Authorization model
- Audit logging
- Background jobs
- API design
- Caching strategy

---

