# Code Style

## Philosophy

- Readability over cleverness
- Consistency over personal preference
- Reuse before duplication
- Follow existing project patterns

---

## Project Structure

InventoryPlatform.Web
InventoryPlatform.Application
InventoryPlatform.Domain
InventoryPlatform.Infrastructure
InventoryPlatform.Shared

Describe what belongs in each layer.

---

## Naming Conventions

Classes
Interfaces
Records
Enums
Constants
Methods
Variables

Examples:

ProductRepository
IProductRepository
GetProductsHandler
ProductSortFields

---

## Feature Organization

Each module should contain:

Features/
    GetProducts/
    GetProduct/
    CreateProduct/
    UpdateProduct/
    ActivateProduct/
    DeactivateProduct/

---

## Handler Guidelines

- One handler per use case
- Keep handlers focused
- Return Result<T>
- Avoid business logic duplication

---

## Repository Guidelines

- Query only
- No business rules
- Async methods
- CancellationToken support

---

## Entity Guidelines

- Business rules belong here
- Avoid anemic models where practical
- No infrastructure dependencies

---

## Razor Pages

- One responsibility per page
- Keep PageModels thin
- Delegate work to handlers

---

## Shared Infrastructure

When to place code into Shared.

General rule:

Extract only after it has been proven reusable.

Examples:

- Paging
- Sorting
- Filtering
- Result Pattern

---

## Error Handling

Use Result / Result<T>

Avoid exceptions for expected validation failures.

---

## General Formatting

- File-scoped namespaces
- Expression-bodied members where appropriate
- Meaningful method names
- Consistent spacing