using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Relational;

/// <summary>
/// Sprint 18 T04 — R4/R5: provider storage semantics contracts (SQL Server/LocalDB).
///
/// <para>
/// <b>R4 — single-SaveChanges all-or-nothing atomicity.</b> When one
/// <see cref="DbContext.SaveChangesAsync()"/> contains multiple pending changes and
/// one of them violates an already-proven relational constraint (the T03-proven
/// unique index <c>IX_Products_Sku</c>), SQL Server/EF Core persists nothing from
/// that SaveChanges unit. The uniqueness violation is only the failure mechanism —
/// the assertion is that the <i>otherwise-valid</i> pending change in the same
/// SaveChanges did not partially persist. Scope claim: one SaveChanges unit only —
/// not multi-SaveChanges transactions, application orchestration, or workflow
/// atomicity.
/// </para>
///
/// <para>
/// <b>R5 — deterministic decimal(18,2) storage behavior.</b>
/// <c>Product.QuantityOnHand</c> is mapped with <c>HasPrecision(18, 2)</c>
/// (<c>ProductConfiguration</c>) and persisted as <c>decimal(18,2)</c>
/// (migration <c>20260723104601_InitialInventorySchema</c>). Observation first
/// (temporary probe, per T04 prompt §12): submitting a value with three fractional
/// digits <i>succeeds</i> and SQL Server stores the value rounded to scale 2 —
/// <c>12.345 → 12.35</c>, <c>10.555 → 10.56</c>, <c>20.125 → 20.13</c> — matching
/// the SQL Server engine's own <c>DECIMAL(18,3) → DECIMAL(18,2)</c> cast result
/// (round half away from zero). The final test encodes that observed contract on
/// the single representative property <c>QuantityOnHand</c>, including a
/// nearest-value (non-ceiling) case <c>12.344 → 12.34</c>, and proves the stored
/// value at the raw storage layer (varchar cast) in addition to EF read-back.
/// </para>
///
/// <para>
/// Both tests use the accepted T01 guarded lifecycle (unique
/// <c>InventoryPlatformRelationalTests_&lt;guid&gt;</c> database on
/// <c>(localdb)\MSSQLLocalDB</c>) and apply the real migration chain per accepted
/// T02. No <c>EnsureCreated</c>, no seeders. Clean contexts verify persisted state.
/// No R6 semantics are asserted. Default xUnit parallelism preserved.
/// </para>
/// </summary>
public sealed class SqlServerStorageSemanticsTests
{
    // SQL Server error number for a duplicate key on a unique index (same stable
    // provider metadata observed and asserted by accepted T03).
    private const int UniqueIndexViolationErrorNumber = 2601;

    // =====================================================================
    // R4 — single-SaveChanges all-or-nothing atomicity
    // =====================================================================

