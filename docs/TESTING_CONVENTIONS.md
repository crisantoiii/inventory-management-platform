# Testing Conventions

This document establishes the automated testing conventions for the Inventory Platform. These conventions are derived from Sprint 11, Sprint 12, Sprint 13, and Sprint 14 implementations and are authoritative for current test authoring.

---

## Test Architecture

### Test Projects

| Project | Purpose | References |
|---------|---------|------------|
| `InventoryPlatform.UnitTests` | Domain behavior, Application service isolation tests | Domain, Application, Shared |
| `InventoryPlatform.IntegrationTests` | Persistence/Infrastructure behavior tests | Domain, Application, Infrastructure, Shared |
| `InventoryPlatform.Web.Tests` | Web-layer authorization handler tests | Web (transitively: Application, Domain, Infrastructure, Shared) |

UnitTests must NOT reference `InventoryPlatform.Web`.
IntegrationTests must NOT reference `InventoryPlatform.Web`.
Web.Tests must NOT reference `InventoryPlatform.UnitTests` or `InventoryPlatform.IntegrationTests`.

### Dependency Boundaries

```text
InventoryPlatform.UnitTests
    -> InventoryPlatform.Domain
    -> InventoryPlatform.Application
    -> InventoryPlatform.Shared

InventoryPlatform.IntegrationTests
    -> InventoryPlatform.Domain
    -> InventoryPlatform.Application
    -> InventoryPlatform.Infrastructure
    -> InventoryPlatform.Shared

InventoryPlatform.Web.Tests
    -> InventoryPlatform.Web
```

UnitTests must NOT reference:
- `InventoryPlatform.Infrastructure`
- `InventoryPlatform.Web`

IntegrationTests must NOT reference:
- `InventoryPlatform.Web`

Web.Tests must NOT reference:
- `InventoryPlatform.UnitTests`
- `InventoryPlatform.IntegrationTests`

### Framework and Packages

**All three projects use:**
- xUnit test framework
- Microsoft.NET.Test.Sdk 17.*
- xunit 2.*
- xunit.runner.visualstudio 2.*
- Target framework: net10.0

**IntegrationTests additionally uses:**
- Microsoft.EntityFrameworkCore.InMemory 10.0.*

**No mocking frameworks** (Moq, NSubstitute, FakeItEasy) are used in any test project. Test doubles are hand-written where required.

---

## When to Use Each Project

### UnitTests

Use UnitTests for behavior that does NOT require database access:

- Domain entity behavior (construction, validation, state transitions, invariants)
- Application service behavior where dependencies can be isolated with hand-written fakes
- Business rule verification
- Validation logic
- Computed properties

**Examples from Sprint 11:**
- `PurchaseOrderTests` — domain workflow state transitions
- `PurchaseOrderItemTests` — item-level behavior
- `ProductTests` — stock operations, pricing, activation
- `CapabilityTests` — enable/disable, rename
- `AuthorizationGroupTests` — capability/user assignment
- `CapabilityAuthorizationServiceTests` — authorization resolution with hand-written fakes

**Examples from Sprint 13:**
- `CreatePurchaseOrderHandlerTests` / `SubmitPurchaseOrderHandlerTests` / `ApprovePurchaseOrderHandlerTests` / `ReceivePurchaseOrderHandlerTests` — Application handler orchestration (error mapping, validation short-circuits, save behavior, observable ordering) with hand-written fakes
- `CreatePurchaseOrderValidatorTests` / `CreatePurchaseOrderItemValidatorTests` — FluentValidation rules via direct instantiation
- `GetPurchaseOrderHandlerTests` / `GetPurchaseOrdersHandlerTests` — query mapping and repository-argument pass-through with fakes

### IntegrationTests

Use IntegrationTests where persistence or Infrastructure behavior is the subject:

- Repository query behavior
- Seeder/seed-data verification
- EF Core configuration and relationship loading
- Eager-loading behavior (Include)
- Persistence contract verification

