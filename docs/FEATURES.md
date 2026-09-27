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

# Sprint 20 - Provider-Neutral Continuous Verification

Sprint 20 added an engineering verification capability, not a user-facing feature: provider-neutral continuous verification. Every IntegrationTest now carries exactly one explicit `TestTier` — `ProviderNeutral` or `SqlServerRelational` — enforced by a fail-safe classification audit that rejects missing, duplicate/multiple, and unknown tier values and locks the 14-identity SQL-provider-bound inventory by fully qualified name. Classification follows actual runtime provider dependency (tests that connect or attempt to connect to SQL Server/LocalDB are `SqlServerRelational`), not folder or namespace.

- ✅ Shared verification command: `scripts/verify-provider-neutral.ps1` — repository-local `dotnet-ef 10.0.10` tool restore and resolution evidence, solution restore, normal Release build, UnitTests, Web.Tests, affirmative `TestTier=ProviderNeutral` IntegrationTests, EF `migrations has-pending-model-changes`, distinct TRX files under `artifacts/verification/`, explicit executed/excluded tier summary, strict exit-code propagation, caller-working-directory independence
- ✅ GitHub Actions gate: `.github/workflows/provider-neutral-verification.yml` — PR→`main`, push→`main`, `workflow_dispatch`; `windows-latest`; .NET `10.0.x`; `contents: read`; full delegation to the shared script; `provider-neutral-verification-results` TRX artifact upload with `if: always()`; no secrets, no database/browser setup
- ✅ Hosted run: the job `Provider-neutral verification (windows-latest)` completed successfully on a clean hosted runner, uploading a three-file TRX artifact
- ✅ Zero provider contact: with `MSSQLLocalDB` stopped before an integrated gate run, it remained stopped afterward (last-start unchanged); no connection, database creation, or provider execution attempt
- ✅ Verification-driven remediation: one misclassified LocalDB connection probe was reclassified from `ProviderNeutral` to `SqlServerRelational` (relational inventory 13 → 14, ProviderNeutral 127 → 126) after T03 verification empirically exposed its real provider contact

