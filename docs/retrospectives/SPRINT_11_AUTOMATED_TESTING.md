# Sprint 11 Retrospective — Automated Testing & Test Automation

## Sprint Overview

**Sprint:** 11
**Purpose:** Establish automated testing infrastructure and create regression protection for the highest-risk behaviors in InventoryPlatform.
**Approved objective:** Automated Testing & Test Automation
**Planned scope:** Test infrastructure, domain unit tests, authorization service tests, authorization seed integration tests, authorization repository integration tests, CI preparation, test conventions documentation
**Starting baseline:** v1.6.0 — Dynamic Capability-Based Authorization. Zero automated test infrastructure.

---

## Planned Tasks

| Task | Name | Status |
|------|------|--------|
| T01 | Test Infrastructure Foundation | COMPLETE |
| T02 | PurchaseOrder Domain Tests | COMPLETE |
| T03 | Product Domain Tests | COMPLETE |
| T04 | Authorization Domain Tests | COMPLETE |
| T05 | CapabilityAuthorizationService Tests | COMPLETE |
| T06 | Authorization Handler Tests | DEFERRED TO SPRINT 12 |
| T07 | AuthorizationSeeder Integration Tests | NOT STARTED |
| T08 | Authorization Repository Integration Tests | NOT STARTED |
| T09 | CI / Automated Test Execution | NOT STARTED |
| T10 | Test Conventions and Sprint Documentation | NOT STARTED |
| T11 | Documentation Synchronization and Sprint Closure | NOT STARTED |

---

## Starting State

Before Sprint 11:
- No automated test infrastructure existed
- No test projects
- No test files
- No test packages
- No CI/CD configuration
- Five production projects (Shared, Domain, Application, Infrastructure, Web)
- Clean Architecture dependency boundaries intact
- All verification performed through manual browser testing and source-level inspection

---

## T01 — Test Infrastructure Foundation

**Status: COMPLETE**

### Implementation

Created two test projects:

```text
tests/InventoryPlatform.UnitTests/
tests/InventoryPlatform.IntegrationTests/
```

Both added to `src/InventoryPlatform/InventoryPlatform.slnx`.

### Dependency Boundaries (Verified)

**UnitTests:**
```text
InventoryPlatform.UnitTests
    -> InventoryPlatform.Domain
    -> InventoryPlatform.Application
    -> InventoryPlatform.Shared
```

No reference to Infrastructure or Web. No EF Core dependency.

**IntegrationTests:**
```text
InventoryPlatform.IntegrationTests
    -> InventoryPlatform.Infrastructure
    -> InventoryPlatform.Application
    -> InventoryPlatform.Domain
    -> InventoryPlatform.Shared
```

No reference to Web.

No test-project cross-references.

### Packages

**UnitTests:**
- Microsoft.NET.Test.Sdk 17.*
- xunit 2.*
- xunit.runner.visualstudio 2.*

**IntegrationTests:**
- Microsoft.NET.Test.Sdk 17.*
- xunit 2.*
- xunit.runner.visualstudio 2.*
- Microsoft.EntityFrameworkCore.InMemory 10.0.*

### Validation

```text
dotnet restore          SUCCESS
dotnet build            SUCCESS (0 errors, 0 warnings)
dotnet test             SUCCESS (2 tests, 2 passed, 0 failed)
```

### Production source changes: NONE

### T01 Notes

Initial build failed with CS0246 (`FactAttribute` not found) because `using Xunit;` was missing from placeholder test files. xUnit's `[Fact]` attribute is not included in implicit usings. Corrected by adding the namespace import. This was a test-file authoring issue, not an architectural deviation.

---

## Architecture Decisions

| Decision | Value | Rationale |
|----------|-------|-----------|
| Test project count | 2 (UnitTests, IntegrationTests) | Clean separation of database-free vs database-dependent tests |
| Test framework | xUnit | Default for .NET 10, good ASP.NET Core integration |
| Mocking | Hand-written fakes | Interfaces are simple; Moq premature for Sprint 11 scope |
| EF testing | InMemory | Adequate for authorization queries; no Docker/Testcontainers available |
| Web handler tests | Deferred to Sprint 12 | Handlers live in Web; unit test architecture prohibits Web reference |
| CI | Conditional on GitHub hosting | No CI provider confirmed in repository |

