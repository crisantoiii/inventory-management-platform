using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Shared.Results;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class PurchaseOrderErrorsTests
{
    // =====================================================================
    // NotFound (used by the Submit/Approve transition handlers, T03)
    // =====================================================================

    [Fact]
    public void NotFound_ReturnsExpectedCodeAndMessage()
    {
        // Act
        var error = PurchaseOrderErrors.NotFound;

        // Assert
        Assert.IsType<Error>(error);
        Assert.Equal("PurchaseOrder.NotFound", error.Code);
        Assert.Equal("Purchase order not found.", error.Message);
    }

    // =====================================================================
    // SupplierNotFound
    // =====================================================================

    [Fact]
    public void SupplierNotFound_ReturnsExpectedCodeAndMessage()
    {
        // Act
        var error = PurchaseOrderErrors.SupplierNotFound;

        // Assert
        Assert.IsType<Error>(error);
        Assert.Equal("PurchaseOrder.SupplierNotFound", error.Code);
        Assert.Equal("Supplier not found.", error.Message);
    }

    // =====================================================================
    // SupplierInactive
    // =====================================================================

    [Fact]
    public void SupplierInactive_ReturnsExpectedCodeAndMessage()
    {
        // Act
        var error = PurchaseOrderErrors.SupplierInactive;

        // Assert
        Assert.IsType<Error>(error);
        Assert.Equal("PurchaseOrder.SupplierInactive", error.Code);
        Assert.Equal("The selected supplier is inactive.", error.Message);
    }

    // =====================================================================
    // ProductNotFound(productId)
    // =====================================================================

    [Fact]
    public void ProductNotFound_WithProductId_ReturnsExpectedCodeAndMessage()
    {
        // Arrange
        var productId = 42;

        // Act
        var error = PurchaseOrderErrors.ProductNotFound(productId);

        // Assert
        Assert.IsType<Error>(error);
        Assert.Equal("PurchaseOrder.ProductNotFound", error.Code);
        Assert.Equal("Product with ID '42' was not found.", error.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9999)]
    public void ProductNotFound_WithVariousProductIds_ReturnsParameterizedMessage(int productId)
    {
        // Act
        var error = PurchaseOrderErrors.ProductNotFound(productId);

        // Assert
        Assert.Equal("PurchaseOrder.ProductNotFound", error.Code);
        Assert.Equal($"Product with ID '{productId}' was not found.", error.Message);
    }

    // =====================================================================
    // ProductInactive(productId)
    // =====================================================================

    [Fact]
    public void ProductInactive_WithProductId_ReturnsExpectedCodeAndMessage()
    {
        // Arrange
        var productId = 7;

        // Act
        var error = PurchaseOrderErrors.ProductInactive(productId);

        // Assert
        Assert.IsType<Error>(error);
        Assert.Equal("PurchaseOrder.ProductInactive", error.Code);
        Assert.Equal("Product with ID '7' is inactive.", error.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void ProductInactive_WithVariousProductIds_ReturnsParameterizedMessage(int productId)
    {
        // Act
        var error = PurchaseOrderErrors.ProductInactive(productId);

        // Assert
        Assert.Equal("PurchaseOrder.ProductInactive", error.Code);
        Assert.Equal($"Product with ID '{productId}' is inactive.", error.Message);
    }
}
