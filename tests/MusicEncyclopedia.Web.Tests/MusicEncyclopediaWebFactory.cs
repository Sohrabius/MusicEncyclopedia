using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.TestInfrastructure;
using System.Security.Claims;

namespace MusicEncyclopedia.Web.Tests;

public sealed class MusicEncyclopediaWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqlServerTestDatabase? _database;

    public string DatabaseName => _database?.DatabaseName
        ?? throw new InvalidOperationException("The test database has not been initialized.");

    public async Task<(string Email, string Password)> CreateUserAsync(params string[] permissions)
    {
        _ = Services; // Force application startup and database initialization.
        await using var scope = Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var email = $"test-{Guid.NewGuid():N}@example.test";
        const string password = "Testing-123!";
        var user = new IdentityUser { UserName = email, Email = email };

        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", created.Errors.Select(error => error.Description)));
        }

        foreach (var permission in permissions)
        {
            var claimResult = await users.AddClaimAsync(user, new Claim("Permission", permission));
            if (!claimResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", claimResult.Errors.Select(error => error.Description)));
            }
        }

        return (email, password);
    }

    public async Task<string> SeedLyricsTrackAsync(string availabilityCode, string secretText)
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        var trackEntityTypeId = await context.EntityTypes
            .Where(type => type.Code == "Track")
            .Select(type => type.EntityTypeId)
            .SingleAsync();
        var poemEntityTypeId = await context.EntityTypes
            .Where(type => type.Code == "Poem")
            .Select(type => type.EntityTypeId)
            .SingleAsync();
        var sungVersionEntityTypeId = await context.EntityTypes
            .Where(type => type.Code == "SungVersion")
            .Select(type => type.EntityTypeId)
            .SingleAsync();
        var availabilityId = await context.LyricsAvailabilityTypes
            .Where(type => type.Code == availabilityCode)
            .Select(type => type.LyricsAvailabilityTypeId)
            .SingleAsync();

        var trackEntity = new MusicEncyclopedia.Data.Entities.Entity
        {
            EntityTypeId = trackEntityTypeId,
            Slug = $"track-{suffix}",
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var poemEntity = new MusicEncyclopedia.Data.Entities.Entity
        {
            EntityTypeId = poemEntityTypeId,
            Slug = $"poem-{suffix}",
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var sungVersionEntity = new MusicEncyclopedia.Data.Entities.Entity
        {
            EntityTypeId = sungVersionEntityTypeId,
            Slug = $"sung-version-{suffix}",
            CreatedBy = "integration-test",
            CreatedAt = now
        };

        var track = new Track
        {
            Entity = trackEntity,
            Title = $"Test track {suffix}",
            Slug = trackEntity.Slug,
            LyricsAvailabilityTypeId = availabilityId,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var poem = new Poem
        {
            Entity = poemEntity,
            Title = $"Test poem {suffix}",
            Slug = poemEntity.Slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var sungVersion = new SungVersion
        {
            Entity = sungVersionEntity,
            Poem = poem,
            Title = $"Test sung version {suffix}",
            Slug = sungVersionEntity.Slug,
            Text = secretText,
            IsCanonical = true,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        context.TrackSungVersions.Add(new TrackSungVersion
        {
            Track = track,
            SungVersion = sungVersion,
            SequenceNumber = 1,
            IsPrimary = true
        });
        await context.SaveChangesAsync();
        return track.Slug;
    }

    public async Task<int> GetFirstAlbumCategoryIdAsync()
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .AlbumCategories.Select(category => category.AlbumCategoryId).FirstAsync();
    }

    public async Task<bool> AlbumExistsAsync(string slug)
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .Albums.AnyAsync(album => album.Slug == slug);
    }

    public async Task<string> SeedDeletedAlbumAsync()
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var slug = $"deleted-album-{suffix}";
        var now = DateTime.UtcNow;
        var entityTypeId = await context.EntityTypes
            .Where(type => type.Code == "Album")
            .Select(type => type.EntityTypeId)
            .SingleAsync();
        var categoryId = await context.AlbumCategories
            .Select(category => category.AlbumCategoryId)
            .FirstAsync();
        var entity = new MusicEncyclopedia.Data.Entities.Entity
        {
            EntityTypeId = entityTypeId,
            Slug = slug,
            IsDeleted = true,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        context.Albums.Add(new Album
        {
            Entity = entity,
            AlbumCategoryId = categoryId,
            Title = $"Deleted album {suffix}",
            Slug = slug,
            IsDeleted = true,
            CreatedBy = "integration-test",
            CreatedAt = now
        });
        await context.SaveChangesAsync();
        return slug;
    }

    public async Task<AlbumTestRecord> SeedActiveAlbumAsync()
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var slug = $"cache-album-{suffix}";
        var title = $"Original cache title {suffix}";
        var now = DateTime.UtcNow;
        var entityTypeId = await context.EntityTypes
            .Where(type => type.Code == "Album")
            .Select(type => type.EntityTypeId)
            .SingleAsync();
        var categoryId = await context.AlbumCategories
            .Select(category => category.AlbumCategoryId)
            .FirstAsync();
        var entity = new MusicEncyclopedia.Data.Entities.Entity
        {
            EntityTypeId = entityTypeId,
            Slug = slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var album = new Album
        {
            Entity = entity,
            AlbumCategoryId = categoryId,
            Title = title,
            Slug = slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        context.Albums.Add(album);
        await context.SaveChangesAsync();
        return new AlbumTestRecord(album.AlbumId, categoryId, slug, title, album.RowVersion);
    }

    public async Task<HomeAlbumSet> SeedHomeAlbumsAsync(int count)
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var marker = $"home-{suffix}";
        var now = DateTime.UtcNow;
        var entityTypeId = await context.EntityTypes
            .Where(type => type.Code == "Album")
            .Select(type => type.EntityTypeId)
            .SingleAsync();
        var category = new AlbumCategory
        {
            Code = marker,
            Name = $"Home category {suffix}"
        };
        context.AlbumCategories.Add(category);

        var albums = Enumerable.Range(1, count)
            .Select(index =>
            {
                var slug = $"{marker}-{index:D3}";
                return new Album
                {
                    Entity = new MusicEncyclopedia.Data.Entities.Entity
                    {
                        EntityTypeId = entityTypeId,
                        Slug = slug,
                        CreatedBy = "integration-test",
                        CreatedAt = now
                    },
                    AlbumCategory = category,
                    Title = $"{marker} album {index:D3}",
                    Slug = slug,
                    ReleaseDate = new DateOnly(2000, 1, 1).AddDays(index),
                    CreatedBy = "integration-test",
                    CreatedAt = now
                };
            })
            .ToList();

        context.Albums.AddRange(albums);
        await context.SaveChangesAsync();

        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
        await cache.RemoveAsync(CacheKeys.Home("categories"));
        await cache.RemoveAsync(CacheKeys.Home("about"));

        return new HomeAlbumSet(
            marker,
            category.Code,
            albums.FirstOrDefault()?.AlbumId,
            albums.OrderByDescending(album => album.ReleaseDate)
                .ThenByDescending(album => album.AlbumId)
                .Select(album => album.Slug)
                .ToArray());
    }

    public async Task<string> SeedPlainPoemAsync()
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var slug = $"plain-poem-{suffix}";
        var entityTypeId = await context.EntityTypes
            .Where(type => type.Code == "Poem")
            .Select(type => type.EntityTypeId)
            .SingleAsync();
        context.Poems.Add(new Poem
        {
            Entity = new MusicEncyclopedia.Data.Entities.Entity
            {
                EntityTypeId = entityTypeId,
                Slug = slug,
                CreatedBy = "integration-test",
                CreatedAt = DateTime.UtcNow
            },
            Title = slug,
            Slug = slug,
            CreatedBy = "integration-test",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        return slug;
    }

    public async Task<AlbumDatabaseState> GetAlbumStateAsync(int albumId)
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>()
            .Albums
            .Where(album => album.AlbumId == albumId)
            .Select(album => new AlbumDatabaseState(album.Title, album.IsDeleted, album.RowVersion))
            .SingleAsync();
    }

    public async Task<PublicCatalogTestRecord> SeedPublicCatalogAsync()
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var marker = $"catalog-{suffix}";
        var now = DateTime.UtcNow;
        var entityTypeIds = await context.EntityTypes.ToDictionaryAsync(type => type.Code, type => type.EntityTypeId);

        MusicEncyclopedia.Data.Entities.Entity NewEntity(string type, string slug) => new()
        {
            EntityTypeId = entityTypeIds[type],
            Slug = slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };

        var albumEntity = NewEntity("Album", $"album-{suffix}");
        var trackEntity = NewEntity("Track", $"track-{suffix}");
        var personEntity = NewEntity("Person", $"person-{suffix}");
        var companyEntity = NewEntity("Company", $"company-{suffix}");
        var poemEntity = NewEntity("Poem", $"poem-{suffix}");
        var sungEntity = NewEntity("SungVersion", $"sung-version-{suffix}");
        var genreEntity = NewEntity("Genre", $"genre-{suffix}");
        var moodEntity = NewEntity("Mood", $"mood-{suffix}");
        var instrumentEntity = NewEntity("Instrument", $"instrument-{suffix}");
        var sourceEntity = NewEntity("Source", $"source-{suffix}");
        var locationEntity = NewEntity("Location", $"location-{suffix}");
        var publicationEntity = NewEntity("Publication", $"publication-{suffix}");
        var sessionEntity = NewEntity("RecordingSession", $"session-{suffix}");
        var eventEntity = NewEntity("PerformanceEvent", $"event-{suffix}");
        var awardEntity = NewEntity("Award", $"award-{suffix}");
        var certificationEntity = NewEntity("Certification", $"certification-{suffix}");
        var chartEntity = NewEntity("Chart", $"chart-{suffix}");
        var tagEntity = NewEntity("Tag", $"tag-{suffix}");

        var categoryId = await context.AlbumCategories.Select(x => x.AlbumCategoryId).FirstAsync();
        var personKindId = await context.PersonKinds.Where(x => x.Code == "INDIVIDUAL").Select(x => x.PersonKindId).SingleAsync();
        var mainArtistTypeId = await context.PersonTypes.Where(x => x.Code == "MAIN_ARTIST").Select(x => x.PersonTypeId).SingleAsync();
        var poetTypeId = await context.PersonTypes.Where(x => x.Code == "POET").Select(x => x.PersonTypeId).SingleAsync();
        var creditRoleId = await context.CreditRoles.Where(x => x.Code == "PRIMARY_ARTIST").Select(x => x.CreditRoleId).SingleAsync();
        var personScopeId = await context.RoleScopeTypes.Where(x => x.Code == "Person").Select(x => x.RoleScopeTypeId).SingleAsync();
        var publicLyricsId = await context.LyricsAvailabilityTypes.Where(x => x.Code == "PUBLIC").Select(x => x.LyricsAvailabilityTypeId).SingleAsync();

        var person = new Person
        {
            Entity = personEntity,
            FullName = $"{marker} person",
            Biography = $"Biography for {marker}",
            PersonKindId = personKindId,
            Slug = personEntity.Slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var company = new Company
        {
            Entity = companyEntity,
            Name = $"{marker} company",
            History = $"History for {marker}",
            Slug = companyEntity.Slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var album = new Album
        {
            Entity = albumEntity,
            AlbumCategoryId = categoryId,
            Title = $"{marker} album",
            Description = $"Description for {marker}",
            ReleaseDate = new DateOnly(1997, 6, 15),
            Slug = albumEntity.Slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var track = new Track
        {
            Entity = trackEntity,
            Title = $"{marker} track",
            Description = $"Description for {marker}",
            LyricsAvailabilityTypeId = publicLyricsId,
            Slug = trackEntity.Slug,
            CreatedBy = "integration-test",
            CreatedAt = now
        };
        var genre = new Genre { Entity = genreEntity, Name = $"{marker} genre", Description = marker, Slug = genreEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var mood = new Mood { Entity = moodEntity, Name = $"{marker} mood", Description = marker, Slug = moodEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var instrument = new Instrument { Entity = instrumentEntity, Name = $"{marker} instrument", Description = marker, Slug = instrumentEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var location = new Location { Entity = locationEntity, Name = $"{marker} location", Slug = locationEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var publication = new Publication { Entity = publicationEntity, Person = person, Title = $"{marker} publication", PublicationDate = new DateOnly(1998, 1, 1), Slug = publicationEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var poem = new Poem { Entity = poemEntity, Person = person, Publication = publication, Title = $"{marker} poem", CanonicalText = $"Canonical text {marker}", Slug = poemEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var sungVersion = new SungVersion { Entity = sungEntity, Poem = poem, Title = $"{marker} sung version", Text = $"Sung text {marker}", IsCanonical = true, Slug = sungEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var source = new Source { Entity = sourceEntity, Title = $"{marker} source", Author = marker, Slug = sourceEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var session = new RecordingSession { Entity = sessionEntity, Location = location, Notes = $"{marker} session", Slug = sessionEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var performanceEvent = new PerformanceEvent { Entity = eventEntity, Location = location, PerformanceNotes = $"{marker} event", Slug = eventEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var award = new Award { Entity = awardEntity, Name = $"{marker} award", Description = marker, Slug = awardEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var certification = new Certification { Entity = certificationEntity, Name = $"{marker} certification", Description = marker, Slug = certificationEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var chart = new Chart { Entity = chartEntity, Name = $"{marker} chart", Publisher = marker, Slug = chartEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };
        var tag = new Tag { Entity = tagEntity, Name = $"{marker} tag", Description = marker, Slug = tagEntity.Slug, CreatedBy = "integration-test", CreatedAt = now };

        context.AddRange(album, track, person, company, poem, sungVersion, genre, mood, instrument,
            source, location, publication, session, performanceEvent, award, certification, chart, tag);
        context.AddRange(
            new AlbumTrack { Album = album, Track = track, DiscNumber = 1, TrackNumber = 1, SequenceNumber = 1 },
            new AlbumGenre { Album = album, Genre = genre },
            new AlbumMood { Album = album, Mood = mood },
            new TrackGenre { Track = track, Genre = genre },
            new TrackMood { Track = track, Mood = mood },
            new TrackInstrument { Track = track, Instrument = instrument },
            new TrackSungVersion { Track = track, SungVersion = sungVersion, SequenceNumber = 1, IsPrimary = true },
            new PersonTypeAssignment { Person = person, PersonTypeId = mainArtistTypeId },
            new PersonTypeAssignment { Person = person, PersonTypeId = poetTypeId });
        await context.SaveChangesAsync();

        context.Credits.Add(new Credit
        {
            EntityTypeId = entityTypeIds["Track"],
            EntityId = track.EntityId,
            CreditRoleId = creditRoleId,
            RoleScopeTypeId = personScopeId,
            PersonId = person.PersonId,
            IsPrimary = true,
            CreatedBy = "integration-test",
            CreatedAt = now
        });
        var faLanguageId = await context.Languages.Where(x => x.Code == "fa").Select(x => x.LanguageId).SingleAsync();
        context.Localizations.Add(new Localization
        {
            EntityTypeId = entityTypeIds["Album"],
            EntityId = album.EntityId,
            LanguageId = faLanguageId,
            FieldName = "Title",
            LocalizedText = $"عنوان {marker}"
        });
        await context.SaveChangesAsync();

        return new PublicCatalogTestRecord(
            marker,
            album.AlbumId,
            $"عنوان {marker}",
            album.Slug,
            track.Slug,
            person.Slug,
            company.Slug,
            poem.Slug,
            sungVersion.Slug,
            genre.Slug,
            mood.Slug,
            instrument.Slug,
            source.Slug,
            location.Slug,
            publication.Slug,
            session.Slug,
            performanceEvent.Slug,
            award.Slug,
            certification.Slug,
            chart.Slug,
            tag.Slug);
    }

    public async Task<WizardReferenceIds> GetWizardReferenceIdsAsync(PublicCatalogTestRecord catalog)
    {
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return new WizardReferenceIds(
            await context.AlbumCategories.Select(x => x.AlbumCategoryId).FirstAsync(),
            await context.Genres.Where(x => x.Slug == catalog.GenreSlug).Select(x => x.GenreId).SingleAsync(),
            await context.Moods.Where(x => x.Slug == catalog.MoodSlug).Select(x => x.MoodId).SingleAsync(),
            await context.Instruments.Where(x => x.Slug == catalog.InstrumentSlug).Select(x => x.InstrumentId).SingleAsync(),
            await context.People.Where(x => x.Slug == catalog.PersonSlug).Select(x => x.PersonId).SingleAsync(),
            await context.Companies.Where(x => x.Slug == catalog.CompanySlug).Select(x => x.CompanyId).SingleAsync(),
            await context.Poems.Where(x => x.Slug == catalog.PoemSlug).Select(x => x.PoemId).SingleAsync(),
            await context.Tags.Where(x => x.Slug == catalog.TagSlug).Select(x => x.TagId).SingleAsync(),
            await context.CompanyRoleTypes.Select(x => x.CompanyRoleTypeId).FirstAsync(),
            await context.IdentifierTypes.Select(x => x.IdentifierTypeId).FirstAsync(),
            await context.CreditRoles.Where(x => x.Code == "PRIMARY_ARTIST").Select(x => x.CreditRoleId).SingleAsync(),
            await context.CreditRoles.Where(x => x.Code == "MUSICIAN").Select(x => x.CreditRoleId).SingleAsync(),
            await context.RoleScopeTypes.Where(x => x.Code == "Person").Select(x => x.RoleScopeTypeId).SingleAsync(),
            await context.LinkTypes.Select(x => x.LinkTypeId).FirstAsync(),
            await context.AlbumRelationTypes.Select(x => x.AlbumRelationTypeId).FirstAsync(),
            await context.LyricsAvailabilityTypes.Where(x => x.Code == "PUBLIC").Select(x => x.LyricsAvailabilityTypeId).SingleAsync());
    }

    public async Task InitializeAsync()
    {
        _database = await SqlServerTestDatabase.CreateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_database is null)
        {
            throw new InvalidOperationException("InitializeAsync must run before the test host is created.");
        }

        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _database.ConnectionString);
        builder.UseSetting("Seed:OnStartup", "true");
        builder.UseSetting("Seed:SampleContent", "false");
        builder.UseSetting("Seed:AdminUser", "false");
        builder.UseSetting("Admin:BootstrapEnabled", "false");
        builder.UseSetting("BackgroundJobs:Enabled", "false");
        builder.UseSetting("Site:EnableHttpsRedirection", "false");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        Dispose();
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }
}

public sealed record AlbumTestRecord(
    int AlbumId,
    int CategoryId,
    string Slug,
    string Title,
    byte[] RowVersion);

public sealed record AlbumDatabaseState(string Title, bool IsDeleted, byte[] RowVersion);

public sealed record HomeAlbumSet(
    string Marker,
    string CategoryCode,
    int? FirstAlbumId,
    IReadOnlyList<string> Slugs);

public sealed record PublicCatalogTestRecord(
    string Marker,
    int AlbumId,
    string PersianAlbumTitle,
    string AlbumSlug,
    string TrackSlug,
    string PersonSlug,
    string CompanySlug,
    string PoemSlug,
    string SungVersionSlug,
    string GenreSlug,
    string MoodSlug,
    string InstrumentSlug,
    string SourceSlug,
    string LocationSlug,
    string PublicationSlug,
    string SessionSlug,
    string EventSlug,
    string AwardSlug,
    string CertificationSlug,
    string ChartSlug,
    string TagSlug);

public sealed record WizardReferenceIds(
    int AlbumCategoryId,
    int GenreId,
    int MoodId,
    int InstrumentId,
    int PersonId,
    int CompanyId,
    int PoemId,
    int TagId,
    int CompanyRoleTypeId,
    int IdentifierTypeId,
    int CreditRoleId,
    int MusicianCreditRoleId,
    int PersonScopeTypeId,
    int LinkTypeId,
    int AlbumRelationTypeId,
    int PublicLyricsAvailabilityTypeId);
