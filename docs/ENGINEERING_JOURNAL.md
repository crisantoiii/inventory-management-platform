# Engineering Journal

## Contents

- Milestone Timeline
- Foundation
- Business Modules
- Reporting
- Identity
- Architecture Sprint 1
- Reflection
- Architecture Validation
- Engineering Principles
- Engineering Philosophy
- Future Milestones

## Overview

This journal records significant engineering milestones throughout the development of the Inventory Management Platform.

Rather than documenting daily work, it captures important architectural decisions, major feature implementations, refactorings, lessons learned, and the evolution of the codebase.

---

# Milestone Timeline

| Milestone | Focus |
|-----------|-------|
| 1 | Solution Setup |
| 2 | EF Core |
| 3 | Product |
| 4 | Shared Paging |
| 5 | Searching |
| 6 | Sorting |
| 7 | Filtering |
| 8 | Product Completion |
| 9 | Category |
| 10 | Supplier |
| 11 | Customer |
| 12 | Product Foundation |
| 13 | Inventory Transactions |
| 14 | Dashboard |
| 15 | Authentication & Authorization |
| 16 | User Management |
| 17 | Architecture Sprint 1 |
| 18 | Purchasing Application Layer |
| 19 | Purchasing Presentation Layer |
| 20 | Reporting: Inventory Valuation |
| 21 | Account Management |
| 22 | Dynamic Authorization Architecture Decision |
| 23 | Reporting: Purchase History |

---

# Milestone 1 - Project Initialization

## Summary

Created the initial solution structure following Clean Architecture.

Projects:

- InventoryPlatform.Web
- InventoryPlatform.Application
- InventoryPlatform.Domain
- InventoryPlatform.Infrastructure
- InventoryPlatform.Shared

### Outcome

Established a modular architecture with clearly defined responsibilities.

---

# Milestone 2 - Entity Framework Core Setup

## Summary

Configured Entity Framework Core and SQL Server.

Completed:

- DbContext
- Initial Migration
- Database Creation
- Dependency Injection
- Repository Registration

### Lessons Learned

- Keep EF Core confined to the Infrastructure layer.
- Avoid leaking persistence concerns into Application.

---

# Milestone 3 - Product Module

## Summary

Implemented the first complete business module.

Completed:

- Product CRUD
- Product Details
- Product Activation
- Product Deactivation

### Outcome

Validated the overall Clean Architecture design.

---

# Milestone 4 - Shared Paging Infrastructure

## Summary

Initially implemented paging specifically for Products.

After validating the implementation, paging was extracted into reusable infrastructure.

Introduced:

- `PagedRequest`
- `PagedQuery`
- `PagedResult<T>`

### Lesson Learned

Build for one feature first.

Generalize only after the implementation has proven to be reusable.

---

# Milestone 5 - Server-side Searching

## Summary

Moved searching into SQL queries instead of filtering in memory.

Benefits:

- Better scalability
- Reduced memory usage
- Improved response time

### Lesson Learned

Filtering should occur as close to the database as possible.

---

# Milestone 6 - Server-side Sorting

## Summary

Implemented reusable sorting infrastructure.

Initially, sort definitions were implemented for the Product module before being generalized into shared infrastructure.

Later refactored to:

`InventoryPlatform.Shared.Sorting`

### Reason

Sorting definitions are shared between:

- Web
- Application
- Infrastructure

Moving them into Shared removed unnecessary project dependencies.

### Lesson Learned

Shared metadata belongs in the Shared project, not in a feature-specific layer.

---

# Milestone 7 - Status Filtering

## Summary

Implemented reusable status filtering.

Added:

- Shared status filtering infrastructure
- Active
- Inactive
- All

Repository pipeline became:

```text
Status

↓

Search

↓

Count

↓

Sort

↓

Paging
```
### Lesson Learned

Applying filters before sorting and paging results in a cleaner and more efficient query pipeline.

---

# Milestone 8 - Product Lifecycle

## Summary

Completed the Product module.

Implemented:

- Create
- Details
- Edit
- Activate
- Deactivate
- Search
- Pagination
- Sorting
- Status Filtering

### Outcome

The Product module became the reference implementation for future modules.

Future modules should reuse the shared infrastructure rather than introducing module-specific implementations.

---

# Milestone 9 - Category Module

## Summary

Implemented the second complete business module by reusing the established Product module architecture.

Completed:

- Category CRUD
- Category Details
- Category Activation
- Category Deactivation
- Server-side Search
- Server-side Pagination
- Server-side Sorting
- Status Filtering

### Outcome

Validated that the shared paging, filtering, sorting, repository, and Result pattern infrastructure could be reused without architectural changes.

### Lesson Learned

Reusable infrastructure should be extracted only after proving its value through a real implementation.

---

# Milestone 10 - Supplier Module

