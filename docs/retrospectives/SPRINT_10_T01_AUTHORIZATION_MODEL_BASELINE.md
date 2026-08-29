# Sprint 10 T01 - Authorization Model & Architecture Baseline Result

## Purpose

This document is the durable handoff artifact from Sprint 10 T01.

It carries T01's source-grounded findings, architectural recommendations, discrepancies, risks, and task dependencies into subsequent fresh Sprint 10 task sessions.

### Authority hierarchy

1. **Latest repository/source ZIP** - authoritative for actual implementation state.
2. **Sprint 10 README.md** - authoritative for Sprint 10 rules, scope, workflow, and constraints.
3. **This T01 baseline result** - authoritative only for the findings and recommendations produced by T01 source/documentation inspection.
4. **Individual task prompt** - authoritative for the scope of the current task.

If this document conflicts with the latest repository/source, the repository/source must be re-inspected and the discrepancy reported. Do not silently reconcile differences.

---

# 1. T01 Status

T01 was completed as a planning and source-validation task.

No source code was implemented.

No migrations were created.

No Git operations were performed.

No runtime, build, database, or browser verification was claimed.

---

# 2. Source and Documentation Inspected

The supplied repository/source ZIP was inspected directly.

The relevant project structure is:

- InventoryPlatform.Web
- InventoryPlatform.Application
- InventoryPlatform.Domain
- InventoryPlatform.Infrastructure
- InventoryPlatform.Shared

Relevant project documentation inspected included:

- README.md
- PROJECT_STATUS.md
- ROADMAP.md
- FEATURES.md
- DESIGN_DECISIONS.md
- ENGINEERING_JOURNAL.md
- ARCHITECTURE_REVIEW.md
- Sprint 9 baseline/convention/retrospective documentation
- Sprint 9 browser-verification documentation
- Sprint 9 documentation synchronization/final architecture validation

---

# 3. Current Authentication Architecture

Authentication is based on ASP.NET Core Identity.

The Web layer registers:

`AddIdentity<ApplicationUser, IdentityRole<Guid>>()`

with Entity Framework stores and the existing Identity infrastructure.

Authentication uses the application cookie with:

- LoginPath: /Identity/Account/Login
- LogoutPath: /Identity/Account/Logout
- AccessDeniedPath: /Identity/Account/AccessDenied
- Cookie: InventoryPlatform.Auth
- Sliding expiration enabled
- Expiration: 8 hours

Authentication remains an Identity/Web concern.

Existing account-management behavior must remain functional during Sprint 10, including applicable password, email verification, two-factor, recovery-code, and MustChangePassword behavior.

---

# 4. Current Identity User Model

The actual user type is:

`InventoryPlatform.Infrastructure.Identity.ApplicationUser`

It derives from:

`IdentityUser<Guid>`

The current custom property identified is:

`MustChangePassword`

No dynamic authorization Group or Capability properties currently exist on ApplicationUser.

---

# 5. Existing Identity Roles

The current role catalog identified in the source is:

- Administrator
- InventoryManager
- Viewer

The default seeded accounts map to the existing Identity roles:

- admin -> Administrator
- manager -> InventoryManager
- viewer -> Viewer

These roles are part of the current authorization baseline and must not be removed blindly.

---

# 6. Existing Authorization Policies

The Web layer defines:

`InventoryPlatform.Web.Authorization.AuthorizationPolicies`

with these policies:

- Administrator
- InventoryManagement
- ViewInventory

Current role requirements are:

### Administrator

Requires the Administrator role.

### InventoryManagement

Allows:

- Administrator
- InventoryManager

### ViewInventory

Allows:

- Administrator
- InventoryManager
- Viewer

These are static role-backed ASP.NET Core authorization policies.

They are not yet dynamic capability policies.

---

# 7. Existing Claims

The source contains the custom claim type:

`inventory:must-change-password`

It is produced by:

`ApplicationUserClaimsPrincipalFactory`

when:

`ApplicationUser.MustChangePassword == true`

