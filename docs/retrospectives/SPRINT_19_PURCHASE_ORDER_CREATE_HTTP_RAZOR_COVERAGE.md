# Sprint 19 Retrospective - Purchase Order Create HTTP/Razor Integration Coverage

> **STATUS: FINAL — SPRINT COMPLETE / CLOSED.**
>
> Finalized at T06 from the externally accepted T01-T05 execution record.

## Sprint Identity

- **Sprint:** Sprint 19
- **Title:** Purchase Order Create HTTP/Razor Integration Coverage
- **Retrospective state:** FINAL
- **Sprint status:** COMPLETE / CLOSED
- **Release classification:** Technical/non-release — no semantic version increment or tag; v1.6.0 remains the latest release baseline

## 1. Objective

Extend the Sprint 17 database-safe HTTP/Razor integration-test foundation to verify the real Purchase Order Create pipeline through representative authorization, antiforgery, binding, persistence, PRG, expected Domain-failure redisplay, restoration, and no-mutation scenarios, with zero production changes.

## 2. Final Task Outcomes

| Task | Outcome | Status |
|---|---|---|
| T01 | Contract and test-seam lock | COMPLETE / ACCEPTED |
| T02 | GET authorization coverage (H1-H3) | COMPLETE / ACCEPTED |
| T03 | Valid POST coverage (H4, marker `S19-T03-VALID-CREATE`) | COMPLETE / ACCEPTED |
| T04 | Duplicate-product failure coverage (H5, marker `S19-T04-DUPLICATE`) | COMPLETE / ACCEPTED |
| T05 | Integrated regression and architecture verification (8 gates) | COMPLETE / ACCEPTED |
| T06 | Documentation synchronization and Sprint closure | COMPLETE |

## 3. H1-H5 Behavioral Coverage

1. **H1 — Anonymous GET challenge.** Anonymous GET `/Purchasing/PurchaseOrders/Create` challenges to Identity login with the correct semantic ReturnUrl.
2. **H2 — Authorized manager access.** Persisted seeded `manager@inventory.local` accesses the real Create page through the real pipeline.
3. **H3 — Real capability denial.** Persisted test-only `purchaseorder-denied@inventory.test`, with no authorization-group assignment, is denied by real `PurchaseOrder.Create` capability authorization. The seeded Viewer is NOT the H3 identity because the seeded Viewer already possesses `PurchaseOrder.Create` (H3 amendment/discovery). No production seed or authorization bypass was added.
4. **H4 — Valid POST → PRG → persistence.** Real GET + real rendered antiforgery token/cookie + indexed POST → real PageModel/Application handler → immediate 302 PRG to `/Purchasing/PurchaseOrders` → same-factory persistence of a Draft Purchase Order plus one item.
5. **H5 — Domain-failure redisplay/restoration/no mutation.** Duplicate ProductId rows trigger the real Domain invariant with canonical message `The product already exists in this purchase order.`; HTTP 200 redisplay with Supplier/Product options, Supplier selection, date, remarks, and both rows/values restored; PO/item counts unchanged and no marker order persisted.

H4/H5 are EF Core InMemory HTTP-host evidence, not SQL Server relational evidence. Sprint 18 remains the relational architecture authority.

## 4. Test Architecture (preserved)

- `InventoryPlatformWebApplicationFactory` remains the host foundation (early sentinel `DefaultConnection`, pre-provider `ApplicationDbContext` replacement, fail-closed structural validation, unchanged real `UseWeb()` startup seeding)
- Isolated InMemory database per factory (`Guid`-suffixed names); no production SQL from HTTP tests
- Persisted-user authentication resolves real Identity GUIDs; no fabricated role/capability claims
- Real capability authorization and real rendered HTTPS antiforgery active; no antiforgery bypass or token manufacturing
- Real Razor PageModel/Application path; persistence checks open a new scope from the same factory
- Narrow, PO-Create-specific form extraction (`PurchaseOrderCreateFormExtraction`); BCL regex/HTML decoding only; no generic browser/HTTP DSL; no third-party HTML parser
- No relational claim is made from InMemory HTTP evidence

## 5. Final Verification Baseline (Sprint 19 T05 — fresh, current environment)

```text
UnitTests:        346 passed
IntegrationTests: 133 passed (relational tier freshly passed against available LocalDB)
Web.Tests:         63 passed (H1-H5 included)
Total:            542 passed, 0 failed, 0 skipped
Normal build:      0 warnings, 0 errors
Non-incremental:  28 warnings, 0 errors (unchanged historical baseline)
EF:                no pending model changes
Migrations:        10; latest 20260831141400_CreateAuthorizationSchema (unchanged; none created)
LocalDB:           (localdb)\MSSQLLocalDB available; relational tier executed fresh (no fallback)
```

Historical evidence: Sprint 18 closure was 537/537 (58 Web.Tests); Sprint 17 closure was 496/496. The Sprint 18 133/133 relational-capable IntegrationTests baseline was freshly re-established by T05 because LocalDB was available, so no historical figure is folded into the 542 total.

## 6. What Went Well

- The Sprint 17 foundation absorbed the Purchase Order Create coverage without any host or production change — the five behaviors landed as pure test additions.
- The H3 amendment (test-only capability-less identity) produced a stronger proof than a role-based denial: denial flows through the real capability policy against a genuinely unassigned user.
- T05 re-verified the whole solution freshly, including the relational tier on available LocalDB, leaving no dependency on historical counts.
- Form extraction stayed narrow and PO-Create-specific, avoiding both a generic HTTP DSL and third-party parser dependencies.

## 7. Important Observations

- The seeded Viewer has broad `PurchaseOrder.*`, including `PurchaseOrder.Create` — recorded as an observation only; no remediation commitment was made.
- H4/H5 prove the represented Create-pipeline scenarios, not every Purchase Order workflow or report.
- The non-incremental warning baseline remains 28 and is deferred; Sprint 19 introduced no warning regression.

## 8. Scope Discipline

- Zero production behavior/source changes; zero test-source changes during T05/T06 (verification/documentation only)
- Zero seed/policy/configuration, migration, package, or project changes; no new migration
- No antiforgery bypass, no fabricated capability claims, no third-party parser, no generic browser/HTTP DSL
- No relational claim from InMemory HTTP evidence; no SQLite substitution
- No semantic version, release tag, or product-feature claim

## 9. Carry-Forward (unassigned — not allocated to any future sprint)

- C19-01 validation invocation/architecture
- C19-02 provider-neutral continuous verification
- C19-03 relational CI
- C19-04 broader HTTP/Razor coverage
- C19-05 browser/E2E
- C19-06 EditStatus authorization
- C19-07 report authorization
- C19-08 warning remediation (the unchanged 28-warning non-incremental baseline)
- C19-09 broader SQL relational/report verification
- C19-10 Sales
- C19-11 Audit
- C19-12 import/attachment/barcode
- Observation: seeded Viewer has broad `PurchaseOrder.*`, including `PurchaseOrder.Create` (recorded only; no remediation commitment)

No Sprint 20 scope is assigned or implied.

## 10. Closure

Sprint 19 is complete and closed on the accepted T05 verification baseline. T06 changed documentation only. Graphify was not updated because repository convention does not include documentation-only changes in the source graph. Git operations remain owner-controlled and were not performed.
