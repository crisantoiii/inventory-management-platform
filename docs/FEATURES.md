# Features

## Overview

The Inventory Management Platform is a modern ASP.NET Core 10 Razor Pages application built using Clean Architecture principles.

The platform currently provides business modules for:

- Product
- Category
- Supplier
- Customer
- Unit
- Inventory Transactions
- Purchasing
- Dashboard Reporting
- Reporting
- Authentication
- User Management
- Account Management

Shared infrastructure such as paging, filtering, sorting, the Result pattern, and Identity service abstractions are reused consistently across modules while maintaining a clear separation of concerns.

# Architecture Validation

The project completed **Architecture Sprint 1** after implementing the foundational modules.

The review validated:

- Application Layer
- Infrastructure Layer
- Web Layer
- Shared Infrastructure
- Engineering Documentation

Outcome:

- ✅ Architecture validated
- ✅ No structural redesign required
- ✅ Ready for workflow-driven business modules

The Purchasing Presentation Layer is now complete and has been verified through an end-to-end browser workflow using persisted database records.

The first Reporting vertical slice has also been implemented through Inventory Valuation and verified using actual persisted database records.

## Module Summary

| Module | Status |
|---------|--------|
| Dashboard | ✅ Complete |
| Authentication | ✅ Complete |
| User Management | ✅ Complete |
| Product Management | ✅ Complete |
| Account Management | ✅ Complete |
| Category Management | ✅ Complete |
| Supplier Management | ✅ Complete |
| Customer Management | ✅ Complete |
| Unit Management | ✅ Complete |
| Inventory Transactions | ✅ Complete |
| Architecture Sprint | ✅ Complete |
| Purchasing | 🟨 Core Workflow Complete |
| Reporting | 🟨 Inventory Valuation & Purchase History Complete |

## Current Implementation

Completed modules:

- ✅ Product Management
- ✅ Category Management
- ✅ Supplier Management
- ✅ Customer Management
- ✅ Unit Management
- ✅ Inventory Transactions
- ✅ Dashboard
- ✅ Authentication
- ✅ User Management
- ✅ Purchasing Application Layer
- ✅ Purchasing Presentation Layer
- 🟨 Reporting
  - ✅ Inventory Valuation
  - ✅ Purchase History
    - Server-side Search
    - From/To Date Filtering
    - Server-side Pagination
    - Server-side Sorting
- ✅ Account Management

Shared capabilities:

- CRUD
- Search
- Sorting
- Filtering
- Pagination
- Activate / Deactivate
- Success Notifications

---

# Product Management

## Product Lifecycle

- ✅ Create Product
- ✅ View Product Details
- ✅ Edit Product
- ✅ Activate Product
- ✅ Deactivate Product

## Product Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Product Information

Each product supports:

- SKU
- Barcode
- Name
- Description
- Unit
- Quantity On Hand
- Cost Price
- Selling Price
- Active Status

---

# Category Management

## Category Lifecycle

- ✅ Create Category
- ✅ View Category Details
- ✅ Edit Category
- ✅ Activate Category
- ✅ Deactivate Category

## Category Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Category Information

Each category supports:

- Name
- Description
- Active Status

---

# Supplier Management

## Supplier Lifecycle

- ✅ Create Supplier
- ✅ View Supplier Details
- ✅ Edit Supplier
- ✅ Activate Supplier
- ✅ Deactivate Supplier

## Supplier Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Supplier Information

Each supplier supports:

- Name
- Contact Person
- Email
- Phone
- Address
- Active Status

---

# Customer Management

## Customer Lifecycle

- ✅ Create Customer
- ✅ View Customer Details
- ✅ Edit Customer
- ✅ Activate Customer
- ✅ Deactivate Customer

## Customer Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Customer Information

Each customer supports:

- Name
- Contact Person
- Email
- Phone
- Address
- Active Status

---

# Unit Management

## Unit Lifecycle

