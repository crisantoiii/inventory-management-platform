# Sprint 16 Retrospective - Purchase Order POST Round-Trip State and Create Failure Presentation Corrections

> **STATUS: PLANNING-TIME BASELINE — NOT A COMPLETED RETROSPECTIVE.**
>
> This document is the Sprint 16 retrospective **baseline/template** created at planning finalization. Sections marked **[PLANNED]** record accepted planning content only. Sections marked **[AWAITING EXECUTION EVIDENCE]** are placeholders that T01–T05 must fill with actual recorded results. No commits, test results, browser evidence, or completed outcomes are claimed here. Per repository convention (Sprints 11–15), this baseline will be finalized into the retrospective only after T04 acceptance, during T05.

## Sprint Identity

- **Sprint:** Sprint 16
- **Title:** Purchase Order POST Round-Trip State and Create Failure Presentation Corrections
- **Planning authority:** `plan/SPRINT_16_PLANNING_REPORT.md` — Revision 1, **ACCEPTED / FINALIZED**
- **Task breakdown:** `plan/SPRINT_16_TASK_BREAKDOWN.md`
- **Retrospective state:** BASELINE — AWAITING EXECUTION (T01 NOT STARTED)
- **Sprint status:** NOT STARTED
- **Release classification:** [PLANNED] Non-release technical-hardening sprint — no version bump, tag, or GitHub release

---

## 1. Sprint Objective [PLANNED]

Preserve Purchase Order descending-sort state through POST round-trips and render expected Create workflow `DomainException` failures as inline validation feedback — eliminating the last known silent navigation-state loss and the last known HTTP-500 presentation path in the Purchasing workflow.

Accepted scope is limited to:

- **A — Descending round-trip correction:** exactly six hidden `Descending` inputs (`Details.cshtml` Submit/Approve/Cancel/Receive; `Edit.cshtml` UpdateItem/RemoveItem) using the accepted explicit-string design `value="@(Model.Descending ? "true" : "false")"`.
- **B — Create DomainException presentation:** narrow `catch (DomainException)` in `CreateModel.OnPostAsync` rendering the canonical Domain message through model-level `ModelState` and reusing the existing Create failure restoration (`EnsureAtLeastOneItem()`, `PopulateDropdownListsAsync()`, `Page()`).

The sprint must preserve Domain rules, Application contracts, authorization behavior, NotFound behavior, persistence semantics, unexpected-exception propagation, and successful workflow behavior.

---

## 2. Why This Sprint Exists [PLANNED]

- **A:** Sprint 15 T04 manual verification surfaced a pre-existing `Descending=True` hidden-input POST round-trip loss (Razor boolean-attribute rendering on `value="@Model.Descending"`; `true` renders `value="value"`, `false` omits the attribute; the posted `"value"` string fails bool binding). Revision 1 confirmed all six markup sites in source and the mechanism in generated Razor code. Recorded as deferred in Sprint 15 and every current-state doc; now accepted for correction.
- **B:** Sprint 14/15 planning deferred Create-page duplicate-product `DomainException` presentation. Revision 1 confirmed `PurchaseOrder.AddItem` throws the duplicate-product rule, `CreatePurchaseOrderHandler` propagates it uncaught, and `CreateModel` has no Domain-failure handling — Create is the only Purchase Order POST surface without the established inline pattern (Details: 4 catches since Sprint 15; Edit: 2 catches since Sprint 14; Create: 0).

---

## 3. Accepted Architecture Baseline [PLANNED]

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

### In Scope [PLANNED]

- Candidate A markup correction (six inputs, two Razor files)
- Candidate B narrow DomainException handling (one PageModel method)
- regression verification embedded in T02/T03
- T04 integrated and manual verification (including SQL before-vs-after evidence)
- T05 documentation synchronization and closure

### Out of Scope [PLANNED — accepted non-goals]

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

## 5. Planned Tasks

