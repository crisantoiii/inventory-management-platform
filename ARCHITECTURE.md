# Architecture

## Overview

The Inventory Management Platform follows a layered **Clean Architecture** to promote maintainability, testability, and separation of concerns.

The solution is organized into independent projects, each with a clearly defined responsibility.

---

# Solution Structure

```text
InventoryPlatform
│
├── InventoryPlatform.Web
├── InventoryPlatform.Application
├── InventoryPlatform.Domain
├── InventoryPlatform.Infrastructure
└── InventoryPlatform.Shared
```

---

# Layer Responsibilities

## InventoryPlatform.Web

The presentation layer.

Responsibilities:

- Authentication
- Authorization
- Razor Pages
- Model Binding
- Dependency Injection
- Middleware
- Static Assets

The Web project is the only layer directly accessed by users.

---

## InventoryPlatform.Application

The application layer contains business use cases.

Responsibilities:

- Feature Handlers (CQRS-style)
- DTOs
- Interfaces
- Validation
- Application Services

The application layer coordinates workflows but contains no persistence logic.

---

## InventoryPlatform.Domain

The domain layer represents the core business model.

Responsibilities:

- Entities
- Domain Rules
- Domain Behavior

The Domain project has no dependencies on other layers.

---

## InventoryPlatform.Infrastructure

The infrastructure layer implements external dependencies.

Responsibilities:

- Entity Framework Core
- ASP.NET Core Identity
- Identity Services
- Repository Implementations
- Database Context
- Configurations
- Persistence

Infrastructure depends on Application and Domain but is never referenced directly by the UI.

---

## InventoryPlatform.Shared

The shared project contains reusable cross-layer components.

Responsibilities:

- Paging Infrastructure
- Filtering Infrastructure
- Sorting Infrastructure
- Result Pattern
- Shared Utilities

Shared components contain no business logic and are designed to be reused throughout the solution.

---

# Dependency Direction

```text
Web
 │
 ▼
Application
 │
 ▼
Domain

Infrastructure
 │
 ├── implements Application abstractions
 └── depends on Domain

Shared
 │
 └── referenced where required
```

Dependencies point inward toward the Domain.

The Domain project has no knowledge of Web or Infrastructure.

Infrastructure implements persistence and external-service abstractions defined by the inner layers.

---

# Dependency Injection Strategy

The solution registers services through extension methods owned by each layer.

```csharp
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWeb();
```

---

# Current Architectural Patterns

## Proven Architecture

The architecture has been validated through business modules, transactional workflows, dashboard reporting, authentication, comprehensive user management, and the Purchasing vertical slice.

The architecture has been validated through the implementation of:

- Product Management
- Category Management
- Supplier Management
- Customer Management
- Unit Management
- Inventory Transactions
- Dashboard Reporting
- Authentication
- User Management
- Purchasing Application Layer
- Purchasing Presentation Layer

Each module follows the established layered architecture, repository pattern, CQRS-style application handlers, reusable paging, filtering, and sorting infrastructure, and Razor Pages presentation model where applicable.

The successful implementation of multiple independent business modules demonstrates that the architecture scales from CRUD-oriented modules to workflow-driven business capabilities without requiring structural redesign.

## Clean Architecture

Separates business logic from infrastructure concerns.

---

## Repository Pattern

Repositories abstract persistence from application logic.

Current repositories include:

- `ProductRepository`
- `CategoryRepository`
- `SupplierRepository`
- `CustomerRepository`
- `UnitRepository`
- `InventoryTransactionRepository`
- `PurchaseOrderRepository`
- `DashboardRepository`
- `InventoryValuationRepository`

`DashboardRepository` and `InventoryValuationRepository` are intentionally implemented as read-only repositories using DTO projections rather than aggregate entities.

---

## Identity Service Pattern

ASP.NET Core Identity is encapsulated behind `IIdentityService`.

The Application layer depends only on the abstraction, while Infrastructure provides the implementation using:

