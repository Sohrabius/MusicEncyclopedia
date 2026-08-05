using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MusicEncyclopedia.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=MusicEncyclopedia;Trusted_Connection=True;TrustServerCertificate=True;",
            sqlOptions => sqlOptions.CommandTimeout(300));

        return new AppDbContext(optionsBuilder.Options);
    }
}
