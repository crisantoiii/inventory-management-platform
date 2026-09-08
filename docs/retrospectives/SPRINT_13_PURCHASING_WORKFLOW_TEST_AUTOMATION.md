# Sprint 13 Retrospective — Purchasing Workflow Test Automation

> **SPRINT 13 STATUS: IN PROGRESS — T01–T06 COMPLETE (T07–T08 NOT STARTED)**
>
> This document is the Sprint 13 **retrospective baseline**, created only after the Sprint 13 planning report (`plan/SPRINT_13_PLANNING_REPORT.md`, Revision 5) was explicitly accepted. At baseline creation it claimed no completed Sprint 13 work; execution updates are appended below as tasks actually complete (currently: **T01–T06 complete**; T07–T08 not started). It will be **updated during execution** (task status table and execution log) and **finalized during T08** (Documentation Synchronization & Sprint 13 Closure).
>
> Authoritative planning baseline: `plan/SPRINT_13_PLANNING_REPORT.md` (Revision 5, accepted). Executable sprint control: `plan/SPRINT_13_TASK_BREAKDOWN.md` (external planning/control artifact — not intended for repository commit).

---

## 1. Sprint Identity

| Field | Value |
|---|---|
| **Sprint** | 13 |
| **Sprint title** | Purchasing Workflow Test Automation |
| **Status** | IN PROGRESS — T01–T06 COMPLETE (T07–T08 NOT STARTED) |
| **Planning Gate** | PASS / ACCEPTED (Revision 5) |
| **Implementation Status** | IN PROGRESS — T01–T06 COMPLETE |
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
| T02 | CreatePurchaseOrderHandler + Validator Tests | COMPLETE |
| T03 | Workflow Transition Handler Tests (Submit/Approve) | COMPLETE |
| T04 | ReceivePurchaseOrderHandler Tests | COMPLETE |
| T05 | Purchase Order Query Handler Tests | COMPLETE |
| T06 | PurchaseOrderRepository Integration Tests | COMPLETE |
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

### T02 — CreatePurchaseOrderHandler + Validator Tests (COMPLETE)

**Date:** September 7, 2026

**Objective:** Cover the accepted source-grounded contract for `CreatePurchaseOrderHandler`, `CreatePurchaseOrderValidator`, `CreatePurchaseOrderItemValidator`, and the `PurchaseOrderErrors` values used by this workflow, per the T02 contract matrix (planning report §12.1), consuming the accepted T01 shared support.

**Mandatory investigation performed (per `knowledge.md`):** scoped `graphify query` first, then inspected the exact identified source files: `CreatePurchaseOrderHandler.cs`, `CreatePurchaseOrderRequest.cs`, `CreatePurchaseOrderItemRequest.cs`, `CreatePurchaseOrderResponse.cs`, `CreatePurchaseOrderValidator.cs`, `CreatePurchaseOrderItemValidator.cs`, `PurchaseOrderErrors.cs`, `IPurchaseOrderRepository`/`ISupplierRepository`/`IProductRepository`/`IUnitOfWork`, `PurchaseOrder`/`PurchaseOrderItem`/`Supplier`/`Product`, `DomainException`, `Result`/`Result<T>`/`Error`, and all four T01 shared support files. All accepted contracts confirmed in current source. No discrepancy with the accepted task breakdown was found; no STOP condition triggered; no production change required.

**Files created:**

- `tests/InventoryPlatform.UnitTests/Application/Purchasing/CreatePurchaseOrderHandlerTests.cs` — 15 `[Fact]` methods = 15 discovered cases (+ local/private `FakeSupplierRepository` and `FakeProductRepository` implementing the full current interfaces with fail-fast unsupported members)
- `tests/InventoryPlatform.UnitTests/Application/Purchasing/CreatePurchaseOrderValidatorTests.cs` — 11 `[Fact]` + 1 `[Theory]` (3 `[InlineData]` rows) = 12 methods = 14 discovered cases
- `tests/InventoryPlatform.UnitTests/Application/Purchasing/CreatePurchaseOrderItemValidatorTests.cs` — 5 `[Fact]` + 3 `[Theory]` (8 `[InlineData]` rows) = 8 methods = 13 discovered cases
- `tests/InventoryPlatform.UnitTests/Application/Purchasing/PurchaseOrderErrorsTests.cs` — 4 `[Fact]` + 2 `[Theory]` (6 `[InlineData]` rows) = 6 methods = 10 discovered cases

Per-file test-method vs discovered-case inventory (corrected during external review; verified by `dotnet test --list-tests`):

| File | Fact methods | Theory methods | InlineData rows | Test methods | Discovered cases |
|---|---|---|---|---|---|
| CreatePurchaseOrderHandlerTests.cs | 15 | 0 | 0 | 15 | 15 |
| CreatePurchaseOrderValidatorTests.cs | 11 | 1 | 3 | 12 | 14 |
| CreatePurchaseOrderItemValidatorTests.cs | 5 | 3 | 8 | 8 | 13 |
| PurchaseOrderErrorsTests.cs | 4 | 2 | 6 | 6 | 10 |
| **Total** | **35** | **6** | **17** | **41** | **52** |

Note (external-review correction): the original T02 record listed per-file counts of 16/12/14/8 (= 50) — those figures were neither method counts nor discovered-case counts and were inaccurate. The authoritative inventory is 41 test methods expanding to **52 discovered test cases** (35 `[Fact]` + 6 `[Theory]` whose 17 `[InlineData]` data rows each execute as separate cases). The targeted-filter run of 42 covered only the three `CreatePurchaseOrder*` classes (15 + 14 + 13); `PurchaseOrderErrorsTests` contributes the remaining 10 discovered cases. No test-source change was required — only this record was corrected.

