# Sprint 16 Retrospective - Purchase Order POST Round-Trip State and Create Failure Presentation Corrections

> **STATUS: FINAL — SPRINT COMPLETE / CLOSED.**
>
> Finalized at T05 from the accepted T01–T04 execution record. Sprint 16 is complete and closed.

## Sprint Identity

- **Sprint:** Sprint 16
- **Title:** Purchase Order POST Round-Trip State and Create Failure Presentation Corrections
- **Planning authority:** `plan/SPRINT_16_PLANNING_REPORT.md` — Revision 1, **ACCEPTED / FINALIZED**
- **Task breakdown:** `plan/SPRINT_16_TASK_BREAKDOWN.md`
- **Retrospective state:** FINAL
- **Sprint status:** COMPLETE / CLOSED
- **Release classification:** Non-release technical-hardening sprint — no version bump, tag, or GitHub release

---

## 1. Sprint Objective

Preserve Purchase Order descending-sort state through POST round-trips and render expected Create workflow `DomainException` failures as inline validation feedback — eliminating the last known silent navigation-state loss and the last known HTTP-500 presentation path in the Purchasing workflow.

Accepted scope is limited to:

- **A — Descending round-trip correction:** exactly six hidden `Descending` inputs (`Details.cshtml` Submit/Approve/Cancel/Receive; `Edit.cshtml` UpdateItem/RemoveItem) using the accepted explicit-string design `value="@(Model.Descending ? "true" : "false")"`.
- **B — Create DomainException presentation:** narrow `catch (DomainException)` in `CreateModel.OnPostAsync` rendering the canonical Domain message through model-level `ModelState` and reusing the existing Create failure restoration (`EnsureAtLeastOneItem()`, `PopulateDropdownListsAsync()`, `Page()`).

The sprint must preserve Domain rules, Application contracts, authorization behavior, NotFound behavior, persistence semantics, unexpected-exception propagation, and successful workflow behavior.

---

## 2. Why This Sprint Exists

- **A:** Sprint 15 T04 manual verification surfaced a pre-existing `Descending=True` hidden-input POST round-trip loss (Razor boolean-attribute rendering on `value="@Model.Descending"`; `true` renders `value="value"`, `false` omits the attribute; the posted `"value"` string fails bool binding). Revision 1 confirmed all six markup sites in source and the mechanism in generated Razor code. Recorded as deferred in Sprint 15 and every current-state doc; now accepted for correction.
- **B:** Sprint 14/15 planning deferred Create-page duplicate-product `DomainException` presentation. Revision 1 confirmed `PurchaseOrder.AddItem` throws the duplicate-product rule, `CreatePurchaseOrderHandler` propagates it uncaught, and `CreateModel` has no Domain-failure handling — Create is the only Purchase Order POST surface without the established inline pattern (Details: 4 catches since Sprint 15; Edit: 2 catches since Sprint 14; Create: 0).

---

## 3. Accepted Architecture Baseline

- **A (locked):** explicit string rendering — `value="@(Model.Descending ? "true" : "false")"` — on exactly the six affected inputs. No `asp-for` substitution, no query-state redesign, no PageModel binding change.
- **B (locked):** narrow `catch (DomainException)` around the expected handler call in `OnPostAsync` only; `ModelState.AddModelError(string.Empty, exception.Message)`; reuse `EnsureAtLeastOneItem()` + `PopulateDropdownListsAsync()` + `Page()`; no aggregate reload helper, no shared/helper extraction, no validator wiring, no Application/Domain change; success path and class-level `CreatePolicy` authorization unchanged; unexpected exceptions propagate.

Behavior that must remain separate:

- authorization failure → existing AccessDenied/`Forbid()` behavior (Create uses the class-level policy gate)
- lookup failure → existing Result handling (`result.IsFailure` branch unchanged)
- expected workflow `DomainException` → ModelState + existing failure restoration + `Page()`
- unexpected exception → propagate normally

No global exception middleware, no Application contract redesign, no new abstraction (Candidate E excluded).

---

## 4. Sprint Scope

### In Scope

- Candidate A markup correction (six inputs, two Razor files)
- Candidate B narrow DomainException handling (one PageModel method)
- regression verification embedded in T02/T03
- T04 integrated and manual verification (including SQL before-vs-after evidence)
- T05 documentation synchronization and closure

### Out of Scope (accepted non-goals)

