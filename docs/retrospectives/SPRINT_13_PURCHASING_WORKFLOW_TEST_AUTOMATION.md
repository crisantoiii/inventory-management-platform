# Sprint 13 Retrospective — Purchasing Workflow Test Automation

> **SPRINT 13 STATUS: IN PROGRESS — T01 COMPLETE (T02–T08 NOT STARTED)**
>
> This document is the Sprint 13 **retrospective baseline**, created only after the Sprint 13 planning report (`plan/SPRINT_13_PLANNING_REPORT.md`, Revision 5) was explicitly accepted. At baseline creation it claimed no completed Sprint 13 work; execution updates are appended below as tasks actually complete (currently: **T01 complete**; T02–T08 not started). It will be **updated during execution** (task status table and execution log) and **finalized during T08** (Documentation Synchronization & Sprint 13 Closure).
>
> Authoritative planning baseline: `plan/SPRINT_13_PLANNING_REPORT.md` (Revision 5, accepted). Executable sprint control: `plan/SPRINT_13_TASK_BREAKDOWN.md` (external planning/control artifact — not intended for repository commit).

---

## 1. Sprint Identity

| Field | Value |
|---|---|
| **Sprint** | 13 |
| **Sprint title** | Purchasing Workflow Test Automation |
| **Status** | IN PROGRESS — T01 COMPLETE (T02–T08 NOT STARTED) |
| **Planning Gate** | PASS / ACCEPTED (Revision 5) |
| **Implementation Status** | IN PROGRESS — T01 COMPLETE |
| **Closure Status** | OPEN |
| **Retrospective Outcome** | OPEN - SPRINT IN PROGRESS (Section 12) |

## 2. Objective

Accepted Sprint 13 objective (planning report §11): establish risk-based **layered automated coverage** of the Purchasing workflow.

**Primary scope:**

- six Purchasing Application handlers (`CreatePurchaseOrderHandler`, `SubmitPurchaseOrderHandler`, `ApprovePurchaseOrderHandler`, `ReceivePurchaseOrderHandler`, `GetPurchaseOrderHandler`, `GetPurchaseOrdersHandler` — all confirmed in current source under `Application/Features/Purchasing/`)
- two Create validators (`CreatePurchaseOrderValidator`, `CreatePurchaseOrderItemValidator`)
- `PurchaseOrderErrors` / result behavior

**Supporting scope:**

- `PurchaseOrderRepository` integration verification (aggregate loading/includes, query filters, sorting, paging, persistence round-trip)

The two layers are complementary but are **not** executed together in one integrated scenario — the coverage is layered automated coverage, not end-to-end (planning report §11, Coverage precision).

## 3. Baseline

| Metric | Value |
|---|---|
| UnitTests | 219 passed, 0 failed, 0 skipped |
| IntegrationTests | 61 passed, 0 failed, 0 skipped |
| Web.Tests | 33 passed, 0 failed, 0 skipped |
| **Total** | **313 passed, 0 failed, 0 skipped** |
| Purchasing Domain baseline | 101 total |
| — PurchaseOrder workflow tests | 62 |
| — PurchaseOrderItem tests | 39 |
| Full-rebuild warning baseline | 28 (0 errors) |
| Product release baseline | v1.6.0 — Dynamic Capability-Based Authorization (current documented repository value; no new release invented) |

Sprint 13 only **adds** tests; the 313-test baseline must be preserved throughout (0 failed, 0 skipped). A target end-state test count is a sizing inference in the planning report and is **not** an acceptance criterion.

## 4. Task Status Table

| Task | Title | Status |
|---|---|---|
| T01 | Purchasing Test Support Foundation | COMPLETE |
| T02 | CreatePurchaseOrderHandler + Validator Tests | NOT STARTED |
| T03 | Workflow Transition Handler Tests (Submit/Approve) | NOT STARTED |
| T04 | ReceivePurchaseOrderHandler Tests | NOT STARTED |
| T05 | Purchase Order Query Handler Tests | NOT STARTED |
| T06 | PurchaseOrderRepository Integration Tests | NOT STARTED |
| T07 | Integrated Verification | NOT STARTED |
| T08 | Documentation Synchronization & Sprint 13 Closure | NOT STARTED |

## 5. Accepted Boundaries

Recorded from the accepted planning report; these hold for all of Sprint 13:

- No planned production changes (no task carries pre-authorized remediation authority)
- No new packages (including no mocking frameworks or FluentValidation test helpers)
- No database/migration/seed changes (test-side usage confined to EF Core InMemory, already a test dependency)
- No CI changes
- No WebApplicationFactory
- T07 EditStatus outside Sprint 13 (excluded even if the owner's decision is resolved mid-sprint; admission requires explicit sprint re-planning)
- No browser/end-to-end scope
- No SQL Server-specific integration testing (InMemory limitations accepted and documented)
- Production defect findings require separate approval: preserve failing evidence → identify exact behavior and affected files → do NOT modify production source → report the task `BLOCKED - REQUIRES SEPARATELY APPROVED REMEDIATION` → return for external review

## 6. Known Deferred Items

Carried forward **without promotion into Sprint 13** (from planning report §4/§7/§17 and prior sprint records):

- **EditStatus/T07 behavioral decision** — self-deactivation guard inconsistency; owner must choose option (a), (b), or (c); decision may be recorded independently at any time, implementation stays outside Sprint 13
- **WebApplicationFactory / HTTP authorization integration testing** — feasible but requires infrastructure preparation (package decision, DB-provider substitution, seeding strategy, possible `Program` accessibility seam); strong Sprint 14 candidate per planning report §8
- **CI provider decision** — no CI configuration exists; provider cannot be selected from repository evidence (workspace contains no `.git`; hosting unknown)
- **Build-warning cleanup** — 28 pre-existing full-rebuild warnings, including semantically risky member-hiding `CS0108`/`CS0114`; bounded future task, not a sprint objective
- **Future product features requiring separate deep planning** — Sales Module, Audit/Activity Logging, Bulk Import/Export, Barcode/QR completion, REST API (v2.0); none implementation-ready (no design work exists; database/authorization impacts unknown pending feature-specific planning)

## 7. Execution Log

### T01 — Purchasing Test Support Foundation (COMPLETE)

**Date:** September 7, 2026

**Objective:** Implement only the shared Purchasing unit-test support justified by multi-task reuse (Rule-of-Three) and required by T02–T05: `FakePurchaseOrderRepository`, `FakeUnitOfWork`, `PurchasingTestData`, and the shared entity-ID reflection helper. No handler/validator/repository behavioral tests.

**Mandatory investigation performed (per `knowledge.md`):** scoped `graphify query` first, then inspected the exact identified source files. All accepted contracts confirmed in current source — `IRepository<TEntity>` has 7 members; `PurchaseOrderRepository` overrides only `GetByIdAsync` (6 generic members inherited in production); `IPurchaseOrderRepository` adds `GetPagedAsync(PagedQuery, DateOnly?, DateOnly?, PurchaseOrderStatus?, CancellationToken)`; `IUnitOfWork` exposes only `Task<int> SaveChangesAsync(CancellationToken = default)`; `BaseEntity.Id` has a protected setter; `PurchaseOrder.Create(...)` is the factory; `PurchaseOrderItem` has no public ctor (items only via `PurchaseOrder.AddItem`); `Supplier`/`Product` public ctors are guard-checked. Additionally verified which members the six Purchasing handlers actually call (`GetByIdAsync`, `AddAsync`, plus `GetPagedAsync` per the accepted plan) — this grounded the fake's fail-fast member set. No discrepancy with the accepted task breakdown was found; no STOP condition triggered.

**Files created:**

- `tests/InventoryPlatform.UnitTests/TestSupport/Purchasing/FakePurchaseOrderRepository.cs`
- `tests/InventoryPlatform.UnitTests/TestSupport/Purchasing/FakeUnitOfWork.cs`
- `tests/InventoryPlatform.UnitTests/TestSupport/Purchasing/PurchasingTestData.cs`
- `tests/InventoryPlatform.UnitTests/TestSupport/Purchasing/EntityIdHelper.cs`

**Files modified (complete inventory, corrected during external review):** this retrospective, plus the Graphify generated artifacts refreshed by `graphify update .` (required after test-source changes): `graphify-out/graph.json`, `graphify-out/graph.html`, `graphify-out/GRAPH_REPORT.md`, `graphify-out/manifest.json`, `graphify-out/cache/stat-index.json` (plus AST-cache entries under `graphify-out/cache/ast/`). The original T01 record omitted the Graphify artifacts; the omission is corrected here.

**Support design (as implemented):**

- `FakePurchaseOrderRepository` implements the full current `IPurchaseOrderRepository`; supports only the interactions T02–T05 need (configurable `GetByIdAsync` result + call/id recording, `AddAsync` payload recording, `GetPagedAsync` argument recording via a `GetPagedAsyncCall` record + configurable `PagedResult<PurchaseOrder>` with a truthful empty page default); `GetAllAsync`/`FindAsync`/`Update`/`Remove`/`ExistsAsync` throw `NotSupportedException` (fail fast — no accepted Sprint 13 handler test exercises them, verified against current handler source).
- `FakeUnitOfWork` implements the single-member `IUnitOfWork` (no invented transaction API); records save count and records interactions into a test-provided `CallOrder` instance for T04 interaction-order assertions; returns 1 by default (configurable).
- `PurchasingTestData` builds valid Domain objects via real Domain APIs only: active/inactive Suppliers and Products (inactive produced via the real `Deactivate()`), Draft purchase orders via `PurchaseOrder.Create`, items only via `AddItem`, and transition-ready Submitted/Approved aggregates via real `Submit()`/`Approve()`. Fixed deterministic default dates; no `DateTime.Now`/`UtcNow` defaults.
- `EntityIdHelper.SetEntityId` mirrors the established private `SetEntityId` precedent in `CapabilityAuthorizationServiceTests`; promoted to shared Purchasing support because multiple Purchasing test classes need known aggregate ids. Production setters/constructors untouched.

**Explicitly excluded (per accepted disposition):** no shared `FakeSupplierRepository`, `FakeProductRepository`, or `FakeInventoryTransactionRepository`; no handler/validator/repository behavioral tests.

**Production changes:** none. **Test changes:** the four shared support files above; **no behavioral tests added.** **Database/migration/seed changes:** none. **Package changes:** none. **CI changes:** none.

**Tests executed:** `dotnet test src/InventoryPlatform/InventoryPlatform.slnx --no-build` (full three-project suite) — UnitTests **219 passed**, IntegrationTests **61 passed**, Web.Tests **33 passed**; total **313 passed, 0 failed, 0 skipped**. Baseline exactly preserved (T01 adds support code, no new tests).

**Build result:** incremental build succeeded, 0 warnings, 0 errors. Full rebuild (`--no-incremental`) succeeded with **28 warnings, 0 errors** — warning baseline unchanged; verified none of the 28 originate in the new T01 support files (only pre-existing production warnings, e.g., `CS0108` in `Pages/Purchasing/PurchaseOrders/Index.cshtml.cs`).

**Acceptance criteria status:** all 26 T01 criteria satisfied (see task completion report).

**Graphify update status:** `graphify update .` executed after the test-source changes — **success** (AST re-extraction, 6153 nodes; `graph.json`/`graph.html`/`GRAPH_REPORT.md` refreshed).

**Git operations:** none.

**Deferred work:** T02–T08; local/private fakes (`FakeSupplierRepository`, `FakeProductRepository`, `FakeInventoryTransactionRepository`) remain per the accepted disposition unless later reuse proves otherwise and external review approves promotion.

**Newly discovered issues:** none in production behavior. One T01-internal compile correction: the initial `CallOrder` recorder accidentally declared instance members in a `static` class (CS0708); fixed within T01 before the green build. Two pre-existing, harmless source observations recorded in Section 8 (no action taken — production changes are outside T01 authority).

### T01 External Review — Focused Verification / Revision (COMPLETE)

**Date:** September 7, 2026

**Scope:** review-identified items only — changed-file inventory reconciliation and CallOrder isolation verification. T02 not started; production source untouched; no Git commands.

**Item 1 — Changed-file inventory (corrected):** the original T01 report's Files Modified section omitted the Graphify generated artifacts even though the report itself stated `graphify update .` had refreshed them. The complete actual T01-changed inventory is now:

- *Test-support source files created (4):* `tests/InventoryPlatform.UnitTests/TestSupport/Purchasing/FakePurchaseOrderRepository.cs`, `FakeUnitOfWork.cs`, `PurchasingTestData.cs`, `EntityIdHelper.cs`
- *Retrospective documentation modified:* `docs/retrospectives/SPRINT_13_PURCHASING_WORKFLOW_TEST_AUTOMATION.md`
- *Graphify generated artifacts refreshed (by `graphify update .`, twice — after initial implementation and after the review revision):* `graphify-out/graph.json`, `graphify-out/graph.html`, `graphify-out/GRAPH_REPORT.md`, `graphify-out/manifest.json`, `graphify-out/cache/stat-index.json`, plus AST-cache entries under `graphify-out/cache/ast/`

Artifact/commit-treatment observation (evidence-based only): `graphify-out/` is present in the workspace and is a generated index; no Git tracking state was inspected or inferred (Git is user-controlled; no Git commands run).

**Item 2 — CallOrder isolation (finding confirmed, correction applied):** the reviewed implementation used `public static class CallOrder { private static int _sequence; ... }` — **process-global static mutable state**, unsafe under xUnit's parallel test-class execution (non-atomic `++_sequence`; any test calling `Reset()` could corrupt concurrent tests' ordering evidence). Corrected to the smallest test-scoped mechanism: `CallOrder` is now a per-test **instance** recorder (`sealed class`, `List<string> Events`, `Record(label)`); the test creates one instance per test, passes it to every participating fake via an optional constructor parameter, and asserts on the recorded event order. Both shared fakes record fake-prefixed labels into the provided recorder (`PurchaseOrderRepository.GetByIdAsync/AddAsync/GetPagedAsync`, `UnitOfWork.SaveChangesAsync`) and work unchanged when no recorder is supplied. `FakePurchaseOrderRepository` previously contained no sequencing state (verified) — only `FakeUnitOfWork.cs` and `FakePurchaseOrderRepository.cs` changed. No generic fake framework, no production abstraction, no production change; accepted T01 scope/dispositions unchanged (Rule-of-Three respected: the recorder exists because T04 needs cross-fake ordering evidence).

