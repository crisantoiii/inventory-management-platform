# Changelog

## [v1.3.0] - 2026-08-13

### Release Summary

This release completes the Account Management vertical slice, providing authenticated users with self-service account management capabilities while preserving the separation between administrative User Management and authentication workflows.

The implementation extends the existing ASP.NET Core Identity integration through the established Identity abstraction, Application handler patterns, and Razor Pages architecture without requiring structural architectural redesign.

---

### Added

#### Account Management

- User Profile
- Update Profile
- Phone Number Update
- Change Password
- Forgot Password
- Reset Password
- Force Password Change

#### Email Verification

- Request Email Verification
- Email Verification
- Email Confirmation
- Email verification status in the user profile

#### Two-Factor Authentication

- Two-Factor Authentication setup
- Authenticator-based TOTP verification
- 2FA login challenge
- Recovery codes
- Recovery code authentication
- Recovery code regeneration
- Recovery code invalidation
- 2FA disablement

#### Account Management Navigation

- Account Management navigation entry
- Two-Factor Authentication navigation entry

---

### Changed

- Separated self-service Account Management from administrative User Management.
- Updated new user creation so email confirmation defaults to unverified.
- Extended the existing Identity service abstraction to support account-management workflows.
- Integrated email verification with the existing development email service.
- Integrated Two-Factor Authentication with the existing ASP.NET Core Identity infrastructure.
- Added authentication challenge handling for users with 2FA enabled.
- Preserved existing Application handler and Razor Pages patterns.

---

### Improved

- Added self-service profile management for authenticated users.
- Added password management and recovery workflows.
- Added email ownership verification.
- Added authenticator-based two-factor authentication.
- Added recovery-code authentication for users without access to their authenticator.
- Added recovery-code regeneration with invalidation of previously generated codes.
- Added account security management without exposing administrative User Management functionality.
- Preserved Clean Architecture and feature-first organization.
- Preserved the separation between Account Management configuration and authentication enforcement.

---

### Validated

#### Profile and Password Management

- Profile display and update
- Phone number update
- Blank phone number handling
- Password change
- Forgot password
- Password reset
- Forced password change

#### Email Verification

- Verification request
- Verification token generation
- Email confirmation
- Already-verified handling
- Verification state displayed in Profile

#### Two-Factor Authentication

- 2FA setup
- Authenticator-code verification
- 2FA login challenge
- Recovery-code login
- Recovery-code regeneration
- Recovery-code invalidation
- 2FA disablement

#### Regression Validation

- Existing administrative User Management workflows
- Authentication and authorization behavior
- Account Management navigation
- Solution build
- Browser-based workflows

---

### Technical Findings

- Existing ASP.NET Core Identity infrastructure was sufficient for Two-Factor Authentication without introducing custom authentication infrastructure.
- The existing Identity abstraction successfully supports both administrative User Management and self-service Account Management.
- Account Management and authentication enforcement remain separate workflows.
- Recovery-code lifecycle management requires explicit handling of generation, single-use authentication, regeneration, and invalidation.
- The completed Account Management vertical slice required no structural architectural redesign.

---

### Documentation

- Updated `PROJECT_STATUS.md`
- Updated `FEATURES.md`
- Updated `ROADMAP.md`
- Updated `README.md`
- Updated `DESIGN_DECISIONS.md`
- Updated `ENGINEERING_JOURNAL.md`
- Updated `ARCHITECTURE_REVIEW.md`
- Updated Sprint 6 Account Management documentation

---

### Outcome

Account Management is now a completed v1.3.0 milestone.

The platform now provides authenticated users with self-service profile, password, email verification, and two-factor authentication capabilities while preserving the existing Clean Architecture, Vertical Slice Architecture, Identity abstraction, Application handler patterns, and Razor Pages workflows.

The release was validated through repeated solution builds and actual browser workflows without requiring structural architectural redesign.

---

## [v1.2.0] - 2026-08-09

### Release Summary

This release introduces the first Reporting vertical slice through the Inventory Valuation report.

The report provides a read-only view of current inventory valuation using the existing inventory valuation definition:

```text
Inventory Value
= Σ (QuantityOnHand × CostPrice)
```

The implementation extends the existing Dashboard read-model approach into a dedicated Reporting feature without requiring architectural redesign.

---

## Added

### Inventory Valuation Report

