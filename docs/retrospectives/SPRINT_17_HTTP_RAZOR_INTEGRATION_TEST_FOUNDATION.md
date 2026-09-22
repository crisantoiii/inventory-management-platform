# Sprint 17 Retrospective - HTTP/Razor Integration-Test Foundation

> **STATUS: FINAL — SPRINT COMPLETE / CLOSED.**
>
> Finalized at T09 from the accepted T01–T08 execution record. Sprint 17 is complete and closed.

## Sprint Identity

- **Sprint:** Sprint 17
- **Title:** HTTP/Razor Integration-Test Foundation
- **Planning authority:** frozen Sprint 17 planning/task breakdown (T01–T09 execution prompts)
- **Retrospective state:** FINAL
- **Sprint status:** COMPLETE / CLOSED
- **Release classification:** Non-release technical/testing sprint — no version bump, no tag, no GitHub release; v1.6.0 remains the release baseline

---

## 1. Sprint Objective

Close the gap where `InventoryPlatform.Web.Tests` could not exercise the real ASP.NET Core HTTP/Razor pipeline: establish a database-safe in-process `WebApplicationFactory` test foundation with real startup seeding, test-only seeded-user authentication, production capability authorization, and real antiforgery behavior, then prove the foundation through representative HTTP tests — not to add broad HTTP route coverage.

---

## 2. Final Task Outcomes

| Task | Title | Status |
|---|---|---|
| T01 | Discovery — HTTP test-host lifecycle and safety design | COMPLETE — discovered the planned `IStartupFilter` pre-seeding guard was invalid (blocker) |
| T02 | Test-host seam and approved packages | COMPLETE — `public partial class Program { }` + `Microsoft.AspNetCore.Mvc.Testing` + `Microsoft.EntityFrameworkCore.InMemory` |
| T03 | Database-safe `WebApplicationFactory` | COMPLETE / ACCEPTED — sentinel, replacement, fail-closed structural validation, real seeding, isolation (5 tests) |
| T04 | Seeded-user test authentication | COMPLETE / ACCEPTED — real GUID identities, no fabricated claims (8 tests) |
| T05 | Category Create GET authorization matrix | COMPLETE / ACCEPTED — challenge / manager 200 / viewer forbid (3 tests) |
| T06 | Category Create antiforgery POST / PRG / persistence | COMPLETE / ACCEPTED (1 test) |
| T07 | Purchase Order empty-Draft Submit domain-failure HTTP proof | COMPLETE / ACCEPTED — redisplay, navigation preservation, no mutation (2 tests) |
| T08 | Isolation, repeatability, and integrated verification | COMPLETE — **verification-only; no source changes, no commit required** |
| T09 | Documentation synchronization and Sprint 17 closure | COMPLETE |

---

## 3. Starting and Final Baselines

Starting baseline (entering Sprint 17): 477 passing (346 UnitTests, 92 IntegrationTests, 39 Web.Tests).

Final accepted baseline (post-T08):

```text
UnitTests:        346 passed
IntegrationTests:  92 passed
Web.Tests:         58 passed
Total:            496 passed, 0 failed, 0 skipped
Build:             0 errors (normal: 0 warnings; non-incremental: 28 pre-existing warnings)
EF:                no pending model changes
```

Integrated foundation evidence (T08):

- T03–T07 integrated subset: 19/19 — two independent invocations
- Complete Web.Tests: 58/58 — two independent invocations
- Default xUnit parallelism preserved; no serialization, sleeps, or retries

---

## 4. What Went Well

- **Evidence-driven T01 caught the invalid lifecycle assumption before implementation.** The discovery step established that `IStartupFilter` cannot guard `UseWeb()`'s synchronous `IdentitySeeder.SeedAsync` call, preventing an unsafe guard from being built and later trusted.
- **Narrow Revision-4 correction avoided unnecessary production seams.** The accepted architecture reuses the real production pipeline (sentinel containment + pre-provider replacement + structural validation) instead of an alternate startup class, a test-only seeding-suppression flag, or a production environment branch.
- **Representative HTTP tests exercised the real pipeline end-to-end:** real authentication resolution, production capability authorization, real Razor rendering, real antiforgery token/cookie validation, real model binding, PRG, `DomainException` failure redisplay, and same-factory persistence — with no fabricated claims, no antiforgery bypass, and no HTML-parser package.
- **Isolation/repeatability verification passed without serialization or retries.** Two independent integrated runs and two full Web.Tests runs were identical (19/19, 58/58) under default parallelism.
- **The technical baseline stayed clean throughout:** 0/0 normal build, 28 pre-existing warnings non-incremental, no EF pending model changes, and every task gate grew the suite arithmetically (477 → 479 → 480 → 483 → 484 → 486 → 496 with T08 adding nothing).