**Examples from Sprint 11:**
- `AuthorizationSeederTests` — seed data creation and idempotency
- `CapabilityRepositoryTests` — repository query behavior
- `AuthorizationGroupRepositoryTests` — aggregate loading, relationship traversal
- `PurchaseOrderRepositoryTests` (Sprint 13) — real `PurchaseOrderRepository` query shape (Includes/ThenInclude, search, date/status filters, sorting, paging, AsNoTracking) and persistence round-trip under EF Core InMemory with fresh-context isolation

### Web.Tests

Use Web.Tests for Web-layer behavior such as:

- `CapabilityAuthorizationHandler` unit tests
- `MultiCapabilityAuthorizationHandler` unit tests
- Web-layer authorization boundary behavior
- Other Web-layer authorization behavior where appropriate

Handler tests construct handlers directly with a hand-written `FakeCapabilityAuthorizationService` implementing `ICapabilityAuthorizationService`. No WebApplicationFactory or HTTP pipeline is required.**Completed in Sprint 12:**

- `FakeCapabilityAuthorizationService` — hand-written fake implementing `ICapabilityAuthorizationService`
- `FakeCapabilityAuthorizationServiceTests` — verification that the fake compiles, instantiates, and produces controlled results (12 tests)
- `CapabilityAuthorizationHandlerTests` — `CapabilityAuthorizationHandler` behavior tests, COMPLETED (T03, 8 tests)
- `MultiCapabilityAuthorizationHandlerTests` — `MultiCapabilityAuthorizationHandler` behavior tests, COMPLETED (T04, 12 tests)

Web authorization-handler testing is no longer planned/deferred: it is completed and verified (Sprint 12 T08 integrated verification).

---

## Test Naming Convention

Tests use behavior-oriented names following the pattern:

```text
MethodOrBehavior_WhenCondition_ExpectedResult
```

**Examples:**

```text
// Domain tests
Create_WithValidParameters_SetsStatusToDraft
Submit_WhenDraftWithItems_ChangesStatusToSubmitted
Receive_ExceedingOrderedQuantity_ThrowsDomainException

// Repository tests
GetByNameAsync_WhenCapabilityExists_ReturnsCapability
GetByNameAsync_WhenCapabilityDoesNotExist_ReturnsNull
GetByNameAsync_WithDifferentCase_ReturnsNull

// Service tests
HasCapabilityAsync_WhenUserHasEnabledCapability_ReturnsTrue
HasCapabilityAsync_WhenCapabilityIsDisabled_ReturnsFalse
```

Tests should communicate intent clearly without requiring implementation knowledge.

---

## Test Organization

### Folder Structure

```text
tests/
  InventoryPlatform.UnitTests/
    Domain/
      Purchasing/
        PurchaseOrderTests.cs
        PurchaseOrderItemTests.cs
      Products/
        ProductTests.cs
      Authorization/
        CapabilityTests.cs
        AuthorizationGroupTests.cs
    Application/
      CapabilityAuthorizationServiceTests.cs
      Purchasing/
        CreatePurchaseOrderHandlerTests.cs
        CreatePurchaseOrderValidatorTests.cs
        CreatePurchaseOrderItemValidatorTests.cs
        PurchaseOrderErrorsTests.cs
        SubmitPurchaseOrderHandlerTests.cs
        ApprovePurchaseOrderHandlerTests.cs
        ReceivePurchaseOrderHandlerTests.cs
        GetPurchaseOrderHandlerTests.cs
        GetPurchaseOrdersHandlerTests.cs
    TestSupport/
      Purchasing/
        FakePurchaseOrderRepository.cs
        FakeUnitOfWork.cs
        PurchasingTestData.cs
        EntityIdHelper.cs

  InventoryPlatform.IntegrationTests/
    Authorization/
      AuthorizationSeederTests.cs
      CapabilityRepositoryTests.cs
      AuthorizationGroupRepositoryTests.cs
    Purchasing/
      PurchaseOrderRepositoryTests.cs
      PurchaseOrderLifecyclePersistenceTests.cs

  InventoryPlatform.Web.Tests/
    Authorization/
      FakeCapabilityAuthorizationService.cs
      FakeCapabilityAuthorizationServiceTests.cs
      CapabilityAuthorizationHandlerTests.cs
      MultiCapabilityAuthorizationHandlerTests.cs
      PurchaseOrderCapabilityPolicyRegistrationTests.cs
```

