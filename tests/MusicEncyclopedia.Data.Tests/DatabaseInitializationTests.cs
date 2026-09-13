using Microsoft.EntityFrameworkCore;

namespace MusicEncyclopedia.Data.Tests;

[Collection(SqlServerDatabaseCollection.Name)]
public sealed class DatabaseInitializationTests(SqlServerDatabaseFixture fixture)
{
    [Fact]
    public async Task Migrations_CreateDomainIdentityAndSeedTables()
    {
        await using var context = fixture.CreateContext();

        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

        appliedMigrations.Should().Contain("20260722105032_InitialCreate");
        appliedMigrations.Should().Contain("20260812134125_AddCurrentModelTables");
        (await context.EntityTypes.CountAsync()).Should().BeGreaterThan(0);
        (await context.AlbumCategories.CountAsync()).Should().BeGreaterThan(0);

        var identityTableExists = await context.Database
            .SqlQueryRaw<int>("SELECT COUNT(*) AS [Value] FROM sys.tables WHERE name = 'AspNetUsers'")
            .SingleAsync();
        identityTableExists.Should().Be(1);
    }
}
