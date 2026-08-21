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
v1.2  Reporting                ✅
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

### Sprint 7 Status

Sprint 7 Additional Reporting is complete, verified, and documented.

Completed:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel Export
- PDF Export

Sprint 8 Purchasing Enhancements is complete and closed. P0, P1, P2, P3, P4, P5, P6, and P7 are complete and runtime/browser verified. P7 completed the integrated Purchasing regression pass and corrected one in-scope pagination state-preservation defect.

---

# Sprint 8 - Purchasing Enhancements

## Completed

- P0 - Actual Purchasing Source/Documentation Baseline
- P1 - Multiple Purchase Order Item Management
- P2 - Purchase Order Search
- P3 - Purchase Order Filtering
- P4 - Purchase Order Sorting
- P5 - Purchase Order Pagination
- P6 - Inventory Synchronization During Receiving - Complete and verified
- P7 - Integrated Purchasing Verification - Complete and verified

### P3 - Purchase Order Filtering

Verified filters:
- From Date
- To Date
- Purchase Order Status

Verified:
- Individual filters
- Combined filters
- Search + filter interaction
- Empty-result behavior
- Applicable filter-state preservation
- Existing authorization behavior
- No unrelated Purchasing behavior changed

Runtime/browser verification was completed successfully by the project owner.

## Final Sprint 8 State

- D1 - Documentation Synchronization - Complete
- D2 - Design Decision Synchronization - Complete
- D3 - Final Sprint 8 Retrospective - Complete
- D4 - Final Documentation Validation - Complete

The Sprint 8 final save point has been established. No new feature work begins from this roadmap state; the next activity is Next Sprint Planning.

## Next Sprint Planning

Sprint 8 is closed. The next locked priority is Dynamic Capability-Based Authorization. Its implementation requires a separate Sprint Planning process and must not begin automatically from this closure.

## D1 - Documentation Synchronization

**Status: Complete**

Current-state documentation now reflects the verified Sprint 8 Purchasing sequence through P7, including the integrated verification result and the corrected pagination date-filter state preservation.

No implementation behavior was changed during D1.

## Later
- Additional Purchasing User Experience Improvements

Dynamic Capability-Based Authorization remains outside the completed Purchasing implementation scope. It is the next locked priority after Sprint 8 closure and must not be started automatically as part of this handoff.

---

# Current Release

## Version 1.4.0 – Additional Reporting & Exports

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
- Purchase Order Search
- Purchase Order Filtering
- Purchase Order Sorting
- Purchase Order Pagination

---

# Phase 6 — Reporting

Status: ✅ Complete

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
- Low Stock Report
- Low Stock Search
- Low Stock Pagination
- Low Stock Sorting
- Inventory Movement Report
- Inventory Movement Search
- Inventory Movement Date Filtering
- Inventory Movement Reporting Period
- Inventory Movement Pagination
- Inventory Movement Sorting
- Product Reports
- Product Reports Search
- Product Reports Status Filtering
- Product Reports Pagination
- Product Reports Sorting
- Excel Export
- PDF Export

### Export Options

- Excel
- PDF

### Final Verification

- Empty database behavior verification
- Explicit query-failure testing
- Authorization regression
- Final build verification

### Additional Reporting

Additional Reporting was completed as Sprint 7 within the broader Phase 6 Reporting roadmap.

Completed Sprint 7 scope:

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports
- Excel export
- PDF export
- Final project-wide verification

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

Dynamic Capability-Based Authorization remains planned after the completed Additional Reporting release and the next sprint planning decision.

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
| v1.2.0 | Reporting — Inventory Valuation ✅ |
| v1.3.0 | Account Management ✅ |
| v1.4.0 | Additional Reporting & Exports — Released |
| v1.5.0 | Sprint 8 Purchasing Enhancements — Released |
| v1.6.0 | Sales Module ⏳ |
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

## Sprint 7 Final Verification

- [x] Final Project-wide Verification

### P2 - Purchase Order Search - Complete

**Status: Complete and verified**

P2 implements server-side Purchase Order search using the existing Purchase Order listing/query architecture.

Verified behavior:
- Search by Purchase Order ID.
- Search by Supplier Name.
- Empty or whitespace-only search returns the normal unfiltered list.
- No-match searches return the correct empty result state.
- Search state is preserved through the applicable Purchase Order navigation.
- Existing authorization behavior remains intact.
- Existing Purchase Order list behavior outside search remains unchanged.

The project owner completed runtime/browser verification successfully after implementation.

No P3-P6 functionality was implemented as part of P2.

D1 documentation synchronization was completed after the Sprint 8 P7 verification.
