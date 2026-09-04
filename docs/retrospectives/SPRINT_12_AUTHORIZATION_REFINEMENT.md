# Sprint 12 Retrospective/Setup — Authorization Refinement

---

> **SPRINT 12 STATUS: PLANNED - IMPLEMENTATION NOT YET STARTED**

---

## 1. Sprint 12 Identity

| Field | Value |
|-------|-------|
| **Sprint** | 12 |
| **Sprint Name** | Authorization Refinement |
| **Repository** | Inventory Management Platform |
| **Branch** | `feature/authorization_refinement` |
| **Sprint Objective** | Authorization Handler Testing + Authorization Boundary Hardening |
| **Status** | PLANNED — IMPLEMENTATION NOT YET STARTED |

---

## 2. Purpose

This document establishes the official Sprint 12 starting baseline, retrospective/setup documentation, architectural direction, scope, constraints, and planning gate before Sprint 12 implementation begins.

It is NOT the final sprint retrospective. Its purpose is to establish:

- Sprint 11 starting context
- Sprint 11 lessons relevant to Sprint 12
- Sprint 12 starting verification baseline
- Historical authorization context
- Current authorization findings
- Sprint 12 objective
- Initial scope
- Architectural direction
- Testing direction
- Explicit non-goals
- Deferred/future work
- Risks and mitigations
- High-level acceptance criteria
- Planning constraints
- Next planning step

The final retrospective will be written only after Sprint 12 implementation and verification are actually complete.

---

## 3. Sprint 11 Starting Baseline

The following baseline was established by Sprint 11 and forms the Sprint 12 starting point:

```text
UnitTests:        219 passed
IntegrationTests:  61 passed
Total:            280 passed
Failures:           0
Build errors:       0
Build warnings:     0
```

**Verification method:** Sprint 11 T09 independently verified `dotnet build` and `dotnet test` against the current repository state. Sprint 12 planning report independently confirmed these figures.

**Baseline preservation requirement:** Sprint 12 must preserve this baseline unless a later verified change provides a documented reason for a difference.

### Authorized Seed Baseline

```text
Capabilities:                    39
Administrator capabilities:      39
InventoryManager capabilities:   21
Viewer capabilities:            13
Total group-capability relationships: 73
```

**Note:** The original planning report stated Viewer=12, Total=72. Source inspection confirmed Viewer=13, Total=73 because `User.View` matches the Viewer filter's `EndsWith(".View")` predicate. This correction is authoritative.

---

## 4. Relevant Sprint 11 Lessons

The following lessons from Sprint 11 materially affect Sprint 12:

### 4.1 Authorization Architecture

The dynamic capability-based authorization system already exists. Sprint 10 implemented it across all four layers (Domain, Application, Infrastructure, Web). Sprint 12 should improve confidence in and harden the existing authorization architecture. It should NOT become an authorization framework redesign.

### 4.2 Testing Architecture

Sprint 11 established a two-project test architecture:

```text
InventoryPlatform.UnitTests     → Domain, Application, Shared
InventoryPlatform.IntegrationTests → Domain, Application, Infrastructure, Shared
```

Neither project references `InventoryPlatform.Web`. This was an intentional architectural decision: the authorization handlers (`CapabilityAuthorizationHandler`, `MultiCapabilityAuthorizationHandler`) live in `InventoryPlatform.Web.Authorization`, and testing them requires a Web project reference.

Sprint 11 deliberately did NOT move authorization handlers to another production project. Testing them requires either:
1. A dedicated Web test project (`InventoryPlatform.Web.Tests`), or
2. WebApplicationFactory integration tests

Sprint 12 must evaluate and document the architectural justification for the chosen approach.

### 4.3 Documentation Reconciliation

The Sprint 10 retrospective states "All P1 findings resolved." This statement refers to **database-level** seed remediation (stale InventoryManager/Administration.Access and Viewer/Supplier.Create relationships removed in T15). It does NOT mean every source-level authorization attribute gap was fixed. Two source-level authorization defects persisted through Sprint 11 unchanged.

### 4.4 Test Double Strategy

Sprint 11 demonstrated that hand-written fakes are sufficient for the current testing scope. `CapabilityAuthorizationServiceTests` (T05) used `FakeCapabilityRepository` and `FakeAuthorizationGroupRepository` with a single-method interface. This pattern should continue for Web.Tests unless evidence demonstrates otherwise.

### 4.5 Hand-Written Fakes vs Mocking Framework

Sprint 11 established the convention of hand-written fakes. No mocking framework (Moq, NSubstitute, FakeItEasy) is in use. Sprint 12 should maintain this convention.

### 4.6 Test Naming