---

## Authorization Seed Baseline

Verified from source (`AuthorizationSeeder.CapabilityCatalog`):

```text
Capabilities:                  39
Administrator:                 39
InventoryManager:              21
Viewer:                        12
Total group-capability relationships:  72
```

These are intentional regression-test baselines for T07.

---

## Risks

| Risk | Mitigation |
|------|-----------|
| InMemory is not SQL Server | Document limitations; test query correctness not constraints |
| No Web-layer automated coverage until Sprint 12 | T05 covers core authorization logic; handler integration deferred |
| CI provider not verified | T09 is conditional; do not invent CI provider |
| Future test coverage must remain behavior-focused | T10 documents conventions |
| Test suite must not become coupled to implementation details | Prefer behavior-based assertions |

---

## Problems / Deviations

No unresolved deviations.

T01 had a minor test-file authoring issue (`using Xunit;` missing) which was corrected during T01 implementation. Classified as a corrected authoring issue, not an architectural deviation.

---

## Lessons Learned

Sprint 11 observations:

- xUnit `[Fact]` requires the `using Xunit;` namespace import; it is not included in implicit usings
- Project-level test infrastructure can be validated independently before substantive test coverage is added
- Dependency boundaries can be verified directly from `.csproj` files after project creation
- PurchaseOrder domain is well-suited for direct unit testing — no mocking or infrastructure needed
- PurchaseOrderItem behavior is tightly coupled to PurchaseOrder receiving workflow and benefits from dedicated test coverage
- State transition testing (Draft→Submitted→Approved→Receiving→Completed) is high-value for regression protection

---

## Deferred Work

### T06 — Authorization Handler Tests → Sprint 12

The authorization handlers (`CapabilityAuthorizationHandler`, `MultiCapabilityAuthorizationHandler`) live in `InventoryPlatform.Web.Authorization`. Testing them requires a Web project reference, which violates the approved Sprint 11 test architecture.

Sprint 11 does NOT move authorization handlers to another production project.

### Sprint 12+ Candidates

- WebApplicationFactory integration tests
- Full authorization pipeline tests
- Browser automation (Playwright)
- Code coverage gates
- Performance testing
- Mutation testing

---

## T02 — PurchaseOrder Domain Tests

**Status: COMPLETE**

### Implementation

Created two test files:

```text
tests/InventoryPlatform.UnitTests/Domain/Purchasing/PurchaseOrderTests.cs
tests/InventoryPlatform.UnitTests/Domain/Purchasing/PurchaseOrderItemTests.cs
```

### Behaviors Covered

**PurchaseOrder:**
- Creation (valid parameters, initial state, Status=Draft)
- AddItem (valid items, multiple items, TotalAmount calculation, duplicate product rejection, zero/negative quantity rejection, negative unit cost rejection, zero unit cost allowed)
- UpdateItem (quantity/cost updates, nonexistent item rejection, invalid updates)
- RemoveItem (removal, TotalAmount update, nonexistent item rejection)
- Submit (Draft→Submitted, no-items rejection, already-submitted rejection)
- Approve (Submitted→Approved, wrong-state rejections)
- Receive (partial receiving, full receiving, completion detection, invalid state rejections, quantity validation)
- Invalid state transitions (AddItem/UpdateItem/RemoveItem from non-Draft states)
- Domain invariants (TotalAmount calculation, Items is read-only, computed properties)

**PurchaseOrderItem:**
- Creation (all properties, initial ReceivedQuantity=0)
- Computed properties (LineTotal, RemainingQuantity, IsFullyReceived)
- Receive (valid quantities, accumulation, zero/negative rejection, over-receive rejection)
- Update (quantity/cost updates, validation, zero unit cost allowed)

### Validation

```text
dotnet build            SUCCESS (0 errors, 0 warnings)
dotnet test             SUCCESS (102 tests, 102 passed, 0 failed)
```

101 new T02 PurchaseOrder domain tests + 1 T01 placeholder test = 102 total in UnitTests.

### Production source changes: NONE

