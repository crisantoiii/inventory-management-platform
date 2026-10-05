using InventoryPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.BrowserTests.Infrastructure;

public static class BrowserSeedData
{
    public const string CategoryName = "E2E Smoke Category";
    public const string UnitName = "E2E Smoke Unit";
    public const string UnitCode = "E2E-SMOKE-UNIT";
    public const string UnitSymbol = "E2E-SMOKE";
    public const string SupplierName = "E2E Smoke Supplier";
    public const string ProductSku = "E2E-SMOKE-PRODUCT";
    public const string ProductName = "E2E Smoke Product";
    public const decimal ProductCostPrice = 12.50m;
    public const decimal ProductSellingPrice = 20.00m;

    public static async Task SeedAsync(
        DatabaseFixture database,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);

        await using var context = database.CreateDbContext();

        var category = await context.Categories
            .SingleOrDefaultAsync(
                item => item.Name == CategoryName,
                cancellationToken);

        if (category is null)
        {
            category = new Category(CategoryName, description: null);
            context.Categories.Add(category);
        }
        else if (!category.IsActive)
        {
            throw new InvalidOperationException(
                $"The browser seed category '{CategoryName}' is inactive.");
        }

        var unit = await context.Units
            .SingleOrDefaultAsync(
                item => item.Code == UnitCode,
                cancellationToken);

        if (unit is null)
        {
            unit = new Unit(UnitCode, UnitName, UnitSymbol);
            context.Units.Add(unit);
        }
        else if (unit.Name != UnitName ||
                 unit.Symbol != UnitSymbol ||
                 !unit.IsActive)
        {
            throw new InvalidOperationException(
                $"The browser seed unit code '{UnitCode}' already exists with " +
                "values that do not match the frozen smoke data.");
        }

        var supplier = await context.Suppliers
            .SingleOrDefaultAsync(
                item => item.Name == SupplierName,
                cancellationToken);

        if (supplier is null)
        {
            supplier = new Supplier(
                SupplierName,
                contactPerson: null,
                email: null,
                phone: null,
                address: null);
            context.Suppliers.Add(supplier);
        }
        else if (!supplier.IsActive)
        {
            throw new InvalidOperationException(
                $"The browser seed supplier '{SupplierName}' is inactive.");
        }

        await context.SaveChangesAsync(cancellationToken);

        var product = await context.Products
            .SingleOrDefaultAsync(
                item => item.Sku == ProductSku,
                cancellationToken);

        if (product is null)
        {
            product = new Product(
                ProductSku,
                ProductName,
                category.Id,
                unit.Id,
                quantityOnHand: 0m,
                costPrice: ProductCostPrice,
                sellingPrice: ProductSellingPrice)
            {
                CreatedAtUtc = DateTime.UtcNow
            };

            context.Products.Add(product);
        }
        else if (product.Name != ProductName ||
                 product.CategoryId != category.Id ||
                 product.UnitId != unit.Id ||
                 product.QuantityOnHand != 0m ||
                 product.CostPrice != ProductCostPrice ||
                 product.SellingPrice != ProductSellingPrice ||
                 !product.IsActive)
        {
            throw new InvalidOperationException(
                $"The browser seed product SKU '{ProductSku}' already exists " +
                "with values that do not match the frozen smoke data.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