    [Fact]
    public async Task SingleSaveChanges_WithOneConstraintViolation_PersistsNoPartialWrites()
    {
        // ================================================================
        // A. Fresh guarded database + real migration chain.
        // ================================================================
        await using var database = await RelationalTestDatabase.CreateAsync();

        await using (var migrateContext = database.CreateContext())
        {
            await migrateContext.Database.MigrateAsync();
        }

        // ================================================================
        // B. Prerequisite arrangement — one earlier, SUCCESSFUL SaveChanges:
        //    Category + Unit + original Product whose SKU will be duplicated
        //    by the violating pending change below.
        // ================================================================
        const string originalSku = "T04-R4-ORIG";
        const string validSku = "T04-R4-VALID";
        // The violating SKU duplicates the original on purpose — that is the
        // failure mechanism. The violating PRODUCT is distinguished from the
        // prerequisite original by identity (Id), not by SKU.

        int categoryId;
        int unitId;
        int originalProductId;

        await using (var arrangeContext = database.CreateContext())
        {
            var category = new Category("T04 R4 Category", null);
            var unit = new Unit("T04-R4-PCS", "T04 R4 Piece", "t04r4pcs");
            arrangeContext.Categories.Add(category);
            arrangeContext.Units.Add(unit);
            await arrangeContext.SaveChangesAsync();

            categoryId = category.Id;
            unitId = unit.Id;

            var original = new Product(
                originalSku,
                "T04 R4 Original Widget",
                categoryId,
                unitId,
                quantityOnHand: 1m,
                costPrice: 10m,
                sellingPrice: 15m);
            arrangeContext.Products.Add(original);
            await arrangeContext.SaveChangesAsync();

            originalProductId = original.Id;
        }

        // ================================================================
        // C. The R4 operation — ONE SaveChangesAsync containing TWO pending
        //    changes: an independently valid new Product (unique SKU) and a
        //    violating new Product (duplicate SKU). EF Core wraps both in the
        //    implicit SaveChanges transaction; the violation must reject the
        //    whole unit.
        // ================================================================
        await using (var operationContext = database.CreateContext())
        {
            var validProduct = new Product(
                validSku,
                "T04 R4 Valid Widget",
                categoryId,
                unitId,
                quantityOnHand: 3m,
                costPrice: 7m,
                sellingPrice: 11m);

            var violatingProduct = new Product(
                originalSku,
                "T04 R4 Duplicate Widget",
                categoryId,
                unitId,
                quantityOnHand: 5m,
                costPrice: 99m,
                sellingPrice: 199m);

            operationContext.Products.Add(validProduct);
            operationContext.Products.Add(violatingProduct);

            var exception = await Assert.ThrowsAnyAsync<DbUpdateException>(
                () => operationContext.SaveChangesAsync());

            // ============================================================
            // D. Provider evidence — the failure is the known unique-index
            //    violation on IX_Products_Sku (stable error-number metadata,
            //    not localized text), establishing WHY SaveChanges failed.
            // ============================================================
            var sqlException = Assert.IsType<SqlException>(exception.InnerException);
            Assert.Equal(UniqueIndexViolationErrorNumber, sqlException.Number);
            Assert.Contains(
                "IX_Products_Sku",
                sqlException.Message,
                StringComparison.Ordinal);

            operationContext.Entry(violatingProduct).State = EntityState.Detached;
            operationContext.Entry(validProduct).State = EntityState.Detached;
        }

        // ================================================================
        // E. Clean-context verification — nothing from the failed
        //    SaveChanges survived: not the violating write, and not the
        //    otherwise-valid pending write. Prerequisite data is intact.
        // ================================================================
        await using (var verifyContext = database.CreateContext())
        {
            var products = await verifyContext.Products
                .AsNoTracking()
                .ToListAsync();

            // Exactly one product survives: the prerequisite original row
            // (identity-checked). Neither the valid pending product nor the
            // violating duplicate (which would have a different Id with the
            // same SKU) persisted — the failed SaveChanges left no partial write.
            var original = Assert.Single(products);
            Assert.Equal(originalProductId, original.Id);
            Assert.Equal(originalSku, original.Sku);
            Assert.Equal("T04 R4 Original Widget", original.Name);

            Assert.DoesNotContain(products, product => product.Sku == validSku);

            var category = await verifyContext.Categories
                .AsNoTracking()
                .SingleAsync(c => c.Id == categoryId);
            Assert.Equal("T04 R4 Category", category.Name);

            var unit = await verifyContext.Units
                .AsNoTracking()
                .SingleAsync(u => u.Id == unitId);
            Assert.Equal("T04-R4-PCS", unit.Code);
        }

        // ================================================================
        // F. Guarded cleanup through the T01 lifecycle.
        // ================================================================
        var databaseName = database.DatabaseName;
        await database.DisposeAsync();
        Assert.Null(database.LastCleanupError);
        Assert.False(await PhysicalDatabaseExistsAsync(databaseName));
    }

    // =====================================================================
    // R5 — deterministic decimal(18,2) storage behavior
    // =====================================================================