| Task | Title | Status |
|---|---|---|
| T01 | Contract Verification and Design Lock | COMPLETE |
| T02 | Descending Round-Trip Correction | NOT STARTED |
| T03 | Create Domain-Failure Inline Presentation | NOT STARTED |
| T04 | Integrated and Manual Verification | NOT STARTED |
| T05 | Documentation Synchronization and Sprint Closure | NOT STARTED |

Details, boundaries, acceptance criteria, and stop conditions: `plan/SPRINT_16_TASK_BREAKDOWN.md`.

---

## 6. Starting Quality Baseline [PLANNED]

Accepted Revision 1 planning baseline (reproduced during Revision 1 discovery; these are **planning-baseline values, not fresh execution evidence** — T04 must produce fresh recorded results):

- **UnitTests:** 346 passed
- **IntegrationTests:** 92 passed
- **Web.Tests:** 39 passed
- **Total:** 477 passed / 0 failed / 0 skipped
- **Normal build:** 0 warnings, 0 errors
- **Full non-incremental build:** 28 pre-existing warnings, 0 errors
- Warning families (pre-existing): CS0108, CS0114, CS8601, CS8602, CS8604, CS8618 — none in Purchasing Purchase Order files

Sprint 16 execution must record fresh test/build results rather than assuming these counts remain unchanged.

---

## 7. Planned Verification Strategy

### Automated [PLANNED]

- full UnitTests / IntegrationTests / Web.Tests (fresh runs in T02/T03 regression and T04)
- normal solution build and full non-incremental build (T04)
- no new automated tests — no legitimate PageModel/Razor seam exists (Sprint 14 Option B holds); no artificial seams permitted

### Manual Browser [PLANNED — awaiting T04 evidence]

- all six Descending forms × true/false states: rendered HTML check, failure re-render preservation, success PRG/query-state preservation
- Index sort-header/pagination regression
- duplicate-product Create inline message; crafted quantity/unit-cost Domain failures where practical
- successful Create, Details, and Edit regression
- authorization spot check (denied persona → AccessDenied, not validation feedback)

### Persistence [PLANNED — evidence hierarchy]

Primary evidence (both required):

1. SQL/database before-vs-after manual evidence for the Create failure path (read-only queries)
2. existing Application no-save test evidence (`CreatePurchaseOrderHandlerTests.HandleAsync_DuplicateProductInRequest_ThrowsDomainException`)

Application restart check is supplementary only.

**Status: [AWAITING EXECUTION EVIDENCE] — no manual/browser/SQL evidence exists yet.**

---

## 8. Deferred Findings at Sprint Start [PLANNED]

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

- **Status:** NOT STARTED
- **[AWAITING EXECUTION EVIDENCE]** — expected result: six inputs corrected with the explicit-string design; rendered-HTML evidence; fresh regression suite counts; build result; `graphify update .` confirmation.

### T03 - Create Domain-Failure Inline Presentation

- **Status:** NOT STARTED
- **[AWAITING EXECUTION EVIDENCE]** — expected result: narrow `DomainException` catch with the existing restoration reused; fresh regression suite counts; build result; `graphify update .` confirmation.

### T04 - Integrated and Manual Verification

- **Status:** NOT STARTED
- **[AWAITING EXECUTION EVIDENCE]** — expected result: completed six-form × two-state matrix; Create failure/success scenarios; authorization spot check; fresh build/test baseline; SQL before-vs-after tables; restart check (supplementary). If a defect is discovered: record and stop for external review.

### T05 - Documentation Synchronization and Sprint Closure

- **Status:** NOT STARTED
- **[AWAITING EXECUTION EVIDENCE]** — expected result: documentation synchronized; Razor hidden-bool lesson recorded; Candidate E checkpoint completed (Section 12); A/B marked resolved only when proven; retrospective finalized; sprint marked COMPLETE only if T01–T04 acceptance criteria were satisfied.

---

## 10. Sprint Metrics

### Planning Baseline [PLANNED]

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

**[AWAITING EXECUTION EVIDENCE]** — T04 must record fresh per-suite counts, totals, and build warning/error counts here. Do not carry the planning baseline forward as the final result.