### Namespace Convention

Namespaces match folder structure:

```csharp
namespace InventoryPlatform.UnitTests.Domain.Purchasing;
namespace InventoryPlatform.UnitTests.Application;
namespace InventoryPlatform.IntegrationTests.Authorization;
namespace InventoryPlatform.Web.Tests.Authorization;
```

### File Organization

- One test class per subject under test
- Test classes are `public class` with no base class unless shared setup is required
- Each test is a `[Fact]` method
- Helper methods are `private` or `private static` within the test class
- Test data construction uses private helper methods for readability

---

## Test Design Patterns

### Unit Tests

**Direct object construction.** Domain entities are created directly:

```csharp
var order = PurchaseOrder.Create(supplierId, orderDate, null, null);
order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
```

**Hand-written fakes for Application services.** When testing Application services that depend on repository interfaces, create minimal in-memory fake implementations:

```csharp
private class FakeCapabilityRepository : ICapabilityRepository
{
    private readonly Capability? _capability;
    public string? LastQueriedName { get; private set; }

    public Task<Capability?> GetByNameAsync(string name, CancellationToken ct)
    {
        LastQueriedName = name;
        return Task.FromResult(_capability);
    }
    // ... minimal other members
}
```

Fakes are test-project-only and implement only the members actually called by the service under test.

### Integration Tests

**Real implementations under test.** Repository integration tests use the actual concrete repository:

```csharp
private readonly CapabilityRepository _repository;

public CapabilityRepositoryTests()
{
    _context = new ApplicationDbContext(_options);
    _repository = new CapabilityRepository(_context);
}
```

**Direct DbContext for test data setup.** Arrange steps may use DbContext directly:

```csharp
_context.Capabilities.Add(new Capability("Test.Cap"));
await _context.SaveChangesAsync();
```

**Repository for Act/Assert.** The repository under test is used for the action and verification:

```csharp
var result = await _repository.GetByNameAsync("Test.Cap");
Assert.NotNull(result);
```

---

## Test Isolation

### Database Isolation

Each integration test class uses a unique InMemory database:

```csharp
public CapabilityRepositoryTests()
{
    var dbName = $"CapRepoTest_{Guid.NewGuid():N}";
    _options = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseInMemoryDatabase(databaseName: dbName)
        .Options;
    _context = new ApplicationDbContext(_options);
    _repository = new CapabilityRepository(_context);
}
```

Integration test classes implement `IDisposable` to clean up the DbContext:

```csharp
public void Dispose()
{
    _context.Dispose();
}
```

### Test Independence

- Tests do not depend on execution order
- Tests do not share mutable state
- Each test sets up its own data
- Tests are deterministic and repeatable

---

## EF Core InMemory Usage

### What InMemory Validates

- Repository query behavior
- Entity relationship loading (Include)
- Eager-loading completeness
- Basic CRUD persistence
- Seeder idempotency
- Relationship isolation between aggregates

### What InMemory Does NOT Validate

- SQL Server foreign-key enforcement
- SQL Server-specific query translation
- SQL Server constraints and indexes
- Production database schema behavior
- Migration execution against SQL Server
- Decimal precision at the database level
- SQL Server-specific performance characteristics

### Important Notes

