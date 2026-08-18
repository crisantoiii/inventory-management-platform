Architectural decisions are recorded when a significant design direction has been evaluated and accepted.

Where possible, decisions are validated through implementation before being treated as established patterns.

Some decisions may intentionally be recorded before implementation when they define the architecture for an upcoming cross-cutting change. These decisions are explicitly marked as planned and are not treated as implemented capabilities until validated in code.

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

Self-service account operations are implemented separately through
the Account Management feature.

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
- The Identity Service abstraction successfully supports both administrative User Management and self-service Account Management.
- Account Management and Two-Factor Authentication were implemented without requiring structural architectural changes.
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

The Reporting implementations include Inventory Valuation,
Purchase History, and Supplier Purchase Analysis.

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

Reporting features return dedicated DTOs containing only the data required by each report.

Examples include:

- `InventoryValuationDto`
- `PurchaseHistoryDto`
- `SupplierPurchaseAnalysisDto`
- `StockMovementDto`

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

## Aggregated Reporting Consideration

Supplier Purchase Analysis extends the read-only Reporting approach
from direct projections into database-side supplier aggregation.

The report groups Purchase Orders by Supplier and calculates:

- Purchase Order count
- Ordered quantity
- Received quantity
- Remaining quantity
- Total amount
- Earliest Purchase Order date
- Latest Purchase Order date

The aggregation remains inside the Infrastructure query and is
projected into a dedicated read model.

This preserves the same architectural boundary:

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
EF Core Query
     ↓
Database
```

The implementation does not require loading transactional Domain
aggregates into application memory.

## Transactional Reporting Consideration

Stock Movement extends the read-only Reporting approach to
inventory transaction history.

The report reads existing Inventory Transaction records and
projects only the information required by the report:

- Product
- SKU
- Movement Type
- Quantity
- Reference Number
- Remarks
- Transaction Date

The report does not modify Inventory Transaction, Product, or
inventory state.

Filtering, sorting, and pagination remain database-side through
the Infrastructure repository.

The implementation does not require:

- Domain entity changes
- Database schema changes
- New migrations
- A separate transactional model
- Changes to the existing Inventory Transaction workflow

## EF Core Translation Consideration

Supplier Purchase Analysis initially exposed a translation limitation
when sorting directly against a grouped query containing a nested
Purchase Order Item aggregate.

The query was restructured to first project the supplier-level
aggregate values and then apply sorting to those projected values.

This keeps aggregation, sorting, and pagination database-side and
avoids client-side evaluation.

The experience reinforces the existing principle:

- Keep report calculations database-side when practical.
- Structure EF Core queries according to translatable database operations.
- Prefer query restructuring over client-side evaluation.

### Low Stock Reporting Consideration

Low Stock extends the read-oriented Reporting approach to current
Product inventory state.

The report uses the existing Product quantity information and the
existing application low-stock threshold rather than introducing a
separate reporting-specific inventory rule.

The report remains read-only and provides:

- Product
- SKU
- Category
- Quantity On Hand

Filtering, sorting, and pagination remain database-side through the
Infrastructure repository.

The implementation does not require:

- Domain entity changes
- Database schema changes
- New migrations
- Changes to the existing inventory transaction workflow

### Inventory Movement Reporting Consideration

Inventory Movement extends the read-oriented Reporting architecture
from transaction-level movement history into product-level movement
analysis.

Stock Movement remains responsible for displaying individual inventory
transactions.

Inventory Movement instead summarizes movement for each product over
a selected reporting period.

The report provides:

- Product
- SKU
- Opening Quantity
- Stock In
- Stock Out
- Adjustment
- Closing Quantity

The selected reporting period is displayed separately from the table
because the report represents aggregated movement rather than
individual transactions.

The implementation reconstructs opening and closing quantities from
the current inventory state and persisted inventory transactions.

The report remains read-only and does not modify Product or Inventory
Transaction state.

Filtering, aggregation, sorting, and pagination remain database-side
through the Infrastructure repository.

No Domain entity changes, database schema changes, or migrations were
required.

The implementation continues to follow:

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
EF Core Query
     ↓
Database
```

