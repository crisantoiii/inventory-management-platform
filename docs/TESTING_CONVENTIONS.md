# Testing Conventions

This document establishes the automated testing conventions for the Inventory Platform. These conventions are derived from Sprint 11 and Sprint 12 implementations and are authoritative for current test authoring.

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

### Web.Tests

Use Web.Tests for Web-layer behavior such as:

- `CapabilityAuthorizationHandler` unit tests
- `MultiCapabilityAuthorizationHandler` unit tests
- Web-layer authorization boundary behavior
- Other Web-layer authorization behavior where appropriate

Handler tests construct handlers directly with a hand-written `FakeCapabilityAuthorizationService` implementing `ICapabilityAuthorizationService`. No WebApplicationFactory or HTTP pipeline is required.

**Examples from Sprint 12:**
- `FakeCapabilityAuthorizationService` — hand-written fake implementing `ICapabilityAuthorizationService`
- `FakeCapabilityAuthorizationServiceTests` — verification that the fake compiles, instantiates, and produces controlled results
- `CapabilityAuthorizationHandlerTests` — (planned T03)
- `MultiCapabilityAuthorizationHandlerTests` — (planned T04)

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

  InventoryPlatform.IntegrationTests/
    Authorization/
      AuthorizationSeederTests.cs
      CapabilityRepositoryTests.cs
      AuthorizationGroupRepositoryTests.cs

  InventoryPlatform.Web.Tests/
    Authorization/
      FakeCapabilityAuthorizationService.cs
      FakeCapabilityAuthorizationServiceTests.cs
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

---

## Authorization Seed Baseline

The verified Sprint 11 seed baseline (from source):

```text
Capabilities:                    39
Administrator capabilities:      39
InventoryManager capabilities:   21
Viewer capabilities:            13
Total group-capability relationships: 73
```

**Note:** The planning baseline originally stated Viewer=12, Total=72. Source inspection confirmed Viewer=13, Total=73 because `User.View` matches the Viewer filter's `EndsWith(".View")` predicate.

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

### UnitTests (219 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Domain | PurchaseOrder workflow | 62 |
| Domain | PurchaseOrderItem behavior | 39 |
| Domain | Product domain | 56 |
| Domain | Capability domain | 15 |
| Domain | AuthorizationGroup domain | 31 |
| Application | CapabilityAuthorizationService | 15 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **219** |

### IntegrationTests (61 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Authorization | AuthorizationSeeder | 24 |
| Authorization | CapabilityRepository | 12 |
| Authorization | AuthorizationGroupRepository | 24 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **61** |

### Web.Tests (13 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Authorization | FakeCapabilityAuthorizationService verification | 12 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **13** |

**Total: 293 tests, 293 passed, 0 failures**

The Web.Tests verification tests confirm that the `FakeCapabilityAuthorizationService` compiles against the real `ICapabilityAuthorizationService` interface, produces controlled authorization results, and supports the capability inputs required by the handler tests in T03/T04.

---

## Deferred Testing Work

The following testing work is intentionally deferred and NOT part of the current scope:

### Sprint 12 (remaining)

- `CapabilityAuthorizationHandler` unit tests (T03)
- `MultiCapabilityAuthorizationHandler` unit tests (T04)
- Categories/Edit authorization defect remediation (T05)
- Suppliers/Create authorization defect remediation (T06)
- EditStatus dead-code cleanup (T07, optional)

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
