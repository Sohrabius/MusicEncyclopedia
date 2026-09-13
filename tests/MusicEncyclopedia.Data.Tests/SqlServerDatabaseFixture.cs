using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Seed;
using MusicEncyclopedia.TestInfrastructure;

namespace MusicEncyclopedia.Data.Tests;

public sealed class SqlServerDatabaseFixture : IAsyncLifetime
{
    public SqlServerTestDatabase Database { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Database = await SqlServerTestDatabase.CreateAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
        await DatabaseInitializer.InitializeAsync(context, seedSampleContent: false);
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(Database.ConnectionString)
            .Options;
        return new AppDbContext(options);
    }

    public async Task DisposeAsync() => await Database.DisposeAsync();
}

[CollectionDefinition(Name)]
public sealed class SqlServerDatabaseCollection : ICollectionFixture<SqlServerDatabaseFixture>
{
    public const string Name = "SQL Server database";
}