### Product Reports Consideration

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
- Product Status

The report supports:

- Active / Inactive / All Products filtering
- Server-side Product/SKU/Category/Unit search
- Server-side sorting
- Server-side pagination
- Pagination state preservation
- Page-size changes
- Reset behavior
- Combined search and status filtering
- Boundary / No-result behavior

The implementation uses a dedicated read model and repository rather
than reusing the transactional Product management query directly.

The query remains read-only and uses `AsNoTracking()` with database-side
projection, filtering, sorting, and pagination.

No Domain entity changes, database schema changes, or migrations were
required.

The implementation continues to follow:

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
EF Core Query
     ↓
Database
```

The report remains independent of the future Dynamic Capability-Based
Authorization architecture.

### Inventory Movement EF Core Query Adjustment

The initial Inventory Movement query used grouped aggregate projections
combined with left joins.

During browser verification, EF Core raised:

`Nullable object must have a value.`

The issue was caused by nullable SQL results crossing the aggregate
and left-join projection boundary.

The query was restructured to use product-driven correlated aggregate
subqueries with explicit nullable aggregate handling.

This removed the nullable anonymous aggregate join boundary while
keeping the calculations database-side.

The final implementation was browser-verified successfully.

The experience reinforces the existing principle:

- Keep reporting calculations database-side.
- Respect SQL aggregate nullability at the query boundary.
- Prefer query restructuring over client-side evaluation.

## Outcome

Accepted.

Inventory Valuation successfully validates the read-only Reporting projection approach in practice.

The implementation required no structural architectural redesign.

---

# DD-030 — Self-Service Account Management

Version Introduced: v1.3.0

## Decision

Implement Account Management as a self-service capability separate from Administrative User Management.

Authenticated users can manage their own account through dedicated Account pages.

Account Management includes:

- User Profile
- Update Profile
- Change Password
- Forgot Password
- Reset Password
- Force Password Change
- Email Verification
- Two-Factor Authentication

Administrative User Management remains responsible for administrator-driven operations such as:

- Create User
- Edit User
- Assign Roles
- Activate / Deactivate
- Reset Password

## Rationale

Administrative User Management and self-service Account Management have different responsibilities and authorization requirements.

Administrative workflows operate on users managed by an administrator, while Account Management operates only on the currently authenticated user's own account.

Separating these concerns provides:

- Clear authorization boundaries.
- Reduced risk of cross-user account modification.
- Clearer feature organization.
- Simpler security reasoning.
- Independent evolution of administrative and self-service capabilities.

The implementation preserves the existing Identity Service abstraction and Application handler architecture.

## Alternatives Considered

Implement all user and account functionality inside a single User Management or Account area.

This approach was rejected because administrative operations and self-service operations have different responsibilities, authorization requirements, and user experiences.

## Outcome

Accepted.

The Account Management vertical slice was implemented and validated through browser workflows without requiring structural architectural redesign.

---

# DD-031 — Two-Factor Authentication

Version Introduced: v1.3.0

## Decision

Implement Two-Factor Authentication using ASP.NET Core Identity's existing authentication infrastructure with authenticator-based TOTP verification and recovery codes.

Two-factor authentication is integrated into both:

- Account Management for enrollment and configuration.
- Authentication for the login challenge.

The workflow supports:

- 2FA Setup
- TOTP Verification
- 2FA Login Challenge
- Recovery Codes
- Recovery Code Login
- Recovery Code Regeneration
- Recovery Code Invalidation
- Disable 2FA

## Rationale

Two-factor authentication provides an additional authentication factor beyond the user's password.

Using the existing ASP.NET Core Identity infrastructure keeps authentication concerns within the established Identity abstraction rather than introducing a separate authentication mechanism.

Recovery codes provide an alternative authentication method when the user's authenticator is unavailable.

Recovery codes are single-use and are invalidated after successful use. Regenerating recovery codes invalidates the previous set.

Keeping 2FA configuration within Account Management while handling the authentication challenge within the login flow preserves the separation between:

```text
Account Management
        ↓