Sprint 11 established the naming pattern:

```text
MethodOrBehavior_WhenCondition_ExpectedResult
```

Sprint 12 tests should follow the same convention.

---

## 5. Historical Authorization Context

### 5.1 Categories/Edit — Missing [Authorize]

**File:** `src/InventoryPlatform/InventoryPlatform.Web/Pages/Categories/Edit.cshtml.cs`

**Source evidence (verified):** The PageModel has no `[Authorize]` attribute and no authorization import. Any authenticated user (including Viewer) can access and edit categories through direct URL access.

**Documented state:**
- Sprint 10 T10 (line 107): "Categories/Edit.cshtml.cs missing [Authorize] — any authenticated user can edit categories."
- Sprint 10 T12 (line 97): "Categories/Edit.cshtml.cs missing [Authorize] — server-side gap, not repaired by T12."
- Sprint 10 T10 (line 123): "Future: Categories/Edit authorization, Suppliers/Create policy"

**Discrepancy:** Documentation correctly identified this as a pre-existing gap in Sprint 10 T10/T12 and deferred it as "Future" work. The source was never modified. The Sprint 10 retrospective claim "All P1 findings resolved" is misleading — it refers to database-level seed fixes, not source-level attribute gaps.

**Impact:** Any authenticated user can modify categories regardless of their authorization group. The server-side `[Authorize]` attribute is the security boundary. UI visibility (Razor conditional rendering) is NOT security.

**Classification:** Confirmed Sprint 12 authorization-boundary defect.

### 5.2 Suppliers/Create — Overly Broad Policy

**File:** `src/InventoryPlatform/InventoryPlatform.Web/Pages/Suppliers/Create.cshtml.cs`

**Source evidence (verified):** The PageModel currently uses `[Authorize(Policy = AuthorizationPolicies.ViewInventory)]`. Viewers can create suppliers server-side through direct URL access despite the UI button being hidden.

**For comparison, equivalent mutation pages use:**
- `Suppliers/Edit.cshtml.cs` → InventoryManagement
- `Categories/Create.cshtml.cs` → InventoryManagement
- `Customers/Create.cshtml.cs` → InventoryManagement

**Documented state:**
- Sprint 10 T10 (line 108): "Suppliers/Create.cshtml.cs uses ViewInventory — Viewers can create suppliers."
- Sprint 10 T12 (line 99): "Suppliers/Create.cshtml.cs uses ViewInventory policy — Viewers don't see button but server allows it."
- Sprint 10 T10 (line 123): "Future: Categories/Edit authorization, Suppliers/Create policy"

**Discrepancy:** Documentation correctly identified this as a pre-existing issue. The source was never modified. Same retrospective caveat applies as Finding 1.

**Impact:** Server-side enforcement is weaker than intended. Viewers can create suppliers through direct URL access despite UI hiding the button.

**Classification:** Confirmed Sprint 12 authorization-boundary defect.

### 5.3 Administrator/Users/EditStatus — Dead-Code IsInRole

**File:** `src/InventoryPlatform/InventoryPlatform.Web/Pages/Administrator/Users/EditStatus.cshtml.cs`

**Source evidence (verified):** The class-level `[Authorize(Policy = AuthorizationPolicies.Administrator)]` means only Administrators can reach `OnPostAsync`. The `IsInRole` check on line 61 is unreachable dead code — it can only execute for users who already passed the Administrator check.

**Documented state:**
- Sprint 10 T10 (line 32): "Code-level IsInRole: 1 occurrence in EditStatus.cshtml.cs line 61 — unreachable dead code"
- Sprint 10 T10 (line 46): "EditStatus.cshtml.cs IsInRole: Unreachable dead code."
- Sprint 10 T12 (line 18): "One dead-code User.IsInRole(...) occurrence in EditStatus.cshtml.cs was documented but intentionally left unchanged (unreachable behind class-level [Authorize(Policy = Administrator)])."

**Classification:** NOT an authorization defect. Dead code for code quality cleanup only. Out of mandatory Sprint 12 scope unless current evidence demonstrates a meaningful security issue. Optionally included as cleanup.

---

## 6. Current Sprint 12 Findings

### 6.1 Source vs Documentation Reconciliation

The Sprint 10 retrospective states "All P1 findings resolved." This statement requires clarification:

- P1 findings at the **database level** (InventoryManager seed having Administration.Access, Viewer seed having Supplier.Create) **were resolved** by T15 Phase 21/25 remediation.
- P1 findings at the **source code level** (Categories/Edit missing attribute, Suppliers/Create wrong policy, EditStatus dead code) **were NOT resolved** — they were documented as pre-existing issues and explicitly deferred as "Future" work.

