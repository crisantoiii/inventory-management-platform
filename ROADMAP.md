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
v1.3  Account Management       ✅

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

## Version 1.3.0 – Account Management

### Completed

#### Profile

- User Profile
- Update Profile
- Self-Service Account Management

#### Password Management

- Change Password
- Forgot Password
- Reset Password
- Force Password Change

#### Email Verification

- Email Verification
- Verification Request
- Email Confirmation

#### Two-Factor Authentication

- 2FA Setup
- TOTP Verification
- 2FA Login Challenge
- Recovery Codes
- Recovery Code Login
- Recovery Code Regeneration
- Recovery Code Invalidation
- Disable 2FA

### Result

The Account Management vertical slice is now complete.

The implementation provides authenticated users with self-service
account management capabilities while preserving the existing
Clean Architecture, Vertical Slice Architecture, Application
handler patterns, Identity abstraction, and Razor Pages workflows.

The completed functionality includes:

- Self-service user profile management
- Password management
- Email verification
- Two-factor authentication
- Authenticator-based TOTP verification
- Recovery-code authentication
- Recovery-code regeneration
- Recovery-code invalidation
- 2FA disablement

The implementation was verified through actual browser workflows
and completed without requiring structural architectural redesign.

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
- Purchase History
- Purchase History Search
- Purchase History Date Filtering
- Purchase History Pagination
- Purchase History Sorting
- Supplier Purchase Analysis
- Supplier Purchase Analysis Search
- Supplier Purchase Analysis Date Filtering
- Supplier Purchase Analysis Status Filtering
- Supplier Purchase Analysis Pagination
- Supplier Purchase Analysis Sorting
- Supplier Purchase Analysis Purchase Period
- Stock Movement
- Stock Movement Search
- Stock Movement Date Filtering
- Stock Movement Movement Type Filtering
- Stock Movement Pagination
- Stock Movement Sorting

### Remaining Reports

- Low Stock Report
- Inventory Movement Report
- Product Reports

### Export Options

- Excel
- PDF

### Remaining Validation

- Empty database behavior verification
- Explicit query-failure testing

---

# Phase 7 — Dynamic Capability-Based Authorization

Status: ⏳ Planned

**Implementation status:** Design finalized; implementation not yet started.

### Objective

Evolve the existing Identity-based authorization model into a
dynamic capability-based authorization model.

### Model

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

### Capabilities

Capabilities represent atomic application functionality,
actions, or permissions.

Examples:

- PurchaseOrder.View
- PurchaseOrder.Create
- PurchaseOrder.Edit
- PurchaseOrder.Submit
- PurchaseOrder.Approve
- PurchaseOrder.Reject
- PurchaseOrder.Receive

### Groups

Groups compose reusable capabilities into business
responsibilities.

Examples:

- PO Account
- IT Account
- Inventory Manager
- Viewer
- Administrator

### Implementation Goals

- Define capability catalog
- Define groups
- Map groups to capabilities
- Assign groups to users
- Introduce capability authorization
- Preserve existing Identity infrastructure where appropriate
- Preserve Domain state validation
- Apply authorization to Purchasing workflow
- Apply authorization to Reporting
- Validate UI and server-side authorization behavior

### Sequencing

Dynamic Capability-Based Authorization will be implemented
after the current Additional Reporting work.

Additional Reporting does not depend on the new authorization
architecture and can continue independently.

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
| v1.3.0 | Account Management ✅ |
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