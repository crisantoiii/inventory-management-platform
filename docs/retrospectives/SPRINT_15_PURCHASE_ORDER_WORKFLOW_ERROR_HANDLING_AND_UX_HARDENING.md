# Sprint 15 Retrospective - Purchase Order Workflow Error Handling and UX Hardening

## Sprint Identity

- **Sprint:** Sprint 15
- **Title:** Purchase Order Workflow Error Handling and UX Hardening
- **Planning authority:** `plan/SPRINT_15_PLANNING_REPORT.md` - Revision 2
- **Task breakdown:** `SPRINT_15_TASK_BREAKDOWN.md`
- **Retrospective state:** FINALIZED (T05 COMPLETE)
- **Sprint status:** COMPLETE / CLOSED
- **Implementation status:** T01-T05 COMPLETE
- **Release classification:** Non-release technical-hardening sprint

---

## 1. Sprint Objective

Harden Purchase Order Details workflow error handling so expected Domain business-rule failures are presented as user-facing validation feedback rather than propagating to the Development exception page.

The accepted scope is limited to:

- Submit
- Approve
- Receive
- Cancel

The sprint must preserve Domain rules, Application contracts, authorization behavior, NotFound behavior, persistence semantics, and unexpected-exception handling.

---

## 2. Why This Sprint Exists

Sprint 14 T09/T10 identified and intentionally deferred a presentation-boundary issue:

Purchase Order Details POST handlers can invoke Application handlers that propagate expected `DomainException` instances from Domain aggregate operations. The Details Razor Page handles Result/lookup failures but does not currently translate these expected Domain failures into ModelState validation feedback.

The Sprint 14 Draft Edit page established a local catch → ModelState → re-render precedent.

Sprint 15 addresses only the confirmed Details workflow gap.

---

## 3. Accepted Architecture Baseline

The accepted Revision 2 design is:

- local `try/catch (DomainException)` in `DetailsModel`
- one private PageModel-local re-render helper if T01 reconfirms the design
- add canonical Domain message to ModelState
- reload Purchase Order details
- return `Page()`

Behavior that must remain separate:

- authorization failure → `Forbid()`
- lookup failure → existing Result/NotFound path
- expected workflow `DomainException` → ModelState + page re-render
- unexpected exception → propagate normally

No global exception middleware or Application contract redesign is planned.

---

## 4. Sprint Scope

### In Scope

- Submit expected Domain failure rendering
- Approve expected Domain failure rendering
- Receive expected Domain failure rendering
- Cancel expected Domain failure rendering
- automated regression verification
- manual browser verification
- persistence/no-save verification
- documentation closure

### Out of Scope

- Create Purchase Order duplicate-product hardening
- Create FluentValidation remediation
- lifecycle rule changes
- new Purchase Order states
- authorization capability changes
- schema/migration changes
- global exception infrastructure
- Application Result redesign
- new purchasing functionality
- unrelated deferred findings
- version/tag/release work

---

## 5. Planned Tasks

| Task | Title | Status |
|---|---|---|
| T01 | Contract Verification and Design Lock | COMPLETE |
| T02 | Purchase Order Details Workflow Error Handling | COMPLETE |
| T03 | Regression and Coverage Verification | COMPLETE |
| T04 | Integrated and Manual Verification | COMPLETE |
| T05 | Documentation Synchronization and Sprint Closure | COMPLETE |

---

## 6. Starting Quality Baseline

Accepted Sprint 14 final baseline carried into Sprint 15 planning:

- **UnitTests:** 346 passed
- **IntegrationTests:** 92 passed
- **Web.Tests:** 39 passed
- **Total:** 477 passed
- **Failed:** 0
- **Skipped:** 0
- **Normal build:** 0 warnings, 0 errors
- **Full non-incremental build:** 28 warnings, 0 errors

This baseline is historical input only.

Sprint 15 execution must record fresh test/build results rather than assuming these counts remain unchanged.

---

## 7. Planned Verification Strategy

### Automated

Run and record:

- full UnitTests
- full IntegrationTests
- full Web.Tests
- normal solution build
- full non-incremental build

Existing Domain/Application tests are expected to remain the primary automated protection for workflow rule and no-save semantics.

No artificial PageModel seam should be introduced solely to add presentation-layer tests.

