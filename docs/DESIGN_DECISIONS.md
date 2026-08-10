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
- PurchaseOrderRepository
- InventoryValuationRepository

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

### Validation

Sprint 3 successfully validated this decision through the implementation of the complete Purchasing Application layer.

No architectural redesign was required, confirming that the existing architecture scales effectively from CRUD-oriented modules to workflow-driven business processes.

---

# Architecture Review Outcome

Architecture Sprint 1 validated the decisions recorded in this document.

Sprint 3 and Sprint 4 subsequently extended that validation through the implementation of the Purchasing Application and Presentation layers.

The review confirmed that:

- Existing abstractions remain appropriate.
- No architectural redesign was required.
- The Rule of Three continues to guide shared infrastructure.
- Future development should prioritize business capabilities rather than architectural restructuring.

This document will continue to evolve only when new architectural decisions become necessary.

---

# DD-020 — Rich Domain Model

Version Introduced: v1.0.0

## Decision

Business behavior is encapsulated within Domain aggregates rather than implemented inside Application handlers.

Examples include:

- PurchaseOrder.Create()
- PurchaseOrder.AddItem()
- PurchaseOrder.UpdateItem()
- PurchaseOrder.RemoveItem()
- PurchaseOrder.Submit()
- PurchaseOrder.Approve()
- PurchaseOrder.Receive()

## Rationale

Application handlers should coordinate workflows rather than implement business rules.

Keeping business behavior inside aggregates:

- Preserves aggregate invariants.
- Prevents business rule duplication.
- Simplifies application handlers.
- Improves maintainability as workflows grow.

## Alternatives Considered

Implement business rules directly inside application handlers.

This approach was rejected because it scatters business logic across multiple use cases and weakens aggregate consistency.

## Outcome

Accepted.

---

# DD-021 — Thin Application Handlers

Version Introduced: v1.0.0

## Decision

Application handlers are responsible only for orchestrating workflows.

Handlers:

- Load aggregates.
- Invoke domain behavior.
- Persist changes.
- Return Result<T>.

Handlers do not contain business rules.

## Rationale

Keeping handlers small improves:

- Readability.
- Testability.
- Separation of concerns.

Business decisions remain inside the Domain Model.

## Alternatives Considered

Allow handlers to perform workflow validation and state transitions.

This approach was rejected because it duplicates business rules and leads to an Anemic Domain Model.

## Outcome

Accepted.

---

# DD-022 — Workflow-Oriented Commands

Version Introduced: v1.0.0

## Decision

Commands represent business actions rather than generic CRUD operations.

Examples include:

- SubmitPurchaseOrder
- ApprovePurchaseOrder
- ReceivePurchaseOrder

instead of:

- UpdatePurchaseOrderStatus

## Rationale

Business-oriented commands make application behavior explicit and align the codebase with domain terminology.

Benefits include:

- Improved readability.
- Better alignment with business processes.
- Easier future extension.
- Reduced ambiguity.

## Alternatives Considered

Implement a generic status update command.

This approach was rejected because it hides business intent behind technical operations.

## Outcome

Accepted.

---

# DD-023 — Dedicated Read Models

Version Introduced: v1.0.0

## Decision

Queries return dedicated read models rather than exposing Domain entities.

Examples include:

- GetPurchaseOrderResponse
- GetPurchaseOrderSummaryResponse

Separate read models are used for detail and list views.

## Rationale

Read models should be optimized for presentation rather than persistence.

Benefits include:

- Reduced coupling.
- Better query performance.
- Clear separation between commands and queries.
- Freedom to evolve the Domain independently from the UI.

## Alternatives Considered

Return Domain entities directly to the Presentation layer.

This approach was rejected because it exposes business internals and tightly couples the UI to the Domain Model.

## Outcome

Accepted.

---

# DD-024 — Presentation Layer Uses Application Handlers

Version Introduced: v1.1.0

## Decision

Razor PageModels interact with business capabilities through Application handlers rather than accessing repositories, `DbContext`, or other persistence infrastructure directly.

The Purchasing Presentation layer uses handlers such as:

- GetPurchaseOrdersHandler
- CreatePurchaseOrderHandler
- GetPurchaseOrderHandler
- SubmitPurchaseOrderHandler
- ApprovePurchaseOrderHandler
- ReceivePurchaseOrderHandler

## Rationale

The Presentation layer should focus on HTTP request handling, model binding, validation, and user interaction.

Application handlers remain responsible for coordinating business workflows.

This preserves the established architecture:

```text
Presentation
     ↓
Application
     ↓
Domain
     ↓
Infrastructure
```

Benefits include:

- Preserves Clean Architecture boundaries.
- Keeps persistence concerns out of Razor Pages.
- Keeps business workflows out of the Presentation layer.
- Improves testability.
- Provides a consistent integration pattern for future modules.

## Alternatives Considered

Inject repositories or DbContext directly into Razor PageModels.

This approach was rejected because it would bypass the Application layer and couple the Presentation layer to persistence infrastructure.

## Outcome

Accepted.

---

DD-025 — Purchase Order Details as Workflow Screen

Version Introduced: v1.1.0

Decision

Purchase Order workflow actions are implemented on the Purchase Order Details page rather than creating a separate Razor Page for each workflow action.

The Details page currently supports:

Submit
Approve
Receive
Rationale

The Purchase Order Details page already represents the current state of the Purchase Order and its items.

