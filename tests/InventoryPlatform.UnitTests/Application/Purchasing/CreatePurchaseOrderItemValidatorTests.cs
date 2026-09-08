using FluentValidation;
using InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class CreatePurchaseOrderItemValidatorTests
{
    // =====================================================================
    // ProductId > 0
    // =====================================================================

    [Fact]
    public void Validate_ValidProductId_ReturnsNoErrors()
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: 1,
            Quantity: 1m,
            UnitCost: 0m);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_InvalidProductId_FailsWithExpectedPropertyAndMessage(int productId)
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: productId,
            Quantity: 1m,
            UnitCost: 0m);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("ProductId", error.PropertyName);
        Assert.Equal("A valid product must be selected.", error.ErrorMessage);
    }

    // =====================================================================
    // Quantity > 0
    // =====================================================================

    [Fact]
    public void Validate_ZeroQuantity_Fails()
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: 1,
            Quantity: 0m,
            UnitCost: 0m);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("Quantity", error.PropertyName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_NegativeQuantity_Fails(int quantity)
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: 1,
            Quantity: quantity,
            UnitCost: 0m);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_PositiveQuantity_IsValid()
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: 1,
            Quantity: 1m,
            UnitCost: 0m);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // UnitCost >= 0
    // =====================================================================

    [Fact]
    public void Validate_ZeroUnitCost_IsValid()
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: 1,
            Quantity: 1m,
            UnitCost: 0m);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_PositiveUnitCost_IsValid()
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: 1,
            Quantity: 1m,
            UnitCost: 10.50m);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_NegativeUnitCost_Fails(decimal unitCost)
    {
        // Arrange
        var validator = new CreatePurchaseOrderItemValidator();
        var request = new CreatePurchaseOrderItemRequest(
            ProductId: 1,
            Quantity: 1m,
            UnitCost: unitCost);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "UnitCost");
    }
}
