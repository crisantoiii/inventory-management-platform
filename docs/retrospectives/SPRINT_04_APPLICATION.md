# Sprint Summary
## Sprint 4 - Purchasing Presentation Layer

### Sprint Goal

Implement the Purchasing Presentation layer and connect the existing Purchasing Application use cases to a usable Razor Pages interface while preserving the Rich Domain Model and Clean Architecture established in previous sprints.

The sprint focused on delivering and validating a complete Purchase Order workflow through the browser and actual database persistence.

---

## Completed

### Presentation Features

Implemented Razor Pages for:

- Purchase Order Index
- Create Purchase Order
- Purchase Order Details

The Presentation layer now exposes the existing Purchasing Application use cases through a browser-accessible workflow.

---

### Purchase Order Workflow

Implemented and verified the complete Purchase Order lifecycle:

```text
Draft
  ↓ Submit
Submitted
  ↓ Approve
Approved
  ↓ Receive partial quantity
Receiving
  ↓ Receive remaining quantity
Completed
```

Implemented workflow actions:

- Submit Purchase Order
- Approve Purchase Order
- Receive Purchase Order

Receiving supports partial quantities before the Purchase Order reaches `Completed`.

---

### Purchase Order Index

Implemented the Purchase Order list page displaying:

- Purchase Order ID
- Supplier
- Order Date
- Status
- Total Amount
- Details action

The Purchase Order repository list query was updated to load the data required by the aggregate's calculated `TotalAmount`.

This resolved an issue where the Purchase Order Details page displayed the correct total while the Index page displayed `0.00`.

---

### Purchase Order Creation

Implemented the Create Purchase Order page with:

- Supplier selection
- Expected Delivery Date
- Remarks
- Product selection
- Quantity
- Unit Cost
- Validation summary

The Web layer uses a Presentation-specific input model and maps it to the Application `CreatePurchaseOrderRequest`.

---

### Purchase Order Details

Implemented the Details page displaying:

- Purchase Order information
- Supplier
- Order Date
- Expected Delivery Date
- Status
- Total Amount
- Purchase Order items
- Ordered quantity
- Received quantity
- Remaining quantity
- Line totals

Workflow actions are displayed according to the current Purchase Order status.

---

### Submit and Approve Workflow

Implemented Purchase Order workflow actions on the Details page:

- Submit
- Approve

The actions delegate state transitions to the existing Application handlers and Domain aggregate methods.

The verified transitions are:

```text
Draft
  ↓ Submit
Submitted
  ↓ Approve
Approved
```

---

### Receiving

Implemented item-level receiving through the Details page.

The receiving interface supports:

- Partial receiving
- Remaining quantity calculation
- Fully received indication
- Receive quantity validation
- Transition from `Approved` to `Receiving`
- Transition from `Receiving` to `Completed`

Example workflow:

```text
Ordered:   10
Received:   0
Remaining: 10

Receive 5
    ↓

Ordered:   10
Received:   5
Remaining:  5
Status: Receiving

Receive 5
    ↓

Ordered:   10
Received:  10
Remaining:  0
Status: Completed
```

---

## Application Integration

The Presentation layer integrates with the existing Purchasing Application handlers:

- `GetPurchaseOrdersHandler`
- `CreatePurchaseOrderHandler`
- `GetPurchaseOrderHandler`
- `SubmitPurchaseOrderHandler`
- `ApprovePurchaseOrderHandler`
- `ReceivePurchaseOrderHandler`

The Web layer does not access repositories or `DbContext` directly.

The resulting flow remains:

```text
Presentation
     ↓
Application
     ↓
Domain
     ↓
Infrastructure
     ↓
Database
```

---

## Dependency Injection

Registered the Purchasing Application handlers required by the Presentation layer:

- `CreatePurchaseOrderHandler`
- `GetPurchaseOrderHandler`
- `GetPurchaseOrdersHandler`
- `SubmitPurchaseOrderHandler`
- `ApprovePurchaseOrderHandler`
- `ReceivePurchaseOrderHandler`

This allows Razor PageModels to depend on Application use cases through constructor injection.

---

## Repository Integration

Enhanced the Purchase Order repository list query to support the Index summary.

The query loads:

- Supplier
- Purchase Order Items

This allows the existing Domain-calculated `TotalAmount` to produce the correct value when Purchase Orders are displayed in the Index.

The Domain continues to calculate the total from Purchase Order items rather than introducing duplicated persisted total state.

---

## Presentation Validation and Feedback

Added user-facing validation and feedback for the Purchasing workflow.

Implemented:

- Model validation
- Receive quantity constraints
- Success messages through `TempData`
- Validation summaries
- Index query failure messages
- Supplier query failure messages
- Product query failure messages

Client-side validation prevents obvious invalid input before submission, while Domain validation continues to enforce business invariants.

---

## End-to-End Verification

The Purchasing workflow was tested using actual Purchase Orders persisted to the database.

### Successful Workflow

Verified the complete lifecycle:

```text
Create Purchase Order
        ↓
Draft
        ↓
Submit
        ↓
Submitted
        ↓
Approve
        ↓
Approved
        ↓
Receive partial quantity
        ↓
Receiving
        ↓
Receive remaining quantity
        ↓
Completed
```

### Verified Results

- Purchase Orders persisted successfully.
- Index displayed persisted Purchase Orders.
- Details displayed Purchase Order information and items.
- Calculated total displayed correctly.
- Submit changed `Draft` to `Submitted`.
- Approve changed `Submitted` to `Approved`.
- Partial receiving changed `Approved` to `Receiving`.
- Final receiving changed `Receiving` to `Completed`.
- Received quantity reached the ordered quantity.
- Remaining quantity reached zero.
- Fully Received state was displayed.

