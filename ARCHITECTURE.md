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

Infrastructure ───────┘

Shared
▲
│
Referenced by all projects
```

Dependencies always point inward toward the Domain.

The Domain project has no knowledge of Infrastructure or Web.

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

## Architecture Validation

The architecture has been validated through business modules, transactional workflows, dashboard reporting, authentication, and comprehensive user management.

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

Each module follows the same layered architecture, repository pattern, CQRS-style application handlers, reusable paging, filtering, and sorting infrastructure, and Razor Pages presentation model.

The successful implementation of multiple independent business modules demonstrates that the architecture scales without requiring structural changes.

## Clean Architecture

Separates business logic from infrastructure concerns.

---

## Repository Pattern

Repositories abstract persistence from application logic.

Current repositories include:

- ProductRepository
- CategoryRepository
- SupplierRepository
- CustomerRepository
- UnitRepository
- InventoryTransactionRepository
- DashboardRepository

---

## Identity Service Pattern

ASP.NET Core Identity is encapsulated behind `IIdentityService`.

The Application layer depends only on the abstraction, while Infrastructure provides the implementation using:

- UserManager
- RoleManager
- SignInManager

This approach isolates framework-specific APIs from the rest of the application and keeps the Web and Application layers independent of ASP.NET Core Identity.

---

## Feature-based Organization

Application logic is organized by feature rather than technical type.

Example:

```text
Features
└── Users
    ├── CreateUser
    ├── GetUsers
    ├── GetUser
    ├── UpdateUser
    ├── UpdateUserRoles
    └── ResetPassword
```

---

## Read Model Pattern

Read-only features such as the Dashboard use repository-based DTO projections rather than exposing domain entities. This keeps reporting queries optimized while preserving domain encapsulation.

---

## Result Pattern

Application operations return standardized results.

Examples:

- Result
- Result\<T>

This provides consistent success and error handling.

---

## Paging Pattern

Reusable paging is implemented through:

- PagedRequest
- PagedQuery
- PagedResult\<T>

This infrastructure is shared across the Product, Category, Supplier, Customer, Unit, and Inventory Transaction modules.

---

## Filtering Pattern

Reusable filtering currently includes:

- Shared status filtering infrastructure

The Product, Category, Supplier, Customer, Unit, and Inventory Transaction modules follow the same filtering approach where applicable.

---

## Sorting Pattern

Reusable sorting currently includes:

- ProductSortFields
- CategorySortFields
- SupplierSortFields
- CustomerSortFields
- UnitSortFields
- InventoryTransactionSortFields

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

Domain Entity

↓

Repository Interface

↓

Repository Implementation

↓

Entity Framework Core

↓

SQL Server
```

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

---

# Design Principles

The project follows:

Architectural Principles

- Clean Architecture
- SOLID
- Separation of Concerns
- Dependency Inversion

Application Design

- Feature-first Organization
- Thin Razor PageModels
- Thin Application Handlers

Engineering Practices

- Rule of Three Refactoring
- Framework Encapsulation
- Consistency over Premature Abstraction
- DRY

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

## Aggregate Root

The Product entity acts as the aggregate root for inventory updates. All inventory changes are performed through Product domain methods to centralize business rules.

## Read-only Dashboard Projections

The Dashboard uses read-only DTO projections to aggregate reporting data without exposing domain entities. This keeps reporting concerns separate from transactional business workflows while leveraging the existing application and repository architecture.

---

# Future Architecture

## Business Modules

- Purchase Orders
- Sales Orders
- Reporting

## Identity Enhancements

- Change Password
- Forgot Password
- Two-Factor Authentication

## Platform Features

- Audit Logging
- REST API
- Background Jobs

The existing architecture is intended to support these features without major restructuring.