- Inventory Valuation Razor Page
- Inventory Valuation read model
- Inventory Valuation application request
- Inventory Valuation application handler
- Inventory Valuation persistence abstraction
- Inventory Valuation repository
- Product-level inventory valuation
- Category information
- Quantity On Hand display
- Cost Price display
- Inventory Value display
- Total Inventory Value
- Inventory Valuation navigation entry

### Reporting Read Model

- InventoryValuationDto
- Read-only EF Core projection
- AsNoTracking() query

---

## Changed

- Extended the Application layer with the Reporting feature.
- Added `IInventoryValuationRepository` following the existing repository pattern.
- Added `InventoryValuationRepository` to the Infrastructure layer.
- Added Inventory Valuation presentation under Razor Pages.
- Added Inventory Valuation to the Operations navigation.
- Reused the existing Dashboard inventory valuation definition.

---

## Improved
- Added a dedicated browser-accessible Inventory Valuation report.
- Preserved separation between Presentation, Application, Domain, and Infrastructure layers.
- Kept Reporting read-only and separate from transactional Domain workflows.
- Used EF Core projection to retrieve only the data required by the report.
- Kept inventory valuation calculation database-side.
- Reused the existing Result<T> application pattern.

---

## Validated

### Inventory Valuation

```text
QuantityOnHand × CostPrice
```

Each product's inventory value was verified against the expected calculation.

### Total Inventory Value

```text
Total Inventory Value
=
Σ Product Inventory Value
```

The report total was verified against the existing Dashboard Inventory Value.

### Browser Verification

- Inventory Valuation navigation
- Inventory Valuation page loading
- Product data retrieval
- Category projection
- Quantity On Hand display
- Cost Price display
- Individual Inventory Value calculation
- Total Inventory Value calculation
- Dashboard/report total consistency
- Existing application functionality

### EF Core Query Translation

The initial query attempted to order the projected DTO:

```text
Projection
    ↓
OrderBy(DTO.ProductName)
```

EF Core could not translate this expression.

The query was changed to:

```text
Products
    ↓
OrderBy(Product.Name)
    ↓
Projection
    ↓
InventoryValuationDto
```

This kept the query fully database-side without introducing client-side evaluation.

---

## Technical Findings

The following items remain intentionally deferred:

- Empty database behavior verification
- Explicit query-failure testing for the Reporting feature
- Excel export
- PDF export
- Purchase History report
- Supplier Purchase Analysis
- Stock Movement report
- Low Stock report
- Inventory Movement report
- Advanced report filtering
- Advanced report sorting
- Report scheduling
- Generic reporting framework

These are not considered completed features of this release.

---

## Documentation

- Added SPRINT_05_APPLICATION.md
- Updated Reporting architecture documentation
- Updated project status documentation
- Updated roadmap documentation
- Updated feature documentation
- Updated engineering journal
- Updated design decisions where required

---

## Outcome

The Reporting module now has its first usable vertical slice:

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

Inventory Valuation is available as a browser-accessible report backed by actual persisted database data.

The implementation validates that the existing architecture can support read-oriented Reporting capabilities alongside the transactional workflows introduced by the Purchasing module.

The first Reporting slice was implemented without requiring structural architectural redesign.

---

## [v1.1.0] - 2026-08-08

### Release Summary

This release completes the Presentation layer for the Purchasing module, connecting the existing Purchasing Application use cases to a usable Razor Pages workflow.

The release delivers a complete Purchase Order vertical slice from creation through submission, approval, partial receiving, and final completion using actual persisted database records.

The implementation preserves the existing Clean Architecture, Rich Domain Model, Vertical Slice Architecture, and business-oriented Application handler patterns established in previous sprints.

---

### Added

#### Purchasing Presentation Layer

- Purchase Order Index page
- Create Purchase Order page
- Purchase Order Details page
- Supplier selection
- Product selection
- Purchase Order item input
- Expected delivery date
- Purchase Order remarks
- Purchase Order status display
- Ordered quantity display
- Received quantity display
- Remaining quantity display
- Calculated Purchase Order total display

#### Purchase Order Workflow

- Submit Purchase Order action
- Approve Purchase Order action
- Receive Purchase Order action
- Partial Purchase Order receiving
- Final receiving and Completed state
- Fully Received item indication

#### Presentation Validation and Feedback