- `UserManager`
- `RoleManager`
- `SignInManager`
- `IIdentityService`

This approach isolates framework-specific APIs from the rest of the application and keeps the Web and Application layers independent of ASP.NET Core Identity.

---

## Feature-based Organization

Application logic is organized by feature rather than technical type.

Example:

```text
Features
└── Purchasing
    ├── CreatePurchaseOrder
    ├── GetPurchaseOrder
    ├── GetPurchaseOrders
    ├── SubmitPurchaseOrder
    ├── ApprovePurchaseOrder
    └── ReceivePurchaseOrder
```

---

# Vertical Slice Architecture

Business capabilities are organized by feature rather than technical type.

Each feature owns its own:

- Request
- Response
- Handler
- Validator (when required)
- Presentation entry point (when applicable)

Examples include:

- `CreatePurchaseOrder`
- `GetPurchaseOrder`
- `GetPurchaseOrders`
- `SubmitPurchaseOrder`
- `ApprovePurchaseOrder`
- `ReceivePurchaseOrder`

The Presentation layer consumes these Application capabilities through Razor PageModels and does not access persistence infrastructure directly.

This organization minimizes coupling while improving discoverability and maintainability.

---

## Read Model Pattern

The platform uses dedicated read models for presentation concerns.

Examples include:

- Dashboard reporting DTOs
- Inventory Valuation DTOs
- GetPurchaseOrderResponse
- GetPurchaseOrderSummaryResponse

Read models are optimized for presentation while remaining independent from Domain entities.

Read-oriented reporting features use DTO projections to retrieve only the data required by the presentation layer.

---

## Result Pattern

Application operations return standardized results.

Examples:

- `Result`
- `Result<T>`

This provides consistent success and error handling.

---

## Paging Pattern

Reusable paging is implemented through:

- `PagedRequest`
- `PagedQuery`
- `PagedResult<T>`

This infrastructure is shared across the Product, Category, Supplier, Customer, Unit, and Inventory Transaction modules.

---

## Filtering Pattern

Reusable filtering currently includes:

- Shared status filtering infrastructure

The Product, Category, Supplier, Customer, Unit, and Inventory Transaction modules follow the same filtering approach where applicable.

---

## Sorting Pattern

Reusable sorting currently includes:

- `ProductSortFields`
- `CategorySortFields`
- `SupplierSortFields`
- `CustomerSortFields`
- `UnitSortFields`
- `InventoryTransactionSortFields`

The infrastructure supports server-side sorting through strongly typed sort definitions.

---

# Request Flow

Typical request lifecycle:

```text
Browser
   ↓
Razor Page
   ↓
Application Handler
   ↓
Domain Entity / Aggregate
   ↓
Repository Interface
   ↓
Repository Implementation
   ↓
Entity Framework Core
   ↓
SQL Server
```

# Command and Query Separation

The Application layer distinguishes between commands that modify business state and queries that return optimized read models.

Commands

- Create Purchase Order
- Submit Purchase Order
- Approve Purchase Order
- Receive Purchase Order

Queries

- Get Purchase Order
- Get Purchase Orders

Commands delegate business behavior to Domain aggregates.

Queries return dedicated DTO projections optimized for presentation.

# Presentation Validation

The Presentation layer performs user-facing validation to provide immediate feedback.

Examples include:

- Required field validation
- Receive quantity constraints
- Validation summaries

Presentation validation improves user experience but does not replace Domain validation.

Business rules remain enforced by the Domain Model.

```text
User Input
    ↓
Presentation Validation
    ↓
Application Handler
    ↓
Domain Validation
    ↓
Persistence
```

Client-side validation can be bypassed, so business invariants must remain protected by the Domain layer.


# Identity Request

```text
Browser

↓

Razor Page

↓

Application Handler

↓

IIdentityService

↓

IdentityService

↓

UserManager / RoleManager

↓

ASP.NET Core Identity

↓

SQL Server
```

