# Testing Conventions

This document establishes the automated testing conventions for the Inventory Platform. These conventions are derived from Sprint 11, Sprint 12, Sprint 13, Sprint 14, Sprint 17, Sprint 19, and Sprint 20 implementations and are authoritative for current test authoring.

---

## Test Architecture

### Test Projects

| Project | Purpose | References |
|---------|---------|------------|
| `InventoryPlatform.UnitTests` | Domain behavior, Application service isolation tests | Domain, Application, Shared |
| `InventoryPlatform.IntegrationTests` | Persistence/Infrastructure behavior tests | Domain, Application, Infrastructure, Shared |
| `InventoryPlatform.Web.Tests` | Web-layer authorization handler tests + real HTTP/Razor integration tests (Sprint 17) | Web (transitively: Application, Domain, Infrastructure, Shared) |

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

**IntegrationTests tier contract (Sprint 20):** every IntegrationTest method resolves to exactly one supported `TestTier` value — `ProviderNeutral` or `SqlServerRelational` — enforced by the fail-safe classification audit. See the "Integration Test Tier Conventions (Sprint 20)" section below.

**Web.Tests additionally uses (Sprint 17):**
- Microsoft.AspNetCore.Mvc.Testing 10.0.* — in-process `WebApplicationFactory<Program>` test host
- Microsoft.EntityFrameworkCore.InMemory 10.0.* — per-factory isolated database

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

Use Web.Tests for two complementary layers of Web behavior:

**Web-layer authorization handler unit tests** (handler-level, no HTTP):

- `CapabilityAuthorizationHandler` unit tests
- `MultiCapabilityAuthorizationHandler` unit tests
- Web-layer authorization boundary behavior
- Capability policy registration verification

Handler tests construct handlers directly with a hand-written `FakeCapabilityAuthorizationService` implementing `ICapabilityAuthorizationService`.

**Real HTTP/Razor integration tests** (Sprint 17):

Use a real `WebApplicationFactory<Program>` HTTP test when the behavior under proof is only observable through the actual pipeline: rendered Razor output, rendered antiforgery token/cookie semantics, model binding, authentication challenge/forbid redirects, PRG redirects, real `DomainException` presentation, or same-factory persistence after a POST.

- Direct PageModel instantiation remains appropriate where the behavior is fully observable without the pipeline (e.g., policy registration shape).
- Direct handler tests remain appropriate for Application-layer orchestration (UnitTests).
- Reach for the HTTP layer when asserting rendered form semantics, antiforgery, redirect/challenge behavior, or end-to-end no-mutation guarantees — do not reconstruct those behaviors with mocks.

See the "HTTP/Razor Integration-Test Conventions (Sprint 17)" section below for the host, safety, and antiforgery rules.

**Completed in Sprint 12:**

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
    Authentication/
      TestAuthenticationDefaults.cs
      SeededTestUserResolver.cs
      SeededUserAuthenticationHandler.cs
      SeededUserAuthenticationTests.cs
    Infrastructure/
      InventoryPlatformWebApplicationFactory.cs
      InventoryPlatformWebApplicationFactoryTests.cs
    Http/
      CategoryCreateAuthorizationTests.cs
      CategoryCreatePostWorkflowTests.cs
      CategoryCreateFormExtraction.cs
      PurchaseOrderSubmitDomainFailureHttpTests.cs
      PurchaseOrderSubmitFormExtraction.cs
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

The verified Sprint 26 seed baseline (from source, post-Sprint 26; the Sprint 14 baseline of 41 capabilities / 79 relationships is historical):

```text
Capabilities:                    41
Administrator capabilities:      41
InventoryManager capabilities:   23
Viewer capabilities:            9
Total group-capability relationships: 73
```

**Note:** Sprint 26 T05 narrowed Viewer `PurchaseOrder.*` capabilities to `PurchaseOrder.View` only (removed Create, Edit, Submit, Approve, Receive, Cancel). Viewer capability count: 15 → 9 (7 View + PurchaseOrder.View + User.View). Total group-capability relationships: 79 → 73 (6 Viewer PO capabilities removed). Sprint 14 T05 had added `PurchaseOrder.Edit` and `PurchaseOrder.Cancel` (39 → 41 capabilities, 73 → 79 relationships); group assignment emerges from the existing filter-derived seed rules with no special-case seed logic. Earlier baselines (39/73; original planning note Viewer=12/Total=72 vs source Viewer=13/Total=73) are historical records.

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

### CI Status (Sprint 20)

Provider-neutral CI is established. `.github/workflows/provider-neutral-verification.yml` runs on pull requests targeting `main`, pushes to `main`, and manual `workflow_dispatch` on `windows-latest` with .NET `10.0.x` and least-privilege `contents: read`. It delegates entirely to the shared verification script — `scripts/verify-provider-neutral.ps1` — and uploads the `provider-neutral-verification-results` TRX artifact with `if: always()`. The hosted job completed successfully on a clean hosted runner. No SQL Server/LocalDB setup, no browser setup, and no secrets are involved.