Configure Account Security

Authentication
        ↓
Verify Authentication Challenge
```

This also preserves the existing separation between self-service account management and administrative user management.

## Alternatives Considered

Implement a custom TOTP or 2FA mechanism outside ASP.NET Core Identity.

This approach was rejected because it would duplicate framework authentication capabilities and introduce unnecessary security-sensitive infrastructure.

Treat 2FA only as an Account Management feature.

This approach was rejected because 2FA configuration and 2FA authentication are separate concerns. Account Management configures the feature, while the authentication flow enforces the second factor during login.

## Outcome

Accepted.

Two-factor authentication was implemented and validated through browser workflows, including:

- Successful 2FA setup.
- Authenticator-code verification.
- 2FA login challenge.
- Recovery-code login.
- Recovery-code regeneration.
- Invalidating previously generated recovery codes.
- Disabling 2FA.

The implementation required no structural architectural redesign.

---

# DD-032 — Dynamic Capability-Based Authorization

Version Introduced: Planned after v1.3.0

## Decision

The platform will evolve its authorization model toward a Dynamic Capability-Based Authorization architecture.

Authorization will be composed from:

```text
User
  ↓
Group
  ↓
Capabilities
  ↓
Application Action
  ↓
Domain State Validation
```

A Capability represents an atomic functionality, action, or permission within the application.

Examples include:

- PurchaseOrder.View
- PurchaseOrder.Create
- PurchaseOrder.Edit
- PurchaseOrder.Submit
- PurchaseOrder.Approve
- PurchaseOrder.Reject
- PurchaseOrder.Receive

A Group represents a reusable collection of capabilities.

Examples include:

- PO Account
- IT Account
- Inventory Manager
- Viewer
- Administrator

Users receive capabilities through their assigned groups.

## Authorization and Domain Rules

Capability authorization does not replace Domain business rules.

An action is valid only when:

```text
Required Capability
        AND
Valid Domain State
```

For example:

```text
Can Submit Purchase Order
=
PurchaseOrder.Submit capability
AND
PurchaseOrder.Status == Draft
```

The Domain aggregate remains responsible for enforcing business state transitions and invariants.

## Rationale

A fixed list of business-specific roles does not scale well as the platform grows.

Different business responsibilities may require different combinations of capabilities.

A capability-based model allows the platform to create reusable groups without introducing a new authorization implementation for every business responsibility.

For example:

```text
PO Account
    ↓
PurchaseOrder.View
PurchaseOrder.Create
PurchaseOrder.Edit
PurchaseOrder.Submit
```

while:

```text
IT Account
    ↓
PurchaseOrder.View
PurchaseOrder.Approve
PurchaseOrder.Reject
PurchaseOrder.Receive
```

This allows responsibilities to evolve through configuration and composition rather than hard-coded role-specific logic.

## Relationship to Existing Identity

The current platform already uses ASP.NET Core Identity, role-based authorization, policy-based authorization, and the Identity Service abstraction.

The new capability model will evolve from this existing foundation rather than immediately replacing the authentication system.

Existing Identity roles will be reviewed and mapped appropriately during implementation.

## Alternatives Considered

### Add every business responsibility as a hard-coded Identity role

Examples:

- PO
- IT
- Warehouse Receiver
- Reporting Analyst

Rejected because the number of roles would grow with individual business responsibilities and combinations.

### Add a separate AccountType property

Rejected because it would create a second authorization concept alongside Identity roles and require synchronization between account type and authorization behavior.

### Implement authorization entirely inside Domain entities

Rejected because user authorization is an application/security concern, while Domain entities should remain responsible for business rules and state invariants.

## Implementation Status

Accepted as the future authorization architecture.

Not yet implemented.

Additional Reporting remains the current implementation priority.

### Implementation Boundary

The future capability model is an authorization model, not an authentication replacement.

Authentication will continue to be responsible for establishing the user's identity.

Authorization will determine whether the authenticated user has the required capability to attempt an application action.

Domain business rules will determine whether the action is valid for the current business state.

---

# DD-033 — Reporting Excel Export

Version Introduced: Sprint 7

## Decision

Excel export for completed Reporting features is implemented as a Web-layer output concern using ClosedXML while reusing the existing read-oriented Reporting handlers, DTOs, filters, and sorting behavior.

The export flow is:

```text
Razor Page
     ↓