The factory derives from:

`UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>`

and therefore retains normal Identity principal/role behavior through the base implementation.

No dynamic capability claims were identified.

No Group claims were identified.

### T01 recommendation

Do not make persistent capability claims the primary Sprint 10 authorization mechanism.

Capability authorization should resolve from authoritative dynamic authorization state rather than permanently embedding the effective capability set in the authentication cookie.

---

# 8. Existing Razor Authorization Boundaries

The Web application applies global Razor authorization to `/` with explicit anonymous exceptions for applicable public/account pages.

The source also has folder-level authorization, including:

- /Products - authenticated access
- /Administration - Administrator policy
- /Inventory - InventoryManagement policy

Individual PageModels also use:

`[Authorize(Policy = ...)]`

with the existing policies.

Affected areas identified include:

- Administrator Users
- Categories
- Customers
- Dashboard
- Inventory Transactions
- Products
- Suppliers
- Units

This establishes server-side authorization at the Razor PageModel/folder boundary.

---

# 9. Existing Razor UI Visibility

The shared layout contains role-based UI visibility using:

`User.IsInRole(IdentityConstants.Roles.Administrator)`

This is used for Administrator navigation visibility.

Important distinction:

`[Authorize]` and authorization policies are security boundaries.

`User.IsInRole(...)` used for navigation visibility is presentation behavior.

Sprint 10 must never treat hidden navigation/buttons as security.

Direct URL/request authorization must remain server-side enforced.

---

# 10. Purchasing Authorization Baseline

The source inspection did not identify explicit `[Authorize]` attributes or `User.IsInRole(...)` checks directly on the Purchasing PageModels.

The current application therefore does not appear to have fine-grained Purchase Order action authorization at the PageModel level.

The broader Razor authorization configuration provides the current authenticated-access boundary.

T09 must inspect the actual Purchase Order actions and establish capability-backed authorization for actions that actually exist.

Candidate capabilities from the established Sprint 10 architecture are:

- PurchaseOrder.View
- PurchaseOrder.Create
- PurchaseOrder.Edit
- PurchaseOrder.Submit
- PurchaseOrder.Approve
- PurchaseOrder.Reject
- PurchaseOrder.Receive

The final capability catalog must be derived from actual application behavior.

---

# 11. Existing Application Authorization Architecture

The Application layer contains:

`InventoryPlatform.Application.Interfaces.Identity.IIdentityService`

This is the existing Identity abstraction.

It currently supports Identity-oriented operations including applicable:

- role retrieval
- user retrieval
- user creation
- user role updates
- user status updates
- password reset

No dynamic capability authorization abstraction currently exists.

T03 is the intended task for introducing the Application authorization abstraction.

---

# 12. Existing Infrastructure Authorization Architecture

The Infrastructure Identity implementation is:

`InventoryPlatform.Infrastructure.Identity.IdentityService`

It uses:

- ApplicationDbContext
- UserManager<ApplicationUser>
- RoleManager<IdentityRole<Guid>>

It handles existing Identity operations including:

- role retrieval
- user retrieval
- role assignment/removal
- user creation
- password reset
- user status management

Infrastructure is therefore already the established boundary for Identity framework integration.

Dynamic capability persistence and resolution should similarly remain Infrastructure-backed.

---

# 13. Existing Identity Persistence

`ApplicationDbContext` derives from:

`IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`

The existing Identity schema therefore persists the standard Identity structures, including:

- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

No dynamic authorization tables were identified in the current source.

---

# 14. Existing Seed Data

`IdentitySeeder` creates the current Identity roles:

- Administrator
- InventoryManager
- Viewer

and the corresponding default users.

The current seed process assigns Identity roles directly to users.

T05 must introduce the capability/Group seed and compatibility mapping without prematurely removing the current role behavior.

Administrator access must remain safe throughout the transition.

---

# 15. Relevant Architecture Decision

The established target architecture is:

User
  |
  +-- Group
        |
        +-- Capability

