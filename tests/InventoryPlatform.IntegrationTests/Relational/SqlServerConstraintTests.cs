using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Relational;

/// <summary>
/// Sprint 18 T03 — R2/R3: database-enforced integrity contracts (SQL Server/LocalDB).
///
/// <para>
/// <b>R2 — Product SKU unique rejection.</b> The migrated schema contains the unique
/// index <c>IX_Products_Sku</c> on <c>Products.Sku</c> (created by migration
/// <c>20260723104601_InitialInventorySchema</c>, <c>unique: true</c>). A second
/// distinct Product persisted with the same SKU must be rejected by SQL Server —
/// not by domain guards (the aggregate does not check SKU uniqueness), not by
/// application validation, and not by EF InMemory (which ignores unique indexes).
/// Expected provider behavior: <see cref="DbUpdateException"/> wrapping
/// <see cref="SqlException"/> with error number <b>2601</b> (duplicate key on a
/// unique index).
/// </para>
///
/// <para>
/// <b>R3 — Category-in-use delete restriction.</b> <c>Products.CategoryId</c> carries
/// the FK <c>FK_Products_Categories_CategoryId</c> with
/// <c>onDelete: ReferentialAction.Restrict</c> (same migration; matching
/// <c>DeleteBehavior.Restrict</c> in <c>ProductConfiguration</c>). Deleting a
/// Category that is still referenced by a Product must be rejected by SQL Server.
/// To ensure the database — not EF client-side cascade/relationship handling —
/// performs the enforcement, the delete is executed in a FRESH context where the
/// dependent Product is not loaded or tracked (prompt §12 caveat honored). Expected
/// provider behavior: <see cref="DbUpdateException"/> wrapping
/// <see cref="SqlException"/> with error number <b>547</b> (foreign-key conflict —
/// the DELETE statement conflicts with the REFERENCE constraint).
/// </para>
///
/// <para>
/// Both tests use the accepted T01 guarded lifecycle (unique
/// <c>InventoryPlatformRelationalTests_&lt;guid&gt;</c> database on
/// <c>(localdb)\MSSQLLocalDB</c>) and apply the real migration chain per accepted
/// T02 (no <c>EnsureCreated</c>, no seeders). Persisted-state verification uses clean
/// contexts after the failed operation. No R4–R6 semantics are asserted. Default
/// xUnit parallelism preserved — each test owns an independent database.
/// </para>
/// </summary>
[Trait(TestTiers.TraitKey, TestTiers.SqlServerRelational)]
public sealed class SqlServerConstraintTests
{
    // SQL Server error numbers (stable provider metadata, not localized text):
    // 2601 = Cannot insert duplicate key row in object '%.*ls' with unique index '%.*ls'.
    // 547  = The %ls statement conflicted with the %ls constraint "%.*ls".
    private const int UniqueIndexViolationErrorNumber = 2601;
    private const int ForeignKeyConflictErrorNumber = 547;

    // =====================================================================
    // R2 — Product SKU unique rejection
    // =====================================================================

    [Fact]
    public async Task DuplicateProductSku_IsRejectedBySqlServerUniqueIndex()
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
        // B. Arrange — Category + Unit (both Restrict-referenced by Product),
        //    then one valid Product with a chosen SKU.
        // ================================================================
        const string duplicateSku = "T03-DUP-SKU";

        int categoryId;
        int unitId;
        int originalProductId;

        await using (var arrangeContext = database.CreateContext())
        {
            var category = new Category("T03 Components", null);
            var unit = new Unit("T03-PCS", "T03 Piece", "t03pcs");
            arrangeContext.Categories.Add(category);
            arrangeContext.Units.Add(unit);
            await arrangeContext.SaveChangesAsync();

            categoryId = category.Id;
            unitId = unit.Id;

            var original = new Product(
                duplicateSku,
                "T03 Original Widget",
                categoryId,
                unitId,
                quantityOnHand: 0m,
                costPrice: 10m,
                sellingPrice: 15m);
            arrangeContext.Products.Add(original);
            await arrangeContext.SaveChangesAsync();

            originalProductId = original.Id;
        }