### Manual Browser

Verify:

- Submit failure renders inline
- Approve failure renders inline
- Receive failures render inline
- Cancel failure renders inline
- no expected Domain rule reaches Development exception page
- successful paths remain unchanged
- authorization behavior remains unchanged
- NotFound behavior remains unchanged
- navigation/query state remains coherent

### Persistence

Verify rejected operations do not persist state changes.

Use SQL Server verification where appropriate.

---

## 8. Deferred Findings at Sprint Start

### Create Page Duplicate-Product DomainException Presentation

**Status:** DEFERRED - OUT OF SPRINT 15 SCOPE

### Create FluentValidation Production Invocation

**Status:** DEFERRED INVESTIGATION / RECORD-ONLY

These findings must not be silently absorbed into Sprint 15 implementation.

---

## 9. Task Execution Log

### T01 - Contract Verification and Design Lock

- **Status:** COMPLETE
- **Date:** 2026-09-13
- **Result:** DESIGN LOCKED - T02 READY
- **Files changed:** Retrospective only
- **Tests:** None executed (verification-only task)
- **Build:** None executed (verification-only task)
- **Graphify:** Not run - verification-only task; no source/test-source changes
- **Notes:** All accepted Revision 2 assumptions remain supported by current source. No source drift detected.

### T02 - Purchase Order Details Workflow Error Handling

- **Status:** COMPLETE
- **Date:** 2026-09-13
- **Result:** T02 ACCEPTED CANDIDATE - PROCEED TO T03
- **Production file changed:** `src/InventoryPlatform/InventoryPlatform.Web/Pages/Purchasing/PurchaseOrders/Details.cshtml.cs`
- **Behavior implemented:** All four POST handlers (Submit, Approve, Receive, Cancel) now catch `DomainException` and route through `RenderDomainFailureAsync` private helper. Helper adds canonical Domain message to ModelState, reloads Purchase Order via `GetPurchaseOrderHandler`, returns `NotFound()` if reload fails, otherwise sets `PurchaseOrder` property and returns `Page()`.
- **Tests executed:** UnitTests (346 passed), IntegrationTests (92 passed), Web.Tests (39 passed)
- **Build result:** 0 errors, 20 warnings (all pre-existing)
- **Graphify:** Updated successfully (3303 nodes, 6364 edges, 262 communities)
- **Database/migration impact:** None
- **Authorization impact:** None
- **Deviations:** None

### T04 - Integrated and Manual Verification

- **Status:** COMPLETE
- **Date:** 2026-09-15
- **Result:** T04 ACCEPTED CANDIDATE - PROCEED TO T05
- **Files changed:** Retrospective only. No production, test-source, migration, authorization, or package changes.

#### Automated Verification (fresh runs)

| Suite | Passed | Failed | Skipped | Total |
|---|---:|---:|---:|---:|
| UnitTests | 346 | 0 | 0 | 346 |
| IntegrationTests | 92 | 0 | 0 | 92 |
| Web.Tests | 39 | 0 | 0 | 39 |
| **Total** | **477** | **0** | **0** | **477** |

- **Normal build:** 0 warnings, 0 errors.
- **Full non-incremental build:** 28 warnings (all pre-existing: CS0108 x6, CS0114 x6, CS8601 x8, CS8602 x26, CS8604 x6, CS8618 x4), 0 errors. Matches Sprint 14/T03 baseline.
- Re-run again after the manual-verification phase and after a clean rebuild; identical results.

#### Application Startup

- Environment: `ASPNETCORE_ENVIRONMENT=Development`, `https://localhost:7238` (the `https` launch profile URL from `launchSettings.json`).
- Database: local SQL Server 2025 (17.0.1000.7), database `InventoryPlatform`, `Trusted_Connection` per `appsettings.Development.json`. Connection verified before startup.
- Startup succeeded with no exceptions; `IdentitySeeder`/`AuthorizationSeeder` completed (seed users, capabilities, groups present in SQL).
- Login page returned HTTP 200; authorized users reach the Purchase Order Details page.
- Environment observation (record-only): the antiforgery cookie uses `SecurePolicy = Always`, so serving the app over plain HTTP (e.g. forcing the `http` profile URL) makes any antiforgery-dependent request fail with a 500 from `DeveloperExceptionPageMiddleware`. Running the intended `https` profile is unaffected. Pre-existing configuration, not a Sprint 15 change.