The authorization decision is separate from Domain business-state validation.

Example:

Can Submit Purchase Order
=
PurchaseOrder.Submit capability
AND
PurchaseOrder.Status == Draft

Therefore:

Authorization answers:
"May this user attempt this action?"

Domain validation answers:
"Is this action valid for the current business state?"

This separation must be preserved.

---

# 16. Documentation/Source Discrepancies

## Discrepancy 1 - Planned capability architecture vs source

Documentation establishes Dynamic Capability-Based Authorization as the Sprint 10 target architecture.

The current source does not implement the dynamic capability model.

Status:

- Documentation: planned/accepted architecture
- Source: not implemented

This is expected at the Sprint 10 starting point.

## Discrepancy 2 - Existing policies are role-backed

The current source defines Administrator, InventoryManagement, and ViewInventory as role-backed policies.

They do not yet represent granular capability evaluation.

These policies should be treated as the migration baseline.

## Discrepancy 3 - Purchasing has no apparent fine-grained PageModel authorization

Purchasing PageModels did not show direct `[Authorize]` or `User.IsInRole(...)` protection during T01 inspection.

T09 must not assume Purchase Order actions already have capability-level security.

## Discrepancy 4 - Navigation authorization is role-specific and coarse

The shared layout uses `User.IsInRole(...)` for Administrator navigation visibility.

Other navigation is generally not expressed with fine-grained capability visibility.

T12 should address relevant capability-aware UI visibility.

---

# 17. Recommended Capability Model

Capability represents an atomic application action.

Preferred naming:

`<Resource>.<Action>`

Examples:

- PurchaseOrder.View
- PurchaseOrder.Create
- PurchaseOrder.Edit
- PurchaseOrder.Submit
- PurchaseOrder.Approve
- PurchaseOrder.Reject
- PurchaseOrder.Receive

Capabilities should be system-defined.

Administrators assign capabilities but do not arbitrarily invent new application capabilities.

Do not create capabilities for nonexistent application actions.

---

# 18. Recommended Group Model

The target conceptual model is:

User
  |
  +-- AuthorizationGroup
          |
          +-- Capability

Users may belong to multiple Groups.

Effective capabilities are the union of capabilities supplied by all Groups.

Example:

PO Account
- PurchaseOrder.View
- PurchaseOrder.Create
- PurchaseOrder.Edit
- PurchaseOrder.Submit

IT Account
- PurchaseOrder.View
- PurchaseOrder.Approve
- PurchaseOrder.Reject
- PurchaseOrder.Receive

A user assigned to both Groups receives the union.

There must be no "last Group wins" behavior.

---

# 19. Capability Enabled/Disabled Behavior

If Capability has an enabled/disabled state:

`IsEnabled == true`
-> capability can authorize

`IsEnabled == false`
-> DENY

A disabled capability must never grant access.

A missing capability must also result in DENY.

---

# 20. Identity Role Compatibility Strategy

Use a controlled hybrid migration.

Existing Identity roles remain during transition.

Conceptually:

Existing Identity Role
  |
  v
Compatibility Authorization Group
  |
  v
Capabilities

The purpose is to preserve existing effective access while dynamic authorization is introduced.

Do not delete existing roles or role-based policies at the beginning of Sprint 10.

Only consider removing obsolete authorization dependencies after equivalent capability-backed behavior has been implemented and verified.

Administrator access must be explicitly verified before obsolete authorization dependencies are removed.

---

# 21. Recommended Authorization Evaluation

Target evaluation:

Authenticated?
  |
  +-- No -> DENY
  |
  +-- Yes
        |
        v
Required Capability
        |
        v
User's Groups
        |
        v
Group Capabilities
        |
        v
Capability exists and enabled?
        |
        +-- No -> DENY
        |
        +-- Yes -> ALLOW

This authorization result does not bypass Domain business rules.

The final application operation still requires normal domain/application validation.

---

# 22. Proposed Architectural Boundaries

## Domain

