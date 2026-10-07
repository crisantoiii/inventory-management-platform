using System.Diagnostics;
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
        await EnsureLocalDbInstanceAsync(cancellationToken);

        for (var attempt = 1; ; attempt++)
        {
            try
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

                break;
            }
            catch (SqlException exception) when (attempt < DropAttempts && IsTransientDatabaseCreateFailure(exception))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250 * attempt), cancellationToken);
            }
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

    private static async Task EnsureLocalDbInstanceAsync(CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "sqllocaldb",
            Arguments = "info MSSQLLocalDB",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Could not query the LocalDB instance state.");

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode == 0 && stdout.Contains("MSSQLLocalDB", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "sqllocaldb",
            Arguments = "start MSSQLLocalDB",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var startProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the LocalDB instance.");

        var startStdout = await startProcess.StandardOutput.ReadToEndAsync(cancellationToken);
        var startStderr = await startProcess.StandardError.ReadToEndAsync(cancellationToken);
        await startProcess.WaitForExitAsync(cancellationToken);

        if (startProcess.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Could not start the LocalDB instance for browser tests.{Environment.NewLine}stdout:{Environment.NewLine}{startStdout}{Environment.NewLine}stderr:{Environment.NewLine}{startStderr}");
        }
    }

    private static bool IsTransientDatabaseCreateFailure(SqlException exception) =>
        exception.Number is 40615 or 18456 or 4060 or 53 or 1205 or 701;

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