**Files modified:** this retrospective, plus the Graphify generated artifacts refreshed by `graphify update .` after the test-source changes.

**Behavior coverage (as implemented, evidence-grounded):** supplier not found (exact `PurchaseOrderErrors.SupplierNotFound` code/message; no product lookup, no add, no save); supplier inactive (`SupplierInactive`; no add/save); product not found (`ProductNotFound(productId)` parameterized; no add/save); product inactive (`ProductInactive(productId)`; no add/save); multi-item success (each item's product queried, both IDs recorded); second-item product-not-found short-circuit (no add/save); duplicate-product `DomainException` propagation (two product lookups occur — one per item — then the aggregate `AddItem` throws; no add/save; handler does not map the exception); successful creation (success result, `Draft` status, SupplierId/ExpectedDeliveryDate/Remarks/ProductId/Quantity/UnitCost preserved on the added aggregate, single add + single save); add-before-save ordering via the T01 instance-scoped `CallOrder` (one instance per test, shared with participating fakes; supplier/product lookups, `PurchaseOrderRepository.AddAsync` before `UnitOfWork.SaveChangesAsync`; failure path records no add/save); parent validator rules (SupplierId > 0 with exact message, ExpectedDeliveryDate NotEmpty via `DateOnly.MinValue`, Remarks ≤ 500 boundary, Items NotEmpty with exact message, per-item child validation through `RuleForEach` + `SetValidator` with `Items[n].ProductId` property paths); item validator rules (ProductId > 0 with exact message, Quantity > 0, UnitCost >= 0 with zero valid and negatives invalid); `PurchaseOrderErrors` codes/messages verified exactly for all four T02-used members including parameterized variants.

**T01 support usage:** `FakePurchaseOrderRepository`, `FakeUnitOfWork` (with instance-scoped `CallOrder`), `PurchasingTestData` (supplier/product builders, inactive variants via real `Deactivate()`), `EntityIdHelper`. **No T01 support correction was needed; no shared fakes were modified; no shared Supplier/Product fake was introduced** — both local fakes remain private to the T02 handler test file per the accepted disposition.

**Production changes:** none. **Test changes:** the four new test files above — 41 test methods = **52 new discovered test cases**. **Database/migration/seed changes:** none. **Package changes:** none. **CI changes:** none. **WebApplicationFactory:** not introduced. **EditStatus/T07:** untouched.

**Tests executed:** targeted T02 filter (`FullyQualifiedName~...Application.Purchasing.CreatePurchaseOrder`) — 42 passed, 0 failed, 0 skipped (the three `CreatePurchaseOrder*` classes only: 15 + 14 + 13); full solution suite (`dotnet test src/InventoryPlatform/InventoryPlatform.slnx`) — UnitTests **271 passed**, IntegrationTests **61 passed**, Web.Tests **33 passed**; total **365 passed, 0 failed, 0 skipped**. Baseline 313 preserved; **+52 new T02 discovered test cases** (arithmetic: 219 + 52 = 271 UnitTests; 313 + 52 = 365 total; per-class discovery 15 + 14 + 13 + 10 = 52, verified by `dotnet test --list-tests`).

**Build result:** incremental build succeeded, **0 warnings, 0 errors** (T02 files introduce no warnings; the earlier CS8602 in the handler test file was fixed with null-forgiving assertions before the gate). Full rebuild (`--no-incremental`) succeeded with **28 warnings, 0 errors** — warning baseline unchanged; all 28 are pre-existing production warnings, none originate in T02 files.

**Acceptance criteria status:** all 43 T02 criteria satisfied (see task completion report).

**Graphify update status:** `graphify update .` executed after the test-source changes — **success** (AST re-extraction, 9 uncached files; graph now 6255 nodes, 10698 edges, 493 communities; `graph.json`/`graph.html`/`GRAPH_REPORT.md` refreshed).

**Git operations:** none.

**Deferred work:** T03–T08; `FakeInventoryTransactionRepository` remains for T04.

**Newly discovered issues:** none in production behavior. One T02-internal test-authoring correction recorded honestly: an initial draft asserted `AddAsyncCallCount == 1` for the second-item-product-not-found path, but the handler short-circuits before `AddAsync` (add happens only after ALL items validate); the assertion was corrected to `0` to match the confirmed source contract. Also confirmed: the handler queries each item's product once per item (duplicate-product requests therefore cause two product lookups before the aggregate throws), and the created aggregate's Id remains 0 in unit scope (persistence-assigned; the response echoes the aggregate's current Id). No production change was made or needed.

### T02 External Review — Test Count Reconciliation (COMPLETE)

**Date:** September 7, 2026

**Scope:** count-investigation only. No test-source or production-source change; no Git commands.

**Finding:** the original T02 record listed per-file counts of 16/12/14/8 (= 50) — neither method counts nor discovered-case counts — alongside a correct 52-increase claim and a spurious "+2" note. Authoritative evidence (`dotnet test --list-tests` + source annotation counts): the four T02 files contain **41 test methods** (35 `[Fact]` + 6 `[Theory]`) expanding via 17 `[InlineData]` rows into **52 discovered test cases** — Handler 15, Validator 14, ItemValidator 13, Errors 10. The 42-count targeted run covered only the three `CreatePurchaseOrder*` classes (15+14+13); `PurchaseOrderErrorsTests` (10) ran only in the full-suite pass. Arithmetic reconciled exactly: 219+52=271; 313+52=365.

**Correction:** the T02 record's file inventory and tests-executed sections were replaced with the verified methods-vs-discovered table and reconciliation arithmetic. **No test-source change was required.** Graphify update not required (documentation-only change). T02 remains COMPLETE; T03–T08 remained NOT STARTED at review time.

### T03 — Workflow Transition Handler Tests (Submit/Approve) (COMPLETE)

**Date:** September 7, 2026

**Objective:** Cover the accepted source-grounded orchestration contract for `SubmitPurchaseOrderHandler` and `ApprovePurchaseOrderHandler` (load → not-found failure → aggregate transition → save → success response), including `DomainException` propagation on invalid states and the `PurchaseOrderErrors.NotFound` contract both handlers use, per the T03 contract matrix (planning report §12.1), consuming the accepted T01 shared support.

**Mandatory investigation performed (per `knowledge.md`):** scoped `graphify query` first, then inspected the exact identified source files: `SubmitPurchaseOrderHandler.cs`, `SubmitPurchaseOrderRequest.cs`, `SubmitPurchaseOrderResponse.cs`, `ApprovePurchaseOrderHandler.cs`, `ApprovePurchaseOrderRequest.cs`, `ApprovePurchaseOrderResponse.cs`, `PurchaseOrderErrors.cs` (NotFound contract unchanged from T02 inspection), `PurchaseOrder.cs` (`Submit`/`Approve` preconditions unchanged), `DomainException.cs`, all four T01 support files, and the T02 Purchasing test conventions. Both handlers match the accepted contract exactly (GetByIdAsync → null ⇒ `Failure(PurchaseOrderErrors.NotFound)` with no save → `purchaseOrder.Submit()`/`Approve()` → `SaveChangesAsync` → `Success(new Response(Id, Status))`). No discrepancy with the accepted task breakdown; no STOP condition; no production change required.

**Files created:**

- `tests/InventoryPlatform.UnitTests/Application/Purchasing/SubmitPurchaseOrderHandlerTests.cs` — 6 `[Fact]` methods = 6 discovered cases
- `tests/InventoryPlatform.UnitTests/Application/Purchasing/ApprovePurchaseOrderHandlerTests.cs` — 6 `[Fact]` methods = 6 discovered cases

**Files modified:** `tests/InventoryPlatform.UnitTests/Application/Purchasing/PurchaseOrderErrorsTests.cs` (added the `PurchaseOrderErrors.NotFound` contract test — the one error T03 newly exercises; +1 `[Fact]` = +1 discovered case), this retrospective, plus the Graphify generated artifacts refreshed by `graphify update .` after the test-source changes.

**Behavior coverage (as implemented, evidence-grounded):** Submit not-found (exact `PurchaseOrderErrors.NotFound` code/message, repository requested the exact id, no save); Submit success from a valid Draft+item aggregate built via real Domain APIs (status becomes `Submitted` on both the response and the tracked aggregate, response Id matches, exactly one save); Submit load→save ordering via the T01 instance-scoped `CallOrder` (exact two-event sequence `GetByIdAsync` → `SaveChangesAsync`); Submit empty-Draft `DomainException` propagation ("at least one item"; aggregate remains Draft; no save); Submit invalid non-Draft states via real transitions (Submitted and Approved both throw "Only draft"; aggregate state unchanged; no save); Approve not-found (exact NotFound error, no save); Approve success from a real Submitted aggregate (status becomes `Approved` on response and aggregate, response Id matches, one save); Approve load→save ordering via `CallOrder`; Approve Draft-with-item invalid state ("Only submitted"; no save); Approve already-Approved invalid state via real transitions (no save); Approve Receiving invalid state via real transitions (Draft+item → Submit → Approve → partial Receive; "Only submitted"; no save). Domain behavior itself is not re-duplicated — the T03 tests prove Application-boundary orchestration and exception propagation only, reusing the established Domain-level coverage.

**T01 support usage:** `FakePurchaseOrderRepository` (configurable `GetByIdAsync` result, call/id recording, `CallOrder` recording), `FakeUnitOfWork` (save-count + `CallOrder` recording), `PurchasingTestData` (`CreateDraftPurchaseOrderWithItem`, `CreateDraftPurchaseOrder`, `CreateSubmittedPurchaseOrder`, `CreateApprovedPurchaseOrder`), instance-scoped `CallOrder` (one per test, passed to both participating fakes). **No T01 support correction was needed; no new shared fake was introduced** — both handlers need only the two shared fakes, so the Rule-of-Three disposition held without additions.

**Production changes:** none. **Test changes:** the two new handler test files above (12 test methods / 12 discovered cases) plus the one added `NotFound` error-contract test (1 method / 1 discovered case) — **13 new test methods = 13 new discovered test cases total**. **Database/migration/seed changes:** none. **Package changes:** none. **CI changes:** none. **WebApplicationFactory:** not introduced. **EditStatus/T07:** untouched.

**Tests executed:** targeted Purchasing filter (`FullyQualifiedName~InventoryPlatform.UnitTests.Application.Purchasing`) — 65 passed, 0 failed, 0 skipped; discovery via `dotnet test --list-tests` confirmed per-class discovered counts: Submit 6, Approve 6, PurchaseOrderErrorsTests 11 (10 prior + 1 new NotFound); full solution suite (`dotnet test src/InventoryPlatform/InventoryPlatform.slnx`) — UnitTests **284 passed**, IntegrationTests **61 passed**, Web.Tests **33 passed**; total **378 passed, 0 failed, 0 skipped**. Baseline 365 preserved; **+13 new T03 discovered test cases** (arithmetic: 271 + 13 = 284 UnitTests; 365 + 13 = 378 total).

**Build result:** incremental build succeeded, **0 warnings, 0 errors**. Full rebuild (`--no-incremental`) succeeded with **28 warnings, 0 errors** — warning baseline unchanged; verified none of the 28 originate in the T03 test files (grep over full-rebuild output for the T03 file names returned no warnings).

**Acceptance criteria status:** all 37 T03 criteria satisfied (see task completion report).

**Graphify update status:** `graphify update .` executed after the test-source changes — **success** (graph now 6277 nodes, 10832 edges, 510 communities; `graph.json`/`graph.html`/`GRAPH_REPORT.md` refreshed).

**Git operations:** none.

**Deferred work:** T04–T08; local/private `FakeInventoryTransactionRepository` and per-file `FakeProductRepository` remain for T04 per the accepted disposition.

**Newly discovered issues:** none in production behavior. The handlers match the accepted contract exactly; both `DomainException` propagation paths and the no-save-on-exception behavior were confirmed in current source and are now protected by tests. No production change was made or needed.

### T04 — ReceivePurchaseOrderHandler Tests (COMPLETE)

**Date:** September 7, 2026

**Objective:** Cover the accepted source-grounded orchestration contract for `ReceivePurchaseOrderHandler` (PO load → Product load → aggregate `Receive` → `IncreaseStock` → `InventoryTransaction` creation → transaction `AddAsync` → save → success response), including `DomainException` propagation on all invalid paths, the separately loaded Product's stock mutation, the created transaction's fields, and observable cross-dependency ordering, per the T04 contract matrix (planning report §12.1), consuming the accepted T01 shared support.

**Mandatory investigation performed (per `knowledge.md`):** scoped `graphify query` first, then inspected the exact identified source files: `ReceivePurchaseOrderHandler.cs`, `ReceivePurchaseOrderRequest.cs`, `ReceivePurchaseOrderResponse.cs`, `PurchaseOrderErrors.cs`, `IProductRepository.cs`, `IInventoryTransactionRepository.cs`, `IUnitOfWork.cs`, `InventoryTransaction.cs`, `TransactionType.cs`, `PurchaseOrder.cs` (`Receive` preconditions unchanged from T02/T03 inspection), `DomainException.cs`, all four T01 support files, and the T02/T03 Purchasing test conventions. Handler contract confirmed exactly as accepted, including the critical fact that the stock-mutated Product is loaded separately through `IProductRepository.GetByIdAsync(request.ProductId)` — not the repository's Include-chain navigation. Source-level details verified for the tests: the handler's namespace is `...ReceivePurchaseOrder` (folder `ReceivingPurchaseOrder`); the response property is `PurchaseOrderId` (not `Id`); `InventoryTransaction` ctor is `(productId, transactionType, quantity, referenceNumber, remarks, transactionDateUtc)` with exact strings `"PO-{Id}"` / `"Purchase Order {Id} receiving"` and `TransactionType.StockIn`; the `TransactionType` enum is declared in the global namespace (like `Supplier`, no using required). No discrepancy with the accepted task breakdown; no STOP condition; no production change required.

**Files created:**

- `tests/InventoryPlatform.UnitTests/Application/Purchasing/ReceivePurchaseOrderHandlerTests.cs` — 14 `[Fact]` methods = 14 discovered cases, with local/private nested `FakeProductRepository` (GetByIdAsync + id/call recording, fail-fast otherwise) and `FakeInventoryTransactionRepository` (AddAsync + payload/call recording, fail-fast otherwise)

**Files modified:** this retrospective, plus the Graphify generated artifacts refreshed by `graphify update .` after the test-source changes.

**Behavior coverage (as implemented, evidence-grounded):** PO not found (exact `PurchaseOrderErrors.NotFound` code/message; Product lookup short-circuits — zero product-repository calls; no transaction; no save); Product not found (exact parameterized `ProductNotFound(999)` code/message; aggregate state unchanged; no transaction; no save); invalid state via real Domain transitions (Draft-with-item and Submitted both throw "Only approved"; stock unchanged; no transaction; no save); missing PurchaseOrder item (approved order contains only product 10, request targets separately existing product 20 → "not found"; loaded product's stock unchanged; no transaction; no save); zero quantity ("greater than zero"; nothing mutated/persisted); negative quantity (same guard; nothing mutated/persisted); over-receiving (11 > remaining 10 → "cannot exceed"; stock unchanged; nothing persisted); cumulative over-receiving (prior partial receive of 6, then 5 more → 11 > 10 → "cannot exceed"; only the in-arrangement receive reflected; nothing persisted); successful partial receive (approved order for 10, receive 4 → success, response `PurchaseOrderId`=7/`Receiving`, aggregate `Receiving`, item ReceivedQuantity 4/RemainingQuantity 6/not fully received, product stock exactly 4, exactly one transaction with ProductId 10/`StockIn`/quantity 4/`PO-7`/`"Purchase Order 7 receiving"`, one save); successful full receive (receive all 10 → `Completed` on response and aggregate, item fully received/remaining 0, stock exactly 10, one transaction, one save); final-full-receive-after-partial via real transitions (prior 6 then handler receives remaining 4 → `Completed`, stock exactly 4 — only the handler's receive touches the separately loaded product); observable ordering via the T01 instance-scoped `CallOrder` (one instance per test, shared with all four participating fakes): exact sequence `PurchaseOrderRepository.GetByIdAsync` → `ProductRepository.GetByIdAsync` → `InventoryTransactionRepository.AddAsync` → `UnitOfWork.SaveChangesAsync` on success, and only the two load events on the product-not-found path. No exact `DateTime.UtcNow` assertion — the transaction timestamp is production-created and was deliberately not asserted flakily.

**T01 support usage:** `FakePurchaseOrderRepository` (configurable `GetByIdAsync` result + call/id recording + `CallOrder` recording), `FakeUnitOfWork` (save-count + `CallOrder` recording), `PurchasingTestData` (`CreateApprovedPurchaseOrder`, `CreateDraftPurchaseOrderWithItem`, `CreateSubmittedPurchaseOrder`, `CreateProduct`), `EntityIdHelper` (via the builders), instance-scoped `CallOrder`. **No T01 support correction was needed; no new shared fake was introduced** — the Product and InventoryTransaction fakes remain private/nested in the T04 test file per the accepted disposition.

**Production changes:** none. **Test changes:** the one new test file above — 14 test methods = 14 discovered test cases (all `[Fact]`, no theories). **Database/migration/seed changes:** none. **Package changes:** none. **CI changes:** none. **WebApplicationFactory:** not introduced. **EditStatus/T07:** untouched.

**Tests executed:** targeted T04 filter (`FullyQualifiedName~...Application.Purchasing.ReceivePurchaseOrderHandlerTests`) — 14 passed, 0 failed, 0 skipped; discovery via `dotnet test --list-tests` confirmed 14 discovered cases in the T04 class; full solution suite (`dotnet test src/InventoryPlatform/InventoryPlatform.slnx`) — UnitTests **298 passed**, IntegrationTests **61 passed**, Web.Tests **33 passed**; total **392 passed, 0 failed, 0 skipped**. Baseline 378 preserved; **+14 new T04 discovered test cases** (arithmetic: 284 + 14 = 298 UnitTests; 378 + 14 = 392 total).

**Build result:** incremental build succeeded, **0 warnings, 0 errors**. Full rebuild (`--no-incremental`) succeeded with **28 warnings, 0 errors** — warning baseline unchanged; verified none of the 28 originate in the T04 test file (grep over full-rebuild output for the T04 file name returned no warnings).

**Acceptance criteria status:** all 51 T04 criteria satisfied (see task completion report).

**Graphify update status:** `graphify update .` executed after the test-source changes — **success** (graph now 6327 nodes, 11112 edges, 498 communities; `graph.json`/`graph.html`/`GRAPH_REPORT.md` refreshed).

**Git operations:** none.

**Deferred work:** T05–T08.

**Newly discovered issues:** none in production behavior. The handler matches the accepted contract exactly, including the separately-loaded-Product stock mutation and the exact transaction reference/description strings. All `DomainException` paths leave stock unchanged, add no transaction, and never save — confirmed in current source and now test-protected. No production change was made or needed.

### T05 — Purchase Order Query Handler Tests (COMPLETE)

**Date:** September 8, 2026

**Objective:** Cover the accepted source-grounded mapping and pass-through contract for `GetPurchaseOrderHandler` (detail query) and `GetPurchaseOrdersHandler` (paged list query) per the T05 contract matrix (planning report §12.1), consuming the accepted T01 shared support. No T06 repository integration scope.

**Mandatory investigation performed (per `knowledge.md`):** scoped `graphify query` first, then inspected the exact identified source files: `GetPurchaseOrderHandler.cs`, `GetPurchaseOrderRequest.cs`, `GetPurchaseOrderResponse.cs`, `GetPurchaseOrderItemResponse.cs`, `GetPurchaseOrdersHandler.cs`, `GetPurchaseOrdersRequest.cs`, `GetPurchaseOrdersResponse.cs`, `GetPurchaseOrderSummaryResponse.cs`, `PurchaseOrderErrors.cs`, `IPurchaseOrderRepository.cs`, `PagedQuery.cs`, `PagedRequest.cs`, `PagedResult.cs`, `Result.cs`, `ResultOfT.cs`, `Error.cs`, `PurchaseOrder.cs`, `PurchaseOrderItem.cs`, `Supplier.cs`, `Product.cs`, `PurchaseOrderStatus.cs`, `BaseEntity.cs`, all four T01 support files, and the T02–T04 Purchasing test conventions. All accepted contracts confirmed in current source: the detail handler maps Id/SupplierId/`Supplier.Name`/OrderDate/ExpectedDeliveryDate/Status/Remarks/TotalAmount plus per-item ProductId/`Product.Sku`/`Product.Name`/Quantity/UnitCost/LineTotal/ReceivedQuantity/RemainingQuantity/IsFullyReceived; the list handler constructs `PagedQuery { PageNum, PageSize, Search, SortBy, Descending }` (it does NOT copy `PagedRequest.Status` — consistent with the recorded T01 finding about the Product-oriented `ProductStatusFilter`), forwards `GetPagedAsync(query, FromDate, ToDate, PurchaseOrderStatus, ct)`, copies `Page/PageSize/TotalCount` from the repository result, and has **no failure branch**. Two current-source facts additionally verified: `GetPurchaseOrdersResponse` exposes the page as property `PurchaseOrders` (not `Items`), and `PagedRequest` clamps `PageNum < 1` to 1 and `PageSize > 100` to 100 at init time. No discrepancy with the accepted task breakdown; no STOP condition; no production change required.

**Files created:**

- `tests/InventoryPlatform.UnitTests/Application/Purchasing/GetPurchaseOrderHandlerTests.cs` — 7 `[Fact]` methods = 7 discovered cases
- `tests/InventoryPlatform.UnitTests/Application/Purchasing/GetPurchaseOrdersHandlerTests.cs` — 9 `[Fact]` methods = 9 discovered cases

Per-file inventory (verified by `dotnet test --list-tests`; all `[Fact]`, no theories):

| File | Test methods | Discovered cases |
|---|---|---|
| GetPurchaseOrderHandlerTests.cs | 7 | 7 |
| GetPurchaseOrdersHandlerTests.cs | 9 | 9 |
| **Total** | **16** | **16** |

**Files modified:** this retrospective, plus the Graphify generated artifacts refreshed by `graphify update .` after the test-source changes.

**Behavior coverage (as implemented, evidence-grounded):** detail handler — not-found (exact `PurchaseOrderErrors.NotFound` code/message, repository requested the exact id, `Value` null, no fabricated response); full header mapping (Id 7, SupplierId 3, `Supplier.Name` "Acme Supplies", OrderDate, ExpectedDeliveryDate, Draft status, Remarks, TotalAmount 125.00 = 5×25); optional-field absence (null ExpectedDeliveryDate/Remarks still mapped, Supplier navigation still populated); item mapping including navigation-derived fields (ProductId, ProductSku "SKU-10", ProductName "Standard Widget", Quantity, UnitCost, LineTotal 125.00, ReceivedQuantity 0, RemainingQuantity 5, IsFullyReceived false); multi-item mapping (2 items, TotalAmount 155.00 = 125+30, per-item navigation fields); partial-receive state via real Domain transitions (Approved 10×25 → Receive(4) → response status `Receiving`, item ReceivedQuantity 4/RemainingQuantity 6/IsFullyReceived false, TotalAmount unchanged 250.00); fully-received state via real Domain transitions (Receive(10) → response status `Completed`, ReceivedQuantity 10/RemainingQuantity 0/IsFullyReceived true). List handler — non-default pass-through (PageNum 3, PageSize 25, Search "widget", SortBy "Supplier", Descending true, FromDate, ToDate, status `Approved`, all recorded via the fake's `GetPagedAsyncCall`); null-filter pass-through; canonical `PageNum` contract preserved (request `PageNum = 0` clamps to 1 by `PagedRequest` and the clamped value is forwarded); oversized `PageSize` clamped to 100 and forwarded; summary mapping with populated Supplier navigation (Id, SupplierName, OrderDate, Draft status, TotalAmount per aggregate); summary TotalAmount reflects aggregate line-total sums (155.00); paged metadata copied verbatim from the repository result (Page 2/PageSize 20/TotalCount 57, Items mapped); empty result page (handler succeeds, Items empty, metadata preserved, TotalCount 0, no failure invented); unconfigured-fake empty page (the fake's truthful default empty page flows through as success). **T05 does NOT verify real EF Includes, `AsNoTracking`, SQL translation, real filtering/sorting/paging, or persistence round-trips — those belong to T06.**

**Navigation fixture rules (as implemented):** the handlers dereference `po.Supplier.Name` and `item.Product.Sku/Name`, but `PurchaseOrder.Supplier` and `PurchaseOrderItem.Product` expose private setters with no public attach API (EF Core populates them). Per the accepted Navigation Fixture Rules: Domain construction/test precedent was inspected first (no existing precedent populates these navigations), real relationship setup is not available through public Domain APIs, so the smallest test-only mechanism was used — reflection helpers **private to each T05 test file** (`AttachSupplier`/`AttachProduct` and the `CreateSummaryPurchaseOrder` helper), mirroring the established `EntityIdHelper` precedent. No production visibility or setter was changed; no shared-support expansion was made.

**T01 support usage:** `FakePurchaseOrderRepository` (configured `GetByIdAsync` result + call/id recording; configured `PagedResult<PurchaseOrder>` + full `GetPagedAsyncCall` argument recording; truthful empty-page default), `PurchasingTestData` (`CreateDraftPurchaseOrder`/`CreateDraftPurchaseOrderWithItem`/`CreateApprovedPurchaseOrder`/`CreateSupplier`/`CreateProduct`, `DefaultOrderDate`), `EntityIdHelper` (via the builders). **No T01 support correction was needed; no new shared fake was introduced** — the fake already supported everything T05 needs, exactly as T01 designed it.

**Production changes:** none. **Test changes:** the two new test files above — 16 test methods = 16 discovered test cases (all `[Fact]`, no theories). **Database/migration/seed changes:** none. **Package changes:** none. **CI changes:** none. **WebApplicationFactory:** not introduced. **EditStatus/T07:** untouched.

**Tests executed:** targeted T05 filter (`FullyQualifiedName~...Application.Purchasing.GetPurchaseOrder`) — 16 passed, 0 failed, 0 skipped; discovery via `dotnet test --list-tests` confirmed 16 discovered cases (7 + 9); full solution suite (`dotnet test src/InventoryPlatform/InventoryPlatform.slnx`) — UnitTests **314 passed**, IntegrationTests **61 passed**, Web.Tests **33 passed**; total **408 passed, 0 failed, 0 skipped**. Baseline 392 preserved; **+16 new T05 discovered test cases** (arithmetic: 298 + 16 = 314 UnitTests; 392 + 16 = 408 total).

**Build result:** incremental build succeeded, **0 warnings, 0 errors**. Full rebuild (`--no-incremental`) succeeded with **28 warnings, 0 errors** — warning baseline unchanged; verified none of the 28 originate in the T05 test files (grep over full-rebuild output for the T05 file names returned no warnings).

**Acceptance criteria status:** all 60 T05 criteria satisfied (see task completion report).

**Graphify update status:** `graphify update .` executed after the test-source changes — **success** (graph now 6356 nodes, 11287 edges, 493 communities; `graph.json`/`graph.html`/`GRAPH_REPORT.md` refreshed).

**Git operations:** none.

**Deferred work:** T06–T08. The T05 fake-based tests deliberately do not verify real EF repository behavior (Includes, filtering, sorting, paging, persistence) — that verification belongs to T06.

**Newly discovered issues:** none in production behavior. Both handlers match the accepted contract exactly, including the no-failure-branch fact of `GetPurchaseOrdersHandler` and the `PurchaseOrders`-property response shape. One T05-internal test-authoring correction recorded honestly: the initial draft of the populated-page summary test arranged aggregates without items and asserted non-zero totals; the totals are aggregate-derived (`Items.Sum(Quantity * UnitCost)`), so the fixture was corrected to add items before the gate. Two pre-existing observations remain recorded-only (Section 8): `PagedQuery.Status` is typed `ProductStatusFilter` and is not copied by the list handler (current-source behavior, tested as-is), and the Supplier/Product navigation attach requires test-only reflection (no production change authorized). No production change was made or needed.

### T06 — PurchaseOrderRepository Integration Tests (COMPLETE)

**Date:** September 8, 2026

**Objective:** Cover the real current `PurchaseOrderRepository` (aggregate loading/Includes, query filters, sorting, paging, AsNoTracking where observable, persistence round-trip) using the existing IntegrationTests project and its EF Core InMemory pattern, with the mandatory fresh-context isolation pattern for navigation-loading and round-trip assertions. No T07 verification scope.

**Mandatory investigation performed (per `knowledge.md`):** scoped `graphify query` first, then inspected the exact identified source files: `PurchaseOrderRepository.cs`, `Repository.cs` (base), `IPurchaseOrderRepository.cs`, `IRepository.cs`, `ApplicationDBContext.cs`, `PurchaseOrderConfiguration.cs`, `PurchaseOrderItemConfiguration.cs`, `SupplierConfiguration.cs`, `ProductConfiguration.cs`, `Category.cs`, `Unit.cs`, `PagedQuery.cs`, `PagedRequest.cs`, `PagedResult.cs`, `PurchaseOrderSortFields.cs`, the IntegrationTests project file, and both existing repository-test precedents (`CapabilityRepositoryTests`, `AuthorizationGroupRepositoryTests`). All accepted contracts confirmed in current source: `GetByIdAsync` overrides the base with `Include(Supplier).Include(Items).ThenInclude(Product)` + `FirstOrDefaultAsync` and is **tracked** (no AsNoTracking); `GetPagedAsync` uses `AsNoTracking().Include(Supplier).Include(Items)`, trims search then branches numeric (`po.Id == parsedId || Supplier.Name.Contains(search)`) vs name-contains, applies `fromDate >=` / `toDate <=` / exact-status filters, counts before sorting, sorts via `ApplySorting` on `PurchaseOrderSortFields` (Id/Supplier/OrderDate/Status/TotalAmount, asc/desc; fallback `OrderDate desc, Id desc`), pages with `Skip((PageNum-1)*PageSize).Take(PageSize)`, and echoes `Page = query.PageNum`, `PageSize`, `TotalCount`. No discrepancy with the accepted task breakdown; no STOP condition; no production change required.

**Files created:**

- `tests/InventoryPlatform.IntegrationTests/Purchasing/PurchaseOrderRepositoryTests.cs` — 24 `[Fact]` methods = 24 discovered cases (verified by `dotnet test --list-tests`)

Coverage areas (24 methods): GetByIdAsync not-found (1); GetByIdAsync Includes from fresh query context — single-item aggregate with Supplier/Items/ThenInclude(Product) + full header shape (1), multi-item with per-item Product/Quantity/UnitCost (1); GetPagedAsync search — supplier-name match (1), numeric id match (1), numeric OR-branch via supplier name containing digits (1), no-match empty page (1), whitespace search ignored (1); FromDate inclusive boundary (1), ToDate inclusive boundary (1), combined range (1); status filter (1); sorting — Id asc (1), Id desc (1), Supplier name (1), OrderDate (1), Status (1), TotalAmount by `Items.Sum(Quantity * UnitCost)` (1), default `OrderDate desc, Id desc` fallback with distinct seeded dates (1); paging — page slicing incl. out-of-range page (1), Page/PageSize/TotalCount metadata (1); composed filter+sort+page (1); AsNoTracking via fresh-context ChangeTracker (`EntityState.Detached` on purchase orders and items) (1); persistence round-trip — repository `AddAsync` + first save → first context disposal → fresh `GetByIdAsync` reload (full aggregate-shape assertions, Draft status) → real `Submit()` mutation on the freshly loaded aggregate → `Update` + second save → second context disposal → final fresh `GetByIdAsync` reload asserting persisted `Submitted` state (1; corrected during external review — see the T06 review record below).

**Files modified:** this retrospective, plus the Graphify generated artifacts refreshed by `graphify update .` after the test-source changes.

**Fresh-context isolation evidence (as implemented):** the test class constructor generates a unique database name per test instance (`$"PORepoTest_{Guid.NewGuid():N}"` — xUnit creates a new class instance per test, so every test gets its own isolated InMemory database). Every navigation-loading and round-trip test: (1) arranges data through one or more short-lived arrange contexts, (2) saves and disposes them (`using` scopes), (3) creates a fresh query context against the same database name, (4) constructs the real `PurchaseOrderRepository` on the fresh context, (5) executes the repository query, and (6) asserts. Because the arrange contexts are disposed, their change trackers cannot fix up navigations into the query results — only the repository's Include chain can populate `Supplier`, `Items`, and `Item.Product`, so the Include assertions cannot be satisfied by arrange-context fix-up. The isolation reason is documented in the test-class XML doc header, per the accepted task breakdown.

**Fixture design notes (test-side, no production impact):** dependency entities are persisted before aggregates that reference them — the Product constructor guard-rejects zero/negative category/unit ids, so Category/Unit are saved first and the Product is built with persistence-assigned ids; `PurchaseOrder.Create` is called with the persisted SupplierId and items are added with persisted ProductIds (no private-setter writes, no reflection in T06). `Repository.AddAsync` does not save, so round-trip tests call `SaveChangesAsync` explicitly, mirroring the established repository-test precedent. Seeded fixtures use distinct suppliers/dates/statuses/totals so search, filter, sort, and paging assertions stay unambiguous.

**InMemory limitation (recorded as required):** EF Core InMemory does NOT prove SQL Server SQL translation, collation behavior, FK/unique constraint enforcement, transaction semantics, provider-specific date/string behavior, or relational performance/query-plan behavior. These tests provide repository wiring and query-shape regression coverage only, consistent with `docs/TESTING_CONVENTIONS.md` and the accepted planning report.

**Production changes:** none. **Test changes:** the one new test file above — 24 test methods = 24 discovered test cases (all `[Fact]`, no theories). **Database/migration/seed changes:** none (runtime-only isolated InMemory databases; no EnsureCreated needed for the InMemory provider). **Package changes:** none (existing `Microsoft.EntityFrameworkCore.InMemory` reference reused). **CI changes:** none. **WebApplicationFactory:** not introduced. **EditStatus/T07:** untouched.

**Tests executed:** targeted T06 filter (`FullyQualifiedName~InventoryPlatform.IntegrationTests.Purchasing`) — 24 passed, 0 failed, 0 skipped; discovery via `dotnet test --list-tests` confirmed 24 discovered cases in the T06 class; IntegrationTests project — **85 passed**, 0 failed, 0 skipped; full solution suite (`dotnet test src/InventoryPlatform/InventoryPlatform.slnx`) — UnitTests **314 passed**, IntegrationTests **85 passed**, Web.Tests **33 passed**; total **432 passed, 0 failed, 0 skipped**. Baseline 408 preserved; **+24 new T06 discovered test cases** (arithmetic: 61 + 24 = 85 IntegrationTests; 408 + 24 = 432 total).

**Build result:** incremental build succeeded, **0 warnings, 0 errors**. Full rebuild (`--no-incremental`) succeeded with **28 warnings, 0 errors** — warning baseline unchanged; grep over full-rebuild output confirmed no warning originates in the T06 test file.

**Acceptance criteria status:** all 58 T06 criteria satisfied (see task completion report).

**Graphify update status:** `graphify update .` executed after the test-source changes — **success** (graph now 6391 nodes, 11513 edges, 499 communities; `graph.json`/`graph.html`/`GRAPH_REPORT.md` refreshed).

**Git operations:** none.

**Deferred work:** T07–T08.

**Newly discovered issues:** none in production behavior. The repository matches the accepted contract exactly (Includes, search semantics, date/status filters, all five sort fields plus the default fallback, paging math, metadata echo, AsNoTracking on GetPagedAsync only). Two T06-internal test-authoring corrections recorded honestly, both fixed before the green gate: (1) the initial draft constructed `Product` with unsaved Category/Unit ids (still 0), tripping the Domain guard — fixed by saving Category/Unit before constructing the Product; (2) an earlier draft used placeholder-item add/remove gymnastics and wrote `SupplierId` (private setter) — replaced by the create-after-dependency-save pattern. No production change was made or needed.

### T06 External Review — Persistence Round-Trip Correction (COMPLETE)

**Date:** September 8, 2026

**Review finding:** the original T06 round-trip test (`AddAndSave_ThenFreshContextReload_ReturnsPersistedAggregate`) persisted an aggregate that was **already Approved before insertion** (`Submit()` + `Approve()` called in the arrange phase) and then only re-read it. That proves creation/reload and aggregate-shape persistence, but not the accepted planning-baseline requirement: a domain mutation performed on an aggregate **loaded by the real repository** must survive a subsequent save and a further fresh-context reload. Case B applied — the test was corrected.

**Correction (test-only):** the round-trip test was replaced with `AddAndSave_ThenFreshReload_SubmitAndSave_ThenFreshReload_PersistsSubmittedState`, which executes the exact accepted sequence: `PurchaseOrder.Create` (Draft at insertion) + `AddItem` → real `PurchaseOrderRepository.AddAsync` → first `SaveChangesAsync` → **first context disposal** → fresh context + real repository `GetByIdAsync` (aggregate-shape assertions preserved: Supplier navigation, single item, Product navigation, Quantity 7, UnitCost 12.50, TotalAmount 87.50, status **Draft**) → real `Submit()` executed on the freshly loaded aggregate (in-memory status asserted `Submitted`) → real `Update` + second `SaveChangesAsync` → **second context disposal** → another fresh context + `GetByIdAsync` → asserts persisted `PurchaseOrderStatus.Submitted` (plus Items/Supplier/Product navigations intact). The forbidden substitutes are all absent: the aggregate is not already-Submitted/Approved at insertion, no reflection state write, no same-context tracked-entity check, no `ChangeTracker.Clear()`, no production modification. Test count unchanged: still 24 methods = 24 discovered cases (one method renamed/replaced, none added or removed).

**Re-verification after the correction:** incremental build succeeded (initially 1 error CS4008 `await` on void `Update` + 1 warning CS8600, then 1 warning CS1717 from an intermediate null-forgiving assignment — all T06-internal, each fixed immediately; final state 0 errors, 0 warnings from T06 files). Targeted round-trip test: 1 passed. IntegrationTests project: **85 passed**, 0 failed, 0 skipped. Full solution suite: UnitTests **314** / IntegrationTests **85** / Web.Tests **33** = **432 passed, 0 failed, 0 skipped** — totals unchanged from the original T06 gate (baseline 408 + 24 preserved). Full rebuild (`--no-incremental`): **28 warnings, 0 errors** — baseline unchanged; grep confirmed no warning originates in the T06 test file.

**Graphify update status (post-correction):** `graphify update .` re-run after the test-source change — **success** (`graph.json`/`graph.html`/`GRAPH_REPORT.md` refreshed).

**Git operations:** none. **Production changes:** none. **T07–T08:** remain NOT STARTED.

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

**OPEN - SPRINT IN PROGRESS (T01–T06 COMPLETE; T07–T08 NOT STARTED)**

*(To be finalized during T08 after Sprint 13 execution and verification are actually complete.)*