## Summary

Implemented the third complete business module using the established application architecture.

Completed:

- Supplier CRUD
- Supplier Details
- Supplier Activation
- Supplier Deactivation
- Server-side Search
- Server-side Pagination
- Server-side Sorting
- Status Filtering

### Outcome

Confirmed that the architecture scales across multiple business domains while maintaining consistent implementation patterns.

### Lesson Learned

Consistency across modules improves maintainability, readability, and development speed more than introducing module-specific abstractions.

---

# Milestone 11 - Customer Module

## Summary

Implemented the fourth complete business module by reusing the established architecture and shared infrastructure.

Completed:

- Customer CRUD
- Customer Details
- Customer Activation
- Customer Deactivation
- Server-side Search
- Server-side Pagination
- Server-side Sorting
- Status Filtering

### Outcome

Demonstrated that the architecture supports rapid development of new business modules with minimal code duplication while maintaining consistent behavior and user experience.

### Lesson Learned

A well-designed shared infrastructure enables feature development to focus on business logic rather than rebuilding common functionality.

---

# Milestone 12 - Product Foundation Improvements

## Summary

Expanded the Product domain model to support future inventory operations by introducing normalized relationships and inventory-specific attributes.

Completed:

- Unit Management
- Product-Category relationship
- Product-Unit relationship
- Barcode
- QuantityOnHand

### Outcome

Product evolved from a standalone CRUD entity into the central aggregate root for future inventory operations.

### Lesson Learned

Establish a complete domain model before implementing transactional workflows. A stable aggregate reduces rework and keeps future features focused on business behavior rather than structural changes.

---

# Milestone 13 - Inventory Transactions

## Summary

Implemented the first transactional business module responsible for recording inventory movements and maintaining product stock levels.

Completed:

- Inventory Transaction entity
- Inventory Transaction repository
- Stock In workflow
- Stock Out workflow
- Stock Adjustment workflow
- Transaction Details
- Transaction Listing
- Server-side Search
- Server-side Pagination
- Server-side Sorting

### Outcome

Successfully extended the existing architecture from master data management to transactional workflows without requiring structural changes.

The Product entity now serves as the aggregate root for inventory operations while InventoryTransaction provides an immutable history of all inventory movements.

### Lessons Learned

- Domain behavior should remain inside domain entities.
- Historical business events should be immutable.
- Existing shared infrastructure significantly reduced development effort.
- Reusing proven architectural patterns made implementing a new business module straightforward.

---

# Milestone 14 - Dashboard

## Summary

Implemented the first reporting module by introducing a centralized dashboard that aggregates inventory statistics and operational insights.

Completed:

- Dashboard overview
- Inventory statistics
- Inventory value summary
- Recent inventory transactions
- Low stock products
- Read-only dashboard projections
- Responsive dashboard layout

### Outcome

Successfully extended the architecture to support reporting scenarios without introducing new architectural layers or modifying existing domain workflows.

The Dashboard demonstrates that the same Clean Architecture can support both transactional business operations and read-only reporting through dedicated DTO projections and repository queries.

### Lessons Learned

- Reporting requirements differ from transactional workflows.
- Read-only DTO projections improve performance and reduce coupling.
- Existing application and repository patterns were reusable for reporting features.
- Consistent architectural patterns simplify the addition of new modules.

---

# Milestone 15 - Authentication & Authorization

## Summary

Integrated ASP.NET Core Identity into the existing Clean Architecture without introducing dependencies from the Application or Web layers to Identity framework types.

Completed:

- ASP.NET Core Identity
- Cookie Authentication
- Login
- Logout
- Role-based Authorization
- Policy-based Authorization
- Identity Service abstraction

## Outcome

Successfully incorporated authentication and authorization into the existing architecture while preserving separation of concerns.

Identity framework components remain encapsulated within the Infrastructure layer behind `IIdentityService`.

## Lessons Learned

- Framework-specific APIs should remain behind application abstractions.
- Authentication is an infrastructure concern rather than business logic.
- Encapsulation allows Identity to evolve independently from the rest of the application.

---

# Milestone 16 - User Management

## Summary

Implemented a complete administrative user management module using the existing application architecture and Identity service abstraction.

Completed:

- User Listing
- User Details
- Create User
- Edit User
- Assign Roles
- Activate User
- Deactivate User
- Reset Password
- Search
- Pagination
- Sorting
- Status Filtering

## Outcome

Validated that the same architectural patterns used for business modules could also support security and identity management without structural changes.

The Identity module became another feature within the application rather than a special-case implementation.

## Lessons Learned

- Identity operations belong behind an application service rather than inside Razor Pages.
- Administrative workflows should remain independent from end-user account management.
- Existing paging, sorting, filtering, Result, and handler patterns were reusable without modification.

---