May contain persistent authorization concepts where justified.

Must not depend on:

- ASP.NET Core Identity
- ClaimsPrincipal
- UserManager
- RoleManager
- ASP.NET authorization attributes
- Razor
- HTTP concerns

## Application

Owns the authorization abstraction needed by application behavior.

Must not unnecessarily depend on ASP.NET Core authorization framework types.

## Infrastructure

Owns:

- capability persistence
- Group persistence
- user/group relationships
- group/capability relationships
- EF Core configuration
- authorization queries
- seed data
- Identity compatibility integration

## Web

Owns:

- ASP.NET Core authorization requirements
- authorization handlers
- HTTP authorization boundaries
- Razor authorization
- capability-aware navigation/UI visibility

Preferred flow:

ASP.NET Core Policy
  |
  v
Capability Requirement
  |
  v
Capability Authorization Handler
  |
  v
Application Authorization Abstraction
  |
  v
Infrastructure-backed capability resolution

---

# 23. Proposed Database Model

The target conceptual schema is:

ApplicationUser
      |
      v
UserAuthorizationGroup
      |
      v
AuthorizationGroup
      |
      v
AuthorizationGroupCapability
      |
      v
Capability

Conceptually:

ApplicationUser
- Id

AuthorizationGroup
- Id
- Name
- applicable status/metadata as approved

Capability
- Id
- Name
- IsEnabled
- applicable metadata as approved

Relationships must be many-to-many as required by the model.

T04 must finalize:

- primary keys
- foreign keys
- uniqueness
- indexes
- delete behavior
- relationship constraints
- seed requirements

T01 did not create or modify the database model.

T06 is the migration task.

---

# 24. Administration Requirements

Minimum intended administration surface:

## Group management

- view Groups
- manage Groups as approved by T11

## Group capability assignment

- view system capabilities
- assign capabilities to Groups
- remove capabilities from Groups

## User Group assignment

- view Groups assigned to a user
- assign Groups
- remove Groups

## Capability catalog

Capabilities are system-defined.

Out of initial scope:

- user-specific capability overrides
- capability inheritance
- Group inheritance
- deny rules
- authorization scripting
- arbitrary administrator-created capabilities

---

# 25. Primary Security Risks

1. UI-only authorization
2. Stale authorization state in authentication cookies
3. Administrator lockout during role migration
4. Missing capability accidentally allowing access
5. Disabled capability accidentally allowing access
6. Incorrect multiple-Group behavior
7. Domain coupling to ASP.NET/Identity
8. Duplicate authorization representations
9. Incomplete Purchasing action coverage
10. Premature removal of existing role/policy behavior

---

# 26. T02-T15 Plan

## T02 - Capability and Group Domain Model

Define the domain representation for Capability, Authorization Group, and required relationships.

No Web authorization integration.

## T03 - Application Authorization Abstractions

Define Application-facing contracts for capability authorization.

## T04 - Authorization Persistence & EF Core Configuration

Implement persistence/configuration for:

- Capability
- AuthorizationGroup
- user/group relationship
- group/capability relationship

## T05 - Capability/Group Seed Data & Identity Compatibility Mapping

Seed system capabilities and Groups.

Establish Identity role compatibility.

Preserve effective access.

## T06 - Database Migration

Create the EF Core migration after model/persistence work is complete and verified.

## T07 - Capability Authorization Service

Implement effective capability resolution.

Required semantics:

- existing + enabled -> allow
- missing -> deny
- disabled -> deny
- multiple Groups -> union

## T08 - ASP.NET Core Capability Authorization Handler

Connect ASP.NET Core policy/requirements to the Application authorization abstraction.

## T09 - Purchasing Dynamic Authorization Integration

Apply capability-backed authorization to actual Purchase Order operations.

## T10 - Existing Authorization Boundary Migration

Incrementally migrate existing role-backed policies and Razor authorization boundaries.

Do not remove existing behavior until equivalent behavior is verified.

## T11 - Authorization Administration

