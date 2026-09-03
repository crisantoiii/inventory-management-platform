using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using Xunit;

namespace InventoryPlatform.UnitTests.Domain.Purchasing;

public class PurchaseOrderTests
{
    private static PurchaseOrder CreateDraftOrder(
        int supplierId = 1,
        DateOnly? orderDate = null,
        DateOnly? expectedDeliveryDate = null,
        string? remarks = null)
    {
        return PurchaseOrder.Create(
            supplierId,
            orderDate ?? new DateOnly(2026, 1, 1),
            expectedDeliveryDate,
            remarks);
    }

    private static void AddValidItem(
        PurchaseOrder order,
        int productId = 10,
        decimal quantity = 5m,
        decimal unitCost = 25.00m)
    {
        order.AddItem(productId, quantity, unitCost);
    }

    // =====================================================
    // A. Creation
    // =====================================================

    [Fact]
    public void Create_WithValidParameters_SetsStatusToDraft()
    {
        var order = CreateDraftOrder();

        Assert.Equal(PurchaseOrderStatus.Draft, order.Status);
    }

    [Fact]
    public void Create_WithValidParameters_SetsSupplierId()
    {
        var order = CreateDraftOrder(supplierId: 42);

        Assert.Equal(42, order.SupplierId);
    }

    [Fact]
    public void Create_WithValidParameters_SetsOrderDate()
    {
        var date = new DateOnly(2026, 3, 15);
        var order = CreateDraftOrder(orderDate: date);

        Assert.Equal(date, order.OrderDate);
    }

    [Fact]
    public void Create_WithExpectedDeliveryDate_SetsExpectedDeliveryDate()
    {
        var delivery = new DateOnly(2026, 4, 1);
        var order = CreateDraftOrder(expectedDeliveryDate: delivery);

        Assert.Equal(delivery, order.ExpectedDeliveryDate);
    }

    [Fact]
    public void Create_WithNullExpectedDeliveryDate_SetsNull()
    {
        var order = CreateDraftOrder(expectedDeliveryDate: null);

        Assert.Null(order.ExpectedDeliveryDate);
    }

    [Fact]
    public void Create_WithRemarks_SetsRemarks()
    {
        var order = CreateDraftOrder(remarks: "Urgent delivery");

        Assert.Equal("Urgent delivery", order.Remarks);
    }

    [Fact]
    public void Create_WithNullRemarks_SetsNullRemarks()
    {
        var order = CreateDraftOrder(remarks: null);

        Assert.Null(order.Remarks);
    }

    [Fact]
    public void Create_InitialItemsIsEmpty()
    {
        var order = CreateDraftOrder();

        Assert.Empty(order.Items);
    }

    [Fact]
    public void Create_InitialTotalAmountIsZero()
    {
        var order = CreateDraftOrder();

        Assert.Equal(0m, order.TotalAmount);
    }

    // =====================================================
    // B. Adding Items
    // =====================================================