    [Fact]
    public async Task Decimal18_2Column_StoresDeterministicallyRoundedValue_ForSubscaleInput()
    {
        // ================================================================
        // A. Fresh guarded database + real migration chain.
        // ================================================================
        await using var database = await RelationalTestDatabase.CreateAsync();

        await using (var migrateContext = database.CreateContext())
        {
            await migrateContext.Database.MigrateAsync();
        }

        // ================================================================
        // B. Prerequisite arrangement — Category + Unit (one successful
        //    SaveChanges).
        // ================================================================
        int categoryId;
        int unitId;

        await using (var arrangeContext = database.CreateContext())
        {
            var category = new Category("T04 R5 Category", null);
            var unit = new Unit("T04-R5-PCS", "T04 R5 Piece", "t04r5pcs");
            arrangeContext.Categories.Add(category);
            arrangeContext.Units.Add(unit);
            await arrangeContext.SaveChangesAsync();

            categoryId = category.Id;
            unitId = unit.Id;
        }

        // ================================================================
        // C. Observation-derived operation — persist Products whose
        //    representative decimal(18,2) property (QuantityOnHand) receives
        //    three-fractional-digit values within range and domain invariants
        //    (non-negative). Row 1 probes the observed half-away-from-zero
        //    boundary (…5); row 2 proves nearest-value rounding rather than a
        //    universal ceiling.
        // ================================================================
        const decimal submittedHalfBoundary = 12.345m; // observed: stored 12.35
        const decimal submittedBelowHalf = 12.344m;    // observed: stored 12.34

        await using (var writeContext = database.CreateContext())
        {
            writeContext.Products.Add(new Product(
                "T04-R5-HALF",
                "T04 R5 Half Boundary Widget",
                categoryId,
                unitId,
                quantityOnHand: submittedHalfBoundary,
                costPrice: 10m,
                sellingPrice: 15m));

            writeContext.Products.Add(new Product(
                "T04-R5-BELOW",
                "T04 R5 Below Half Widget",
                categoryId,
                unitId,
                quantityOnHand: submittedBelowHalf,
                costPrice: 10m,
                sellingPrice: 15m));

            // Observation result: SaveChanges SUCCEEDS — the provider does not
            // reject subscale values for this column.
            await writeContext.SaveChangesAsync();
        }

        // ================================================================
        // D. Clean-context EF read-back — the deterministic rounded values.
        // ================================================================
        await using (var readBackContext = database.CreateContext())
        {
            var halfBoundary = await readBackContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Sku == "T04-R5-HALF");
            Assert.Equal(12.35m, halfBoundary.QuantityOnHand);

            var belowHalf = await readBackContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Sku == "T04-R5-BELOW");
            Assert.Equal(12.34m, belowHalf.QuantityOnHand);
        }

        // ================================================================
        // E. Raw storage-layer evidence — what SQL Server actually holds,
        //    read through a plain ADO.NET connection (no EF materialization),
        //    plus the engine's own scale-reduction cast for the same input,
        //    tying the stored value to SQL Server's deterministic
        //    decimal(18,3) → decimal(18,2) rounding (half away from zero).
        // ================================================================
        await using (var rawConnection = OpenMasterlessConnection(database.DatabaseName))
        {
            var storedHalfBoundary = await QueryScalarStringAsync(
                rawConnection,
                "SELECT CAST(QuantityOnHand AS varchar(40)) FROM Products WHERE Sku = 'T04-R5-HALF'");
            Assert.Equal("12.35", storedHalfBoundary);

            var storedBelowHalf = await QueryScalarStringAsync(
                rawConnection,
                "SELECT CAST(QuantityOnHand AS varchar(40)) FROM Products WHERE Sku = 'T04-R5-BELOW'");
            Assert.Equal("12.34", storedBelowHalf);

            // The SQL Server engine's own scale reduction of the same input:
            // decimal(18,3) -> decimal(18,2) yields the same deterministic
            // rounded value the column stored (12.35 — half away from zero).
            await using (var engineCommand = rawConnection.CreateCommand())
            {
                engineCommand.CommandText =
                    "SELECT CAST(CAST(12.345 AS DECIMAL(18,3)) AS DECIMAL(18,2))";
                var engineResult = await engineCommand.ExecuteScalarAsync();
                Assert.Equal(12.35m, Convert.ToDecimal(engineResult));
            }
        }

        // ================================================================
        // F. Guarded cleanup through the T01 lifecycle.
        // ================================================================
        var databaseName = database.DatabaseName;
        await database.DisposeAsync();
        Assert.Null(database.LastCleanupError);
        Assert.False(await PhysicalDatabaseExistsAsync(databaseName));
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static SqlConnection OpenMasterlessConnection(string databaseName)
    {
        var connection = new SqlConnection(
            new SqlConnectionStringBuilder
            {
                DataSource = RelationalSafetyGuard.ApprovedServer,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ConnectTimeout = 15
            }.ConnectionString);
        connection.Open();
        return connection;
    }

    private static async Task<string> QueryScalarStringAsync(
        SqlConnection connection,
        string commandText)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        var result = await command.ExecuteScalarAsync();
        return result is DBNull or null ? string.Empty : result.ToString() ?? string.Empty;
    }

    private static async Task<bool> PhysicalDatabaseExistsAsync(string databaseName)
    {
        await using var connection = new SqlConnection(
            new SqlConnectionStringBuilder
            {
                DataSource = RelationalSafetyGuard.ApprovedServer,
                InitialCatalog = "master",
                IntegratedSecurity = true,
                ConnectTimeout = 15
            }.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT CAST(DB_ID('{SqlLiteralEscape(databaseName)}') AS INT)";
        var result = await command.ExecuteScalarAsync();

        return result is not DBNull and not null;
    }

    private static string SqlLiteralEscape(string databaseName)
    {
        return databaseName.Replace("'", "''", StringComparison.Ordinal);
    }
}