Application workflows are coordinated by the Application layer.

Core business rules and domain behavior remain inside the Domain layer.

Persistence remains inside Infrastructure.

---

# Inventory Transaction Workflow

```text
User

    │

    ▼

Create Inventory Transaction

    │

    ▼

Application Handler

    │

    ▼

Validate Request

    │

    ▼

Product Domain Methods

    ├── IncreaseStock()
    ├── DecreaseStock()
    └── AdjustStock()

    │

    ▼

Create InventoryTransaction

    │

    ▼

Save Changes

    │

    ▼

Updated Product Quantity
```

---

# Dashboard Request Workflow

```text
Browser

    │

    ▼

Dashboard Razor Page

    │

    ▼

Application Handler

    │

    ▼

Dashboard Repository

    │

    ▼

Read-only DTO Projections

    │

    ▼

SQL Server

    │

    ▼

Dashboard View
```
---

# Inventory Valuation Request Workflow

```text
Browser

    │

    ▼

Inventory Valuation Razor Page

    │

    ▼

GetInventoryValuationHandler

    │

    ▼

IInventoryValuationRepository

    │

    ▼

InventoryValuationRepository

    │

    ▼

Read-only EF Core Projection

    │

    ▼

SQL Server

    │

    ▼

InventoryValuationDto

    │

    ▼

Inventory Valuation View
```

The Inventory Valuation report is implemented as a read-only vertical slice.

The query projects only the fields required by the report and calculates:

```text
Inventory Value
= QuantityOnHand × CostPrice
```

The query does not modify Domain entities or inventory records.

---

# Reporting Architecture

Reporting features use a read-oriented application flow separate from transactional Domain workflows.

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

The Reporting layer does not require Domain aggregate mutation when the feature is read-only.

This allows reporting queries to use dedicated DTO projections while preserving the existing Clean Architecture boundaries.

Inventory Valuation is the first dedicated Reporting vertical slice implemented using this approach.

The implementation did not require structural architectural redesign.

---

# Purchasing Workflow

```text
Create Purchase Order
        ↓
Draft
        ↓ Submit
Submitted
        ↓ Approve
Approved
        ↓ Receive
Receiving
        ↓ Receive remaining quantity
Completed
```

The Presentation layer exposes the workflow through Razor Pages.

The Application layer coordinates the use cases through business-oriented handlers.

The `PurchaseOrder` aggregate owns workflow state transitions and business rules.

Infrastructure persists the resulting state through the repository and Unit of Work.

The Purchasing workflow is implemented through business-oriented Application handlers that delegate workflow transitions to the `PurchaseOrder` aggregate.

The Application layer coordinates the workflow while the Domain Model owns all business rules and state transitions.

---

# Workflow-driven Business Modules

The Purchasing module represents the platform's first workflow-driven business capability.

Unlike CRUD-oriented modules, Purchasing models explicit business state transitions:

```text
Draft
   ↓
Submitted
   ↓
Approved
   ↓
Receiving
   ↓
Completed
```

The workflow is exposed through the Presentation layer while transitions are implemented through Domain behavior.

Workflow transitions are implemented through business-oriented Application handlers that delegate state changes to the PurchaseOrder aggregate.

This separation ensures:

- Presentation handles user interaction.
- Application coordinates use cases.
- Domain enforces business rules and state transitions.
- Infrastructure handles persistence.

This demonstrates the architecture's ability to support aggregate-driven enterprise workflows while preserving Clean Architecture principles.

---

# Design Principles

The project follows:

### Architectural Principles

- Clean Architecture
- SOLID
- Separation of Concerns
- Dependency Inversion

### Application Design

- Feature-first Organization
- Thin Razor PageModels
- Application Handler-driven Presentation
- Workflow-oriented Razor Pages
- Thin Application Handlers
- Vertical Slice Architecture
- Request / Response / Handler Pattern
- Dedicated Read Models
- Workflow-oriented Commands