**Re-verification after the correction (required because test-support source changed):** incremental build succeeded (0 warnings, 0 errors); UnitTests alone: **219 passed, 0 failed, 0 skipped**; full regression gate: UnitTests **219** / IntegrationTests **61** / Web.Tests **33** = **313 passed, 0 failed, 0 skipped** (baseline preserved); full rebuild (`--no-incremental`): **28 warnings, 0 errors** — baseline unchanged, 0 warnings from T01 support files.

**Graphify update status (post-revision):** `graphify update .` re-run after the source change — **success** (6153 nodes; all five generated artifacts refreshed).

**Git operations:** none. **Production changes:** none. **T02–T08:** remain NOT STARTED.

## 8. Findings / Decisions

**T01 findings (September 7, 2026):**

- **No production defects found.** T01 created no behavioral tests against production behavior; the source-contract investigation confirmed every accepted planning-report claim (Section 12.1 matrix) against current source.
- **T01-internal issue (resolved within T01):** initial `CallOrder` implementation used instance members in a `static` class (compiler error CS0708); corrected before the regression gate. Not a production issue.
- **T01 external-review finding (resolved during review):** the same `CallOrder` recorder, once compilable, still held its counter in `static` fields — process-global mutable state that could interfere between independently executing xUnit tests. Corrected to a per-test instance-scoped recorder shared explicitly by each test with its participating fakes (see Execution Log, T01 External Review). Not a production issue; no production code involved.
- **Pre-existing observation (recorded only, no action — production changes outside T01 authority):** `Supplier` (`Domain/Entities/Supplier.cs`) declares no namespace (global namespace), unlike every other Domain entity. Harmless for tests (no using required); a cosmetic/style inconsistency only.
- **Pre-existing observation (recorded only, no action):** `PagedQuery.Status` is typed `ProductStatusFilter` (a Product-oriented filter enum) even though `PagedQuery` is shared across features; `PurchaseOrderRepository.GetPagedAsync` does not read it (it takes its own `PurchaseOrderStatus?`). Relevant context for T05/T06 pass-through assertions; not a defect.
- **Decision confirmed:** fail-fast (`NotSupportedException`) on `GetAllAsync`/`FindAsync`/`Update`/`Remove`/`ExistsAsync` in `FakePurchaseOrderRepository` — grounded in verified handler usage (only `GetByIdAsync`/`AddAsync`/`GetPagedAsync` are exercised by Purchasing handlers) and the accepted planning-report fake strategy.