The provider-neutral gate executes the normal Release build, UnitTests (346), Web.Tests (63), and the 126 ProviderNeutral IntegrationTests, and explicitly does not execute the 14 `SqlServerRelational` tests or browser/E2E work — no SQL relational-pass claim is made for Sprint 20 (Sprint 18's relational passes remain historical evidence). SQL Server relational CI remains future work. Sprint 20 made zero production changes and was technical/non-release; v1.6.0 remains the latest release.

# Sprint 19 - Purchase Order Create HTTP/Razor Integration Coverage

Sprint 19 added an engineering verification capability, not a user-facing feature: the Sprint 17 database-safe HTTP/Razor integration foundation now verifies the real `/Purchasing/PurchaseOrders/Create` pipeline end-to-end through five accepted HTTP behaviors (H1-H5):

- ✅ H1 — Anonymous GET challenges to Identity login with the correct semantic ReturnUrl
- ✅ H2 — Persisted seeded `manager@inventory.local` accesses the real Create page
- ✅ H3 — Persisted test-only `purchaseorder-denied@inventory.test`, with no authorization-group assignment, is denied by real `PurchaseOrder.Create` capability authorization (the seeded Viewer is NOT the H3 identity because it already possesses `PurchaseOrder.Create`; no production seed or authorization bypass was added)
- ✅ H4 — Valid rendered-antiforgery indexed POST → real PageModel/Application handler → immediate 302 PRG → same-factory persistence of a Draft Purchase Order plus one item (marker `S19-T03-VALID-CREATE`)
- ✅ H5 — Duplicate ProductId rows trigger the real Domain invariant with canonical message `The product already exists in this purchase order.`, HTTP 200 redisplay with Supplier/Product options, selection, date, remarks, and both rows/values restored, PO/item counts unchanged, and no order persisted (marker `S19-T04-DUPLICATE`)

The tests run through the real ASP.NET Core pipeline on the database-safe per-factory InMemory host with production capability authorization and rendered antiforgery. H1-H5 are EF Core InMemory HTTP-host evidence, not SQL Server relational evidence; Sprint 18 remains the relational architecture authority. The final baseline is 542/542 (346 UnitTests, 133 IntegrationTests, 63 Web.Tests; 0 failed, 0 skipped), with the relational tier freshly re-executed against available LocalDB at the Sprint 19 T05 integrated verification. Sprint 19 made zero production changes and was technical/non-release; v1.6.0 remains the latest release.

# Sprint 18 - SQL Server Relational Verification

Sprint 18 added an engineering verification capability, not a user-facing feature: 41 tests under `tests/InventoryPlatform.IntegrationTests/Relational/` exercise SQL Server LocalDB through guarded, unique disposable databases. The tier applies the real migrations and proves the bounded R1-R6 contracts: fresh-schema migration, Product SKU uniqueness, Category FK delete restriction, one-save atomicity, representative `decimal(18,2)` storage behavior, and the actual Inventory Movement production query's SQL Server translation and deterministic aggregates.

The durable safety boundary is LocalDB-only on `(localdb)\MSSQLLocalDB`, trusted authentication, `InventoryPlatformRelationalTests_<guid>` database names, validation before destructive operations, fail-hard/no-skip behavior, no production `DefaultConnection`, no production seeders, guarded cleanup, and default xUnit parallelism. R4 is not a blanket workflow-transaction guarantee; R5 does not attribute conversion to EF or SQL Server; R6 does not prove all reports. The Sprint 18 closure baseline was 537/537 (346 UnitTests, 133 IntegrationTests, 58 Web.Tests; 0 failed, 0 skipped) — historical, superseded as the current baseline by Sprint 19's 542/542. Sprint 18 made zero production changes and was technical/non-release; v1.6.0 remains the latest release.

# Sprint 17 - HTTP/Razor Integration-Test Foundation

Sprint 17 added a real HTTP/Razor integration-test foundation (a testing capability, not a product feature). Representative automated HTTP proofs now exist:

- ✅ Database-safe in-process `WebApplicationFactory` test host: early sentinel connection containment, pre-provider `ApplicationDbContext` replacement, fail-closed structural validation, unchanged real startup seeding, unique InMemory database per factory
- ✅ Test-only seeded-user authentication resolving real persisted GUID identities; production capability authorization preserved; no fabricated authorization claims
- ✅ Category Create GET authorization matrix: anonymous → Identity login challenge; InventoryManager → HTTP 200 rendered page; Viewer → Identity access-denied forbid
- ✅ Category Create successful POST: real rendered antiforgery token + matching cookie, real model binding/PageModel/Application/persistence, successful 302 PRG, same-factory persistence verification
- ✅ Purchase Order empty-Draft Submit domain failure: actual `A purchase order must contain at least one item.` invariant through the real Details/Submit form and `PurchaseOrder.Submit` authorization; existing `DomainException` handling redisplays the page with HTTP 200; navigation state preserved; PO remains Draft with zero items (no unintended Submit mutation)

Coverage is representative, not exhaustive: broader HTTP/Razor route coverage, browser automation, relational-provider verification, and CI automation remain deferred. No relational behavior is claimed from the InMemory-backed test host. See `docs/TESTING_CONVENTIONS.md` for the durable HTTP integration-test conventions.

# Sprint 9 - Code Quality & Consistency

Sprint 9 improves consistency across existing features without adding a new business capability.

Verified implementation changes:

- Purchase History and Supplier Purchase Analysis report forms use `asp-for`.
- Purchase Order sorting and pagination use Razor `asp-route-*` navigation.
- Seven core list PageModels bind complete Application Requests without duplicate paging parameters.
- `PageNum` is the standard Razor/UI paging property and query parameter.
- Seven list-page pagination links use `asp-route-PageNum`.
- Purchase Order Details preserves list pagination and filter/sort context.
- Redundant inherited repository interface declarations were removed.
- Purchase Order Status options are rendered once and filter labels are associated with explicit control IDs.

These changes preserve existing feature behavior and the established Razor -> Application -> Infrastructure boundaries. They do not represent new business features.

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

The Purchasing Presentation Layer is complete and has been verified through an end-to-end browser workflow using persisted database records. Sprint 8 P1-P7 extended and verified the Purchase Order workflow, including multiple items, search, filtering, sorting, pagination, receiving, and inventory synchronization.

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
| Purchasing | ✅ Core Workflow + Sprint 8 Enhancements + Sprint 14 Cancellation/Edit + Sprint 15/16 POST Presentation and Round-Trip Hardening Complete |
| Reporting | ✅ Sprint 7 Additional Reporting Complete |
| Dynamic Capability-Based Authorization | ✅ Sprint 10 Complete — v1.6.0 released |

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
- ✅ Purchase Order Search (P2)
- ✅ Purchase Order Filtering (P3)
- ✅ Purchase Order Sorting (P4)
- ✅ Purchase Order Pagination (P5)
- ✅ Inventory Synchronization During Receiving (P6)
- ✅ Integrated Purchasing Verification (P7)
- ✅ Purchase Order Cancellation (Sprint 14)
- ✅ Draft Purchase Order Item Editing and Removal (Sprint 14)
- ✅ Reporting
  - ✅ Inventory Valuation
  - ✅ Purchase History
    - Server-side Search
    - From/To Date Filtering
    - Server-side Pagination
    - Server-side Sorting
  - ✅ Supplier Purchase Analysis
  - ✅ Stock Movement
  - ✅ Low Stock Report
  - ✅ Inventory Movement Report
  - ✅ Product Reports
  - ✅ Excel Export
  - ✅ PDF Export
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
- ✅ Cancel Purchase Order (Draft and Submitted states — Sprint 14)
- ✅ Purchase Order Cancellation Terminal State (Sprint 14)
- ✅ Draft Purchase Order Item Editing and Removal (Sprint 14)
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
- ✅ Cancel Action (Draft and Submitted states)
- ✅ Edit Items Action (Draft only, dedicated Edit page)

## Validation and Feedback

- ✅ Client-side Receive Quantity Validation
- ✅ Domain Receive Quantity Validation
- ✅ Validation Summaries
- ✅ Expected workflow failures (Submit/Approve/Receive/Cancel) render as inline validation feedback instead of HTTP 500, with no persistence (Sprint 15)
- ✅ All six Details/Edit workflow forms preserve `Descending` in both states using explicit hidden string values (Sprint 16)
- ✅ Expected Create `DomainException` failures render inline after restoring item and dropdown data, with no persistence (Sprint 16)
- ✅ Success Messages
- ✅ Index Query Failure Feedback
- ✅ Supplier Query Failure Feedback
- ✅ Product Query Failure Feedback

## Purchase Order States

```text
Draft
  ↓ Submit          ↓ Cancel
Submitted
  ↓ Approve         ↓ Cancel
Approved
  ↓ Receive partial quantity
Receiving
  ↓ Receive remaining quantity
Completed
```

Cancellation is available from Draft and Submitted states only and is enforced by the `PurchaseOrder.Cancel` capability. `Cancelled` is terminal: a cancelled Purchase Order offers no further workflow actions.

## Business Rules

- Purchase Orders begin in Draft status.
- Only Draft Purchase Orders can be submitted.
- Only Submitted Purchase Orders can be approved.
- Only Approved or Receiving Purchase Orders can receive quantities.
- Receiving supports partial quantities.
- Purchase Order completion is determined automatically by the Domain Model.
- Received quantity cannot exceed the remaining quantity.
- Received quantity must be greater than zero.
- Only Draft and Submitted Purchase Orders can be cancelled (Sprint 14).
- Cancelled is terminal: a cancelled Purchase Order cannot Submit, Approve, Receive, edit items, remove items, return to Draft, or reopen (Sprint 14).
- Only Draft Purchase Orders can edit or remove items; item mutation is identified by `ProductId`, only Quantity and UnitCost can be updated, and Product replacement is not supported (Sprint 14).
- Removing the final item is allowed and an empty Draft may exist temporarily; a Purchase Order with no items still cannot be submitted (Sprint 14).

---

## P6 - Inventory Synchronization During Receiving

**Status: Complete and verified**

Receiving now synchronizes the existing Purchasing workflow with inventory:

- Valid Purchase Order receiving increases the Product `QuantityOnHand`.
- A corresponding `StockIn` InventoryTransaction is recorded with a Purchase Order reference.
- Purchase Order received quantity and status continue to be controlled by the existing Domain rules.
- Received quantity cannot exceed ordered quantity.
- Purchase Order state, Product stock, and the InventoryTransaction are persisted through the same Unit of Work save boundary.
- Existing receiving authorization and Presentation workflow remain unchanged.

The implementation does not redesign inventory behavior or introduce a separate receiving inventory architecture.

Runtime/browser verification was completed successfully by the project owner. Valid full and partial receiving updated Product stock correctly, corresponding StockIn movements were recorded, invalid and over-receiving scenarios were rejected without inventory changes, and existing authorization and receiving workflow behavior remained intact.

## Reporting

### Completed

- Inventory Valuation
- Purchase History
- Supplier Purchase Analysis
- Stock Movement
- Low Stock Report
- Inventory Movement Report
- Product Reports

### Purchase History

- Purchase History Report
- Server-side Search
- From/To Date Filtering
- Server-side Pagination
- Server-side Sorting

### Export

- ✅ Export to Excel
- ✅ Export to PDF

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

## Product Reports

Status: **Complete**

Implemented:

- Product Reports read model
- Product Reports application handler
- Product Reports persistence abstraction
- Product Reports repository
- Product/SKU search
- Category search
- Unit search
- Product status filtering
- Active / Inactive / All filtering
- Server-side sorting
- Server-side pagination
- Page-size changes
- Reset
- Combined filtering
- No-result behavior
- Reports navigation

Verification:

- Page loading
- Reports navigation
- Product/SKU search
- Category/Unit filtering
- Status filtering
- Combined filtering
- Sorting
- Reset
- Pagination
- Page-size changes
- Pagination while filters are active
- Boundary/no-result behavior
- Browser verification

## Excel Export

- ✅ Excel Export
- ✅ Export actions on completed reporting pages
- ✅ Existing report filters preserved during export
- ✅ Existing report sorting preserved during export
- ✅ Full filtered result set exported without UI pagination limits
- ✅ Inventory Valuation total inventory value included in the workbook
- ✅ Report-specific workbook columns and values
- ✅ Browser/manual verification

The Excel export uses the existing read-oriented Reporting queries and DTOs. No new Domain entities, database tables, or migrations were required.

## PDF Export

- ✅ PDF Export
- ✅ Export actions on completed reporting pages
- ✅ Existing report filters preserved during export
- ✅ Existing report sorting preserved during export
- ✅ Full filtered result set exported without UI pagination limits
- ✅ Inventory Valuation total inventory value included in the PDF
- ✅ Report-specific PDF columns and values
- ✅ Browser/manual verification

The PDF export uses the existing read-oriented Reporting queries and DTOs. PDF generation is isolated in the Web layer using QuestPDF. No new Domain entities, database tables, or migrations were required.

## Reporting Verification

Sprint 7 Additional Reporting has completed final project-wide verification.

Verified:

- All seven reporting pages
- All seven Excel exports
- All seven PDF exports
- Filtering, sorting, pagination, and navigation
- Full filtered dataset exports
- Multi-page PDF output
- Inventory Valuation Total Inventory Value
- Empty database behavior
- Explicit query failure and database recovery
- Existing authorization and Access Denied behavior

No Dynamic Capability-Based Authorization was introduced.

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

## Dynamic Capability-Based Authorization

Sprint 10 introduces a dynamic, database-backed capability-based authorization model.

Structure:

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

- T01-T13 Complete (source and runtime verified)
- T14 Documentation Synchronization (complete)
- T15 Final Verification & Retrospective — Complete

### Authorization Administration (T11)

**Status:** Complete

Minimum administration surface for managing dynamic authorization:

- ✅ Group Management (CRUD)
- ✅ Group Capability Assignment (checkbox-based)
- ✅ User Group Assignment (checkbox-based)
- ✅ Capability Catalog Display (read-only)
- ✅ Server-side Authorization Enforcement
- ✅ Delete Safety (refuse if users assigned)

Pages:

- `/Administrator/Groups` — List all groups
- `/Administrator/Groups/Create` — Create group
- `/Administrator/Groups/Edit/{id}` — Edit group name
- `/Administrator/Groups/Details/{id}` — View group details
- `/Administrator/Groups/EditCapabilities/{id}` — Manage capabilities
- `/Administrator/Groups/EditUsers/{id}` — Manage users
- `/Administrator/Capabilities` — View capability catalog

All admin pages require `Administration.Access` capability.

### Domain Model (T02)

- ✅ Capability entity (Name, IsEnabled, GroupCapabilities)
- ✅ AuthorizationGroup entity (Name, Capabilities, UserGroups)
- ✅ AuthorizationGroupCapability join entity
- ✅ UserAuthorizationGroup join entity
- ✅ Rich domain behavior (AddCapability, RemoveCapability, AssignUser, RemoveUser, Enable, Disable)

### Application Abstractions (T03)

- ✅ ICapabilityAuthorizationService (HasCapabilityAsync)
- ✅ ICapabilityRepository (GetByNameAsync)
- ✅ IAuthorizationGroupRepository (GetForUserAsync, GetWithCapabilitiesAsync, GetWithCapabilitiesAndUsersAsync, GetAllWithDetailsAsync)
- ✅ CapabilityAuthorizationService (union semantics, IsEnabled check)

### Persistence & Migration (T04–T06)

- ✅ EF Core configurations for 4 authorization tables
- ✅ CreateAuthorizationSchema migration
- ✅ Unique indexes on group name, capability name, and join-table composites
- ✅ Cascade delete on all relationships

### Seed Data (T05)

- ✅ 39 capabilities (Resource.Action pattern)
- ✅ 3 groups (Administrator, InventoryManager, Viewer)
- ✅ Capability-to-group assignments per group
- ✅ User-to-group assignments for 3 seeded users
- ✅ Additive-only, idempotent seeding

### Authorization Handlers (T08)

- ✅ CapabilityRequirement + CapabilityAuthorizationHandler (single capability)
- ✅ MultiCapabilityRequirement + MultiCapabilityAuthorizationHandler (OR-composite)
- ✅ CapabilityAuthorizationExtensions (AddCapabilityPolicy)
- ✅ Default deny on all failure paths

### Policy Migration (T10)

- ✅ Administrator policy → single cap Administration.Access
- ✅ InventoryManagement policy → OR-composite of 9 capabilities
- ✅ ViewInventory policy → OR-composite of 7 view capabilities
- ✅ All 43 page-level [Authorize(Policy)] attributes preserved unchanged

### Razor UI Visibility (T12)

- ✅ 45 User.IsInRole checks migrated to IAuthorizationService.AuthorizeAsync
- ✅ Pre-computed boolean variables for efficiency
- ✅ Zero IdentityConstants.Roles references in .cshtml files

### Runtime Verification (T13)

- ✅ Build: SUCCESS (0 errors, 0 warnings)
- ✅ Authentication: All 3 seeded users login
- ✅ Unauthenticated access: 302 redirect to login
- ✅ Administrator: All admin pages accessible (200)
- ✅ Manager: All management pages accessible (200)
- ✅ Viewer: View pages accessible (200), management denied (302)
- ✅ Purchasing: Per-action capabilities verified (View, Create, Submit, Approve, Receive)
- ✅ Reports: Accessible to all authenticated users (by design)
- ✅ Database: 39 capabilities, 3 groups, 3 assignments verified
- ✅ AccessDenied page renders correctly

### Known Findings (Deferred) — status synchronized at Sprint 13 closure (September 2026)

- InventoryManager group includes Administration.Access (seed data issue) — **RESOLVED**: stale database relationship removed via authorized Phase 25 remediation; runtime reverification confirmed 0 rows
- Categories/Edit missing [Authorize] attribute — **RESOLVED**: remediated in Sprint 12 (now requires the `InventoryManagement` capability policy)
- InventoryManagement OR-composite grants broad access via single capability — **deferred** (accepted policy design; single capability maps to a 9-capability OR-composite)
- Viewer has User.View capability (seed filter includes all *.View) — **deferred** (by design)
- Reports unrestricted (design decision pending) — **deferred** (owner decision pending)

Findings recorded after the list above was written: the Sprint 12 `EditStatus` `User.IsInRole(InventoryManager)` self-deactivation guard remains deferred pending an explicit behavioral decision; the Sprint 13 T05 investigation recorded that `GetPurchaseOrdersHandler` does not copy the generic `PagedRequest.Status` (`ProductStatusFilter`) into `PagedQuery` — it passes the Purchase Order-specific `PurchaseOrderStatus` separately to `GetPagedAsync` (finding only; no remediation authorized; re-verified against current source at Sprint 17 T09 closure). Sprint 13 itself made no production changes.


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

- Additional User Experience Improvements

## Reporting

Sprint 7 Additional Reporting is complete and verified.

Future reporting enhancements will be selected through the next Sprint Planning process.

## Sales

- Sales Orders
- Customer Invoicing
- Stock Reservation
- Sales History

## Final Project-wide Verification

Sprint 7 Additional Reporting has completed final project-wide verification.

Verified:

- Authentication, Account Management, and 2FA
- Product, Category, Supplier, and Customer management
- Purchase Orders and Inventory operations
- All seven reporting pages
- All seven Excel exports
- All seven PDF exports
- Filtering, sorting, pagination, and navigation
- Full filtered dataset exports
- Multi-page PDF output
- Inventory Valuation Total Inventory Value
- Empty database behavior
- Explicit query failure and database recovery
- Existing authorization boundaries

No Dynamic Capability-Based Authorization was introduced.

### Purchase Order Search - P2

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

### Purchase Order Filtering - P3

**Status: Complete and verified**

P3 adds confirmed server-side Purchase Order filters:

- From Date
- To Date
- Purchase Order Status

Verified behavior:
- Individual filters work correctly.
- Multiple filters can be combined.
- Existing Purchase Order search works together with filtering.
- Empty filter combinations return the existing no-results state correctly.
- Filter state is preserved where applicable.
- Existing authorization and unrelated Purchase Order behavior remain unchanged.

Runtime/browser verification was completed successfully by the project owner.

### Purchase Order Sorting - P4

**Status: Complete and verified**

P4 adds server-side Purchase Order sorting using the established shared sorting conventions.

Supported sort fields:
- Purchase Order ID
- Supplier
- Order Date
- Status
- Total Amount

Verified behavior:
- Ascending and descending sorting work for all supported fields.
- Sorting integrates with Purchase Order search and filtering.
- Sorting state is preserved through applicable Purchase Order navigation and workflow actions.
- Sorting is executed server-side.
- Existing authorization and unrelated Purchase Order behavior remain unchanged.

Runtime/browser verification was completed successfully by the project owner.

P6 - Inventory Synchronization During Receiving is complete and verified.

### Integrated Purchasing Verification - P7

**Status: Complete and verified**

P7 completed the integrated Purchasing regression pass across the full workflow from Create through Receive.

Verified areas include:
- Multiple Purchase Order items
- List, search, date/status filtering, sorting, and pagination
- Details, Submit, Approve, and Receive
- Inventory synchronization during receiving
- Existing authorization boundaries
- Empty-result behavior
- Relevant failure/recovery behavior

During verification, an in-scope pagination regression was found: pagination links did not preserve `FromDate` and `ToDate`. The Purchase Order listing was corrected so pagination preserves the active date filters together with search, status, page size, and sorting state. The corrected behavior was runtime/browser tested successfully by the project owner.

No Dynamic Capability-Based Authorization implementation was introduced.

Sprint 8 is closed and released as v1.5.0. Sprint 9 code-quality and consistency work is the current development workstream.

### Purchase Order Pagination - P5

P5 adds server-side pagination to the Purchase Order listing using the established shared paging infrastructure.

- Page parameter convention: `PageNum`
- Page size convention: `PageSize`
- Previous / numbered-page / Next navigation
- First and last page boundary handling
- Pagination compatible with search
- Pagination compatible with date/status filtering
- Pagination compatible with sorting
- Pagination state preservation across navigation
- Empty-result behavior remains within the existing Purchase Order listing flow

The pagination implementation is server-side and preserves the existing Purchase Order query architecture.