- InMemory is NOT equivalent to SQL Server integration testing
- Future tests requiring relational behavior should use SQLite or SQL Server test containers
- Direct `ApplicationDbContext` access is acceptable for test data setup and verification queries
- Repository methods under test should use `AsNoTracking()` where the production implementation does

---## Authorization Seed Baseline

The verified Sprint 14 seed baseline (from source, post-T05; the Sprint 11 baseline of 39 capabilities / 73 relationships is historical):

```text
Capabilities:                    41
Administrator capabilities:      41
InventoryManager capabilities:   23
Viewer capabilities:            15
Total group-capability relationships: 79
```

**Note:** Sprint 14 T05 added `PurchaseOrder.Edit` and `PurchaseOrder.Cancel` (39 → 41 capabilities, 73 → 79 relationships); group assignment emerged from the existing filter-derived seed rules with no special-case seed logic. Earlier baselines (39/73; original planning note Viewer=12/Total=72 vs source Viewer=13/Total=73) are historical records.

---

## Running Tests

### Local Execution

```text
# From the solution directory
cd src/InventoryPlatform

# Restore
dotnet restore

# Build
dotnet build --no-restore

# Run all tests
dotnet test --no-build

# Run only UnitTests
dotnet test tests/InventoryPlatform.UnitTests --no-build

# Run only IntegrationTests
dotnet test tests/InventoryPlatform.IntegrationTests --no-build

# Run only Web.Tests
dotnet test tests/InventoryPlatform.Web.Tests --no-build
```

### CI Status

No CI provider is currently configured in the repository. The complete test suite is locally reproducible using the commands above. When a CI provider is established, the provider-neutral commands can be directly translated into a provider-specific workflow.

---

## Current Test Coverage

### UnitTests (346 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Domain | PurchaseOrder workflow (incl. Sprint 14 cancellation + draft-edit coverage) | 83 |
| Domain | PurchaseOrderItem behavior | 39 |
| Domain | Product domain | 56 |
| Domain | Capability domain | 15 |
| Domain | AuthorizationGroup domain | 31 |
| Application | CapabilityAuthorizationService | 15 |
| Application | Purchasing — CreatePurchaseOrder handler (Sprint 13 T02) | 15 |
| Application | Purchasing — CreatePurchaseOrder validator (T02) | 14 discovered cases |
| Application | Purchasing — CreatePurchaseOrderItem validator (T02) | 13 discovered cases |
| Application | Purchasing — PurchaseOrderErrors contracts (T02/T03) | 11 discovered cases |
| Application | Purchasing — Submit handler (T03) | 6 |
| Application | Purchasing — Approve handler (T03) | 6 |
| Application | Purchasing — Receive handler (T04) | 14 |
| Application | Purchasing — GetPurchaseOrder handler (T05) | 7 |
| Application | Purchasing — GetPurchaseOrders handler (T05) | 9 |
| Application | Purchasing — Cancel handler (Sprint 14 T03) | 7 |
| Application | Purchasing — UpdateItem/RemoveItem handlers (Sprint 14 T04) | 14 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **346** |

Rows marked "discovered cases" contain `[Theory]` methods whose `[InlineData]` rows each execute as a separate case; unmarked rows are `[Fact]` methods where methods and discovered cases are equal.

### IntegrationTests (92 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Authorization | AuthorizationSeeder | 24 |
| Authorization | CapabilityRepository | 12 |
| Authorization | AuthorizationGroupRepository | 24 |
| Purchasing | PurchaseOrderRepository (Sprint 13 T06) | 24 |
| Purchasing | PurchaseOrderLifecyclePersistence (Sprint 14 T06) | 5 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **92** |

### Web.Tests (39 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Authorization | FakeCapabilityAuthorizationService verification | 12 |
| Authorization | CapabilityAuthorizationHandler behavior (Sprint 12 T03) | 8 |
| Authorization | MultiCapabilityAuthorizationHandler behavior (Sprint 12 T04) | 12 |
| Authorization | PurchaseOrder capability policy registration (Sprint 14 T05) | 6 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **39** |