The retrospective's "All P1 findings resolved" claim is accurate only for the database-level remediation scope. The source-level defects were never within T15's remediation scope.

### 6.2 Current Authorization State

```text
Page-level [Authorize(Policy)] attributes:     50 total
  - Correct:                                    48
  - Missing:                                     1 (Categories/Edit)
  - Wrong policy:                                1 (Suppliers/Create)

Razor UI IAuthorizationService.AuthorizeAsync:  21 checks (all correct, migrated in Sprint 10 T12)

RequireRole usages:                              0
Explicit Forbid() calls:                         0

Authorization handlers:                           2 (CapabilityAuthorizationHandler, MultiCapabilityAuthorizationHandler)
Handler automated tests:                          0 (the gap Sprint 12 addresses)
```

### 6.3 Authorization Handler Architecture

**CapabilityAuthorizationHandler:**
- Location: `InventoryPlatform.Web.Authorization`
- Base class: `AuthorizationHandler<CapabilityRequirement>`
- Service: `ICapabilityAuthorizationService` (Application layer interface)
- Behavior: Checks authentication → extracts `ClaimTypes.NameIdentifier` → parses GUID → calls `HasCapabilityAsync` → calls `context.Succeed` if true → returns (fails) otherwise

**MultiCapabilityAuthorizationHandler:**
- Location: `InventoryPlatform.Web.Authorization`
- Base class: `AuthorizationHandler<MultiCapabilityRequirement>`
- Service: `ICapabilityAuthorizationService` (Application layer interface)
- Behavior: Checks authentication → extracts `ClaimTypes.NameIdentifier` → parses GUID → iterates `requirement.CapabilityNames` → calls `HasCapabilityAsync` for each → calls `context.Succeed` on first match → returns (fails) if no match

**Requirement types:**
- `CapabilityRequirement`: single `string CapabilityName`
- `MultiCapabilityRequirement`: `IReadOnlyList<string> CapabilityNames` (params constructor)

### 6.4 Policy Registration

Authorization policies are registered via `CapabilityAuthorizationExtensions.AddCapabilityPolicy`:

- **Single capability policy:** Adds `RequireAuthenticatedUser()` + `CapabilityRequirement(capabilityName)`
- **Multi capability policy:** Adds `RequireAuthenticatedUser()` + `MultiCapabilityRequirement(capabilityNames)`

The `Administrator` policy is registered via the single-capability overload with `Administration.Access`.

The `ViewInventory` policy is registered via the multi-capability overload with 7 capabilities.

The `InventoryManagement` policy is registered via the multi-capability overload with 9 capabilities.

---

## 7. Sprint 12 Objective

**Authorization Handler Testing + Authorization Boundary Hardening**

This objective addresses two distinct layers:

### Layer 1 — Authorization Handler Behavior

Test `CapabilityAuthorizationHandler` and `MultiCapabilityAuthorizationHandler` to validate their actual behavior. Tests must verify:
- Authentication requirement enforcement
- Claims extraction and parsing
- Service delegation
- Succeed/fail path behavior

Tests should be based on actual handler requirements, actual claims, actual service interaction, and actual authorization semantics. They must not invent behavior.

### Layer 2 — Authorization Boundary

Protect the authorization declarations on Razor PageModels. A handler test can pass even if a developer later accidentally removes `[Authorize]`, changes the policy, or assigns an overly permissive policy. Therefore handler tests alone are insufficient. Sprint 12 must evaluate lightweight automated protection for page-level authorization metadata/boundaries.

---

## 8. Initial Sprint 12 Scope

### 8.1 IN SCOPE

| # | Task | Description | Type |
|---|------|-------------|------|
| T01 | Create `InventoryPlatform.Web.Tests` project with dependency boundaries | Web test infrastructure | Infrastructure |
| T02 | Create `FakeCapabilityAuthorizationService` test double | Implements `ICapabilityAuthorizationService` | Test infrastructure |
| T03 | Create `CapabilityAuthorizationHandlerTests` | Handler unit tests | Handler tests |
| T04 | Create `MultiCapabilityAuthorizationHandlerTests` | Handler unit tests | Handler tests |
| T05 | Remediate Categories/Edit — add `[Authorize(Policy = InventoryManagement)]` | Defect fix | Authorization fix |
| T06 | Remediate Suppliers/Create — change to `[Authorize(Policy = InventoryManagement)]` | Defect fix | Authorization fix |
| T07 | Remove EditStatus.cshtml.cs dead-code IsInRole (optional cleanup) | Code cleanup | Optional |
| T08 | Verify all handler tests pass, build succeeds, regression confirmed | Validation | Verification |
| T09 | Documentation synchronization and Sprint 12 closure | Documentation | Documentation |

