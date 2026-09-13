using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MusicEncyclopedia.Web.Tests;

public sealed class StartupTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Fact]
    public async Task LiveHealthEndpoint_StartsAgainstIsolatedMigratedDatabase()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.DatabaseName.Should().StartWith("MusicEncyclopedia_Test_");
    }

    [Fact]
    public async Task Root_RedirectsToDefaultCulture()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.OriginalString.Should().Be("/fa");
    }
}
