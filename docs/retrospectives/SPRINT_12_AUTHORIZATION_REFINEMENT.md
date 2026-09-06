# Sprint 12 Retrospective/Setup — Authorization Refinement

---

> **SPRINT 12 STATUS: COMPLETED — CLOSED (T09)** — Implementation and verification complete (T01–T06, T08, T09); T07 remains BLOCKED/deferred (see Sections 24 and 26). Sections 1–19 below are preserved as the historical planning artifact; later task-execution records (Sections 20–26) are authoritative for actual outcomes.

---

## 1. Sprint 12 Identity

| Field | Value |
|-------|-------|
| **Sprint** | 12 |
| **Sprint Name** | Authorization Refinement |
| **Repository** | Inventory Management Platform |
| **Branch** | `feature/authorization_refinement` |
| **Sprint Objective** | Authorization Handler Testing + Authorization Boundary Hardening |
| **Status** | COMPLETED — CLOSED (final status; see Section 26). Planning-era status: PLANNED — IMPLEMENTATION NOT YET STARTED |

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

**Source evidence (planning-era assessment — SUPERSEDED by Section 24.2 and the corrected classification below):** The class-level `[Authorize(Policy = AuthorizationPolicies.Administrator)]` was believed to mean only Administrators can reach `OnPostAsync`, making the `IsInRole` check on line 61 appear to be unreachable dead code. Sprint 12 source inspection disproved this: the guard is reachable for supported multi-role users.

**Documented state:**
- Sprint 10 T10 (line 32): "Code-level IsInRole: 1 occurrence in EditStatus.cshtml.cs line 61 — unreachable dead code"
- Sprint 10 T10 (line 46): "EditStatus.cshtml.cs IsInRole: Unreachable dead code."
- Sprint 10 T12 (line 18): "One dead-code User.IsInRole(...) occurrence in EditStatus.cshtml.cs was documented but intentionally left unchanged (unreachable behind class-level [Authorize(Policy = Administrator)])."

**Classification (planning-era, SUPERSEDED):** This planning-era classification was invalidated by Sprint 12 T07 source inspection: the `IsInRole` block is a reachable, behavior-affecting self-deactivation guard for supported multi-role users, NOT dead/unreachable code. Section 24.2 of this document supersedes this assessment with current-source evidence; T07 is BLOCKED/deferred, not completed.

---

## 6. Current Sprint 12 Findings

### 6.1 Source vs Documentation Reconciliation

The Sprint 10 retrospective states "All P1 findings resolved." This statement requires clarification:

- P1 findings at the **database level** (InventoryManager seed having Administration.Access, Viewer seed having Supplier.Create) **were resolved** by T15 Phase 21/25 remediation.
- P1 findings at the **source code level** (Categories/Edit missing attribute, Suppliers/Create wrong policy, EditStatus "dead code" — label superseded by Section 24.2: the EditStatus guard is reachable, not dead) **were NOT resolved** — they were documented as pre-existing issues and explicitly deferred as "Future" work.

The retrospective's "All P1 findings resolved" claim is accurate only for the database-level remediation scope. The source-level defects were never within T15's remediation scope.

### 6.2 Current Authorization State

```text
Page-level [Authorize(Policy)] attributes:     50 total (planning-era)
  - Correct:                                    48
  - Missing:                                     1 (Categories/Edit)   → REMEDIATED in Sprint 12 (T05)
  - Wrong policy:                                1 (Suppliers/Create)   → REMEDIATED in Sprint 12 (T06)

Razor UI IAuthorizationService.AuthorizeAsync:  21 checks (all correct, migrated in Sprint 10 T12)

RequireRole usages:                              0
Explicit Forbid() calls:                         0

Authorization handlers:                           2 (CapabilityAuthorizationHandler, MultiCapabilityAuthorizationHandler)
Handler automated tests:                          0 at planning time (the gap Sprint 12 addressed)
                                                  → ESTABLISHED in Sprint 12 (T03: 8 tests, T04: 12 tests; passing)
```

**Sprint 12 outcome note (authoritative):** Categories/Edit now requires `AuthorizationPolicies.InventoryManagement` (T05); Suppliers/Create now requires `AuthorizationPolicies.InventoryManagement` with zero `ViewInventory` occurrences (T06); Web authorization-handler tests exist and pass (T03/T04). The single remaining `User.IsInRole` occurrence in `Pages/Administrator/Users/EditStatus.cshtml.cs` (line 61) is reachable and behavior-affecting — NOT dead code; T07 is blocked/deferred.

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
| T07 | Remove EditStatus.cshtml.cs dead-code IsInRole (optional cleanup) | Code cleanup | Optional — BLOCKED: premise disproved; guard is reachable (Section 24) |
| T08 | Verify all handler tests pass, build succeeds, regression confirmed | Validation | Verification |
| T09 | Documentation synchronization and Sprint 12 closure | Documentation | Documentation |

Final Sprint 12 task outcomes: T01–T06 COMPLETE, T07 BLOCKED/DEFERRED, T08 COMPLETE, T09 COMPLETE (Section 26).

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

- Administrator/Users/EditStatus "dead-code" cleanup: initially classified as a code-quality item, not an authorization defect (optionally included as cleanup). Planning-era classification; SUPERSEDED by Sprint 12 T07 source inspection (Section 24.2) — the block is a reachable, behavior-affecting self-deactivation guard, and the optional cleanup task was blocked rather than completed.
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

**SPRINT 12 STATUS (planning-era): PLANNED — IMPLEMENTATION NOT YET STARTED** — superseded by the task-execution records (Sections 20–26); the sprint is COMPLETED/CLOSED with T07 blocked/deferred.

- No code had been written at planning time
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
| Current authorization findings are identified | ✅ 2 confirmed defects, 1 "dead code" (label superseded by Section 24.2: reachable, not dead) |
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

## 21. Task Execution Record — T04 (MultiCapabilityAuthorizationHandlerTests)

> **T04 STATUS: COMPLETE** — recorded below with factual execution results only.