Implement minimum administration for Groups, Group capabilities, User Groups, and capability visibility.

## T12 - Razor Navigation & UI Capability Visibility

Introduce capability-aware UI visibility where appropriate.

UI visibility remains secondary to server-side authorization.

## T13 - Integrated Authorization Verification

Verify:

- authorized access
- unauthorized access
- direct URL access
- capability assignment
- capability removal
- multiple Groups
- disabled capabilities
- administrator behavior
- existing authorization regression
- navigation visibility
- server-side enforcement
- login/logout
- active-session behavior
- restart persistence
- seed behavior

## T14 - Documentation Synchronization & Architecture Validation

Synchronize documentation with actual verified implementation.

## T15 - Final Verification, Retrospective & Save Point

Complete final verification, retrospective, architecture validation, documentation validation, and Sprint 10 save point.

---

# 27. Dependency Recommendation

Recommended chain:

T01
 |
 v
T02
 |
 v
T03
 |
 v
T04
 |
 v
T05
 |
 v
T06
 |
 v
T07
 |
 v
T08
 |
 +----> T09
 |
 +----> T10
         |
         v
        T11
         |
         v
        T12
         |
         v
        T13
         |
         v
        T14
         |
         v
        T15

T09/T10 may only be partially parallel if the actual implementation after T08 makes that safe.

---

# 28. Verification Strategy

T01 verification was limited to source/documentation inspection.

No runtime verification was performed or claimed.

Future Sprint 10 verification must distinguish:

- source inspection
- build/test verification
- database/migration verification
- browser verification
- server-side authorization verification
- UI visibility verification

A hidden navigation element is not evidence of authorization.

Direct URL/request attempts must be used to verify server-side enforcement.

---

# 29. Documentation Strategy

Documentation must describe actual verified behavior.

Potential affected documents include:

- PROJECT_STATUS.md
- FEATURES.md
- ROADMAP.md
- DESIGN_DECISIONS.md
- ENGINEERING_JOURNAL.md
- ARCHITECTURE_REVIEW.md
- docs/retrospectives/SPRINT_10_*.md

Only affected documentation should be changed.

Implementation and documentation changes remain separate commits.

Recommended commit pattern:

`feat(auth): ...`

followed by:

`docs(auth): ...`

---

# 30. Deferred Scope

Remain outside Sprint 10 unless explicitly approved:

- Sales Module
- Audit/Activity Logging
- Bulk Import/Export
- Barcode/QR
- user-specific capability overrides
- Group inheritance
- capability inheritance
- deny rules
- authorization scripting
- arbitrary administrator-created capabilities
- authentication replacement
- broad unrelated refactoring
- unrelated UI redesign
- unrelated database redesign

---

# 31. Final T01 Architecture Recommendation

Proceed with Dynamic Capability-Based Authorization as an incremental evolution over the existing Identity foundation.

The architectural responsibilities should be:

Identity
= authentication and existing identity foundation

Groups + Capabilities
= dynamic authorization model

ASP.NET Core Authorization
= Web/server authorization boundary

Application authorization abstraction
= cross-layer authorization contract

Infrastructure
= authoritative capability persistence and resolution

Domain
= business-state validation

Existing roles and policies
= compatibility/migration baseline until equivalent capability behavior is implemented and verified.

---

# 32. T01 Handoff Instructions for T02

T02 must:

1. Read the Sprint 10 README.
2. Read the T02 task prompt.
3. Read this T01 baseline result.
4. Inspect the latest repository/source ZIP independently.
5. Treat the repository/source as authoritative for actual implementation.
6. Use this T01 result as the architectural planning baseline.
7. Report any discrepancy between this T01 result and the latest source.
8. Implement only T02.
9. Do not start T03.
10. Do not perform Git operations unless manually performed by the developer outside ChatGPT.
11. Follow the Sprint 10 standard task flow.
12. Produce the next durable handoff result needed for T03.

## Next task

T02 - Capability and Group Domain Model

STOP.