# Milestone 17 - Architecture Sprint 1

## Summary

Completed a comprehensive architectural review of the Inventory Management Platform after implementing the foundational business modules, reporting features, authentication, and administrative user management.

The objective of this milestone was to validate the architecture before introducing larger workflow-driven business modules such as Purchasing.

The review covered:

- Application Layer
- Infrastructure Layer
- Web Layer
- Shared Infrastructure
- Documentation

## Completed

### Application Layer

- Reviewed feature organization
- Reviewed request and response models
- Reviewed validators
- Reviewed application handlers
- Validated feature-first organization

### Infrastructure Layer

- Reviewed dependency injection
- Reviewed ApplicationDbContext
- Reviewed generic repository
- Reviewed feature repositories
- Reviewed IdentityService
- Reviewed Unit of Work
- Reviewed Entity Framework configurations

### Web Layer

- Reviewed Razor Pages organization
- Reviewed Users module
- Reviewed Products module
- Reviewed Categories module
- Reviewed shared layout
- Reviewed navigation
- Reviewed reusable UI patterns

### Documentation

- Updated README
- Updated PROJECT_STATUS
- Updated CHANGELOG
- Updated ROADMAP
- Updated Architecture documentation

## Outcome

Architecture Sprint 1 confirmed that the existing Clean Architecture has successfully scaled across:

- Master Data
- Transactional Workflows
- Reporting
- Authentication
- User Management

without requiring structural redesign.

The review concluded that the project is ready to transition from architectural foundation work to business workflow implementation.

## Lessons Learned

- Well-defined architectural boundaries reduce long-term maintenance costs.
- Consistent implementation patterns are more valuable than introducing additional abstractions.
- Architecture should be validated before expanding into larger business domains.
- The Rule of Three remains an effective guideline for introducing shared infrastructure.
- Stable architecture accelerates future feature development.


---

## Reflection

The Inventory Transactions milestone confirmed that the shared architecture was flexible enough to support transactional business logic without introducing new architectural patterns.

The Dashboard milestone further demonstrated that the same architecture could support read-optimized reporting through dedicated DTO projections and repository queries while maintaining a clear separation between reporting and transactional workflows.

The Authentication and User Management milestones extended this validation into the security domain. By encapsulating ASP.NET Core Identity behind `IIdentityService`, authentication, authorization, and administrative user management were integrated without exposing framework-specific APIs to the Application or Presentation layers.

Across master data management, transactional workflows, reporting, and identity management, the same architectural principles remained consistent. Existing application handlers, shared paging, filtering, sorting infrastructure, and the Result pattern were reused without structural changes, allowing development to focus on business requirements rather than framework concerns.

Together, these milestones demonstrate that the architecture successfully supports:

- Master data management
- Transactional workflows
- Reporting
- Authentication
- Administrative user management
- Self-service account management
- Email verification
- Two-factor authentication

The Account Management milestone further validated that security-sensitive self-service workflows can be introduced using the existing Identity abstraction and Application handler patterns without requiring structural architectural redesign.

The implementation also reinforced the separation between administrative User Management, self-service Account Management, and authentication enforcement. This separation provides clearer authorization boundaries while allowing each workflow to evolve independently.

Architecture Sprint 1 provided an opportunity to validate these assumptions through a comprehensive review of the Application, Infrastructure, and Web layers. Rather than identifying major redesigns, the review confirmed that the existing architectural decisions remained consistent and scalable across all implemented modules.

This milestone represents the continued evolution of the platform from its foundational business modules into a broader set of validated capabilities, including workflow-driven business processes, read-oriented reporting, administrative identity management, and self-service account security.

Future development will focus primarily on expanding business capabilities such as Sales and additional Reporting features while preserving the validated architectural principles established throughout the project's development.

---

# Milestone 18 - Purchasing Application Layer

## Summary

Implemented the Application layer for the Purchasing module by exposing the PurchaseOrder aggregate through business-oriented use cases while preserving the Rich Domain Model established during previous sprints.

Completed:

### Commands

- Create Purchase Order
- Submit Purchase Order
- Approve Purchase Order
- Receive Purchase Order

### Queries

- Get Purchase Order
- Get Purchase Orders

### Supporting Components

- Request / Response models
- Application handlers
- Repository integration
- Purchasing error definitions

---

## Outcome

The Purchasing module became the first workflow-driven business module within the Inventory Management Platform.

Unlike previous CRUD-oriented modules, Purchasing introduced explicit business workflows while preserving the existing Clean Architecture.

Application handlers remained intentionally small by delegating business behavior to the PurchaseOrder aggregate.

The successful implementation confirmed that the architecture scales naturally from CRUD operations to workflow-oriented business processes without requiring structural redesign.

---

## Lessons Learned

