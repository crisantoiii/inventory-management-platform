# Sprint 26 Retrospective: Authorization Intent & Role-Coupled Cleanup

## Sprint Overview

**Sprint ID:** 26  
**Objective:** Authorization Intent & Role-Coupled Cleanup (Option 1)  
**Classification:** Security behavior / behavior-affecting  
**Branch:** feature/sprint-26-discovery  
**Baseline Commit:** 3a81e69 (post-Sprint 25 documentation synchronization)  
**Date:** 2026-10-08  
**Status:** Planning — Discovery Gate Complete

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

**Implementation Required:**
- Modify `AuthorizationSeeder.cs` `CapabilityCatalog.Viewer` filter to exclude PurchaseOrder.Create, Edit, Submit, Approve, Receive, Cancel
- Viewer retains only `PurchaseOrder.View` capability
- Update affected authorization tests in `AuthorizationSeederTests.cs`

### BF-Q-002 Decision: EditStatus Role Guard Fate
**Decision:** Refactor and align the legacy `EditStatus` role guard with standard policy-based authorization handlers tied to InventoryManager.

**Implementation Required:**
- Remove `User.IsInRole(IdentityConstants.Roles.InventoryManager)` check from `EditStatus.cshtml.cs:61`
- Replace with capability-based authorization using `AuthorizationPolicies.InventoryManagement` policy or a new capability requirement
- Ensure self-deactivation protection applies to all users with appropriate capabilities (not just InventoryManager role)
- Update affected Web tests if any

## Non-Goals
- No capability model rewrite beyond Viewer PO scope narrowing
- No migration (seed data change only)
- No major rollout or feature additions
- No changes to Administrator or InventoryManager capability sets (except test expectations)

## Target Metrics (Post-Sprint)

| Metric | Target |
|--------|--------|
| Provider-Neutral Tests | 558 passing (test expectations updated for Viewer PO scope change) |
| BrowserTests | 4 passing (unchanged) |
| Normal Release Build | 0 warnings / 0 errors |
| Forced Non-Incremental Release Build | 0 warnings / 0 errors |
| Capabilities Seeded | 41 (unchanged) |
| Authorization Groups | 3 (unchanged) |
| Viewer Capability Count | 9 (was 15) — 7 View + PurchaseOrder.View + User.View |
| Authorization Group-Capability Relationships | 73 (was 79) — 6 Viewer PO capabilities removed |

## Risk Assessment
- **Risk Level:** Medium (behavior-affecting security changes)
- **Release Classification:** Security behavior / behavior-affecting
- **Rollback Plan:** Git revert on feature branch; no schema/migration changes

## Gate Decision Matrix

| Decision Required | Authority | Status |
|---|---|---|
| BF-Q-001: Viewer PO intent | Product Owner | **CONFIRMED** — Restrict to View-only |
| BF-Q-002: EditStatus guard fate | Product Owner | **CONFIRMED** — Refactor to capability-based |
| Sprint 26 primary scope selection | Collaboration Agent / PO | **SELECTED** — Option 1 |

## Discovery Gate Verdict

**VERDICT: `READY_FOR_SPRINT_26_PLANNING`**

### Conditions:
1. ✅ Baseline verified: 558 provider-neutral + 4 browser tests passing (562 total)
2. ✅ Repository clean: `feature/sprint-26-discovery` branch, no uncommitted changes
3. ✅ ARPF operational state initialized: SYSTEM_STATE, CURRENT_WORK, HANDOFF current
4. ✅ Human confirmations received: BF-Q-001 and BF-Q-002 resolved
5. ✅ Frozen target requirements established in CURRENT_WORK.md
6. ✅ Retrospective initialized with scope and target metrics

### Next Action:
**Collaboration Agent authorizes Sprint 26 planning prompt materialization for Development Agent implementation.**

---

*Discovery Gate executed by Collaboration Agent per ARPF documented-brownfield adoption model. No production code modified. All findings classified per evidence rules (OBSERVED/INFERRED/UNKNOWN/CONFLICT).*