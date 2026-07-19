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

- Feature Handlers
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

## Clean Architecture

Separates business logic from infrastructure concerns.

---

## Repository Pattern

Repositories abstract persistence from application logic.

Current repositories include:

- ProductRepository

Future repositories:

- CategoryRepository
- SupplierRepository
- CustomerRepository

---

## Result Pattern

Application operations return standardized results.

Examples:

- Result
- Result<T>

This provides consistent success and error handling.

---

## Paging Pattern

Reusable paging is implemented through:

- PagedRequest
- PagedQuery
- PagedResult<T>

This infrastructure is shared across all future modules.

---

## Filtering Pattern

Reusable filtering currently includes:

- ProductStatusFilter

Future modules will introduce their own filters while following the same pattern.

---

## Sorting Pattern

Reusable sorting currently includes:

- ProductSortFields

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

---

# Future Architecture

Planned additions include:

- Authentication
- Authorization
- Audit Logging
- Inventory Transactions
- Reporting
- Dashboard
- Background Jobs
- API Endpoints

The existing architecture is intended to support these features without major restructuring.