- Candidate C — Create FluentValidation production invocation (DEFERRED)
- Candidate D1 — EditStatus `IsInRole(InventoryManager)` decision (DEFERRED)
- Candidate D4 — WebApplicationFactory / CI / automated SQL Server infrastructure (DEFERRED)
- Candidate E — shared POST failure-render helper extraction (DEFERRED — mandatory retrospective re-evaluation checkpoint, Section 12; not implemented in Sprint 16)
- `PagedRequest.Status` Purchase Order finding (REJECTED)
- antiforgery SecurePolicy/plain-HTTP observation (REJECTED — record-only)
- resolved Sprint 10 seed findings (STALE/RESOLVED)
- unrelated roadmap features; shared paging redesign; global DomainException middleware; broad tag-helper normalization
- lifecycle rule changes; new Purchase Order states; authorization capability changes; schema/migration changes; package changes; version/tag/release work

---

## 5. Completed Tasks

| Task | Title | Status |
|---|---|---|
| T01 | Contract Verification and Design Lock | COMPLETE |
| T02 | Descending Round-Trip Correction | COMPLETE / ACCEPTED |
| T03 | Create Domain-Failure Inline Presentation | COMPLETE / ACCEPTED |
| T04 | Integrated and Manual Verification | COMPLETE / ACCEPTED |
| T05 | Documentation Synchronization and Sprint Closure | COMPLETE |

Details, boundaries, acceptance criteria, and stop conditions: `plan/SPRINT_16_TASK_BREAKDOWN.md`.

---

## 6. Starting Quality Baseline