Application Handler
     ↓
Existing Report Read Model / DTO
     ↓
Excel Report Writer
     ↓
.xlsx Response
```

The export preserves the report's active filters and sorting but does not preserve the UI pagination limit. The workbook contains the full filtered result set.

The Inventory Valuation export also includes the report-level Total Inventory Value summary already displayed by the browser report.

## Rationale

Excel generation is an output-format concern and does not belong in Domain or the read repositories. Keeping workbook generation in the Web layer avoids coupling the Application and Domain layers to an external document-generation library.

Reusing the existing report queries and DTOs prevents the Excel export from developing separate filtering, sorting, or business rules from the browser report.

No generic reporting export framework was introduced because the current requirement can be satisfied by a focused Excel writer without creating premature abstraction.

## Consequences

- Existing Reporting architecture remains unchanged.
- No Domain entity changes are required.
- No database schema changes or migrations are required.
- Report filters and sorting remain consistent between browser and Excel output.
- Excel-specific formatting remains isolated from the Application and Domain layers.

## Implementation Status

Implemented and browser-verified for the completed Reporting pages.

PDF export is complete for the completed Sprint 7 Reporting scope.

# Future Decisions

This document will continue to evolve as the project grows.

Potential future decisions may include:

- Sales Order workflow
- Audit Logging
- Background Jobs
- REST API
- Caching Strategy

---

# DD-034 — Reporting PDF Export

## Context

PDF export was the remaining export capability for the completed Sprint 7 Reporting features. The goal was to provide downloadable PDF reports while preserving the established read-oriented Reporting architecture and avoiding a generic export framework.

## Decision

Implement PDF generation as a focused Web-layer output concern using QuestPDF.

The export flow is:

```text
Razor Page
    ↓
Application Handler
    ↓
Existing Report DTO / Read Model
    ↓
PdfReportWriter
    ↓
PDF File Response
```

The existing Reporting queries and DTOs remain the source of report data. The PDF writer is responsible only for document composition and presentation.

## Scope

PDF export was implemented for:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports

The export preserves active report filters and sorting while removing the UI pagination limit so the generated PDF contains the full filtered result set.

Inventory Valuation also includes the Total Inventory Value summary already displayed by the browser report.

## Rationale

QuestPDF provides a focused C# document-generation API suitable for the project's report-oriented PDF requirements without requiring PDF generation concerns in Domain, Application, or Infrastructure.

No generic reporting/export abstraction was introduced because the current requirements are satisfied by a focused Web-layer writer.

No Domain entity, database schema, or migration changes were required.

## Verification

PDF Export was built and verified through browser/manual workflows.

Validated:

- PDF export action availability on completed Reporting pages
- PDF generation
- Report-specific columns and values
- Preservation of active filters
- Preservation of active sorting
- Export of the full filtered result set without UI pagination limits
- Inventory Valuation Total Inventory Value summary

## Sprint Position

PDF Export is complete for the current Sprint 7 implementation scope.

Final project-wide verification has been completed successfully.

Verified:

- Normal application regression
- All seven reporting pages
- All seven Excel exports
- All seven PDF exports
- Filters and sorting preservation
- Full filtered result set export
- Multi-page PDF output
- Inventory Valuation Total Inventory Value
- Empty database behavior
- Explicit query failure and database recovery
- Existing authorization boundaries

The implementation remains independent of the future Dynamic Capability-Based Authorization architecture.