### 8.2 NOT IN SCOPE

- WebApplicationFactory integration tests
- Razor Page authorization integration tests
- HTTP pipeline authorization testing
- CI provider establishment
- Browser automation (Playwright/Selenium)
- SQL Server integration testing
- Code coverage measurement or gates
- Mutation testing
- Performance/load testing
- Sales Module
- Audit Logging
- Bulk Import/Export
- Barcode/QR
- Any production feature work beyond the two defect fixes

---

## 9. Architectural Direction

### 9.1 Web.Tests Architecture

Sprint 12 introduces a third test project:

```text
tests/InventoryPlatform.Web.Tests
    → InventoryPlatform.Web
    → InventoryPlatform.Domain
    → InventoryPlatform.Application
    → InventoryPlatform.Infrastructure
    → InventoryPlatform.Shared
```

**Architectural justification:**

The authorization handlers are physically located in `InventoryPlatform.Web.Authorization`. Testing them requires a Web project reference. The existing test architecture explicitly prohibits UnitTests and IntegrationTests from referencing Web:

```text
UnitTests → Web         NOT ALLOWED
IntegrationTests → Web  NOT ALLOWED
```

Modifying UnitTests or IntegrationTests to reference Web would violate the established dependency boundary. A dedicated Web.Tests project preserves the separation:

| Test Project | Domain | Application | Infrastructure | Web |
|---|---|---|---|---|
| UnitTests | ✅ | ✅ | ❌ | ❌ |
| IntegrationTests | ✅ | ✅ | ✅ | ❌ |
| Web.Tests | ✅ | ✅ | ✅ | ✅ |

**Cross-reference rules:**

```text
Web.Tests → UnitTests           NOT ALLOWED
Web.Tests → IntegrationTests    NOT ALLOWED
UnitTests → Web.Tests           NOT ALLOWED
IntegrationTests → Web.Tests    NOT ALLOWED
```

**Future extensibility:** Web.Tests can later host WebApplicationFactory integration tests without polluting other test projects.

### 9.2 Handler Testability

Both handlers can be unit tested WITHOUT WebApplicationFactory:

1. Handlers are `public sealed` classes
2. Both accept `ICapabilityAuthorizationService` via constructor injection
3. The interface lives in `InventoryPlatform.Application.Interfaces.Authorization`
4. A hand-written fake implementing this interface suffices
5. `ClaimsPrincipal` can be constructed directly with test claims
6. `AuthorizationHandlerContext` can be constructed directly
7. No database, HTTP pipeline, or middleware is required

### 9.3 Defect Remediation Approach

Both defects are bounded attribute-level changes:

- **Categories/Edit:** Add `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]` and the `using InventoryPlatform.Web.Authorization` import
- **Suppliers/Create:** Change `AuthorizationPolicies.ViewInventory` to `AuthorizationPolicies.InventoryManagement`

No database migration is expected. No other source changes are required.

---

## 10. Testing Direction

### 10.1 Handler Unit Test Approach

**Framework:** xUnit (consistent with Sprint 11)

**Test doubles:** Hand-written `FakeCapabilityAuthorizationService` implementing `ICapabilityAuthorizationService`. One method: `HasCapabilityAsync(Guid userId, string capabilityName)` → returns `bool`.

**Handler test approach:**
1. Construct the fake service with configurable return values
2. Construct a `ClaimsPrincipal` with `ClaimTypes.NameIdentifier` set to a test GUID
3. Construct an `AuthorizationHandlerContext` with the principal and requirement
4. Invoke the handler's `HandleRequirementAsync`
5. Assert the context outcome (Succeeded / Failed / Unhandled)

**ClaimsPrincipal construction:**

```csharp
var identity = new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.NameIdentifier, testUserId.ToString())
});
var principal = new ClaimsPrincipal(identity);
```

**No database access required:** The fake service short-circuits all database interaction.

**No WebApplicationFactory required:** Handlers are constructed directly.

### 10.2 Page Authorization Boundary Testing

Evaluate lightweight reflection/metadata-based verification that confirms authorization attributes are declared on PageModels. Prefer this over WebApplicationFactory simply to verify attributes.

At minimum, evaluate:
- Categories/Edit
- Suppliers/Create

Reflection can inspect `typeof(PageModel).GetCustomAttribute<AuthorizeAttribute>()` to verify the correct policy is present.

### 10.3 Test Naming Convention

Follow established Sprint 11 convention:

```text
MethodOrBehavior_WhenCondition_ExpectedResult
```

### 10.4 Test Organization