#### Manual Browser Verification Method

Real Chrome browser (Selenium) driving the running application over HTTPS with authenticated sessions (the seeded `admin` user and a dedicated capability-less user). Crafted POSTs were executed same-origin from inside the authenticated browser session with a valid antiforgery token harvested from a real rendered page - genuine requests through the running application boundary, not out-of-band HTTP calls. Every scenario recorded SQL before/after state via read-only `sqlcmd` queries. Representative non-default navigation query used for failure scenarios: `Search=Air&FromDate=2026-01-01&ToDate=2026-12-31&Status=1&SortBy=OrderDate&Descending=True&PageNum=2&PageSize=5`.

#### Submit Failure (empty Draft PO 21, real UI button)

- Starting state: Draft (1), zero items (item removed through the real Edit-page RemoveItem UI first).
- Action: click "Submit Purchase Order" on Details.
- Result: Details re-rendered (`Page()`), ModelOnly validation summary showed canonical message `A purchase order must contain at least one item.`, no Development exception page, no HTTP 500.
- SQL before == after: status Draft (1), zero items, ledger count unchanged (10).
- Navigation state: Back link and hidden fields preserved Search/FromDate/ToDate/Status/SortBy/PageNum/PageSize (see navigation finding below for `Descending`).

#### Submit Failure (non-Draft, crafted POST, PO 9 Approved)

- Crafted POST `?handler=Submit` on Approved PO 9 (UI hides Submit for non-Draft).
- Result: Details re-rendered with `Only draft purchase orders can be modified.`, no HTTP 500.
- SQL: status unchanged (Approved, 3); items unchanged.

#### Approve Failure (crafted POST, PO 6 Draft)

- Result: Details re-rendered with `Only submitted purchase orders can be approved.`, no HTTP 500.
- SQL: status unchanged (Draft, 1); no persistence mutation.

#### Cancel Failure (crafted POST, PO 9 Approved)

- Result: Details re-rendered with `Only draft or submitted purchase orders can be cancelled.`, no HTTP 500.
- SQL: status unchanged (Approved, 3); no persistence mutation.

#### Receive Failures (all four, through the running app)

| Scenario | Trigger | Canonical message rendered | SQL before == after |
|---|---|---|---|
| Invalid status | Receive on Draft PO 6 (crafted POST) | `Only approved purchase orders can receive inventory.` | status Draft; item ReceivedQuantity 0.00; product QuantityOnHand 16.00; ledger count 10 - unchanged |
| Item not found | Receive product 1 (exists in catalog, not in PO 7) on Approved PO 7 (crafted POST) | `Purchase order item was not found.` | unchanged (status, items, product stock, ledger) |
| Invalid quantity | Receive quantity 0 on Approved PO 7 (crafted POST) | `Received quantity must be greater than zero.` | unchanged |
| Over-receive | Receive quantity 15 > ordered 10 on Approved PO 7 (crafted POST) | `Received quantity cannot exceed ordered quantity.` | unchanged |

All four: no HTTP 500 / Development exception page; Details re-rendered with unchanged status badge; no PO receipt mutation, no inventory mutation, no stock-transaction/ledger row created by the rejected operations.

Note: the item-not-found scenario intentionally used an existing product absent from the PO (product 1 on PO 7, which contains only product 2) so the Domain item-lookup rule fires rather than the Application product-lookup Result failure. A truly non-existent product id (e.g. 999) yields the Application Result failure `Product with ID '999' was not found.` before the Domain rule - a different (Result, not DomainException) path, verified separately below.

#### Success Path Regression (real UI)

| Workflow | PO | Outcome (visible + SQL) |
|---|---|---|
| Submit | 6 | success alert `Purchase Order '6' was submitted successfully.`; redirect to Details with query state; status Draft → Submitted (2) |
| Approve | 6 | success alert; status Submitted → Approved (3) |
| Receive | 7 (product 2, qty 10) | success alert; status Approved → Completed (5); item ReceivedQuantity 0.00 → 10.00; product QuantityOnHand 123.00 → 133.00; InventoryTransactions 10 → 11 |
| Cancel | 21 (empty Draft) | success alert; status Draft → Cancelled (6) |

