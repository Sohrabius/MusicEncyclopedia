using System.Net;
using MusicEncyclopedia.Core.Constants;

namespace MusicEncyclopedia.Web.Tests;

public sealed class CacheFreshnessTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Fact]
    public async Task WarmAlbumResponses_RefreshAfterEditDeleteAndRestore()
    {
        var album = await factory.SeedActiveAlbumAsync();
        var updatedTitle = $"Updated cache title {Guid.NewGuid():N}";
        var credentials = await factory.CreateUserAsync(
            PermissionConstants.CanManageAlbums,
            PermissionConstants.CanDeleteContent);
        using var publicClient = CreateClientWithoutRedirects();
        using var adminClient = CreateClientWithoutRedirects();
        await adminClient.LoginAsync(credentials.Email, credentials.Password);

        await AssertAlbumVisibleAsync(publicClient, album.Slug, album.Title);
        var warmHome = await publicClient.GetStringAsync("/fa/home/albums?page=1");
        warmHome.Should().Contain(album.Title);

        var editToken = await adminClient.GetAntiForgeryTokenAsync($"/admin/albums/{album.AlbumId}/Edit");
        var editResponse = await adminClient.PostAsync(
            $"/admin/albums/{album.AlbumId}/Edit",
            AlbumForm(album, updatedTitle, editToken));
        editResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        editResponse.Headers.Location?.OriginalString.Should().Contain("/Edit");
        (await factory.GetAlbumStateAsync(album.AlbumId)).Title.Should().Be(updatedTitle);

        await AssertAlbumVisibleAsync(publicClient, album.Slug, updatedTitle);
        var refreshedHome = await publicClient.GetStringAsync("/fa/home/albums?page=1");
        refreshedHome.Should().Contain(updatedTitle).And.NotContain(album.Title);

        var deleteToken = await adminClient.GetAntiForgeryTokenAsync($"/admin/albums/{album.AlbumId}/Edit");
        var deleteResponse = await adminClient.PostAsync(
            $"/admin/albums/{album.AlbumId}/Delete",
            TokenForm(deleteToken));
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        (await publicClient.GetAsync($"/fa/albums/{album.Slug}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await publicClient.GetAsync($"/api/v1/albums/{album.Slug}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await publicClient.GetStringAsync("/fa/home/albums?page=1"))
            .Should().NotContain(updatedTitle);

        var restoreToken = await adminClient.GetAntiForgeryTokenAsync("/admin/albums?deleted=true");
        var restoreResponse = await adminClient.PostAsync(
            $"/admin/albums/{album.AlbumId}/Restore",
            TokenForm(restoreToken));
        restoreResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        await AssertAlbumVisibleAsync(publicClient, album.Slug, updatedTitle);
        (await publicClient.GetStringAsync("/fa/home/albums?page=1"))
            .Should().Contain(updatedTitle);
    }

    private static async Task AssertAlbumVisibleAsync(HttpClient client, string slug, string title)
    {
        var pageResponse = await client.GetAsync($"/fa/albums/{slug}");
        var apiResponse = await client.GetAsync($"/api/v1/albums/{slug}");

        pageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await pageResponse.Content.ReadAsStringAsync()).Should().Contain(title);
        (await apiResponse.Content.ReadAsStringAsync()).Should().Contain(title);
    }

    private static FormUrlEncodedContent AlbumForm(
        AlbumTestRecord album,
        string title,
        string token) => new(new Dictionary<string, string>
    {
        ["AlbumId"] = album.AlbumId.ToString(),
        ["Title"] = title,
        ["Slug"] = album.Slug,
        ["AlbumCategoryId"] = album.CategoryId.ToString(),
        ["RowVersion"] = Convert.ToBase64String(album.RowVersion),
        ["__RequestVerificationToken"] = token
    });

    private static FormUrlEncodedContent TokenForm(string token) => new(new Dictionary<string, string>
    {
        ["__RequestVerificationToken"] = token
    });

    private HttpClient CreateClientWithoutRedirects() => factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
        HandleCookies = true
    });
}