---

## 11. Expected Observations (to confirm or refute during execution) [PLANNED HYPOTHESES — NOT FINDINGS]

- The explicit-string rendering fixes the `Descending` round-trip in all six forms with no binding-side effects.
- The narrow Create catch is sufficient because the duplicate-product `DomainException` is thrown inside `PurchaseOrder.AddItem` before `AddAsync`, so the failure path persists nothing.
- The existing `ModelOnly` validation summary on `Create.cshtml` renders the new model-level error without markup changes.
- These are design expectations to be confirmed by T01–T04 evidence; they must not be recorded as outcomes until verified.

---

## 12. Candidate E — Rule-of-Three Re-Evaluation Checkpoint [MANDATORY IN T05 — NOT TO BE IMPLEMENTED IN SPRINT 16]

After T03, three PageModels will share the same shape by copy:

1. `DetailsModel.RenderDomainFailureAsync` (Sprint 15 — ModelState + aggregate reload + `Page()`)
2. `EditModel.RenderDomainFailureAsync` (Sprint 14 — ModelState + aggregate reload + Draft-check redirect + `Page()`)
3. `CreateModel` failure restoration (Sprint 16 — ModelState + item restore + dropdown reload + `Page()`)

**[AWAITING T05 EXECUTION]** — during T05, re-apply the repository Rule-of-Three test (DD-018, knowledge.md §3): are the three occurrences genuinely the same responsibility, or do the differing reload/restore steps (aggregate reload vs dropdown repopulation vs Draft-check redirect) mean the shared part is only the two-line ModelState+catch prologue? Record the decision and reasoning here. **Do not implement any extraction in Sprint 16 regardless of the conclusion.**

---

## 13. Decisions Made During Sprint

**Planning-era accepted decisions [PLANNED]:**

1. Sprint 16 limited to Candidates A + B.
2. Candidate A uses the explicit-string design; `asp-for` and query-state redesign rejected.
3. Candidate B uses a narrow `catch (DomainException)` reusing the existing restoration; no reload helper, no validator wiring, no shared extraction.
4. Non-release technical-hardening classification.
5. Candidates C, D1, D4, E deferred; `PagedRequest.Status` finding, antiforgery observation, and Sprint 10 seed findings rejected/stale.

**Decisions recorded during execution:**

**[AWAITING EXECUTION EVIDENCE]** — T01–T05 record actual decisions/deviations here.

---

## 14. Known Non-Goals [PLANNED]

Sprint 16 does not attempt to: redesign exception handling application-wide; change Purchase Order lifecycle semantics; add purchasing features; change authorization; change the database schema; wire FluentValidation; extract shared helpers; normalize tag helpers; resolve the EditStatus decision; create a semantic release.

---

## 15. Definition of Done [PLANNED]

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

**[AWAITING EXECUTION EVIDENCE]** — to be written only after T05, from actual recorded results.

## 17. Release Decision [PLANNED]

Planned classification: **non-release technical-hardening sprint** — no new business capability, lifecycle transition, authorization capability, or schema change. Version bump: none (v1.6.0 remains the release baseline). Tag: none. GitHub release: none. To be confirmed at T05 against actual outcomes.

## 18. Retrospective Closure

- **Sprint status:** NOT STARTED (baseline stage)
- **All tasks accepted:** No — T01–T05 NOT STARTED
- **Final test baseline:** [AWAITING EXECUTION EVIDENCE]
- **Manual verification:** [AWAITING EXECUTION EVIDENCE]
- **Database migration:** [PLANNED] None expected — confirm during T04
- **Authorization changes:** [PLANNED] None expected — confirm during T04
- **Release/tag:** [PLANNED] None — non-release classification
- **Known deferred findings (carried):** Candidate C; Candidate D1; Candidate D4; Candidate E (re-evaluation checkpoint in Section 12)
- **Graphify:** not run at baseline stage (planning/documentation only); T02/T03 must run `graphify update .` after their source changes
- **Git operations:** none — the developer controls Git manually
