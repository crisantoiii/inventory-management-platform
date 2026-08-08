# Sprint Retrospective
## Sprint 1 - Purchasing Domain Model

### Sprint Goal

Design and implement the Purchasing domain model before introducing persistence or user interfaces.

---

## Completed

- PurchaseOrder aggregate
- PurchaseOrderItem entity
- Purchase order workflow
- Business rules
- Aggregate behavior

Implemented operations:

- Create
- AddItem
- UpdateItem
- RemoveItem
- Submit
- Approve
- Receive

Implemented workflow:

Draft
↓

Submitted
↓

Approved
↓

Receiving
↓

Completed

---

## What Went Well

- Business-first design reduced implementation complexity.
- Aggregate responsibilities remained well defined.
- PurchaseOrderItem owns line-level behavior.
- Workflow was modeled without introducing unnecessary complexity.

---

## Design Decisions

- PurchaseOrder is the aggregate root.
- PurchaseOrderItem owns receiving behavior.
- Completed is a derived state rather than a user action.
- Partial receiving is supported through ReceivedQuantity.

---

## Lessons Learned

- Model the business process before designing persistence.
- Build simple workflows that are extensible.
- Avoid implementing future requirements prematurely.

---

## Next Sprint

Sprint 2 focuses on persistence.

Planned work:

- Entity Framework Core configurations
- Repository implementation
- Database migration
- Persistence review

---

## Sprint Metrics

Duration
1 Sprint

Domain Entities
2

Business Operations
7

Workflow States
6

Files Added
2

Files Modified
2

Breaking Changes
None

Architecture Decisions
4