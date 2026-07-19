# Roadmap

## Overview

This roadmap outlines the planned evolution of the Inventory Management Platform.

The project is developed incrementally, with each completed module validating the architecture before expanding into additional business domains.

---

# Current Release

## Version 0.2

### Completed

### Product Management

- ✅ Product CRUD
- ✅ Product Details
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Product Activation
- ✅ Product Deactivation

### Shared Infrastructure

- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ Result Pattern
- ✅ Shared Paging
- ✅ Shared Filtering
- ✅ Shared Sorting

---

# Phase 1 — Core Master Data

## Category Management

Status: Planned

Features:

- Category CRUD
- Search
- Pagination
- Sorting
- Status Filtering
- Activate / Deactivate

---

## Supplier Management

Status: Planned

Features:

- Supplier CRUD
- Contact Information
- Search
- Pagination
- Sorting

---

## Customer Management

Status: Planned

Features:

- Customer CRUD
- Search
- Pagination
- Sorting

---

## Unit Management

Status: Planned

Features:

- Unit CRUD
- Search
- Pagination

---

# Phase 2 — Inventory Operations

## Inventory Transactions

Planned:

- Stock In
- Stock Out
- Stock Adjustment
- Inventory History

---

## Stock Monitoring

Planned:

- Low Stock Alerts
- Out of Stock Items
- Reorder Level Monitoring

---

# Phase 3 — Dashboard & Reporting

## Dashboard

Planned Widgets:

- Total Products
- Active Products
- Inactive Products
- Categories
- Suppliers
- Low Stock
- Inventory Value

---

## Reporting

Planned Reports:

- Product Report
- Inventory Report
- Transaction Report

Export Options:

- Excel
- PDF

---

# Phase 4 — Security

Authentication

- Login
- Logout
- Password Reset

Authorization

- Administrator
- Inventory Manager
- Viewer

---

# Phase 5 — Advanced Features

Planned:

- Audit Trail
- Activity Logs
- File Uploads
- Product Images
- Barcode Support
- QR Code Support
- Email Notifications

---

# Long-Term Goals

Future enhancements may include:

- REST API
- Mobile Application
- Barcode Scanner Integration
- Multi-Warehouse Support
- Purchase Orders
- Sales Orders
- Inventory Forecasting

---

# Guiding Principles

Each new module should:

- Reuse the shared paging infrastructure.
- Reuse the shared filtering infrastructure.
- Reuse the shared sorting infrastructure.
- Follow Clean Architecture.
- Maintain consistent UI behavior.
- Prefer composition over duplication.

The architecture should evolve through reuse rather than introducing module-specific implementations whenever possible.