| Field | Value |
|-------|-------|
| **Task** | T04 — Create `MultiCapabilityAuthorizationHandlerTests` |
| **Status** | COMPLETE |
| **Depends on** | T02 (`FakeCapabilityAuthorizationService`), T03 (test conventions) |
| **Test file created** | `tests/InventoryPlatform.Web.Tests/Authorization/MultiCapabilityAuthorizationHandlerTests.cs` |
| **Tests added** | 12 |
| **Production source changes** | None |

### 21.1 Test File Created

`tests/InventoryPlatform.Web.Tests/Authorization/MultiCapabilityAuthorizationHandlerTests.cs` —
xUnit tests constructing `MultiCapabilityAuthorizationHandler` directly with the T02
hand-written `FakeCapabilityAuthorizationService`, a real `ClaimsPrincipal`/`ClaimsIdentity`,
and a real `MultiCapabilityRequirement` inside an `AuthorizationHandlerContext`. No mocking
framework, no database, no WebApplicationFactory.

### 21.2 Behavior Covered

Verified against the actual `MultiCapabilityAuthorizationHandler` source behavior
(OR semantics confirmed from source: the handler iterates the requirement's capability
list and calls `context.Succeed` on the first granted capability, then returns):

1. OR semantics — first capability granted, second denied → requirement succeeds; the
   handler short-circuits (exactly 1 service call).
2. OR semantics — first denied, second granted → requirement succeeds; 2 service calls,
   proving the handler evaluates more than the first capability.
3. OR semantics — three capabilities, only the third granted → requirement succeeds;
   3 service calls.
4. All capabilities denied → requirement does not succeed (context neither succeeded
   nor failed); all 3 capabilities are evaluated.
5. Capabilities granted only to a different user → requirement does not succeed; the
   handler passes the principal's own user ID (`ClaimTypes.NameIdentifier` GUID) to the
   service (`LastRequestedUserId` verified).
6. Not-authenticated identity (even carrying a valid NameIdentifier claim) → returns
   before consulting the service; no service call.
7. Authenticated identity with no NameIdentifier claim → no service call; not succeeded.
8. Authenticated identity with a non-GUID NameIdentifier value → no service call; not succeeded.
9. `MultiCapabilityRequirement` with valid names → stores the capability names in order.
10. `MultiCapabilityRequirement` with `null` names → throws `ArgumentException`.
11. `MultiCapabilityRequirement` with an empty list → throws `ArgumentException`.
12. `MultiCapabilityRequirement` with a whitespace capability name → throws `ArgumentException`.

`CapabilityAuthorizationHandler` was NOT re-tested in T04 (belongs to T03); the T03 test
file remains unchanged.

### 21.3 Test Counts

| Project | Before T04 | After T04 |
|---------|-----------|----------|
| UnitTests | 219 | 219 |
| IntegrationTests | 61 | 61 |
| Web.Tests | 21 | 33 |
| **Total** | **301** | **313** |

All 313 tests passed, 0 failed, 0 skipped.

### 21.4 Validation Results

```text
dotnet build InventoryPlatform.slnx
  Build succeeded.
  0 Warning(s)
  0 Error(s)

dotnet test (per project)
  UnitTests:       219 passed, 0 failed
  IntegrationTests: 61 passed, 0 failed
  Web.Tests:        33 passed, 0 failed
  (of which MultiCapabilityAuthorizationHandlerTests: 12 passed)
```

### 21.5 Architecture / Dependency Verification

- `InventoryPlatform.Web.Tests.csproj` references only `InventoryPlatform.Web` (unchanged).
- No reference to `InventoryPlatform.UnitTests` or `InventoryPlatform.IntegrationTests`; no cross-test-project dependency introduced.
- No new NuGet packages added (xUnit only, matching existing test projects).
- UnitTests/IntegrationTests csproj files unchanged (no Web reference).
- No production source files changed.

### 21.6 Deviations / Notes

- The actual `MultiCapabilityAuthorizationHandler` implements OR semantics exactly as
  documented in the task breakdown (first granted capability satisfies the requirement);
  no AND-semantics behavior was found or tested.
- The handler's unauthenticated / missing-claim / invalid-GUID branches are duplicated
  in the two handlers (separate production code paths); T04 covers them for the
  multi-capability handler without modifying either handler.
- The retrospective top banner still reads "PLANNED" and earlier planning sections were
  intentionally left untouched (only additive T04 facts recorded here).
- `docs/TESTING_CONVENTIONS.md` still lists T03/T04 as "planned"; per task scope,
  documentation synchronization is deferred to the Sprint 12 documentation-closure task.
- T05 (`Categories/Edit` authorization remediation) was NOT implemented.

### 21.7 Next Task

T05 — Remediate Categories/Edit (`[Authorize(Policy = InventoryManagement)]`);
not started (see governing rules).

---

## 22. Task Execution Record — T05 (Categories/Edit Authorization Remediation)

> **T05 STATUS: COMPLETE** — recorded below with factual execution results only.

| Field | Value |
|-------|-------|
| **Task** | T05 — Remediate Categories/Edit — add `[Authorize(Policy = InventoryManagement)]` |
| **Status** | COMPLETE |
| **Depends on** | T03/T04 (test conventions; no blocking dependency for the production change) |
| **Production file changed** | `src/InventoryPlatform/InventoryPlatform.Web/Pages/Categories/Edit.cshtml.cs` |
| **Tests added** | 0 (T05 is a production authorization fix; test scope excludes new test projects, HTTP-pipeline testing, and WebApplicationFactory) |
| **Git operations** | None |

### 22.1 Defect Confirmation (Current Source, Pre-Implementation)

Before modification, the current repository source was inspected and confirmed the defect:

- `Pages/Categories/Edit.cshtml.cs` (`EditModel`) had **no `[Authorize]` attribute** and **no authorization imports** (`InventoryPlatform.Web.Authorization`, `Microsoft.AspNetCore.Authorization` were absent).
- No fallback authorization policy is registered (`options.FallbackPolicy` unused), and the Razor `Conventions.AuthorizeFolder` registrations cover only `/Products`, `/Administration`, and `/Inventory` — `/Categories` is NOT covered by any folder convention. The page was therefore genuinely reachable by any authenticated user (including Viewer) through direct URL access.
- Neighboring PageModels confirm the intended boundary: `Categories/Create` → `InventoryManagement`; `Categories/Activate` and `Categories/Deactivate` → `Administrator`; `Units/Edit`, `Suppliers/Edit`, `Products/Edit`, `Customers/Edit`, `Products/Create`, `Customers/Create`, `InventoryTransactions/Create` → all `InventoryManagement`.
- The `InventoryManagement` policy already exists (`AuthorizationPolicies.InventoryManagement`, `nameof`-based constant) and is registered via the multi-capability OR-composite overload with the 9 `InventoryManagementCapabilities.*` capabilities in `Extensions/ServiceCollectionExtensions.cs`. No new policy or capability was needed.