---

## Validation Review

Verified receiving business rules through both client-side and Domain validation.

### Client-Side Validation

The Receive input prevents obvious invalid values such as zero through the HTML minimum constraint.

### Domain Validation

The Domain independently rejects invalid receiving quantities.

For example:

```text
Quantity <= 0
    ↓
DomainException
    ↓
"Received quantity must be greater than zero."
```

This confirms that business rules remain protected at the Domain layer even when client-side validation is bypassed.

---

## Architecture Review

Sprint 4 confirmed that the Presentation layer can consume the existing Purchasing Application architecture without requiring structural redesign.

The review confirmed:

- Razor Pages depend on Application handlers.
- Application handlers coordinate use cases.
- Domain entities enforce business rules.
- Persistence remains behind repository abstractions.
- The Presentation layer does not directly access the database.
- Workflow actions remain business-oriented rather than generic CRUD operations.

No architecture violations were identified during the Sprint 4 review.

---

## What Went Well

- Existing Sprint 3 Application layer integrated cleanly with Razor Pages.
- Complete Purchasing workflow was delivered without architectural redesign.
- Real database records were used for end-to-end verification.
- The Rich Domain Model continued to own Purchase Order workflow rules.
- Partial receiving was implemented without moving business logic into the Presentation layer.
- A repository query issue affecting calculated totals was identified and corrected during integration testing.
- UI validation and Domain validation provide complementary protection.
- Success and failure feedback was improved in the Presentation layer.
- Sprint implementation and documentation were kept as separate commits.

---

## Technical Decisions

### Presentation Uses Application Handlers

Razor PageModels depend on Application handlers rather than repositories or `DbContext`.

This preserves the established architecture:

```text
Presentation
     ↓
Application
     ↓
Domain
     ↓
Infrastructure
```

---

### Details Page as Workflow Screen

Submit, Approve, and Receive actions were implemented on the Purchase Order Details page rather than creating separate Razor Pages for every workflow action.

This keeps the Purchase Order workflow centralized around the aggregate's Details view.

---

### Item-Level Receiving

Receiving was implemented at the Purchase Order Item level because the Application contract requires:

- Purchase Order ID
- Product ID
- Quantity

This supports partial receiving and uses the Domain's `ReceivedQuantity` and `RemainingQuantity` behavior.

---

### Domain Remains the Business Rule Authority

The Presentation layer provides user-friendly validation but does not implement Purchase Order state transitions.

State changes remain delegated to:

- `PurchaseOrder.Submit()`
- `PurchaseOrder.Approve()`
- `PurchaseOrder.Receive()`

This preserves the Rich Domain Model established in previous sprints.

---

### Calculated Purchase Order Total

The Purchase Order total remains calculated from its items rather than being stored as duplicated state.

The repository query loads the Purchase Order items required for the aggregate to calculate the total when producing the Index summary.

---

## Technical Debt

The following improvements were intentionally deferred:

### Cross-Cutting Domain Exception Handling

Domain exceptions can currently propagate from Application handlers when client-side validation is bypassed.

A consistent project-wide strategy for converting Domain exceptions into the application's Result/error-handling mechanism should be evaluated rather than implementing a Purchasing-specific workaround.

---

### Inventory Update During Receiving

The current Receive workflow updates:

- Purchase Order Item received quantity
- Purchase Order status

It does not currently update Product inventory.

Inventory synchronization should be addressed only after the required business behavior and architectural boundary are explicitly defined.

---

### Multiple Purchase Order Items in Create UI

The Application contract supports multiple Purchase Order items, while the Presentation implementation at the time of Sprint 4 used a single item row.

Dynamic item management was subsequently implemented in Sprint 8 P1 - Multiple Purchase Order Item Management. The Sprint 4 limitation is retained here as historical context and is no longer a current-state limitation.

---

### Purchase Order Search and Pagination

The current Index page displays the available Purchase Orders without advanced search, filtering, sorting, or pagination.

These can be added as future usability and scalability improvements.

---

### Product Selection Improvements

The Product selection UI can eventually display additional identifying information such as SKU alongside the Product name.

---

## Lessons Learned

- A working Application layer can be integrated into a usable Presentation layer without moving business logic into Razor Pages.
- End-to-end browser testing exposed a repository query issue that compilation alone could not identify.
- Calculated Domain properties require persistence queries to load the data needed by the aggregate.
- Client-side validation improves user experience, but Domain validation remains necessary for business-rule protection.
- Workflow-driven interfaces are better modeled around business actions than generic CRUD operations.
- Partial receiving requires item-level interaction rather than a single Purchase Order-level action.
- Separating implementation commits from documentation commits provides a clearer project history.
- A complete vertical slice provides a stronger validation point than implementing isolated pages without workflow verification.

---

## Next Sprint

Future work may include:

- Additional Presentation-layer features
- Inventory integration
- Cross-cutting exception handling
- Multiple Purchase Order item management
- Search, filtering, sorting, and pagination
- Additional workflow validation
- Automated tests

---

## Sprint Metrics

| Metric | Count |
|---|---:|
| Presentation Pages | 3 |
| Workflow Actions | 3 |
| Purchase Order States Verified | 5 |
| End-to-End Workflows Tested | 2 |
| Partial Receiving Tests | 1 |
| Architecture Violations | 0 |
| Breaking Changes | 0 |

### Architecture Status

**PASS**