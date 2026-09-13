using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace MusicEncyclopedia.TestInfrastructure;

/// <summary>
/// Owns one disposable SQL Server database. On Windows it uses LocalDB; in CI
/// it starts a SQL Server container. TEST_SQLSERVER_CONNECTION_STRING can point
/// at another disposable-capable server, while the generated database name
/// still prevents writes to the configured catalog.
/// </summary>
public sealed class SqlServerTestDatabase : IAsyncDisposable
{
    private const string DatabasePrefix = "MusicEncyclopedia_Test_";
    private readonly MsSqlContainer? _container;
    private bool _disposed;

    private SqlServerTestDatabase(string databaseName, string connectionString, MsSqlContainer? container)
    {
        DatabaseName = databaseName;
        ConnectionString = connectionString;
        _container = container;
    }

    public string DatabaseName { get; }
    public string ConnectionString { get; }

    public static async Task<SqlServerTestDatabase> CreateAsync(CancellationToken cancellationToken = default)
    {
        var databaseName = DatabasePrefix + Guid.NewGuid().ToString("N");
        MsSqlContainer? container = null;
        string serverConnectionString;

        var configuredServer = Environment.GetEnvironmentVariable("TEST_SQLSERVER_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(configuredServer))
        {
            serverConnectionString = configuredServer;
        }
        else if (OperatingSystem.IsWindows())
        {
            serverConnectionString =
                "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";
        }
        else
        {
            container = new MsSqlBuilder()
                .WithCleanUp(true)
                .Build();
            await container.StartAsync(cancellationToken);
            serverConnectionString = container.GetConnectionString();
        }

        var masterBuilder = new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = "master",
            TrustServerCertificate = true
        };

        await using (var connection = new SqlConnection(masterBuilder.ConnectionString))
        {
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE [{databaseName}]";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        var databaseBuilder = new SqlConnectionStringBuilder(masterBuilder.ConnectionString)
        {
            InitialCatalog = databaseName,
            MultipleActiveResultSets = true
        };

        return new SqlServerTestDatabase(databaseName, databaseBuilder.ConnectionString, container);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (!DatabaseName.StartsWith(DatabasePrefix, StringComparison.Ordinal) ||
            !Guid.TryParseExact(DatabaseName[DatabasePrefix.Length..], "N", out _))
        {
            throw new InvalidOperationException($"Refusing to delete unexpected database '{DatabaseName}'.");
        }

        SqlConnection.ClearAllPools();
        var masterBuilder = new SqlConnectionStringBuilder(ConnectionString) { InitialCatalog = "master" };
        await using (var connection = new SqlConnection(masterBuilder.ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"""
                IF DB_ID(N'{DatabaseName}') IS NOT NULL
                BEGIN
                    ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{DatabaseName}];
                END
                """;
            await command.ExecuteNonQueryAsync();
        }

        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }
}