**Total: 477 tests, 477 passed, 0 failures, 0 skipped** (346 + 92 + 39; Sprint 14 T09 re-verification; arithmetic: 346 + 92 + 39 = 477 — the earlier 478 figure was an arithmetic slip corrected during the sprint).

The Web.Tests verification tests confirm that the `FakeCapabilityAuthorizationService` compiles against the real `ICapabilityAuthorizationService` interface and produces controlled authorization results. The T03/T04 handler tests exercise the actual `CapabilityAuthorizationHandler` and `MultiCapabilityAuthorizationHandler` production sources directly (the Sprint 14 `PurchaseOrderCapabilityPolicyRegistrationTests` additionally verify Edit/Cancel capability constants and their real `AddWeb` policy registration) (authentication gate, NameIdentifier extraction/parsing, service delegation, succeed/do-not-succeed outcomes, OR semantics with short-circuit, requirement constructor validation). Handler testing is source-level/unit-level; Razor Page authorization boundaries (Categories/Edit, Suppliers/Create) were verified at source level and by the remediations themselves — no HTTP-pipeline or browser testing exists or is claimed.

---

## Sprint 14 Conventions (Reusable)

Established by the Sprint 14 lifecycle implementation and reusable for future test authoring:

1. **Domain-owned rules stay Domain-owned in tests.** Application handler tests assert that state guards (`Cancel()`), item rules (`UpdateItem`/`RemoveItem` by `ProductId`), and exception messages propagate from the aggregate unchanged — no rule is re-implemented or asserted at the wrong layer.
2. **Real `DomainException` propagation is the contract.** Invalid transitions and item rules are tested through the handler call path with the no-save-after-exception guarantee, matching the actual Web-layer behavior.
3. **Fresh-context isolation extends to lifecycle round-trips.** Cancellation, item update, item removal, and final-item removal persistence are each asserted through contexts different from the mutating context; EF change-tracker state is never the evidence.
4. **No artificial coverage at seams that do not exist.** Where PageModels depend on sealed concrete Application handler classes, capability/policy registration is tested instead and page behavior is verified manually — coverage is never inflated by distorting production design.

## Sprint 13 Conventions (Reusable)

Established by the Sprint 13 Purchasing test automation and reusable for future test authoring:

1. **Distinguish test methods from discovered test cases.** A `[Fact]` is one method = one case; a `[Theory]` with N `[InlineData]` rows is one method = N cases. Suite totals must state which unit they report, and counts must reconcile (verified with `dotnet test --list-tests` where needed).
2. **Per-test instance-scoped fake state — never static/global mutable state.** xUnit executes test classes in parallel; recording interaction order (a `CallOrder` instance created per test and shared explicitly with the participating fakes) is safe only when the state is per-test.
3. **Fresh-context isolation when asserting Include/ThenInclude under EF Core InMemory.** Arrange data through short-lived contexts that are saved and disposed, then query through a fresh context so relationship fix-up cannot mask a missing Include — only the repository's query shape can populate the navigations.
4. **Separate fake-based handler/query tests from repository integration coverage.** Fakes prove orchestration, mapping, and pass-through; the real repository on InMemory proves query shape, Includes, sorting/paging, and persistence round-trips. Neither substitutes for the other, and neither proves SQL Server relational behavior (see "What InMemory Does NOT Validate").
5. **Test real `DomainException` propagation where that is actual behavior**, including the no-save-after-exception guarantee, rather than shielding handlers from aggregate exceptions. Do not assert exact `DateTime.UtcNow` values where timing is not the contract.

---

## Deferred Testing Work

The following testing work is intentionally deferred and NOT part of the current scope:

### Sprint 12 Outcome

Completed:

