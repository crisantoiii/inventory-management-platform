using InventoryPlatform.Application.Features.Reporting.GetInventoryMovement;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using InventoryPlatform.Shared.Paging;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace InventoryPlatform.IntegrationTests.Relational;

/// <summary>
/// Sprint 18 T05 — R6: Inventory Movement aggregation/query translation
/// (SQL Server/LocalDB).
///
/// <para>
/// Proves that the ACTUAL production Inventory Movement query path translates
/// through the real SQL Server EF Core provider, executes against the real migrated
/// schema, and returns the deterministic aggregate/projection result for a minimal
/// controlled dataset. The real production call chain is exercised end-to-end —
/// <see cref="GetInventoryMovementHandler"/> (Application) →
/// <see cref="IInventoryMovementRepository"/> (Application abstraction) →
/// <see cref="InventoryMovementRepository.GetInventoryMovementAsync(PagedQuery, DateOnly?, DateOnly?, System.Threading.CancellationToken)"/>
/// (Infrastructure) → real <see cref="ApplicationDbContext"/> — with NO test doubles
/// anywhere on the query path and NO copied LINQ.
/// </para>
///
/// <para>
/// <b>Production semantics (inspected from source — the repository is authoritative):</b>
/// <c>InventoryMovementRepository.GetInventoryMovementAsync</c> builds one row per
/// Product (products with no movement still produce a row with zero aggregates;
/// <c>TotalCount</c> counts every Product). For the requested period
/// <c>[FromDate 00:00, ToDate+1day 00:00)</c> compared against the
/// <c>datetime2</c> column <c>InventoryTransactions.TransactionDateUtc</c>
/// (FromDate inclusive; ToDate inclusive via the exclusive next-midnight bound):
///
/// <list type="bullet">
/// <item><c>StockInQuantity/StockOutQuantity/AdjustmentQuantity</c> = in-period
/// <c>SUM(Quantity)</c> per <c>TransactionType</c>, coalesced to 0 when empty;</item>
/// <item>signed movement = StockIn +q, StockOut −q, Adjustment +q;</item>
/// <item><c>OpeningQuantity = QuantityOnHand − Σ signed(≥ FromDate)</c> — no upper
/// bound, i.e. post-period movement is subtracted too (stock as of period start);</item>
/// <item><c>ClosingQuantity = QuantityOnHand − Σ signed(≥ ToDate-exclusive)</c>
/// (stock as of period end).</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Server-side vs client-side (source-inspected, §13):</b> no materialization
/// occurs before aggregation — the entire expression tree (correlated SUM subqueries,
/// CASE sign mapping, COALESCE defaults, Opening/Closing arithmetic against
/// <c>QuantityOnHand</c>, the <c>CountAsync</c> total, the default
/// <c>OrderBy(ProductName).ThenBy(ProductSku)</c>, Skip/Take paging, and the final
/// DTO projection) is translated and executed by SQL Server inside the single
/// <c>ToListAsync</c>. Client-side work is only <c>PagedResult</c> construction and
/// the <c>Result&lt;T&gt;.Success</c> wrapper. The repository signature does not
/// expose an <c>IQueryable</c>, so <c>ToQueryString()</c> is unavailable without
/// production changes; generated-SQL evidence is instead captured naturally through
/// EF Core <c>CommandExecuted</c> diagnostic logging enabled on the test-side options
/// builder (no production code modified).
/// </para>
///
/// <para>
/// <b>Scope discipline (frozen R6 boundary):</b> exactly one representative
/// Inventory Movement query contract. No second report query (StockMovement /
/// Supplier Purchase Analysis are NOT exercised), no HTTP/Razor page, no Excel/PDF
/// export path (<c>HandleExportAsync</c> is NOT invoked), no search-filter contract
/// (<c>Search</c> is left null), and no ordering assertion (the default ordering is
/// explicitly defined in production but is not part of the frozen R6 aggregation
/// contract; result rows are selected deterministically by <c>ProductId</c>).
/// </para>
///
/// <para>
/// Uses the accepted T01 guarded lifecycle (unique
/// <c>InventoryPlatformRelationalTests_&lt;guid&gt;</c> database on
/// <c>(localdb)\MSSQLLocalDB</c>) and the real migration chain per accepted T02.
/// No <c>EnsureCreated</c>, no seeders, no InMemory/SQLite substitute. Default xUnit
/// parallelism preserved — this test owns its own independent database.
/// </para>
/// </summary>
public sealed class SqlServerInventoryMovementQueryTests
{
    private readonly ITestOutputHelper _output;

    public SqlServerInventoryMovementQueryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task GetInventoryMovementAsync_OnRealSqlServer_TranslatesProductionQueryAndReturnsDeterministicAggregates()
    {
        // ================================================================
        // A. Fresh guarded database via accepted T01 lifecycle + real
        //    migration chain per accepted T02.
        // ================================================================
        await using var database = await RelationalTestDatabase.CreateAsync();

        await using (var migrateContext = database.CreateContext())
        {
            await migrateContext.Database.MigrateAsync();
        }

        // ================================================================
        // B. Arrange — minimal deterministic dataset, explicitly timestamped
        //    (no DateTime.Now/UtcNow; no timezone assumptions).
        //
        //    Reporting period requested: FromDate 2026-06-01, ToDate 2026-06-30
        //    → production converts to [2026-06-01T00:00:00, 2026-07-01T00:00:00).
        //
        //    Product "T05 Alpha Widget": QuantityOnHand = 100.00, with SIX
        //    deterministic transactions covering every meaningful distinction:
        //
        //    | # | type        | qty    | timestamp            | role
        //    |---|-------------|--------|----------------------|------------------
        //    | 1 | StockIn     | 25.00  | 2026-05-31T23:59:59  | pre-period (excluded everywhere — proves the FromDate filter)
        //    | 2 | StockIn     | 30.00  | 2026-06-01T00:00:00  | in-period, EXACTLY at the FromDate boundary (must be INCLUDED — proves >=)
        //    | 3 | StockIn     | 12.50  | 2026-06-10T15:30:00  | in-period; second transaction feeding the same StockIn aggregate (proves SUM)
        //    | 4 | StockOut    | 17.25  | 2026-06-15T09:00:00  | in-period negative/outbound movement
        //    | 5 | Adjustment  |  3.75  | 2026-06-20T11:00:00  | in-period adjustment movement
        //    | 6 | StockOut    |  9.00  | 2026-07-01T00:00:00  | EXACTLY at the exclusive ToDate bound (must be EXCLUDED from in-period aggregates — proves <; still counts in Closing)
        //
        //    Product "T05 Beta Widget": QuantityOnHand = 40.00, NO transactions
        //    (a product row with zero movement and unchanged Opening/Closing;
        //    also proves TotalCount counts every Product).
        // ================================================================
        int alphaProductId;
        int betaProductId;

        await using (var arrangeContext = database.CreateContext())
        {
            var category = new Category("T05 Movement Category", null);
            var unit = new Unit("T05-MV-PCS", "T05 Movement Piece", "t05mvpcs");
            arrangeContext.Categories.Add(category);
            arrangeContext.Units.Add(unit);
            await arrangeContext.SaveChangesAsync();

            var alpha = new Product(
                "T05-MV-ALPHA",
                "T05 Alpha Widget",
                category.Id,
                unit.Id,
                quantityOnHand: 100.00m,
                costPrice: 10m,
                sellingPrice: 15m);

            var beta = new Product(
                "T05-MV-BETA",
                "T05 Beta Widget",
                category.Id,
                unit.Id,
                quantityOnHand: 40.00m,
                costPrice: 5m,
                sellingPrice: 8m);

            arrangeContext.Products.AddRange(alpha, beta);
            await arrangeContext.SaveChangesAsync();

            alphaProductId = alpha.Id;
            betaProductId = beta.Id;

            arrangeContext.InventoryTransactions.AddRange(
                new InventoryTransaction(
                    alphaProductId,
                    TransactionType.StockIn,
                    25.00m,
                    "T05-PRE-IN",
                    remarks: null,
                    new DateTime(2026, 5, 31, 23, 59, 59)),

                new InventoryTransaction(
                    alphaProductId,
                    TransactionType.StockIn,
                    30.00m,
                    "T05-IN-1",
                    remarks: null,
                    new DateTime(2026, 6, 1, 0, 0, 0)),

                new InventoryTransaction(
                    alphaProductId,
                    TransactionType.StockIn,
                    12.50m,
                    "T05-IN-2",
                    remarks: null,
                    new DateTime(2026, 6, 10, 15, 30, 0)),

                new InventoryTransaction(
                    alphaProductId,
                    TransactionType.StockOut,
                    17.25m,
                    "T05-OUT-1",
                    remarks: null,
                    new DateTime(2026, 6, 15, 9, 0, 0)),

                new InventoryTransaction(
                    alphaProductId,
                    TransactionType.Adjustment,
                    3.75m,
                    "T05-ADJ-1",
                    remarks: null,
                    new DateTime(2026, 6, 20, 11, 0, 0)),

                new InventoryTransaction(
                    alphaProductId,
                    TransactionType.StockOut,
                    9.00m,
                    "T05-POST-OUT",
                    remarks: null,
                    new DateTime(2026, 7, 1, 0, 0, 0)));

            await arrangeContext.SaveChangesAsync();
        }

        // ================================================================
        // C. Execute the REAL production query path: real Application handler
        //    + real Infrastructure repository + real ApplicationDbContext over
        //    the real SQL Server (LocalDB) provider. The request mirrors the
        //    canonical production invocation (Reports/InventoryMovement page,
        //    first page, default page size, no search, default sort).
        //
        //    The query context is built on the T01 guard-validated options with
        //    EF Core CommandExecuted logging added TEST-SIDE ONLY (no production
        //    change) so the generated SQL is captured as natural evidence.
        // ================================================================
        var executedSql = new List<string>();

        var queryOptions = new DbContextOptionsBuilder<ApplicationDbContext>(
                database.CreateContextOptions())
            .LogTo(
                message => executedSql.Add(message),
                new[] { RelationalEventId.CommandExecuted })
            .Options;

        await using (var queryContext = new ApplicationDbContext(queryOptions))
        {
            var repository = new InventoryMovementRepository(queryContext);
            var handler = new GetInventoryMovementHandler(repository);

            var request = new GetInventoryMovementRequest(
                new PagedQuery { PageNum = 1, PageSize = 10 },
                FromDate: new DateOnly(2026, 6, 1),
                ToDate: new DateOnly(2026, 6, 30));

            // ============================================================
            // D. Translation + execution proof: the real production LINQ must
            //    translate and execute against real SQL Server with no
            //    translation exception, and return the deterministic result.
            // ============================================================
            var result = await handler.HandleAsync(request);

            Assert.True(result.IsSuccess);

            var page = result.Value!;
            Assert.NotNull(page);

            // TotalCount counts EVERY product (one movement row per product,
            // including the zero-movement product) — a real CountAsync contract
            // of the production method.
            Assert.Equal(2, page.TotalCount);
            Assert.Equal(1, page.Page);
            Assert.Equal(10, page.PageSize);
            Assert.Equal(2, page.Items.Count);

            // ------------------------------------------------------------
            // Expected values calculated INDEPENDENTLY from the arranged data
            // by simple arithmetic (physical walkthrough), NOT by re-running
            // the production LINQ:
            //
            // Signed in-period movement (rows 2–5 only):
            //   +30.00 +12.50 −17.25 +3.75 = +29.00
            //
            // Backwards from QuantityOnHand 100.00 through the full history
            // (row 1 pre-period is already inside QoH and must not reappear;
            // row 6 post-period StockOut of 9.00 still lies ahead):
            //   Opening = 100.00 − (29.00 − 9.00) = 80.00
            //   In-period columns: StockIn = 30.00 + 12.50 = 42.50
            //                      StockOut = 17.25
            //                      Adjustment = 3.75
            //   Closing = 80.00 + 29.00 = 109.00
            //
            // Any translation defect (broken type/date filter, wrong boundary
            // comparison, wrong sign, broken SUM, broken COALESCE) shifts at
            // least one of these values.
            // ------------------------------------------------------------
            var alpha = Assert.Single(page.Items, item => item.ProductId == alphaProductId);
            Assert.Equal("T05 Alpha Widget", alpha.ProductName);
            Assert.Equal("T05-MV-ALPHA", alpha.ProductSku);
            Assert.Equal(80.00m, alpha.OpeningQuantity);
            Assert.Equal(42.50m, alpha.StockInQuantity);
            Assert.Equal(17.25m, alpha.StockOutQuantity);
            Assert.Equal(3.75m, alpha.AdjustmentQuantity);
            Assert.Equal(109.00m, alpha.ClosingQuantity);

            // Zero-movement product: every aggregate 0; Opening = Closing = QoH.
            var beta = Assert.Single(page.Items, item => item.ProductId == betaProductId);
            Assert.Equal("T05 Beta Widget", beta.ProductName);
            Assert.Equal("T05-MV-BETA", beta.ProductSku);
            Assert.Equal(40.00m, beta.OpeningQuantity);
            Assert.Equal(0m, beta.StockInQuantity);
            Assert.Equal(0m, beta.StockOutQuantity);
            Assert.Equal(0m, beta.AdjustmentQuantity);
            Assert.Equal(40.00m, beta.ClosingQuantity);
        }

        // ================================================================
        // E. Generated-SQL evidence (optional per prompt §12, naturally
        //    available via test-side diagnostic logging): HandleAsync issues
        //    exactly two server-side commands — the CountAsync total and the
        //    paged projection — and the aggregation command provably queries
        //    the InventoryTransactions table on the server.
        // ================================================================
        Assert.Equal(2, executedSql.Count);

        foreach (var command in executedSql)
        {
            _output.WriteLine("=== SQL Server executed command ===");
            _output.WriteLine(command);
        }

        Assert.Contains(
            executedSql,
            sql => sql.Contains("InventoryTransactions", StringComparison.Ordinal));
        Assert.Contains(
            executedSql,
            sql => sql.Contains("Products", StringComparison.Ordinal));

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