- Client-side Receive quantity validation
- Validation summaries
- Success messages using `TempData`
- Index query failure feedback
- Supplier query failure feedback
- Product query failure feedback

---

### Changed

- Registered Purchasing Application handlers required by the Presentation layer.
- Connected Razor PageModels to the existing Purchasing Application use cases through dependency injection.
- Updated Purchase Order repository queries to load Purchase Order items required for calculated totals.
- Improved Purchase Order Index total calculation.
- Added Presentation-layer handling for Application query failures.
- Added Supplier and Product lookup failure handling during Purchase Order creation.
- Added success feedback after successful Purchase Order operations.

---

### Improved

- Completed the Purchasing workflow as a browser-accessible vertical slice.
- Preserved the separation between Presentation, Application, Domain, and Infrastructure layers.
- Kept business workflow rules inside the `PurchaseOrder` aggregate.
- Improved Purchase Order receiving usability through item-level quantity and remaining-quantity display.
- Improved user feedback for successful and failed operations.
- Validated client-side and Domain-level receiving rules.
- Reused existing Application handlers, Repository, Unit of Work, and Result pattern infrastructure.

---

### Validated

#### Purchase Order Lifecycle

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

### End-to-End Verification

- Purchase Order creation
- Purchase Order listing
- Purchase Order details
- Purchase Order submission
- Purchase Order approval
- Partial receiving
- Final receiving
- Completed status
- Remaining quantity calculation
- Fully Received state
- Calculated Purchase Order total
- Client-side validation
- Domain validation
- Success feedback
- Query failure feedback

The workflow was verified using actual database records rather than seed data.

### Technical Findings

The following items were identified during Sprint 4 but intentionally deferred:

- Cross-cutting DomainException-to-Result/error handling strategy
- Inventory synchronization during Purchase Order receiving
- Multiple Purchase Order item management in the Create UI
- Purchase Order search, filtering, sorting, and pagination
- Additional Product identification information such as SKU in selection controls

These are documented as technical debt or future enhancements and are not considered completed features of this release.

### Documentation
- Added SPRINT_04_APPLICATION.md
- Updated ARCHITECTURE.md
- Updated DESIGN_DECISIONS.md
- Updated ENGINEERING_JOURNAL.md
- Updated FEATURES.md
- Updated PROJECT_STATUS.md
- Updated README.md

### Outcome

The Purchasing module is now a complete browser-accessible vertical slice spanning:

```text
Presentation
     ↓
Application
     ↓
Domain
     ↓
Infrastructure
     ↓
Database
```

The release demonstrates that the architecture can support a workflow-driven business module from Domain and Application implementation through a usable Presentation layer without requiring structural redesign.

The Purchasing module has progressed from an Application-layer capability in v1.0.0 to an end-to-end browser-accessible workflow in v1.1.0.

## [v1.0.0] - 2026-08-07

### Release Summary

This release introduces the Application layer for the Purchasing module, extending the platform beyond CRUD-oriented business modules into workflow-driven business processes.

The Purchasing module demonstrates the successful application of the Rich Domain Model, Vertical Slice Architecture, and business-oriented application handlers without requiring changes to the existing Clean Architecture.

---

### Added

#### Purchasing Module

- Create Purchase Order
- Get Purchase Order
- Get Purchase Orders
- Submit Purchase Order
- Approve Purchase Order
- Receive Purchase Order

#### Application Layer

- Purchase Order command handlers
- Purchase Order query handlers
- Request / Response models
- Purchasing error definitions
- Repository integration for Purchasing workflows

---

### Changed

- Extended the Application layer with workflow-oriented business capabilities.
- Introduced dedicated read models for Purchasing queries.
- Standardized Purchasing handlers using the established Request / Response / Handler pattern.
- Expanded the Domain-driven workflow through the Application layer without modifying architectural boundaries.

---

### Improved

- Preserved thin Application handlers by delegating business behavior to the PurchaseOrder aggregate.
- Maintained consistent Vertical Slice Architecture across all Purchasing features.
- Reused existing Repository, Unit of Work, and Result pattern infrastructure.
- Validated the architecture's ability to support workflow-driven business modules.

---

### Documentation

- Added SPRINT_03_APPLICATION.md
- Updated ARCHITECTURE
- Updated DESIGN_DECISIONS
- Updated ENGINEERING_JOURNAL
- Updated FEATURES
- Updated PROJECT_STATUS