---

## 5. What Changed From the Original Plan

- **Original `IStartupFilter` pre-seeding guard rejected.** Final safety architecture moved to early host-configuration sentinel injection + pre-provider `ApplicationDbContext` replacement + fail-closed structural validation. `IStartupFilter` must not be documented as the final guard anywhere.
- **T08 required no source change.** Existing T03–T07 evidence already proved isolation, independent seeding, repeatability, order independence, and integrated compatibility; no test was created merely because T08 existed.
- **Rendered-behavior discoveries (test-harness level only, no production change):**
  - The effective Submit form action rendered by `asp-page-handler` is `/Purchasing/PurchaseOrders/Details/{id}?handler=Submit` — the handler selector is appended to the action, not carried in a hidden field.
  - The Details Submit form renders `FromDate`/`ToDate` hidden inputs with the host culture's short-date pattern (unlike anchor links, which use `yyyy-MM-dd`); the tests therefore assert semantic date round-trips under the server's binding culture rather than literal strings.
- **No T05/T06/T07 production defects surfaced** — all discovered issues were test-expectation corrections (three materially distinct, evidence-based T07 test-only corrections).

---

## 6. What Remains (Deferred, Not Completed)

- Broader HTTP/Razor route coverage beyond the representative Category GET / Category POST / Purchase Order failure POST cases (other modules and remaining Category/Purchase Order routes are not HTTP-covered).
- Browser/Playwright end-to-end automation.
- Relational-provider (SQL Server) verification — including for the HTTP test host, which claims no relational behavior from InMemory.
- CI provider establishment.
- Create Category / Create PO FluentValidation production-invocation question (Sprint 16 Candidate C) — still deferred.
- Sprint 12 EditStatus self-deactivation/authorization behavior decision — still deferred.
- Broader InventoryManagement OR-composite coverage question — still deferred.
- Viewer/User.View decision — still deferred.
- Report authorization coverage decision — still deferred.
- Compiler warning baseline remediation (28 pre-existing warnings) — untouched.
- Candidate E / shared POST failure-render extraction — still deferred per Sprint 16 re-evaluation.

---

## 7. Lessons

- **Startup lifecycle assumptions must be verified against actual minimal-host execution order.** The `IStartupFilter` plan would have shipped an ineffective guard; executing `app.UseWeb()`'s real call order in T01 exposed it before any code was written.
- **Database safety needs both containment and structural fail-closed proof.** A sentinel connection value alone would not detect a failed replacement; the structural validator plus pre-provider failure closes the gap deterministically.
- **Rendered Razor behavior should be observed rather than guessed.** Form action semantics (`?handler=Submit`) and culture-formatted hidden values were proven only by reading actual responses — source intent differed in both cases.
- **Verification tasks should not create code when existing evidence is sufficient.** T08's verification-first rule (run before deciding, classify A/B/C) prevented a redundant test from being added.
- **Reconciliation arithmetic stated explicitly kept the baseline honest** (477 → 496: +5 T03, +8 T04, +3 T05, +1 T06, +2 T07, +0 T08).

---

## 8. Sprint 17 Does Not Claim

- Exhaustive HTTP coverage of Category, Purchase Order, or any other module's routes.
- SQL Server/relational behavior proven by the InMemory-backed HTTP tests.
- Any product feature change: production behavior, Domain rules, authorization semantics, and schema are unchanged; the only production seam is `public partial class Program { }`.
- A release: Sprint 17 is non-release; no version, tag, or GitHub release was created.

---

## 9. Sprint 17 Closure

- T09 documentation synchronization complete; all current-state documents synchronized to the 496-test baseline.
- T01 lifecycle correction and Revision-4 safety architecture recorded in `docs/DESIGN_DECISIONS.md` (DD-043) and `docs/TESTING_CONVENTIONS.md`.
- No Git operations performed by the agent; commit remains owner-controlled.

The next activity is a separate Sprint Planning session. No next-sprint scope is defined and none was invented here.