```text
tests/InventoryPlatform.Web.Tests/
    Authorization/
        CapabilityAuthorizationHandlerTests.cs
        MultiCapabilityAuthorizationHandlerTests.cs
        FakeCapabilityAuthorizationService.cs
    Pages/
        PageAuthorizationBoundaryTests.cs
```

Namespace convention: `InventoryPlatform.Web.Tests.Authorization`, `InventoryPlatform.Web.Tests.Pages`

---

## 11. Architectural Constraints

Sprint 12 MUST preserve the established project architecture. It must NOT introduce:

- MediatR
- AutoMapper
- Generic CRUD frameworks
- Generic authorization frameworks
- Unnecessary abstractions
- Unnecessary mocking frameworks (Moq, NSubstitute, FakeItEasy)
- Database access for pure handler unit tests
- WebApplicationFactory for unit-level behavior testing
- Unrelated refactoring

Sprint 12 MUST follow the existing:

- Clean Architecture boundaries
- Feature-first organization
- Explicit handler approach
- Repository abstractions
- Result pattern
- Domain/Application separation
- Web presentation conventions
- Testing conventions (xUnit, hand-written fakes, behavior-oriented naming)

---

## 12. Sprint 12 Non-Goals

Sprint 12 is explicitly NOT:

- A general security rewrite
- A general testing rewrite
- An authorization framework replacement
- A Clean Architecture redesign
- A database redesign
- A Sales implementation sprint
- An Audit/Activity Logging sprint
- A Bulk Import/Export sprint
- A Barcode/QR sprint
- A CI implementation sprint
- A browser automation sprint
- A performance-testing sprint
- A general refactoring sprint

Only work directly supporting **Authorization Handler Testing + Authorization Boundary Hardening** should be considered Sprint 12 scope.

---

## 13. Deferred and Future Work

### 13.1 Deferred from Sprint 11 → Sprint 12

| Item | Classification | Reasoning |
|------|---------------|-----------|
| T06: Authorization Handler Tests | **Sprint 12** | Core objective — directly supports Sprint 12 objective |
| WebApplicationFactory integration tests | **Later Sprint** | Valuable but introduces DI wiring complexity; should be planned separately |
| Razor Page authorization integration tests | **Later Sprint** | Depends on WebApplicationFactory |
| CI provider establishment | **Later Sprint** | Valuable but outside authorization testing scope |

### 13.2 Future Work Classification

| Item | Classification | Reasoning |
|------|---------------|-----------|
| Browser automation (Playwright) | **Backlog** | No current evidence justifies the infrastructure investment |
| Code coverage reporting and gates | **Backlog** | Premature; current tests are behavior-focused |
| Mutation testing | **Backlog** | Low current risk profile does not justify the tooling |
| Performance/load testing | **Backlog** | Not a current gap |
| IdentitySeeder / user-group assignment tests | **Backlog** | Seed data already verified by T07 integration tests |
| SQL Server integration testing | **Backlog** | InMemory coverage is adequate for current authorization logic |
| Broader repository test coverage | **Backlog** | Lower-risk modules; not the highest priority |
| FluentValidation test coverage | **Backlog** | Valid work but not authorization-specific |
| Sales Module | **Later Sprint** | Major new business capability; not authorization-related |
| Audit Logging | **Later Sprint** | Compliance feature; not authorization-specific |
| REST API | **Later Sprint** | Separate architectural concern |
| Additional Reporting features | **Later Sprint** | Outside authorization scope |

### 13.3 Items Explicitly Not Promoted to Sprint 12

The following items appeared in roadmap or previous sprint documentation but do NOT belong in Sprint 12:

- Administrator/Users/EditStatus dead-code cleanup: Code quality item, not an authorization defect. Optionally included as cleanup, but not required for the authorization objective.
- Reports authorization (unrestricted by design): This is an intentional design decision (DD-032), not a defect. No action required.
- InventoryManagement OR-composite broad access: This is the intended behavior of the authorization model. It is not a defect.

---

## 14. Risks and Mitigations

### Risk 1 — Testing Only Authorization Handlers

**Risk:** Page-level authorization regressions remain undetected.

**Mitigation:** Add lightweight authorization metadata/boundary regression tests that verify `[Authorize]` attributes are present and correctly configured on PageModels.

### Risk 2 — Invalid Test-Project Dependency

**Risk:** Web authorization tests introduce an inappropriate dependency into existing UnitTests or IntegrationTests.

**Mitigation:** Use a dedicated `InventoryPlatform.Web.Tests` project that maintains clear dependency boundaries. Verify no test-project cross-references are introduced.

### Risk 3 — Over-Engineered Test Infrastructure

**Risk:** Testing becomes more complicated than the authorization logic.