- Rich Domain Models simplify Application layer implementation.
- Workflow-oriented modules benefit from business-focused commands rather than generic CRUD operations.
- Separate read models improve clarity and reduce coupling between presentation and domain models.
- Feature-first organization scales effectively as business workflows become more complex.
- Architecture reviews before implementation reduce technical debt and improve consistency.

---

## Reflection

Sprint 3 demonstrated that the architectural foundation established during previous milestones was sufficient to support significantly more complex business behavior.

The Purchasing module introduced state transitions, aggregate coordination, and workflow-driven business logic while preserving existing architectural boundaries.

Rather than expanding the responsibilities of the Application layer, business behavior was intentionally concentrated within the Domain Model.

This milestone validated several architectural principles adopted throughout the project:

- Rich Domain Model
- Thin Application Handlers
- Vertical Slice Architecture
- Command / Query Separation
- Business-oriented Repository Design

The result is an Application layer that remains focused on orchestration while the Domain Model owns business rules and workflow transitions.

This milestone represents the project's transition from CRUD-oriented business modules toward workflow-driven enterprise functionality.

---

# Milestone 19 - Purchasing Presentation Layer

## Summary

Implemented the Presentation layer for the Purchasing module and connected the existing Purchasing Application use cases to a usable Razor Pages workflow.

The milestone focused on turning the Purchasing Application layer implemented in Sprint 3 into a complete browser-accessible vertical slice.

Completed:

### Presentation Pages

- Purchase Order Index
- Create Purchase Order
- Purchase Order Details

### Workflow Actions

- Submit Purchase Order
- Approve Purchase Order
- Receive Purchase Order

### Presentation Features

- Supplier selection
- Product selection
- Purchase Order item input
- Expected delivery date
- Remarks
- Purchase Order status display
- Ordered quantity display
- Received quantity display
- Remaining quantity display
- Calculated total display
- Success messages
- Validation summaries
- Receive quantity validation
- Fully received indication

## End-to-End Workflow

The complete Purchase Order workflow was implemented and verified through the browser using persisted database records.

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

The workflow was tested using actual Purchase Orders rather than seeded test data.

This allowed the Presentation layer, Application layer, Domain model, persistence infrastructure, and database to be validated together.

---

## Integration with the Application Layer

The Presentation layer consumes the existing Purchasing Application handlers through dependency injection.

The Purchasing Details page coordinates:

- `GetPurchaseOrderHandler`
- `SubmitPurchaseOrderHandler`
- `ApprovePurchaseOrderHandler`
- `ReceivePurchaseOrderHandler`

The Index page uses:

GetPurchaseOrdersHandler

The Create page uses:

- `CreatePurchaseOrderHandler`
- `GetSuppliersHandler`
- `GetProductsHandler`

The resulting flow remains:

```text
Razor Page
    ↓
Application Handler
    ↓
Domain Aggregate
    ↓
Repository / Unit of Work
    ↓
Database
```

No direct DbContext or repository access was introduced into the Presentation layer.

---

## Repository Integration Issue

During end-to-end testing, the Purchase Order Index initially displayed a Total Amount of 0.00 even though the Details page displayed the correct calculated total.

Investigation showed that the Purchase Order list query did not load the Purchase Order items required by the aggregate to calculate TotalAmount.

The repository query was updated to load the required Purchase Order items.

The solution preserved the existing Domain calculation rather than introducing a duplicated persisted total.

### Lesson Learned

A calculated Domain property still depends on the persistence query loading the data required by the aggregate.

Successful compilation does not guarantee that the complete object graph required by a read model has been loaded.

---

## Receiving Workflow

Receiving was implemented at the Purchase Order Item level.

The workflow supports partial receiving:

```text
Ordered:   10
Received:   0
Remaining: 10

Receive 5
    ↓

Ordered:   10
Received:   5
Remaining:  5
Status: Receiving

Receive 5
    ↓

Ordered:   10
Received:  10
Remaining:  0
Status: Completed
```
The Details page displays Fully Received when an item reaches its ordered quantity.

### Lesson Learned

Item-level workflow actions provide the flexibility required to represent partial business events while keeping the aggregate responsible for enforcing the resulting state.

---

## Validation Testing

The Receive workflow was tested through both client-side and Domain validation.

### Client-Side Validation

The Receive input prevents invalid values such as zero through the HTML minimum constraint.

### Domain Validation

Client-side validation was intentionally bypassed during testing to verify that the Domain remained authoritative.

Submitting a zero quantity reached the Domain and resulted in:

```text
DomainException
Received quantity must be greater than zero.
```

This confirmed that business invariants remain protected even when Presentation-layer validation is bypassed.

### Lesson Learned

Client-side validation should improve user experience, but it should never be treated as the business-rule boundary.

---

## Presentation Feedback Improvements

