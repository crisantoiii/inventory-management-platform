# Sprint 15 Retrospective - Purchase Order Workflow Error Handling and UX Hardening

## Sprint Identity

- **Sprint:** Sprint 15
- **Title:** Purchase Order Workflow Error Handling and UX Hardening
- **Planning authority:** `plan/SPRINT_15_PLANNING_REPORT.md` - Revision 2
- **Task breakdown:** `SPRINT_15_TASK_BREAKDOWN.md`
- **Retrospective state:** BASELINE
- **Sprint status:** NOT STARTED
- **Implementation status:** NOT STARTED
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

| Task | Title | Initial Status |
|---|---|---|
| T01 | Contract Verification and Design Lock | NOT STARTED |
| T02 | Purchase Order Details Workflow Error Handling | NOT STARTED |
| T03 | Regression and Coverage Verification | NOT STARTED |
| T04 | Integrated and Manual Verification | NOT STARTED |
| T05 | Documentation Synchronization and Sprint Closure | NOT STARTED |

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

- **Status:** NOT STARTED
- **Date:** -
- **Result:** -
- **Files changed:** -
- **Tests:** -
- **Build:** -
- **Graphify:** -
- **Notes:** -

### T02 - Purchase Order Details Workflow Error Handling

- **Status:** NOT STARTED
- **Date:** -
- **Result:** -
- **Files changed:** -
- **Tests:** -
- **Build:** -
- **Graphify:** -
- **Notes:** -

### T03 - Regression and Coverage Verification

- **Status:** NOT STARTED
- **Date:** -
- **Result:** -
- **Files changed:** -
- **Tests:** -
- **Build:** -
- **Graphify:** -
- **Notes:** -

### T04 - Integrated and Manual Verification

- **Status:** NOT STARTED
- **Date:** -
- **Result:** -
- **Automated tests:** -
- **Normal build:** -
- **Full non-incremental build:** -
- **Manual browser verification:** -
- **SQL Server verification:** -
- **Graphify:** -
- **Notes:** -

### T05 - Documentation Synchronization and Sprint Closure

- **Status:** NOT STARTED
- **Date:** -
- **Result:** -
- **Files changed:** -
- **Documentation synchronized:** -
- **Graphify:** -
- **Notes:** -

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

To be completed during T04/T05.

| Metric | Final |
|---|---:|
| UnitTests | TBD |
| IntegrationTests | TBD |
| Web.Tests | TBD |
| Total | TBD |
| Failed | TBD |
| Skipped | TBD |
| Normal build warnings | TBD |
| Normal build errors | TBD |
| Full rebuild warnings | TBD |
| Full rebuild errors | TBD |

---

## 11. What Went Well

To be completed during Sprint 15 closure.

---

## 12. What Could Be Improved

To be completed during Sprint 15 closure.

---

## 13. Decisions Made During Sprint

To be completed as tasks are accepted.

Initial accepted planning decisions:

1. Keep Sprint 15 limited to Purchase Order Details Submit/Approve/Receive/Cancel.
2. Handle expected `DomainException` at the Razor Page boundary.
3. Preserve existing Domain and Application contracts.
4. Do not introduce a global DomainException handler for this localized issue.
5. Do not create artificial test seams merely to increase automated test counts.
6. Keep Create-page findings deferred.

---

## 14. Architecture Observations

To be updated during execution.

Initial observation:

The sprint is intended to correct a presentation-boundary asymmetry without moving business rules out of the Domain or altering established Application handler contracts.

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

**Status:** NOT STARTED

To be completed during T05.

---

## 19. Release Decision

**Initial classification:** Non-release technical-hardening sprint.

No semantic version is assigned during planning.

Final closure must confirm whether execution remained within this classification.

---

## 20. Retrospective Closure

To be completed after T05 acceptance.

- **Sprint status:** NOT STARTED
- **All tasks accepted:** No
- **Final test baseline:** TBD
- **Manual verification:** TBD
- **Database migration:** Expected none
- **Authorization changes:** Expected none
- **Release/tag:** Not planned
- **Known deferred findings:** Create duplicate-product error presentation; Create FluentValidation production invocation