**Mitigation:** Prefer xUnit, direct handler invocation, and hand-written fakes. No mocking framework. No DI container. No WebApplicationFactory.

### Risk 4 — WebApplicationFactory Scope Creep

**Risk:** Sprint expands into full integration testing.

**Mitigation:** Keep WebApplicationFactory explicitly deferred. Handler unit tests do not require it.

### Risk 5 — Historical Documentation Confusion

**Risk:** Previous "resolved" findings are interpreted as proof that all related source issues are fixed.

**Mitigation:** Verify current source state and explicitly reconcile historical/current differences. This document explicitly addresses the discrepancy between "All P1 findings resolved" and the two persistent source-level defects.

### Risk 6 — Scope Creep

**Risk:** Unrelated roadmap work enters Sprint 12.

**Mitigation:** Require every task to directly support the Sprint 12 objective. Non-authorization work is explicitly excluded.

### Risk 7 — Handler Tests Reveal Unexpected Behavior

**Risk:** Handler tests uncover behavior that complicates the testing approach.

**Mitigation:** Handlers are thin wrappers around `ICapabilityAuthorizationService`. Their logic is straightforward. If unexpected behavior is discovered, it will be documented and addressed within Sprint 12 scope.

---

## 15. Verification Baseline

Sprint 12 implementation must demonstrate:

### Existing Tests

- UnitTests remain passing (219 tests)
- IntegrationTests remain passing (61 tests)
- No regression from the Sprint 11 baseline

### New Tests

- CapabilityAuthorizationHandler tests pass
- MultiCapabilityAuthorizationHandler tests pass
- Page authorization-boundary tests pass

### Authorization Fixes

- Categories/Edit uses `AuthorizationPolicies.InventoryManagement`
- Suppliers/Create uses `AuthorizationPolicies.InventoryManagement`

### Build

- 0 build errors
- 0 build warnings

### Architecture

- No invalid project dependency
- No unnecessary production abstraction
- No database dependency in pure handler tests
- No unauthorized testing infrastructure expansion

### Scope

- No unrelated feature work
- No accidental promotion of deferred work

---

## 16. High-Level Acceptance Criteria

Sprint 12 should ultimately demonstrate:

1. `CapabilityAuthorizationHandler` has meaningful automated tests
2. `MultiCapabilityAuthorizationHandler` has meaningful automated tests
3. The Web test architecture respects existing project boundaries
4. Categories/Edit uses `AuthorizationPolicies.InventoryManagement`
5. Suppliers/Create uses `AuthorizationPolicies.InventoryManagement`
6. Page-level authorization declarations have appropriate regression protection
7. Existing Sprint 11 tests remain passing (280 total)
8. Build remains clean (0 errors, 0 warnings)
9. No WebApplicationFactory/full HTTP testing has been silently introduced
10. Deferred work remains explicitly documented
11. No unrelated feature scope has entered the sprint
12. Documentation accurately reflects actual Sprint 12 status

These are high-level acceptance criteria. The subsequent Task Breakdown must convert them into task-level acceptance criteria.

---

## 17. Sprint 12 Status

**SPRINT 12 STATUS: PLANNED — IMPLEMENTATION NOT YET STARTED**

- No code has been written
- No tests have been created
- No defects have been fixed
- No project files have been modified
- No documentation has been changed in the source tree
- The retrospective/setup document is being created as the initial planning artifact

---

## 18. Planning Gate

The following conditions are evaluated to determine whether Sprint 12 has a sufficiently clear foundation for the next planning step:

| Condition | Status |
|-----------|--------|
| Sprint objective is clear | ✅ Authorization Handler Testing + Authorization Boundary Hardening |
| Sprint 11 baseline is established | ✅ 280 tests, 0 failures, 0 build errors |
| Relevant Sprint 11 lessons are captured | ✅ Architecture, testing, documentation, test doubles |
| Current authorization findings are identified | ✅ 2 confirmed defects, 1 dead code |
| Categories/Edit defect is addressed | ✅ In scope (T05) |
| Suppliers/Create defect is addressed | ✅ In scope (T06) |
| Handler testing scope is clear | ✅ Both handlers, with fake service |
| Page-level authorization-boundary testing is addressed | ✅ In scope (reflection/metadata approach) |
| WebApplicationFactory remains appropriately bounded | ✅ Explicitly deferred |
| Non-goals are explicit | ✅ 13 explicit non-goals |
| Future work is classified | ✅ Sprint 12, Later Sprint, Backlog |
| Architecture constraints are clear | ✅ Existing conventions preserved |
| Verification expectations are measurable | ✅ 12 acceptance criteria |
| No implementation has been performed | ✅ Documentation only |

