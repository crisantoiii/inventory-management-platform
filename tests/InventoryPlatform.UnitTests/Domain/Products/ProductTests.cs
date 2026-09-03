using InventoryPlatform.Domain.Entities;
using Xunit;

namespace InventoryPlatform.UnitTests.Domain.Products;

public class ProductTests
{
    private const int ValidCategoryId = 1;
    private const int ValidUnitId = 1;

    [Fact]
    public void Constructor_WithValidParameters_CreatesProduct()
    {
        var product = CreateValidProduct();

        Assert.True(product.Id >= 0, "Product Id should be a valid integer");
        Assert.Equal("TEST-SKU-001", product.Sku);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal(ValidCategoryId, product.CategoryId);
        Assert.Equal(ValidUnitId, product.UnitId);
        Assert.Equal(50m, product.QuantityOnHand);
        Assert.Equal(10.50m, product.CostPrice);
        Assert.Equal(25.99m, product.SellingPrice);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void Constructor_WithInvalidSku_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Product(
            string.Empty,
            "Test Product",
            ValidCategoryId,
            ValidUnitId,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithNullSku_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Product(
            null!,
            "Test Product",
            ValidCategoryId,
            ValidUnitId,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithInvalidName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Product(
            "SKU-001",
            string.Empty,
            ValidCategoryId,
            ValidUnitId,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Product(
            "SKU-001",
            null!,
            ValidCategoryId,
            ValidUnitId,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithNegativeCostPrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            "SKU-001",
            "Test Product",
            ValidCategoryId,
            ValidUnitId,
            0m,
            -10m,
            25m));
    }

    [Fact]
    public void Constructor_WithZeroCostPrice_AllowsCreation()
    {
        var product = new Product("SKU-001", "Test Product", ValidCategoryId, ValidUnitId, 0m, 0m, 25m);
        Assert.Equal(0m, product.CostPrice);
    }

    [Fact]
    public void Constructor_WithNegativeSellingPrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            "SKU-001",
            "Test Product",
            ValidCategoryId,
            ValidUnitId,
            0m,
            10m,
            -25m));
    }

    [Fact]
    public void Constructor_WithZeroSellingPrice_AllowsCreation()
    {
        var product = new Product("SKU-001", "Test Product", ValidCategoryId, ValidUnitId, 0m, 10m, 0m);
        Assert.Equal(0m, product.SellingPrice);
    }

    [Fact]
    public void Constructor_WithZeroCategoryId_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            "SKU-001",
            "Test Product",
            0,
            ValidUnitId,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithNegativeCategoryId_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            "SKU-001",
            "Test Product",
            -1,
            ValidUnitId,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithZeroUnitId_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            "SKU-001",
            "Test Product",
            ValidCategoryId,
            0,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithNegativeUnitId_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            "SKU-001",
            "Test Product",
            ValidCategoryId,
            -1,
            0m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithNegativeQuantityOnHand_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(
            "SKU-001",
            "Test Product",
            ValidCategoryId,
            ValidUnitId,
            -10m,
            10m,
            25m));
    }

    [Fact]
    public void Constructor_WithZeroQuantityOnHand_AllowsCreation()
    {
        var product = new Product("SKU-001", "Test Product", ValidCategoryId, ValidUnitId, 0m, 10m, 25m);
        Assert.Equal(0m, product.QuantityOnHand);
    }

    // --- Rename ---

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var product = CreateValidProduct();

        product.Rename("Updated Product");

        Assert.Equal("Updated Product", product.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsArgumentException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentException>(() => product.Rename(string.Empty));
    }

    [Fact]
    public void Rename_WithNullName_ThrowsArgumentException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentException>(() => product.Rename(null!));
    }

    // --- ChangeCategory ---

    [Fact]
    public void ChangeCategory_WithValidId_UpdatesCategoryId()
    {
        var product = CreateValidProduct();

        product.ChangeCategory(5);

        Assert.Equal(5, product.CategoryId);
    }

    [Fact]
    public void ChangeCategory_WithZeroId_ThrowsArgumentOutOfRangeException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ChangeCategory(0));
    }

    [Fact]
    public void ChangeCategory_WithNegativeId_ThrowsArgumentOutOfRangeException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ChangeCategory(-1));
    }

    // --- ChangeUnit ---

    [Fact]
    public void ChangeUnit_WithValidId_UpdatesUnitId()
    {
        var product = CreateValidProduct();

        product.ChangeUnit(3);

        Assert.Equal(3, product.UnitId);
    }

    [Fact]
    public void ChangeUnit_WithZeroId_ThrowsArgumentOutOfRangeException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ChangeUnit(0));
    }

    [Fact]
    public void ChangeUnit_WithNegativeId_ThrowsArgumentOutOfRangeException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ChangeUnit(-1));
    }

    // --- AdjustStock ---

    [Fact]
    public void AdjustStock_WithPositiveQuantity_IncreasesStock()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        product.AdjustStock(10m);

        Assert.Equal(60m, product.QuantityOnHand);
    }

    [Fact]
    public void AdjustStock_WithNegativeQuantity_DecreasesStock()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        product.AdjustStock(-10m);

        Assert.Equal(40m, product.QuantityOnHand);
    }

    [Fact]
    public void AdjustStock_WithZeroQuantity_AllowsChange()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        product.AdjustStock(0m);

        Assert.Equal(50m, product.QuantityOnHand);
    }

    [Fact]
    public void AdjustStock_WhenNegativeAdjustmentExceedsStock_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 10m, 10m, 25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.AdjustStock(-50m));
    }

    [Fact]
    public void AdjustStock_AfterFailedAdjustment_StockRemainsUnchanged()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 10m, 10m, 25m);

        try { product.AdjustStock(-50m); }
        catch (ArgumentOutOfRangeException) { }

        Assert.Equal(10m, product.QuantityOnHand);
    }

    // --- IncreaseStock ---

    [Fact]
    public void IncreaseStock_WithValidQuantity_IncreasesStock()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        product.IncreaseStock(20m);

        Assert.Equal(70m, product.QuantityOnHand);
    }

    [Fact]
    public void IncreaseStock_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.IncreaseStock(0m));
    }

    [Fact]
    public void IncreaseStock_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.IncreaseStock(-10m));
    }

    [Fact]
    public void IncreaseStock_MultipleTimes_AccumulatesQuantity()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 0m, 10m, 25m);

        product.IncreaseStock(30m);
        product.IncreaseStock(20m);

        Assert.Equal(50m, product.QuantityOnHand);
    }

    // --- DecreaseStock ---

    [Fact]
    public void DecreaseStock_WithValidQuantity_DecreasesStock()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        product.DecreaseStock(30m);

        Assert.Equal(20m, product.QuantityOnHand);
    }

    [Fact]
    public void DecreaseStock_ToExactlyZero_SetsStockToZero()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        product.DecreaseStock(50m);

        Assert.Equal(0m, product.QuantityOnHand);
    }

    [Fact]
    public void DecreaseStock_WhenQuantityExceedsAvailableStock_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 10m, 10m, 25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.DecreaseStock(50m));
    }

    [Fact]
    public void DecreaseStock_WithZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.DecreaseStock(0m));
    }

    [Fact]
    public void DecreaseStock_WithNegativeQuantity_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.DecreaseStock(-5m));
    }

    [Fact]
    public void DecreaseStock_WithNoStock_ThrowsArgumentOutOfRangeException()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 0m, 10m, 25m);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.DecreaseStock(1m));
    }

    [Fact]
    public void DecreaseStock_AfterFailedDecrease_StockRemainsUnchanged()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 10m, 10m, 25m);

        try { product.DecreaseStock(50m); }
        catch (ArgumentOutOfRangeException) { }

        Assert.Equal(10m, product.QuantityOnHand);
    }

    // --- CanDecreaseStock ---

    [Fact]
    public void CanDecreaseStock_WhenQuantityAvailable_ReturnsTrue()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        Assert.True(product.CanDecreaseStock(30m));
    }

    [Fact]
    public void CanDecreaseStock_WhenQuantityEqualToAvailable_ReturnsTrue()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        Assert.True(product.CanDecreaseStock(50m));
    }

    [Fact]
    public void CanDecreaseStock_WhenQuantityExceedsAvailable_ReturnsFalse()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 50m, 10m, 25m);

        Assert.False(product.CanDecreaseStock(51m));
    }

    [Fact]
    public void CanDecreaseStock_WhenZeroStock_ReturnsFalse()
    {
        var product = new Product("SKU-001", "Product", ValidCategoryId, ValidUnitId, 0m, 10m, 25m);

        Assert.False(product.CanDecreaseStock(1m));
    }

    // --- ChangeCostPrice ---

    [Fact]
    public void ChangeCostPrice_WithValidValue_UpdatesCostPrice()
    {
        var product = CreateValidProduct();

        product.ChangeCostPrice(30m);

        Assert.Equal(30m, product.CostPrice);
    }

    [Fact]
    public void ChangeCostPrice_WithZeroPrice_UpdatesCostPrice()
    {
        var product = CreateValidProduct();

        product.ChangeCostPrice(0m);

        Assert.Equal(0m, product.CostPrice);
    }

    [Fact]
    public void ChangeCostPrice_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ChangeCostPrice(-10m));
    }

    // --- ChangeSellingPrice ---

    [Fact]
    public void ChangeSellingPrice_WithValidValue_UpdatesSellingPrice()
    {
        var product = CreateValidProduct();

        product.ChangeSellingPrice(35m);

        Assert.Equal(35m, product.SellingPrice);
    }

    [Fact]
    public void ChangeSellingPrice_WithZeroPrice_UpdatesSellingPrice()
    {
        var product = CreateValidProduct();

        product.ChangeSellingPrice(0m);

        Assert.Equal(0m, product.SellingPrice);
    }

    [Fact]
    public void ChangeSellingPrice_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        var product = CreateValidProduct();

        Assert.Throws<ArgumentOutOfRangeException>(() => product.ChangeSellingPrice(-5m));
    }

    // --- Activation / Deactivation ---

    [Fact]
    public void Deactivate_WhenActive_DeactivatesProduct()
    {
        var product = CreateValidProduct();
        Assert.True(product.IsActive);

        product.Deactivate();

        Assert.False(product.IsActive);
    }

    [Fact]
    public void Activate_WhenInactive_ActivatesProduct()
    {
        var product = CreateValidProduct();
        product.Deactivate();

        product.Activate();

        Assert.True(product.IsActive);
    }

    // --- Barcode / Description ---

    [Fact]
    public void ChangeBarcode_WithValidBarcode_UpdatesBarcode()
    {
        var product = CreateValidProduct();

        product.ChangeBarcode("BC-12345");

        Assert.Equal("BC-12345", product.Barcode);
    }

    [Fact]
    public void ChangeBarcode_WithNullBarcode_SetsNull()
    {
        var product = CreateValidProduct();
        product.ChangeBarcode("BC-12345");

        product.ChangeBarcode(null);

        Assert.Null(product.Barcode);
    }

    [Fact]
    public void UpdateDescription_WithValidDescription_UpdatesDescription()
    {
        var product = CreateValidProduct();

        product.UpdateDescription("A detailed description");

        Assert.Equal("A detailed description", product.Description);
    }

    [Fact]
    public void UpdateDescription_WithNull_SetsNull()
    {
        var product = CreateValidProduct();
        product.UpdateDescription("Some description");

        product.UpdateDescription(null);

        Assert.Null(product.Description);
    }

    private static Product CreateValidProduct()
    {
        return new Product(
            "TEST-SKU-001",
            "Test Product",
            ValidCategoryId,
            ValidUnitId,
            50m,
            10.50m,
            25.99m);
    }
}