During the final review, several Presentation-layer improvements were implemented.

### Success Messages

Existing `TempData["SuccessMessage"]` values are now displayed by the relevant Razor Pages.

This provides visible confirmation after operations such as:

- Create
- Submit
- Approve
- Receive

### Query Failure Feedback

The Index page now surfaces Application query failures through the validation summary rather than silently presenting an empty Purchase Order list.

### Dropdown Failure Feedback

The Create page now checks Supplier and Product query results and reports failures through `ModelState`.

This prevents a failed lookup from being silently interpreted as an empty selection list.

---

## Architecture Validation

Sprint 4 confirmed that the existing architecture could support a complete workflow-driven Presentation layer without structural redesign.

The review confirmed:

- Razor Pages depend on Application handlers.
- Application handlers coordinate use cases.
- Domain entities enforce business rules.
- Repositories remain behind Application abstractions.
- The Presentation layer does not directly access persistence.
- Workflow actions are expressed as business-oriented commands.
- Existing Result-based response handling remains the Application contract.

The implementation therefore extended the existing architecture rather than introducing a new Presentation-specific pattern.

---

## Technical Findings

Two issues were identified during the Sprint 4 review but intentionally deferred.

### DomainException Boundary Handling

When client-side validation is bypassed, Domain exceptions can propagate from Application handlers.

This is a cross-cutting concern rather than a Purchasing-specific problem.

No Purchasing-specific workaround was introduced.

A consistent project-wide strategy for converting Domain exceptions into the application's Result/error-handling mechanism should be evaluated separately.

### Inventory Update During Receiving

The current Receive workflow updates:

- Purchase Order Item received quantity
- Purchase Order status

It does not currently update Product inventory.

No inventory synchronization behavior was added during Sprint 4 because the required business rule and architectural boundary have not yet been formally established.

These findings are therefore treated as technical debt/future design work rather than incomplete Sprint 4 implementation.

---

## Lessons Learned

- A complete vertical slice is more valuable than implementing isolated Presentation pages without validating the workflow.
- Existing Application handlers can be exposed through Razor Pages without moving business logic into the Web layer.
- Repository queries must load the data required by calculated aggregate properties.
- Client-side validation improves usability, while Domain validation protects business invariants.
- Partial receiving is naturally represented at the Purchase Order Item level.
- End-to-end testing can reveal integration issues that compilation and unit-level inspection do not expose.
- Presentation-layer error handling should distinguish between an empty result and an actual Application or persistence failure.
- Cross-cutting concerns should be solved consistently rather than through feature-specific workarounds.
- Implementation and documentation commits should remain separate so that Git history clearly distinguishes software changes from documentation changes.

---

# Milestone 20 - Reporting: Inventory Valuation

## Summary

Implemented the first dedicated Reporting vertical slice through the Inventory Valuation report.

The milestone extended the existing read-oriented architecture used by Dashboard Reporting into a dedicated Reporting feature without introducing structural architectural changes.

Completed:

### Application

- Inventory Valuation read model
- `InventoryValuationDto`
- `GetInventoryValuationRequest`
- `GetInventoryValuationHandler`
- `IInventoryValuationRepository`

### Infrastructure

- `InventoryValuationRepository`
- Read-only EF Core projection
- Category relationship projection
- Database-side inventory valuation calculation

### Presentation

- Inventory Valuation Razor Page
- Inventory Valuation navigation entry
- Product-level valuation display
- Total Inventory Value display

## Inventory Valuation

The report calculates inventory value using:

```text
Inventory Value
= QuantityOnHand × CostPrice
```

The report total is calculated as:

```text
Total Inventory Value
= Σ (QuantityOnHand × CostPrice)
```

The implementation uses actual persisted Product and Category data.

## Reporting Workflow

The completed read-oriented workflow is:

```text
Inventory Valuation Razor Page
        ↓
GetInventoryValuationHandler
        ↓
IInventoryValuationRepository
        ↓
InventoryValuationRepository
        ↓
EF Core Projection
        ↓
SQL Server
        ↓
InventoryValuationDto
        ↓
Inventory Valuation View
```

The report does not modify Product or Inventory Transaction entities.

## EF Core Query Translation Issue

During implementation, the initial query attempted to order the projected DTO:

```text
Projection
     ↓
OrderBy(DTO.ProductName)
```

EF Core could not translate the resulting expression.

The query was changed to order the underlying entity property before performing the DTO projection:

```text
Product
     ↓
OrderBy(Product.Name)
     ↓
DTO Projection
     ↓
InventoryValuationDto
```

This kept ordering, calculation, and projection database-side without introducing client-side evaluation.

## Lesson Learned

When using EF Core read projections, ordering and filtering should preferably be applied to translatable entity properties before the final DTO projection when the projected DTO expression cannot be translated.

## Validation