Accepted Revision 1 planning baseline (reproduced during Revision 1 discovery; these were planning values and were subsequently matched by T04's fresh execution evidence):

- **UnitTests:** 346 passed
- **IntegrationTests:** 92 passed
- **Web.Tests:** 39 passed
- **Total:** 477 passed / 0 failed / 0 skipped
- **Normal build:** 0 warnings, 0 errors
- **Full non-incremental build:** 28 pre-existing warnings, 0 errors
- Warning families (pre-existing): CS0108, CS0114, CS8601, CS8602, CS8604, CS8618 — none in Purchasing Purchase Order files

Sprint 16 execution must record fresh test/build results rather than assuming these counts remain unchanged.

---

## 7. Verification Strategy and Result

### Automated

- full UnitTests / IntegrationTests / Web.Tests (fresh runs in T02/T03 regression and T04)
- normal solution build and full non-incremental build (T04)
- no new automated tests — no legitimate PageModel/Razor seam exists (Sprint 14 Option B holds); no artificial seams permitted

### Runtime Matrix

- all six Descending forms × true/false states: rendered HTML check, failure re-render preservation, success PRG/query-state preservation
- Index sort-header/pagination regression
- duplicate-product Create inline message; crafted quantity/unit-cost Domain failures where practical
- successful Create, Details, and Edit regression
- authorization spot check (denied persona → AccessDenied, not validation feedback)

### Persistence

Primary evidence (both required):

1. SQL/database before-vs-after manual evidence for the Create failure path (read-only queries)
2. existing Application no-save test evidence (`CreatePurchaseOrderHandlerTests.HandleAsync_DuplicateProductInRequest_ThrowsDomainException`)

Application restart check is supplementary only.

**Status: COMPLETE.** All 12 boolean-state cases passed. The verification used the running HTTPS application through same-origin requests with antiforgery tokens because an interactive browser surface was unavailable; this limitation changed the interaction mechanism, not the exercised HTTP/Razor/handler path. Read-only SQL before/after evidence confirmed invalid Create attempts produced no new Purchase Order, item, or rejected remark rows. The existing handler test independently retained its zero-save assertion.

---

## 8. Deferred Findings

- **Create FluentValidation production invocation (Candidate C):** DEFERRED — T03 must not wire validators.
- **EditStatus `IsInRole(InventoryManager)` decision (Candidate D1):** DEFERRED — blocked since Sprint 12 pending an explicit behavioral decision.
- **WebApplicationFactory / CI / automated SQL Server infrastructure (Candidate D4):** DEFERRED.
- **Shared POST failure-render helper extraction (Candidate E):** DEFERRED — re-evaluate via the Rule-of-Three checkpoint in Section 12 during T05; do not implement in Sprint 16.

These findings must not be silently absorbed into Sprint 16 implementation.

---

## 9. Task Execution Log

### T01 - Contract Verification and Design Lock

- **Status:** COMPLETE (2026-09-16)
- **Result:** `DESIGN LOCKED — T02 READY`
- **Verification performed (source-level, evidence below):**
  - **Contract A:** all six accepted hidden `Descending` input sites reconfirmed at exact lines — `Details.cshtml` 67 (Submit), 86 (Approve), 107 (Cancel), 288 (Receive); `Edit.cshtml` 165–166 (UpdateItem), 242–243 (RemoveItem); all `name="Descending" value="@Model.Descending"` on the `[BindProperty(SupportsGet = true)] public bool Descending` properties (`Details.cshtml.cs:58–59`, `Edit.cshtml.cs:50–51`). Repo-wide sweep: no additional Purchase Order POST `name="Descending"` occurrence exists (only these two files); all other `Descending` navigation uses `asp-route-` tag helpers (unaffected). Hidden-`value="@Model.<prop>"` sweep: the six `Descending` inputs are the only CLR-bool ones (all other hidden values bind string/number/enum types via `ToString`/direct rendering).
  - **Binding contract:** accepted explicit-string rendering `value="@(Model.Descending ? "true" : "false")"` remains compatible with the existing bool property. Framework evidence class: GET-side `Descending=True`/`Descending=False` binding is proven working in production today (Sprint 14 T09 PRG evidence; Sprint 15 T04 navigation evidence). POST-side format acceptance is not separately provable without a production/test change (forbidden in T01) — T02's rendered-HTML acceptance criterion is the designated proof point. **Note:** the T01 execution prompt suggested a bool-parse framework diagnostic; binary string inspection of the installed shared-framework assemblies was inconclusive and no production/test change was made to force the proof.
  - **Contract B:** duplicate-product rule still in `PurchaseOrder.AddItem` (`PurchaseOrder.cs:54–57`, canonical message `"The product already exists in this purchase order."`); `CreatePurchaseOrderHandler` contains zero try/catch and calls `AddItem` (line 70) in its item loop; `CreateModel.OnPostAsync` still lacks `DomainException` handling (0 occurrences) and retains the exact restoration T03 reuses: `result.IsFailure` branch (lines 107–115: `ModelState.AddModelError(string.Empty, …)` → `EnsureAtLeastOneItem()` → `PopulateDropdownListsAsync()` → `Page()`), `ModelState.IsValid` gate (line 84), success PRG to Index (line 120); `Create.cshtml:25` has the `ModelOnly` validation summary; Create authorization remains the class-level `[Authorize(Policy = PurchaseOrder.CreatePolicy)]` (line 12).
  - **No-persistence evidence reconfirmed:** `CreatePurchaseOrderHandlerTests.HandleAsync_DuplicateProductInRequest_ThrowsDomainException` (two identical product ids 10; asserts `DomainException`, message contains "already exists", `AddAsyncCallCount == 0`, `SaveChangesAsyncCallCount == 0`); Domain rule test `PurchaseOrderTests.AddItem_DuplicateProduct_ThrowsDomainException`.
  - **Boundaries/non-goals reconfirmed:** no DB/migration/auth/package change required; no global `DomainException` middleware in the pipeline (`Program.cs` → `UseWeb()`: only `/Error` exception handler in non-Development, no DomainException middleware); no shared POST-failure helper exists outside the Purchase Order pages (Candidate E remains deferred/unimplemented); no FluentValidation invocation in the Purchasing Web path (Candidate C excluded); EditStatus `IsInRole` guard untouched at line 61 (Candidate D1 deferred).
- **Tests executed (fresh, this task):** full solution suite — UnitTests 346 passed, IntegrationTests 92 passed, Web.Tests 39 passed, total 477 / 0 failed / 0 skipped; targeted Purchasing filter (`FullyQualifiedName~CreatePurchaseOrder|FullyQualifiedName~PurchaseOrderTests`) — 125 passed / 0 failed. Normal build: 0 warnings / 0 errors.
- **Build result:** normal solution build succeeded (0 warnings, 0 errors). Full non-incremental build not executed in T01 (reserved for T04).
- **Graphify:** not run — verification-only task; no source/test-source changes.
- **Database/migration impact:** none. **Authorization impact:** none.
- **Deviations:** none. No source drift found; all fourteen T01 acceptance criteria satisfied.
- **Files changed:** this retrospective T01 task-log section only.

### T02 - Descending Round-Trip Correction

- **Status:** ACCEPTED — EDIT RUNTIME VERIFICATION DEFERRED TO T04 (externally accepted; synchronized 2026-09-18)
- **Implementation:** all six accepted inputs were changed to `value="@(Model.Descending ? "true" : "false")"`: Details Submit/Approve/Cancel/Receive and Edit UpdateItem/RemoveItem. Source verification found zero remaining `value="@Model.Descending"` occurrences. No PageModel, Create, test-source, database, migration, authorization, package, Domain, or Application change was made.
- **Rendered/runtime evidence obtained:** the HTTPS application started against the configured local SQL Server. The currently renderable Details Receive forms (Purchase Orders 3, 6, 8, and 9) emitted literal lowercase `value="true"` for `Descending=true` and `value="false"` for `Descending=false`. Safe Receive POSTs against Purchase Order 3 / Product 2 with `quantity=0` returned HTTP 200 failure re-renders and preserved the posted hidden value as `true` and `false`, respectively. A read-only SQL check afterward confirmed Purchase Order 3 remained status 3 and Product 2 remained quantity 8.00 / received quantity 0.00.
- **Environmental verification limitation:** the current database contains no Draft purchase order (statuses present: 3, 4, 5, and 6). Consequently `Edit.cshtml` cannot render its UpdateItem/RemoveItem forms, and representative Edit true/false POST checks cannot be executed without creating or changing fixture/business data. No such out-of-scope mutation was performed. Details Submit/Approve/Cancel forms were likewise not all present in the current state-specific fixture set. The complete rendered/form matrix remains unclaimed.
- **Automated tests:** `dotnet test InventoryPlatform.slnx --no-build` — UnitTests 346 passed, IntegrationTests 92 passed, Web.Tests 39 passed; total 477 passed / 0 failed / 0 skipped.
- **Build:** `dotnet build InventoryPlatform.slnx` succeeded with 20 warnings / 0 errors; warnings were in pre-existing Administrator/Account/Reports/Purchase Order Index files, not the two changed Razor files.
- **Graphify:** `graphify update .` completed; `graphify-out/graph.json` refreshed at 2026-09-17 19:18:22, followed by a successful scoped query resolving the Purchase Order Details/Edit models and POST handlers.
- **Result:** externally accepted with the existing source correction, representative Details proof, automated-test evidence, build result, and Graphify evidence preserved. Edit `UpdateItem` and `RemoveItem` runtime verification for both `Descending=true` and `Descending=false` remains explicitly pending for T04 using an authorized Draft Purchase Order workflow/fixture. No Edit runtime result is claimed here.

### T03 - Create Domain-Failure Inline Presentation

- **Status:** COMPLETE / ACCEPTED (2026-09-18)
- **Production correction:** `CreateModel.OnPostAsync` now imports `InventoryPlatform.Domain.Exceptions` and places a narrow `catch (DomainException exception)` around only `_handler.HandleAsync(request, cancellationToken)`. The catch adds `exception.Message` as a model-level error via `ModelState.AddModelError(string.Empty, exception.Message)`, reuses `EnsureAtLeastOneItem()` and `PopulateDropdownListsAsync(cancellationToken)`, and returns `Page()`. The existing `ModelState.IsValid` gate, ordinary Result-failure restoration, class-level `PurchaseOrder.CreatePolicy`, success `TempData` message and PRG redirect remain unchanged. No `catch (Exception)` was introduced, so unexpected exceptions remain uncaught.
- **Locked-contract reconfirmation:** before editing, current source confirmed that Create had no `DomainException` catch; `CreatePurchaseOrderHandler` allowed `PurchaseOrder.AddItem` exceptions to propagate; `PurchaseOrder.AddItem` owned the duplicate-product invariant and canonical message; the ordinary Result-failure path used model-level ModelState, `EnsureAtLeastOneItem()`, `PopulateDropdownListsAsync(cancellationToken)`, and `Page()`; and `Create.cshtml` rendered `asp-validation-summary="ModelOnly"`. No material source drift was found.
- **Live HTTP/page evidence:** the HTTPS application started against the configured local SQL Server. An authenticated same-origin Create POST containing Product 3 twice returned HTTP 200 at `/Purchasing/PurchaseOrders/Create`, not an unhandled failure. The `ModelOnly` validation summary rendered the canonical message `The product already exists in this purchase order.` Both item rows were restored with Product 3 selected and quantities 2 and 4; Supplier 1 remained selected; supplier and product dropdown options were populated; and two item rows remained available.
- **Crafted numeric evidence:** a Create POST with quantity `0` returned HTTP 200 and rendered `Quantity must be greater than zero.` with quantity `0` restored. A Create POST with unit cost `-1` returned HTTP 200 and rendered `Unit cost cannot be negative.` with unit cost `-1` restored. The page's HTML `min` attributes are client-side constraints only; these crafted requests passed model binding, reached the Domain guards, and were handled by the new catch. Candidate C validation wiring remains deferred and unchanged.
- **No-persistence evidence:** read-only SQL counts before the duplicate-product request were `PurchaseOrders=12` and `PurchaseOrderItems=12`; counts after the duplicate-product request and again after all three rejected requests were unchanged at 12 and 12. The existing Application test `HandleAsync_DuplicateProductInRequest_ThrowsDomainException` also passed unchanged and asserted `AddAsyncCallCount == 0` and `SaveChangesAsyncCallCount == 0`.
- **Automated verification:** focused duplicate-product Domain/Application tests: 2 passed / 0 failed / 0 skipped; Web.Tests: 39 passed / 0 failed / 0 skipped; full solution: UnitTests 346, IntegrationTests 92, Web.Tests 39 — total 477 passed / 0 failed / 0 skipped.
- **Build:** normal `dotnet build InventoryPlatform.slnx` succeeded with 0 warnings / 0 errors. One preceding invocation used an unsupported build `--logger` switch and failed before compilation; the required normal command was rerun successfully. A focused test/build invocation emitted 20 pre-existing warnings outside the changed file; no warning was reported in `Create.cshtml.cs`, and warning-baseline reconciliation remains T04 work.
- **Graphify:** `graphify update .` completed successfully; the rebuilt graph contains 8,078 nodes, 13,562 edges, and 623 communities. The optional community-label refresh note did not prevent the update. The pre-edit scoped query resolved the Web `CreateModel` → Application `CreatePurchaseOrderHandler` → Domain `PurchaseOrder.AddItem` boundary.
- **Files changed:** production — `src/InventoryPlatform/InventoryPlatform.Web/Pages/Purchasing/PurchaseOrders/Create.cshtml.cs` (narrow presentation-boundary catch only); documentation — this retrospective (known T02 disposition synchronization plus actual T03 evidence); generated Graphify artifacts refreshed by the required update. Temporary HTTP/cookie/header evidence files were removed after verification.
- **Scope audit:** no Domain, Application handler, repository, persistence, migration/schema, authorization/capability/policy/seed, package, configuration, test-source, Create markup, Details/Edit, Candidate C, or Candidate E change was made. T04 and T05 remain unimplemented. Edit `UpdateItem`/`RemoveItem` runtime verification for both `Descending=true` and `Descending=false` remains pending for T04 using an authorized Draft Purchase Order workflow/fixture.
- **Result:** Accepted; proceeded to T04.

### T04 - Integrated and Manual Verification

- **Status:** COMPLETE / ACCEPTED (2026-09-18)
- **Authorities/preflight:** `knowledge.md`, `plan/SPRINT_16_PLANNING_REPORT.md`, `plan/SPRINT_16_TASK_BREAKDOWN.md`, and this retrospective were present and read. The actual five-layer InventoryPlatform source tree was present. Source audit found exactly six accepted explicit-string `Descending` sites (Details 67/86/107/288; Edit 165/242), zero legacy `value="@Model.Descending"` sites, unchanged `[BindProperty(SupportsGet = true)] bool Descending` properties, the accepted narrow Create `catch (DomainException)`, zero broad Create `catch (Exception)`, canonical `exception.Message`, existing restoration calls, and unchanged Domain/Application contracts. No source drift was found.
- **Authorized fixtures and successful Create regression:** six disposable Draft Purchase Orders were created through the running application's normal authenticated Create workflow, each returning HTTP 302 to the Purchase Order Index: 22 `T04-FLOW-TRUE`, 23 `T04-FLOW-FALSE`, 24 `T04-CANCEL-TRUE`, 25 `T04-CANCEL-FALSE`, 26 `T04-EDIT-TRUE`, and 27 `T04-EDIT-FALSE`. Orders 22–25 were created with Product 1, quantity 5, unit cost 10. Orders 26–27 were created with Product 1 (5 × 10) and Product 2 (3 × 20). SQL changed exactly from 12 Purchase Orders / 12 items to 18 / 20 after these six successful Creates.
- **Descending six-form × two-state runtime matrix:** all requests used `Search=T04`, `SortBy=OrderDate`, `PageNum=2`, and `PageSize=5`; rendered values were literal lowercase strings, submitted values were lowercase `true`/`false`, and successful redirects contained the correctly bound `Descending=True`/`Descending=False` query value.

| Page/action | Starting PO/state | Rendered | Posted | HTTP/result and preserved query state | Expected business mutation |
|---|---|---:|---:|---|---|
| Details Submit | 22 / Draft | `true` | `true` | 302 → Details/22 with `Descending=True` | Draft → Submitted |
| Details Submit | 23 / Draft | `false` | `false` | 302 → Details/23 with `Descending=False` | Draft → Submitted |
| Details Approve | 22 / Submitted | `true` | `true` | 302 → Details/22 with `Descending=True` | Submitted → Approved |
| Details Approve | 23 / Submitted | `false` | `false` | 302 → Details/23 with `Descending=False` | Submitted → Approved |
| Details Cancel | 24 / Draft | `true` | `true` | 302 → Details/24 with `Descending=True` | Draft → Cancelled |
| Details Cancel | 25 / Draft | `false` | `false` | 302 → Details/25 with `Descending=False` | Draft → Cancelled |
| Details Receive | 22 / Approved | `true` | `true` | 302 → Details/22 with `Descending=True` | Product 1 received 0 → 1; PO → Receiving |
| Details Receive | 23 / Approved | `false` | `false` | 302 → Details/23 with `Descending=False` | Product 1 received 0 → 1; PO → Receiving |
| Edit UpdateItem | 26 / Draft | `true` | `true` | 302 → Edit/26 with `Descending=True` | Product 1 quantity/cost 5/10 → 6/11 |
| Edit UpdateItem | 27 / Draft | `false` | `false` | 302 → Edit/27 with `Descending=False` | Product 1 quantity/cost 5/10 → 7/12 |
| Edit RemoveItem | 26 / Draft | `true` | `true` | 302 → Edit/26 with `Descending=True` | Product 2 removed; Product 1 retained |
| Edit RemoveItem | 27 / Draft | `false` | `false` | 302 → Edit/27 with `Descending=False` | Product 2 removed; Product 1 retained |

- **Deferred Edit closure:** Edit GETs for Draft orders 26/27 rendered both UpdateItem and RemoveItem forms. Order 26 rendered four literal `value="true"` inputs (two items × two forms); order 27 rendered four literal `value="false"` inputs. UpdateItem and RemoveItem both redirected with their posted boolean preserved. SQL confirmed orders 26/27 remained Draft, Product 1 became 6 × 11 and 7 × 12 respectively, and Product 2 was removed from each. The four T02-deferred Edit cells are closed with actual evidence.
- **Details rendered-HTML evidence:** Draft Details pages rendered two matching literal inputs (Submit and Cancel) in each boolean state; Submitted pages rendered matching Approve/Cancel inputs; Approved pages rendered a matching Receive input. No `value="value"` or omitted accepted-site value was observed. The successful PRG locations above prove POST model binding and query-state preservation.
- **Create Domain-failure verification:** with SQL at 18 Purchase Orders / 18 items, a duplicate Product 3 Create request returned HTTP 200 at Create and rendered `The product already exists in this purchase order.` in the model-level summary. Supplier 1, two Product 3 selections, quantities 2 and 4, populated dropdowns, and two item rows were restored. Crafted quantity `0` and unit cost `-1` requests each returned HTTP 200 and rendered `Quantity must be greater than zero.` / `Unit cost cannot be negative.` with the submitted invalid value restored. These crafted values passed model binding and reached the Domain; Candidate C remained untouched.
- **Persistence evidence:** before T04, SQL contained 12 Purchase Orders, 12 PurchaseOrderItems, 11 InventoryTransactions, and Product 1 quantity 16. After the six authorized Creates and matrix actions, SQL contained 18 Purchase Orders, 18 items, and 13 transactions; Product 1 quantity was 18, exactly reflecting the two successful receives. Orders 22/23 were Receiving with ReceivedQuantity 1; 24/25 were Cancelled; 26/27 were Draft with the expected updates/removals. After all three rejected Create requests, counts remained 18/18 and zero Purchase Orders had the rejected remarks. The focused existing duplicate-product Domain/Application tests passed 2/2, including unchanged zero-AddAsync/zero-SaveChangesAsync assertions.
- **Authorization:** source retains `PurchaseOrder.ViewPolicy` on Details, `PurchaseOrder.EditPolicy` on Edit, `PurchaseOrder.CreatePolicy` on Create, and per-action capability checks before Details/Edit business handlers. Authenticated Administrator requests completed the authorized workflow. Fresh unauthenticated GETs to Create, Details/22, and Edit/26 each returned HTTP 302 to `/Identity/Account/Login` with the appropriate ReturnUrl. Existing no-group accounts remain present in SQL; their credentials were not available and were not reset or directly mutated for T04. No authorization weakening was found.
- **Automated tests:** focused duplicate-product rule/propagation tests: 2 passed / 0 failed / 0 skipped. Fresh full solution run (`dotnet test InventoryPlatform.slnx --no-build`): UnitTests 346, IntegrationTests 92, Web.Tests 39 — total 477 passed / 0 failed / 0 skipped.
- **Build/warning reconciliation:** normal `dotnet build InventoryPlatform.slnx` succeeded with 0 warnings / 0 errors. `dotnet build InventoryPlatform.slnx --no-incremental` succeeded with 28 warnings / 0 errors, exactly matching the planning baseline. Warning families were CS0108, CS0114, CS8601, CS8602, CS8604, and CS8618 in pre-existing Application InventoryTransactions, Infrastructure, Administrator/Account, Purchase Order Index, and Reports files. None originated in the six corrected Razor sites or `Create.cshtml.cs`; Sprint 16 introduced no warning.
- **Graphify:** no update was run because T04 changed no source/test-source. `graphify explain CreatePurchaseOrderHandler` successfully resolved the current Application node, its `CreateModel` Web caller, DI registration, persistence interfaces, and duplicate-product test connection. An initial ambiguous `graphify path CreateModel PurchaseOrder` found no directed path; this was a query-selection limitation, not a stale-graph or product defect.
- **Files/data changed:** repository file change is this T04 retrospective section only. Runtime business data consists of disposable Purchase Orders 22–27 and their expected workflow/item/inventory mutations, all created through authorized normal application flows and retained as documented verification fixtures. Sixty-six temporary local HTTP cookie/body/header files were removed after evidence capture.
- **Scope audit:** no production source, test source, Razor markup, Domain/Application/repository behavior, schema/migration, authorization capability/policy/seed, package, configuration, Candidate C, or Candidate E change was made during T04. No Git command, release, version, or tag operation was performed.
- **Defects/new findings:** none. Tooling limitation: no browser UI surface was available, so verification used authenticated same-origin HTTPS requests with genuine antiforgery tokens against the running application, rendered HTML inspection, redirect headers, and read-only SQL. This exercised the real Razor/PageModel/Application/Domain/persistence boundary and established every mandatory matrix cell without creating permanent test infrastructure.
- **Result:** Accepted; T05 closure authorized.

### T05 - Documentation Synchronization and Sprint Closure

- **Status:** COMPLETE (2026-09-18)
- Synchronized README, project status, roadmap, changelog, feature catalog, engineering journal, design decisions, task tracker, and this retrospective.
- Converted the hidden-bool observation into an explicit rendering convention; completed the Candidate E checkpoint; marked A/B resolved and kept C/D1/D4/E deferred.
- Documentation-only: no tests rerun, Graphify update, release, version, tag, production/test source, schema, authorization, package, or configuration change.

---

## 10. Sprint Metrics

### Planning Baseline

| Metric | Value |
|---|---:|
| UnitTests | 346 |
| IntegrationTests | 92 |
| Web.Tests | 39 |
| Total | 477 |
| Failed / Skipped | 0 / 0 |
| Normal build warnings / errors | 0 / 0 |
| Full rebuild warnings / errors | 28 / 0 |

### Final Sprint 16 Results

| Gate | Final result |
|---|---|
| UnitTests | 346 passed, 0 failed, 0 skipped |
| IntegrationTests | 92 passed, 0 failed, 0 skipped |
| Web.Tests | 39 passed, 0 failed, 0 skipped |
| Total | 477 passed, 0 failed, 0 skipped |
| Normal build | 0 warnings, 0 errors |
| Non-incremental build | 28 pre-existing warnings, 0 errors; no new warning |
| Descending matrix | 12/12 passed |
| Invalid Create matrix | duplicate product, quantity 0, and cost -1 all inline; no persistence |
| Success/auth regression | passed |

---

## 11. Observations Confirmed During Execution

- The explicit-string rendering fixes the `Descending` round-trip in all six forms with no binding-side effects.
- The narrow Create catch is sufficient because the duplicate-product `DomainException` is thrown inside `PurchaseOrder.AddItem` before `AddAsync`, so the failure path persists nothing.
- The existing `ModelOnly` validation summary on `Create.cshtml` renders the new model-level error without markup changes.
- T04 confirmed each observation through runtime and persistence evidence.

---

## 12. Candidate E — Rule-of-Three Re-Evaluation

After T03, three PageModels will share the same shape by copy:

1. `DetailsModel.RenderDomainFailureAsync` (Sprint 15 — ModelState + aggregate reload + `Page()`)
2. `EditModel.RenderDomainFailureAsync` (Sprint 14 — ModelState + aggregate reload + Draft-check redirect + `Page()`)
3. `CreateModel` failure restoration (Sprint 16 — ModelState + item restore + dropdown reload + `Page()`)

**Decision: DEFER.** The Rule-of-Three checkpoint was satisfied, but the repeated shape is not yet one stable responsibility. Details reloads the aggregate and can return NotFound; Edit reloads, enforces Draft state, and can redirect; Create restores posted item rows and dropdown data without an aggregate reload. Extracting only the catch/ModelState prologue would save little, while abstracting restoration would introduce delegates or PageModel coupling that obscures page-specific behavior. Candidate E remains reserved for a dedicated future design/refactoring task; Sprint 16 implemented no extraction.

---

## 13. Decisions Made During Sprint

**Accepted decisions:**

1. Sprint 16 limited to Candidates A + B.
2. Candidate A uses the explicit-string design; `asp-for` and query-state redesign rejected.
3. Candidate B uses a narrow `catch (DomainException)` reusing the existing restoration; no reload helper, no validator wiring, no shared extraction.
4. Non-release technical-hardening classification.
5. Candidates C, D1, D4, E deferred; `PagedRequest.Status` finding, antiforgery observation, and Sprint 10 seed findings rejected/stale.

**Decisions recorded during execution:**

Execution confirmed the accepted decisions. T03's first attempt stopped without implementation because the required authority set was unavailable; the retry began only after the authorities were supplied and then completed normally. T04's interactive browser surface was unavailable, so equivalent same-origin HTTPS requests with antiforgery tokens and read-only SQL evidence were used and disclosed. Neither deviation expanded scope or weakened the required behavioral checks.

---

## 14. Known Non-Goals

Sprint 16 does not attempt to: redesign exception handling application-wide; change Purchase Order lifecycle semantics; add purchasing features; change authorization; change the database schema; wire FluentValidation; extract shared helpers; normalize tag helpers; resolve the EditStatus decision; create a semantic release.

---

## 15. Definition of Done

Sprint 16 can be marked COMPLETE only when (per the accepted task breakdown):

- all six `Descending` inputs use the accepted explicit-string design and round-trip correctly in both states (rendered-HTML-verified)
- expected Create `DomainException` failures render inline with no HTTP 500 and no persistence
- successful Create/Details/Edit workflows unchanged
- authorization, NotFound, and unexpected-exception semantics unchanged
- fresh full suite passes with exact numbers recorded; normal build 0/0; full non-incremental build 28-warning/0-error baseline preserved
- SQL before-vs-after + Application no-save evidence recorded
- no schema/migration, authorization, package, or configuration change
- documentation synchronized; retrospective finalized with the Candidate E checkpoint
- no version bump, tag, or release

---

## 16. Final Sprint Outcome

Sprint 16 met its objective. Candidate A is resolved: all six hidden fields use explicit lowercase boolean strings and every true/false round trip passed. Candidate B is resolved: expected Create Domain failures render inline, restore the page, and persist nothing. Successful behavior and authorization remain intact. Candidates C, D1, D4, and E remain deferred; rejected items remain rejected. No new defect or warning was introduced.

## 17. Release Decision

Final classification: **non-release technical-hardening sprint** — no new business capability, lifecycle transition, authorization capability, or schema change. Version bump: none (v1.6.0 remains the release baseline). Tag: none. GitHub release: none.

## 18. Retrospective Closure

- **Sprint status:** COMPLETE / CLOSED
- **All tasks accepted:** Yes — T01 through T05 complete
- **Final test baseline:** 477 passed, 0 failed, 0 skipped (346/92/39)
- **Runtime verification:** 12/12 round-trip matrix plus invalid/successful Create and authorization checks passed
- **Database migration:** None
- **Authorization changes:** None
- **Release/tag:** None — non-release classification; v1.6.0 remains the release baseline
- **Known deferred findings (carried):** Candidate C; Candidate D1; Candidate D4; Candidate E (re-evaluation checkpoint in Section 12)
- **Graphify:** T02/T03 source changes were updated as recorded in their task evidence; no update was required for verification-only T04 or documentation-only T05
- **Git operations:** none — the developer controls Git manually