No new inline error behavior appeared on any successful action.

#### Authorization Verification

- Subject: dedicated user `t04noperm@inventory.local` created through the running app (no roles, no AuthorizationGroup membership, hence zero capabilities).
- Details GET: redirected to `/Identity/Account/AccessDenied` (existing behavior).
- Crafted POST `?handler=Submit` on Draft PO 6: response was the AccessDenied page with an **empty** ModelState validation summary - authorization denial was NOT converted into validation feedback; handler business code never executed.
- SQL: PO 6 unchanged (Draft; item intact). Capability policy model, `Forbid()` ordering, and seed data unchanged.

#### Result / NotFound Verification

- GET `/Purchasing/PurchaseOrders/Details/999`: HTTP 404 (existing NotFound path).
- Crafted POST `?handler=Submit` with `id=999` (valid antiforgery, authenticated session): HTTP 404 via the Result failure → NotFound() path; response was not a Details re-render and contained no validation summary.
- Distinct from DomainException handling (re-render + validation summary), as required.

#### Navigation / Query State Verification

After the Submit failure re-render, the Back link and post values preserved `Search=Air`, `FromDate=2026-01-01`, `ToDate=2026-12-31`, `Status` (round-trips as `Status=Draft`, equal value), `SortBy=OrderDate`, `PageNum=2`, `PageSize=5`.

**Finding (pre-existing, not a T02 regression):** `Descending=True` did not survive the POST round-trip. Root cause (confirmed by DOM inspection, raw server HTML, clean rebuild, and the emitted Razor source): `value="@Model.Descending"` on a CLR `bool` triggers Razor boolean-attribute rendering - `true` renders the attribute as the literal string `value="value"`, `false` omits the attribute. The posted `"value"` string fails bool model binding, so `Descending` rebinds as `false` on the re-render. The identical markup pattern exists on `Edit.cshtml` (hidden inputs at lines 165-166, 242-243), proving it is a pre-existing page-markup convention, not something introduced by T02 (which changed only `Details.cshtml.cs`). Per T04 scope rules this was NOT corrected in source; recorded as a pre-existing UX limitation / correction requirement for a future approved task (remedy, if approved: render `value="@Model.Descending"` as a non-bool expression, e.g. `value="@(Model.Descending ? "true" : "false")"`, or use `asp-for`/`asp-page-handler` tag helpers).

#### Real SQL Server Persistence Verification

All rejected operations compared before/after via read-only queries: PurchaseOrders.Status, PurchaseOrderItems (Quantity/UnitCost/ReceivedQuantity), Products.QuantityOnHand, InventoryTransactions count. Every rejected Submit/Approve/Receive/Cancel left persisted state byte-identical (evidence tables above). No SQL data modification was performed outside deliberate application actions under test, except removal of intermediate draft Purchase Orders (ids 12-18) created accidentally by the verification harness itself during its debugging, before the recorded verification run (documented cleanup, no application data touched).

#### Restart Persistence Check

1. Stopped the application (all rejected and successful operations already performed).
2. Restarted against the same database; reloaded relevant Purchase Orders through the UI.
3. Results: PO 6 still Approved (successful submit+approve persisted); PO 7 still Completed with ReceivedQuantity 10.00, product QuantityOnHand 133.00, ledger 11 (successful receive persisted); PO 9 still Approved with zero ReceivedQuantity and unchanged items (rejected submit/approve/cancel left no mutation); PO 21 still Cancelled (successful cancel persisted).

#### Database / Migration Verification

- `__EFMigrationsHistory` contains exactly the 10 migration files present in source (latest `20260831141400_CreateAuthorizationSchema`); no Sprint 15 migration exists.
- `dotnet ef migrations has-pending-model-changes` (dotnet-ef 10.0.10): "No changes have been made to the model since the last migration." - no model drift from T02.
- No schema change; no migration created.

#### Graphify

`graphify update .`: Not run - verification-only task; no source/test-source changes.

#### Deviations / Findings

