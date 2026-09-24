using Microsoft.Data.SqlClient;

namespace InventoryPlatform.IntegrationTests.Relational;

/// <summary>
/// Sprint 18 T01 — fail-closed safety guard for the SQL Server relational test tier.
///
/// Positively validates a relational test target BEFORE any connection, migration,
/// creation, or drop is attempted. Validation is fail-hard: an unsafe or unexpected
/// target throws <see cref="InvalidOperationException"/> and no lifecycle operation
/// can proceed.
///
/// Frozen safety contract (Sprint 18 Scope Freeze §6):
/// - the approved server is exactly the accepted LocalDB instance
///   <c>(localdb)\MSSQLLocalDB</c>;
/// - every test database name begins with the exact disposable-test prefix
///   <c>InventoryPlatformRelationalTests_</c> followed by a unique identifier;
/// - the normal development database <c>InventoryPlatform</c> can never pass;
/// - the application <c>DefaultConnection</c> is never read or reused — connection
///   strings are constructed from test-owned constants only;
/// - no credentials or secrets are involved (LocalDB trusted authentication).
/// </summary>
public static class RelationalSafetyGuard
{
    /// <summary>The only approved relational test server for Sprint 18.</summary>
    public const string ApprovedServer = "(localdb)\\MSSQLLocalDB";

    /// <summary>
    /// The exact disposable-test database-name prefix. Any name not starting with
    /// this prefix is rejected before it can be connected to, migrated, or dropped.
    /// </summary>
    public const string DisposableDatabasePrefix = "InventoryPlatformRelationalTests_";

    /// <summary>The normal development database, which must never be a test target.</summary>
    public const string DevelopmentDatabaseName = "InventoryPlatform";

    /// <summary>
    /// Validates a (server, database) target and throws when it is not an approved,
    /// disposable relational test target.
    /// </summary>
    /// <param name="server">The SQL Server instance name the target lives on.</param>
    /// <param name="databaseName">The database name the lifecycle wants to use.</param>
    /// <exception cref="ArgumentException">A required value is null/whitespace.</exception>
    /// <exception cref="InvalidOperationException">
    /// The target is not an approved disposable test target (server, prefix, or
    /// development-database rule violated).
    /// </exception>
    public static void ValidateTarget(string server, string databaseName)
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            throw new ArgumentException(
                "Relational test server must be provided.",
                nameof(server));
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException(
                "Relational test database name must be provided.",
                nameof(databaseName));
        }

        if (!string.Equals(
                server,
                ApprovedServer,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Relational test target server '{server}' is not the approved " +
                $"LocalDB test server '{ApprovedServer}'.");
        }

        // Development-database rejection is checked BEFORE the prefix rule so the
        // intent is observable on its own: the normal development database can never
        // be a test target, independent of any other rule.
        if (string.Equals(
                databaseName,
                DevelopmentDatabaseName,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Relational tests must never target the development database " +
                $"'{DevelopmentDatabaseName}'.");
        }

        if (!databaseName.StartsWith(
                DisposableDatabasePrefix,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Relational test database name '{databaseName}' does not begin " +
                $"with the required disposable-test prefix " +
                $"'{DisposableDatabasePrefix}'.");
        }

        // The disposable identity must continue beyond the prefix: a bare-prefix or
        // whitespace-suffix name is not a uniquely owned disposable database.
        var uniqueIdentifier = databaseName[DisposableDatabasePrefix.Length..];

        if (string.IsNullOrWhiteSpace(uniqueIdentifier))
        {
            throw new InvalidOperationException(
                $"Relational test database name '{databaseName}' must include a " +
                "unique identifier after the disposable-test prefix.");
        }

        if (uniqueIdentifier.Any(char.IsWhiteSpace))
        {
            throw new InvalidOperationException(
                $"Relational test database name '{databaseName}' must not contain " +
                "whitespace in its unique identifier.");
        }
    }

    /// <summary>
    /// Validates the full target carried by a SQL Server connection string by parsing
    /// it (no fragile substring checks) and applying <see cref="ValidateTarget"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The connection string is unparseable or its target is unapproved.
    /// </exception>
    public static void ValidateConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        ValidateTarget(
            builder.DataSource,
            builder.InitialCatalog);
    }

    /// <summary>
    /// Constructs the test-owned connection string for an approved disposable test
    /// database. The application <c>DefaultConnection</c> is never consulted; no
    /// credentials are involved (LocalDB trusted authentication).
    /// </summary>
    public static string BuildConnectionString(string databaseName)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = ApprovedServer,
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 15,
            MultipleActiveResultSets = false
        };

        var connectionString = builder.ConnectionString;

        // Fail closed on our own construction: the guard must accept every connection
        // string this type produces, or the type is misconfigured.
        ValidateConnectionString(connectionString);

        return connectionString;
    }

    /// <summary>
    /// Generates a unique, guard-valid disposable test database name
    /// (<c>InventoryPlatformRelationalTests_&lt;guid&gt;</c>).
    /// </summary>
    public static string GenerateDatabaseName()
    {
        return $"{DisposableDatabasePrefix}{Guid.NewGuid():N}";
    }
}
