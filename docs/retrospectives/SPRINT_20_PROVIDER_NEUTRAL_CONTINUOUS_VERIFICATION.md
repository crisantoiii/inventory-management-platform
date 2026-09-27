# Sprint 20 Retrospective - Provider-Neutral Continuous Verification

> **STATUS: FINAL — SPRINT COMPLETE / CLOSED.**
>
> Finalized at T06 from the externally accepted T01-T05 execution record, including the T03 external-review remediation.

## Sprint Identity

- **Sprint:** Sprint 20
- **Title:** Provider-Neutral Continuous Verification
- **Retrospective state:** FINAL
- **Sprint status:** COMPLETE / CLOSED
- **Release classification:** Technical/non-release — no semantic version increment or tag; v1.6.0 remains the latest release baseline

## 1. Objective

Establish provider-neutral continuous verification for the platform: an explicit, fail-safe IntegrationTest tier contract; a single shared local/CI verification command; a GitHub Actions provider-neutral gate; and full documentation synchronization — with zero production changes and without executing the SQL Server relational tier or browser/E2E work.

## 2. Delivered Outcome

- **Explicit tier contract:** every IntegrationTest resolves to exactly one supported `TestTier` value — `ProviderNeutral` or `SqlServerRelational`. Classification follows actual runtime provider dependency, not folder/namespace.
- **Fail-safe classification audit** (`TierClassificationAudit` + tests): rejects missing, duplicate/multiple, and unknown tier values; fails closed; locks the SQL-provider-bound inventory by fully qualified identity.
- **Shared verification entry point:** `scripts/verify-provider-neutral.ps1` — the single command authority for tool restore (`dotnet-ef 10.0.10`), tool-resolution evidence, solution restore, normal Release build, UnitTests, Web.Tests, affirmative ProviderNeutral IntegrationTests, EF `migrations has-pending-model-changes`, TRX output, and an explicit executed/excluded tier summary. Working-directory independent; strict native exit-code propagation.
- **GitHub Actions gate:** `.github/workflows/provider-neutral-verification.yml` — PR→`main`, push→`main`, `workflow_dispatch`; `windows-latest`; .NET `10.0.x`; `contents: read`; delegates entirely to the shared script (no duplicated verification commands); no `-SkipRestore`; TRX artifact upload with `if: always()`; no secrets, no SQL/LocalDB/browser setup.
- **Zero provider contact:** the final provider-neutral gate makes no LocalDB/SQL Server connection, database creation, or provider execution attempt.

## 3. Final Task Outcomes

| Task | Outcome | Status |
|---|---|---|
| T01 | Explicit `TestTier` contract across all IntegrationTests | COMPLETE / ACCEPTED |
| T02 | Fail-safe tier classification audit + locked SQL inventory (13 at T02) | COMPLETE / ACCEPTED |
| T03 | Shared provider-neutral verification script | COMPLETE / ACCEPTED (after external-review remediation) |
| T03-REM | Reclassification of the LocalDB reachability probe; inventory 13→14 / 127→126 | COMPLETE / ACCEPTED |
| T04 | GitHub Actions provider-neutral gate (structural/local verification) | COMPLETE / ACCEPTED |
| T05 | Integrated verification + hosted-evidence gate | COMPLETE / ACCEPTED (hosted status conditional at T05 time) |
| T06 | Documentation synchronization and Sprint closure | COMPLETE |

## 4. Final Verification Baseline (Sprint 20 T05 — fresh, current environment)

```text
UnitTests:               346 passed
Web.Tests:                63 passed
ProviderNeutral:         126 passed (classification audit included)
SqlServerRelational:      14 discovered only (NOT executed during Sprint 20)
Classification audit:      7/7 passed
EF pending-model:          none
Normal Release build:      0 warnings, 0 errors
Shared script exit:        0
LocalDB/SQL contact:       none (instance Stopped before and after; last-start unchanged)
Browser/E2E:               NOT EXECUTED (outside the gate)
```

## 5. Hosted Evidence (qualified)