Keeping workflow actions on the same page:

Keeps the workflow localized.
Reduces unnecessary navigation.
Provides immediate visibility of the current Purchase Order state.
Allows actions to be displayed according to the current status.

The Presentation layer reflects the workflow while the Domain remains responsible for enforcing valid state transitions.

Alternatives Considered

Create separate Razor Pages for:

Submit Purchase Order
Approve Purchase Order
Receive Purchase Order

This approach was rejected because it would fragment a workflow that is naturally centered around the Purchase Order Details view.

Outcome

Accepted.

---

# DD-026 — Item-Level Purchase Order Receiving

Version Introduced: v1.1.0

## Decision

Purchase Order receiving is implemented at the Purchase Order Item level.

Receiving requires:

- Purchase Order ID
- Product ID
- Quantity

The workflow supports partial receiving before the Purchase Order becomes fully completed.

## Rationale

A Purchase Order may contain multiple items and each item may be received independently.

Item-level receiving allows the system to track:

- Ordered quantity
- Received quantity
- Remaining quantity
- Fully received state

This supports real-world partial delivery scenarios.

The Domain remains responsible for enforcing receiving rules.

## Alternatives Considered

Implement a single Purchase Order-level Receive action that marks the entire Purchase Order as received.

This approach was rejected because it cannot represent partial deliveries or item-specific receiving quantities.

## Outcome

Accepted.

---

# DD-027 — Calculated Purchase Order Totals

Version Introduced: v1.1.0

## Decision

Purchase Order TotalAmount remains a calculated Domain property based on Purchase Order items rather than being persisted as duplicated state.

The persistence query loads the Purchase Order items required to calculate the total when generating the Purchase Order summary.

## Rationale

The Purchase Order total is derived from its line items.

Keeping the total calculated:

- Avoids duplicated state.
- Prevents synchronization problems.
- Keeps the total consistent with the underlying items.
- Preserves the Rich Domain Model.

During Sprint 4 integration testing, the Index initially displayed 0.00 while Details displayed the correct total because the list query did not load the Purchase Order items.

The repository query was updated to load the required items rather than introducing a separate persisted total column.

## Alternatives Considered

Add a TotalAmount database column and update it whenever Purchase Order items change.

This approach was rejected because it introduces duplicated state and creates synchronization responsibilities between the item collection and stored total.

## Outcome

Accepted.

---

# DD-028 — Client-Side Validation as UX Layer

Version Introduced: v1.1.0

## Decision

Client-side validation is used to provide immediate user feedback, while Domain validation remains the authoritative enforcement of business rules.

For example, the Receive quantity input prevents zero or negative values through HTML validation, while the Domain independently validates the receiving quantity.

## Rationale

Client-side validation improves usability by preventing obviously invalid submissions before they reach the server.

However, client-side validation cannot be considered a security or business-rule boundary because it can be bypassed.

Therefore:
```text
Client Validation
       ↓
User Experience
       ↓
Application
       ↓
Domain Validation
       ↓
Business Rule Authority
```

This provides both a responsive user experience and reliable business-rule enforcement.

## Alternatives Considered

Rely only on client-side validation.

This approach was rejected because client-side validation can be bypassed.

Rely only on Domain validation.

This approach was rejected because it provides poorer immediate user feedback.

## Outcome

Accepted.

---

# DD-029 — Read-only Reporting Projections

Version Introduced: v1.2.0

## Decision

Read-only Reporting features use dedicated DTO projections rather than loading Domain entities into memory.

The first implementation is Inventory Valuation.

The Reporting flow is:

```text
Presentation
     ↓
Application Handler
     ↓
Read Model
     ↓
Repository Abstraction
     ↓
Infrastructure Repository
     ↓
EF Core Projection
     ↓
Database
```

Inventory Valuation returns a dedicated InventoryValuationDto containing only the data required by the report.

## Rationale

Reporting queries have different requirements from transactional workflows.

A read-only report does not need to load Domain aggregates when it only needs a projection of persisted data.

Using dedicated DTO projections:

- Keeps reporting concerns separate from transactional workflows.
- Avoids unnecessary Domain entity loading.
- Reduces coupling between Presentation and Domain entities.
- Retrieves only the fields required by the report.
- Keeps calculations and projections database-side.
- Supports efficient read-oriented queries.
- Preserves Clean Architecture boundaries.

Inventory Valuation calculates:

```text
Inventory Value
= QuantityOnHand × CostPrice
```

without modifying Product or Inventory Transaction entities.

## Alternatives Considered

Load Product and Category Domain entities and construct the report in application memory.

This approach was rejected because it would retrieve more data than required and move report processing away from the database.

Return Domain entities directly to the Presentation layer.

This approach was rejected because it exposes persistence/domain models to presentation concerns and increases coupling.

## EF Core Translation Consideration

The initial Inventory Valuation query attempted to order the projected DTO directly.

```text
Projection
     ↓
OrderBy(DTO.ProductName)
```

EF Core could not translate the resulting expression.

The query was changed to order the underlying entity property before performing the DTO projection:

```text
Product
     ↓
OrderBy(Product.Name)
     ↓
DTO Projection
     ↓
InventoryValuationDto
```

This keeps ordering, calculation, and projection database-side without introducing client-side evaluation.

## Outcome

Accepted.

Inventory Valuation successfully validates the read-only Reporting projection approach in practice.

The implementation required no structural architectural redesign.

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

