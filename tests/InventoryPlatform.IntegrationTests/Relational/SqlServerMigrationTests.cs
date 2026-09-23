using System.Reflection;
using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Relational;

/// <summary>
/// Sprint 18 T02 — R1: Fresh Database Migration to Latest (SQL Server/LocalDB).
///
/// Proves the REAL SQL Server migration chain creates the current InventoryPlatform
/// application schema on a fresh, guard-validated, disposable LocalDB database:
///
/// <list type="number">
/// <item>T01 lifecycle creates a unique <c>InventoryPlatformRelationalTests_&lt;guid&gt;</c>
/// database (empty from the application's perspective);</item>
/// <item>pre-migration freshness is positively established: the physical database
/// exists but <c>__EFMigrationsHistory</c> is absent and no application tables exist;</item>
/// <item>the real EF Core migration chain executes through
/// <see cref="RelationalDatabaseFacadeExtensions.MigrateAsync"/> against the real
/// production migration assembly (10 migrations, latest
/// <c>20260831141400_CreateAuthorizationSchema</c>);</item>
/// <item>all repository migrations are applied, none remain pending, and the actual
/// latest repository migration is present in the history table;</item>
/// <item>the migrated schema is positively queryable through the real
/// <see cref="ApplicationDbContext"/> (a core table read — no uniqueness/FK/
/// transaction/precision/report contracts; those are R2–R6).</item>
/// </list>
///
/// Distinct failure class proven here: migration execution failures, SQL Server
/// migration incompatibility, schema-creation failures, and migration-chain
/// ordering/application problems — none of which EF Core InMemory can exercise, and
/// none of which <c>has-pending-model-changes</c> (snapshot synchronization) proves.
///
/// No production seeders are invoked: R1 is migration/schema verification, not seed
/// verification. Cleanup remains guarded through the T01 lifecycle. Default xUnit
/// parallelism is preserved (unique database per test). LocalDB unavailable → real
/// failure, never a skip.
/// </summary>
public sealed class SqlServerMigrationTests
{
    /// <summary>
    /// The actual latest migration ID from the current repository source
    /// (<c>InventoryPlatform.Infrastructure/Persistence/Migrations</c>) — confirmed by
    /// inspection, not assumed from planning. Hard-coded here as an explicit
    /// regression contract: if the migration chain changes, this test legitimately
    /// fails until the contract is updated.
    /// </summary>
    private const string LatestMigrationId = "20260831141400_CreateAuthorizationSchema";

    /// <summary>
    /// Total number of migrations in the repository's migration chain, from the same
    /// inspected source — reconciled against the applied list at runtime.
    /// </summary>
    private const int ExpectedMigrationCount = 10;

    [Fact]
    public async Task MigrateAsync_OnFreshGuardedLocalDBDatabase_AppliesFullMigrationChain()
    {
        // ================================================================
        // A. Fresh guarded database via accepted T01 lifecycle.
        // ================================================================
        await using var database = await RelationalTestDatabase.CreateAsync();

        // ================================================================
        // B. Freshness proof — the database exists physically (T01 created it)
        //    but carries no application schema and no migration history.
        // ================================================================
        Assert.True(await PhysicalDatabaseExistsAsync(database.DatabaseName));
        Assert.False(await MigrationsHistoryTableExistsAsync(database.DatabaseName));

        await using (var preMigrationContext = database.CreateContext())
        {
            // No pending-migration state can exist before history exists; prove the
            // application schema is absent by checking a core table does not exist.
            Assert.False(await TableExistsAsync(preMigrationContext, "Suppliers"));
            Assert.False(await TableExistsAsync(preMigrationContext, "PurchaseOrders"));
        }

        // ================================================================
        // C. Real migration chain execution (not EnsureCreated, not copied SQL).
        // ================================================================
        await using (var migrateContext = database.CreateContext())
        {
            await migrateContext.Database.MigrateAsync();
        }

        // ================================================================
        // D. Latest-migration verification — all applied, none pending, actual
        //    latest repository migration positively present.
        // ================================================================
        await using (var verifyContext = database.CreateContext())
        {
            var applied = await verifyContext.Database.GetAppliedMigrationsAsync();

            Assert.Equal(ExpectedMigrationCount, applied.Count());
            Assert.Contains(applied, id => id.EndsWith(LatestMigrationId, StringComparison.Ordinal));

            var pending = await verifyContext.Database.GetPendingMigrationsAsync();
            Assert.Empty(pending);
        }

        // ================================================================
        // E. Positive schema usability — the migrated schema serves real
        //    queries through the real ApplicationDbContext. A core-table read
        //    proves queryability without overlapping R2–R6 contracts.
        // ================================================================
        await using (var usableContext = database.CreateContext())
        {
            Assert.Equal(0, await usableContext.Suppliers.CountAsync());
            Assert.Equal(0, await usableContext.PurchaseOrders.CountAsync());
            Assert.Equal(0, await usableContext.Capabilities.CountAsync());

            // A real write/read round-trip proves the schema is usable, not merely
            // present. No constraint/transaction/precision behavior is asserted —
            // that belongs to R2–R6.
            // NOTE: the Supplier entity is intentionally declared in the global
            // namespace in current Domain source, so it is referenced unqualified here.
            var supplier = new Supplier(
                "R1 Schema Usability Supplier",
                contactPerson: null,
                email: null,
                phone: null,
                address: null);
            usableContext.Suppliers.Add(supplier);
            await usableContext.SaveChangesAsync();

            var persisted = await usableContext.Suppliers
                .AsNoTracking()
                .SingleAsync();
            Assert.Equal(supplier.Id, persisted.Id);
            Assert.Equal("R1 Schema Usability Supplier", persisted.Name);
        }

        // ================================================================
        // F. Isolation/cleanup remains intact through the T01 lifecycle
        //    (guarded drop on dispose; this test owns its unique database).
        // ================================================================
        var databaseName = database.DatabaseName;
        await database.DisposeAsync();
        Assert.Null(database.LastCleanupError);
        Assert.False(await PhysicalDatabaseExistsAsync(databaseName));
    }