**Shared verification command (local and CI):**

```powershell
# From any working directory; resolves the repository from the script location
./scripts/verify-provider-neutral.ps1 -Configuration Release -ResultsDirectory artifacts/verification
```

The script is the single command authority: it restores repository-local .NET tools (`dotnet-ef 10.0.10` via `src/InventoryPlatform/dotnet-tools.json`) and proves tool resolution, restores the solution, performs a normal Release build, runs UnitTests (no filter), Web.Tests (no filter), and IntegrationTests with the affirmative `TestTier=ProviderNeutral` filter, runs EF `migrations has-pending-model-changes` (Infrastructure project / Web startup, matching configuration, `--no-build`), writes distinct TRX files to `artifacts/verification/`, prints an explicit executed/excluded tier summary, and returns non-zero on any mandatory failure. Parameters: `-Configuration` (default `Release`), `-ResultsDirectory` (default repository-relative `artifacts/verification`), `-SkipRestore` (controlled local reuse — skips ONLY the two restore steps; never build/tests/EF; **never used in CI**).

---

## Current Test Coverage

### UnitTests (355 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Domain | PurchaseOrder workflow (incl. Sprint 14 cancellation + draft-edit coverage) | 83 |
| Domain | PurchaseOrderItem behavior | 29 |
| Domain | Product domain | 56 |
| Domain | Capability domain | 16 |
| Domain | AuthorizationGroup domain | 30 |
| Application | CapabilityAuthorizationService | 15 |
| Application | Purchasing — CreatePurchaseOrder handler (Sprint 13 T02; validation-invocation contract added Sprint 21 T02) | 23 |
| Application | Purchasing — CreatePurchaseOrder validator (Sprint 21 T01) | 14 discovered cases |
| Application | Purchasing — CreatePurchaseOrderItem validator (Sprint 21 T01) | 13 discovered cases |
| Application | Purchasing — PurchaseOrderErrors contracts (incl. `Validation` contract, Sprint 21 T01) | 12 discovered cases |
| Application | Purchasing — Submit handler (T03) | 6 |
| Application | Purchasing — Approve handler (T03) | 6 |
| Application | Purchasing — Receive handler (T04) | 14 |
| Application | Purchasing — GetPurchaseOrder handler (T05) | 7 |
| Application | Purchasing — GetPurchaseOrders handler (T05) | 9 |
| Application | Purchasing — Cancel handler (Sprint 14 T03) | 7 |
| Application | Purchasing — UpdateItem/RemoveItem handlers (Sprint 14 T04) | 14 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **355** |

Rows marked "discovered cases" contain `[Theory]` methods whose `[InlineData]` rows each execute as a separate case; unmarked rows are `[Fact]` methods where methods and discovered cases are equal.

### IntegrationTests (140 tests: 126 ProviderNeutral + 14 SqlServerRelational)

Every IntegrationTest carries exactly one `TestTier` trait (Sprint 20). Tier classification follows runtime provider dependency.

| Area | Subject | Tests | Tier |
|------|---------|-------|------|
| Authorization | AuthorizationSeeder | 26 | ProviderNeutral |
| Authorization | AuthorizationGroupRepository | 23 | ProviderNeutral |
| Authorization | CapabilityRepository | 13 | ProviderNeutral |
| Purchasing | PurchaseOrderRepository (Sprint 13 T06) | 24 | ProviderNeutral |
| Purchasing | PurchaseOrderLifecyclePersistence (Sprint 14 T06) | 5 | ProviderNeutral |
| Tier audit | Classification rules (T02) | 6 | ProviderNeutral |
| Tier audit | Live assembly audit + locked SQL inventory (T02; 14 identities) | 1 | ProviderNeutral |
| Relational | SQL Server LocalDB safety/infrastructure guard + construction (no connection) | 20 | ProviderNeutral |
| Relational | SQL Server LocalDB lifecycle, reachability, R1-R6 contracts (Sprint 18; remediated T03) | 21 | SqlServerRelational |
| Infrastructure | Placeholder (`IntegrationTest1`) | 1 | ProviderNeutral |
| **Total** | | **140** | **126 PN + 14 SQL** |

The locked `SqlServerRelational` inventory is 14 identities (see `TierClassificationAuditTests.LockedSqlServerRelationalInventory`). The provider-neutral gate executes the 126 ProviderNeutral tests and intentionally does not execute the 14 relational tests; the full local suite remains 545 + 14 tests.

### Web.Tests (77 tests)