The Inventory Valuation report was verified through the browser using actual persisted database records.

Validated:

- Inventory Valuation navigation
- Report page loading
- Product data retrieval
- Category projection
- Quantity On Hand display
- Cost Price display
- Individual Inventory Value calculation
- Total Inventory Value calculation
- Dashboard/report total consistency
- Existing application functionality
- Solution build

The report total was compared against the existing Dashboard Inventory Value and matched.

## Architecture Validation

Sprint 5 demonstrated that the existing architecture supports dedicated read-oriented Reporting features without requiring structural redesign.

The Reporting path follows:

```text
Presentation
     ↓
Application
     ↓
Read Model
     ↓
Repository Abstraction
     ↓
Infrastructure
     ↓
Database
```

This differs from transactional workflows where Domain aggregates participate in business state changes.

The implementation confirms that read-only Reporting can coexist with transactional business workflows while preserving the existing Clean Architecture boundaries.

## Lessons Learned

- Dedicated DTO projections are appropriate for read-only Reporting features.
- Reporting queries should retrieve only the data required by the presentation layer.
- EF Core translation should be validated before introducing client-side evaluation.
- Database-side ordering, calculation, and projection help keep read queries efficient.
- Existing Dashboard reporting patterns provided a proven foundation for the first dedicated Reporting slice.
- A complete browser-verified vertical slice provides stronger validation than implementation alone.
- Reporting can be introduced without creating a separate architectural layer.
- Documentation should distinguish implemented Reporting capabilities from future reports and exports.

## Reflection

Sprint 5 extended the platform from its existing Dashboard reporting capability into a dedicated Reporting module.

Inventory Valuation became the first Reporting vertical slice and demonstrated that the existing architecture can support read-oriented business capabilities alongside workflow-driven transactional modules.

The implementation reused established patterns rather than introducing speculative abstractions.

The resulting architecture remains:

```text
Transactional Workflows

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

and

```text
Read-Oriented Reporting

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

No structural architectural redesign was required.

The remaining Reporting roadmap includes additional reports, Excel/PDF export, and further validation of empty-state and query-failure behavior.

---

# Milestone 21 - Account Management

## Summary

Implemented the Account Management vertical slice to provide authenticated users with self-service account management capabilities while preserving the existing Identity abstraction and Clean Architecture boundaries.

The milestone extended the existing Identity and User Management foundation into end-user account workflows without introducing a separate authentication architecture.

Completed:

### Profile Management

- User Profile
- Update Profile
- Phone Number Update
- Blank Phone Handling
- Self-Service Account Management

### Password Management

- Change Password
- Forgot Password
- Reset Password
- Force Password Change
- `MustChangePassword` support

### Email Verification

- Request Email Verification
- Email Verification
- Email Confirmation
- Verification state displayed in Profile

### Two-Factor Authentication

- 2FA Setup
- Authenticator-based TOTP Verification
- 2FA Login Challenge
- Recovery Codes
- Recovery Code Login
- Recovery Code Regeneration
- Recovery Code Invalidation
- Disable 2FA

## Identity Integration

The Account Management workflows use the existing Identity Service abstraction rather than exposing ASP.NET Core Identity framework types directly to the Web or Application layers.

The resulting flow remains:

```text
Razor Page
     ↓
Application Handler
     ↓
Identity Service Abstraction
     ↓
ASP.NET Core Identity
```

This preserves the existing separation between:

```text
Administrative User Management
        ↓
Manage users

Account Management
        ↓
Manage authenticated user's own account

Authentication
        ↓
Authenticate the user
```

## Two-Factor Authentication Flow

The 2FA implementation separates account security configuration from authentication enforcement.

Account Management is responsible for:

- Enabling 2FA
- Verifying authenticator setup
- Generating recovery codes
- Regenerating recovery codes
- Invalidating previous recovery codes
- Disabling 2FA

The authentication flow is responsible for:

- Detecting that 2FA is required during login
- Displaying the 2FA challenge
- Verifying the authenticator code
- Supporting recovery-code authentication

This separation keeps security configuration and authentication enforcement within their respective workflows.

## Validation Testing

The Account Management features were verified through actual browser workflows.

Validated:

- Profile display and update
- Phone number update
- Blank phone number handling
- Password change
- Forced password change
- Forgot password
- Password reset
- Email verification request
- Email confirmation
- 2FA setup
- Authenticator-code verification
- 2FA login challenge
- Recovery-code login
- Recovery-code regeneration
- Recovery-code invalidation
- 2FA disablement
- Navigation to Account Management and 2FA

The solution was repeatedly built during implementation and completed successfully after resolving integration issues encountered during development.

