# Roadmap

## Overview

This roadmap outlines the planned evolution of the Inventory Management Platform.

The project is developed incrementally, with each completed module validating the architecture before expanding into additional business domains.

v0.1  Products                 ✅
v0.2  Categories               ✅
v0.3  Suppliers                ✅
v0.4  Customers / Units        ✅
v0.5  Shared Infrastructure    ✅
v0.6  Inventory Transactions   ✅
v0.7  Dashboard                ✅
v0.8  Identity & Users         ✅
v0.9  Architecture Sprint      ✅
v1.0  Purchasing Application   ✅
v1.1  Purchasing Presentation  ✅
v1.2  Reporting                🟨
v1.3  Account Management       ⏳

---

# Current Development Strategy

The project has completed its architectural foundation and the first end-to-end Purchasing vertical slice.

Future development will prioritize expanding business capabilities while preserving the validated architecture.

Focus Areas:

- Business workflows
- Domain modeling
- Enterprise features
- Reporting
- APIs
- Incremental vertical slices

Each major business capability should be implemented from Domain and Application logic through a usable Presentation workflow before being considered complete.

---

# Current Release

## Version 1.2.0 – Reporting: Inventory Valuation

### Completed

- Inventory Valuation Report
- Inventory Valuation Read Model
- Inventory Valuation Application Handler
- Inventory Valuation Persistence Abstraction
- Inventory Valuation Repository
- Read-only EF Core Projection
- Product-level Inventory Valuation
- Category Projection
- Quantity On Hand Display
- Cost Price Display
- Inventory Value Display
- Total Inventory Value
- Inventory Valuation Navigation
- Dashboard/Report Value Consistency
- Browser Verification

### Result

The first Reporting vertical slice is now available as a browser-accessible read-only report backed by actual persisted database data.

Inventory valuation is calculated using:

```text
QuantityOnHand × CostPrice
```

The report total was verified against the existing Dashboard Inventory Value.

The implementation preserves the existing Clean Architecture, Vertical Slice Architecture, Application handler patterns, and repository abstractions.

The Reporting read path is:

```text
Presentation
     ↓
Application
     ↓
Read Model
     ↓
Infrastructure
     ↓
Database
```

### Remaining Reporting Work

- Inventory Valuation empty-state verification
- Explicit query-failure testing
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Excel Export
- PDF Export

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

### Authentication

- ✅ ASP.NET Core Identity
- ✅ Cookie Authentication
- ✅ Login
- ✅ Logout
- ✅ Role-based Authorization
- ✅ Policy-based Authorization

### User Management

- ✅ User Listing
- ✅ User Details
- ✅ Create User
- ✅ Edit User
- ✅ Assign Roles
- ✅ Activate / Deactivate User
- ✅ Reset Password
- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting

---

# Phase 1 — Foundation ✅

---

# Phase 2 — Inventory Core ✅

---

# Phase 3 — Identity & User Management ✅

---

# Phase 4 – Architecture Sprint 1

Objectives

- Review overall solution architecture
- Apply Rule of Three refactoring where justified
- Improve shared UI components
- Standardize Razor Page patterns
- Review dependency registration
- Update project documentation
- Prepare foundation for Purchasing

---

# Phase 5 — Purchasing Module

Status: 🟨 Core Workflow Complete

Completed:

- Purchase Orders
- Purchase Order Items
- Purchase Approval Workflow
- Goods Receiving
- Partial Receiving
- Purchase Order Completion
- Purchasing Presentation Layer
- End-to-End Workflow Validation

Remaining:

- Inventory Integration
- Purchase History
- Supplier Purchase History
- Multiple Purchase Order Item Management
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination

---

# Phase 6 — Reporting

Status: 🟨 In Progress

### Completed

- Inventory Valuation

### Remaining Reports

- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report

### Export Options

- Excel
- PDF

### Remaining Validation

- Inventory Valuation empty-state verification
- Explicit query-failure testing

---

# Phase 7 — Account Management

Status: ⏳ Planned

Planned

- Change Password
- Forgot Password
- Force Password Change
- Email Verification
- Two-Factor Authentication
- User Profile

---

# Phase 8 — Advanced Features

Status: ⏳ Planned

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

- Purchasing Enhancements
- Sales
- Warehouse
- Inventory Transfers
- Cycle Counts
- Returns
- Stock Adjustments Approval

## Integrations

- REST API
- Barcode Scanner Integration

## Client Applications

- Mobile Application

## Intelligence

- Inventory Forecasting

---

# Planned Releases

| Version | Milestone |
|---------|-----------|
| v0.9.0 | Architecture Sprint 1 ✅ |
| v1.0.0 | Purchasing Application Layer ✅ |
| v1.1.0 | Purchasing Presentation Layer ✅ |
| v1.2.0 | Reporting — Inventory Valuation 🟨 |
| v1.3.0 | Account Management ⏳ |
| v1.4.0 | Sales Module ⏳ |
| v2.0.0 | REST API & Blazor ⏳ |

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
- Encapsulate framework-specific implementations behind application abstractions.
- Apply the Rule of Three before introducing shared abstractions.
- Prefer complete vertical slices over isolated technical implementations.
- Validate new workflows through real application usage before considering the feature complete.
- Validate read-oriented queries against actual EF Core translation before introducing client-side evaluation.

The architecture should evolve through reuse rather than introducing module-specific implementations whenever possible.