# Roadmap

## Overview

This roadmap outlines the planned evolution of the Inventory Management Platform.

The project is developed incrementally, with each completed module validating the architecture before expanding into additional business domains.

---

# Current Release

## Version 0.7.0 – Dashboard

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

### Category Management

- ✅ Category CRUD
- ✅ Category Details
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Category Activation
- ✅ Category Deactivation

### Supplier Management

- ✅ Supplier CRUD
- ✅ Supplier Details
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Supplier Activation
- ✅ Supplier Deactivation

### Customer Management

- ✅ Customer CRUD
- ✅ Customer Details
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Customer Activation
- ✅ Customer Deactivation

### Unit Management

- ✅ Unit CRUD
- ✅ Unit Details
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Unit Activation
- ✅ Unit Deactivation

### Product Foundation

- ✅ Product → Category
- ✅ Product → Unit
- ✅ Quantity On Hand
- ✅ Barcode

### Shared Infrastructure

- ✅ Clean Architecture
- ✅ Repository Pattern
- ✅ Result Pattern
- ✅ Shared Paging
- ✅ Shared Filtering
- ✅ Shared Sorting

### Inventory Transactions

- ✅ Create Inventory Transaction
- ✅ Transaction Details
- ✅ Stock In
- ✅ Stock Out
- ✅ Stock Adjustment
- ✅ Inventory History
- ✅ Product Quantity Updates
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting

### Dashboard

- ✅ Dashboard Overview
- ✅ Inventory Statistics
- ✅ Inventory Value Summary
- ✅ Recent Inventory Transactions
- ✅ Low Stock Products
- ✅ Responsive Dashboard Layout
- ✅ Empty State Handling

---

# Phase 3 — Security

Authentication

- Login
- Logout
- Password Reset

Authorization

- Administrator
- Inventory Manager
- Viewer

---

# Phase 4 — Reporting

Planned Reports:

- Product Report
- Inventory Report
- Transaction Report

Export Options:

- Excel
- PDF

---

# Phase 5 — Advanced Features

Planned:

- Audit Trail
- Activity Logs
- File Uploads
- Barcode Scanner Integration
- Product Images
- QR Code Support
- Email Notifications
- Bulk Import
- Bulk Export

---

# Long-Term Goals

Future enhancements may include:

## Business Modules

- Purchase Orders
- Sales Orders
- Warehouse Management
- Inventory Transfers
- Stock Counts
- Stock Adjustments Approval

## Integrations

- REST API
- Barcode Scanner Integration

## Client Applications

- Mobile Application

## Intelligence

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
- Reuse established application handler patterns.
- Maintain consistent Razor Pages workflows.
- Keep business rules inside domain entities.
- Favor consistency over premature abstraction.
- Prefer DTO projections for read-only reporting features.

The architecture should evolve through reuse rather than introducing module-specific implementations whenever possible.