        // ================================================================
        // C. Violating operation — a DISTINCT product with the SAME SKU, in a
        //    fresh context. The Product aggregate performs no SKU-uniqueness
        //    check, so the insert reaches SQL Server and the unique index
        //    IX_Products_Sku must reject it.
        // ================================================================
        await using (var violatingContext = database.CreateContext())
        {
            var duplicate = new Product(
                duplicateSku,
                "T03 Duplicate Widget",
                categoryId,
                unitId,
                quantityOnHand: 0m,
                costPrice: 99m,
                sellingPrice: 199m);
            violatingContext.Products.Add(duplicate);

            var exception = await Assert.ThrowsAnyAsync<DbUpdateException>(
                () => violatingContext.SaveChangesAsync());

            // ============================================================
            // D. Provider evidence — the EF boundary failure must wrap the
            //    SQL Server unique-index violation (2601), distinguishing it
            //    from an arbitrary database error. Stable error-number
            //    metadata is asserted rather than localized message text.
            // ============================================================
            var sqlException = Assert.IsType<SqlException>(exception.InnerException);
            Assert.Equal(UniqueIndexViolationErrorNumber, sqlException.Number);
            Assert.Contains(
                "IX_Products_Sku",
                sqlException.Message,
                StringComparison.Ordinal);

            // EF rethrows with the change tracker still holding the failed entity;
            // explicitly stop tracking it so no later step can accidentally retry it.
            violatingContext.Entry(duplicate).State = EntityState.Detached;
        }

        // ================================================================
        // E. Persisted-state verification — clean context: exactly the
        //    original product exists; no duplicate was stored.
        // ================================================================
        await using (var verifyContext = database.CreateContext())
        {
            var productsWithSku = await verifyContext.Products
                .AsNoTracking()
                .Where(product => product.Sku == duplicateSku)
                .ToListAsync();

            var original = Assert.Single(productsWithSku);
            Assert.Equal(originalProductId, original.Id);
            Assert.Equal("T03 Original Widget", original.Name);
            Assert.Equal(categoryId, original.CategoryId);
            Assert.Equal(unitId, original.UnitId);
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
    // R3 — Category-in-use delete restriction
    // =====================================================================

    [Fact]
    public async Task DeletingReferencedCategory_IsRejectedBySqlServerForeignKey()
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
        // B. Arrange — Category referenced by a Product (which also needs a
        //    Unit; both FKs are Restrict in the migrated schema).
        // ================================================================
        int categoryId;
        int productId;

        await using (var arrangeContext = database.CreateContext())
        {
            var category = new Category("T03 Referenced Category", null);
            var unit = new Unit("T03-PCS2", "T03 Piece 2", "t03p2");
            arrangeContext.Categories.Add(category);
            arrangeContext.Units.Add(unit);
            await arrangeContext.SaveChangesAsync();

            categoryId = category.Id;

            var product = new Product(
                "T03-REF-SKU",
                "T03 Referencing Widget",
                categoryId,
                unit.Id,
                quantityOnHand: 0m,
                costPrice: 5m,
                sellingPrice: 8m);
            arrangeContext.Products.Add(product);
            await arrangeContext.SaveChangesAsync();

            productId = product.Id;
        }

        // ================================================================
        // C. Violating operation — delete the referenced Category in a FRESH
        //    context where the dependent Product is NOT tracked. With no
        //    tracked dependents, EF Core issues a plain DELETE against
        //    Categories and the database FK (ReferentialAction.Restrict)
        //    must reject it — proving database-level enforcement.
        // ================================================================
        await using (var violatingContext = database.CreateContext())
        {
            // Load ONLY the Category by id: the dependent Product is deliberately not
            // queried, so nothing on the Product side is tracked and EF issues a plain
            // DELETE against Categories — the database FK must do the enforcing.
            var categoryToDelete = await violatingContext.Categories
                .AsNoTracking()
                .SingleAsync(c => c.Id == categoryId);
            violatingContext.Categories.Remove(categoryToDelete);

            var exception = await Assert.ThrowsAnyAsync<DbUpdateException>(
                () => violatingContext.SaveChangesAsync());

            // ============================================================
            // D. Provider evidence — SQL Server FK conflict (error 547) on
            //    the actual Product→Category reference constraint.
            // ============================================================
            var sqlException = Assert.IsType<SqlException>(exception.InnerException);
            Assert.Equal(ForeignKeyConflictErrorNumber, sqlException.Number);
            Assert.Contains(
                "FK_Products_Categories_CategoryId",
                sqlException.Message,
                StringComparison.Ordinal);

            violatingContext.Entry(categoryToDelete).State = EntityState.Detached;
        }

        // ================================================================
        // E. Persisted-state verification — clean context: Category and
        //    Product both remain, relationship intact.
        // ================================================================
        await using (var verifyContext = database.CreateContext())
        {
            var category = await verifyContext.Categories
                .AsNoTracking()
                .SingleAsync(c => c.Id == categoryId);
            Assert.Equal("T03 Referenced Category", category.Name);

            var product = await verifyContext.Products
                .AsNoTracking()
                .SingleAsync(p => p.Id == productId);
            Assert.Equal(categoryId, product.CategoryId);
            Assert.Equal("T03-REF-SKU", product.Sku);
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