1. `Descending` bool hidden-input round-trip loss - pre-existing markup limitation (Details and Edit pages), recorded above as a correction requirement; not fixed during T04.
2. HTTP-only serving is incompatible with the antiforgery `SecurePolicy = Always` configuration (startup/HTTPS observation above); the `https` launch profile is the intended local configuration and was used.
3. Test tooling: verification used a temporary, untracked local browser-automation harness (Selenium + Chrome) driving the real app; the harness files were removed after evidence capture and are not part of the repository change set.
4. Setup side effects through the application (documented, deliberate test data): user `t04noperm@inventory.local` created and retained (same precedent as Sprint 14 `t09denied`); PO 21 created then cancelled; PO 6 advanced Draft→Submitted→Approved; PO 7 received in full (product 2 stock 123→133, one new InventoryTransaction).

### T03 - Regression and Coverage Verification

- **Status:** COMPLETE
- **Date:** 2026-09-13
- **Result:** T03 ACCEPTED CANDIDATE - PROCEED TO T04

- **T02 implementation recheck:** All 14 locked implementation properties confirmed. Source matches accepted T02 design.
- **Result/NotFound behavior corrected:** Existing Result failures in Submit/Approve/Receive/Cancel use ModelState + reload + Page() (NOT NotFound()). This is a documentation-only correction; T02 did not change Result semantics.
- **Domain coverage:** All four workflows have comprehensive Domain test coverage (83 tests in PurchaseOrderTests).
- **Application coverage:** All four handler test files prove DomainException propagation, no-save behavior, and success paths.
- **Web coverage decision:** No legitimate PageModel test seam exists. No artificial seam introduced.
- **Tests executed:** UnitTests (346 passed), IntegrationTests (92 passed), Web.Tests (39 passed)
- **Build result:** 0 errors, 28 warnings (all pre-existing, matches Sprint 14 baseline)
- **Graphify:** Not run - verification-only task; no source/test-source changes
- **Persistence safety:** All four rejected workflows avoid persistence (confirmed by Application tests).
- **Authorization regression:** All policies unchanged, ordering unchanged, Web authorization tests pass.
- **Deviations:** None

### T05 - Documentation Synchronization and Sprint Closure

