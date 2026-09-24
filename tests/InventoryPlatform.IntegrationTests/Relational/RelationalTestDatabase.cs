using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.IntegrationTests.Relational;

/// <summary>
/// Sprint 18 T01 — disposable SQL Server (LocalDB) test database lifecycle helper.
///
/// Each instance owns exactly one uniquely named, guard-validated LocalDB database
/// (database-per-test isolation). The lifecycle:
///
/// <list type="number">
/// <item>generates a unique <c>InventoryPlatformRelationalTests_&lt;guid&gt;</c> name;</item>
/// <item>runs the fail-closed <see cref="RelationalSafetyGuard"/> validation BEFORE any
/// connection is opened (fail-hard: no skip, no fallback);</item>
/// <item>creates the empty database on the approved LocalDB instance (a real connection
/// proving LocalDB reachability — LocalDB instances auto-start on first connection);</item>
/// <item>exposes guard-validated <see cref="DbContextOptions{ TContext }"/> and a
/// connection string for the relational contract tasks (T02–T05);</item>
/// <item>on disposal, attempts a best-effort drop of the owned database.</item>
/// </list>
///
/// Safety derives from unique identity + positive validation + ownership — never from
/// assuming cleanup succeeds. A failed drop is captured and reported (never silently
/// swallowed) but cannot enable database reuse because every name is unique per instance.
///
/// This helper does NOT execute the application migration chain: T02 owns the R1
/// real-migration proof. T01 exercises only the minimal lifecycle behavior needed to
/// prove the infrastructure works.
///
/// SQL identifiers cannot be command parameters, so DDL uses bracket-escaped
/// identifiers and quoted literals (both escaping helpers applied defensively); the
/// names being escaped were themselves produced by the guard-validated construction.
///
/// Default xUnit parallelism is preserved: every instance owns an independent database,
/// so concurrently executing tests share no database state.
/// </summary>
public sealed class RelationalTestDatabase : IAsyncDisposable
{
    private readonly string _connectionString;
    private bool _disposed;

    private RelationalTestDatabase(string databaseName, string connectionString)
    {
        DatabaseName = databaseName;
        _connectionString = connectionString;
    }

    /// <summary>The unique disposable test database name owned by this instance.</summary>
    public string DatabaseName { get; }

    /// <summary>The guard-validated connection string for the owned database.</summary>
    public string ConnectionString => _connectionString;

    /// <summary>Measured LocalDB create time, for runtime evidence only.</summary>
    public long CreatedElapsedMilliseconds { get; private set; }

    /// <summary>The last cleanup error, if any. Null when cleanup succeeded (or was not yet attempted).</summary>
    public Exception? LastCleanupError { get; private set; }

    /// <summary>
    /// Creates a new relational test database lifecycle: generates a unique disposable
    /// name, validates the target fail-closed, creates the empty LocalDB database, and
    /// returns the owned lifecycle instance.
    /// </summary>
    public static async Task<RelationalTestDatabase> CreateAsync(
        CancellationToken cancellationToken = default)
    {
        var databaseName = RelationalSafetyGuard.GenerateDatabaseName();

        // Fail-closed validation happens BEFORE any connection is opened.
        var connectionString =
            RelationalSafetyGuard.BuildConnectionString(databaseName);

        var lifecycle = new RelationalTestDatabase(databaseName, connectionString);

        await lifecycle.CreateDatabaseAsync(cancellationToken);

        return lifecycle;
    }

    /// <summary>
    /// Guard-validated SQL Server options for <see cref="ApplicationDbContext"/>
    /// against the owned database.
    /// </summary>
    public DbContextOptions<ApplicationDbContext> CreateContextOptions()
    {
        RelationalSafetyGuard.ValidateConnectionString(_connectionString);

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .Options;
    }

    /// <summary>
    /// Creates a new <see cref="ApplicationDbContext"/> against the owned database
    /// using guard-validated options. Callers own the returned context's lifetime.
    /// </summary>
    public ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(CreateContextOptions());
    }

    private async Task CreateDatabaseAsync(CancellationToken cancellationToken)
    {
        // Connect to the approved LocalDB server (master) and create the owned empty
        // database. This is the minimal real LocalDB operation proving the lifecycle
        // works; it does not run the application migration chain (T02/R1 owns that).
        // The derived master connection string is re-validated so the server remains
        // the approved instance before anything destructive can occur.
        var masterConnectionString = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        RelationalSafetyGuard.ValidateConnectionString(_connectionString);

        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE [{BracketEscape(DatabaseName)}]";

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await command.ExecuteNonQueryAsync(cancellationToken);
        stopwatch.Stop();

        CreatedElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
    }

    /// <summary>
    /// Best-effort drop of the owned database. The drop target is always the instance's
    /// own guard-validated name — cleanup can never target any database other than the
    /// one this instance created. Drop failure is captured in
    /// <see cref="LastCleanupError"/> (never silently swallowed) but does not throw:
    /// safety does not depend on cleanup success, and a best-effort contract must not
    /// mask the test result that cleanup accompanies.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Re-validate immediately before the destructive operation — the guard runs
        // again so cleanup can only ever target an approved disposable identity.
        try
        {
            RelationalSafetyGuard.ValidateConnectionString(_connectionString);

            var masterConnectionString = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString;

            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText =
                $"IF DB_ID('{SqlLiteralEscape(DatabaseName)}') IS NOT NULL " +
                $"BEGIN " +
                $"ALTER DATABASE [{BracketEscape(DatabaseName)}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                $"DROP DATABASE [{BracketEscape(DatabaseName)}]; " +
                $"END";

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception exception)
        {
            // Best-effort cleanup: report, do not throw. Unique identity means a
            // lingering database cannot be reused by any other test instance.
            LastCleanupError = exception;
        }
    }

    private static string BracketEscape(string databaseName)
    {
        return databaseName.Replace("]", "]]", StringComparison.Ordinal);
    }

    private static string SqlLiteralEscape(string databaseName)
    {
        return databaseName.Replace("'", "''", StringComparison.Ordinal);
    }
}