- ✅ Create Unit
- ✅ View Unit Details
- ✅ Edit Unit
- ✅ Activate Unit
- ✅ Deactivate Unit

## Unit Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering
- ✅ Success Notifications

## Unit Information

Each unit supports:

- Code
- Name
- Symbol
- Active Status

---

# Inventory Transactions

## Inventory Workflow

- ✅ Create Inventory Transaction
- ✅ View Transaction Details
- ✅ Stock In
- ✅ Stock Out
- ✅ Stock Adjustment

## Transaction Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Success Notifications

## Transaction Information

Each inventory transaction records:

- Product
- Transaction Type
- Quantity
- Reference Number
- Remarks
- Transaction Date

## Business Rules

- Inventory transactions are immutable.
- Product inventory is updated automatically.
- Stock Out validates available inventory.
- Every inventory movement is recorded for audit purposes.

---

# Purchasing

## Purchase Order Workflow

- ✅ Create Purchase Order
- ✅ Get Purchase Order
- ✅ Get Purchase Orders
- ✅ Submit Purchase Order
- ✅ Approve Purchase Order
- ✅ Receive Purchase Order
- ✅ Partial Purchase Order Receiving
- ✅ Final Purchase Order Receiving
- ✅ Completed Purchase Order State
- ✅ Rich Domain Workflow
- ✅ CQRS-style Application Layer

## Purchase Order Presentation

- ✅ Purchase Order Listing
- ✅ Create Purchase Order
- ✅ Purchase Order Details
- ✅ Supplier Selection
- ✅ Product Selection
- ✅ Expected Delivery Date
- ✅ Remarks
- ✅ Ordered Quantity Display
- ✅ Received Quantity Display
- ✅ Remaining Quantity Display
- ✅ Calculated Purchase Order Total
- ✅ Submit Action
- ✅ Approve Action
- ✅ Receive Action

## Validation and Feedback

- ✅ Client-side Receive Quantity Validation
- ✅ Domain Receive Quantity Validation
- ✅ Validation Summaries
- ✅ Success Messages
- ✅ Index Query Failure Feedback
- ✅ Supplier Query Failure Feedback
- ✅ Product Query Failure Feedback

## Purchase Order States

```text
Draft
  ↓ Submit
Submitted
  ↓ Approve
Approved
  ↓ Receive partial quantity
Receiving
  ↓ Receive remaining quantity
Completed
```

## Business Rules

- Purchase Orders begin in Draft status.
- Only Draft Purchase Orders can be submitted.
- Only Submitted Purchase Orders can be approved.
- Only Approved or Receiving Purchase Orders can receive quantities.
- Receiving supports partial quantities.
- Purchase Order completion is determined automatically by the Domain Model.
- Received quantity cannot exceed the remaining quantity.
- Received quantity must be greater than zero.

---

## Reporting

### Completed

- Inventory Valuation
- Purchase History

### Purchase History

- Purchase History Report
- Server-side Search
- From/To Date Filtering
- Server-side Pagination
- Server-side Sorting

### Remaining

- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Export to Excel
- Export to PDF

## Inventory Valuation

- ✅ Inventory Valuation Report
- ✅ Inventory Valuation Read Model
- ✅ Inventory Valuation Application Handler
- ✅ Inventory Valuation Persistence Abstraction
- ✅ Inventory Valuation Repository
- ✅ Read-only EF Core Projection
- ✅ Product-level Inventory Valuation
- ✅ Category Projection
- ✅ Quantity On Hand Display
- ✅ Cost Price Display
- ✅ Inventory Value Display
- ✅ Total Inventory Value
- ✅ Inventory Valuation Navigation
- ✅ Dashboard/Report Value Consistency
- ✅ Browser Verification

## Inventory Valuation Calculation

```text
Inventory Value
= QuantityOnHand × CostPrice
```

