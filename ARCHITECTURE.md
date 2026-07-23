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

- Razor Pages
- User Interface
- Model Binding
- Dependency Injection Configuration
- Middleware
- Authentication (future)
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

# Current Architectural Patterns

## Architecture Validation

The architecture has been validated through the implementation of three independent business modules:

- Product Management
- Category Management
- Supplier Management
- Customer Management
- Unit Management

Each module follows the same layered architecture, repository pattern, CQRS-style application handlers, reusable paging/filtering/sorting infrastructure, and Razor Pages presentation model.

This consistency demonstrates that the architecture scales across multiple business domains without requiring structural changes.

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

Planned repositories:

- InventoryTransactionRepository

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

This infrastructure is currently shared across the Product, Category, and Supplier modules and is designed to support future modules.

---

## Filtering Pattern

Reusable filtering currently includes:

- Shared status filtering infrastructure

The Product, Category, and Supplier modules all use the same filtering approach, and future modules are expected to follow the same pattern.

---

## Sorting Pattern

Reusable sorting currently includes:

- ProductSortFields
- CategorySortFields
- SupplierSortFields
- CustomerSortFields
- UnitSortFields

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

Business logic remains inside the Application layer.

Persistence remains inside Infrastructure.

---

# Design Principles

The project follows:

- SOLID Principles
- Separation of Concerns
- Dependency Inversion
- DRY (Don't Repeat Yourself)
- Single Responsibility Principle
- Consistency over Premature Abstraction
- Vertical Slice Feature Organization

---

# Future Architecture

Planned additions include:

- Inventory Transactions
- Dashboard
- Reporting
- Authentication
- Authorization
- Audit Logging
- Background Jobs
- API Endpoints

The existing architecture is intended to support these features without major restructuring.