using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Exceptions;
using Xunit;

namespace InventoryPlatform.UnitTests.Domain.Purchasing;

public class PurchaseOrderItemTests
{
    // =====================================================
    // Creation
    // =====================================================

    [Fact]
    public void Create_WithValidParameters_SetsProductId()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        Assert.Equal(10, item.ProductId);
    }

    [Fact]
    public void Create_WithValidParameters_SetsPurchaseOrderId()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 42,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        Assert.Equal(42, item.PurchaseOrderId);
    }

    [Fact]
    public void Create_WithValidParameters_SetsQuantity()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 7m,
            unitCost: 25.00m);

        Assert.Equal(7m, item.Quantity);
    }

    [Fact]
    public void Create_WithValidParameters_SetsUnitCost()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 33.50m);

        Assert.Equal(33.50m, item.UnitCost);
    }

    [Fact]
    public void Create_InitialReceivedQuantityIsZero()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        Assert.Equal(0m, item.ReceivedQuantity);
    }

    // =====================================================
    // Computed Properties
    // =====================================================

    [Fact]
    public void LineTotal_MultipliesQuantityByUnitCost()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 7m,
            unitCost: 15.00m);

        Assert.Equal(105.00m, item.LineTotal);
    }

    [Fact]
    public void LineTotal_WithZeroQuantity_IsZero()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 0m,
            unitCost: 25.00m);

        // Note: Create doesn't validate quantity > 0; that validation is on PurchaseOrder.AddItem
        Assert.Equal(0m, item.LineTotal);
    }

    [Fact]
    public void RemainingQuantity_EqualsQuantityMinusReceivedQuantity()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        Assert.Equal(10m, item.RemainingQuantity);
    }

    [Fact]
    public void RemainingQuantity_AfterPartialReceive_IsCorrect()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        item.Receive(4m);

        Assert.Equal(6m, item.RemainingQuantity);
    }

    [Fact]
    public void RemainingQuantity_AfterFullReceive_IsZero()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        item.Receive(10m);

        Assert.Equal(0m, item.RemainingQuantity);
    }

    [Fact]
    public void IsFullyReceived_WhenRemainingIsZero_ReturnsTrue()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Receive(5m);

        Assert.True(item.IsFullyReceived);
    }

    [Fact]
    public void IsFullyReceived_WhenRemainingIsPositive_ReturnsFalse()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Receive(3m);

        Assert.False(item.IsFullyReceived);
    }

    [Fact]
    public void IsFullyReceived_WhenNoReceive_ReturnsFalse()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        Assert.False(item.IsFullyReceived);
    }

    // =====================================================
    // Receive
    // =====================================================

    [Fact]
    public void Receive_ValidQuantity_IncreasesReceivedQuantity()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        item.Receive(3m);

        Assert.Equal(3m, item.ReceivedQuantity);
    }

    [Fact]
    public void Receive_MultipleValidReceives_AccumulatesReceivedQuantity()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        item.Receive(3m);
        item.Receive(4m);

        Assert.Equal(7m, item.ReceivedQuantity);
    }

    [Fact]
    public void Receive_ExactOrderedQuantity_SetsReceivedQuantityToOrderedQuantity()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Receive(5m);

        Assert.Equal(5m, item.ReceivedQuantity);
    }

    [Fact]
    public void Receive_ZeroQuantity_ThrowsDomainException()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() => item.Receive(0m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void Receive_NegativeQuantity_ThrowsDomainException()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() => item.Receive(-1m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void Receive_ExceedingOrderedQuantity_ThrowsDomainException()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() => item.Receive(6m));

        Assert.Contains("cannot exceed", ex.Message);
    }

    [Fact]
    public void Receive_AccumulatedExceedingOrderedQuantity_ThrowsDomainException()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Receive(3m);

        var ex = Assert.Throws<DomainException>(() => item.Receive(3m)); // 3+3=6 > 5

        Assert.Contains("cannot exceed", ex.Message);
    }

    [Fact]
    public void Receive_DoesNotAffectLineTotal()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);

        item.Receive(5m);

        Assert.Equal(250.00m, item.LineTotal);
    }

    // =====================================================
    // Update
    // =====================================================

    [Fact]
    public void Update_WithValidQuantity_UpdatesQuantity()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Update(quantity: 10m, unitCost: 25.00m);

        Assert.Equal(10m, item.Quantity);
    }

    [Fact]
    public void Update_WithValidUnitCost_UpdatesUnitCost()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Update(quantity: 5m, unitCost: 30.00m);

        Assert.Equal(30.00m, item.UnitCost);
    }

    [Fact]
    public void Update_ReflectsOnLineTotal()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Update(quantity: 10m, unitCost: 30.00m);

        Assert.Equal(300.00m, item.LineTotal);
    }

    [Fact]
    public void Update_ReflectsOnRemainingQuantity()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Update(quantity: 10m, unitCost: 25.00m);

        Assert.Equal(10m, item.RemainingQuantity);
    }

    [Fact]
    public void Update_ZeroQuantity_ThrowsDomainException()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            item.Update(quantity: 0m, unitCost: 25.00m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void Update_NegativeQuantity_ThrowsDomainException()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            item.Update(quantity: -1m, unitCost: 25.00m));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void Update_NegativeUnitCost_ThrowsDomainException()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var ex = Assert.Throws<DomainException>(() =>
            item.Update(quantity: 5m, unitCost: -1m));

        Assert.Contains("cannot be negative", ex.Message);
    }

    [Fact]
    public void Update_ZeroUnitCost_IsAllowed()
    {
        var item = PurchaseOrderItem.Create(
            purchaseOrderId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        item.Update(quantity: 5m, unitCost: 0m);

        Assert.Equal(0m, item.UnitCost);
    }
}