---

## T03 — Product Domain Tests

**Status: COMPLETE**

### Implementation

Created one test file:

```text
tests/InventoryPlatform.UnitTests/Domain/Products/ProductTests.cs
```

### Behaviors Covered

**Product Creation:**
- Constructor with valid parameters creates product correctly
- Constructor rejects invalid/null SKU
- Constructor rejects invalid/null Name
- Constructor rejects negative CostPrice
- Constructor rejects negative SellingPrice
- Constructor rejects zero/negative CategoryId
- Constructor rejects zero/negative UnitId
- Constructor rejects negative QuantityOnHand
- Zero prices and zero quantity allowed where applicable

**Rename:**
- Valid name updates Name
- Empty/null name throws ArgumentException

**ChangeCategory:**
- Valid ID updates CategoryId
- Zero/negative IDs throw ArgumentOutOfRangeException

**ChangeUnit:**
- Valid ID updates UnitId
- Zero/negative IDs throw ArgumentOutOfRangeException

**AdjustStock:**
- Positive quantity increases QuantityOnHand
- Negative quantity decreases QuantityOnHand
- Zero quantity allowed
- Negative adjustment exceeding stock throws ArgumentOutOfRangeException
- Failed adjustment does not mutate state

**IncreaseStock:**
- Valid quantity increases stock
- Zero/negative quantities throw ArgumentOutOfRangeException
- Multiple calls accumulate correctly

**DecreaseStock:**
- Valid quantity decreases stock
- Decrease to exactly zero succeeds
- Quantity exceeding available stock throws ArgumentOutOfRangeException
- Zero/negative quantities throw ArgumentOutOfRangeException
- Failed decrease does not mutate state

**CanDecreaseStock:**
- Returns true when quantity available
- Returns true when quantity equals available
- Returns false when quantity exceeds available
- Returns false when no stock

**ChangeCostPrice / ChangeSellingPrice:**
- Valid values update prices
- Zero prices allowed
- Negative prices throw ArgumentOutOfRangeException

**Activation / Deactivation:**
- Deactivate sets IsActive to false
- Activate sets IsActive to true

**Barcode / Description:**
- ChangeBarcode updates value
- ChangeBarcode(null) sets null
- UpdateDescription updates value
- UpdateDescription(null) sets null

### Validation

```text
dotnet build            SUCCESS (0 errors, 0 warnings)
dotnet test             SUCCESS (159 tests, 159 passed, 0 failed)
```

56 new T03 Product domain tests + 101 T02 tests + 1 T01 placeholder test = 158 in UnitTests. 1 in IntegrationTests.

### Production source changes: NONE

### T03 Notes

Product domain uses Guard-based validation (ArgumentException/ArgumentOutOfRangeException) rather than DomainException. Tests assert the actual exception types from the production code.

---

## T04 — Authorization Domain Tests

**Status: COMPLETE**

### Implementation

Created two test files:

```text
tests/InventoryPlatform.UnitTests/Domain/Authorization/CapabilityTests.cs
tests/InventoryPlatform.UnitTests/Domain/Authorization/AuthorizationGroupTests.cs
```

### Behaviors Covered

**Capability:**
- Construction with valid name
- Rejects empty/null/whitespace name
- Default state: IsEnabled=true
- Rename updates name
- Rename rejects empty/null name
- Rename preserves IsEnabled state
- Enable when disabled enables
- Enable when already enabled is idempotent
- Disable when enabled disables
- Disable when already disabled is idempotent
- Disable then Enable returns to enabled
- GroupCapabilities initially empty
- GroupCapabilities is read-only

**AuthorizationGroup:**
- Construction with valid name
- Rejects empty/null/whitespace name
- Rename updates name
- Rename rejects empty/null name

**AuthorizationGroup — Capability Management:**
- AddCapability with valid capability adds to group
- AddCapability with null throws ArgumentNullException
- AddCapability with unpersisted capability (Id=0) throws DomainException
- AddCapability duplicate capability is idempotent
- AddCapability multiple different capabilities all added
- AddCapability relationship reflects correct group and capability IDs
- AddCapability updates capability's GroupCapabilities collection
- RemoveCapability existing capability removes from group
- RemoveCapability nonexistent capability ID does nothing
- RemoveCapability with zero/negative ID throws ArgumentOutOfRangeException
- RemoveCapability only removes specified capability