| Area | Subject | Tests |
|------|---------|-------|
| Authorization | FakeCapabilityAuthorizationService verification | 12 |
| Authorization | CapabilityAuthorizationHandler behavior (Sprint 12 T03) | 8 |
| Authorization | MultiCapabilityAuthorizationHandler behavior (Sprint 12 T04) | 12 |
| Authorization | PurchaseOrder capability policy registration (Sprint 14 T05) | 6 |
| Infrastructure | Database-safe WebApplicationFactory safety/isolation (Sprint 17 T03) | 5 |
| Authentication | Seeded-user test authentication (Sprint 17 T04) | 8 |
| HTTP | Category Create GET authorization matrix (Sprint 17 T05) | 3 |
| HTTP | Category Create antiforgery POST/PRG/persistence (Sprint 17 T06) | 1 |
| HTTP | Purchase Order Submit domain-failure proof (Sprint 17 T07) | 2 |
| HTTP | Purchase Order Create GET authorization coverage H1-H3 (Sprint 19 T02) | 3 |
| HTTP | Purchase Order Create valid-POST PRG/persistence H4 (Sprint 19 T03) | 1 |
| HTTP | Purchase Order Create duplicate-product failure redisplay H5 (Sprint 19 T04) | 1 |
| HTTP | Purchase Order Create validator-only failure redisplay H6 (Sprint 21 T03) | 1 |
| HTTP | Purchase Order Edit HTTP coverage H1-H6 (Sprint 22) | 6 |
| HTTP | Purchase Order Lifecycle HTTP coverage H1-H7 (Sprint 23) | 7 |
| Infrastructure | Placeholder | 1 |
| **Total** | | **77** |

> **Note:** Sprint 24 added a separate **BrowserTests** project (Playwright-based) with 4 browser/E2E smoke journeys (J1-J4). These are a separate verified tier and not included in the Web.Tests count above.

**Current total: provider-neutral gate 558 passed, 0 failed, 0 skipped** (355 UnitTests + 77 Web.Tests + 126 ProviderNeutral IntegrationTests; Sprint 24 integrated verification, `scripts/verify-provider-neutral.ps1` exit 0). **BrowserTests: 4 passing** (J1-J4). **Total: 562 tests passing across two separate tiers.** The 14 `SqlServerRelational` tests are discovered and locked but intentionally not executed in the gate — the full local suite remains 558 + 4 tests, and no SQL relational-pass claim is made for Sprint 24. The Sprint 21 historical closure total remains 545 (355 + 64 + 126); the Sprint 20 historical closure total remains 535 (346 + 63 + 126); the Sprint 19 historical closure total remains 542 (346 + 133 + 63, with the relational tier freshly executed against available LocalDB); the Sprint 18 historical closure total remains 537 (346 + 133 + 58); the Sprint 17 historical closure total remains 496 (346 + 92 + 58).

The Web.Tests verification tests confirm that the `FakeCapabilityAuthorizationService` compiles against the real `ICapabilityAuthorizationService` interface and produces controlled authorization results. The T03/T04 handler tests exercise the actual `CapabilityAuthorizationHandler` and `MultiCapabilityAuthorizationHandler` production sources directly (the Sprint 14 `PurchaseOrderCapabilityPolicyRegistrationTests` additionally verify Edit/Cancel capability constants and their real `AddWeb` policy registration) (authentication gate, NameIdentifier extraction/parsing, service delegation, succeed/do-not-succeed outcomes, OR semantics with short-circuit, requirement constructor validation). Handler testing is source-level/unit-level.

Since Sprint 17, Web.Tests additionally exercises the real ASP.NET Core HTTP/Razor pipeline through a database-safe `WebApplicationFactory<Program>`: real startup seeding, test-only seeded-user authentication with real GUID identities, production capability authorization, real rendered antiforgery token/cookie semantics, real model binding, challenge/forbid redirects, PRG, `DomainException` failure redisplay, and same-factory persistence are all verified through actual HTTP requests (see the Sprint 17 conventions section below). Since Sprint 19, this coverage includes the five Purchase Order Create behaviors H1-H5 (anonymous challenge, authorized manager access, real-capability denial, valid-antiforgery POST → 302 PRG → same-factory persistence, and duplicate-product Domain-failure redisplay with restoration and no mutation). Since Sprint 21 it additionally includes H6, a validator-only failure (over-length `Remarks`) reaching the real handler through the real composition and presented model-level. See the Sprint 19 and Sprint 21 conventions sections below. Coverage is representative — one Category GET matrix, one Category successful POST, one Purchase Order domain-failure POST, and the six Purchase Order Create behaviors — not exhaustive route coverage; no relational behavior is claimed from the InMemory-based host.

---

## Sprint 14 Conventions (Reusable)

Established by the Sprint 14 lifecycle implementation and reusable for future test authoring:

1. **Domain-owned rules stay Domain-owned in tests.** Application handler tests assert that state guards (`Cancel()`), item rules (`UpdateItem`/`RemoveItem` by `ProductId`), and exception messages propagate from the aggregate unchanged — no rule is re-implemented or asserted at the wrong layer.
2. **Real `DomainException` propagation is the contract.** Invalid transitions and item rules are tested through the handler call path with the no-save-after-exception guarantee, matching the actual Web-layer behavior.
3. **Fresh-context isolation extends to lifecycle round-trips.** Cancellation, item update, item removal, and final-item removal persistence are each asserted through contexts different from the mutating context; EF change-tracker state is never the evidence.
4. **No artificial coverage at seams that do not exist.** Where PageModels depend on sealed concrete Application handler classes, capability/policy registration is tested instead and page behavior is verified manually — coverage is never inflated by distorting production design.

## HTTP/Razor Integration-Test Conventions (Sprint 17)

Established by the Sprint 17 HTTP/Razor integration-test foundation and authoritative for future HTTP test authoring:

1. **Choose the right layer before writing the test.** Use direct PageModel/handler tests when the behavior is fully observable without the pipeline; use a real HTTP test only when rendered Razor output, antiforgery, challenge/forbid redirects, PRG, `DomainException` presentation, or end-to-end persistence/no-mutation is the subject.
2. **Database safety by containment plus structural proof.** The test host injects a deliberately non-production sentinel `DefaultConnection` through early host configuration (before application registration reads configuration), lets normal production registrations occur, replaces `ApplicationDbContext` registrations in test service customization before the final root provider is built, and runs a fail-closed structural validation that rejects any surviving production context configuration. HTTP tests must never open a production SQL connection, and the sentinel means a failed replacement cannot silently reach real infrastructure.
3. **Real startup seeding, not test-only suppression.** The unchanged `UseWeb()` pipeline performs Identity/authorization seeding against the unique per-factory EF Core InMemory database. Do not add a test-only seeder suppression flag, an alternate startup class, or an extra production environment branch for tests — the minimal production seam is `public partial class Program { }`.
4. **Unique database per factory; unique data per test.** `DatabaseName` is a `Guid`-suffixed instance property; each test creates and disposes its own factory. Test-created rows use unique (Guid-suffixed) names. No test reads another test's mutations.
5. **Authentication is test-only; authorization stays production.** Requests carry a logical seeded-user selector header (`X-Test-User`); the test authentication handler resolves the selector through the real `UserManager`/Identity store and issues only the two real claims (`NameIdentifier` from the persisted GUID, `Name`). Never fabricate role or capability claims — capability authorization must flow through the production `ICapabilityAuthorizationService`/handlers/repositories. Challenge/forbid remain production Identity application-cookie behavior.
6. **Antiforgery is exercised, never bypassed or manufactured.** Extract the antiforgery token from the actually rendered form and preserve the matching antiforgery cookie from the GET response (narrow BCL-only regex/attribute decoding is the accepted extraction approach — no AngleSharp/parser packages). POST to the effective rendered form action with exactly the rendered hidden fields. Multiple forms on a page are disambiguated by a production-stable marker (button text), not by position.
7. **Environment isolation stays test-only.** Ephemeral Data Protection (no machine key ring), cleared logging providers (no Windows EventLog dependency), HTTPS client base address, and in-process `TestServer` requests only. None of this isolation may leak into production configuration.
8. **Isolation under default parallelism.** No collection attributes, no `maxParallelThreads` changes, no global serialization, no sleeps/retries. Factories, clients, and cookie containers are per-test state disposed with `await using`/`using`.
9. **No relational claims from the InMemory host.** HTTP integration tests prove wiring, rendered behavior, and same-factory persistence — not SQL translation, constraints, transactions, or provider-specific behavior.
10. **Assert semantics, not HTML snapshots.** Prefer stable production markers (button text, exact invariant messages, hidden-field values) over full-HTML equality. Where rendering is culture-dependent (e.g., `DateOnly` hidden inputs rendered with the host culture's short-date pattern), assert the semantic round-trip (parse the rendered value back under the same culture the server binds with) rather than a literal string.
11. **Rendered behavior is observed, not guessed.** Effective form actions, redirect destinations, and hidden-field formatting must be taken from the actual rendered response (Sprint 17 evidence: `asp-page-handler` renders `?handler=Submit` in the action; culture-formatted date hidden inputs) — never assumed from source intent.
12. **Domain-failure redisplay contract.** Expected `DomainException` failures are proven through the real POST, asserting the existing production presentation (ModelState redisplay with HTTP 200 in the current source), preserved navigation/query state, and a fresh-scope same-factory reload proving no mutation. Never inject or catch the exception in the test.

## Purchase Order Create HTTP Conventions (Sprint 19)

Established by the Sprint 19 Purchase Order Create HTTP/Razor coverage on top of the Sprint 17 foundation and authoritative for future Purchase Order Create HTTP test authoring:

1. **Authorization is proven through the real capability policy.** H3 uses a persisted test-only user with no authorization-group assignment (`purchaseorder-denied@inventory.test`) so denial flows through the production `PurchaseOrder.Create` capability decision. Do not substitute the seeded Viewer (it already possesses `PurchaseOrder.Create`) and do not fabricate or stub capability outcomes in HTTP tests.
2. **H3 identity is test-only and factory-local.** The denied identity is arranged per-test through factory-local helpers; no production seed, group, or policy change exists or is permitted to back it.
3. **Antiforgery and form semantics come from the rendered Create page.** Extract the token, matching cookie, effective form action, and indexed item fields with the narrow `PurchaseOrderCreateFormExtraction` helper (PO-Create path and submit-marker specific, BCL regex/decoding only); POST to the effective action with exactly the rendered hidden fields.
4. **Persistence is verified from a new scope on the same factory.** After the 302 PRG, reload through `factory.Services.CreateAsyncScope()` on the same factory instance — never a second factory and never the mutating scope.
5. **Domain failure is asserted as redisplay + restoration + no mutation.** H5 asserts HTTP 200, the canonical `The product already exists in this purchase order.` message, restored Supplier/Product options, Supplier selection, date, remarks, and both rows/values, unchanged PO/item counts, and no persisted order (marker `S19-T04-DUPLICATE`).
6. **No relational claim from Create HTTP evidence.** H1-H5 are EF Core InMemory HTTP-host evidence; SQL Server relational truth remains with the Sprint 18 tier under `tests/InventoryPlatform.IntegrationTests/Relational/`.

## SQL Server Relational-Test Conventions (Sprint 18)

The SQL Server relational tier lives under `tests/InventoryPlatform.IntegrationTests/Relational/` and contains these Sprint 18 files:

- `RelationalSafetyGuard.cs`
- `RelationalTestDatabase.cs`
- `RelationalTestInfrastructureSqlServerTests.cs`
- `SqlServerMigrationTests.cs`
- `SqlServerConstraintTests.cs`
- `SqlServerStorageSemanticsTests.cs`
- `SqlServerInventoryMovementQueryTests.cs`

Durable conventions:

1. **SQL Server fidelity is the purpose.** Run against SQL Server LocalDB only, at exact server `(localdb)\MSSQLLocalDB`. SQLite is relational but is not an equivalent substitute for SQL Server migrations, error codes, precision behavior, or query translation.
2. **Fail closed and fail hard.** Relational execution must fail if LocalDB is unavailable or any safety assertion is false. Never turn provider absence into a conditional skip.
3. **Own a unique disposable database per test.** Names begin with `InventoryPlatformRelationalTests_` and end with a unique GUID. Validate both the exact server and guarded database prefix before connecting, creating, migrating, or dropping.
4. **Never reuse application configuration.** Construct the test connection using trusted local authentication; do not read production `DefaultConnection`, target development database `InventoryPlatform`, store credentials, or invoke production seeders.
5. **Use the real migration chain.** Create schema with `MigrateAsync()`, not `EnsureCreated()`. R1 proves all 10 migrations apply to a fresh database through `20260831141400_CreateAuthorizationSchema` and leave zero pending migrations.
6. **Cleanup is guarded and best-effort.** Apply the safety guard again before drop. Cleanup failure must be surfaced but is not the primary safety mechanism; unique names and positive target validation provide containment.
7. **Retain default xUnit parallelism.** Database-per-test ownership removes shared mutable database state; do not add global serialization, collection-level disabling, sleeps, or retries for convenience.
8. **Exercise real production paths where translation is the contract.** R6 calls `GetInventoryMovementHandler`, which delegates to `InventoryMovementRepository.GetInventoryMovementAsync`; do not copy the LINQ into a test-only query.
9. **Observe provider behavior before locking assertions.** Provider-specific storage assertions, such as R5's representative `Product.QuantityOnHand` `decimal(18,2)` case, must be based on observed deterministic behavior. Do not claim whether EF/provider conversion or SQL Server assignment caused the result without separate evidence.
10. **Keep claims bounded.** R2 proves `IX_Products_Sku`; R3 proves `FK_Products_Categories_CategoryId`; R4 proves all-or-nothing behavior for one failing `SaveChangesAsync`; R5 proves one mapped decimal property; R6 proves one report query. None establishes a universal guarantee for every constraint, workflow, decimal mapping, or report.

Run only the relational tier from the solution directory:

```powershell
dotnet test tests/InventoryPlatform.IntegrationTests/InventoryPlatform.IntegrationTests.csproj --filter FullyQualifiedName~Relational
```

EF pending-model checks require the Web startup project because `ApplicationDbContext` is DI-constructed. The repository tool baseline is `dotnet-ef` 10.0.10:

```powershell
dotnet ef migrations has-pending-model-changes --project src/InventoryPlatform/InventoryPlatform.Infrastructure --startup-project src/InventoryPlatform/InventoryPlatform.Web
```

## Integration Test Tier Conventions (Sprint 20)

Established by Sprint 20 and authoritative for all current and future IntegrationTest authoring:

1. **Exactly one tier, always.** Every IntegrationTest method must resolve to exactly one supported `TestTier` value. The supported values are exactly `ProviderNeutral` and `SqlServerRelational`. The classification audit fails closed on missing, duplicate/multiple, and unknown tier values.
2. **Runtime provider dependency determines classification.** A test that opens, or attempts to open, a real connection to SQL Server/LocalDB belongs to `SqlServerRelational` — regardless of whether it could pass without the provider, and regardless of folder or namespace. Tests that use the EF Core InMemory provider, pure string/KB-level construction (e.g., `SqlConnectionStringBuilder` parsing), or no provider at all are `ProviderNeutral`.
3. **Affirmative ProviderNeutral selection only.** The provider-neutral gate selects with `--filter TestTier=ProviderNeutral`. Never implement the gate by excluding `SqlServerRelational`; positive inclusion keeps the classification audit inside every gate run so contract drift fails the gate itself.
4. **The SQL inventory is locked by identity.** `TierClassificationAuditTests.LockedSqlServerRelationalInventory` holds the fully qualified names of all SQL-provider-bound tests (currently 14). Intentional additions, removals, renames, or reclassifications of relational tests require a deliberate lock update in the same change; an unexplained drift fails the audit.
5. **Zero provider contact is the ProviderNeutral property.** Passing without LocalDB is insufficient: a ProviderNeutral test must make no LocalDB/SQL Server connection, database creation, or provider execution attempt. When in doubt, classify by what the test attempts, not by what it requires.
6. **SQL relational execution stays separate.** The provider-neutral gate never executes `TestTier=SqlServerRelational`. Relational execution remains a local, explicitly-invoked activity against `(localdb)\MSSQLLocalDB` under the Sprint 18 conventions (and remains future work for CI).
7. **One shared command authority.** Local and CI verification both run `scripts/verify-provider-neutral.ps1`. Do not duplicate its restore/build/test/EF command sequence in CI or docs; extend the script when the verification contract changes.

## Validation Invocation Proof Conventions (Sprint 21)

Established by Sprint 21 (see `docs/DESIGN_DECISIONS.md` DD-046) and authoritative for all current and future validation-related test authoring:

1. **A direct validator test is not proof of production invocation.** `CreatePurchaseOrderValidatorTests` / `CreatePurchaseOrderItemValidatorTests` instantiate the validator directly. They prove the rule matrix and nothing else. Before Sprint 21 the production runtime authority was the Domain aggregate and handler `Result` checks, so a fully green validator suite coexisted with a completely unwired validator. Never cite validator tests as evidence that a request is validated at runtime.
2. **Three distinct evidence layers, none substitutable.** Validator rule tests prove *what the rules are*. Handler tests prove *that validation is invoked, that it runs first, and that the `Result` contract is honored*. HTTP tests prove *that the whole real composition invokes it and presents it correctly*. A change to invocation or the error contract must be proven at the handler layer at minimum; a change to presentation or composition must be proven at the HTTP layer.
3. **Invocation-first is an observable, testable property.** Prove validation runs before any side effect by asserting zero supplier reads, zero product reads, zero `AddAsync`, zero `SaveChangesAsync` — and, where a shared `CallOrder` is available, that it recorded no events at all. Asserting only the returned `Result` does not prove ordering.
4. **The scalar `Errors[0]` contract must be asserted literally.** A validation failure returns exactly one error with code `PurchaseOrder.Validation` and the verbatim first FluentValidation `ErrorMessage`. When asserting the message, do not re-derive the expectation from the same validator instance in the test and call that a proof of the contract; assert the locked message text for the deterministic cases (`A valid supplier must be selected.`, `A valid product must be selected.`, and the exact `MaximumLength(500)` text) and separately assert the "equals `Errors[0]`" property where it is the property under test.
5. **Deterministic precedence must be locked by test, not assumed.** Rule declaration order is the frozen precedence order (see DD-046). When more than one rule can fail, add a test that fails several simultaneously and asserts which message wins — both at the top level and inside a child validator.
6. **Empty collection and Domain-invalid empty state are different things.** `Items` empty is **valid input**: the handler creates and saves an empty Draft and returns success, and this is asserted at the handler layer. `Submit()` on an empty Draft remains **Domain-invalid** and is proven by `PurchaseOrderTests.Submit_WhenDraftWithNoItems_ThrowsDomainException` / `SubmitPurchaseOrderHandlerTests.HandleAsync_EmptyDraft_DomainExceptionPropagatesAndDoesNotSave`. Never conflate the two, and never re-add a validator-level item-count rule to express the submit rule — the Domain owns it.
7. **Overlapping Application and Domain rules are both proven, with an explicit winner.** Quantity and UnitCost are validated at the Application boundary *and* guarded in the Domain. Test the Application rejection with zero repository interaction (validation wins), and separately test that the Domain guard still exists and still throws. Neither test substitutes for the other.
8. **Cancellation-token forwarding is part of the invocation contract.** Assert that the same token passed to `HandleAsync` reaches `ValidateAsync` and the downstream repository/`SaveChangesAsync` calls. Where a shared fake does not expose this, add a recording-only property to a fake that already records call order — never change fake behavior to make it observable.
9. **When asserting an exact externally-visible message, prove the assertion is live.** A deliberately mutated expected value must be observed to fail (then reverted). An exact-string assertion that has never been seen to fail is not evidence.
10. **A validator-only failure HTTP test must use the real form.** Post the over-limit value through the rendered form with the extracted antiforgery token and preserved cookie, and assert HTTP 200 with a null `Location` (a redisplay, not a redirect), the exact message in the `asp-validation-summary="ModelOnly"` region, full restoration of options/selection/date/remarks/item row, and unchanged Purchase Order and Purchase Order Item counts from a fresh same-factory scope. Do not assert field-level mapping — the frozen contract is a single model-level message.

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

Resolved in Sprint 26:
- EditStatus `User.IsInRole` cleanup (T07) — refactored to capability-based self-deactivation protection using `User.EditStatus` capability via `ICapabilityAuthorizationService` (BF-Q-002 decision). The legacy `User.IsInRole(InventoryManager)` guard was reachable for multi-role users and behavior-affecting; replaced with capability-based check for consistent protection.

### Sprint 14 Outcome

Completed:

- Domain cancellation coverage (T02): 83 PurchaseOrder Domain tests passing (cancellation transitions, terminal `Cancelled` guards over Submit/Approve/Receive/UpdateItem/RemoveItem)
- Application workflows (T03/T04): 7 cancellation handler tests + 14 UpdateItem/RemoveItem handler tests (not-found, success, invalid state, missing line, save counts/call order, validators-free Domain-owned validation)
- Authorization seed/policy coverage (T05): `AuthorizationSeederTests` expected-data updates (41 capabilities, 79 relationships) + new `PurchaseOrderCapabilityPolicyRegistrationTests` (6 Web.Tests) proving Edit/Cancel constants, naming, and real `AddWeb` registration
- Persistence coverage (T06): 5 fresh-context lifecycle round-trip tests (Cancel Draft/Submitted, UpdateItem, RemoveItem, final-item removal → empty Draft)
- Integrated verification (T09): **477 passed, 0 failed, 0 skipped** (346/92/39); full rebuild 28 warnings / 0 errors — baseline preserved; manual browser and real SQL Server provider verification also passed (manual verification remains manual, not automated testing)

### Sprint 19 Outcome

Completed:

- Purchase Order Create HTTP coverage (T02-T04): five real-pipeline behaviors — H1 anonymous GET challenge with semantic ReturnUrl, H2 authorized manager access, H3 real `PurchaseOrder.Create` capability denial of the test-only `purchaseorder-denied@inventory.test` user (no authorization-group assignment; the seeded Viewer is not the H3 identity), H4 valid-antiforgery POST → 302 PRG → same-factory persistence (marker `S19-T03-VALID-CREATE`), H5 duplicate-product Domain-failure redisplay/restoration with no mutation (marker `S19-T04-DUPLICATE`)
- Narrow `PurchaseOrderCreateFormExtraction` (PO-Create-specific, BCL-only) and factory-local data-arrangement helpers
- Integrated verification (T05): **542 passed, 0 failed, 0 skipped** (346/133/63); relational tier freshly re-executed against available LocalDB; normal build 0W/0E; non-incremental 28W/0E; no pending EF model changes; 10 migrations unchanged — zero production/test-source changes in T05/T06

Still deferred (Sprint 20 state):

- `GetPurchaseOrdersHandler` does not copy `PagedRequest.Status` into `PagedQuery` (T05 recorded finding; no remediation authorized; re-verified against current source at Sprint 17 T09 closure)
- SQL Server relational CI — the provider-neutral gate deliberately excludes relational execution; a hosted SQL Server/LocalDB endpoint decision is required before the relational tier can run in CI (Sprint 20 carry-forward C20-01)
- Broader SQL Server relational verification beyond the bounded Sprint 18 R1-R6 contracts, including other constraints, workflows, reports, collation, and provider-specific behavior; the Sprint 17/Sprint 19 HTTP host remains InMemory and makes no relational claims
- Broader HTTP/Razor route coverage beyond the Sprint 17 representative cases and the Sprint 19 Purchase Order Create behaviors (more Category/Purchase Order routes, other modules)
- Browser/E2E automation (carried forward; outside the provider-neutral gate)

Resolved in Sprint 26:
- EditStatus self-deactivation guard — refactored to capability-based protection using `User.EditStatus` capability (BF-Q-002 decision)

Completed:

- Purchase Order Create validation invocation proof (T01-T03): the validator rule matrix (Items collection rule retired, empty collection valid; top-level and child order locked), the `PurchaseOrderErrors.Validation` scalar contract (code `PurchaseOrder.Validation`, verbatim message), 8 new handler tests covering validation-first ordering with zero repository/UoW interaction, deterministic `Errors[0]` selection at both top level and inside a child validator, the overlaps with Domain quantity/unit-cost guards, empty-Draft Create success, and cancellation-token forwarding to `ValidateAsync` plus all downstream calls, and one real HTTP/Razor scenario (H6) proving a validator-only failure reaches the user model-level with full restoration and no mutation
- Integrated verification (T04): `scripts/verify-provider-neutral.ps1` exit `0` — **545 passed, 0 failed, 0 skipped** (355 UnitTests, 64 Web.Tests, 126 ProviderNeutral IntegrationTests; 14 SqlServerRelational discovered and locked, not executed); normal Release build 0W/0E; no pending EF model changes; 10-migration chain unchanged; zero diagnostics from any Sprint 21 file

### Future Considerations

- Broader HTTP/Razor route coverage (the Sprint 17 foundation exists; only representative cases are covered)
- Browser automation (Playwright)
- SQL Server relational CI (requires a hosted SQL Server/LocalDB endpoint decision; C20-01)
- Code coverage reporting and gates
- Mutation testing
- Performance/load testing
- Broader SQL Server relational integration testing beyond R1-R6 (including any future SQL Server-backed HTTP host)
- Broader repository coverage
- FluentValidation coverage beyond the Sprint 21 Purchase Order Create path — that flow now has rule tests, handler invocation/contract tests, and a real HTTP composition test; other features' validators remain rule-tested only, with no production invocation, and wiring them is a separate architectural decision

---

## Key Conventions Summary

1. **Three test projects:** UnitTests (no DB), IntegrationTests (EF Core InMemory plus Sprint 18 SQL Server LocalDB relational tier), Web.Tests (handler tests + Sprint 17 real HTTP/Razor integration tests)
2. **No Web reference:** UnitTests and IntegrationTests do not reference InventoryPlatform.Web
3. **No test-project cross-references:** Web.Tests does not reference UnitTests or IntegrationTests
4. **xUnit framework:** All tests use `[Fact]` attributes
5. **Hand-written fakes:** No mocking framework; minimal in-memory fakes for Application service and handler tests
6. **Behavior-oriented naming:** `MethodOrBehavior_WhenCondition_ExpectedResult`
7. **Isolated databases:** Each InMemory integration test class uses a unique database name; each HTTP test factory owns a unique `Guid`-suffixed InMemory database; each relational test owns a unique guarded `InventoryPlatformRelationalTests_<guid>` LocalDB database
8. **Real implementations:** Integration tests use actual repository implementations; HTTP integration tests exercise the real pipeline (seeding, authentication resolution, capability authorization, antiforgery, model binding, persistence)
9. **No production changes:** Test infrastructure does not alter production code; the only production seam is `public partial class Program { }`
10. **No production SQL from HTTP tests:** Sentinel connection containment, pre-provider DbContext replacement, and fail-closed structural validation (Sprint 17)
11. **No fabricated authorization:** Test authentication issues only real persisted-GUID claims; capability authorization flows through production services; antiforgery is extracted from rendered forms, never bypassed or manufactured (Sprint 17)
12. **Provider-neutral gate, provider-specific relational dependency:** CI is established via `.github/workflows/provider-neutral-verification.yml` delegating to `scripts/verify-provider-neutral.ps1`; the gate is provider-neutral (zero LocalDB/SQL contact, verified) and never executes the relational tier. Relational execution still requires LocalDB fidelity — a future hosted relational runner must supply SQL Server fidelity rather than substituting SQLite.
13. **Behavior-focused coverage:** Tests verify business behavior, not implementation details
14. **Discovered-case accounting:** State whether a reported count is test methods or discovered test cases (`[Theory]` × `[InlineData]` rows); reconcile suite totals against actual execution
15. **Exactly-one-tier IntegrationTest contract:** every IntegrationTest resolves to exactly one supported `TestTier` (`ProviderNeutral`, `SqlServerRelational`); classification follows runtime provider dependency; the audit fails closed and the 14-identity relational inventory is locked (Sprint 20)
16. **No "all tests" language for the provider-neutral gate:** the gate executes 355 UnitTests + 64 Web.Tests + 126 ProviderNeutral IntegrationTests and explicitly does not execute the 14 `SqlServerRelational` tests or browser/E2E work — describe coverage by tier, never as "all tests pass"
17. **Three non-substitutable validation proof layers:** validator rule tests prove the rule matrix, handler tests prove invocation and the scalar `Result` contract, HTTP tests prove real composition and presentation — a direct validator test is never evidence of production invocation (Sprint 21)
