# Sprint 18 Retrospective - SQL Server Relational Verification

> **STATUS: FINAL — SPRINT COMPLETE / CLOSED.**
>
> Finalized at T07 from the externally accepted T01-T06 execution record.

## Sprint Identity

- **Sprint:** Sprint 18
- **Title:** SQL Server Relational Verification
- **Retrospective state:** FINAL
- **Sprint status:** COMPLETE / CLOSED
- **Release classification:** Technical/non-release — no semantic version increment or tag; v1.6.0 remains the latest release baseline

## 1. Objective

Establish an automated SQL Server-backed relational verification tier executing the real migration chain and proving high-value database-enforced/provider behaviors on LocalDB under a fail-closed disposable-database safety contract, with zero production behavior changes.

## 2. Final Task Outcomes

| Task | Outcome | Status |
|---|---|---|
| T01 | Fail-closed LocalDB safety and database-per-test lifecycle foundation | COMPLETE / ACCEPTED |
| T02 / R1 | Fresh-database real migration-chain proof | COMPLETE / ACCEPTED |
| T03 / R2-R3 | SQL Server-enforced Product SKU uniqueness and Category FK delete restriction | COMPLETE / ACCEPTED |
| T04 / R4-R5 | Single-save atomicity and observed representative decimal storage semantics | COMPLETE / ACCEPTED |
| T05 / R6 | Actual Inventory Movement production-query translation and deterministic aggregation | COMPLETE / ACCEPTED |
| T06 | Integrated verification | COMPLETE / ACCEPTED |
| T07 | Documentation synchronization and Sprint closure | COMPLETE |

## 3. Relational Contracts

1. **R1 — Fresh database migration chain.** `MigrateAsync()` applied all 10 real migrations to a new disposable database through `20260831141400_CreateAuthorizationSchema`; no migrations remained pending.
2. **R2 — Product SKU uniqueness.** SQL Server enforced `IX_Products_Sku`; a duplicate was rejected with SQL error 2601.
3. **R3 — Category FK delete restriction.** SQL Server enforced `FK_Products_Categories_CategoryId`; deletion of a referenced Category was rejected with SQL error 547.
4. **R4 — Single-save atomicity.** One `SaveChangesAsync()` containing an otherwise-valid pending write and a constraint-violating pending write persisted neither. This does not generalize to every application workflow.
5. **R5 — Representative `decimal(18,2)` storage semantics.** Actual `Product.QuantityOnHand` persistence behavior was observed first and then asserted. The evidence does not establish whether EF/provider conversion or SQL Server assignment performs the conversion.
6. **R6 — Inventory Movement translation/aggregation.** The actual `GetInventoryMovementHandler` → `InventoryMovementRepository.GetInventoryMovementAsync` path translated and executed on SQL Server with deterministic aggregates. This does not generalize to all reports.

## 4. Durable Safety Contract

- SQL Server LocalDB only; exact approved server `(localdb)\MSSQLLocalDB`
- Trusted local authentication; no committed credentials
- Unique `InventoryPlatformRelationalTests_<guid>` database per test
- Exact server and guarded prefix validation before connection, creation, migration, and drop
- Fail-hard behavior with no conditional skips when the tier is intentionally run
- No application/production `DefaultConnection`; development database `InventoryPlatform` is never targeted
- No production seeders; tests arrange only their own minimum data
- Real `MigrateAsync()` migrations, never `EnsureCreated()`
- Guarded best-effort cleanup; cleanup is not the primary safety control
- Default xUnit parallelism retained; no global serialization, sleeps, or retries

## 5. Final Verification Baseline

```text
Relational:        41 passed
UnitTests:        346 passed
IntegrationTests: 133 passed
Web.Tests:         58 passed
Total:            537 passed, 0 failed, 0 skipped
Normal build:      0 warnings, 0 errors
Non-incremental:  28 warnings, 0 errors (unchanged historical baseline)
EF:                no pending model changes
Migrations:        10
Latest migration:  20260831141400_CreateAuthorizationSchema
dotnet-ef:         10.0.10
```

The 41 relational cases include infrastructure and safety verification; they are not 41 distinct business contracts.

## 6. What Went Well

- LocalDB enabled real SQL Server verification without a Docker dependency.
- Fail-closed target validation prevented accidental development-database targeting.
- R1-R6 stayed bounded and represented distinct failure classes that InMemory cannot prove.
- Provider-specific tests covered migrations, constraints, atomicity, storage, and translation behavior outside InMemory's evidence boundary.
- Integrated verification reached 537/537 with zero failed and zero skipped while retaining default parallelism.

## 7. Important Observations

- R5 needed observation before assertion; documentation intentionally avoids claiming which conversion layer produced the persisted value.
- R6 exercised the real production handler/repository query instead of copied test LINQ.
- EF pending-model tooling requires `--startup-project InventoryPlatform.Web` because `ApplicationDbContext` is DI-constructed.
- The non-incremental warning baseline remains 28 and is deferred; Sprint 18 introduced no warning regression.

## 8. Scope Discipline

- Zero production behavior/source changes
- Zero EF model/mapping or migration changes
- Zero package/project, startup, or configuration changes
- No production seeder use and no HTTP test-host changes
- No CI workflow work
- No expansion beyond R1-R6
- No SQLite substitution
- No semantic version, release tag, or product-feature claim

## 9. Deferred Follow-up

- CI pipeline and selection/provisioning of a future CI SQL Server endpoint
- Broader HTTP/Razor coverage and browser/Playwright automation
- FluentValidation production invocation issue discovered during planning
- Open authorization decisions, including previously recorded behavior/design questions
- Cleanup of the unchanged 28-warning non-incremental baseline
- Broader relational and report-query coverage beyond R1-R6
- SQLite remains rejected as an equivalent SQL Server verification provider

No Sprint 19 scope is assigned or implied.

## 10. Closure

Sprint 18 is complete and closed on the accepted T06 technical baseline. T07 changed documentation only. Graphify was not updated because repository convention does not include documentation-only changes in the source graph. Git operations remain owner-controlled and were not performed.
