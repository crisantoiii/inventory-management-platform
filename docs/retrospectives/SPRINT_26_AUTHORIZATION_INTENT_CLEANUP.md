# Sprint 26 Retrospective: Authorization Intent & Role-Coupled Cleanup

## Sprint Overview

**Sprint ID:** 26  
**Objective:** Authorization Intent & Role-Coupled Cleanup (Option 1)  
**Classification:** Security behavior / behavior-affecting  
**Branch:** feature/sprint-26-discovery  
**Baseline Commit:** 3a81e69 (post-Sprint 25 documentation synchronization)  
**Implementation Commit:** f713ed6 (Sprint 26 Option 1 complete)  
**Date:** 2026-10-09  
**Status:** COMPLETE

## Baseline Metrics (Pre-Sprint)

| Metric | Value |
|--------|-------|
| Provider-Neutral Tests | 558 passing (355 Unit, 77 Web, 126 Integration) |
| BrowserTests | 4 passing (J1-J4) |
| Total Verified Tests | 562 passing across two tiers |
| Normal Release Build | 0 warnings / 0 errors |
| Forced Non-Incremental Release Build | 0 warnings / 0 errors |
| EF Pending Model Changes | None |
| Migrations | 10 (unchanged) |
| Capabilities Seeded | 41 |
| Authorization Groups | 3 (Administrator, InventoryManager, Viewer) |
| Graphify | 12,760 nodes / 19,018 edges / 949 communities |

## Scope (Frozen per Human Confirmations)

### BF-Q-001 Decision: Viewer Purchase Order Capability Intent
**Decision:** Restrict Viewer role to read-only Purchase Order access; restrict creation, editing, and submission to InventoryManager and Admin roles.

**Implementation Completed:**
- Modified `AuthorizationSeeder.cs` `CapabilityCatalog.Viewer` filter to exclude PurchaseOrder.Create, Edit, Submit, Approve, Receive, Cancel
- Viewer retains only `PurchaseOrder.View` capability
- Updated affected authorization tests in `AuthorizationSeederTests.cs`

### BF-Q-002 Decision: EditStatus Role Guard Fate
**Decision:** Refactor and align the legacy `EditStatus` role guard with standard policy-based authorization handlers tied to InventoryManager.

**Implementation Completed:**
- Removed `User.IsInRole(IdentityConstants.Roles.InventoryManager)` check from `EditStatus.cshtml.cs:61`
- Replaced with capability-based authorization using `User.EditStatus` capability
- Self-deactivation protection now applies to all users with `User.EditStatus` capability (not just InventoryManager role)

## Non-Goals (Confirmed Unchanged)
- No capability model rewrite beyond Viewer PO scope narrowing
- No migration (seed data change only)
- No major rollout or feature additions
- No changes to Administrator or InventoryManager capability sets (except test expectations)

## Actual Metrics (Post-Sprint)

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| Provider-Neutral Tests | 558 passing | 558 passing | ✅ |
| BrowserTests | 4 passing | 4 passing | ✅ |
| Normal Release Build | 0 warnings / 0 errors | 0 warnings / 0 errors | ✅ |
| Forced Non-Incremental Release Build | 0 warnings / 0 errors | 0 warnings / 0 errors | ✅ |
| Capabilities Seeded | 41 (unchanged) | 41 | ✅ |
| Authorization Groups | 3 (unchanged) | 3 | ✅ |
| Viewer Capability Count | 9 (was 15) | 9 | ✅ |
| Authorization Group-Capability Relationships | 73 (was 79) | 73 | ✅ |

## Files Modified

1. `src/InventoryPlatform/InventoryPlatform.Infrastructure/Identity/AuthorizationSeeder.cs` — Line 193-194: Viewer filter narrowed from `StartsWith("PurchaseOrder.")` to exact match `"PurchaseOrder.View"`
2. `tests/InventoryPlatform.IntegrationTests/Authorization/AuthorizationSeederTests.cs` — Updated 6 test expectations (Viewer count 15→9, relationships 79→73, PO capability assertions)
3. `src/InventoryPlatform/InventoryPlatform.Web/Pages/Administrator/Users/EditStatus.cshtml.cs` — Removed role guard, injected `ICapabilityAuthorizationService`, added `User.EditStatus` capability check for self-deactivation protection

## Risk Assessment
- **Risk Level:** Medium (behavior-affecting security changes)
- **Release Classification:** Security behavior / behavior-affecting
- **Rollback Plan:** Git revert on feature branch; no schema/migration changes

## Gate Decision Matrix

| Decision Required | Authority | Status |
|---|---|---|
| BF-Q-001: Viewer PO intent | Product Owner | **CONFIRMED + IMPLEMENTED** — Restrict to View-only |
| BF-Q-002: EditStatus guard fate | Product Owner | **CONFIRMED + IMPLEMENTED** — Refactor to capability-based |
| Sprint 26 primary scope selection | Collaboration Agent / PO | **SELECTED + COMPLETE** — Option 1 |

## Discovery Gate Verdict (Historical)

**VERDICT: `READY_FOR_SPRINT_26_PLANNING`** (achieved 2026-10-08)

### Conditions Verified:
1. ✅ Baseline verified: 558 provider-neutral + 4 browser tests passing (562 total)
2. ✅ Repository clean: `feature/sprint-26-discovery` branch, no uncommitted changes
3. ✅ ARPF operational state initialized: SYSTEM_STATE, CURRENT_WORK, HANDOFF current
4. ✅ Human confirmations received: BF-Q-001 and BF-Q-002 resolved
5. ✅ Frozen target requirements established in CURRENT_WORK.md
6. ✅ Retrospective initialized with scope and target metrics

## Implementation Gate Verdict (Final)

**VERDICT: `SPRINT_26_IMPLEMENTATION_COMPLETE`** (achieved 2026-10-09)

### Conditions Verified:
1. ✅ All 562 tests passing (558 provider-neutral + 4 browser)
2. ✅ Normal Release build: 0 warnings / 0 errors
3. ✅ Forced non-incremental Release build: 0 warnings / 0 errors
4. ✅ EF pending model changes: None
5. ✅ All frozen requirements implemented per SPRINT_26_DISCOVERY_GATE.md
6. ✅ No scope creep — only 3 files modified as specified
7. ✅ Retrospective finalized with actual metrics matching targets

## Lessons Learned

1. **Capability-based authorization is more granular than role-based** — The `User.EditStatus` capability provides finer-grained control than the `InventoryManager` role, applying self-deactivation protection consistently regardless of role composition.

2. **Test expectations must be updated in lockstep** — The AuthorizationSeederTests required 6 distinct test updates to reflect the new Viewer capability count and relationship count.

3. **Role guards can coexist with capability model but create inconsistency** — The legacy `IsInRole(InventoryManager)` check was reachable for multi-role users and provided inconsistent protection (plain Administrators could self-deactivate). The capability-based replacement is consistent.

4. **Documented-brownfield adoption enables confident behavior changes** — The 562-test baseline (verified pre-implementation) provided strong regression coverage for security behavior changes.

## Next Steps

- Sprint 27 Planning: Await Product Owner scope selection from remaining Options 3, 4, 5
- BF-Q-003 (historical 39 vs 41 capability docs) remains deferred as low-impact documentation-only item

---

*Implementation executed by Development Agent per ARPF documented-brownfield adoption model. All findings classified per evidence rules (OBSERVED/INFERRED/UNKNOWN/CONFLICT). Human confirmations BF-Q-001 and BF-Q-002 explicitly resolved, implemented, and verified.*