using FluentValidation;
using InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class CreatePurchaseOrderValidatorTests
{
    // =====================================================================
    // Valid request passes
    // =====================================================================

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: "Some remarks",
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 2m, 5m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    // =====================================================================
    // SupplierId > 0
    // =====================================================================

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_InvalidSupplierId_FailsWithExpectedPropertyAndMessage(int supplierId)
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: supplierId,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("SupplierId", error.PropertyName);
        Assert.Equal("A valid supplier must be selected.", error.ErrorMessage);
    }

    [Fact]
    public void Validate_SupplierIdOne_IsValid()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // ExpectedDeliveryDate NotEmpty
    // =====================================================================

    [Fact]
    public void Validate_ExpectedDeliveryDateEmpty_Fails()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: DateOnly.MinValue,
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ExpectedDeliveryDate");
    }

    [Fact]
    public void Validate_ExpectedDeliveryDateProvided_IsValid()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // Remarks maximum length 500
    // =====================================================================

    [Fact]
    public void Validate_RemarksAtMaxLength500_IsValid()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: new string('x', 500),
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RemarksOverMaxLength500_Fails()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: new string('x', 501),
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Remarks");
    }

    [Fact]
    public void Validate_RemarksNull_IsValid()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // =====================================================================
    // Items NotEmpty
    // =====================================================================

    [Fact]
    public void Validate_ItemsEmpty_FailsWithExpectedPropertyAndMessage()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: Array.Empty<CreatePurchaseOrderItemRequest>());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items");
        Assert.Equal(
            "At least one purchase order item is required.",
            result.Errors.First(e => e.PropertyName == "Items").ErrorMessage);
    }

    [Fact]
    public void Validate_ItemsNull_Fails()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: null!);

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items");
    }

    // =====================================================================
    // Per-item child validation via SetValidator / RuleForEach
    // =====================================================================

    [Fact]
    public void Validate_InvalidItemInCollection_FailsChildRules()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(0, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items[0].ProductId");
    }

    [Fact]
    public void Validate_MultipleItems_OnlyInvalidItemsProduceErrors()
    {
        // Arrange
        var validator = new CreatePurchaseOrderValidator();
        var request = new CreatePurchaseOrderRequest(
            SupplierId: 1,
            ExpectedDeliveryDate: new DateOnly(2026, 6, 1),
            Remarks: null,
            Items: new CreatePurchaseOrderItemRequest[]
            {
                new(10, 1m, 1m),
                new(0, 1m, 1m)
            }.AsReadOnly());

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items[1].ProductId");
    }
}