The report uses a read-only EF Core projection and does not modify Domain entities or inventory records.

## Reporting Architecture

```text
Presentation
     ↓
Application
     ↓
Read Model
     ↓
IInventoryValuationRepository
     ↓
InventoryValuationRepository
     ↓
EF Core
     ↓
Database
```

## Remaining Reporting Work

- ⬜ Empty database behavior verification
- ⬜ Explicit query-failure testing
- ⬜ Supplier Purchase Analysis
- ⬜ Stock Movement
- ⬜ Low Stock Report
- ⬜ Inventory Movement Report
- ⬜ Excel Export
- ⬜ PDF Export

---

# Dashboard

## Dashboard Overview

- ✅ Inventory statistics
- ✅ Inventory value summary
- ✅ Recent inventory transactions
- ✅ Low stock product monitoring
- ✅ Refresh dashboard

## Dashboard Widgets

- Total Products
- Active Products
- Inactive Products
- Low Stock Products
- Out of Stock Products
- Inventory Value
- Recent Transactions
- Low Stock Product List

## Dashboard Features

- Responsive dashboard layout
- Read-only reporting
- Empty state handling
- Real-time inventory summary

## Dashboard Design

The Dashboard aggregates inventory statistics, recent inventory activity, and low stock alerts into a single read-only view using DTO projections optimized for reporting.
- Read-only repository architecture

---

# Authentication

## Authentication Features

- ✅ Login
- ✅ Logout
- ✅ Cookie Authentication
- ✅ ASP.NET Core Identity
- ✅ Role-based Authorization
- ✅ Policy-based Authorization
- ✅ Two-Factor Authentication Login Challenge
- ✅ Authenticator Code Verification
- ✅ Recovery Code Authentication

## Security

- Protected Razor Pages
- Authorization Policies
- Identity Cookie Authentication
- Secure Password Management

---

# User Management

## User Lifecycle

- ✅ Create User
- ✅ View User Details
- ✅ Edit User
- ✅ Assign Roles
- ✅ Activate User
- ✅ Deactivate User
- ✅ Reset Password

## User Listing

- ✅ Server-side Search
- ✅ Server-side Pagination
- ✅ Server-side Sorting
- ✅ Status Filtering

## User Information

Each user supports:

- Username
- Email
- Assigned Roles
- Active Status
- Lockout Status

---

# Account Management

## Profile

- ✅ User Profile
- ✅ Update Profile
- ✅ Self-Service Account Management

## Password Management

- ✅ Change Password
- ✅ Forgot Password
- ✅ Reset Password
- ✅ Force Password Change

## Email Verification

- ✅ Email Verification
- ✅ Verification Request
- ✅ Email Confirmation

## Two-Factor Authentication

- ✅ 2FA Setup
- ✅ TOTP Verification
- ✅ 2FA Login Challenge
- ✅ Recovery Codes
- ✅ Recovery Code Login
- ✅ Recovery Code Regeneration
- ✅ Recovery Code Invalidation
- ✅ Disable 2FA

---

# Shared Infrastructure

## Shared Identity Infrastructure

Implemented through ASP.NET Core Identity.

Features:

- User Authentication
- Role Management
- Password Reset
- Cookie Authentication
- Authorization Policies
- Identity Service Abstraction
- Email Verification
- Two-Factor Authentication
- Recovery Code Management

## Paging

Implemented through reusable shared models.

Features:

- Page Number
- Page Size
- Total Count
- Total Pages
- Previous Page
- Next Page

Reusable Classes:

- PagedRequest
- PagedQuery
- PagedResult<T>

---

## Sorting

Reusable server-side sorting.

Currently implemented for:

### Products

- SKU
- Name
- Category
- Unit
- Quantity On Hand
- Cost Price
- Selling Price
- Status

### Categories

- Name
- Description
- Status

### Suppliers

- Name
- Contact Person
- Email
- Phone
- Status

### Customers