The Sprint 20 GitHub Actions provider-neutral job `Provider-neutral verification (windows-latest)` completed successfully on a clean hosted runner. Supplied hosted evidence confirms successful checkout, .NET 10.0.x setup, provider-neutral script execution, workflow-summary generation, and successful upload of a three-file `provider-neutral-verification-results` artifact. Detailed test counts and EF console output remain supported by the accepted local/integrated T05 verification record rather than independently extracted from the supplied hosted evidence.

Artifact metadata (recorded for traceability): artifact ID `10926782773`; 3 uploaded files; size 111464 bytes; SHA256 `38a540e304f33bd77cfbf9d6f4e21086dbf250ee60e0e8eb7320d108ddde739f`. The Node `punycode`/`url.parse()` deprecation warnings emitted by `actions/upload-artifact@v4` did not fail the upload and are not application/build/test failures.

## 6. What Went Well

- **Explicit tiers made provider contact visible.** The T03 zero-contact experiment (stop LocalDB → run gate → instance auto-started) surfaced a real misclassification that source review alone had rationalized away. The contract did exactly what it was designed to do.
- **One command authority removed drift risk.** The workflow is a thin shell around `scripts/verify-provider-neutral.ps1`; nothing in CI can silently diverge from local verification semantics.
- **Affirmative inclusion is audit-compatible.** Positive `TestTier=ProviderNeutral` selection means the classification audit itself runs inside the gate on every invocation, so contract drift fails the gate rather than hiding behind an exclusion filter.
- **Runtime dependency beat success-requirements reasoning.** The remediation rule — a test that attempts a provider connection is relational, regardless of whether it can pass without the provider — is now codified in source comments and conventions.
- **Clean-hosted-runner proof.** Hosted `windows-latest` has no LocalDB at all, so the hosted PASS is simultaneously a clean-environment proof of the tool-manifest EF resolution path and the gate's provider-neutrality.

## 7. Important Observations

- The T03 initial classification of `RelationalTier_FailsHard_WhenServerIsUnavailable` as ProviderNeutral was a genuine misclassification: its body opens a real 1-second-timeout `SqlConnection` to the approved LocalDB instance (by-design "fail hard, never skip" probe). "Passes with LocalDB absent" was insufficient; the remediation standard is no connection attempt at all.
- The 28-warning non-incremental historical build baseline is unchanged and remains deferred (C19-08).
- Hosted evidence proves job/step/artifact success; it does not independently expose hosted test counts or the artifact's internal file names — those remain locally evidenced (T05).

## 8. Scope Discipline

- Zero production behavior/source changes; zero test/script/workflow source changes in T06
- No SQL Server relational test execution during Sprint 20 (discovery and lock verification only)
- No browser/E2E execution
- No semantic version, tag, or release; v1.6.0 remains latest
- No Git operations from the agentic workspace

## 9. Carry-Forward (unassigned — not allocated to any future sprint)

- C20-01 SQL Server relational CI (hosted relational execution remains future work; requires a hosted SQL Server/LocalDB endpoint decision)
- C20-02 broader HTTP/Razor coverage (carried from C19-04)
- C20-03 browser/E2E (carried from C19-05)
- C20-04 EditStatus authorization guard decision (carried from C19-06)
- C20-05 report authorization (carried from C19-07)
- C20-06 warning remediation — the unchanged 28-warning non-incremental baseline (carried from C19-08)
- C20-07 broader SQL relational/report verification (carried from C19-09)
- C20-08 Sales (carried from C19-10)
- C20-09 Audit (carried from C19-11)
- C20-10 import/attachment/barcode (carried from C19-12)
- C20-11 validation invocation/architecture (carried from C19-01)
- Observation preserved without a remediation commitment: the seeded Viewer has broad `PurchaseOrder.*`, including `PurchaseOrder.Create`

Sprint 20 closed the C19-02 provider-neutral continuous verification carry-forward item. No Sprint 21 scope is selected; next-sprint scope remains to be planned.

## 10. Closure

Sprint 20 is complete and closed on the accepted T05 integrated verification baseline plus the accepted hosted run evidence. T06 changed documentation only. Graphify was not updated because repository convention does not include documentation-only changes in the source graph. Git operations remain owner-controlled and were not performed.