---

### Outcome

The Purchasing module became the first workflow-oriented business module within the platform.

This release validates that the existing architecture successfully scales from CRUD-based modules to aggregate-driven business workflows while preserving Clean Architecture principles.

---

## [v0.9.0] - 2026-08-04

### Release Summary

This release completes **Architecture Sprint 1**, a comprehensive review of the Inventory Management Platform architecture across the Application, Infrastructure, and Web layers.

The objective of this milestone was to validate the existing Clean Architecture implementation before introducing larger business workflows. The review confirmed that the architecture scales successfully and does not require structural redesign prior to implementing the Purchasing module.

---

### Reviewed

#### Application Layer

- Reviewed feature-first organization
- Reviewed request and response models
- Reviewed validators
- Reviewed Create, Get, Update, Activate, and Deactivate handlers
- Reviewed Identity handlers
- Validated handler consistency and application boundaries

#### Infrastructure Layer

- Reviewed dependency injection
- Reviewed ApplicationDbContext
- Reviewed generic repository
- Reviewed feature repositories
- Reviewed IdentityService
- Reviewed Unit of Work
- Reviewed Entity Framework Core configurations

#### Web Layer

- Reviewed shared layout
- Reviewed navigation
- Reviewed Users module
- Reviewed Products module
- Reviewed Categories module
- Reviewed shared Razor Pages patterns

---

### Changed

- Improved naming consistency across the solution
- Improved IdentityService result handling
- Improved handler consistency
- Standardized architecture documentation
- Updated project documentation for Architecture Sprint 1

---

### Validated

- Clean Architecture
- Feature-first organization
- Repository Pattern
- Unit of Work
- Result Pattern
- ASP.NET Core Identity isolation
- Razor Pages architecture
- Shared paging, filtering, and sorting infrastructure

---

### Documentation

- Updated README
- Updated PROJECT_STATUS
- Updated ROADMAP
- Updated ARCHITECTURE
- Updated ENGINEERING_JOURNAL
- Updated DESIGN_DECISIONS
- Updated FEATURES

---

### Outcome

Architecture Sprint 1 concluded that the existing architecture remains stable, maintainable, and suitable for future expansion.

The project is now ready to begin implementation of the **Purchasing Module (v1.0.0)**.

All notable changes to this project will be documented in this file.

---

## [v0.8.0] - 2026-08-01

### Release Summary

This release introduces a complete Identity and User Management subsystem built on ASP.NET Core Identity. Authentication, authorization, and administrative user management are now fully integrated into the existing Clean Architecture while preserving separation of concerns through the `IIdentityService` abstraction.

### Added

#### Authentication

- ASP.NET Core Identity integration
- Cookie authentication
- Login page
- Logout functionality
- Role-based authorization
- Policy-based authorization
- Identity service abstraction

#### User Management

- User listing page
- User details page
- Create user page
- Edit user page
- User role management
- User activation
- User deactivation
- Password reset
- User search
- Server-side pagination
- Server-side sorting
- Status filtering

#### Identity Infrastructure

- ApplicationUser entity
- Identity database integration
- Identity service implementation
- Role initialization
- Authorization policies
- Identity dependency registration

### Changed

- Extended the Clean Architecture to support ASP.NET Core Identity.
- Added `IIdentityService` abstraction to isolate Identity framework APIs.
- Updated the Infrastructure layer to encapsulate user and role management.
- Added administrator-only Razor Pages for user administration.
- Improved project documentation to include authentication, identity, and user management architecture.

### Improved

- Standardized user management workflows with existing application patterns.
- Reused shared paging, filtering, sorting, and Result pattern infrastructure.
- Improved security by encapsulating framework-specific functionality behind application abstractions.
- Maintained consistent feature-first organization across Identity and business modules.
- Preserved Clean Architecture boundaries while integrating authentication and authorization.

---

## [v0.7.0] - 2026-07-27

### Added

#### Dashboard Module

- Dashboard overview page
- Dashboard statistics cards
- Recent inventory transactions widget
- Low stock products widget
- Inventory value summary
- Dashboard refresh action

#### Dashboard Statistics

- Total Products
- Active Products
- Inactive Products
- Low Stock Products
- Out of Stock Products
- Total Inventory Value

#### Dashboard Reporting

- Recent inventory transactions
- Low stock product monitoring
- Read-only dashboard projections
- Empty state handling for dashboard widgets