## Lessons Learned
- Existing Identity abstractions can support self-service account workflows without exposing framework-specific APIs.
- Administrative User Management and self-service Account Management should remain separate concerns.
- Two-factor authentication configuration and authentication enforcement are related but distinct workflows.
- Recovery-code lifecycle management should explicitly handle generation, regeneration, single-use authentication, and invalidation.
- Browser-based validation is essential for authentication workflows because successful compilation does not guarantee correct authentication state transitions.
- Existing Application handler and Razor Pages patterns were sufficient for Account Management without introducing a new architectural pattern.
- Security-sensitive workflows benefit from incremental implementation and validation rather than implementing the entire feature at once.

## Outcome

The Account Management vertical slice was completed and validated without requiring structural architectural redesign.

The implementation extended the existing Identity architecture while preserving the established Clean Architecture, feature-first organization, Application handler patterns, and Razor Pages workflows.

---

# Milestone 22 - Dynamic Authorization Architecture Decision

## Summary

Reviewed the existing Identity and authorization implementation in preparation for expanding business workflow responsibilities.

The current platform uses ASP.NET Core Identity with role-based and policy-based authorization behind the Identity Service abstraction.

The current authorization implementation remains unchanged during the Additional Reporting phase.

The capability model is an architectural decision only at this stage.

The review identified that future business responsibilities should not require a growing collection of hard-coded roles.

A Dynamic Capability-Based Authorization model was therefore selected as the future authorization direction.

## Finalized Model

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

Capabilities represent atomic functionality or actions.

Groups compose capabilities into reusable business responsibilities.

Examples:

```text
PO Account
    ↓
PurchaseOrder.View
PurchaseOrder.Create
PurchaseOrder.Edit
PurchaseOrder.Submit
```

```text
IT Account
    ↓
PurchaseOrder.View
PurchaseOrder.Approve
PurchaseOrder.Reject
PurchaseOrder.Receive
```

## Architectural Boundary

Capability authorization will determine whether the current user is authorized to attempt an application action.

The Domain Model will continue to determine whether that action is valid for the current business state.

Therefore:

```text
Authorization
       +
Domain State Validation
```

remain separate responsibilities.

## Implementation Sequencing

The Dynamic Capability-Based Authorization architecture will not interrupt the current Additional Reporting work.

The agreed sequence is:

```text
Additional Reporting
        ↓
Complete current reporting scope
        ↓
Dynamic Capability-Based Authorization
        ↓
Apply capabilities to Purchasing
        ↓
Extend Purchasing workflow where required
```

Additional Reporting can continue independently because the
reporting architecture does not depend on the future
authorization implementation.

## Outcome

The Dynamic Capability-Based Authorization architecture was
accepted as the future authorization direction.

No authorization implementation changes were made as part of
this design decision.

The current Identity implementation remains unchanged until the
authorization implementation phase begins.

## Lessons Learned

- Authorization should represent capabilities rather than individual business labels whenever responsibilities are expected to evolve.
- Groups provide a reusable way to compose capabilities.
- Authorization and Domain state validation are separate concerns.
- Architectural decisions should be recorded before introducing cross-cutting implementation changes.
- Current feature development should not be blocked by future architectural enhancements when the two concerns are independently evolvable.

---

# Milestone 23 - Reporting: Purchase History

## Summary

Implemented the Purchase History reporting vertical slice,
extending the existing read-oriented Reporting architecture
established by Inventory Valuation.

The feature provides a read-only view of historical Purchase
Orders without modifying Purchasing Domain aggregates.

## Completed

### Application

- Purchase History read model
- Purchase History DTO
- Purchase History request model
- Purchase History application handler
- Purchase History repository abstraction

### Infrastructure

- Purchase History repository
- Read-only EF Core projection
- Supplier projection
- Purchase Order status projection
- Purchase Order totals projection

### Presentation

- Purchase History Razor Page
- Reporting navigation
- Search
- From/To date filtering
- Pagination
- Sorting

## Reporting Query

The reporting flow is:

```text
Purchase History Razor Page
        ↓
GetPurchaseHistoryHandler
        ↓
IPurchaseHistoryRepository
        ↓
PurchaseHistoryRepository
        ↓
EF Core Projection
        ↓
SQL Server
        ↓
PurchaseHistoryDto
        ↓
Purchase History View
```

The report is read-only and does not modify Purchase Order, Purchase Order Item, Product, Supplier, or Inventory state.

## Filtering

The report supports:

- Server-side search
- From date
- To date
- Pagination
- Sorting

Filtering, sorting, and paging are performed server-side to avoid loading the complete Purchase History dataset into memory.

## Architecture Validation

Purchase History confirms that the read-oriented Reporting
architecture established by Inventory Valuation can be reused
for another business domain without introducing a separate
Reporting architecture.

The implementation continues to follow:

```text
Presentation
     ↓
Application
     ↓
Read Model
     ↓
Repository Abstraction
     ↓
Infrastructure
     ↓
Database
```

