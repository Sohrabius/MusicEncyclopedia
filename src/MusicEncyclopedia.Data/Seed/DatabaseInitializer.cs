using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Data.Seed;

/// <summary>
/// Seeds the database with initial lookup data required by the application.
/// For SQLite, also creates the schema via EnsureCreatedAsync.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(AppDbContext context, bool isSqlite)
    {
        if (isSqlite)
        {
            // Create the schema for SQLite (migrations handle SQL Server)
            await context.Database.EnsureCreatedAsync();
        }

        // Check if data already exists (using MediaTypes as a sentinel table)
        if (await context.MediaTypes.AnyAsync())
        {
            return; // Already seeded
        }

        await SeedLookupDataAsync(context);
    }

    private static async Task SeedLookupDataAsync(AppDbContext context)
    {
        // ──────────────────────────────────────────────
        // EntityType (1-18)
        // ──────────────────────────────────────────────
        context.EntityTypes.AddRange(
            new EntityType { EntityTypeId = 1, Code = "Album", Name = "Album" },
            new EntityType { EntityTypeId = 2, Code = "Track", Name = "Track" },
            new EntityType { EntityTypeId = 3, Code = "Person", Name = "Person" },
            new EntityType { EntityTypeId = 4, Code = "Company", Name = "Company" },
            new EntityType { EntityTypeId = 5, Code = "Genre", Name = "Genre" },
            new EntityType { EntityTypeId = 6, Code = "Mood", Name = "Mood" },
            new EntityType { EntityTypeId = 7, Code = "Instrument", Name = "Instrument" },
            new EntityType { EntityTypeId = 8, Code = "Poem", Name = "Poem" },
            new EntityType { EntityTypeId = 9, Code = "SungVersion", Name = "Sung Version" },
            new EntityType { EntityTypeId = 10, Code = "Publication", Name = "Publication" },
            new EntityType { EntityTypeId = 11, Code = "RecordingSession", Name = "Recording Session" },
            new EntityType { EntityTypeId = 12, Code = "PerformanceEvent", Name = "Performance Event" },
            new EntityType { EntityTypeId = 13, Code = "Location", Name = "Location" },
            new EntityType { EntityTypeId = 14, Code = "Award", Name = "Award" },
            new EntityType { EntityTypeId = 15, Code = "Certification", Name = "Certification" },
            new EntityType { EntityTypeId = 16, Code = "Chart", Name = "Chart" },
            new EntityType { EntityTypeId = 17, Code = "Source", Name = "Source" },
            new EntityType { EntityTypeId = 18, Code = "Tag", Name = "Tag" }
        );

        // ──────────────────────────────────────────────
        // Language (1)
        // ──────────────────────────────────────────────
        context.Languages.AddRange(
            new Language { LanguageId = 1, Code = "fa", Name = "Persian" }
        );

        // ──────────────────────────────────────────────
        // Country (1-14)
        // ──────────────────────────────────────────────
        context.Countries.AddRange(
            new Country { CountryId = 1, Code = "IR", Name = "Iran" },
            new Country { CountryId = 2, Code = "US", Name = "United States" },
            new Country { CountryId = 3, Code = "GB", Name = "United Kingdom" },
            new Country { CountryId = 4, Code = "FR", Name = "France" },
            new Country { CountryId = 5, Code = "DE", Name = "Germany" },
            new Country { CountryId = 6, Code = "CA", Name = "Canada" },
            new Country { CountryId = 7, Code = "AU", Name = "Australia" },
            new Country { CountryId = 8, Code = "LB", Name = "Lebanon" },
            new Country { CountryId = 9, Code = "EG", Name = "Egypt" },
            new Country { CountryId = 10, Code = "AE", Name = "United Arab Emirates" },
            new Country { CountryId = 11, Code = "TR", Name = "Turkey" },
            new Country { CountryId = 12, Code = "AF", Name = "Afghanistan" },
            new Country { CountryId = 13, Code = "IN", Name = "India" },
            new Country { CountryId = 14, Code = "PK", Name = "Pakistan" }
        );

        // ──────────────────────────────────────────────
        // AlbumCategory (1-10)
        // ──────────────────────────────────────────────
        context.AlbumCategories.AddRange(
            new AlbumCategory { AlbumCategoryId = 1, Code = "Studio", Name = "Studio Album" },
            new AlbumCategory { AlbumCategoryId = 2, Code = "Live", Name = "Live Album" },
            new AlbumCategory { AlbumCategoryId = 3, Code = "Compilation", Name = "Compilation" },
            new AlbumCategory { AlbumCategoryId = 4, Code = "EP", Name = "EP" },
            new AlbumCategory { AlbumCategoryId = 5, Code = "Single", Name = "Single" },
            new AlbumCategory { AlbumCategoryId = 6, Code = "BoxSet", Name = "Box Set" },
            new AlbumCategory { AlbumCategoryId = 7, Code = "Soundtrack", Name = "Soundtrack" },
            new AlbumCategory { AlbumCategoryId = 8, Code = "Demo", Name = "Demo" },
            new AlbumCategory { AlbumCategoryId = 9, Code = "Mixtape", Name = "Mixtape" },
            new AlbumCategory { AlbumCategoryId = 10, Code = "Remix", Name = "Remix Album" }
        );

        // ──────────────────────────────────────────────
        // CompanyType (1-6)
        // ──────────────────────────────────────────────
        context.CompanyTypes.AddRange(
            new CompanyType { CompanyTypeId = 1, Code = "Label", Name = "Record Label" },
            new CompanyType { CompanyTypeId = 2, Code = "Publisher", Name = "Publisher" },
            new CompanyType { CompanyTypeId = 3, Code = "Studio", Name = "Recording Studio" },
            new CompanyType { CompanyTypeId = 4, Code = "Production", Name = "Production Company" },
            new CompanyType { CompanyTypeId = 5, Code = "Distributor", Name = "Distributor" },
            new CompanyType { CompanyTypeId = 6, Code = "Manufacturer", Name = "Manufacturer" }
        );

        // ──────────────────────────────────────────────
        // CompanyRoleType (1-5)
        // ──────────────────────────────────────────────
        context.CompanyRoleTypes.AddRange(
            new CompanyRoleType { CompanyRoleTypeId = 1, Code = "Label", Name = "Label" },
            new CompanyRoleType { CompanyRoleTypeId = 2, Code = "Publisher", Name = "Publisher" },
            new CompanyRoleType { CompanyRoleTypeId = 3, Code = "Distributor", Name = "Distributor" },
            new CompanyRoleType { CompanyRoleTypeId = 4, Code = "Production", Name = "Production Company" },
            new CompanyRoleType { CompanyRoleTypeId = 5, Code = "Studio", Name = "Recording Studio" }
        );

        // ──────────────────────────────────────────────
        // RoleScopeType (1-3)
        // ──────────────────────────────────────────────
        context.RoleScopeTypes.AddRange(
            new RoleScopeType { RoleScopeTypeId = 1, Code = "Person", Name = "Person only" },
            new RoleScopeType { RoleScopeTypeId = 2, Code = "Company", Name = "Company only" },
            new RoleScopeType { RoleScopeTypeId = 3, Code = "Both", Name = "Person or Company" }
        );

        // ──────────────────────────────────────────────
        // CreditRole (1-20)
        // ──────────────────────────────────────────────
        context.CreditRoles.AddRange(
            new CreditRole { CreditRoleId = 1, Code = "PRIMARY_ARTIST", Name = "Primary Artist", DisplayOrder = 1, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 2, Code = "FEATURED_ARTIST", Name = "Featured Artist", DisplayOrder = 2, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 3, Code = "GUEST_ARTIST", Name = "Guest Artist", DisplayOrder = 3, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 4, Code = "MUSICIAN", Name = "Musician", DisplayOrder = 10, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 5, Code = "COMPOSER", Name = "Composer", DisplayOrder = 11, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 6, Code = "LYRICIST", Name = "Lyricist", DisplayOrder = 12, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 7, Code = "PRODUCER", Name = "Producer", DisplayOrder = 13, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 8, Code = "ARRANGER", Name = "Arranger", DisplayOrder = 14, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 9, Code = "CONDUCTOR", Name = "Conductor", DisplayOrder = 15, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 10, Code = "ENGINEER", Name = "Engineer", DisplayOrder = 20, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 11, Code = "MIXER", Name = "Mixing Engineer", DisplayOrder = 21, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 12, Code = "MASTERER", Name = "Mastering Engineer", DisplayOrder = 22, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 13, Code = "RECORDING", Name = "Recording Engineer", DisplayOrder = 23, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 14, Code = "ARTWORK", Name = "Artwork/Design", DisplayOrder = 30, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 15, Code = "PHOTOGRAPHER", Name = "Photographer", DisplayOrder = 31, RoleScopeTypeId = 1 },
            new CreditRole { CreditRoleId = 16, Code = "WRITER", Name = "Writer", DisplayOrder = 40, RoleScopeTypeId = 3 },
            new CreditRole { CreditRoleId = 17, Code = "LABEL", Name = "Label", DisplayOrder = 50, RoleScopeTypeId = 2 },
            new CreditRole { CreditRoleId = 18, Code = "PUBLISHER", Name = "Publisher", DisplayOrder = 51, RoleScopeTypeId = 2 },
            new CreditRole { CreditRoleId = 19, Code = "DISTRIBUTOR", Name = "Distributor", DisplayOrder = 52, RoleScopeTypeId = 2 },
            new CreditRole { CreditRoleId = 20, Code = "COLLABORATOR", Name = "Collaborator", DisplayOrder = 60, RoleScopeTypeId = 3 }
        );

        // ──────────────────────────────────────────────
        // AliasType (1-6)
        // ──────────────────────────────────────────────
        context.AliasTypes.AddRange(
            new AliasType { AliasTypeId = 1, Code = "OFFICIAL", Name = "Official Alias" },
            new AliasType { AliasTypeId = 2, Code = "TRANSLITERATION", Name = "Transliteration" },
            new AliasType { AliasTypeId = 3, Code = "SEARCH", Name = "Search Alias" },
            new AliasType { AliasTypeId = 4, Code = "ABBREVIATION", Name = "Abbreviation" },
            new AliasType { AliasTypeId = 5, Code = "ORIGINAL_SCRIPT", Name = "Original Script" },
            new AliasType { AliasTypeId = 6, Code = "SLUG", Name = "Slug" }
        );

        // ──────────────────────────────────────────────
        // MediaType (1-4)
        // ──────────────────────────────────────────────
        context.MediaTypes.AddRange(
            new MediaType { MediaTypeId = 1, Code = "IMAGE", Name = "Image" },
            new MediaType { MediaTypeId = 2, Code = "AUDIO", Name = "Audio" },
            new MediaType { MediaTypeId = 3, Code = "VIDEO", Name = "Video" },
            new MediaType { MediaTypeId = 4, Code = "PDF", Name = "Document" }
        );

        // ──────────────────────────────────────────────
        // MediaRoleType (1-14)
        // ──────────────────────────────────────────────
        context.MediaRoleTypes.AddRange(
            new MediaRoleType { MediaRoleTypeId = 1, Code = "COVER", Name = "Cover Art" },
            new MediaRoleType { MediaRoleTypeId = 2, Code = "BACK_COVER", Name = "Back Cover" },
            new MediaRoleType { MediaRoleTypeId = 3, Code = "BOOKLET", Name = "Booklet" },
            new MediaRoleType { MediaRoleTypeId = 4, Code = "PROMO", Name = "Promotional Image" },
            new MediaRoleType { MediaRoleTypeId = 5, Code = "LIVE", Name = "Live Photo" },
            new MediaRoleType { MediaRoleTypeId = 6, Code = "STUDIO", Name = "Studio Photo" },
            new MediaRoleType { MediaRoleTypeId = 7, Code = "THUMBNAIL", Name = "Thumbnail" },
            new MediaRoleType { MediaRoleTypeId = 8, Code = "BANNER", Name = "Banner" },
            new MediaRoleType { MediaRoleTypeId = 9, Code = "LOGO", Name = "Logo" },
            new MediaRoleType { MediaRoleTypeId = 10, Code = "AUDIO_PREVIEW", Name = "Audio Preview" },
            new MediaRoleType { MediaRoleTypeId = 11, Code = "MUSIC_VIDEO", Name = "Music Video" },
            new MediaRoleType { MediaRoleTypeId = 12, Code = "LYRIC_SHEET", Name = "Lyric Sheet" },
            new MediaRoleType { MediaRoleTypeId = 13, Code = "SCORE", Name = "Musical Score" },
            new MediaRoleType { MediaRoleTypeId = 14, Code = "INTERVIEW", Name = "Interview" }
        );

        // ──────────────────────────────────────────────
        // LinkType (1-19)
        // ──────────────────────────────────────────────
        context.LinkTypes.AddRange(
            new LinkType { LinkTypeId = 1, Code = "OFFICIAL", Name = "Official Website" },
            new LinkType { LinkTypeId = 2, Code = "WIKIPEDIA", Name = "Wikipedia" },
            new LinkType { LinkTypeId = 3, Code = "DISCOGS", Name = "Discogs" },
            new LinkType { LinkTypeId = 4, Code = "MUSICBRAINZ", Name = "MusicBrainz" },
            new LinkType { LinkTypeId = 5, Code = "SPOTIFY", Name = "Spotify" },
            new LinkType { LinkTypeId = 6, Code = "APPLE_MUSIC", Name = "Apple Music" },
            new LinkType { LinkTypeId = 7, Code = "YOUTUBE", Name = "YouTube" },
            new LinkType { LinkTypeId = 8, Code = "YOUTUBE_MUSIC", Name = "YouTube Music" },
            new LinkType { LinkTypeId = 9, Code = "SOUNDCLOUD", Name = "SoundCloud" },
            new LinkType { LinkTypeId = 10, Code = "BANDCAMP", Name = "Bandcamp" },
            new LinkType { LinkTypeId = 11, Code = "INSTAGRAM", Name = "Instagram" },
            new LinkType { LinkTypeId = 12, Code = "TWITTER", Name = "Twitter/X" },
            new LinkType { LinkTypeId = 13, Code = "TELEGRAM", Name = "Telegram" },
            new LinkType { LinkTypeId = 14, Code = "FACEBOOK", Name = "Facebook" },
            new LinkType { LinkTypeId = 15, Code = "IMDB", Name = "IMDb" },
            new LinkType { LinkTypeId = 16, Code = "LASTFM", Name = "Last.fm" },
            new LinkType { LinkTypeId = 17, Code = "GENIUS", Name = "Genius" },
            new LinkType { LinkTypeId = 18, Code = "PURCHASE", Name = "Purchase Link" },
            new LinkType { LinkTypeId = 19, Code = "STREAMING", Name = "Streaming Link" }
        );

        // ──────────────────────────────────────────────
        // SourceType (1-10)
        // ──────────────────────────────────────────────
        context.SourceTypes.AddRange(
            new SourceType { SourceTypeId = 1, Code = "BOOK", Name = "Book" },
            new SourceType { SourceTypeId = 2, Code = "MAGAZINE", Name = "Magazine" },
            new SourceType { SourceTypeId = 3, Code = "NEWSPAPER", Name = "Newspaper" },
            new SourceType { SourceTypeId = 4, Code = "WEBSITE", Name = "Website" },
            new SourceType { SourceTypeId = 5, Code = "INTERVIEW", Name = "Interview" },
            new SourceType { SourceTypeId = 6, Code = "LINER_NOTES", Name = "Liner Notes" },
            new SourceType { SourceTypeId = 7, Code = "OFFICIAL", Name = "Official Source" },
            new SourceType { SourceTypeId = 8, Code = "ARCHIVE", Name = "Archive" },
            new SourceType { SourceTypeId = 9, Code = "ACADEMIC", Name = "Academic Publication" },
            new SourceType { SourceTypeId = 10, Code = "SOCIAL_MEDIA", Name = "Social Media" }
        );

        // ──────────────────────────────────────────────
        // IdentifierType (1-5)
        // ──────────────────────────────────────────────
        context.IdentifierTypes.AddRange(
            new IdentifierType { IdentifierTypeId = 1, Code = "BARCODE", Name = "Barcode" },
            new IdentifierType { IdentifierTypeId = 2, Code = "CATALOG", Name = "Catalog Number" },
            new IdentifierType { IdentifierTypeId = 3, Code = "ISRC", Name = "ISRC" },
            new IdentifierType { IdentifierTypeId = 4, Code = "UPC", Name = "UPC" },
            new IdentifierType { IdentifierTypeId = 5, Code = "MATRIX", Name = "Matrix/Runout" }
        );

        // ──────────────────────────────────────────────
        // PersonKind (1-5)
        // ──────────────────────────────────────────────
        context.PersonKinds.AddRange(
            new PersonKind { PersonKindId = 1, Code = "INDIVIDUAL", Name = "Individual" },
            new PersonKind { PersonKindId = 2, Code = "GROUP", Name = "Group" },
            new PersonKind { PersonKindId = 3, Code = "CHOIR", Name = "Choir" },
            new PersonKind { PersonKindId = 4, Code = "ORCHESTRA", Name = "Orchestra" },
            new PersonKind { PersonKindId = 5, Code = "ENSEMBLE", Name = "Ensemble" }
        );

        // ──────────────────────────────────────────────
        // PersonType (1-8)
        // ──────────────────────────────────────────────
        context.PersonTypes.AddRange(
            new PersonType { PersonTypeId = 1, Code = "MAIN_ARTIST", Name = "Main Artist" },
            new PersonType { PersonTypeId = 2, Code = "POET", Name = "Poet" },
            new PersonType { PersonTypeId = 3, Code = "MUSICIAN", Name = "Musician" },
            new PersonType { PersonTypeId = 4, Code = "COMPOSER", Name = "Composer" },
            new PersonType { PersonTypeId = 5, Code = "LYRICIST", Name = "Lyricist" },
            new PersonType { PersonTypeId = 6, Code = "PRODUCER", Name = "Producer" },
            new PersonType { PersonTypeId = 7, Code = "ENGINEER", Name = "Engineer" },
            new PersonType { PersonTypeId = 8, Code = "CONDUCTOR", Name = "Conductor" }
        );

        // ──────────────────────────────────────────────
        // LocationType (1-6)
        // ──────────────────────────────────────────────
        context.LocationTypes.AddRange(
            new LocationType { LocationTypeId = 1, Code = "CITY", Name = "City" },
            new LocationType { LocationTypeId = 2, Code = "COUNTRY", Name = "Country" },
            new LocationType { LocationTypeId = 3, Code = "VENUE", Name = "Venue" },
            new LocationType { LocationTypeId = 4, Code = "STUDIO", Name = "Recording Studio" },
            new LocationType { LocationTypeId = 5, Code = "REGION", Name = "Region" },
            new LocationType { LocationTypeId = 6, Code = "PROVINCE", Name = "Province/State" }
        );

        // ──────────────────────────────────────────────
        // EventType (1-8)
        // ──────────────────────────────────────────────
        context.EventTypes.AddRange(
            new EventType { EventTypeId = 1, Code = "CONCERT", Name = "Concert" },
            new EventType { EventTypeId = 2, Code = "FESTIVAL", Name = "Festival" },
            new EventType { EventTypeId = 3, Code = "TOUR", Name = "Tour" },
            new EventType { EventTypeId = 4, Code = "TV_PERFORMANCE", Name = "TV Performance" },
            new EventType { EventTypeId = 5, Code = "RADIO_PERFORMANCE", Name = "Radio Performance" },
            new EventType { EventTypeId = 6, Code = "AWARD_CEREMONY", Name = "Award Ceremony" },
            new EventType { EventTypeId = 7, Code = "BOOK_SIGNING", Name = "Book Signing" },
            new EventType { EventTypeId = 8, Code = "MEET_AND_GREET", Name = "Meet and Greet" }
        );

        // ──────────────────────────────────────────────
        // SessionType (1-7)
        // ──────────────────────────────────────────────
        context.SessionTypes.AddRange(
            new SessionType { SessionTypeId = 1, Code = "ALBUM", Name = "Album Recording Session" },
            new SessionType { SessionTypeId = 2, Code = "SINGLE", Name = "Single Recording Session" },
            new SessionType { SessionTypeId = 3, Code = "DEMO", Name = "Demo Session" },
            new SessionType { SessionTypeId = 4, Code = "REHEARSAL", Name = "Rehearsal" },
            new SessionType { SessionTypeId = 5, Code = "MIXING", Name = "Mixing Session" },
            new SessionType { SessionTypeId = 6, Code = "MASTERING", Name = "Mastering Session" },
            new SessionType { SessionTypeId = 7, Code = "LIVE_RECORDING", Name = "Live Recording Session" }
        );

        // ──────────────────────────────────────────────
        // PublicationType (1-5)
        // ──────────────────────────────────────────────
        context.PublicationTypes.AddRange(
            new PublicationType { PublicationTypeId = 1, Code = "BOOK", Name = "Book" },
            new PublicationType { PublicationTypeId = 2, Code = "MAGAZINE", Name = "Magazine" },
            new PublicationType { PublicationTypeId = 3, Code = "JOURNAL", Name = "Journal" },
            new PublicationType { PublicationTypeId = 4, Code = "DIGITAL", Name = "Digital Publication" },
            new PublicationType { PublicationTypeId = 5, Code = "COLLECTION", Name = "Collection" }
        );

        // ──────────────────────────────────────────────
        // AwardResultType (1-6)
        // ──────────────────────────────────────────────
        context.AwardResultTypes.AddRange(
            new AwardResultType { AwardResultTypeId = 1, Code = "WON", Name = "Won" },
            new AwardResultType { AwardResultTypeId = 2, Code = "NOMINATED", Name = "Nominated" },
            new AwardResultType { AwardResultTypeId = 3, Code = "PLACE_1", Name = "First Place" },
            new AwardResultType { AwardResultTypeId = 4, Code = "PLACE_2", Name = "Second Place" },
            new AwardResultType { AwardResultTypeId = 5, Code = "PLACE_3", Name = "Third Place" },
            new AwardResultType { AwardResultTypeId = 6, Code = "HONORABLE", Name = "Honorable Mention" }
        );

        // ──────────────────────────────────────────────
        // TrackRelationType (1-10)
        // ──────────────────────────────────────────────
        context.TrackRelationTypes.AddRange(
            new TrackRelationType { TrackRelationTypeId = 1, Code = "COVER", Name = "Cover" },
            new TrackRelationType { TrackRelationTypeId = 2, Code = "REMIX", Name = "Remix" },
            new TrackRelationType { TrackRelationTypeId = 3, Code = "LIVE_VERSION", Name = "Live Version" },
            new TrackRelationType { TrackRelationTypeId = 4, Code = "ACOUSTIC", Name = "Acoustic Version" },
            new TrackRelationType { TrackRelationTypeId = 5, Code = "DEMO", Name = "Demo" },
            new TrackRelationType { TrackRelationTypeId = 6, Code = "REMASTER", Name = "Remaster" },
            new TrackRelationType { TrackRelationTypeId = 7, Code = "RE_RECORDING", Name = "Re-recording" },
            new TrackRelationType { TrackRelationTypeId = 8, Code = "ORIGINAL", Name = "Original" },
            new TrackRelationType { TrackRelationTypeId = 9, Code = "SAMPLED_IN", Name = "Sampled In" },
            new TrackRelationType { TrackRelationTypeId = 10, Code = "SAMPLES", Name = "Samples" }
        );

        // ──────────────────────────────────────────────
        // AlbumRelationType (1-6)
        // ──────────────────────────────────────────────
        context.AlbumRelationTypes.AddRange(
            new AlbumRelationType { AlbumRelationTypeId = 1, Code = "REISSUE", Name = "Reissue" },
            new AlbumRelationType { AlbumRelationTypeId = 2, Code = "REMASTER", Name = "Remaster" },
            new AlbumRelationType { AlbumRelationTypeId = 3, Code = "BOX_SET", Name = "Box Set Contains" },
            new AlbumRelationType { AlbumRelationTypeId = 4, Code = "PART_OF", Name = "Part Of" },
            new AlbumRelationType { AlbumRelationTypeId = 5, Code = "FOLLOW_UP", Name = "Follow-up" },
            new AlbumRelationType { AlbumRelationTypeId = 6, Code = "COMPILATION", Name = "Compilation Contains" }
        );

        // ──────────────────────────────────────────────
        // TrackVersionType (1-11)
        // ──────────────────────────────────────────────
        context.TrackVersionTypes.AddRange(
            new TrackVersionType { TrackVersionTypeId = 1, Code = "DEMO", Name = "Demo" },
            new TrackVersionType { TrackVersionTypeId = 2, Code = "ALTERNATE", Name = "Alternate Take" },
            new TrackVersionType { TrackVersionTypeId = 3, Code = "RADIO_EDIT", Name = "Radio Edit" },
            new TrackVersionType { TrackVersionTypeId = 4, Code = "ACOUSTIC", Name = "Acoustic" },
            new TrackVersionType { TrackVersionTypeId = 5, Code = "LIVE", Name = "Live" },
            new TrackVersionType { TrackVersionTypeId = 6, Code = "REMASTER", Name = "Remaster" },
            new TrackVersionType { TrackVersionTypeId = 7, Code = "REMIX", Name = "Remix" },
            new TrackVersionType { TrackVersionTypeId = 8, Code = "INSTRUMENTAL", Name = "Instrumental" },
            new TrackVersionType { TrackVersionTypeId = 9, Code = "A_CAPPELLA", Name = "A Cappella" },
            new TrackVersionType { TrackVersionTypeId = 10, Code = "EXTENDED", Name = "Extended Mix" },
            new TrackVersionType { TrackVersionTypeId = 11, Code = "ORIGINAL", Name = "Original Version" }
        );

        // ──────────────────────────────────────────────
        // VocalStyle (1-12)
        // ──────────────────────────────────────────────
        context.VocalStyles.AddRange(
            new VocalStyle { VocalStyleId = 1, Code = "SOLO", Name = "Solo" },
            new VocalStyle { VocalStyleId = 2, Code = "DUET", Name = "Duet" },
            new VocalStyle { VocalStyleId = 3, Code = "GROUP", Name = "Group" },
            new VocalStyle { VocalStyleId = 4, Code = "CHORUS", Name = "Chorus" },
            new VocalStyle { VocalStyleId = 5, Code = "BACKING", Name = "Backing Vocals" },
            new VocalStyle { VocalStyleId = 6, Code = "NARRATION", Name = "Narration" },
            new VocalStyle { VocalStyleId = 7, Code = "SPOKEN", Name = "Spoken Word" },
            new VocalStyle { VocalStyleId = 8, Code = "SCAT", Name = "Scat" },
            new VocalStyle { VocalStyleId = 9, Code = "FALSETTO", Name = "Falsetto" },
            new VocalStyle { VocalStyleId = 10, Code = "RAP", Name = "Rap" },
            new VocalStyle { VocalStyleId = 11, Code = "SCREAM", Name = "Scream" },
            new VocalStyle { VocalStyleId = 12, Code = "WHISPER", Name = "Whisper" }
        );

        // ──────────────────────────────────────────────
        // MusicalKey (1-28)
        // ──────────────────────────────────────────────
        context.MusicalKeys.AddRange(
            new MusicalKey { MusicalKeyId = 1, Code = "C", Name = "C Major" },
            new MusicalKey { MusicalKeyId = 2, Code = "Cm", Name = "C Minor" },
            new MusicalKey { MusicalKeyId = 3, Code = "C#", Name = "C# Major" },
            new MusicalKey { MusicalKeyId = 4, Code = "C#m", Name = "C# Minor" },
            new MusicalKey { MusicalKeyId = 5, Code = "Db", Name = "Db Major" },
            new MusicalKey { MusicalKeyId = 6, Code = "Dbm", Name = "Db Minor" },
            new MusicalKey { MusicalKeyId = 7, Code = "D", Name = "D Major" },
            new MusicalKey { MusicalKeyId = 8, Code = "Dm", Name = "D Minor" },
            new MusicalKey { MusicalKeyId = 9, Code = "Eb", Name = "Eb Major" },
            new MusicalKey { MusicalKeyId = 10, Code = "Ebm", Name = "Eb Minor" },
            new MusicalKey { MusicalKeyId = 11, Code = "E", Name = "E Major" },
            new MusicalKey { MusicalKeyId = 12, Code = "Em", Name = "E Minor" },
            new MusicalKey { MusicalKeyId = 13, Code = "F", Name = "F Major" },
            new MusicalKey { MusicalKeyId = 14, Code = "Fm", Name = "F Minor" },
            new MusicalKey { MusicalKeyId = 15, Code = "F#", Name = "F# Major" },
            new MusicalKey { MusicalKeyId = 16, Code = "F#m", Name = "F# Minor" },
            new MusicalKey { MusicalKeyId = 17, Code = "Gb", Name = "Gb Major" },
            new MusicalKey { MusicalKeyId = 18, Code = "Gbm", Name = "Gb Minor" },
            new MusicalKey { MusicalKeyId = 19, Code = "G", Name = "G Major" },
            new MusicalKey { MusicalKeyId = 20, Code = "Gm", Name = "G Minor" },
            new MusicalKey { MusicalKeyId = 21, Code = "Ab", Name = "Ab Major" },
            new MusicalKey { MusicalKeyId = 22, Code = "Abm", Name = "Ab Minor" },
            new MusicalKey { MusicalKeyId = 23, Code = "A", Name = "A Major" },
            new MusicalKey { MusicalKeyId = 24, Code = "Am", Name = "A Minor" },
            new MusicalKey { MusicalKeyId = 25, Code = "Bb", Name = "Bb Major" },
            new MusicalKey { MusicalKeyId = 26, Code = "Bbm", Name = "Bb Minor" },
            new MusicalKey { MusicalKeyId = 27, Code = "B", Name = "B Major" },
            new MusicalKey { MusicalKeyId = 28, Code = "Bm", Name = "B Minor" }
        );

        // ──────────────────────────────────────────────
        // LyricsAvailabilityType (1-5)
        // ──────────────────────────────────────────────
        context.LyricsAvailabilityTypes.AddRange(
            new LyricsAvailabilityType { LyricsAvailabilityTypeId = 1, Code = "NONE", Name = "No Lyrics" },
            new LyricsAvailabilityType { LyricsAvailabilityTypeId = 2, Code = "PUBLIC", Name = "Public" },
            new LyricsAvailabilityType { LyricsAvailabilityTypeId = 3, Code = "REGISTERED", Name = "Registered Users Only" },
            new LyricsAvailabilityType { LyricsAvailabilityTypeId = 4, Code = "REQUEST", Name = "Available Upon Request" },
            new LyricsAvailabilityType { LyricsAvailabilityTypeId = 5, Code = "RESTRICTED", Name = "Restricted Access" }
        );

        // ──────────────────────────────────────────────
        // CountryRoleType (1-4)
        // ──────────────────────────────────────────────
        context.CountryRoleTypes.AddRange(
            new CountryRoleType { CountryRoleTypeId = 1, Code = "NATIONALITY", Name = "Nationality" },
            new CountryRoleType { CountryRoleTypeId = 2, Code = "ORIGIN", Name = "Country of Origin" },
            new CountryRoleType { CountryRoleTypeId = 3, Code = "OPERATION", Name = "Country of Operation" },
            new CountryRoleType { CountryRoleTypeId = 4, Code = "REGISTRATION", Name = "Country of Registration" }
        );

        // ──────────────────────────────────────────────
        // InstrumentFamily (1-9)
        // ──────────────────────────────────────────────
        context.InstrumentFamilies.AddRange(
            new InstrumentFamily { InstrumentFamilyId = 1, Code = "STRING", Name = "String Instruments" },
            new InstrumentFamily { InstrumentFamilyId = 2, Code = "WOODWIND", Name = "Woodwind Instruments" },
            new InstrumentFamily { InstrumentFamilyId = 3, Code = "BRASS", Name = "Brass Instruments" },
            new InstrumentFamily { InstrumentFamilyId = 4, Code = "PERCUSSION", Name = "Percussion Instruments" },
            new InstrumentFamily { InstrumentFamilyId = 5, Code = "KEYBOARD", Name = "Keyboard Instruments" },
            new InstrumentFamily { InstrumentFamilyId = 6, Code = "ELECTRONIC", Name = "Electronic Instruments" },
            new InstrumentFamily { InstrumentFamilyId = 7, Code = "VOCAL", Name = "Vocal" },
            new InstrumentFamily { InstrumentFamilyId = 8, Code = "FOLK", Name = "Folk/Traditional Instruments" },
            new InstrumentFamily { InstrumentFamilyId = 9, Code = "OTHER", Name = "Other Instruments" }
        );

        await context.SaveChangesAsync();
    }
}