    [Fact]
    public void AddItem_WithValidItem_AddsItemToPurchaseOrder()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        Assert.Single(order.Items);
    }

    [Fact]
    public void AddItem_WithValidItem_SetsCorrectProductId()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        Assert.Equal(10, order.Items.First().ProductId);
    }

    [Fact]
    public void AddItem_WithValidItem_SetsCorrectQuantity()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        Assert.Equal(5m, order.Items.First().Quantity);
    }

    [Fact]
    public void AddItem_WithValidItem_SetsCorrectUnitCost()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        Assert.Equal(25.00m, order.Items.First().UnitCost);
    }

    [Fact]
    public void AddItem_WithValidItem_SetsReceivedQuantityToZero()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        Assert.Equal(0m, order.Items.First().ReceivedQuantity);
    }

    [Fact]
    public void AddItem_MultipleItems_AllAdded()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m);
        order.AddItem(productId: 30, quantity: 1m, unitCost: 100.00m);

        Assert.Equal(3, order.Items.Count);
    }

    [Fact]
    public void AddItem_MultipleItems_CorrectProductIds()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m);

        var productIds = order.Items.Select(x => x.ProductId).ToList();

        Assert.Equal(new[] { 10, 20 }, productIds);
    }

    [Fact]
    public void AddItem_UpdatesTotalAmount()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        // 5 * 25 = 125
        Assert.Equal(125.00m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_MultipleItems_SumsTotalAmount()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);  // 125
        order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m);  // 30

        Assert.Equal(155.00m, order.TotalAmount);
    }

    [Fact]
    public void AddItem_DuplicateProduct_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            order.AddItem(productId: 10, quantity: 2m, unitCost: 30.00m));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void AddItem_ZeroQuantity_ThrowsDomainException()
    {
        var order = CreateDraftOrder();

        var ex = Assert.Throws<DomainException>(() =>
            order.AddItem(productId: 10, quantity: 0m, unitCost: 25.00m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void AddItem_NegativeQuantity_ThrowsDomainException()
    {
        var order = CreateDraftOrder();

        var ex = Assert.Throws<DomainException>(() =>
            order.AddItem(productId: 10, quantity: -1m, unitCost: 25.00m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void AddItem_NegativeUnitCost_ThrowsDomainException()
    {
        var order = CreateDraftOrder();

        var ex = Assert.Throws<DomainException>(() =>
            order.AddItem(productId: 10, quantity: 5m, unitCost: -1m));

        Assert.Contains("cannot be negative", ex.Message);
    }

    [Fact]
    public void AddItem_ZeroUnitCost_IsAllowed()
    {
        var order = CreateDraftOrder();

        order.AddItem(productId: 10, quantity: 5m, unitCost: 0m);

        Assert.Single(order.Items);
        Assert.Equal(0m, order.Items.First().UnitCost);
    }

    // =====================================================
    // C. Updating Items
    // =====================================================

    [Fact]
    public void UpdateItem_ExistingItem_UpdatesQuantity()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        order.UpdateItem(productId: 10, quantity: 10m, unitCost: 25.00m);

        Assert.Equal(10m, order.Items.First().Quantity);
    }

    [Fact]
    public void UpdateItem_ExistingItem_UpdatesUnitCost()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        order.UpdateItem(productId: 10, quantity: 5m, unitCost: 30.00m);

        Assert.Equal(30.00m, order.Items.First().UnitCost);
    }

    [Fact]
    public void UpdateItem_ExistingItem_UpdatesTotalAmount()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m); // 125

        order.UpdateItem(productId: 10, quantity: 10m, unitCost: 30.00m); // 300

        Assert.Equal(300.00m, order.TotalAmount);
    }

    [Fact]
    public void UpdateItem_NonexistentItem_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            order.UpdateItem(productId: 99, quantity: 5m, unitCost: 25.00m));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void UpdateItem_ZeroQuantity_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            order.UpdateItem(productId: 10, quantity: 0m, unitCost: 25.00m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void UpdateItem_NegativeUnitCost_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            order.UpdateItem(productId: 10, quantity: 5m, unitCost: -1m));

        Assert.Contains("cannot be negative", ex.Message);
    }

    // =====================================================
    // D. Removing Items
    // =====================================================

    [Fact]
    public void RemoveItem_ExistingItem_RemovesFromOrder()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m);

        order.RemoveItem(productId: 10);

        Assert.Single(order.Items);
        Assert.Equal(20, order.Items.First().ProductId);
    }

    [Fact]
    public void RemoveItem_UpdatesTotalAmount()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m); // 125
        order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m); // 30

        order.RemoveItem(productId: 10);

        Assert.Equal(30.00m, order.TotalAmount);
    }

    [Fact]
    public void RemoveItem_LastItem_TotalAmountBecomesZero()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        order.RemoveItem(productId: 10);

        Assert.Empty(order.Items);
        Assert.Equal(0m, order.TotalAmount);
    }

    [Fact]
    public void RemoveItem_NonexistentItem_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            order.RemoveItem(productId: 99));

        Assert.Contains("not found", ex.Message);
    }

    // =====================================================
    // E. Submit
    // =====================================================

    [Fact]
    public void Submit_WhenDraftWithItems_ChangesStatusToSubmitted()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        order.Submit();

        Assert.Equal(PurchaseOrderStatus.Submitted, order.Status);
    }

    [Fact]
    public void Submit_WhenDraftWithNoItems_ThrowsDomainException()
    {
        var order = CreateDraftOrder();

        var ex = Assert.Throws<DomainException>(() => order.Submit());

        Assert.Contains("at least one item", ex.Message);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();

        var ex = Assert.Throws<DomainException>(() => order.Submit());

        Assert.Contains("Only draft", ex.Message);
    }

    [Fact]
    public void Submit_WhenApproved_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        var ex = Assert.Throws<DomainException>(() => order.Submit());

        Assert.Contains("Only draft", ex.Message);
    }

    // =====================================================
    // F. Approve
    // =====================================================

    [Fact]
    public void Approve_WhenSubmitted_ChangesStatusToApproved()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();

        order.Approve();

        Assert.Equal(PurchaseOrderStatus.Approved, order.Status);
    }

    [Fact]
    public void Approve_WhenDraft_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() => order.Approve());

        Assert.Contains("Only submitted", ex.Message);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        var ex = Assert.Throws<DomainException>(() => order.Approve());

        Assert.Contains("Only submitted", ex.Message);
    }

    [Fact]
    public void Approve_WhenReceiving_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();
        order.Receive(productId: 10, quantity: 5m); // Receiving

        var ex = Assert.Throws<DomainException>(() => order.Approve());

        Assert.Contains("Only submitted", ex.Message);
    }

    // =====================================================
    // G. Receive
    // =====================================================

    [Fact]
    public void Receive_WhenApproved_UpdatesReceivedQuantity()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 3m);

        Assert.Equal(3m, order.Items.First().ReceivedQuantity);
    }

    [Fact]
    public void Receive_WhenApproved_ChangesStatusToReceiving()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 3m);

        Assert.Equal(PurchaseOrderStatus.Receiving, order.Status);
    }

    [Fact]
    public void Receive_WhenDraft_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            order.Receive(productId: 10, quantity: 5m));

        Assert.Contains("Only approved", ex.Message);
    }

    [Fact]
    public void Receive_WhenSubmitted_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();

        var ex = Assert.Throws<DomainException>(() =>
            order.Receive(productId: 10, quantity: 5m));

        Assert.Contains("Only approved", ex.Message);
    }

    [Fact]
    public void Receive_NonexistentProduct_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        var ex = Assert.Throws<DomainException>(() =>
            order.Receive(productId: 99, quantity: 5m));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void Receive_ZeroQuantity_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        var ex = Assert.Throws<DomainException>(() =>
            order.Receive(productId: 10, quantity: 0m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void Receive_NegativeQuantity_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        var ex = Assert.Throws<DomainException>(() =>
            order.Receive(productId: 10, quantity: -1m));

        Assert.Contains("greater than zero", ex.Message);
    }

    // =====================================================
    // H. Partial Receiving
    // =====================================================

    [Fact]
    public void Receive_PartialQuantity_UpdatesReceivedQuantity()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 4m);

        Assert.Equal(4m, order.Items.First().ReceivedQuantity);
    }

    [Fact]
    public void Receive_PartialQuantity_StatusIsReceiving()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 4m);

        Assert.Equal(PurchaseOrderStatus.Receiving, order.Status);
    }

    [Fact]
    public void Receive_PartialQuantity_RemainingQuantityIsCorrect()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 4m);

        Assert.Equal(6m, order.Items.First().RemainingQuantity);
    }

    [Fact]
    public void Receive_PartialQuantity_ItemIsNotFullyReceived()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 4m);

        Assert.False(order.Items.First().IsFullyReceived);
    }

    [Fact]
    public void Receive_MultiplePartialReceives_AccumulatesReceivedQuantity()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 3m);
        order.Receive(productId: 10, quantity: 4m);

        Assert.Equal(7m, order.Items.First().ReceivedQuantity);
    }

    [Fact]
    public void Receive_FromReceivingStatus_IsAllowed()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();
        order.Receive(productId: 10, quantity: 3m); // Now Receiving

        order.Receive(productId: 10, quantity: 2m);

        Assert.Equal(5m, order.Items.First().ReceivedQuantity);
    }

    [Fact]
    public void Receive_ExceedingOrderedQuantity_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        var ex = Assert.Throws<DomainException>(() =>
            order.Receive(productId: 10, quantity: 6m));

        Assert.Contains("cannot exceed", ex.Message);
    }

    [Fact]
    public void Receive_AccumulatedExceedingOrderedQuantity_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();
        order.Approve();
        order.Receive(productId: 10, quantity: 3m);

        var ex = Assert.Throws<DomainException>(() =>
            order.Receive(productId: 10, quantity: 3m)); // 3+3=6 > 5

        Assert.Contains("cannot exceed", ex.Message);
    }

    // =====================================================
    // I. Full Receiving / Completion
    // =====================================================

    [Fact]
    public void Receive_AllItemsFullyReceived_ChangesStatusToCompleted()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 10m);

        Assert.Equal(PurchaseOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Receive_AllItemsFullyReceived_ItemIsFullyReceived()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 10m);

        Assert.True(order.Items.First().IsFullyReceived);
    }

    [Fact]
    public void Receive_AllItemsFullyReceived_RemainingQuantityIsZero()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 10m);

        Assert.Equal(0m, order.Items.First().RemainingQuantity);
    }

    [Fact]
    public void Receive_MultipleItems_AllFullyReceived_CompletesOrder()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 5m);
        order.Receive(productId: 20, quantity: 3m);

        Assert.Equal(PurchaseOrderStatus.Completed, order.Status);
    }

    [Fact]
    public void Receive_MultipleItems_PartiallyReceived_RemainsReceiving()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 5m); // Item 10 complete
        // Item 20 not yet received

        Assert.Equal(PurchaseOrderStatus.Receiving, order.Status);
    }

    [Fact]
    public void Receive_ExactQuantity_CompletesOrder()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 7m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        order.Receive(productId: 10, quantity: 7m);

        Assert.Equal(PurchaseOrderStatus.Completed, order.Status);
        Assert.Equal(0m, order.Items.First().RemainingQuantity);
    }

    // =====================================================
    // J. Invalid State Transitions (additional)
    // =====================================================

    [Fact]
    public void AddItem_WhenSubmitted_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();

        var ex = Assert.Throws<DomainException>(() =>
            order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m));

        Assert.Contains("Only draft", ex.Message);
    }

    [Fact]
    public void UpdateItem_WhenSubmitted_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();

        var ex = Assert.Throws<DomainException>(() =>
            order.UpdateItem(productId: 10, quantity: 10m, unitCost: 25.00m));

        Assert.Contains("Only draft", ex.Message);
    }

    [Fact]
    public void RemoveItem_WhenSubmitted_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();

        var ex = Assert.Throws<DomainException>(() =>
            order.RemoveItem(productId: 10));

        Assert.Contains("Only draft", ex.Message);
    }

    [Fact]
    public void AddItem_WhenApproved_ThrowsDomainException()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();
        order.Approve();

        var ex = Assert.Throws<DomainException>(() =>
            order.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m));

        Assert.Contains("Only draft", ex.Message);
    }

    // =====================================================
    // K. Domain Invariants
    // =====================================================

    [Fact]
    public void TotalAmount_ReflectsLineTotals()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);   // 125
        order.AddItem(productId: 20, quantity: 10m, unitCost: 12.50m);  // 125

        Assert.Equal(250.00m, order.TotalAmount);
    }

    [Fact]
    public void Items_IsReadOnly()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);

        // Items returns IReadOnlyCollection, verify it's not mutable via the public API
        var items = order.Items;
        Assert.IsAssignableFrom<IReadOnlyCollection<PurchaseOrderItem>>(items);
    }

    [Fact]
    public void Item_LineTotal_IsCalculatedCorrectly()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 7m, unitCost: 15.00m);

        Assert.Equal(105.00m, order.Items.First().LineTotal);
    }

    [Fact]
    public void Item_RemainingQuantity_IsCalculatedCorrectly()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 10m, unitCost: 25.00m);
        order.Submit();
        order.Approve();
        order.Receive(productId: 10, quantity: 3m);

        Assert.Equal(7m, order.Items.First().RemainingQuantity);
    }

    [Fact]
    public void Item_IsFullyReceived_WhenRemainingIsZero()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();
        order.Approve();
        order.Receive(productId: 10, quantity: 5m);

        Assert.True(order.Items.First().IsFullyReceived);
    }

    [Fact]
    public void Item_IsNotFullyReceived_WhenRemainingIsPositive()
    {
        var order = CreateDraftOrder();
        order.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        order.Submit();
        order.Approve();
        order.Receive(productId: 10, quantity: 2m);

        Assert.False(order.Items.First().IsFullyReceived);
    }
}