## Outcome

Purchase History reporting is complete for the current scope.

Remaining Reporting work includes additional reports, export capabilities, and explicit empty-state/query-failure validation.

## Lessons Learned
- Reporting features can reuse the same read-oriented architecture across different business domains.
- Server-side filtering, sorting, and pagination should remain part of the query pipeline.
- Reporting should remain independent from transactional Domain workflows.
- Additional capabilities can be added without coupling the report to the Purchasing aggregate.

---

# Architecture Validation

After implementing:

- Product Management
- Category Management
- Supplier Management
- Customer Management
- Unit Management
- Inventory Transactions
- Dashboard Reporting
- Authentication
- User Management
- Purchasing Application Layer
- Purchasing Presentation Layer
- Reporting: Inventory Valuation
- Account Management

the architecture has demonstrated:

- Consistent implementation patterns
- Reusable application handlers
- Reusable repository infrastructure
- Shared paging, sorting, and filtering
- Stable Clean Architecture boundaries
- Seamless evolution from master data to transactional workflows
- Separation between workflow orchestration and Presentation concerns
- End-to-end integration between Presentation, Application, Domain, Infrastructure, and database layers

The combined milestones demonstrate that the architecture supports:

- Master data modules
- Transactional workflows
- Read-only reporting modules
- Authentication and authorization
- Administrative user management
- Workflow-driven business modules
- Browser-accessible end-to-end workflows
- Self-service account management
- Email verification
- Two-factor authentication

without requiring structural redesign.

Architecture Sprint 1 formally validated these conclusions through a comprehensive review of the solution before the introduction of larger business workflow modules.

---

# Current Development Position

The platform is currently continuing development of Additional
Reporting capabilities.

Purchase History reporting has been implemented with:

- Server-side Search
- From/To Date Filtering
- Server-side Pagination
- Server-side Sorting

The next reporting work will continue independently of the future
authorization architecture.

Dynamic Capability-Based Authorization has been finalized as the
future authorization direction but has not yet been implemented.

The agreed implementation sequence is:

```text
Additional Reporting
        ↓
Complete current reporting scope
        ↓
Dynamic Capability-Based Authorization
        ↓
Apply capabilities to Purchasing
        ↓
Extend Purchasing workflow where required
```

This sequencing intentionally prevents a cross-cutting authorization redesign from interrupting the current reporting implementation.

---

# Workflow Architecture Validation

Sprint 3 validated the Application-layer architecture for workflow-driven business processes through the implementation of the Purchasing Application layer.

Sprint 4 extended that validation into the Presentation layer by connecting the existing Purchasing Application use cases to Razor Pages and verifying the complete workflow through the browser.

The Purchasing module confirmed that:

- Rich Domain Models scale effectively for business workflows.
- Existing repository infrastructure supports aggregate-based operations.
- Feature-first organization remains effective as workflow complexity increases.
- Request / Response / Handler organization provides a consistent implementation pattern.
- Razor Pages can consume Application handlers without bypassing architectural boundaries.
- The existing architecture required no structural redesign to support an end-to-end workflow-driven business capability.

This milestone validates the architecture's ability to evolve from CRUD-oriented modules into enterprise workflow modules while preserving Clean Architecture principles.

---

# Engineering Principles Reinforced

Throughout development the following principles have consistently guided implementation:

- Separation of Concerns
- SOLID Principles
- Dependency Inversion
- Reuse before duplication
- Build first, generalize later
- Prefer compile-time safety
- Push processing to the database whenever practical
- Maintain consistent module architecture
- Favor proven patterns over premature abstraction
- Keep domain behavior inside entities
- Prefer immutable business history for transactional data
- Use read-only DTO projections for reporting features
- Encapsulate framework-specific implementations behind application abstractions
- Apply the Rule of Three before introducing shared abstractions
- Keep Application handlers focused on orchestration
- Model business workflows as explicit commands
- Return dedicated read models for query operations
- Prefer workflow-oriented business behavior over generic CRUD operations
- Separate administrative identity management from self-service account management
- Separate account security configuration from authentication enforcement

---

# Engineering Philosophy

Throughout development the project has intentionally favored incremental evolution over speculative design.

Common infrastructure is introduced only after proving its value across multiple independent implementations.

This approach has helped keep the solution simple while allowing reusable components to emerge naturally as the application has grown.

The project deliberately applies the Rule of Three to balance maintainability against premature abstraction.

---

# Current and Future Journal Entries

## Current Development

- Additional Reporting
- Purchase History Reporting
- Remaining Reporting Enhancements

## Planned Architecture

- Dynamic Capability-Based Authorization
- Purchasing Workflow Authorization

## Future Milestones

- Sales Module
- Audit Logging
- REST API
- Integration Testing