**AuthorizationGroup — User Management:**
- AssignUser with valid user ID adds user to group
- AssignUser with empty GUID throws ArgumentException
- AssignUser duplicate user is idempotent
- AssignUser multiple different users all added
- AssignUser relationship reflects correct user and group IDs
- RemoveUser existing user removes from group
- RemoveUser nonexistent user does nothing
- RemoveUser with empty GUID throws ArgumentException
- RemoveUser only removes specified user

**Collections:**
- Capabilities is read-only
- UserGroups is read-only

### Test Design Notes

- Tests that need persisted entity IDs (for relationship creation) use reflection to simulate a persisted BaseEntity.Id, since the group/capability relationship entities require positive IDs.
- A static `_nextId` counter ensures each capability gets a unique simulated ID to avoid false duplicate detection.
- All relationship tests go through the public AuthorizationGroup API (AddCapability, AssignUser) which internally creates AuthorizationGroupCapability and UserAuthorizationGroup via their internal factory methods.

### Validation

```text
dotnet build            SUCCESS (0 errors, 0 warnings)
dotnet test             SUCCESS (205 tests, 205 passed, 0 failed)
```

46 new T04 authorization domain tests + 158 previous UnitTests + 1 IntegrationTests placeholder = 205 total.

### Production source changes: NONE

---

## T05 — CapabilityAuthorizationService Tests

**Status: COMPLETE**

### Implementation

Created one test file:

```text
tests/InventoryPlatform.UnitTests/Application/CapabilityAuthorizationServiceTests.cs
```

Also created two hand-written fake implementations (test-project-only):

- `FakeCapabilityRepository` — implements `ICapabilityRepository`
- `FakeAuthorizationGroupRepository` — implements `IAuthorizationGroupRepository`

No mocking framework was added.

### Service Behaviors Tested

**Capability Granted:**
- User with enabled capability in one of their groups → returns true

**Capability Not Granted:**
- Capability exists but user's groups don't contain it → returns false

**Disabled Capability:**
- Matching capability exists but is disabled → returns false

**Multiple Groups:**
- Capability in second group (not first) → returns true (multi-group union)
- Capability absent from all user's groups → returns false

**Multiple Capabilities Within a Group:**
- Group with multiple capabilities returns correct result

**User With No Groups:**
- User belongs to no authorization groups → returns false

**Empty/Invalid Inputs:**
- Empty user ID → returns false
- Capability not found → returns false
- Null capability name → returns false
- Empty capability name → returns false
- Whitespace capability name → returns false

**Repository Interaction:**
- GetByNameAsync receives the correct capability name
- GetForUserAsync receives the correct user ID
- When capability not found, GetForUserAsync is NOT called (short-circuit)

### Test Double Strategy

- Hand-written fakes for `ICapabilityRepository` and `IAuthorizationGroupRepository`
- No mocking framework added
- Fakes are test-project-only, not shared
- Minimal implementation: only methods actually called by `CapabilityAuthorizationService` have meaningful behavior; other `IRepository<T>` methods return defaults

### Validation

```text
dotnet build            SUCCESS (0 errors, 0 warnings)
dotnet test             SUCCESS (220 tests, 220 passed, 0 failed)
```

15 new T05 application service tests + 204 previous UnitTests + 1 IntegrationTests = 220 total.

### Production source changes: NONE

### T05 Notes

- The service short-circuits on empty user ID or null/whitespace capability name without calling repositories. Repository interaction tests verify this behavior.
- The service uses capability name lookup, not ID-based lookup, so test fakes simulate name-based resolution.
- The service compares `groupCapability.CapabilityId == capability.Id` to match capabilities across groups, which is verified through multi-group tests.

---

## Final Sprint Assessment

PENDING — Sprint 11 is not complete. T01–T05 are complete. T07–T11 (minus T06) remain to be implemented.

T11 is the final Sprint 11 task. It synchronizes project-wide documentation with actual implementation and formally closes Sprint 11.
