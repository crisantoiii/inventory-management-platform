# Sprint 22 Retrospective - Purchase Order Draft Item Edit HTTP/Razor Coverage

> **STATUS: FINAL — SPRINT COMPLETE / CLOSED.**
>
> Finalized at T05 from the externally accepted T01-T04 execution record.

## Sprint Identity

- **Sprint:** Sprint 22
- **Title:** Purchase Order Draft Item Edit HTTP/Razor Coverage
- **Retrospective state:** FINAL
- **Sprint status:** COMPLETE / CLOSED
- **Release classification:** Technical/testing, non-release — no semantic version increment or tag; v1.6.0 remains the latest release baseline

## 1. Objective and Boundary

Close the bounded Purchase Order Draft Edit real-host HTTP/Razor coverage gap without changing production behavior. Sprint 22 verifies the existing Edit GET, UpdateItem, and RemoveItem paths through the real ASP.NET Core host, Razor forms, capability authorization, antiforgery, model binding, Application handlers, Domain rules, and EF Core InMemory persistence.

Sprint 22 made no production, Domain, Application, Web, schema, migration, package, project, configuration, script, or CI change. Its authored executable surface is limited to:

- `tests/InventoryPlatform.Web.Tests/Http/PurchaseOrderEditHttpTests.cs`
- `tests/InventoryPlatform.Web.Tests/Http/PurchaseOrderEditFormExtraction.cs`

## 2. Final Task Outcomes

| Task | Outcome | Status |
|---|---|---|
| T01 | Edit HTTP fixture and narrow form-extraction foundation | COMPLETE / ACCEPTED |
| T02 | H1-H3 Edit GET and authorization boundary | COMPLETE / ACCEPTED |
| T03 | H4-H6 Edit POST, persistence, and failure boundary | COMPLETE / ACCEPTED |
| T04 | Integrated provider-neutral verification and frozen-scope audit | COMPLETE / ACCEPTED |
| T05 | Documentation synchronization and Sprint closure | COMPLETE |

## 3. H1-H6 Behavioral Coverage

1. **H1 — Authorized Draft Edit GET.** A persisted InventoryManager passes the real `PurchaseOrder.Edit` policy and receives HTTP 200 with the Draft editor, Update/Remove forms, antiforgery material, and eight-field navigation state.
2. **H2 — Denied Edit GET.** A persisted test user with no authorization-group relationship is denied by the real capability pipeline and receives HTTP 302 to AccessDenied with the correct semantic ReturnUrl. No capability claim or production bypass is fabricated.
3. **H3 — Submitted Edit GET.** An existing Submitted Purchase Order receives HTTP 302 to Details and remains unchanged.
4. **H4 — Valid UpdateItem POST.** The real rendered form, antiforgery token, and matching cookie produce HTTP 302 PRG to Edit. A fresh same-factory scope proves the exact quantity/unit-cost update and totals, with Draft status, item identity/count, and header fields preserved.
5. **H5 — Zero-quantity Domain failure.** Posting `quantity=0` reaches the Domain invariant, returns HTTP 200 with a null `Location`, displays exactly `Quantity must be greater than zero.`, restores the editor/navigation state, and proves no mutation or partial write.
6. **H6 — Final-item removal.** The real RemoveItem form produces HTTP 302 PRG. Fresh-scope inspection proves the final item was removed, the empty Draft persists, total is 0, and header fields remain unchanged.

Exactly six behavioral cases exist; no seventh case or duplicate scenario was added.

## 4. Test Architecture and Evidence

- The real `PurchaseOrder.Edit` policy is exercised for page and POST authorization.
- Authentication resolves persisted identities; H2 uses a persisted no-group user.
- H4-H6 GET the real page, extract the rendered action and `__RequestVerificationToken`, preserve the matching cookie, and POST the actual form contract. Antiforgery is neither disabled nor manufactured.
- Persistence and no-mutation assertions use a fresh scope from the same isolated factory.
- `PurchaseOrderEditFormExtraction` is purpose-specific and BCL-only, not a generic DOM or route framework.
- The host uses EF Core InMemory. The evidence does not establish SQL Server relational semantics.

## 5. Final Verification Baseline

```text
Purchase Order Edit:       6 passed
Purchase Order Create:     6 passed
Purchase Order Submit:     2 passed
UnitTests:                355 passed
Web.Tests:                 70 passed
ProviderNeutral:          126 passed
Provider-neutral total:   551 passed, 0 failed, 0 skipped
Normal Release build:       0 warnings, 0 errors
Shared script exit:         0
EF pending-model changes:   none
Migrations:                10; latest 20260831141400_CreateAuthorizationSchema
```

The final accepted Graphify refresh from T03 succeeded with 12,041 nodes, 18,148 edges, and 902 communities. T04 and T05 did not refresh Graphify because neither changed executable source.

## 6. Explicit Exclusions

- `SqlServerRelational` tests were not required or executed; no SQL Server claim is made from H1-H6.
- Browser/E2E, JavaScript, visual, and manual-browser verification were not required or executed.
- Approve, Receive, Cancel, broader Details transitions, anonymous/missing-order Edit, antiforgery rejection matrices, and generic route coverage remain outside this sprint.

## 7. Carry-Forward Reconciliation

Sprint 22 closes only the Purchase Order Draft Edit GET/UpdateItem/RemoveItem real-host HTTP/Razor portion of the broader coverage gap. Remaining evidence-supported gaps stay unassigned: Approve/Receive/Cancel HTTP coverage; broader HTTP/Razor routes; SQL Server relational CI and expansion; browser/E2E; report authorization; Viewer permission intent; the EditStatus authorization decision; warning remediation; Sales; Audit; and import/attachment/barcode work.

No Sprint 23 direction is selected.

## 8. Closure

Sprint 22 is complete as technical/testing, non-release work. The latest semantic release remains v1.6.0. The implementation and provider-neutral evidence are ready for the manual PR/merge gate.