All conditions are satisfied.

> **SPRINT 12 RETROSPECTIVE/SETUP: APPROVED - READY FOR TASK BREAKDOWN**

---

## 19. Next Required Artifact

The next required artifact is:

**`SPRINT_12_TASK_BREAKDOWN.md`**

This document must convert the high-level acceptance criteria into detailed task-level specifications with exact files, exact changes, exact verification steps, and exact dependency chains.

The Task Breakdown must receive an explicit:

> **SPRINT 12 TASK BREAKDOWN: APPROVED - READY FOR T01 EXECUTION**

before any T01 implementation begins.

---

## 20. Task Execution Record — T03 (CapabilityAuthorizationHandlerTests)

> **T03 STATUS: COMPLETE** — recorded below with factual execution results only.

| Field | Value |
|-------|-------|
| **Task** | T03 — Create `CapabilityAuthorizationHandlerTests` |
| **Status** | COMPLETE |
| **Depends on** | T02 (`FakeCapabilityAuthorizationService`) |
| **Test file created** | `tests/InventoryPlatform.Web.Tests/Authorization/CapabilityAuthorizationHandlerTests.cs` |
| **Tests added** | 8 |
| **Production source changes** | None |

### 20.1 Test File Created

`tests/InventoryPlatform.Web.Tests/Authorization/CapabilityAuthorizationHandlerTests.cs` —
xUnit tests constructing `CapabilityAuthorizationHandler` directly with the T02
hand-written `FakeCapabilityAuthorizationService`, a real `ClaimsPrincipal`/`ClaimsIdentity`,
and a real `CapabilityRequirement` inside an `AuthorizationHandlerContext`. No mocking
framework, no database, no WebApplicationFactory.

### 20.2 Behavior Covered

Verified against the actual `CapabilityAuthorizationHandler` source behavior:

1. Authenticated user whose (userId, capability) pair is authorized → requirement succeeds.
2. Authenticated user lacking the required capability (granted only to another user) → not succeeded; service is consulted with the principal's user ID.
3. Service explicitly returns `false` for the user/capability → handler does not succeed.
4. Handler passes the authenticated identity's `ClaimTypes.NameIdentifier` GUID value as the user ID to `ICapabilityAuthorizationService`.
5. Handler requests the requirement's capability name (not a different granted capability) and does not succeed on an unrelated grant.
6. Not-authenticated identity (even carrying a valid NameIdentifier claim) → returns before consulting the service; no service call.
7. Authenticated identity with no NameIdentifier claim → no service call; not succeeded.
8. Authenticated identity with a non-GUID NameIdentifier value → no service call; not succeeded.

`MultiCapabilityAuthorizationHandler` was NOT tested in T03 (belongs to T04).

### 20.3 Test Counts

| Project | Before T03 | After T03 |
|---------|-----------|----------|
| UnitTests | 219 | 219 |
| IntegrationTests | 61 | 61 |
| Web.Tests | 13 | 21 |
| **Total** | **293** | **301** |

All 301 tests passed, 0 failed, 0 skipped.

### 20.4 Validation Results

```text
dotnet build InventoryPlatform.slnx
  Build succeeded.
  0 Warning(s)
  0 Error(s)

dotnet test (per project)
  UnitTests:       219 passed, 0 failed
  IntegrationTests: 61 passed, 0 failed
  Web.Tests:        21 passed, 0 failed
```

### 20.5 Architecture / Dependency Verification

- `InventoryPlatform.Web.Tests.csproj` references only `InventoryPlatform.Web` (unchanged).
- No reference to `InventoryPlatform.UnitTests` or `InventoryPlatform.IntegrationTests`; no cross-test-project dependency introduced.
- No new NuGet packages added (xUnit only, matching existing test projects).
- UnitTests/IntegrationTests csproj files unchanged (no Web reference).

### 20.6 Deviations / Notes

- The retrospective top banner still reads "PLANNED" and earlier planning sections were intentionally left untouched (only additive T03 facts recorded here).
- `docs/TESTING_CONVENTIONS.md` still lists T03/T04 as "planned"; per task scope, documentation synchronization is deferred to the Sprint 12 documentation-closure task.
- T04 (`MultiCapabilityAuthorizationHandlerTests`) was NOT implemented.

### 20.7 Next Task

T04 — Create `MultiCapabilityAuthorizationHandlerTests` (not started; see governing rules).

---

## Appendix A — Source Files Verified

### Authorization Handlers and Requirements