    [Fact]
    public async Task MigrateAsync_IsRepeatableAcrossIndependentFreshDatabases()
    {
        // Fresh-database repeatability: two independent guarded databases each run
        // the full real chain from empty — proving the chain is deterministic and the
        // lifecycle isolation holds under default parallelism.
        await using var first = await RelationalTestDatabase.CreateAsync();
        await using var second = await RelationalTestDatabase.CreateAsync();

        Assert.NotEqual(first.DatabaseName, second.DatabaseName);

        await using (var firstContext = first.CreateContext())
        {
            await firstContext.Database.MigrateAsync();
        }

        await using (var secondContext = second.CreateContext())
        {
            await secondContext.Database.MigrateAsync();
        }

        foreach (var database in new[] { first, second })
        {
            await using var context = database.CreateContext();
            var applied = await context.Database.GetAppliedMigrationsAsync();
            Assert.Equal(ExpectedMigrationCount, applied.Count());
            Assert.Empty(await context.Database.GetPendingMigrationsAsync());
        }
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static async Task<bool> PhysicalDatabaseExistsAsync(string databaseName)
    {
        await using var connection = CreateMasterConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT CAST(DB_ID('{SqlLiteralEscape(databaseName)}') AS INT)";
        var result = await command.ExecuteScalarAsync();

        return result is not DBNull and not null;
    }

    private static async Task<bool> MigrationsHistoryTableExistsAsync(string databaseName)
    {
        await using var connection = CreateOwnedConnection(databaseName);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM sys.tables WHERE name = '__EFMigrationsHistory'";
        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> TableExistsAsync(
        ApplicationDbContext context,
        string tableName)
    {
        var connection = context.Database.GetDbConnection();
        var connectionWasOpen = connection.State == System.Data.ConnectionState.Open;

        if (!connectionWasOpen)
        {
            await connection.OpenAsync();
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM sys.tables WHERE name = @tableName";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            return Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;
        }
        finally
        {
            if (!connectionWasOpen)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static Microsoft.Data.SqlClient.SqlConnection CreateMasterConnection()
    {
        return new Microsoft.Data.SqlClient.SqlConnection(
            new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = RelationalSafetyGuard.ApprovedServer,
                InitialCatalog = "master",
                IntegratedSecurity = true,
                ConnectTimeout = 15
            }.ConnectionString);
    }

    private static Microsoft.Data.SqlClient.SqlConnection CreateOwnedConnection(string databaseName)
    {
        // Re-validate through the accepted T01 guard before touching the target.
        RelationalSafetyGuard.ValidateTarget(
            RelationalSafetyGuard.ApprovedServer,
            databaseName);

        return new Microsoft.Data.SqlClient.SqlConnection(
            new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = RelationalSafetyGuard.ApprovedServer,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                ConnectTimeout = 15
            }.ConnectionString);
    }

    private static string SqlLiteralEscape(string databaseName)
    {
        return databaseName.Replace("'", "''", StringComparison.Ordinal);
    }
}
