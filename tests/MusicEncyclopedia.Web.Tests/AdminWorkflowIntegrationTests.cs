using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.Tests;

public sealed class AdminWorkflowIntegrationTests(MusicEncyclopediaWebFactory factory)
    : IClassFixture<MusicEncyclopediaWebFactory>
{
    [Fact]
    public async Task AlbumWizard_PersistsCompleteGraphWithGlobalEntityAssignments()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        var ids = await factory.GetWizardReferenceIdsAsync(catalog);
        var slug = $"wizard-{Guid.NewGuid():N}";
        var credentials = await factory.CreateUserAsync(
            PermissionConstants.CanManageAlbums,
            PermissionConstants.CanManageTracks);
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);
        var token = await client.GetAntiForgeryTokenAsync("/admin/albums/wizard");

        var response = await client.PostAsync("/admin/albums/wizard",
            WizardForm(slug, ids, catalog.AlbumId, token));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location?.OriginalString.Should().MatchRegex(@"/admin/albums/\d+/Edit");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var album = await db.Albums
            .Include(x => x.AlbumGenres)
            .Include(x => x.AlbumMoods)
            .Include(x => x.AlbumCompanies)
            .Include(x => x.AlbumIdentifiers)
            .Include(x => x.AlbumTracks).ThenInclude(x => x.Track).ThenInclude(x => x.TrackGenres)
            .Include(x => x.AlbumTracks).ThenInclude(x => x.Track).ThenInclude(x => x.TrackMoods)
            .Include(x => x.AlbumTracks).ThenInclude(x => x.Track).ThenInclude(x => x.TrackInstruments)
            .Include(x => x.AlbumTracks).ThenInclude(x => x.Track).ThenInclude(x => x.TrackSungVersions)
            .SingleAsync(x => x.Slug == slug);

        album.AlbumGenres.Should().ContainSingle(x => x.GenreId == ids.GenreId);
        album.AlbumMoods.Should().ContainSingle(x => x.MoodId == ids.MoodId);
        album.AlbumCompanies.Should().ContainSingle(x => x.CompanyId == ids.CompanyId);
        album.AlbumIdentifiers.Should().ContainSingle(x => x.Value == $"CAT-{slug}");
        album.AlbumTracks.Should().HaveCount(2);
        album.AlbumTracks.Select(x => x.DiscNumber).Should().Equal(1, 2);
        album.AlbumTracks.Select(x => x.SequenceNumber).Should().Equal(1, 1);
        var firstTrack = album.AlbumTracks.OrderBy(x => x.DiscNumber).First().Track;
        firstTrack.TrackGenres.Should().ContainSingle(x => x.GenreId == ids.GenreId);
        firstTrack.TrackMoods.Should().ContainSingle(x => x.MoodId == ids.MoodId);
        firstTrack.TrackInstruments.Should().ContainSingle(x => x.InstrumentId == ids.InstrumentId);
        firstTrack.TrackSungVersions.Should().ContainSingle();

        (await db.Credits.CountAsync(x => x.EntityId == album.EntityId)).Should().Be(1);
        (await db.Credits.CountAsync(x => x.EntityId == firstTrack.EntityId
            && x.CreditRoleId == ids.MusicianCreditRoleId
            && x.InstrumentId == ids.InstrumentId)).Should().Be(1);
        (await db.TagAssignments.CountAsync(x => x.EntityId == album.EntityId && x.TagId == ids.TagId)).Should().Be(1);
        (await db.EntityLinks.CountAsync(x => x.EntityId == album.EntityId && x.Url == "https://example.test/wizard")).Should().Be(1);
        (await db.AlbumRelations.CountAsync(x =>
            (x.AlbumId == album.AlbumId && x.RelatedAlbumId == catalog.AlbumId)
            || (x.AlbumId == catalog.AlbumId && x.RelatedAlbumId == album.AlbumId))).Should().Be(2);

        var publicHtml = await client.GetStringAsync($"/fa/albums/{slug}");
        publicHtml.Should().Contain("Wizard album").And.Contain("https://example.test/wizard");
    }

    [Fact]
    public async Task AlbumWizard_LateForeignKeyFailure_RollsBackEntireGraph()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        var ids = await factory.GetWizardReferenceIdsAsync(catalog);
        var slug = $"wizard-rollback-{Guid.NewGuid():N}";
        var credentials = await factory.CreateUserAsync(PermissionConstants.CanManageAlbums);
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);
        var token = await client.GetAntiForgeryTokenAsync("/admin/albums/wizard");
        var form = WizardFields(slug, ids, catalog.AlbumId, token);
        form["Links[0].LinkTypeId"] = int.MaxValue.ToString();

        var response = await client.PostAsync("/admin/albums/wizard", new FormUrlEncodedContent(form));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Wizard album");
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Albums.AnyAsync(x => x.Slug == slug)).Should().BeFalse();
        (await db.Tracks.AnyAsync(x => x.Slug == $"{slug}-track-1")).Should().BeFalse();
        (await db.Entities.AnyAsync(x => x.Slug == slug || x.Slug == $"{slug}-track-1")).Should().BeFalse();
    }

    [Fact]
    public async Task AlbumEdit_WithStaleRowVersion_RejectsSecondWriter()
    {
        var album = await factory.SeedActiveAlbumAsync();
        var credentials = await factory.CreateUserAsync(PermissionConstants.CanManageAlbums);
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);

        var firstTitle = $"First writer {Guid.NewGuid():N}";
        var firstToken = await client.GetAntiForgeryTokenAsync($"/admin/albums/{album.AlbumId}/Edit");
        var first = await client.PostAsync($"/admin/albums/{album.AlbumId}/Edit",
            AlbumForm(album, firstTitle, firstToken));
        first.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var staleTitle = $"Stale writer {Guid.NewGuid():N}";
        var staleToken = await client.GetAntiForgeryTokenAsync($"/admin/albums/{album.AlbumId}/Edit");
        var stale = await client.PostAsync($"/admin/albums/{album.AlbumId}/Edit",
            AlbumForm(album, staleTitle, staleToken));

        stale.StatusCode.Should().Be(HttpStatusCode.OK);
        WebUtility.HtmlDecode(await stale.Content.ReadAsStringAsync()).Should().Contain("توسط کاربر دیگری تغییر کرده است");
        (await factory.GetAlbumStateAsync(album.AlbumId)).Title.Should().Be(firstTitle);
    }

    [Fact]
    public async Task AlbumTracklist_AddUpdateMoveDuplicateAndRemove_Persist()
    {
        var album = await factory.SeedActiveAlbumAsync();
        var firstCatalog = await factory.SeedPublicCatalogAsync();
        var secondCatalog = await factory.SeedPublicCatalogAsync();
        var credentials = await factory.CreateUserAsync(
            PermissionConstants.CanManageAlbums,
            PermissionConstants.CanDeleteContent);
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);
        var token = await client.GetAntiForgeryTokenAsync($"/admin/albums/{album.AlbumId}/Edit");
        var firstTrackId = await TrackIdAsync(firstCatalog.TrackSlug);
        var secondTrackId = await TrackIdAsync(secondCatalog.TrackSlug);

        (await PostJsonAsync(client, "/admin/album-tracklist/add-existing", new()
        {
            ["albumId"] = album.AlbumId.ToString(), ["trackId"] = firstTrackId.ToString(),
            ["discNumber"] = "1", ["trackNumber"] = "1", ["__RequestVerificationToken"] = token
        })).GetProperty("success").GetBoolean().Should().BeTrue();
        (await PostJsonAsync(client, "/admin/album-tracklist/add-existing", new()
        {
            ["albumId"] = album.AlbumId.ToString(), ["trackId"] = firstTrackId.ToString(),
            ["__RequestVerificationToken"] = token
        })).GetProperty("success").GetBoolean().Should().BeFalse();
        (await PostJsonAsync(client, "/admin/album-tracklist/add-existing", new()
        {
            ["albumId"] = album.AlbumId.ToString(), ["trackId"] = secondTrackId.ToString(),
            ["discNumber"] = "1", ["trackNumber"] = "2", ["__RequestVerificationToken"] = token
        })).GetProperty("success").GetBoolean().Should().BeTrue();

        var rows = await TracklistRowsAsync(album.AlbumId);
        var firstRow = rows.Single(x => x.TrackId == firstTrackId);
        var secondRow = rows.Single(x => x.TrackId == secondTrackId);
        (await PostJsonAsync(client, "/admin/album-tracklist/update", new()
        {
            ["albumTrackId"] = firstRow.AlbumTrackId.ToString(), ["discNumber"] = "1",
            ["trackNumber"] = "7", ["durationOverrideSeconds"] = "321",
            ["titleOverride"] = "Edited title", ["isBonus"] = "true", ["isHidden"] = "true",
            ["__RequestVerificationToken"] = token
        })).GetProperty("success").GetBoolean().Should().BeTrue();
        (await PostJsonAsync(client, $"/admin/album-tracklist/move/{secondRow.AlbumTrackId}/up", new()
        {
            ["__RequestVerificationToken"] = token
        })).GetProperty("success").GetBoolean().Should().BeTrue();

        rows = await TracklistRowsAsync(album.AlbumId);
        rows.OrderBy(x => x.SequenceNumber).Select(x => x.TrackId).Should().Equal(secondTrackId, firstTrackId);
        var updated = rows.Single(x => x.TrackId == firstTrackId);
        updated.TrackNumber.Should().Be(7);
        updated.TitleOverride.Should().Be("Edited title");
        updated.DurationOverride.Should().Be(321);
        updated.IsBonus.Should().BeTrue();
        updated.IsHidden.Should().BeTrue();

        (await PostJsonAsync(client, $"/admin/album-tracklist/remove/{secondRow.AlbumTrackId}", new()
        {
            ["__RequestVerificationToken"] = token
        })).GetProperty("success").GetBoolean().Should().BeTrue();
        (await TracklistRowsAsync(album.AlbumId)).Should().ContainSingle(x => x.TrackId == firstTrackId);
    }

    [Fact]
    public async Task CreditEditor_EnforcesContributorScopeEntityAndExactDuplicateRules()
    {
        var catalog = await factory.SeedPublicCatalogAsync();
        var ids = await factory.GetWizardReferenceIdsAsync(catalog);
        var entity = await AlbumEntityAsync(catalog.AlbumId);
        var credentials = await factory.CreateUserAsync(
            PermissionConstants.CanManageAlbums,
            PermissionConstants.CanManageTracks);
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);
        var token = await client.GetAntiForgeryTokenAsync($"/admin/albums/{catalog.AlbumId}/Edit");

        var both = CreditFields(entity.EntityTypeId, entity.EntityId, ids, token);
        both["CompanyId"] = ids.CompanyId.ToString();
        (await PostJsonAsync(client, "/admin/credits/save", both))
            .GetProperty("success").GetBoolean().Should().BeFalse();

        var wrongEntity = CreditFields(entity.EntityTypeId, int.MaxValue, ids, token);
        (await PostJsonAsync(client, "/admin/credits/save", wrongEntity))
            .GetProperty("success").GetBoolean().Should().BeFalse();

        var valid = CreditFields(entity.EntityTypeId, entity.EntityId, ids, token);
        (await PostJsonAsync(client, "/admin/credits/save", valid))
            .GetProperty("success").GetBoolean().Should().BeTrue();
        (await PostJsonAsync(client, "/admin/credits/save", valid))
            .GetProperty("success").GetBoolean().Should().BeFalse();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Credits.CountAsync(x => x.EntityId == entity.EntityId
            && x.CreditRoleId == ids.CreditRoleId
            && x.PersonId == ids.PersonId)).Should().Be(1);
    }

    [Fact]
    public async Task MediaAssignment_ValidatesGlobalEntity_AndEmptyUploadCreatesNoRow()
    {
        var album = await factory.SeedActiveAlbumAsync();
        var entity = await AlbumEntityAsync(album.AlbumId);
        int mediaId;
        int mediaCount;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var media = new MusicEncyclopedia.Data.Entities.Media
            {
                FileName = "b07-test.jpg", FilePath = "/test/b07-test.jpg", Url = "/test/b07-test.jpg",
                FileSize = 10, MimeType = "image/jpeg", CreatedAt = DateTime.UtcNow, CreatedBy = "integration-test"
            };
            db.Media.Add(media);
            await db.SaveChangesAsync();
            mediaId = media.MediaId;
            mediaCount = await db.Media.CountAsync();
        }

        var credentials = await factory.CreateUserAsync(PermissionConstants.CanManageMedia);
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);
        var token = await client.GetAntiForgeryTokenAsync($"/admin/media/{mediaId}/Edit");
        var valid = await client.PostAsync("/admin/media/Assign", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["MediaId"] = mediaId.ToString(), ["EntityTypeId"] = entity.EntityTypeId.ToString(),
            ["EntityId"] = entity.EntityId.ToString(), ["IsPrimary"] = "true", ["DisplayOrder"] = "1",
            ["__RequestVerificationToken"] = token
        }));
        valid.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var invalid = await client.PostAsync("/admin/media/Assign", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["MediaId"] = mediaId.ToString(), ["EntityTypeId"] = (entity.EntityTypeId + 1).ToString(),
            ["EntityId"] = entity.EntityId.ToString(), ["DisplayOrder"] = "2",
            ["__RequestVerificationToken"] = token
        }));
        invalid.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var uploadToken = await client.GetAntiForgeryTokenAsync("/admin/media/Upload");
        var upload = await client.PostAsync("/admin/media/Upload", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["FileName"] = "orphan.jpg", ["__RequestVerificationToken"] = uploadToken
        }));
        upload.StatusCode.Should().Be(HttpStatusCode.OK);

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await verifyDb.MediaAssignments.CountAsync(x => x.MediaId == mediaId)).Should().Be(1);
        (await verifyDb.Media.CountAsync()).Should().Be(mediaCount);
    }

    [Fact]
    public async Task AdminInventory_AllPrimaryGetRoutesRenderWithoutServerErrors()
    {
        string[] routes =
        [
            "/admin", "/admin/albums", "/admin/albums/wizard", "/admin/tracks", "/admin/people",
            "/admin/companies", "/admin/poems", "/admin/sung-versions", "/admin/publications",
            "/admin/genres", "/admin/moods", "/admin/instruments", "/admin/sessions", "/admin/events",
            "/admin/locations", "/admin/awards", "/admin/certifications", "/admin/charts", "/admin/media",
            "/admin/sources", "/admin/citations", "/admin/tags", "/admin/attributes", "/admin/localizations",
            "/admin/lookup-tables", "/admin/users", "/admin/roles", "/admin/settings", "/admin/audit-log"
        ];
        var credentials = await factory.CreateUserAsync(PermissionConstants.All.ToArray());
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);

        foreach (var route in routes)
        {
            var response = await client.GetAsync(route);
            response.StatusCode.Should().Be(HttpStatusCode.OK, $"the admin inventory route {route} must render");
        }
    }

    [Fact]
    public async Task AttributeDefinitions_CreateUpdateAndDelete_Persist()
    {
        var album = await factory.SeedActiveAlbumAsync();
        var entity = await AlbumEntityAsync(album.AlbumId);
        var name = $"B07 attribute {Guid.NewGuid():N}";
        var credentials = await factory.CreateUserAsync(
            PermissionConstants.CanManageLookupTables,
            PermissionConstants.CanDeleteContent);
        using var client = CreateClient();
        await client.LoginAsync(credentials.Email, credentials.Password);
        var token = await client.GetAntiForgeryTokenAsync("/admin/attributes");

        var create = await client.PostAsync("/admin/attributes/create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["EntityTypeId"] = entity.EntityTypeId.ToString(), ["Name"] = name,
            ["DataTypeCode"] = "STRING", ["__RequestVerificationToken"] = token
        }));
        create.StatusCode.Should().Be(HttpStatusCode.Redirect);

        int definitionId;
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            definitionId = await db.AttributeDefinitions.Where(x => x.Name == name)
                .Select(x => x.AttributeDefinitionId).SingleAsync();
        }

        var update = await client.PostAsync("/admin/attributes/update", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["AttributeDefinitionId"] = definitionId.ToString(), ["EntityTypeId"] = entity.EntityTypeId.ToString(),
            ["Name"] = name, ["DataTypeCode"] = "INTEGER", ["IsRequired"] = "true",
            ["__RequestVerificationToken"] = token
        }));
        update.StatusCode.Should().Be(HttpStatusCode.Redirect);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var item = await scope.ServiceProvider.GetRequiredService<AppDbContext>().AttributeDefinitions
                .SingleAsync(x => x.AttributeDefinitionId == definitionId);
            item.DataTypeCode.Should().Be("INTEGER");
            item.IsRequired.Should().BeTrue();
        }

        var delete = await client.PostAsync($"/admin/attributes/delete/{definitionId}",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }));
        delete.StatusCode.Should().Be(HttpStatusCode.Redirect);
        await using var verifyScope = factory.Services.CreateAsyncScope();
        (await verifyScope.ServiceProvider.GetRequiredService<AppDbContext>().AttributeDefinitions
            .AnyAsync(x => x.AttributeDefinitionId == definitionId)).Should().BeFalse();
    }

    private static FormUrlEncodedContent WizardForm(string slug, WizardReferenceIds ids, int relatedAlbumId, string token) =>
        new(WizardFields(slug, ids, relatedAlbumId, token));

    private static Dictionary<string, string> WizardFields(string slug, WizardReferenceIds ids, int relatedAlbumId, string token) => new()
    {
        ["Title"] = "Wizard album", ["Slug"] = slug, ["AlbumCategoryId"] = ids.AlbumCategoryId.ToString(),
        ["GenreIds[0]"] = ids.GenreId.ToString(), ["MoodIds[0]"] = ids.MoodId.ToString(),
        ["Companies[0].CompanyId"] = ids.CompanyId.ToString(), ["Companies[0].CompanyRoleTypeId"] = ids.CompanyRoleTypeId.ToString(),
        ["Companies[0].CatalogNumber"] = $"CAT-{slug}", ["Identifiers[0].IdentifierTypeId"] = ids.IdentifierTypeId.ToString(),
        ["Identifiers[0].Value"] = $"CAT-{slug}",
        ["Tracks[0].Title"] = "Wizard track one", ["Tracks[0].Slug"] = $"{slug}-track-1", ["Tracks[0].DiscNumber"] = "1", ["Tracks[0].TrackNumber"] = "1",
        ["Tracks[0].LyricsAvailabilityTypeId"] = ids.PublicLyricsAvailabilityTypeId.ToString(),
        ["Tracks[0].GenreIds[0]"] = ids.GenreId.ToString(), ["Tracks[0].MoodIds[0]"] = ids.MoodId.ToString(), ["Tracks[0].InstrumentIds[0]"] = ids.InstrumentId.ToString(),
        ["Tracks[0].SungVersions[0].Title"] = "Wizard sung version", ["Tracks[0].SungVersions[0].PoemId"] = ids.PoemId.ToString(),
        ["Tracks[0].SungVersions[0].Text"] = "Wizard lyrics", ["Tracks[0].SungVersions[0].IsCanonical"] = "true",
        ["Tracks[0].Credits[0].CreditRoleId"] = ids.MusicianCreditRoleId.ToString(), ["Tracks[0].Credits[0].RoleScopeTypeId"] = ids.PersonScopeTypeId.ToString(),
        ["Tracks[0].Credits[0].PersonId"] = ids.PersonId.ToString(), ["Tracks[0].Credits[0].InstrumentId"] = ids.InstrumentId.ToString(),
        ["Tracks[0].Credits[0].IsPrimary"] = "true",
        ["Tracks[1].Title"] = "Wizard track two", ["Tracks[1].Slug"] = $"{slug}-track-2", ["Tracks[1].DiscNumber"] = "2", ["Tracks[1].TrackNumber"] = "1",
        ["AlbumCredits[0].CreditRoleId"] = ids.CreditRoleId.ToString(), ["AlbumCredits[0].RoleScopeTypeId"] = ids.PersonScopeTypeId.ToString(),
        ["AlbumCredits[0].PersonId"] = ids.PersonId.ToString(), ["AlbumCredits[0].IsPrimary"] = "true",
        ["TagIds[0]"] = ids.TagId.ToString(), ["Links[0].Url"] = "https://example.test/wizard", ["Links[0].LinkTypeId"] = ids.LinkTypeId.ToString(),
        ["Links[0].Label"] = "Wizard link", ["RelatedAlbums[0].AlbumId"] = relatedAlbumId.ToString(),
        ["RelatedAlbums[0].AlbumRelationTypeId"] = ids.AlbumRelationTypeId.ToString(), ["__RequestVerificationToken"] = token
    };

    private static FormUrlEncodedContent AlbumForm(AlbumTestRecord album, string title, string token) => new(new Dictionary<string, string>
    {
        ["AlbumId"] = album.AlbumId.ToString(), ["Title"] = title, ["Slug"] = album.Slug,
        ["AlbumCategoryId"] = album.CategoryId.ToString(), ["RowVersion"] = Convert.ToBase64String(album.RowVersion),
        ["__RequestVerificationToken"] = token
    });

    private static Dictionary<string, string> CreditFields(
        int entityTypeId, int entityId, WizardReferenceIds ids, string token) => new()
    {
        ["entityTypeId"] = entityTypeId.ToString(), ["entityId"] = entityId.ToString(),
        ["CreditRoleId"] = ids.CreditRoleId.ToString(), ["RoleScopeTypeId"] = ids.PersonScopeTypeId.ToString(),
        ["PersonId"] = ids.PersonId.ToString(), ["DisplayOrder"] = "1", ["IsPrimary"] = "true",
        ["__RequestVerificationToken"] = token
    };

    private async Task<EntityContext> AlbumEntityAsync(int albumId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Albums
            .Where(x => x.AlbumId == albumId)
            .Select(x => new EntityContext(x.EntityId, x.Entity.EntityTypeId))
            .SingleAsync();
    }

    private async Task<int> TrackIdAsync(string slug)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Tracks
            .Where(x => x.Slug == slug).Select(x => x.TrackId).SingleAsync();
    }

    private async Task<List<TracklistState>> TracklistRowsAsync(int albumId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().AlbumTracks
            .Where(x => x.AlbumId == albumId)
            .Select(x => new TracklistState(x.AlbumTrackId, x.TrackId, x.TrackNumber, x.SequenceNumber,
                x.TrackTitleOverride, x.DurationSecondsOverride, x.IsBonus, x.IsHidden))
            .ToListAsync();
    }

    private static async Task<JsonElement> PostJsonAsync(HttpClient client, string path, Dictionary<string, string> fields)
    {
        var response = await client.PostAsync(path, new FormUrlEncodedContent(fields));
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private HttpClient CreateClient() => factory.CreateClient(new()
    {
        BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = true
    });

    private sealed record TracklistState(int AlbumTrackId, int TrackId, int TrackNumber, int SequenceNumber,
        string? TitleOverride, int? DurationOverride, bool IsBonus, bool IsHidden);

    private sealed record EntityContext(int EntityId, int EntityTypeId);
}