## 9. Final Verification

**PENDING T07**

*(This section will record the T07 integrated verification results: solution build, UnitTests / IntegrationTests / Web.Tests results, final full regression, baseline comparison, warning comparison against the 28-warning full-rebuild baseline, and boundary verifications — no production changes, no package/database/migration/CI changes, T07 EditStatus not implemented, WebApplicationFactory not introduced, CI not introduced.)*

## 10. Final Documentation / Closure

**PENDING T08**

*(This section will record the T08 documentation synchronization and sprint closure: retrospective finalization, TESTING_CONVENTIONS / CHANGELOG / PROJECT_STATUS / README / ROADMAP / ENGINEERING_JOURNAL updates, FEATURES.md stale deferred-findings audit, and the DESIGN_DECISIONS.md inspection outcome.)*

## 11. Baseline References

- Planning baseline: `plan/SPRINT_13_PLANNING_REPORT.md` (Revision 5, accepted)
- Executable sprint control: `plan/SPRINT_13_TASK_BREAKDOWN.md` (external planning/control artifact — not intended for repository commit)
- Testing conventions: `docs/TESTING_CONVENTIONS.md`
- Project agent rules: `knowledge.md`

## 12. Retrospective Outcome

**OPEN - SPRINT IN PROGRESS (T01 COMPLETE; T02–T08 NOT STARTED)**

*(To be finalized during T08 after Sprint 13 execution and verification are actually complete.)*
