using InventoryPlatform.Infrastructure.Persistence.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryPlatform.BrowserTests.Infrastructure;

public sealed class DatabaseFixture : IAsyncDisposable
{
    private const int DropAttempts = 5;
    private readonly string _masterConnectionString;
    private bool _databaseCreated;
    private int _disposed;

    private DatabaseFixture(string runId)
    {
        RunId = runId;
        DatabaseName = $"InventoryPlatform_BrowserSmoke_{runId}";

        var connectionBuilder = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = DatabaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            Pooling = false,
            ConnectTimeout = 30,
            ApplicationName = $"InventoryPlatformBrowserTests_{runId}"
        };

        ConnectionString = connectionBuilder.ConnectionString;
        connectionBuilder.InitialCatalog = "master";
        _masterConnectionString = connectionBuilder.ConnectionString;
    }

    public string RunId { get; }

    public string DatabaseName { get; }

    internal string ConnectionString { get; }

    public static async Task<DatabaseFixture> CreateAsync(
        string runId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(runId);
        if (runId.Any(character => !char.IsAsciiLetterOrDigit(character)))
        {
            throw new ArgumentException(
                "The browser run ID may contain only ASCII letters and digits.",
                nameof(runId));
        }

        var fixture = new DatabaseFixture(runId);
        await fixture.CreateAndMigrateAsync(cancellationToken);
        return fixture;
    }

    internal ApplicationDbContext CreateDbContext()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                ConnectionString,
                sql => sql.MigrationsAssembly(
                    typeof(ApplicationDbContext).Assembly.GetName().Name))
            .Options;

        return new ApplicationDbContext(options);
    }

    private async Task CreateAndMigrateAsync(
        CancellationToken cancellationToken)
    {
        await using (var connection = new SqlConnection(_masterConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE {QuoteIdentifier(DatabaseName)};";
            command.CommandTimeout = 30;
            await command.ExecuteNonQueryAsync(cancellationToken);
            _databaseCreated = true;
        }

        try
        {
            await using var context = CreateDbContext();
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception migrationException)
        {
            try
            {
                await DropDatabaseAsync(CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                throw new AggregateException(
                    $"Applying the existing EF migration chain failed for " +
                    $"LocalDB database '{DatabaseName}', and cleanup also failed.",
                    migrationException,
                    cleanupException);
            }

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0 || !_databaseCreated)
        {
            return;
        }

        await DropDatabaseAsync(CancellationToken.None);
    }

    private async Task DropDatabaseAsync(CancellationToken cancellationToken)
    {
        Exception? finalException = null;

        for (var attempt = 1; attempt <= DropAttempts; attempt++)
        {
            SqlConnection.ClearAllPools();

            try
            {
                await using var connection = new SqlConnection(_masterConnectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = connection.CreateCommand();
                var quotedName = QuoteIdentifier(DatabaseName);
                var databaseLiteral = DatabaseName.Replace("'", "''", StringComparison.Ordinal);
                command.CommandText =
                    $"IF DB_ID(N'{databaseLiteral}') IS NOT NULL " +
                    $"BEGIN " +
                    $"ALTER DATABASE {quotedName} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                    $"DROP DATABASE {quotedName}; " +
                    $"END;";
                command.CommandTimeout = 30;
                await command.ExecuteNonQueryAsync(cancellationToken);

                _databaseCreated = false;
                return;
            }
            catch (SqlException exception) when (attempt < DropAttempts)
            {
                finalException = exception;
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
            catch (SqlException exception)
            {
                throw new InvalidOperationException(
                    $"Failed to drop disposable LocalDB database '{DatabaseName}' " +
                    $"after {DropAttempts} attempts.",
                    exception);
            }
        }

        throw new InvalidOperationException(
            $"Failed to drop disposable LocalDB database '{DatabaseName}'.",
            finalException);
    }

    private static string QuoteIdentifier(string identifier) =>
        $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";
}
