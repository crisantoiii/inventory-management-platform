# Sprint 23 — Purchase Order Lifecycle HTTP/Razor Completion

## Identity

- Sprint: 23
- Direction: Candidate A
- Classification: technical/testing, non-release
- Status: Complete/Closed after accepted T04 integrated verification

## Objective

Close the remaining frozen real-host HTTP/Razor coverage for Purchase Order Approve, Receive, and Cancel without changing production behavior.

## Delivered Coverage

1. H1 — Submitted Approve success.
2. H2 — Approved partial Receive transitions to `Receiving`.
3. H3 — Approved final Receive transitions to `Completed`.
4. H4 — Approved excess Receive returns the exact Domain error and proves atomic no-mutation.
5. H5 — Submitted Cancel success.
6. H6 — A genuinely rendered Submitted Cancel form becomes stale after the persisted order advances to Approved and is rejected without Cancel-side mutation.
7. H7 — A persisted View-only user passes the Details View gate and is denied at Receive action authorization without mutation.

No eighth lifecycle behavior was added.

## Technical Evidence

The tests exercise the real in-process ASP.NET Core host. They GET the production Details page, extract genuine rendered form actions and values, retain the real antiforgery token with its matching cookie, and POST through production authorization, model binding, PageModel, Domain, and persistence. Redirect and navigation checks compare all eight navigation fields semantically.

Persistence assertions use fresh scopes from the same isolated factory. Partial and final Receive distinguish `Receiving` from `Completed`, assert exact ReceivedQuantity and Product stock deltas, and prove exactly one correct `StockIn` transaction with the expected ProductId, quantity, `PO-{id}` reference, remarks, and plausible UTC timestamp. Excess Receive proves complete atomic no-mutation.

The stale-Cancel case uses a genuinely rendered form followed by a persisted Submitted-to-Approved transition before POST. The View-only case creates a real user through `UserManager<ApplicationUser>`, persists a dedicated group with the existing `PurchaseOrder.View` capability through `AddCapability`, assigns the user through `AssignUser`, grants no Receive capability, and fabricates no claims or seed data.

## Final Verification

- UnitTests: 355 passed
- Web.Tests: 77 passed
- ProviderNeutral IntegrationTests: 126 passed
- Provider-neutral total: 558 passed
- Failed: 0
- Skipped: 0
- Purchase Order HTTP fixtures: Create 6, Submit 2, Edit 6, Lifecycle 7
- Shared provider-neutral script: exit 0
- Normal Release: 0 warnings, 0 errors
- Forced non-incremental Release: 28 historical warnings, 0 errors
- Sprint 23 warnings: 0
- EF pending model changes: none
- Migrations: 10
- Graphify: 12,760 nodes, 19,018 edges, 949 communities

## File and Production Surface

Exactly two executable/test-source files comprise Sprint 23:

1. `tests/InventoryPlatform.Web.Tests/Http/PurchaseOrderLifecycleHttpTests.cs`
2. `tests/InventoryPlatform.Web.Tests/Http/PurchaseOrderLifecycleFormExtraction.cs`

Production source, shared test support, schema, migrations, packages, projects, configuration, scripts, CI, and release state were unchanged.

## Boundaries and Limitations

SQL Server relational acceptance was not executed for these lifecycle HTTP cases, so no SQL Server transaction-semantics claim is made. Browser/E2E and manual browser verification were not executed. The accepted evidence proves real-host HTTP/Razor composition and provider-neutral persisted behavior for the frozen seven-case matrix.

Sprint 23 did not redesign Viewer permissions, report authorization, or EditStatus authorization; remediate historical warnings; or change production UI behavior.

## Outcome and Follow-On

Sprint 23 closed successfully after T04 acceptance. No version bump or tag was created, and v1.6.0 remains the latest semantic release.

The next sprint requires fresh post-Sprint-23 repository discovery and candidate selection. This retrospective does not select Sprint 24 work.