- Name
- Contact Person
- Email
- Phone
- Status

### Unit

- Code
- Unit Name
- Symbol
- Status

### Inventory Transactions

- Product
- Transaction Type
- Quantity
- Transaction Date

---

## Filtering

Reusable filtering infrastructure.

Currently supports:

- Active
- Inactive
- All

---

## Search

Server-side search is implemented for:

### Products

- SKU
- Product Name
- Category
- Unit
- Quantity On Hand

### Categories

- Category Name
- Description

### Suppliers

- Supplier Name
- Contact Person
- Email

### Customers

- Customer Name
- Contact Person
- Email

### Unit

- Code
- Unit Name
- Symbol

### Inventory Transactions

- Product
- Transaction Type
- Reference Number

---

## Result Pattern

Operation results are standardized using:

- Result
- Result<T>

---

# Architecture Features

- Clean Architecture
- Repository Pattern
- Dependency Injection
- Entity Framework Core
- Razor Pages
- Layered Project Structure
- CQRS-style Application Handlers
- Result Pattern
- Unit of Work Pattern
- Fluent Entity Configurations
- Server-side Paging
- Server-side Sorting
- Server-side Filtering
- Soft Activation / Deactivation
- Reusable Shared Infrastructure
- Inventory Transaction History
- Immutable Business Records
- Automatic Inventory Updates
- Domain-driven Inventory Management
- Dashboard Analytics
- ASP.NET Core Identity
- Identity Service Pattern
- Feature-first Organization
- Thin Razor PageModels
- Application Handler-driven Presentation
- Workflow-oriented Razor Pages
- Thin Application Handlers
- Rule of Three Refactoring
- Architecture Sprint Review
- Workflow-oriented Application Handlers
- Rich Domain Model
- Vertical Slice Architecture

---

# Platform Features

## Identity

- ASP.NET Core Identity
- Cookie Authentication
- Role-based Authorization
- Policy-based Authorization
- Administrative User Management
- Self-Service Account Management
- Email Verification
- Two-Factor Authentication
- Recovery Code Management

## Planned Authorization

The platform will evolve from the current Identity role/policy
authorization model toward Dynamic Capability-Based Authorization.

Planned structure:

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

Status:

- Design finalized
- Implementation not yet started
- Additional Reporting remains the current implementation priority


## User Administration

- Create User
- Assign Roles
- Activate / Deactivate
- Reset Password

## Shared UI

- Server-side Paging
- Server-side Search
- Server-side Sorting
- Status Filtering
- Success Notifications

---

# Engineering Features

The project emphasizes maintainability in addition to business functionality.

Engineering practices include:

- Architecture Review Process
- Feature-first Organization
- Pull Request Workflow
- Versioned Milestones
- Engineering Journal
- Design Decision Records
- Comprehensive Documentation
- Sprint Review Process
- Architecture Validation Reviews
- Technical Debt Tracking
- Sprint Retrospectives

---

# Planned Features

## Purchasing

Remaining work:

- Multiple Purchase Order Item Management
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination
- Inventory Integration During Receiving
- Additional User Experience Improvements

## Reporting

### Completed

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis

### Purchase History

- Purchase History Report
- Server-side Search
- From/To Date Filtering
- Server-side Pagination
- Server-side Sorting

### Supplier Purchase Analysis

- Supplier Purchase Analysis Report
- Supplier-level Purchase Aggregation
- Purchase Period
- Purchase Order Count
- Ordered Quantity
- Received Quantity
- Remaining Quantity
- Total Amount
- Server-side Supplier Search
- From/To Date Filtering
- Status Filtering
- Server-side Pagination
- Server-side Sorting
- Pagination State Preservation
- No-result Behavior
- Browser Verification

### Remaining

- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Export to Excel
- Export to PDF

## Sales

- Sales Orders
- Customer Invoicing
- Stock Reservation
- Sales History