- `CapabilityAuthorizationHandler` unit tests (T03) — 8 tests, passing
- `MultiCapabilityAuthorizationHandler` unit tests (T04) — 12 tests, passing
- Categories/Edit authorization remediation (T05) — `[Authorize(Policy = InventoryManagement)]`
- Suppliers/Create authorization remediation (T06) — `ViewInventory` replaced with `InventoryManagement`
- Integrated verification (T08) — 313 passed, 0 failed, 0 skipped

Blocked/deferred:

- EditStatus `User.IsInRole` cleanup (T07) — the remaining occurrence (`Pages/Administrator/Users/EditStatus.cshtml.cs`, line 61) is a reachable, behavior-affecting self-deactivation guard for supported multi-role users, NOT dead code. Removing it would change observable behavior; T07 remains blocked/deferred pending an explicit behavioral decision.

### Sprint 14 Outcome

Completed:

- Domain cancellation coverage (T02): 83 PurchaseOrder Domain tests passing (cancellation transitions, terminal `Cancelled` guards over Submit/Approve/Receive/UpdateItem/RemoveItem)
- Application workflows (T03/T04): 7 cancellation handler tests + 14 UpdateItem/RemoveItem handler tests (not-found, success, invalid state, missing line, save counts/call order, validators-free Domain-owned validation)
- Authorization seed/policy coverage (T05): `AuthorizationSeederTests` expected-data updates (41 capabilities, 79 relationships) + new `PurchaseOrderCapabilityPolicyRegistrationTests` (6 Web.Tests) proving Edit/Cancel constants, naming, and real `AddWeb` registration
- Persistence coverage (T06): 5 fresh-context lifecycle round-trip tests (Cancel Draft/Submitted, UpdateItem, RemoveItem, final-item removal → empty Draft)
- Integrated verification (T09): **477 passed, 0 failed, 0 skipped** (346/92/39); full rebuild 28 warnings / 0 errors — baseline preserved; manual browser and real SQL Server provider verification also passed (manual verification remains manual, not automated testing)

Still deferred (unchanged by Sprint 14):

- EditStatus self-deactivation guard (above) — untouched by Sprint 13
- `GetPurchaseOrdersHandler` does not copy `PagedRequest.Status` into `PagedQuery` (T05 recorded finding; no remediation authorized)
- SQL Server relational verification beyond InMemory (FK/unique constraints, transactions, SQL translation, collation, provider-specific behavior)
- WebApplicationFactory / Razor-page HTTP-pipeline testing; CI provider establishment

### Future Considerations

- WebApplicationFactory integration tests
- Razor Page authorization integration tests
- HTTP pipeline testing
- Browser automation (Playwright)
- Code coverage reporting and gates
- Mutation testing
- Performance/load testing
- SQL Server integration testing
- Broader repository coverage
- FluentValidation test coverage
- CI provider establishment

---

## Key Conventions Summary

1. **Three test projects:** UnitTests (no DB), IntegrationTests (EF Core InMemory), Web.Tests (handler tests)
2. **No Web reference:** UnitTests and IntegrationTests do not reference InventoryPlatform.Web
3. **No test-project cross-references:** Web.Tests does not reference UnitTests or IntegrationTests
4. **xUnit framework:** All tests use `[Fact]` attributes
5. **Hand-written fakes:** No mocking framework; minimal in-memory fakes for Application service and handler tests
6. **Behavior-oriented naming:** `MethodOrBehavior_WhenCondition_ExpectedResult`
7. **Isolated databases:** Each integration test class uses a unique InMemory database name
8. **Real implementations:** Integration tests use actual repository implementations
9. **No production changes:** Test infrastructure does not alter production code
10. **Provider-neutral CI:** Tests are locally reproducible; no CI provider is configured
11. **Behavior-focused coverage:** Tests verify business behavior, not implementation details
12. **Discovered-case accounting:** State whether a reported count is test methods or discovered test cases (`[Theory]` × `[InlineData]` rows); reconcile suite totals against actual execution
