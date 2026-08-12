using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Data.Seed;

/// <summary>
/// Seeds the database with initial lookup data required by the application.
/// For SQLite, also creates the schema via EnsureCreatedAsync.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Initializes schema and seed data.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="isSqlite">True when running on SQLite (EnsureCreated); false applies on SQL Server (migrations are applied by the caller).</param>
    /// <param name="seedSampleContent">
    /// When false, only lookup data is seeded — the demo artists/albums/tracks/
    /// poems and Phase 4 content are skipped. Production deployments should set
    /// this to false (config <c>Seed:SampleContent</c>).
    /// </param>
    public static async Task InitializeAsync(
        AppDbContext context, bool isSqlite, bool seedSampleContent = true)
    {
        if (isSqlite)
        {
            // Create the schema for SQLite (migrations handle SQL Server).
            // EnsureCreated does not alter an existing database, so tables added
            // after the initial schema creation are created explicitly here.
            await context.Database.EnsureCreatedAsync();

            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "AuditLog" (
                    "AuditLogId" INTEGER NOT NULL CONSTRAINT "PK_AuditLog" PRIMARY KEY AUTOINCREMENT,
                    "Timestamp" TEXT NOT NULL,
                    "UserName" TEXT NOT NULL,
                    "Action" TEXT NOT NULL,
                    "EntityType" TEXT NULL,
                    "EntityId" INTEGER NULL,
                    "IsSuccess" INTEGER NOT NULL,
                    "Details" TEXT NULL,
                    "IpAddress" TEXT NULL
                )
                """);

            // ASP.NET Core Identity tables. Fresh databases get these from the
            // model (EnsureCreated) / migration (MigrateAsync); the CREATE TABLE
            // IF NOT EXISTS guards below are a backward-compat safety net for
            // existing databases created before Identity was added to the model.
            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "AspNetRoles" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
                    "Name" TEXT NULL,
                    "NormalizedName" TEXT NULL,
                    "ConcurrencyStamp" TEXT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

                CREATE TABLE IF NOT EXISTS "AspNetUsers" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
                    "UserName" TEXT NULL,
                    "NormalizedUserName" TEXT NULL,
                    "Email" TEXT NULL,
                    "NormalizedEmail" TEXT NULL,
                    "EmailConfirmed" INTEGER NOT NULL,
                    "PasswordHash" TEXT NULL,
                    "SecurityStamp" TEXT NULL,
                    "ConcurrencyStamp" TEXT NULL,
                    "PhoneNumber" TEXT NULL,
                    "PhoneNumberConfirmed" INTEGER NOT NULL,
                    "TwoFactorEnabled" INTEGER NOT NULL,
                    "LockoutEnd" TEXT NULL,
                    "LockoutEnabled" INTEGER NOT NULL,
                    "AccessFailedCount" INTEGER NOT NULL
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
                CREATE UNIQUE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

                CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
                    "RoleId" TEXT NOT NULL,
                    "ClaimType" TEXT NULL,
                    "ClaimValue" TEXT NULL
                );
                CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

                CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
                    "UserId" TEXT NOT NULL,
                    "ClaimType" TEXT NULL,
                    "ClaimValue" TEXT NULL
                );
                CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

                CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
                    "LoginProvider" TEXT NOT NULL,
                    "ProviderKey" TEXT NOT NULL,
                    "ProviderDisplayName" TEXT NULL,
                    "UserId" TEXT NOT NULL,
                    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey")
                );
                CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

                CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
                    "UserId" TEXT NOT NULL,
                    "RoleId" TEXT NOT NULL,
                    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId")
                );
                CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

                CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
                    "UserId" TEXT NOT NULL,
                    "LoginProvider" TEXT NOT NULL,
                    "Name" TEXT NOT NULL,
                    "Value" TEXT NULL,
                    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name")
                );
                """);
        }
        else
        {
            // SQL Server: EnsureCreated does not alter an existing database, so the
            // AuditLog table is created explicitly when missing (matches EF's mapping:
            // DateTime → datetime2, bool → bit, strings → nvarchar with lengths).
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[AuditLog]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AuditLog] (
                        [AuditLogId] int NOT NULL IDENTITY(1,1) CONSTRAINT [PK_AuditLog] PRIMARY KEY,
                        [Timestamp] datetime2 NOT NULL,
                        [UserName] nvarchar(255) NOT NULL,
                        [Action] nvarchar(255) NOT NULL,
                        [EntityType] nvarchar(100) NULL,
                        [EntityId] int NULL,
                        [IsSuccess] bit NOT NULL,
                        [Details] nvarchar(4000) NULL,
                        [IpAddress] nvarchar(100) NULL
                    )
                END
                """);

            // ASP.NET Core Identity tables. On a fresh SQL Server the
            // AddCurrentModelTables migration creates them (idempotent DDL below
            // no-ops via the IF OBJECT_ID guards); these remain as a
            // backward-compat safety net for databases created before the
            // migration existed. Types match the IdentityDbContext mapping.
            await context.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[AspNetRoles]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AspNetRoles] (
                        [Id] nvarchar(450) NOT NULL CONSTRAINT [PK_AspNetRoles] PRIMARY KEY,
                        [Name] nvarchar(256) NULL,
                        [NormalizedName] nvarchar(256) NULL,
                        [ConcurrencyStamp] nvarchar(max) NULL
                    );
                    CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
                END

                IF OBJECT_ID(N'[AspNetUsers]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AspNetUsers] (
                        [Id] nvarchar(450) NOT NULL CONSTRAINT [PK_AspNetUsers] PRIMARY KEY,
                        [UserName] nvarchar(256) NULL,
                        [NormalizedUserName] nvarchar(256) NULL,
                        [Email] nvarchar(256) NULL,
                        [NormalizedEmail] nvarchar(256) NULL,
                        [EmailConfirmed] bit NOT NULL,
                        [PasswordHash] nvarchar(max) NULL,
                        [SecurityStamp] nvarchar(max) NULL,
                        [ConcurrencyStamp] nvarchar(max) NULL,
                        [PhoneNumber] nvarchar(max) NULL,
                        [PhoneNumberConfirmed] bit NOT NULL,
                        [TwoFactorEnabled] bit NOT NULL,
                        [LockoutEnd] datetimeoffset NULL,
                        [LockoutEnabled] bit NOT NULL,
                        [AccessFailedCount] int NOT NULL
                    );
                    CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
                    CREATE UNIQUE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]) WHERE [NormalizedEmail] IS NOT NULL;
                END

                IF OBJECT_ID(N'[AspNetRoleClaims]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AspNetRoleClaims] (
                        [Id] int NOT NULL IDENTITY(1,1) CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY,
                        [RoleId] nvarchar(450) NOT NULL,
                        [ClaimType] nvarchar(max) NULL,
                        [ClaimValue] nvarchar(max) NULL
                    );
                    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
                END

                IF OBJECT_ID(N'[AspNetUserClaims]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AspNetUserClaims] (
                        [Id] int NOT NULL IDENTITY(1,1) CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY,
                        [UserId] nvarchar(450) NOT NULL,
                        [ClaimType] nvarchar(max) NULL,
                        [ClaimValue] nvarchar(max) NULL
                    );
                    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
                END

                IF OBJECT_ID(N'[AspNetUserLogins]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AspNetUserLogins] (
                        [LoginProvider] nvarchar(128) NOT NULL,
                        [ProviderKey] nvarchar(128) NOT NULL,
                        [ProviderDisplayName] nvarchar(max) NULL,
                        [UserId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey])
                    );
                    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
                END

                IF OBJECT_ID(N'[AspNetUserRoles]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AspNetUserRoles] (
                        [UserId] nvarchar(450) NOT NULL,
                        [RoleId] nvarchar(450) NOT NULL,
                        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId])
                    );
                    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
                END

                IF OBJECT_ID(N'[AspNetUserTokens]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AspNetUserTokens] (
                        [UserId] nvarchar(450) NOT NULL,
                        [LoginProvider] nvarchar(128) NOT NULL,
                        [Name] nvarchar(128) NOT NULL,
                        [Value] nvarchar(max) NULL,
                        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name])
                    );
                END
                """);
        }

        // Check if data already exists (using MediaTypes as a sentinel table)
        if (await context.MediaTypes.AnyAsync())
        {
            await SeedOptionalContentAsync(context, seedSampleContent);
            await SeedLocalizationsAsync(context);
            return; // Lookup data already seeded
        }

        await SeedLookupDataAsync(context);

        await SeedOptionalContentAsync(context, seedSampleContent);

        await SeedLocalizationsAsync(context);
    }

    /// <summary>
    /// Seeds demo/sample content only when explicitly enabled (config
    /// <c>Seed:SampleContent</c>). Production deployments keep lookup-only data.
    /// </summary>
    private static async Task SeedOptionalContentAsync(AppDbContext context, bool seedSampleContent)
    {
        if (!seedSampleContent)
        {
            return;
        }

        await SeedSampleContentAsync(context);
        await SeedPhase4ContentAsync(context);
    }

    /// <summary>
    /// Seeds the en/ar/fr Language rows (fresh and existing databases) and a small
    /// set of sample Persian (fa) localizations for the seeded content. The Persian
    /// overlays demonstrate the spec 7.3 fallback chain: fa pages show Persian text,
    /// while other cultures fall back to the English base columns.
    /// Every section is guarded so re-runs never duplicate data.
    /// </summary>
    private static async Task SeedLocalizationsAsync(AppDbContext context)
    {
        // Languages (idempotent — existing databases won't have en/ar/fr)
        if (!await context.Languages.AnyAsync(l => l.Code == "en"))
        {
            context.Languages.AddRange(
                new Language { LanguageId = 2, Code = "en", Name = "English" },
                new Language { LanguageId = 3, Code = "ar", Name = "Arabic" },
                new Language { LanguageId = 4, Code = "fr", Name = "French" }
            );
            await context.SaveChangesAsync();
        }

        // Guard: seed sample localizations only once
        if (await context.Localizations.AnyAsync())
            return;

        var fa = await context.Languages.FirstAsync(l => l.Code == "fa");

        var albumEntity = await context.Entities.FirstOrDefaultAsync(e => e.Slug == "midnight-garden");
        if (albumEntity is not null)
        {
            context.Localizations.AddRange(
                new Localization
                {
                    EntityTypeId = 1, // Album
                    EntityId = albumEntity.EntityId,
                    LanguageId = fa.LanguageId,
                    FieldName = "Title",
                    LocalizedText = "باغ نیمه‌شب"
                },
                new Localization
                {
                    EntityTypeId = 1,
                    EntityId = albumEntity.EntityId,
                    LanguageId = fa.LanguageId,
                    FieldName = "Description",
                    LocalizedText = "سفری شبانه در لایه‌های سازهای زهی و ملودی‌های آرام؛ " +
                                     "ضبط‌شده در استودیوی اختصاصی گروه در تهران."
                }
            );
        }

        var trackEntity = await context.Entities.FirstOrDefaultAsync(e => e.Slug == "garden-of-stars");
        if (trackEntity is not null)
        {
            context.Localizations.Add(new Localization
            {
                EntityTypeId = 2, // Track
                EntityId = trackEntity.EntityId,
                LanguageId = fa.LanguageId,
                FieldName = "Title",
                LocalizedText = "باغ ستارگان"
            });
        }

        var personEntity = await context.Entities.FirstOrDefaultAsync(e => e.Slug == "darya-ensemble");
        if (personEntity is not null)
        {
            context.Localizations.AddRange(
                new Localization
                {
                    EntityTypeId = 3, // Person
                    EntityId = personEntity.EntityId,
                    LanguageId = fa.LanguageId,
                    FieldName = "Name",
                    LocalizedText = "گروه دریا"
                },
                new Localization
                {
                    EntityTypeId = 3,
                    EntityId = personEntity.EntityId,
                    LanguageId = fa.LanguageId,
                    FieldName = "Biography",
                    LocalizedText = "گروه دریا گروهی معاصر است که سنت‌های موسیقی کلاسیک ایرانی را " +
                                     "با تنظیم‌های مدرن در هم می‌آمیزد. این گروه در تهران بنیان‌گذاری شد " +
                                     "و به‌خاطر خط‌های ظریف سنتور، آواز شاعرانه و تصنیف‌های مراقبه‌گونه شناخته می‌شود."
                }
            );
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds genres, moods, instruments, and a small set of sample encyclopedia
    /// content (artist, albums, tracks, poems, and credits) so the public pages
    /// are browsable out of the box. Each section is guarded independently so
    /// re-runs never duplicate data.
    /// </summary>
    private static async Task SeedSampleContentAsync(AppDbContext context)
    {
        // ── Genres / Moods / Instruments (lookup content missing from the original seed) ──
        Dictionary<string, int> genreIds;
        Dictionary<string, int> moodIds;
        Dictionary<string, int> instrumentIds;

        if (!await context.Genres.AnyAsync())
        {
            genreIds = await SeedGenresAsync(context);
        }
        else
        {
            genreIds = await context.Genres.ToDictionaryAsync(g => g.Slug, g => g.GenreId);
        }

        if (!await context.Moods.AnyAsync())
        {
            moodIds = await SeedMoodsAsync(context);
        }
        else
        {
            moodIds = await context.Moods.ToDictionaryAsync(m => m.Slug, m => m.MoodId);
        }

        if (!await context.Instruments.AnyAsync())
        {
            instrumentIds = await SeedInstrumentsAsync(context);
        }
        else
        {
            instrumentIds = await context.Instruments.ToDictionaryAsync(i => i.Slug, i => i.InstrumentId);
        }

        // ── Sample content (skipped once albums exist) ──
        if (await context.Albums.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        // Artist person
        var artistEntity = await CreateEntityAsync(context, 3, "darya-ensemble");
        var artist = new Person
        {
            EntityId = artistEntity.EntityId,
            FullName = "Darya Ensemble",
            FullNameSort = "Darya Ensemble",
            EnglishName = "Darya Ensemble",
            PersonKindId = 2, // Group
            NationalityCountryId = 1,
            Biography = "Darya Ensemble is a contemporary ensemble blending Persian classical traditions " +
                         "with modern arrangements. Founded in Tehran, the group is known for its " +
                         "intricate santur lines, poetic vocals, and meditative compositions.",
            RowVersion = new byte[8],
            Slug = "darya-ensemble",
            CreatedBy = "seed",
            CreatedAt = now
        };
        context.People.Add(artist);
        await context.SaveChangesAsync();
        context.PersonTypeAssignments.Add(new PersonTypeAssignment { PersonId = artist.PersonId, PersonTypeId = 1 });

        // Poet person
        var poetEntity = await CreateEntityAsync(context, 3, "niloofar-rahimi");
        var poet = new Person
        {
            EntityId = poetEntity.EntityId,
            FullName = "Niloofar Rahimi",
            FullNameSort = "Rahimi, Niloofar",
            EnglishName = "Niloofar Rahimi",
            PersonKindId = 1, // Individual
            NationalityCountryId = 1,
            Biography = "Niloofar Rahimi is a contemporary poet whose verses explore memory, " +
                         "light, and the quiet rhythms of everyday life.",
            RowVersion = new byte[8],
            Slug = "niloofar-rahimi",
            CreatedBy = "seed",
            CreatedAt = now
        };
        context.People.Add(poet);
        await context.SaveChangesAsync();
        context.PersonTypeAssignments.Add(new PersonTypeAssignment { PersonId = poet.PersonId, PersonTypeId = 2 });

        // ── Tracks ──
        async Task<Track> AddTrackAsync(string slug, string title, int durationSeconds,
            int[] genreSlugIds, int[] moodSlugIds, int[] instrumentSlugIds)
        {
            var entity = await CreateEntityAsync(context, 2, slug);
            var track = new Track
            {
                EntityId = entity.EntityId,
                Title = title,
                TitleSort = title,
                EnglishTitle = title,
                DurationSeconds = durationSeconds,
                LyricsAvailabilityTypeId = 2, // Public
                RowVersion = new byte[8],
                Slug = slug,
                CreatedBy = "seed",
                CreatedAt = now
            };
            context.Tracks.Add(track);
            await context.SaveChangesAsync();

            foreach (var genreId in genreSlugIds)
            {
                context.TrackGenres.Add(new TrackGenre { TrackId = track.TrackId, GenreId = genreId });
            }
            foreach (var moodId in moodSlugIds)
            {
                context.TrackMoods.Add(new TrackMood { TrackId = track.TrackId, MoodId = moodId });
            }
            foreach (var instrumentId in instrumentSlugIds)
            {
                context.TrackInstruments.Add(new TrackInstrument { TrackId = track.TrackId, InstrumentId = instrumentId });
            }
            await context.SaveChangesAsync();

            return track;
        }

        var gardenTracks = new List<Track>
        {
            await AddTrackAsync("garden-of-stars", "Garden of Stars", 214,
                [genreIds["classical"], genreIds["folk"]], [moodIds["calm"]], [instrumentIds["santur"], instrumentIds["setar"]]),
            await AddTrackAsync("velvet-dusk", "Velvet Dusk", 187,
                [genreIds["folk"]], [moodIds["melancholic"]], [instrumentIds["violin"]]) ,
            await AddTrackAsync("moonlit-mirror", "Moonlit Mirror", 245,
                [genreIds["classical"]], [moodIds["romantic"]], [instrumentIds["piano"]]) ,
            await AddTrackAsync("silent-petals", "Silent Petals", 198,
                [genreIds["folk"], genreIds["classical"]], [moodIds["calm"], moodIds["melancholic"]], [instrumentIds["santur"]]) ,
            await AddTrackAsync("nightingale-whisper", "Nightingale Whisper", 233,
                [genreIds["folk"]], [moodIds["romantic"]], [instrumentIds["setar"], instrumentIds["tombak"]]) ,
        };

        var riverTracks = new List<Track>
        {
            await AddTrackAsync("river-of-memories", "River of Memories", 261,
                [genreIds["classical"]], [moodIds["melancholic"]], [instrumentIds["piano"]]),
            await AddTrackAsync("drifting-leaves", "Drifting Leaves", 205,
                [genreIds["folk"]], [moodIds["calm"]], [instrumentIds["santur"]]),
            await AddTrackAsync("glassy-surface", "Glassy Surface", 178,
                [genreIds["classical"], genreIds["jazz"]], [moodIds["calm"]], [instrumentIds["piano"]]),
            await AddTrackAsync("undertow", "Undertow", 224,
                [genreIds["folk"]], [moodIds["melancholic"], moodIds["energetic"]], [instrumentIds["tombak"]]),
        };

        var dawnTracks = new List<Track>
        {
            await AddTrackAsync("first-light", "First Light", 241,
                [genreIds["classical"]], [moodIds["joyful"]], [instrumentIds["violin"]]),
            await AddTrackAsync("amber-sky", "Amber Sky", 196,
                [genreIds["folk"]], [moodIds["calm"], moodIds["romantic"]], [instrumentIds["setar"]]),
            await AddTrackAsync("horizon-line", "Horizon Line", 212,
                [genreIds["classical"], genreIds["jazz"]], [moodIds["energetic"]], [instrumentIds["piano"], instrumentIds["guitar"]]),
            await AddTrackAsync("awakening", "Awakening", 254,
                [genreIds["folk"], genreIds["classical"]], [moodIds["joyful"]], [instrumentIds["santur"], instrumentIds["tombak"]]),
        };

        // ── Albums ──
        async Task AddAlbumAsync(string slug, string title, DateOnly releaseDate, string description,
            int[] albumGenreIds, int[] albumMoodIds, IReadOnlyList<Track> albumTracks, DateTime createdAt)
        {
            var entity = await CreateEntityAsync(context, 1, slug);
            var album = new Album
            {
                EntityId = entity.EntityId,
                Title = title,
                TitleSort = title,
                EnglishTitle = title,
                AlbumCategoryId = 1, // Studio
                ReleaseDate = releaseDate,
                ReleaseDatePrecision = "day",
                Description = description,
                Slug = slug,
                IsOfficial = true,
                DurationSeconds = albumTracks.Sum(t => t.DurationSeconds ?? 0),
                RowVersion = new byte[8],
                CreatedBy = "seed",
                CreatedAt = createdAt
            };
            context.Albums.Add(album);
            await context.SaveChangesAsync();

            foreach (var genreId in albumGenreIds)
            {
                context.AlbumGenres.Add(new AlbumGenre { AlbumId = album.AlbumId, GenreId = genreId });
            }
            foreach (var moodId in albumMoodIds)
            {
                context.AlbumMoods.Add(new AlbumMood { AlbumId = album.AlbumId, MoodId = moodId });
            }
            for (var i = 0; i < albumTracks.Count; i++)
            {
                context.AlbumTracks.Add(new AlbumTrack
                {
                    AlbumId = album.AlbumId,
                    TrackId = albumTracks[i].TrackId,
                    DiscNumber = 1,
                    TrackNumber = i + 1,
                    SequenceNumber = i + 1
                });
            }

            // Primary artist credit + composer credit
            context.Credits.Add(new Credit
            {
                EntityTypeId = 1, // Album
                EntityId = album.EntityId,
                CreditRoleId = 1, // Primary Artist
                RoleScopeTypeId = 1,
                PersonId = artist.PersonId,
                DisplayOrder = 1,
                IsPrimary = true,
                CreatedBy = "seed",
                CreatedAt = now
            });
            context.Credits.Add(new Credit
            {
                EntityTypeId = 1, // Album
                EntityId = album.EntityId,
                CreditRoleId = 5, // Composer
                RoleScopeTypeId = 1,
                PersonId = artist.PersonId,
                DisplayOrder = 2,
                IsPrimary = false,
                CreatedBy = "seed",
                CreatedAt = now
            });

            await context.SaveChangesAsync();
        }

        await AddAlbumAsync(
            "midnight-garden", "Midnight Garden", new DateOnly(2022, 6, 15),
            "A nocturnal journey through layered strings and hushed melodies, " +
            "recorded in the ensemble's own Tehran studio.",
            [genreIds["classical"], genreIds["folk"]], [moodIds["calm"], moodIds["melancholic"]],
            gardenTracks, now.AddDays(-2));

        await AddAlbumAsync(
            "the-silent-river", "The Silent River", new DateOnly(2024, 3, 20),
            "An intimate set exploring stillness and flow, featuring solo piano " +
            "interludes against the ensemble's santur-led arrangements.",
            [genreIds["classical"]], [moodIds["melancholic"], moodIds["calm"]],
            riverTracks, now.AddDays(-1));

        await AddAlbumAsync(
            "echoes-of-dawn", "Echoes of Dawn", new DateOnly(2020, 10, 8),
            "The ensemble's debut — bright, celebratory compositions that announce " +
            "a fresh voice in contemporary Persian instrumental music.",
            [genreIds["folk"], genreIds["jazz"]], [moodIds["joyful"], moodIds["energetic"]],
            dawnTracks, now);

        // ── Poems ──
        var poem1Entity = await CreateEntityAsync(context, 8, "the-garden-of-quiet");
        context.Poems.Add(new Poem
        {
            EntityId = poem1Entity.EntityId,
            Title = "The Garden of Quiet",
            EnglishTitle = "The Garden of Quiet",
            PersonId = poet.PersonId,
            CanonicalText = "Under the still trees, the day lays down its instruments.\n" +
                            "A single leaf rehearses the sound of arrival.\n" +
                            "I count the syllables of the evening, and they are few.\n" +
                            "The garden keeps no account of my patience.",
            RowVersion = new byte[8],
            Slug = "the-garden-of-quiet",
            CreatedBy = "seed",
            CreatedAt = now.AddHours(-6)
        });

        var poem2Entity = await CreateEntityAsync(context, 8, "a-letter-to-the-dawn");
        context.Poems.Add(new Poem
        {
            EntityId = poem2Entity.EntityId,
            Title = "A Letter to the Dawn",
            EnglishTitle = "A Letter to the Dawn",
            PersonId = poet.PersonId,
            CanonicalText = "Morning, forgive my lateness: I was arranging yesterday's\n" +
                            "unopened light into something smaller than a wish.\n" +
                            "Here is my reply — a window left ajar,\n" +
                            "and the first bird already rehearsing its answer.",
            RowVersion = new byte[8],
            Slug = "a-letter-to-the-dawn",
            CreatedBy = "seed",
            CreatedAt = now.AddHours(-4)
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds Phase 4 (Advanced Encyclopedia Features) data: a recording session,
    /// a performance event, an award + assignment, a chart + entries, a
    /// publication, musician/track credits, musician instruments, and album/
    /// track relations — so the timeline, discography filters, contribution
    /// filters and related-items sections are demonstrable out of the box.
    /// Every section is guarded so re-runs never duplicate data.
    /// </summary>
    private static async Task SeedPhase4ContentAsync(AppDbContext context)
    {
        if (await context.RecordingSessions.AnyAsync())
            return; // Phase 4 content already seeded

        var now = DateTime.UtcNow;

        // ── Resolve seeded entities by slug (works on fresh and existing DBs) ──
        var artist = await context.People.FirstOrDefaultAsync(p => p.Slug == "darya-ensemble");
        var poet = await context.People.FirstOrDefaultAsync(p => p.Slug == "niloofar-rahimi");
        var album = await context.Albums.FirstOrDefaultAsync(a => a.Slug == "midnight-garden");
        var otherAlbum = await context.Albums.FirstOrDefaultAsync(a => a.Slug == "the-silent-river");
        var track = await context.Tracks.FirstOrDefaultAsync(t => t.Slug == "garden-of-stars");
        var otherTrack = await context.Tracks.FirstOrDefaultAsync(t => t.Slug == "silent-petals");

        if (artist is null || album is null || track is null)
            return; // Sample content not present — nothing to attach Phase 4 data to

        // ── Resolve lookup ids by code (never hard-code against row order) ──
        var sessionTypeAlbum = await context.SessionTypes.FirstOrDefaultAsync(s => s.Code == "ALBUM");
        var eventTypeConcert = await context.EventTypes.FirstOrDefaultAsync(e => e.Code == "CONCERT");
        var awardResultWon = await context.AwardResultTypes.FirstOrDefaultAsync(r => r.Code == "WON");
        var publicationTypeBook = await context.PublicationTypes.FirstOrDefaultAsync(p => p.Code == "BOOK");
        var albumRelationFollowUp = await context.AlbumRelationTypes.FirstOrDefaultAsync(r => r.Code == "FOLLOW_UP");
        var trackRelationAcoustic = await context.TrackRelationTypes.FirstOrDefaultAsync(r => r.Code == "ACOUSTIC");
        var santur = await context.Instruments.FirstOrDefaultAsync(i => i.Slug == "santur");
        var tombak = await context.Instruments.FirstOrDefaultAsync(i => i.Slug == "tombak");

        // ── Location: the ensemble's own Tehran studio ──
        var location = await context.Locations.FirstOrDefaultAsync(l => l.Slug == "darya-studio-tehran");
        if (location is null)
        {
            var locationEntity = await CreateEntityAsync(context, 13, "darya-studio-tehran");
            location = new Location
            {
                EntityId = locationEntity.EntityId,
                Name = "Darya Studio",
                LocationTypeId = 4, // Recording Studio
                CountryId = 1,
                Slug = "darya-studio-tehran",
                IsDeleted = false,
                RowVersion = new byte[8],
                CreatedBy = "seed",
                CreatedAt = now
            };
            context.Locations.Add(location);
            await context.SaveChangesAsync();
        }

        // ── Recording session ──
        var session = await context.RecordingSessions.FirstOrDefaultAsync(s => s.Slug == "midnight-garden-session");
        if (session is null)
        {
            var sessionEntity = await CreateEntityAsync(context, 11, "midnight-garden-session");
            session = new RecordingSession
            {
                EntityId = sessionEntity.EntityId,
                SessionTypeId = sessionTypeAlbum?.SessionTypeId,
                LocationId = location.LocationId,
                StartDate = new DateOnly(2022, 2, 10),
                EndDate = new DateOnly(2022, 4, 28),
                Notes = "Principal tracking for Midnight Garden.",
                Slug = "midnight-garden-session",
                IsDeleted = false,
                RowVersion = new byte[8],
                CreatedBy = "seed",
                CreatedAt = now
            };
            context.RecordingSessions.Add(session);
            await context.SaveChangesAsync();

            context.RecordingSessionAlbums.Add(new RecordingSessionAlbum { RecordingSessionId = session.RecordingSessionId, AlbumId = album.AlbumId });
            context.RecordingSessionTracks.Add(new RecordingSessionTrack { RecordingSessionId = session.RecordingSessionId, TrackId = track.TrackId });
            await context.SaveChangesAsync();
        }

        // ── Performance event ──
        var perfEvent = await context.PerformanceEvents.FirstOrDefaultAsync(e => e.Slug == "darya-tehran-concert-2023");
        if (perfEvent is null)
        {
            var eventEntity = await CreateEntityAsync(context, 12, "darya-tehran-concert-2023");
            perfEvent = new PerformanceEvent
            {
                EntityId = eventEntity.EntityId,
                EventTypeId = eventTypeConcert?.EventTypeId,
                LocationId = location.LocationId,
                Date = new DateTime(2023, 9, 15, 20, 0, 0),
                PerformanceNotes = "Album-release concert for Midnight Garden.",
                Slug = "darya-tehran-concert-2023",
                IsDeleted = false,
                RowVersion = new byte[8],
                CreatedBy = "seed",
                CreatedAt = now
            };
            context.PerformanceEvents.Add(perfEvent);
            await context.SaveChangesAsync();

            context.PerformanceEventAlbums.Add(new PerformanceEventAlbum { PerformanceEventId = perfEvent.PerformanceEventId, AlbumId = album.AlbumId });
            context.PerformanceEventTracks.Add(new PerformanceEventTrack { PerformanceEventId = perfEvent.PerformanceEventId, TrackId = track.TrackId });
            await context.SaveChangesAsync();
        }

        // ── Award + assignment ──
        var award = await context.Awards.FirstOrDefaultAsync(a => a.Slug == "persian-music-award");
        if (award is null)
        {
            var awardEntity = await CreateEntityAsync(context, 14, "persian-music-award");
            award = new Award
            {
                EntityId = awardEntity.EntityId,
                Name = "Persian Music Award",
                Organization = "Persian Music Academy",
                CountryId = 1,
                Description = "Annual award honoring excellence in Persian music.",
                Slug = "persian-music-award",
                IsDeleted = false,
                RowVersion = new byte[8],
                CreatedBy = "seed",
                CreatedAt = now
            };
            context.Awards.Add(award);
            await context.SaveChangesAsync();
        }

        if (!await context.AwardAssignments.AnyAsync(aa => aa.AwardId == award.AwardId && aa.EntityId == album.EntityId))
        {
            context.AwardAssignments.Add(new AwardAssignment
            {
                AwardId = award.AwardId,
                EntityTypeId = 1, // Album
                EntityId = album.EntityId,
                AwardResultTypeId = awardResultWon?.AwardResultTypeId,
                Year = 2023,
                Category = "Best Instrumental Album",
                Notes = "Awarded for Midnight Garden."
            });
            await context.SaveChangesAsync();
        }

        // ── Chart + entries ──
        var chart = await context.Charts.FirstOrDefaultAsync(c => c.Slug == "iranian-album-chart");
        if (chart is null)
        {
            var chartEntity = await CreateEntityAsync(context, 16, "iranian-album-chart");
            chart = new Chart
            {
                EntityId = chartEntity.EntityId,
                Name = "Iranian Album Chart",
                Publisher = "Music Weekly",
                CountryId = 1,
                Frequency = "Weekly",
                Slug = "iranian-album-chart",
                IsDeleted = false,
                RowVersion = new byte[8],
                CreatedBy = "seed",
                CreatedAt = now
            };
            context.Charts.Add(chart);
            await context.SaveChangesAsync();
        }

        if (!await context.ChartEntries.AnyAsync(ce => ce.ChartId == chart.ChartId && ce.EntityId == album.EntityId))
        {
            context.ChartEntries.AddRange(
                new ChartEntry
                {
                    ChartId = chart.ChartId,
                    EntityTypeId = 1, // Album
                    EntityId = album.EntityId,
                    Date = new DateOnly(2022, 6, 25),
                    Position = 3,
                    PreviousPosition = null,
                    WeeksOnChart = 1
                },
                new ChartEntry
                {
                    ChartId = chart.ChartId,
                    EntityTypeId = 1,
                    EntityId = album.EntityId,
                    Date = new DateOnly(2022, 7, 2),
                    Position = 1,
                    PreviousPosition = 3,
                    WeeksOnChart = 2
                });
            await context.SaveChangesAsync();
        }

        // ── Publication for the poet ──
        if (poet is not null && !await context.Publications.AnyAsync(p => p.Slug == "garden-of-quiet-poems"))
        {
            var pubEntity = await CreateEntityAsync(context, 10, "garden-of-quiet-poems");
            context.Publications.Add(new Publication
            {
                EntityId = pubEntity.EntityId,
                PersonId = poet.PersonId,
                Title = "The Garden of Quiet: Collected Poems",
                PublicationTypeId = publicationTypeBook?.PublicationTypeId,
                PublicationDate = new DateOnly(2024, 1, 12),
                ISBN = "978-600-000-000-0",
                Slug = "garden-of-quiet-poems",
                IsDeleted = false,
                RowVersion = new byte[8],
                CreatedBy = "seed",
                CreatedAt = now
            });
            await context.SaveChangesAsync();
        }

        // ── Musician instruments (artist plays santur & tombak) ──
        if (santur is not null &&
            !await context.MusicianInstruments.AnyAsync(mi => mi.PersonId == artist.PersonId && mi.InstrumentId == santur.InstrumentId))
        {
            context.MusicianInstruments.Add(new MusicianInstrument { PersonId = artist.PersonId, InstrumentId = santur.InstrumentId, Notes = "Principal instrument" });
        }
        if (tombak is not null &&
            !await context.MusicianInstruments.AnyAsync(mi => mi.PersonId == artist.PersonId && mi.InstrumentId == tombak.InstrumentId))
        {
            context.MusicianInstruments.Add(new MusicianInstrument { PersonId = artist.PersonId, InstrumentId = tombak.InstrumentId });
        }
        await context.SaveChangesAsync();

        // ── Track musician credits (drives Track Contributions + timeline TrackRelease) ──
        if (!await context.Credits.AnyAsync(c => c.EntityTypeId == 2 && c.PersonId == artist.PersonId))
        {
            var trackEntity = await context.Entities.FirstOrDefaultAsync(e => e.EntityId == track.EntityId);
            if (trackEntity is not null)
            {
                context.Credits.Add(new Credit
                {
                    EntityTypeId = 2, // Track
                    EntityId = track.EntityId,
                    CreditRoleId = 4, // Musician
                    RoleScopeTypeId = 1,
                    PersonId = artist.PersonId,
                    InstrumentId = santur?.InstrumentId,
                    DisplayOrder = 1,
                    IsPrimary = true,
                    CreatedBy = "seed",
                    CreatedAt = now
                });
            }

            if (otherTrack is not null)
            {
                var otherTrackEntity = await context.Entities.FirstOrDefaultAsync(e => e.EntityId == otherTrack.EntityId);
                if (otherTrackEntity is not null)
                {
                    context.Credits.Add(new Credit
                    {
                        EntityTypeId = 2,
                        EntityId = otherTrack.EntityId,
                        CreditRoleId = 4,
                        RoleScopeTypeId = 1,
                        PersonId = artist.PersonId,
                        InstrumentId = tombak?.InstrumentId,
                        DisplayOrder = 1,
                        IsPrimary = false,
                        CreatedBy = "seed",
                        CreatedAt = now
                    });
                }
            }
            await context.SaveChangesAsync();
        }

        // ── Album relation: The Silent River is the follow-up to Midnight Garden ──
        if (otherAlbum is not null && albumRelationFollowUp is not null &&
            !await context.AlbumRelations.AnyAsync(r => r.AlbumId == album.AlbumId && r.RelatedAlbumId == otherAlbum.AlbumId))
        {
            context.AlbumRelations.Add(new AlbumRelation
            {
                AlbumId = album.AlbumId,
                RelatedAlbumId = otherAlbum.AlbumId,
                AlbumRelationTypeId = albumRelationFollowUp.AlbumRelationTypeId
            });
            await context.SaveChangesAsync();
        }

        // ── Track relation: Silent Petals is the acoustic sibling of Garden of Stars ──
        if (otherTrack is not null && trackRelationAcoustic is not null &&
            !await context.TrackRelations.AnyAsync(r => r.TrackId == track.TrackId && r.RelatedTrackId == otherTrack.TrackId))
        {
            context.TrackRelations.Add(new TrackRelation
            {
                TrackId = track.TrackId,
                RelatedTrackId = otherTrack.TrackId,
                TrackRelationTypeId = trackRelationAcoustic.TrackRelationTypeId
            });
            await context.SaveChangesAsync();
        }
    }

    private static async Task<Entity> CreateEntityAsync(AppDbContext context, int entityTypeId, string slug)
    {
        var entity = new Entity
        {
            EntityTypeId = entityTypeId,
            Slug = slug,
            IsDeleted = false,
            RowVersion = new byte[8], // SQLite has no server-generated rowversion
            CreatedBy = "seed",
            CreatedAt = DateTime.UtcNow
        };
        context.Entities.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    private static async Task<Dictionary<string, int>> SeedGenresAsync(AppDbContext context)
    {
        var ids = new Dictionary<string, int>();
        foreach (var (slug, name, description) in new[]
        {
            ("pop", "Pop", "Mainstream popular music with catchy hooks and broad appeal."),
            ("rock", "Rock", "Guitar-driven popular music with roots in blues and rhythm & blues."),
            ("classical", "Classical", "Art music rooted in formal traditions and written compositions."),
            ("folk", "Folk", "Traditional music passed down through generations and regional styles."),
            ("jazz", "Jazz", "Improvisational music originating in early 20th-century America."),
            ("electronic", "Electronic", "Music created primarily with synthesizers and digital instruments.")
        })
        {
            var entity = await CreateEntityAsync(context, 5, slug);
            var genre = new Genre
            {
                EntityId = entity.EntityId,
                Name = name,
                RowVersion = new byte[8],
                Slug = slug,
                Description = description,
                CreatedBy = "seed",
                CreatedAt = DateTime.UtcNow
            };
            context.Genres.Add(genre);
            await context.SaveChangesAsync();
            ids[slug] = genre.GenreId;
        }
        return ids;
    }

    private static async Task<Dictionary<string, int>> SeedMoodsAsync(AppDbContext context)
    {
        var ids = new Dictionary<string, int>();
        foreach (var (slug, name, description) in new[]
        {
            ("joyful", "Joyful", "Bright and uplifting."),
            ("melancholic", "Melancholic", "Somber and reflective."),
            ("romantic", "Romantic", "Warm and affectionate."),
            ("energetic", "Energetic", "Fast-paced and invigorating."),
            ("calm", "Calm", "Peaceful and soothing.")
        })
        {
            var entity = await CreateEntityAsync(context, 6, slug);
            var mood = new Mood
            {
                EntityId = entity.EntityId,
                Name = name,
                RowVersion = new byte[8],
                Slug = slug,
                Description = description,
                CreatedBy = "seed",
                CreatedAt = DateTime.UtcNow
            };
            context.Moods.Add(mood);
            await context.SaveChangesAsync();
            ids[slug] = mood.MoodId;
        }
        return ids;
    }

    private static async Task<Dictionary<string, int>> SeedInstrumentsAsync(AppDbContext context)
    {
        var ids = new Dictionary<string, int>();
        foreach (var (slug, name, familyId, countryId, description) in new[]
        {
            ("piano", "Piano", (int?)5, (int?)null, "A keyboard instrument with hammers striking strings."),
            ("guitar", "Guitar", (int?)1, (int?)null, "A plucked string instrument with a fretted neck."),
            ("violin", "Violin", (int?)1, (int?)null, "A bowed string instrument, highest in the violin family."),
            ("santur", "Santur", (int?)8, (int?)1, "A Persian hammered dulcimer with trapezoid-shaped soundbox."),
            ("setar", "Setar", (int?)8, (int?)1, "A Persian four-stringed instrument played with the fingertip."),
            ("tombak", "Tombak", (int?)4, (int?)1, "The principal percussion instrument of Persian classical music.")
        })
        {
            var entity = await CreateEntityAsync(context, 7, slug);
            var instrument = new Instrument
            {
                EntityId = entity.EntityId,
                Name = name,
                RowVersion = new byte[8],
                Slug = slug,
                Description = description,
                InstrumentFamilyId = familyId,
                CountryId = countryId,
                CreatedBy = "seed",
                CreatedAt = DateTime.UtcNow
            };
            context.Instruments.Add(instrument);
            await context.SaveChangesAsync();
            ids[slug] = instrument.InstrumentId;
        }
        return ids;
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
            new Language { LanguageId = 1, Code = "fa", Name = "Persian" },
            new Language { LanguageId = 2, Code = "en", Name = "English" },
            new Language { LanguageId = 3, Code = "ar", Name = "Arabic" },
            new Language { LanguageId = 4, Code = "fr", Name = "French" }
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
