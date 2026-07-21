# Inventory Management Platform

A modern **Inventory Management Platform** built with **ASP.NET Core 10 Razor Pages**, **Entity Framework Core**, and **Clean Architecture**.

The project is designed as a production-style portfolio application that demonstrates enterprise software development practices including layered architecture, reusable infrastructure, server-side data processing, and maintainable code organization.

---

# Project Goals

This project aims to demonstrate:

- Clean Architecture
- Repository Pattern
- Result Pattern
- Dependency Injection
- Entity Framework Core
- Razor Pages
- Server-side Searching
- Server-side Sorting
- Server-side Pagination
- Reusable Filtering Infrastructure
- Scalable Module Design

The long-term goal is to evolve this project into a complete inventory management system suitable for small and medium-sized businesses.

---

# Current Features

## Product Management

### Product Lifecycle

- ✅ Create Product
- ✅ View Product Details
- ✅ Edit Product
- ✅ Activate Product
- ✅ Deactivate Product

### Product Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Category Management

### Category Lifecycle

- ✅ Create Category
- ✅ View Category Details
- ✅ Edit Category
- ✅ Activate Category
- ✅ Deactivate Category

### Category Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

---

# Architecture

The solution follows a layered Clean Architecture approach.

```text
InventoryPlatform
│
├── InventoryPlatform.Web
│
├── InventoryPlatform.Application
│
├── InventoryPlatform.Domain
│
├── InventoryPlatform.Infrastructure
│
└── InventoryPlatform.Shared
```

Responsibilities:

| Project | Responsibility |
|----------|----------------|
| Web | UI, Razor Pages, Dependency Injection |
| Application | Business Logic, Use Cases, DTOs |
| Domain | Entities, Domain Rules |
| Infrastructure | Entity Framework Core, Repositories |
| Shared | Common Infrastructure (Paging, Filtering, Sorting, Result Pattern) |

---

# Shared Infrastructure

Reusable infrastructure has been implemented to support future modules.

## Paging

- PagedRequest
- PagedQuery
- PagedResult\<T>

## Filtering

- ProductStatusFilter (shared across modules)

## Sorting

- ProductSortFields
- CategorySortFields

## Result Pattern

- Result
- Result\<T>

This infrastructure is intended to be reused by all future modules such as Categories, Suppliers, Customers, and Inventory Transactions.

---

# Technology Stack

Backend

- ASP.NET Core 10
- Razor Pages
- Entity Framework Core 10
- SQL Server

Architecture

- Clean Architecture
- Repository Pattern
- Dependency Injection
- Result Pattern

Frontend

- Bootstrap 5
- HTML5
- CSS3

Development Tools

- Visual Studio 2026
- Git
- GitHub

---

# Current Progress

| Module | Status |
|----------|--------|
| Product Management | ✅ Complete |
| Category Management | ✅ Complete |
| Supplier Management | ⬜ Planned |
| Customer Management | ⬜ Planned |
| Dashboard | ⬜ Planned |
| Inventory Transactions | ⬜ Planned |

---

# Roadmap

Upcoming development includes:

- Supplier Management
- Customer Management
- Inventory Transactions
- Dashboard
- Authentication & Authorization
- Reporting
- Audit Logging

---

# Screenshots

Screenshots will be added as the project progresses.

---

# Learning Objectives

This project is focused on applying modern enterprise development practices including:

- Separation of Concerns
- SOLID Principles
- Clean Architecture
- Maintainable Code
- Reusable Components
- Enterprise CRUD Design
- Scalable Repository Design

---

# License

This project is intended for educational and portfolio purposes.
