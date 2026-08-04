Each decision is recorded only after implementation has validated the approach in practice, ensuring that architectural guidance reflects proven patterns rather than speculative design.

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

The architecture has now been validated across:

- Product Management
- Category Management
- Supplier Management
- Customer Management
- Unit Management
- Inventory Transactions
- Dashboard Reporting
- Authentication
- User Management

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

# DD-015 — Identity Service Abstraction

Version Introduced: v0.8.0

## Decision

Encapsulate ASP.NET Core Identity behind `IIdentityService` rather than exposing `UserManager`, `RoleManager`, or `SignInManager` to the Application or Web layers.

## Rationale

ASP.NET Core Identity is a framework concern and should remain isolated within the Infrastructure layer.

The Application layer coordinates user management workflows through an abstraction without depending on framework-specific APIs.

Benefits include:

- Preserves Clean Architecture boundaries.
- Reduces coupling to ASP.NET Core Identity.
- Simplifies testing.
- Allows Identity implementation details to evolve independently of business workflows.

## Alternatives Considered

Inject `UserManager` and `RoleManager` directly into Razor Pages or application handlers.

This approach was rejected because it tightly couples business workflows to the Identity framework.

## Outcome

Accepted.

---

# DD-016 — Administrative User Management

Version Introduced: v0.8.0

## Decision

Separate administrative user management from self-service account management.

Administrative operations include:

- Create User
- Edit User
- Assign Roles
- Activate / Deactivate
- Reset Password

Self-service account operations will be implemented separately.

## Rationale

Administrative workflows differ significantly from end-user account management.

Separating these concerns results in:

- Clearer authorization boundaries.
- Simpler page organization.
- Easier future expansion.

## Alternatives Considered

Implement all identity-related functionality inside a single Account area.

This approach was rejected because administrator features and end-user features have different responsibilities and security requirements.

## Outcome

Accepted.

---

# DD-017 — Feature-first Organization

Version Introduced: v0.8.0

## Decision

Organize the Application layer by feature rather than by technical type.

Example:

Features
└── Users
    ├── CreateUser
    ├── GetUser
    ├── GetUsers
    ├── UpdateUser
    ├── UpdateUserRoles
    └── ResetPassword

## Rationale

Feature-first organization keeps all components of a use case together.

Benefits include:

- Improved discoverability.
- Better maintainability.
- Reduced navigation across folders.
- Easier onboarding for new developers.

## Alternatives Considered

Organize handlers, DTOs, and validators into separate technical folders.

This approach was rejected because related code becomes fragmented as the application grows.

## Outcome

Accepted.

---

# DD-018 — Rule of Three Refactoring

Version Introduced: v0.8.0

## Decision

Reusable abstractions are introduced only after demonstrating value across multiple independent implementations.

## Rationale

The project intentionally avoids premature abstraction.

Infrastructure and shared components are extracted only after proving their usefulness through repeated implementation.

Examples include:

- Shared Paging
- Shared Filtering
- Shared Sorting
- IdentityResult mapping
- Shared UI components (future)
- IdentityService helper methods

Benefits include:

- Simpler initial implementations.
- Reduced speculative design.
- Better long-term maintainability.
- Abstractions driven by real requirements rather than assumptions.

## Alternatives Considered

Create generic infrastructure before multiple implementations exist.

This approach was rejected because it often increases complexity without providing immediate value.

## Outcome

Accepted.

---

# DD-019 — Architecture Validation Before Business Expansion

Version Introduced: v0.9.0

## Decision

Conduct an Architecture Sprint after completing the platform foundation and before implementing workflow-driven business modules.

## Rationale

Rather than continuing to add new functionality indefinitely, the project pauses at key milestones to validate that the existing architecture remains appropriate.

Architecture Sprint 1 reviewed:

- Application Layer
- Infrastructure Layer
- Web Layer
- Shared Infrastructure
- Engineering Documentation

The review confirmed that the existing architecture scaled successfully across:

- Master Data
- Inventory Transactions
- Dashboard Reporting
- Authentication
- User Management

without requiring structural redesign.

Benefits include:

- Confirms architectural decisions before expanding the solution.
- Prevents unnecessary redesign later in the project.
- Identifies implementation improvements while preserving stable architecture.
- Provides confidence that future business modules can build upon the existing foundation.

## Alternatives Considered

Continue implementing additional business modules without performing an architectural review.

This approach was rejected because architectural drift becomes increasingly difficult to correct as the project grows.

## Outcome

Accepted.

Architecture Sprint 1 concluded that the existing architecture remains stable and is ready for expansion into workflow-driven business domains beginning with the Purchasing Module.

---

# Architecture Review Outcome

Architecture Sprint 1 validated the decisions recorded in this document.

The review confirmed that:

- Existing abstractions remain appropriate.
- No architectural redesign was required.
- The Rule of Three continues to guide shared infrastructure.
- Future development should prioritize business capabilities rather than architectural restructuring.

This document will continue to evolve only when new architectural decisions become necessary.

---

# Future Decisions

This document will continue to evolve as the project grows.

Examples:

- Account Management
- Change Password workflow
- Purchase Order workflow
- Sales Order workflow
- Audit Logging
- Background Jobs
- REST API
- Caching Strategy

---