### 22.2 Exact Production Change

`src/InventoryPlatform/InventoryPlatform.Web/Pages/Categories/Edit.cshtml.cs` — two additive edits, matching the established Web project convention exactly:

1. Added the two imports used by every other protected PageModel:
   - `using InventoryPlatform.Web.Authorization;`
   - `using Microsoft.AspNetCore.Authorization;`
2. Added the class-level attribute directly above `public class EditModel : PageModel`:
   - `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]`

No other production change was required. No handler, requirement, policy, capability, seed data, Application, Domain, Infrastructure, or database/migration file was touched.

### 22.3 Test Counts

| Project | Before T05 | After T05 |
|---------|-----------|----------|
| UnitTests | 219 | 219 |
| IntegrationTests | 61 | 61 |
| Web.Tests | 33 | 33 |
| **Total** | **313** | **313** |

All 313 tests passed, 0 failed, 0 skipped. The T05 pre-T05 baseline stated in the task (219 / 61 / 33 = 313) matched the actual repository state exactly.

### 22.4 Validation Results

```text
dotnet build src/InventoryPlatform/InventoryPlatform.slnx
  Build succeeded.
  20 Warning(s) — all pre-existing CS8601/CS8602 nullable-reference warnings in
  unrelated Web PageModels (Pages/Account/*, Pages/Administrator/Users/*,
  Pages/Reports/PurchaseHistory/*); zero warnings reference Categories/Edit.
  0 Error(s)

dotnet test (per project)
  UnitTests:       219 passed, 0 failed, 0 skipped
  IntegrationTests: 61 passed, 0 failed, 0 skipped
  Web.Tests:        33 passed, 0 failed, 0 skipped
  Total:           313 passed
```

Note: the T03/T04 records above report "0 Warning(s)" builds; the current build reports 20 warnings, all in files untouched since (and unrelated to) T05. Actual current-repository results are treated as authoritative per task rules.

### 22.5 Authorization / Architecture Verification

- `Categories/Edit.cshtml.cs` now requires `AuthorizationPolicies.InventoryManagement` (verified by source inspection: the attribute is present at the class level; both authorization imports added).
- Capability-based authorization remains the only enforcement mechanism: the attribute references the existing registered policy; no handler, requirement, or registration was changed.
- No role-based authorization introduced: no `RequireRole`, no `Roles =`, no new role checks in the changed file.
- No `User.IsInRole` introduced: the single pre-existing occurrence (then classified "dead code"; superseded by Section 24.2 — reachable) remains only in `Administrator/Users/EditStatus.cshtml.cs` line 61 (T07 scope, intentionally untouched).
- No new policy created: `AuthorizationPolicies.cs` unchanged; no new `AddPolicy`/`AddCapabilityPolicy` registration.
- No new capability created: `InventoryManagementCapabilities`/`ViewInventoryCapabilities` unchanged; seed data untouched.
- Unrelated authorization boundaries unchanged: `Suppliers/Create.cshtml.cs` still uses `ViewInventory` (T06 scope, verified in source); `Units/Create.cshtml.cs` still uses `Administrator` (verified in source); EditStatus dead `IsInRole` unchanged (T07 scope, verified in source).
- Authorization logic remains in the Web/PageModel layer; no authorization logic moved into Application, Domain, or Infrastructure; no direct repository/DbContext access introduced.

### 22.6 Deviations / Notes

- Build warnings: 20 pre-existing nullable-reference warnings observed in the current build (see 22.4); none caused by T05 and none in Categories/Edit. Recorded as an authoritative current-state fact rather than silently reporting the historical "0 warnings".
- Test project location note: the three test projects live under the repository-root `tests/` directory (not under `src/InventoryPlatform/`), which required running `dotnet test` from the repository root. No files were moved.
- No new test was added for this change. The Sprint 12 planning explicitly bounds T05 to the production attribute change; reflection/metadata boundary tests (`PageAuthorizationBoundaryTests`) remain part of the sprint's separate boundary-testing task, not T05.
- The retrospective top banner and planning sections were intentionally left untouched (only additive T05 facts recorded here), consistent with T03/T04 practice.
- `SPRINT_12_TASK_BREAKDOWN.md` was NOT modified.
- T06 (`Suppliers/Create` policy change) was NOT implemented.

### 22.7 Next Task

T06 — Remediate Suppliers/Create (change `ViewInventory` to `InventoryManagement`); not started (see governing rules).

---

## 23. Task Execution Record — T06 (Suppliers/Create Authorization Remediation)

> **T06 STATUS: COMPLETE** — recorded below with factual execution results only.

| Field | Value |
|-------|-------|
| **Task** | T06 — Remediate Suppliers/Create — change `ViewInventory` to `InventoryManagement` |
| **Status** | COMPLETE |
| **Depends on** | T05 (scope verification confirmed the defect; no blocking dependency for the production change) |
| **Production file changed** | `src/InventoryPlatform/InventoryPlatform.Web/Pages/Suppliers/Create.cshtml.cs` |
| **Tests added** | 0 (T06 is a production policy-alignment fix; test scope excludes new test projects, HTTP-pipeline testing, and WebApplicationFactory) |
| **Git operations** | None |

### 23.1 Defect Confirmation (Current Source, Pre-Implementation)

Before modification, the current repository source was inspected and confirmed the defect:

- `Pages/Suppliers/Create.cshtml.cs` (`CreateModel`) used `[Authorize(Policy = AuthorizationPolicies.ViewInventory)]` at class level (line 9). Both required imports (`InventoryPlatform.Web.Authorization`, `Microsoft.AspNetCore.Authorization`) were already present.
- `Pages/Suppliers/Create.cshtml` contains no authorization logic of its own (pure markup); the class-level PageModel attribute is the only server-side boundary.
- Neighboring evidence confirms the mismatch: the equivalent mutation page `Suppliers/Edit` → `InventoryManagement`; view pages `Suppliers/Index` and `Suppliers/Details` → `ViewInventory` (correct for read operations); `Suppliers/Activate` and `Suppliers/Deactivate` → `Administrator`.
- The `InventoryManagement` policy is the intended target and already exists: it is registered via the multi-capability OR-composite overload and includes `SupplierCreate` (`InventoryManagementCapabilities.SupplierCreate`) among its 9 capabilities (`Extensions/ServiceCollectionExtensions.cs`). No new policy or capability was needed.
- Effect of the defect: any authenticated user holding any single ViewInventory capability (e.g., a Viewer with `Category.View`) could create suppliers server-side through direct URL access, despite the UI button being hidden.

### 23.2 Exact Production Change

`src/InventoryPlatform/InventoryPlatform.Web/Pages/Suppliers/Create.cshtml.cs` — one-line policy swap at the class level:

```text
- [Authorize(Policy = AuthorizationPolicies.ViewInventory)]
+ [Authorize(Policy = AuthorizationPolicies.InventoryManagement)]
```

No other change of any kind. No import was added or removed. The supplier creation workflow, `CreateSupplierHandler` call, validation, persistence, TempData/redirect behavior, and the Razor markup (`Create.cshtml`) were left untouched. No additional production change was required.

### 23.3 Test Counts

| Project | Before T06 | After T06 |
|---------|-----------|----------|
| UnitTests | 219 | 219 |
| IntegrationTests | 61 | 61 |
| Web.Tests | 33 | 33 |
| **Total** | **313** | **313** |

All 313 tests passed, 0 failed, 0 skipped. The pre-T06 baseline stated in the task (219 / 61 / 33 = 313) matched the actual repository state exactly.

### 23.4 Validation Results

```text
dotnet build src/InventoryPlatform/InventoryPlatform.slnx
  Build succeeded.
  20 Warning(s) — identical pre-existing CS8601/CS8602 nullable-reference warning set
  recorded during T05 (Pages/Account/*, Pages/Administrator/Users/*,
  Pages/Reports/PurchaseHistory/*); zero warnings reference Suppliers or the
  changed file. 0 warnings introduced by T06.
  0 Error(s)

dotnet test (per project)
  UnitTests:       219 passed, 0 failed, 0 skipped
  IntegrationTests: 61 passed, 0 failed, 0 skipped
  Web.Tests:        33 passed, 0 failed, 0 skipped
  Total:           313 passed
```

### 23.5 Authorization / Architecture Verification

- `Suppliers/Create.cshtml.cs` now requires `AuthorizationPolicies.InventoryManagement` (verified by source inspection, line 9).
- `AuthorizationPolicies.ViewInventory` is no longer referenced anywhere in `Suppliers/Create.cshtml.cs` (verified: 0 occurrences).
- Capability-based authorization remains the only enforcement mechanism: the attribute references the existing registered capability policy; no handler, requirement, registration, or seed data was changed.
- No role-based authorization introduced: no `RequireRole`, no `Roles =`, no new role checks in the changed file; no role names hard-coded.
- No `User.IsInRole` introduced: the single pre-existing occurrence (then classified "dead code"; superseded by Section 24.2 — reachable) remains only in `Administrator/Users/EditStatus.cshtml.cs` line 61 (T07 scope, intentionally untouched).
- No new policy created: `AuthorizationPolicies.cs` unchanged; no new `AddPolicy`/`AddCapabilityPolicy` registration.
- No new capability created: capability constants and seed data unchanged.
- Unrelated authorization boundaries unchanged (verified in source): `Categories/Edit` retains the T05 fix (`InventoryManagement`); `Units/Create` unchanged (`Administrator`); `Suppliers/Index` and `Suppliers/Details` unchanged (`ViewInventory`, correct for read pages); EditStatus dead `IsInRole` unchanged (T07 scope).
- Intended semantics achieved without role names: Administrator/InventoryManager users holding any `InventoryManagement` composite capability (including `Supplier.Create`) are allowed; view-only users are denied; unauthenticated users are handled by the existing authentication pipeline (`RequireAuthenticatedUser` inside the registered policy).
- Authorization logic remains in the Web/PageModel layer; no authorization logic moved into Application, Domain, or Infrastructure; no direct repository/DbContext access introduced; no new package/dependency.

### 23.6 Deviations / Notes

- None. Build-warning count (20) matched the T05 record exactly; no new warnings were introduced. Test counts matched the stated baseline exactly.
- The retrospective top banner and planning sections were intentionally left untouched (only additive T06 facts recorded here), consistent with T03/T04/T05 practice.
- `SPRINT_12_TASK_BREAKDOWN.md` was NOT modified.
- T07 (EditStatus `IsInRole` cleanup — premise later disproved; reachable guard, not dead code) was NOT implemented.

### 23.7 Next Task

T07 — Remove EditStatus.cshtml.cs "dead-code" `IsInRole` (optional cleanup; premise later disproved by source inspection — the guard is reachable, see Section 24); not started at T06 time.

---

## 24. Task Execution Record — T07 (EditStatus Dead-Code `IsInRole` Removal)

> **T07 STATUS: BLOCKED — PRE-IMPLEMENTATION PRECONDITION FAILED; NO CODE CHANGED** — recorded below with factual execution results only.