### Changed

- Added Dashboard feature to the application navigation.
- Extended the application layer with dashboard queries and handlers.
- Added dashboard repository for read-only reporting.
- Introduced dashboard DTOs for statistics and widget data.
- Improved project documentation to reflect the completed Dashboard module.

### Improved

- Added responsive dashboard layout using Bootstrap cards.
- Improved visibility of inventory metrics through KPI cards.
- Enhanced dashboard usability with transaction badges and low stock indicators.
- Formatted inventory value for improved readability.
- Added user-friendly empty state messages when dashboard widgets contain no data.

---

## [v0.6.0] - 2026-07-25

### Added

#### Product Module
- Product search
- Server-side pagination
- Server-side sorting
- Product status filtering
- Product activation
- Product deactivation
- Product details page
- Product create page
- Product edit page
- Product barcode support
- Product category relationship
- Product unit relationship
- Product quantity tracking

#### Category Module
- Category search
- Server-side pagination
- Server-side sorting
- Category status filtering
- Category activation
- Category deactivation
- Category details page
- Category create page
- Category edit page

#### Supplier Module
- Supplier search
- Server-side pagination
- Server-side sorting
- Supplier status filtering
- Supplier activation
- Supplier deactivation
- Supplier details page
- Supplier create page
- Supplier edit page

#### Customer Module
- Customer search
- Server-side pagination
- Server-side sorting
- Customer status filtering
- Customer activation
- Customer deactivation
- Customer details page
- Customer create page
- Customer edit page

#### Unit Module
- Unit search
- Server-side pagination
- Server-side sorting
- Unit status filtering
- Unit activation
- Unit deactivation
- Unit details page
- Unit create page
- Unit edit page

#### Inventory Transactions Module
- Inventory transaction management
- Inventory transaction details page
- Inventory transaction create page
- Inventory transaction listing page
- Stock In transactions
- Stock Out transactions
- Stock Adjustment transactions
- Product selection dropdown
- Transaction type selection
- Transaction reference number
- Transaction remarks
- Transaction date tracking

#### Inventory Workflow
- Automatic Quantity On Hand updates
- Immutable inventory transaction history
- Inventory movement audit trail
- Product inventory validation

#### Shared Infrastructure
- Reusable paging infrastructure
  - PagedRequest
  - PagedQuery
  - PagedResult<T>
- Shared status filter enum
- Shared product sort field definitions
- Shared category sort field definitions
- Shared supplier sort field definitions
- Shared customer sort field definitions
- Shared unit sort field definitions
- Shared inventory transaction sort field definitions

### Changed

- Refactored product repository to support reusable filtering, sorting and paging.
- Refactored product listing to use server-side search.
- Refactored product queries to use reusable paging models.
- Implemented Customer module using the established Product, Category, and Supplier architecture.
- Product now references Category
- Product now references Unit
- Product no longer stores Unit as string
- Product pages updated to use dropdowns
- Product repository updated to load Category and Unit
- Product inventory is now maintained through inventory transactions.
- Product stock updates are handled through domain methods (`IncreaseStock`, `DecreaseStock`, and `AdjustStock`).
- Inventory movements are persisted as historical records instead of directly modifying product quantities.


### Improved

- Product listing preserves filter state across pagination.
- Product listing preserves sorting state.
- Product activation workflow.
- Product deactivation workflow.
- Category listing preserves filter state across pagination.
- Category listing preserves sorting state.
- Category activation workflow.
- Category deactivation workflow.
- Supplier listing preserves filter state across pagination.
- Supplier listing preserves sorting state.
- Supplier activation workflow.
- Supplier deactivation workflow.
- Customer listing preserves filter state across pagination.
- Customer listing preserves sorting state.
- Customer activation workflow.
- Customer deactivation workflow.
- Unit listing preserves filter state across pagination.
- Unit listing preserves sorting state.
- Unit activation workflow.
- Unit deactivation workflow.
- Stronger inventory domain model.
- Foundation prepared for Inventory Transactions.
- Added server-side search for inventory transactions.
- Added server-side sorting for inventory transactions.
- Added server-side pagination for inventory transactions.
- Added Bootstrap badges for transaction types.
- Improved quantity display using positive and negative values.
- Added success notifications after transaction creation.
- Improved inventory transaction user experience with consistent Razor Pages UI.