| File | Finding |
|------|---------|
| `src/InventoryPlatform/InventoryPlatform.Web/Authorization/CapabilityAuthorizationHandler.cs` | Handler under test — `public sealed`, accepts `ICapabilityAuthorizationService` |
| `src/InventoryPlatform/InventoryPlatform.Web/Authorization/MultiCapabilityAuthorizationHandler.cs` | Handler under test — `public sealed`, accepts `ICapabilityAuthorizationService` |
| `src/InventoryPlatform/InventoryPlatform.Web/Authorization/AuthorizationPolicies.cs` | Policy constants (Administrator, InventoryManagement, ViewInventory, PurchaseOrder, capability subclasses) |
| `src/InventoryPlatform/InventoryPlatform.Web/Authorization/CapabilityRequirement.cs` | Single-capability requirement type |
| `src/InventoryPlatform/InventoryPlatform.Web/Authorization/MultiCapabilityRequirement.cs` | Multi-capability requirement type |
| `src/InventoryPlatform/InventoryPlatform.Web/Authorization/CapabilityAuthorizationExtensions.cs` | Policy registration extensions |
| `src/InventoryPlatform/InventoryPlatform.Web/Extensions/ServiceCollectionExtensions.cs` | Policy wiring |
| `src/InventoryPlatform/InventoryPlatform.Application/Interfaces/Authorization/ICapabilityAuthorizationService.cs` | Service interface (single method: `HasCapabilityAsync`) |
| `src/InventoryPlatform/InventoryPlatform.Application/Authorization/CapabilityAuthorizationService.cs` | Concrete service implementation |
| `src/InventoryPlatform/InventoryPlatform.Infrastructure/Identity/IdentityConstants.cs` | Role constants |

### Defect Files

| File | Finding |
|------|---------|
| `src/InventoryPlatform/InventoryPlatform.Web/Pages/Categories/Edit.cshtml.cs` | Missing `[Authorize]` — CONFIRMED DEFECT |
| `src/InventoryPlatform/InventoryPlatform.Web/Pages/Suppliers/Create.cshtml.cs` | Uses `ViewInventory` instead of `InventoryManagement` — CONFIRMED DEFECT |
| `src/InventoryPlatform/InventoryPlatform.Web/Pages/Administrator/Users/EditStatus.cshtml.cs` | Dead-code `IsInRole` behind Administrator policy — NOT A DEFECT |

### Comparison PageModels (for policy consistency verification)

| File | Policy |
|------|--------|
| `Pages/Categories/Create.cshtml.cs` | `InventoryManagement` |
| `Pages/Suppliers/Edit.cshtml.cs` | `InventoryManagement` |
| `Pages/Customers/Create.cshtml.cs` | `InventoryManagement` |
| `Pages/Products/Create.cshtml.cs` | `InventoryManagement` |
| `Pages/Products/Edit.cshtml.cs` | `InventoryManagement` |
| `Pages/InventoryTransactions/Create.cshtml.cs` | `InventoryManagement` |

### Test Infrastructure

| File | Finding |
|------|---------|
| `tests/InventoryPlatform.UnitTests/InventoryPlatform.UnitTests.csproj` | References Domain, Application, Shared. No Web reference. |
| `tests/InventoryPlatform.IntegrationTests/InventoryPlatform.IntegrationTests.csproj` | References Domain, Application, Infrastructure, Shared. No Web reference. |
| `tests/InventoryPlatform.UnitTests/Application/CapabilityAuthorizationServiceTests.cs` | Existing test patterns: hand-written fakes, behavior naming, direct construction |
| `src/InventoryPlatform/InventoryPlatform.Web/InventoryPlatform.Web.csproj` | Web project — references Application, Infrastructure, Shared |
| `src/InventoryPlatform/InventoryPlatform.slnx` | Solution file — 5 production + 2 test projects |

### Documentation

| Document | Relevance |
|----------|-----------|
| `SPRINT_12_PLANNING_REPORT.md` | Sprint 12 planning findings and recommendations |
| `SPRINT_11_TASK_BREAKDOWN.md` | Sprint 11 scope, T06 deferral, test baseline |
| `docs/retrospectives/SPRINT_11_AUTOMATED_TESTING.md` | Sprint 11 outcomes, test counts, T06 deferral |
| `docs/TESTING_CONVENTIONS.md` | Test architecture and conventions |
| `docs/ENGINEERING_JOURNAL.md` | Sprint 11 summary, deferred work |
| `docs/DESIGN_DECISIONS.md` | Authorization architecture decisions (DD-032, DD-039 through DD-047) |
| `ROADMAP.md` | Sprint 11 completion, Sprint 12 direction |
| `README.md` | Project status and features |

---

*Document created as the initial Sprint 12 retrospective/setup artifact.*
*This document will be updated at the end of Sprint 12 with actual implementation results.*