| Field | Value |
|-------|-------|
| **Task** | T07 — Remove EditStatus.cshtml.cs dead-code `IsInRole` (optional cleanup) |
| **Status** | BLOCKED (stop condition from the task's own pre-implementation rules) |
| **Production source changes** | None — the removal precondition ("role check is redundant/dead") could not be confirmed from current source |
| **Files changed by T07** | This retrospective document only (discrepancy record) |
| **Git operations** | None |

### 24.1 Current-Source Confirmation of the Targeted Code

The targeted occurrence exists as documented: `Pages/Administrator/Users/EditStatus.cshtml.cs` line 61, inside `OnPostAsync`:

```csharp
var user = await _getUserHandler.HandleAsync(Input.Id);
var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

if (User.IsInRole(customRoleIdentity.IdentityConstants.Roles.InventoryManager))
{
    if (user.Value.Id.ToString() == currentUserId)
    {
        return Page();
    }
}
```

The PageModel is class-level protected by `[Authorize(Policy = AuthorizationPolicies.Administrator)]` (line 22, unchanged). It is the only `User.IsInRole` occurrence in the entire Web project (search result: 1 occurrence).

### 24.2 Why the "Dead / Unreachable" Premise Is NOT Confirmed

The historical classification (Sprint 10 T10/T12, Sections 5.3 and 6.2 of this document) describes the block as "unreachable dead code" because the page requires the Administrator policy. Current-source reachability analysis refutes that premise:

1. **Nothing enforces single-role assignment.** `IdentityService.CreateUserAsync` uses `AddToRolesAsync(user, request.Roles)` (plural, unvalidated beyond `CreateUserValidator` requiring ≥ 1 role) and `UpdateUserRolesAsync` uses `RemoveFromRolesAsync`/`AddToRolesAsync` with no single-role restriction. No FluentValidation validator exists for `UpdateUserRolesRequest`.
2. **The EditRoles UI allows selecting multiple roles.** `EditRoles.cshtml` renders one checkbox per role (Administrator, InventoryManager, Viewer) with no mutual-exclusion constraint, and `EditRolesModel.OnPostAsync` submits the full selection.
3. **Role claims are functional.** `.AddIdentity<LocalIdentity.ApplicationUser, IdentityRole<Guid>>` is registered; Identity populates role claims on sign-in, so `User.IsInRole(...)` evaluates real claims at runtime.
4. **Authorization-group membership is independent of Identity roles.** `IdentitySeeder.AssignUsersToGroupsAsync` maps seeded users to capability groups by e-mail, and no code path synchronizes the Administrator authorization-group membership with the absence of the InventoryManager Identity role. Therefore "satisfies the Administrator capability policy" does NOT imply "lacks the InventoryManager role claim".

Consequently a user holding BOTH the Administrator role (or Administrator authorization-group membership) AND the InventoryManager role is constructible through supported application flows, and for that user the `IsInRole` branch IS reachable. The block is a **reachable business-rule guard** (self-deactivation prevention), not dead authorization code:

- Condition 3 of the task ("the role check does not provide an independent authorization boundary that is required for correctness") — NOT satisfied.
- Condition 4 ("removing it will not weaken authorization") — NOT satisfied: removing the whole block removes a reachable self-deactivation protection for dual-role users (a plain Administrator without the manager role is not blocked by the current guard, so the guard is additionally inconsistent).

### 24.3 Action Taken

Per the task's own instruction — "If any of these are not true, do not remove the code. Report the discrepancy and stop before broadening scope" — the code was NOT removed and no production file was modified. Alternative re-scoping options (e.g., unwrapping the role condition to make the self-deactivation guard unconditional, or removing the entire block) each change observable behavior and were NOT implemented without a user decision.

### 24.4 Validation Results (Repository Unchanged, Confirmed Green)

```text
dotnet build src/InventoryPlatform/InventoryPlatform.slnx
  Build succeeded.
  20 Warning(s) — the same pre-existing CS8601/CS8602 nullable-reference set
  recorded during T05/T06 (including 3 in EditStatus.cshtml.cs itself); none
  introduced by T07 (no source was modified).
  0 Error(s)

dotnet test (per project)
  UnitTests:       219 passed, 0 failed, 0 skipped
  IntegrationTests: 61 passed, 0 failed, 0 skipped
  Web.Tests:        33 passed, 0 failed, 0 skipped
  Total:           313 passed
```

### 24.5 Authorization / Scope Verification

- `EditStatus` retains `[Authorize(Policy = AuthorizationPolicies.Administrator)]` — unchanged.
- `IsInRole` occurrences in the Web project: 1 (unchanged; the targeted line 61 remains).
- Categories/Edit (T05 result), Suppliers/Create (T06 result), Units/Create, policy registration, capability constants, seed data, handlers, requirements: all unchanged (verified in source during inspection).
- No new policy, capability, role check, package, or dependency introduced. Application, Domain, Infrastructure, database/migrations untouched.

### 24.6 Deviations / Discoveries

- **Deviation from plan (blocker):** the task's core premise ("unreachable dead code") is contradicted by current source; the block is a reachable, role-conditioned self-deactivation guard. T07 cannot be completed as specified without changing authorization/business semantics.
- **Guard inconsistency discovered:** the self-deactivation protection only applies when the operator also holds the InventoryManager role; a plain Administrator can deactivate their own account. This is a pre-existing behavior observation, recorded only — not fixed (out of T07 scope).
- **Documentation discrepancy:** Sections 5.3 and 6.2 (and the T03/T04 execution records) repeat the "dead/unreachable" classification; Section 24.2 supersedes that assessment with current-source evidence.
- `SPRINT_12_TASK_BREAKDOWN.md` was NOT modified. T08 and later tasks NOT implemented.

### 24.7 Next Step

Requires a user decision on T07 disposition before any Sprint 12 task is executed: (a) defer/skip T07, (b) re-scope T07 as an explicit behavior-changing remediation (e.g., unconditional self-deactivation guard), or (c) remove the block accepting the documented semantic change. No next task is auto-selected.

---

## 25. Task Execution Record — T08 (Authorization Refinement Integrated Verification)

> **T08 STATUS: COMPLETE — VERIFICATION-ONLY; NO CODE OR TEST CHANGES** — recorded below with factual execution results only.

| Field | Value |
|-------|-------|
| **Task** | T08 — Authorization Refinement Integrated Verification |
| **Status** | COMPLETE |
| **Depends on** | T03, T04 (handler tests), T05, T06 (boundary remediations), T07 (deferred — verified unchanged) |
| **Production source changes** | None |
| **Test source changes** | None — no tests added, modified, or weakened |
| **Files changed by T08** | This retrospective document only (additive T08 record) |
| **Git operations** | One read-only `git status` probe attempted (process deviation — see 25.11); it failed because the workspace is not a Git repository. No Git state was changed; no branch, staging, commit, push, merge, tag, release, or Git-history operation occurred. |

### 25.1 Pre-Verification Source Findings (Current Source, Authoritative)

Current source was inspected before running any verification, per task rules:

- **CapabilityAuthorizationHandler** (`src/InventoryPlatform/InventoryPlatform.Web/Authorization/CapabilityAuthorizationHandler.cs`): `public sealed`, `AuthorizationHandler<CapabilityRequirement>`; authentication gate → `ClaimTypes.NameIdentifier` extraction → `Guid.TryParse` → `HasCapabilityAsync(userId, requirement.CapabilityName)` → `context.Succeed` on `true`; no `context.Fail` anywhere. Consistent with T03 tests.
- **MultiCapabilityAuthorizationHandler** (`.../Authorization/MultiCapabilityAuthorizationHandler.cs`): same gate/claim/parsing pattern; iterates `requirement.CapabilityNames` and calls `context.Succeed` on the first granted capability, then returns (OR semantics with short-circuit). Consistent with T04 tests.
- **CapabilityRequirement**: single `CapabilityName`, constructor rejects null/whitespace. **MultiCapabilityRequirement**: `params string[]`, constructor rejects null/empty arrays and null/whitespace names; stores an ordered copy as `IReadOnlyList<string>`.
- **Categories/Edit** (`Pages/Categories/Edit.cshtml.cs`): class-level `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]` present, with both authorization imports. T05 remediation preserved. No role-based attribute, no new policy.
- **Suppliers/Create** (`Pages/Suppliers/Create.cshtml.cs`): class-level `[Authorize(Policy = AuthorizationPolicies.InventoryManagement)]`; zero occurrences of `ViewInventory` in the file. T06 remediation preserved. No role-based replacement.
- **EditStatus** (`Pages/Administrator/Users/EditStatus.cshtml.cs`): class-level `[Authorize(Policy = AuthorizationPolicies.Administrator)]` unchanged; the `User.IsInRole(customRoleIdentity.IdentityConstants.Roles.InventoryManager)` self-deactivation guard remains in `OnPostAsync`. **T07 remains BLOCKED/deferred; untouched by T08; the occurrence is NOT dead code** (reachable for dual-role users, per Section 24.2).
- **Policy registration** (`Extensions/ServiceCollectionExtensions.cs`): `Administrator` → single-capability `Administration.Access`; `InventoryManagement` → multi-capability OR-composite (9 capabilities incl. `Supplier.Create`); `ViewInventory` → multi-capability OR-composite (7 capabilities); PurchaseOrder capability policies → `ForCapability(...)` single-capability registrations; Razor conventions authorize `/Products`, `/Administration` (Administrator), `/Inventory` (InventoryManagement). All registrations intact; no new policy added.
- `docs/TESTING_CONVENTIONS.md` inspected: it still describes the Web.Tests coverage table at its pre-T03/T04 state (13 tests) and lists T03–T07 as "planned/deferred"; per prior task practice, documentation synchronization is deferred to T09 (Sprint 12 closure). Recorded as a known documentation discrepancy, not corrected under T08.

### 25.2 Authorization Source Scan (Focused, Web Project)

| Pattern | Count | Location(s) |
|---------|-------|-------------|
| `User.IsInRole` (Web project) | 1 | `Pages/Administrator/Users/EditStatus.cshtml.cs` line 61 — the known remaining occurrence; reachable, NOT dead code |
| `RequireRole` (all of `src/InventoryPlatform`) | 0 | — |
| `[Authorize(Roles = ...)]` (Web project) | 0 | — |
| Categories/Edit policy | `InventoryManagement` | line 10 of `Edit.cshtml.cs` |
| Suppliers/Create policy | `InventoryManagement` | line 9 of `Create.cshtml.cs` |
| Suppliers/Create `ViewInventory` | 0 occurrences | file remediated by T06 |
| Capability-policy registrations | intact | `AddCapabilityPolicy` usages in `Extensions/ServiceCollectionExtensions.cs` unchanged |

Additional pattern results (context, findings only): `AuthorizationPolicies.ViewInventory` remains referenced by the read-only pages `Suppliers/Index.cshtml.cs`, `Suppliers/Details.cshtml.cs`, `Categories/Index.cshtml.cs`, `Categories/Details.cshtml.cs` (correct for view operations). No authorization regression found.

### 25.3 T03 Handler-Test Verification

`CapabilityAuthorizationHandlerTests` (8 tests, all passing) exercise the actual production handler directly:

- authenticated user with allowed capability → succeeds; capability name passed correctly (grants keyed to the requirement's capability, not an unrelated one);
- denied capability / explicit `false` service result → does not succeed, does not fail;
- NameIdentifier extraction: principal's NameIdentifier GUID is passed to the service (`LastRequestedUserId` verified);
- unauthenticated principal, missing NameIdentifier claim, invalid-GUID NameIdentifier → no service call, not succeeded.

Failures: none. Production handler unchanged; no rewrite performed.

### 25.4 T04 Multi-Handler-Test Verification

`MultiCapabilityAuthorizationHandlerTests` (12 tests, all passing) exercise the actual production handler directly:

- OR semantics: first capability granted → succeeds with exactly 1 service call (short-circuit after first success); first denied/second granted → succeeds with 2 calls; third-of-three granted → succeeds with 3 calls;
- denial when all capabilities return `false` → not succeeded (context neither succeeded nor failed; all capabilities evaluated);
- correct user ID usage: grants keyed to another user's GUID do not satisfy the requirement; the principal's own user ID is passed;
- unauthenticated principal, missing NameIdentifier, invalid GUID → no service call, not succeeded;
- requirement constructor validation: valid names stored in order; null, empty, and whitespace inputs throw `ArgumentException`.

Failures: none. OR semantics unchanged ("succeed on any allowed capability"); no semantics change made.

### 25.5 T05/T06 Boundary Verification

- **Categories/Edit:** still requires `AuthorizationPolicies.InventoryManagement`; not unprotected; no role-based attribute; no new policy created. T05 remediation intact.
- **Suppliers/Create:** still requires `AuthorizationPolicies.InventoryManagement`; `ViewInventory` no longer present; no role-based replacement. T06 remediation intact.

### 25.6 T07 Deferred Status — Preserved

Inspected, not modified. The PageModel remains protected by `AuthorizationPolicies.Administrator`; the remaining `User.IsInRole(...)` guard still exists and remains behavior-affecting (reachable self-deactivation prevention for dual-role users). T08 does NOT claim the occurrence is dead/unreachable code. T07 remains unresolved/deferred pending a user decision (Section 24.7); T08 made no change to it.

### 25.7 Build Verification

```text
dotnet build src/InventoryPlatform/InventoryPlatform.slnx            (incremental)
  Build succeeded. 0 Warning(s), 0 Error(s).

dotnet build src/InventoryPlatform/InventoryPlatform.slnx --no-incremental
  Build succeeded. 28 Warning(s), 0 Error(s).
```

Warning-count note: the incremental build reports 0 warnings (already-compiled projects are not re-warned); a full rebuild reports 28 warnings. The T05–T07 records state 20 warnings, which matched their builds' reporting scope. The 8 additional warnings are pre-existing, in Application/Infrastructure files outside the Web project (`GetInventoryTransactionsResponse.cs` CS8618 ×2, `AccountService.cs` CS8604 ×3, `ApplicationDBContext.cs`/`InventoryTransactionRepository.cs` CS0114 ×3). All 28 warnings are pre-existing; zero were introduced by T08 (no source was modified). Unrelated warning cleanup remains out of scope.

### 25.8 Test Results

```text
dotnet test (per project, and full solution run)
  UnitTests:       219 passed, 0 failed, 0 skipped
  IntegrationTests: 61 passed, 0 failed, 0 skipped
  Web.Tests:        33 passed, 0 failed, 0 skipped
  Total:           313 passed, 0 failed, 0 skipped
```

Per-class Web.Tests counts: `CapabilityAuthorizationHandlerTests` = 8 passed; `MultiCapabilityAuthorizationHandlerTests` = 12 passed; `FakeCapabilityAuthorizationServiceTests` = 12 passed; remaining 1 = project placeholder.

No tests were added or modified during T08 — this is expected: T08 is an integrated verification task, not a test-infrastructure task.

### 25.9 Regression Assessment

- T03 handler behavior: intact (production source consistent with tests; tests pass).
- T04 multi-handler behavior: intact (OR semantics with short-circuit preserved; tests pass).
- T05 Categories/Edit remediation: intact.
- T06 Suppliers/Create remediation: intact (`ViewInventory` absent).
- T07 deferred behavior: unchanged (guard preserved, task still blocked/deferred).
- Authorization regression: none discovered. No new role-based authorization (`RequireRole`, `[Authorize(Roles=...)]`, new `User.IsInRole`) anywhere in the Web project.

### 25.10 Deviations / Discoveries

- **Warning-count difference (documentation, not regression):** full-solution rebuild reports 28 warnings vs the documented 20. The 8 additional warnings are pre-existing and located in Application/Infrastructure (outside the Web project and outside all Sprint 12-touched files). Recorded as an authoritative current-state fact; no cleanup performed.
- **`SPRINT_12_TASK_BREAKDOWN.md` intentionally absent from the repository** (searched repository root and `docs/`): it is an external planning/control artifact, not a repository file. Nothing to preserve or modify; the "do not modify" constraint was satisfied vacuously. Neither this file nor any replacement was created.
- `docs/TESTING_CONVENTIONS.md` still shows pre-T03/T04 coverage figures (13 Web.Tests) and lists T03–T07 as planned/deferred; intentionally not synchronized under T08 (documentation closure belongs to T09). Not changed during T08 or during this correction.
- No production, test, or dependency changes of any kind were required or made by T08.

### 25.11 Process Deviation — Read-Only Git Status Probe

- One read-only `git status --porcelain && git diff --stat` probe was attempted after verification, to confirm the changed-file list.
- It failed immediately with `fatal: not a git repository (or any of the parent directories): .git` — the workspace is not a Git repository.
- **Classification: non-state-changing process deviation only.** Because the command failed before touching any Git metadata and was read-only in intent, no Git state was read or changed: no branch create/switch, no staging, no commit, no push, no merge, no tag, no release, and no Git-history inspection or modification occurred.
- **Impact: none on the technical verification result.** All build, test, and source-scan results were produced before the probe and are unaffected.
- Consequence for acceptance criteria: the T08 rule "No Git operation is performed" was not literally satisfied, so **Acceptance Criterion 23 is recorded as not literally met** (22 of 23 criteria satisfied; the sole miss is this process deviation). No other criterion is affected.

### 25.12 Next Task

T09 — Documentation synchronization and Sprint 12 closure (not started at T08 time; see governing rules). T08 did not implement it.

---

## 26. Task Execution Record — T09 (Sprint 12 Documentation Synchronization and Closure)

> **T09 STATUS: COMPLETE — DOCUMENTATION-ONLY** — recorded below with factual execution results only.

| Field | Value |
|-------|-------|
| **Task** | T09 — Sprint 12 Documentation Synchronization and Closure |
| **Status** | COMPLETE |
| **Depends on** | T01–T06, T08 (completed records), T07 (blocked/deferred — preserved) |
| **Production source changes** | None |
| **Test source changes** | None |
| **Git operations** | None |

### 26.1 Documentation Synchronized

- **`docs/TESTING_CONVENTIONS.md`** — synchronized to the current test architecture: Web.Tests 33-test baseline (fake 12 + T03 8 + T04 12 + placeholder 1), total 313; handler testing marked COMPLETED (no longer planned/deferred); file-structure tree extended with both handler-test files; deferred section replaced with the factual Sprint 12 outcome (completed T03–T06/T08, T07 blocked/deferred with the reachable-guard explanation). No HTTP-pipeline or browser-testing coverage is claimed.
- **`docs/retrospectives/SPRINT_12_AUTHORIZATION_REFINEMENT.md`** — sprint status finalized (top banner and Section 17 annotated as COMPLETED/CLOSED, preserving the historical planning text); planning-era "dead/unreachable" classifications in Sections 5.3 and 6.2 explicitly superseded (marking the historical evidence quotes as historical while stating the current-source conclusion); scope table (8.1) and Appendix A defect table annotated with final outcomes; T08 record's Categories/Edit scan-row line number corrected (line 10, not 9); this T09 record added.
- **`PROJECT_STATUS.md`** — Sprint 12 Authorization Refinement section added (Complete, with T07 Blocked/Deferred); project status line and Current Focus updated to Sprint 12 with the 313-test baseline; test conventions line updated to name all three test projects.
- **`README.md`** — Current Development Status updated to Sprint 12 Complete with the 313-test baseline; Completed Modules list now includes Web authorization-handler testing (Sprint 12) and removes the resolved Categories/Edit item from Known Deferred Items; Roadmap Current section updated (Sprint 12 complete, T07 deferred item recorded, 313-test baseline). No new version/release/tag invented — v1.6.0 remains the current release baseline and Sprint 12 is documented as a non-release sprint (tests and authorization fixes only).
- **`ROADMAP.md`** — version banner extended with `v1.7 Sprint 12 Authorization Testing ✅` (status marker only); Sprint 12 section added (Completed / Blocked-Deferred / Final Test Baseline); "Next Sprint Planning" updated from the stale "next locked priority is Sprint 12" to "Sprint 12 is complete; the next activity is a separate Sprint Planning session". No Sprint 13 scope invented.
- **`CHANGELOG.md`** — `[Sprint 12]` entry added at the top following the established `[Sprint N]` entry convention (Sprint 11 precedent), documenting only actual completed changes (Web.Tests infrastructure, T03/T04 handler tests, T05/T06 boundary fixes, T08 verification) plus the unresolved EditStatus behavior under Known/Deferred. No release date, version, or tag invented.
- **`docs/ENGINEERING_JOURNAL.md`** — Current Release State updated to Sprint 12; a Sprint 12 section added with the key evidence-driven lesson: the EditStatus `User.IsInRole` block was previously classified as dead/unreachable, but source inspection showed it is reachable for supported multi-role users and therefore behavior-affecting — recorded as an engineering finding, not a remediation.
- **`docs/DESIGN_DECISIONS.md`** — NOT modified. Sprint 12 introduced no new architectural/design decision: the T07 blocker is a discovered discrepancy, not a design change; the capability-based authorization model, handlers, requirements, and policy-registration approach are unchanged.

### 26.2 Final Verified Baseline (Authoritative, Re-Executed After Documentation Changes)

```text
dotnet build src/InventoryPlatform/InventoryPlatform.slnx            (incremental)
  Build succeeded. 0 Warning(s), 0 Error(s).

dotnet build src/InventoryPlatform/InventoryPlatform.slnx --no-incremental
  Build succeeded. 28 Warning(s), 0 Error(s).  — all 28 pre-existing; none introduced
  (8 beyond the historical 20-warning records are Application/Infrastructure warnings
  surfaced by full-rebuild reporting scope; see Section 25.7)

dotnet test (full solution run)
  UnitTests:       219 passed, 0 failed, 0 skipped
  IntegrationTests: 61 passed, 0 failed, 0 skipped
  Web.Tests:        33 passed, 0 failed, 0 skipped
  Total:           313 passed, 0 failed, 0 skipped
```

### 26.3 T07 Disposition — Preserved

T07 remains BLOCKED/DEFERRED. The remaining `User.IsInRole(InventoryManager)` occurrence in `Pages/Administrator/Users/EditStatus.cshtml.cs` (line 61) is a reachable, behavior-affecting self-deactivation guard for supported multi-role users — NOT dead/unreachable code. No documentation produced by T09 describes it as dead code, claims it was removed, reports the `User.IsInRole` count as zero, or invents a remediation decision. The count remains 1 in the Web project; `RequireRole` = 0; `[Authorize(Roles = ...)]` = 0.

### 26.4 T08 Process Deviation — Preserved

The T08 record (Section 25.11) is preserved verbatim: one read-only `git status` probe was attempted during T08, failed because the workspace was not a Git repository, changed no Git state, and is classified as a non-state-changing process deviation that did not affect the technical verification result. No Git command was executed during T09.

### 26.5 Scope Verification

- No production source, test source, project file, package reference, database/migration, authorization policy, handler, requirement, PageModel, or CI configuration was modified by T09.
- `SPRINT_12_TASK_BREAKDOWN.md` was NOT created or modified — it is intentionally maintained outside the repository as a planning/control artifact and is not treated as a missing repository document.
- No Sprint 13 scope invented; no release/version/tag invented; no later task implemented.
- Validation was re-executed after all documentation edits (results in 26.2), confirming the documentation-only nature of T09.

### 26.6 Sprint 12 Closure

Sprint 12 achieved its core objective — Authorization Handler Testing + Authorization Boundary Hardening — despite the T07 cleanup assumption being invalidated by current-source evidence: both authorization handlers are covered by passing automated tests via a reusable hand-written fake (T02–T04), both confirmed authorization-boundary defects are remediated with the existing capability-based `InventoryManagement` policy (T05/T06), integrated verification passed with no authorization regression (T08), and documentation is synchronized (T09). The blocked T07 cleanup is a deferred code-quality item that requires an explicit behavioral decision, not a closure blocker for the approved sprint scope. **SPRINT 12 IS CLOSED.**

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
| `src/InventoryPlatform/InventoryPlatform.Web/Pages/Categories/Edit.cshtml.cs` | Missing `[Authorize]` — CONFIRMED DEFECT at planning time → REMEDIATED by T05 (now `[Authorize(Policy = InventoryManagement)]`) |
| `src/InventoryPlatform/InventoryPlatform.Web/Pages/Suppliers/Create.cshtml.cs` | Uses `ViewInventory` instead of `InventoryManagement` — CONFIRMED DEFECT at planning time → REMEDIATED by T06 (now `[Authorize(Policy = InventoryManagement)]`; 0 `ViewInventory` occurrences) |
| `src/InventoryPlatform/InventoryPlatform.Web/Pages/Administrator/Users/EditStatus.cshtml.cs` | `IsInRole` behind Administrator policy — planning-era "NOT A DEFECT / dead code" classification SUPERSEDED by Section 24: reachable, behavior-affecting self-deactivation guard; T07 BLOCKED/deferred |

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