### Engineering Practices

- Rule of Three Refactoring
- Framework Encapsulation
- Consistency over Premature Abstraction
- DRY
- Validate architecture before expanding business domains

---

## Incremental Refactoring

The solution follows the Rule of Three when introducing reusable abstractions.

Patterns are extracted only after demonstrating reuse across multiple independent features.

This avoids premature abstraction while maintaining long-term maintainability.

---

# Key Architectural Decisions

## Immutable Inventory History

Inventory transactions are treated as historical records and cannot be edited or deleted.

Corrections are performed by creating adjustment transactions, preserving a complete audit trail.

## Aggregate Roots

Current aggregate roots include:

### Product

Responsible for inventory operations through domain behavior.

### PurchaseOrder

Responsible for the complete purchasing workflow and owns PurchaseOrderItems.

Business rules and workflow transitions are encapsulated within the aggregate.

## Read-only Dashboard Projections

The Dashboard uses read-only DTO projections to aggregate reporting data without exposing domain entities. This keeps reporting concerns separate from transactional business workflows while leveraging the existing application and repository architecture.

## Read-only Reporting Projections

Read-only Reporting features should prefer database-side DTO projections rather than loading Domain entities into memory.

Inventory Valuation uses an EF Core projection:

```text
Product
   ↓
OrderBy(Product.Name)
   ↓
DTO Projection
   ↓
InventoryValuationDto
```

The initial implementation attempted to order the projected DTO directly. EF Core could not translate the DTO constructor expression used by the ordering operation.

The query was therefore changed to order the underlying entity property before performing the DTO projection.

This keeps filtering, ordering, calculation, and projection database-side and avoids unnecessary client-side evaluation.

## Architecture Validation Before Expansion

The project deliberately introduced an Architecture Sprint before implementing workflow-driven modules.

Rather than continuously adding new features, the architecture was reviewed to confirm that existing abstractions remained appropriate.

This milestone established a stable foundation for future business modules without requiring structural redesign.

### Validation

Sprint 3 validated this architectural decision through the implementation of the Purchasing Application layer.

Sprint 4 extended that validation into the Presentation layer by connecting the existing Purchasing Application handlers to Razor Pages and verifying the complete Purchase Order lifecycle through actual browser interactions and persisted database records.

The combined implementation demonstrated that the architecture supports workflow-driven vertical slices from Presentation through Application, Domain, Infrastructure, and database persistence without requiring structural redesign.

---

# Architecture Sprint 1

Architecture Sprint 1 was completed after the implementation of the platform foundation, including:

- Master Data
- Inventory Transactions
- Dashboard
- Authentication
- User Management

The objective of the sprint was to validate the architecture before introducing larger workflow-driven business modules.

## Review Scope

- Application Layer
- Infrastructure Layer
- Web Layer
- Shared Infrastructure
- Documentation

## Result

The review confirmed that:

- Clean Architecture boundaries remain consistent.
- Feature-first organization scales effectively.
- Shared infrastructure has been successfully reused across independent modules.
- Repository and Unit of Work patterns remain appropriate.
- ASP.NET Core Identity integration preserves architectural separation.
- Razor Pages maintain consistent UI patterns.

Sprint 3 subsequently validated these findings through the implementation of the Purchasing Application layer, demonstrating that the existing architecture scales successfully into workflow-driven business modules without requiring structural redesign.

Sprint 4 extended the Purchasing implementation into the Presentation layer and verified the complete workflow through the browser without requiring architectural changes.

---

# Future Architecture

## Business Modules

- Sales
- Warehouse
- Blazor Administration

## Identity Enhancements

- Change Password
- Forgot Password
- Two-Factor Authentication

## Platform Features

- REST API
- Audit Logging
- Background Jobs

### Reporting Enhancements

- Additional reporting modules
- Excel Export
- PDF Export
- Advanced report filtering
- Advanced report sorting

The existing architecture is intended to support these features without major restructuring.