- **Status:** COMPLETE
- **Date:** 2026-09-15
- **Result:** T05 ACCEPTED CANDIDATE - SPRINT 15 READY TO CLOSE
- **Files changed:** Retrospective (finalized), README.md, ROADMAP.md, PROJECT_STATUS.md, CHANGELOG.md, docs/ENGINEERING_JOURNAL.md. No production, test-source, migration, authorization, or package changes.
- **Tests executed:** None (documentation-only task; the accepted T04 baseline stands as the sprint's final baseline and no fresh runs were executed or claimed)
- **Build:** None executed (documentation-only task)
- **Graphify:** Not run - documentation-only task; no source/test-source changes
- **Notes:** Documentation synchronized with the accepted T01-T04 evidence. The deferred `Descending=True` finding was preserved with appropriately qualified navigation wording; the antiforgery `SecurePolicy = Always` HTTPS observation was recorded as record-only; no stale `result.IsFailure -> NotFound()` wording remains in current-state docs; Sprint 15 was marked COMPLETE/CLOSED only after synchronization. Exact changed files are listed in the T05 execution report.
- **Deviations:** None

---

## 10. Sprint Metrics

### Initial Baseline

| Metric | Baseline |
|---|---:|
| UnitTests | 346 |
| IntegrationTests | 92 |
| Web.Tests | 39 |
| Total | 477 |
| Failed | 0 |
| Skipped | 0 |
| Normal build warnings | 0 |
| Normal build errors | 0 |
| Full rebuild warnings | 28 |
| Full rebuild errors | 0 |

### Final Sprint 15 Results

Recorded during T04 (fresh runs, re-confirmed after a clean rebuild): UnitTests 346, IntegrationTests 92, Web.Tests 39, total 477 passed, 0 failed, 0 skipped; normal build 0 warnings/0 errors; full non-incremental build 28 warnings (all pre-existing)/0 errors. T05 was documentation-only and executed no fresh test or build runs; the accepted T04 baseline stands as the sprint's final baseline.

| Metric | Final |
|---|---:|
| UnitTests | 346 |
| IntegrationTests | 92 |
| Web.Tests | 39 |
| Total | 477 |
| Failed | 0 |
| Skipped | 0 |
| Normal build warnings | 0 |
| Normal build errors | 0 |
| Full rebuild warnings | 28 |
| Full rebuild errors | 0 |

---

## 11. What Went Well

- **Design lock held end to end.** T01 verified all Revision 2 assumptions against current source before implementation; T02 then landed in a single file (`Details.cshtml.cs`) with zero deviations, and no later task exposed drift.
- **The Edit-page precedent carried.** The local catch → ModelState → re-render helper pattern mirrored the established Sprint 14 Edit-page behavior, which made T02 a per-handler try/catch plus one shared private helper rather than new architecture.
- **Narrow scope stayed narrow.** Authorization remained before try/catch, Result/NotFound semantics stayed untouched, and unexpected exceptions still propagate — the planning matrix predicted exactly what happened.
- **Verification depth paid off.** T04's real-browser plus SQL before/after method surfaced a subtle pre-existing defect (`Descending=True` round-trip loss) that automated suites could never see, with root cause proven down to Razor boolean-attribute rendering.
- **No-save semantics were already proven.** Existing Application tests asserting `SaveChangesAsync` non-invocation on failure meant the sprint needed no new test infrastructure to demonstrate persistence safety.

---

## 12. What Could Be Improved

- **The `Descending=True` hidden-field defect survived four sprints of verification.** Razor boolean-attribute rendering silently produced `value="value"` for `value="@Model.Descending"`; earlier sort-state checks evidently never exercised a descending round-trip through a POST re-render. Markup-level round-trip checks belong in manual verification checklists.
- **Razor bool attributes are a recurring trap.** The same pattern exists on the Edit page and would resurface anywhere `value="@someBool"` is used; a convention note (prefer `asp-for` or a non-bool expression for hidden bool fields) would prevent recurrence.
- **PageModel-level error handling is becoming a repeated convention.** Edit (Sprint 14) and Details (Sprint 15) now share the same catch → ModelState → re-render shape by copy, not by abstraction; per the Rule of Three, a third occurrence should trigger extraction of a shared helper or base behavior.
- **Crafted-POST verification is powerful but heavy.** T04 required a temporary browser-automation harness; a lightweight, documented harness convention would reduce setup cost for future manual verification tasks.

---

## 13. Decisions Made During Sprint

Initial accepted planning decisions:

1. Keep Sprint 15 limited to Purchase Order Details Submit/Approve/Receive/Cancel.
2. Handle expected `DomainException` at the Razor Page boundary.
3. Preserve existing Domain and Application contracts.
4. Do not introduce a global DomainException handler for this localized issue.
5. Do not create artificial test seams merely to increase automated test counts.
6. Keep Create-page findings deferred.

Decisions recorded during execution:

7. T03 (record-only correction): existing Application Result failures in Details use ModelState + reload + Page(), NOT NotFound() — T02 preserved pre-existing Result semantics, and documentation was corrected rather than code.
8. T04: `Descending=True` round-trip loss recorded as a pre-existing UX limitation and deferred to a future approved task; NOT corrected within Sprint 15 scope.
9. T04: authorization denial kept entirely separate from validation feedback (AccessDenied page with empty summary) — confirmed unchanged by crafted-POST verification.
10. T05: per repository convention (Sprints 11-14), a per-sprint CHANGELOG entry was added even though the sprint is non-release; no version was assigned.

---

## 14. Architecture Observations

Initial observation (confirmed by execution):

The sprint corrected a presentation-boundary asymmetry without moving business rules out of the Domain or altering established Application handler contracts.

Execution observations:

- The Domain remains the sole rule owner: every inline message rendered during T04 came verbatim from the aggregate's `DomainException` — no message text was duplicated into Web code.
- The Application layer was untouched; handlers still propagate `DomainException` and return `Result` failures exactly as before, so the change is invisible below the Razor boundary.
- The private `RenderDomainFailureAsync` helper is PageModel-local by design; no base-class, middleware, or contract abstraction was introduced, and no Web persistence was added.
- Authorization stays structurally first: policy checks and `Forbid()` occur before try/catch, keeping security failures and business-rule failures observably distinct.

---

## 15. Risks

### Confirmed

- A broad `catch (Exception)` would incorrectly hide unexpected failures.
- A global `DomainException` handler could conflate expected workflow validation with invariant/programming failures elsewhere.
- Introducing a new Application Result contract would unnecessarily widen Sprint 15.
- Artificial test seams could add production complexity without meaningful behavioral value.

### Execution Risks

- Details re-render may require more state reload than initially expected.
- query/navigation state may need explicit preservation if current binding behavior differs from planning evidence.
- manual crafted POST scenarios must avoid being confused with authorization or antiforgery failures.

---

## 16. Known Non-Goals

Sprint 15 does not attempt to:

- redesign exception handling for the entire application
- change Purchase Order lifecycle semantics
- add new purchasing features
- change authorization capabilities
- change database schema
- resolve Create-page deferred findings
- create a semantic release

---

## 17. Definition of Done

Sprint 15 can be marked COMPLETE only when:

- all confirmed affected Details POST actions handle expected workflow `DomainException`
- canonical Domain messages render as user-facing validation feedback
- expected workflow failures no longer reach the Development exception page
- authorization semantics remain unchanged
- NotFound semantics remain unchanged
- unexpected exceptions remain unhandled by the local expected-failure path
- rejected operations persist no state changes
- successful workflow paths remain unchanged
- navigation/query state remains coherent
- full automated suites pass
- normal build passes
- full non-incremental build is executed and recorded
- manual browser verification passes
- SQL Server persistence behavior is verified where required
- no schema/migration change is introduced
- no authorization capability/seed change is introduced
- authoritative documentation is synchronized
- deferred Create-page findings remain outside Sprint 15 implementation

---

## 18. Final Sprint Outcome

**Status:** COMPLETE / CLOSED

Sprint 15 delivered its accepted scope exactly: all four Purchase Order Details POST workflows (Submit, Approve, Receive, Cancel) catch expected `DomainException` failures, render the canonical Domain message as user-facing inline validation, reload the Purchase Order through `GetPurchaseOrderHandler`, and return `Page()` via one private local helper (helper reload failure → `NotFound()`), with no HTTP 500 for expected Domain failures. Authorization remains before try/catch and unchanged; existing Result failures keep their ModelState + reload + Page() semantics with `NotFound()` only on helper reload failure; unexpected exceptions propagate. Rejected operations persist nothing (automated no-save tests plus T04 SQL before/after and restart evidence); successful paths are unchanged. Final baseline: 477 passed / 0 failed / 0 skipped; normal build 0 warnings / 0 errors; full rebuild 28 pre-existing warnings / 0 errors. No schema/migration, authorization/seed, package, or project changes; no version bump, tag, or release. The sprint closed as a non-release technical-hardening sprint.

---

## 19. Release Decision

**Final classification:** Non-release technical-hardening sprint — confirmed.

No new business capability, lifecycle transition, authorization capability, or schema change was introduced; the sprint corrected only presentation-layer handling of existing Domain failures.

- Version bump: none — v1.6.0 remains the current release baseline.
- Tag: none.
- GitHub release: none.

---

## 20. Retrospective Closure

- **Sprint status:** COMPLETE / CLOSED
- **All tasks accepted:** Yes — T01-T05 COMPLETE (external review of T05 remains outstanding)
- **Final test baseline:** 477 passed (346 UnitTests, 92 IntegrationTests, 39 Web.Tests), 0 failed, 0 skipped; normal build 0 warnings/0 errors; full non-incremental build 28 pre-existing warnings/0 errors
- **Manual verification:** Passed — real-browser scenarios through the running app (all four failure modes inline, no HTTP 500), SQL before/after persistence checks, authorization denial, Result/NotFound, success-path regression, restart persistence check
- **Database migration:** None (10 existing migrations unchanged; no pending model changes)
- **Authorization changes:** None (capabilities, policies, seeds, and Forbid() ordering unchanged)
- **Release/tag:** None — non-release technical-hardening sprint; v1.6.0 remains the release baseline
- **Known deferred findings:**
  - `Descending=True` hidden-input POST round-trip loss (Details and Edit pages; pre-existing Razor boolean-attribute rendering) — deferred to a future approved task
  - Create duplicate-product `DomainException` presentation — deferred, out of Sprint 15 scope
  - Create FluentValidation production invocation — deferred investigation / record-only
  - Antiforgery `SecurePolicy = Always` vs plain-HTTP serving — record-only environment observation
  - Sprint 12 T07 EditStatus `IsInRole` cleanup — remains Blocked/Deferred from Sprint 12
