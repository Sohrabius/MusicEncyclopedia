using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Data;

/// <summary>
/// Application data context. Derives from <see cref="IdentityDbContext{TUser,TRole,TKey}"/>
/// so the ASP.NET Core Identity model (AspNetUsers/AspNetRoles/AspNetUserRoles/…
/// tables) is part of the EF model. The tables themselves are created via
/// idempotent DDL in <see cref="Seed.DatabaseInitializer"/> for existing databases
/// (the same pattern already used for <see cref="AuditLog"/>).
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Lookup tables
    public DbSet<EntityType> EntityTypes => Set<EntityType>();
    public DbSet<AlbumCategory> AlbumCategories => Set<AlbumCategory>();
    public DbSet<CompanyType> CompanyTypes => Set<CompanyType>();
    public DbSet<CompanyRoleType> CompanyRoleTypes => Set<CompanyRoleType>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<SourceType> SourceTypes => Set<SourceType>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();
    public DbSet<MediaRoleType> MediaRoleTypes => Set<MediaRoleType>();
    public DbSet<AliasType> AliasTypes => Set<AliasType>();
    public DbSet<LinkType> LinkTypes => Set<LinkType>();
    public DbSet<IdentifierType> IdentifierTypes => Set<IdentifierType>();
    public DbSet<LocationType> LocationTypes => Set<LocationType>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<SessionType> SessionTypes => Set<SessionType>();
    public DbSet<PublicationType> PublicationTypes => Set<PublicationType>();
    public DbSet<AwardResultType> AwardResultTypes => Set<AwardResultType>();
    public DbSet<TrackRelationType> TrackRelationTypes => Set<TrackRelationType>();
    public DbSet<AlbumRelationType> AlbumRelationTypes => Set<AlbumRelationType>();
    public DbSet<TrackVersionType> TrackVersionTypes => Set<TrackVersionType>();
    public DbSet<VocalStyle> VocalStyles => Set<VocalStyle>();
    public DbSet<MusicalKey> MusicalKeys => Set<MusicalKey>();
    public DbSet<LyricsAvailabilityType> LyricsAvailabilityTypes => Set<LyricsAvailabilityType>();
    public DbSet<CountryRoleType> CountryRoleTypes => Set<CountryRoleType>();
    public DbSet<PersonKind> PersonKinds => Set<PersonKind>();
    public DbSet<PersonType> PersonTypes => Set<PersonType>();
    public DbSet<InstrumentFamily> InstrumentFamilies => Set<InstrumentFamily>();
    public DbSet<RoleScopeType> RoleScopeTypes => Set<RoleScopeType>();
    public DbSet<CreditRole> CreditRoles => Set<CreditRole>();

    // Core entities
    public DbSet<Entities.Entity> Entities => Set<Entities.Entity>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<AlbumTrack> AlbumTracks => Set<AlbumTrack>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Credit> Credits => Set<Credit>();
    public DbSet<CreditRoleEntityType> CreditRoleEntityTypes => Set<CreditRoleEntityType>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Mood> Moods => Set<Mood>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<Poem> Poems => Set<Poem>();
    public DbSet<SungVersion> SungVersions => Set<SungVersion>();
    public DbSet<TrackSungVersion> TrackSungVersions => Set<TrackSungVersion>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<RecordingSession> RecordingSessions => Set<RecordingSession>();
    public DbSet<PerformanceEvent> PerformanceEvents => Set<PerformanceEvent>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<MediaAssignment> MediaAssignments => Set<MediaAssignment>();
    public DbSet<EntityLink> EntityLinks => Set<EntityLink>();
    public DbSet<Alias> Aliases => Set<Alias>();
    public DbSet<Localization> Localizations => Set<Localization>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TagAssignment> TagAssignments => Set<TagAssignment>();
    public DbSet<Citation> Citations => Set<Citation>();
    public DbSet<Source> Sources => Set<Source>();
    public DbSet<Award> Awards => Set<Award>();
    public DbSet<AwardAssignment> AwardAssignments => Set<AwardAssignment>();
    public DbSet<Certification> Certifications => Set<Certification>();
    public DbSet<CertificationAssignment> CertificationAssignments => Set<CertificationAssignment>();
    public DbSet<Chart> Charts => Set<Chart>();
    public DbSet<ChartEntry> ChartEntries => Set<ChartEntry>();
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeValue> AttributeValues => Set<AttributeValue>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Join/association tables
    public DbSet<AlbumGenre> AlbumGenres => Set<AlbumGenre>();
    public DbSet<TrackGenre> TrackGenres => Set<TrackGenre>();
    public DbSet<AlbumMood> AlbumMoods => Set<AlbumMood>();
    public DbSet<TrackMood> TrackMoods => Set<TrackMood>();
    public DbSet<TrackInstrument> TrackInstruments => Set<TrackInstrument>();
    public DbSet<MusicianInstrument> MusicianInstruments => Set<MusicianInstrument>();
    public DbSet<RecordingSessionAlbum> RecordingSessionAlbums => Set<RecordingSessionAlbum>();
    public DbSet<RecordingSessionTrack> RecordingSessionTracks => Set<RecordingSessionTrack>();
    public DbSet<PerformanceEventAlbum> PerformanceEventAlbums => Set<PerformanceEventAlbum>();
    public DbSet<PerformanceEventTrack> PerformanceEventTracks => Set<PerformanceEventTrack>();
    public DbSet<TrackRelation> TrackRelations => Set<TrackRelation>();
    public DbSet<AlbumRelation> AlbumRelations => Set<AlbumRelation>();
    public DbSet<TrackVersionTypeAssignment> TrackVersionTypeAssignments => Set<TrackVersionTypeAssignment>();
    public DbSet<PersonTypeAssignment> PersonTypeAssignments => Set<PersonTypeAssignment>();
    public DbSet<AlbumLanguage> AlbumLanguages => Set<AlbumLanguage>();
    public DbSet<AlbumCountry> AlbumCountries => Set<AlbumCountry>();
    public DbSet<AlbumCompany> AlbumCompanies => Set<AlbumCompany>();
    public DbSet<AlbumIdentifier> AlbumIdentifiers => Set<AlbumIdentifier>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // SQLite has no server-generated rowversion type. EF Core's convention marks
        // byte[] RowVersion properties as ValueGeneratedOnAddOrUpdate, which omits them
        // from INSERT statements (expecting the database to fill them). On SQLite that
        // leaves the NOT NULL column unset and every insert fails, so tell EF to persist
        // the explicitly-provided values instead.
        if (Database.IsSqlite())
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var rowVersion = entityType.FindProperty("RowVersion");
                if (rowVersion is not null && rowVersion.ClrType == typeof(byte[]))
                {
                    rowVersion.ValueGenerated = ValueGenerated.Never;
                }
            }
        }

        ConfigureLookupTables(modelBuilder);
        ConfigureCoreEntities(modelBuilder);
        ConfigureJoinTables(modelBuilder);
        ConfigurePolymorphicAssociations(modelBuilder);
    }

    private static void ConfigureLookupTables(ModelBuilder modelBuilder)
    {
        // All lookup tables with Code+Name get unique index on Code
        var lookupTypes = new[]
        {
            typeof(EntityType), typeof(AlbumCategory), typeof(CompanyType),
            typeof(CompanyRoleType), typeof(Language), typeof(Country),
            typeof(SourceType), typeof(MediaType), typeof(MediaRoleType),
            typeof(AliasType), typeof(LinkType), typeof(IdentifierType),
            typeof(LocationType), typeof(EventType), typeof(SessionType),
            typeof(PublicationType), typeof(AwardResultType), typeof(TrackRelationType),
            typeof(AlbumRelationType), typeof(TrackVersionType), typeof(VocalStyle),
            typeof(MusicalKey), typeof(LyricsAvailabilityType), typeof(CountryRoleType),
            typeof(PersonKind), typeof(PersonType), typeof(InstrumentFamily),
            typeof(RoleScopeType)
        };

        foreach (var type in lookupTypes)
        {
            var entity = modelBuilder.Entity(type);
            entity.HasIndex("Code").IsUnique().HasDatabaseName($"IX_{type.Name}_Code");
        }

        // Unique index on CreditRole.Code
        modelBuilder.Entity<CreditRole>()
            .HasIndex(cr => cr.Code)
            .IsUnique()
            .HasDatabaseName("IX_CreditRole_Code");
    }

    private static void ConfigureCoreEntities(ModelBuilder modelBuilder)
    {
        // ---- Entity ----
        modelBuilder.Entity<Entities.Entity>(entity =>
        {
            entity.HasIndex(e => e.EntityTypeId).HasDatabaseName("IX_Entity_EntityTypeId");
            entity.HasIndex(e => e.Slug).HasDatabaseName("IX_Entity_Slug");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ---- Album ----
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasIndex(a => a.EntityId).IsUnique().HasDatabaseName("IX_Album_EntityId");
            entity.HasIndex(a => a.AlbumCategoryId).HasDatabaseName("IX_Album_AlbumCategoryId");
            entity.HasIndex(a => a.CoverMediaId).HasDatabaseName("IX_Album_CoverMediaId");
            entity.HasIndex(a => a.Slug).IsUnique().HasDatabaseName("IX_Album_Slug");
            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ---- Track ----
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasIndex(t => t.EntityId).IsUnique().HasDatabaseName("IX_Track_EntityId");
            entity.HasIndex(t => t.LyricsAvailabilityTypeId).HasDatabaseName("IX_Track_LyricsAvailabilityTypeId");
            entity.HasIndex(t => t.VocalStyleId).HasDatabaseName("IX_Track_VocalStyleId");
            entity.HasIndex(t => t.MusicalKeyId).HasDatabaseName("IX_Track_MusicalKeyId");
            entity.HasIndex(t => t.Slug).IsUnique().HasDatabaseName("IX_Track_Slug");
            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        // ---- Person ----
        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasOne(p => p.BirthLocation).WithMany(l => l.PeopleBornHere).HasForeignKey(p => p.BirthLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(p => p.DeathLocation).WithMany(l => l.PeopleDiedHere).HasForeignKey(p => p.DeathLocationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(p => p.EntityId).IsUnique().HasDatabaseName("IX_Person_EntityId");
            entity.HasIndex(p => p.PersonKindId).HasDatabaseName("IX_Person_PersonKindId");
            entity.HasIndex(p => p.BirthLocationId).HasDatabaseName("IX_Person_BirthLocationId");
            entity.HasIndex(p => p.DeathLocationId).HasDatabaseName("IX_Person_DeathLocationId");
            entity.HasIndex(p => p.NationalityCountryId).HasDatabaseName("IX_Person_NationalityCountryId");
            entity.HasIndex(p => p.ImageMediaId).HasDatabaseName("IX_Person_ImageMediaId");
            entity.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("IX_Person_Slug");
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ---- Company ----
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(c => c.EntityId).IsUnique().HasDatabaseName("IX_Company_EntityId");
            entity.HasIndex(c => c.CompanyTypeId).HasDatabaseName("IX_Company_CompanyTypeId");
            entity.HasIndex(c => c.CountryId).HasDatabaseName("IX_Company_CountryId");
            entity.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("IX_Company_Slug");
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // ---- Genre ----
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasIndex(g => g.EntityId).IsUnique().HasDatabaseName("IX_Genre_EntityId");
            entity.HasIndex(g => g.ParentGenreId).HasDatabaseName("IX_Genre_ParentGenreId");
            entity.HasIndex(g => g.Slug).IsUnique().HasDatabaseName("IX_Genre_Slug");
            entity.HasQueryFilter(g => !g.IsDeleted);
        });

        // ---- Mood ----
        modelBuilder.Entity<Mood>(entity =>
        {
            entity.HasIndex(m => m.EntityId).IsUnique().HasDatabaseName("IX_Mood_EntityId");
            entity.HasIndex(m => m.Slug).IsUnique().HasDatabaseName("IX_Mood_Slug");
            entity.HasQueryFilter(m => !m.IsDeleted);
        });

        // ---- Instrument ----
        modelBuilder.Entity<Instrument>(entity =>
        {
            entity.HasIndex(i => i.EntityId).IsUnique().HasDatabaseName("IX_Instrument_EntityId");
            entity.HasIndex(i => i.InstrumentFamilyId).HasDatabaseName("IX_Instrument_InstrumentFamilyId");
            entity.HasIndex(i => i.CountryId).HasDatabaseName("IX_Instrument_CountryId");
            entity.HasIndex(i => i.Slug).IsUnique().HasDatabaseName("IX_Instrument_Slug");
            entity.HasQueryFilter(i => !i.IsDeleted);
        });

        // ---- Poem ----
        modelBuilder.Entity<Poem>(entity =>
        {
            entity.HasIndex(p => p.EntityId).IsUnique().HasDatabaseName("IX_Poem_EntityId");
            entity.HasIndex(p => p.PersonId).HasDatabaseName("IX_Poem_PersonId");
            entity.HasIndex(p => p.PublicationId).HasDatabaseName("IX_Poem_PublicationId");
            entity.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("IX_Poem_Slug");
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ---- SungVersion ----
        modelBuilder.Entity<SungVersion>(entity =>
        {
            entity.HasIndex(sv => sv.EntityId).IsUnique().HasDatabaseName("IX_SungVersion_EntityId");
            entity.HasIndex(sv => sv.PoemId).HasDatabaseName("IX_SungVersion_PoemId");
            entity.HasIndex(sv => sv.VocalStyleId).HasDatabaseName("IX_SungVersion_VocalStyleId");
            entity.HasIndex(sv => sv.Slug).IsUnique().HasDatabaseName("IX_SungVersion_Slug");
            entity.HasQueryFilter(sv => !sv.IsDeleted);
        });

        // ---- Publication ----
        modelBuilder.Entity<Publication>(entity =>
        {
            entity.HasIndex(p => p.EntityId).IsUnique().HasDatabaseName("IX_Publication_EntityId");
            entity.HasIndex(p => p.PersonId).HasDatabaseName("IX_Publication_PersonId");
            entity.HasIndex(p => p.PublicationTypeId).HasDatabaseName("IX_Publication_PublicationTypeId");
            entity.HasIndex(p => p.PublisherId).HasDatabaseName("IX_Publication_PublisherId");
            entity.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("IX_Publication_Slug");
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // ---- RecordingSession ----
        modelBuilder.Entity<RecordingSession>(entity =>
        {
            entity.HasIndex(rs => rs.EntityId).IsUnique().HasDatabaseName("IX_RecordingSession_EntityId");
            entity.HasIndex(rs => rs.SessionTypeId).HasDatabaseName("IX_RecordingSession_SessionTypeId");
            entity.HasIndex(rs => rs.LocationId).HasDatabaseName("IX_RecordingSession_LocationId");
            entity.HasIndex(rs => rs.Slug).IsUnique().HasDatabaseName("IX_RecordingSession_Slug");
            entity.HasQueryFilter(rs => !rs.IsDeleted);
        });

        // ---- PerformanceEvent ----
        modelBuilder.Entity<PerformanceEvent>(entity =>
        {
            entity.HasIndex(pe => pe.EntityId).IsUnique().HasDatabaseName("IX_PerformanceEvent_EntityId");
            entity.HasIndex(pe => pe.EventTypeId).HasDatabaseName("IX_PerformanceEvent_EventTypeId");
            entity.HasIndex(pe => pe.LocationId).HasDatabaseName("IX_PerformanceEvent_LocationId");
            entity.HasIndex(pe => pe.Slug).IsUnique().HasDatabaseName("IX_PerformanceEvent_Slug");
            entity.HasQueryFilter(pe => !pe.IsDeleted);
        });

        // ---- Location ----
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasIndex(l => l.EntityId).IsUnique().HasDatabaseName("IX_Location_EntityId");
            entity.HasIndex(l => l.LocationTypeId).HasDatabaseName("IX_Location_LocationTypeId");
            entity.HasIndex(l => l.ParentLocationId).HasDatabaseName("IX_Location_ParentLocationId");
            entity.HasIndex(l => l.CountryId).HasDatabaseName("IX_Location_CountryId");
            entity.HasIndex(l => l.Slug).IsUnique().HasDatabaseName("IX_Location_Slug");
            entity.HasQueryFilter(l => !l.IsDeleted);
        });

        // ---- Tag ----
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(t => t.EntityId).IsUnique().HasDatabaseName("IX_Tag_EntityId");
            entity.HasIndex(t => t.Slug).IsUnique().HasDatabaseName("IX_Tag_Slug");
            entity.HasQueryFilter(t => !t.IsDeleted);
        });

        // ---- Source ----
        modelBuilder.Entity<Source>(entity =>
        {
            entity.HasIndex(s => s.EntityId).IsUnique().HasDatabaseName("IX_Source_EntityId");
            entity.HasIndex(s => s.SourceTypeId).HasDatabaseName("IX_Source_SourceTypeId");
            entity.HasIndex(s => s.PublisherId).HasDatabaseName("IX_Source_PublisherId");
            entity.HasIndex(s => s.Slug).IsUnique().HasDatabaseName("IX_Source_Slug");
            entity.HasQueryFilter(s => !s.IsDeleted);
        });

        // ---- Award ----
        modelBuilder.Entity<Award>(entity =>
        {
            entity.HasIndex(a => a.EntityId).IsUnique().HasDatabaseName("IX_Award_EntityId");
            entity.HasIndex(a => a.CountryId).HasDatabaseName("IX_Award_CountryId");
            entity.HasIndex(a => a.Slug).IsUnique().HasDatabaseName("IX_Award_Slug");
            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ---- Certification ----
        modelBuilder.Entity<Certification>(entity =>
        {
            entity.HasIndex(c => c.EntityId).IsUnique().HasDatabaseName("IX_Certification_EntityId");
            entity.HasIndex(c => c.CountryId).HasDatabaseName("IX_Certification_CountryId");
            entity.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("IX_Certification_Slug");
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // ---- Chart ----
        modelBuilder.Entity<Chart>(entity =>
        {
            entity.HasIndex(c => c.EntityId).IsUnique().HasDatabaseName("IX_Chart_EntityId");
            entity.HasIndex(c => c.CountryId).HasDatabaseName("IX_Chart_CountryId");
            entity.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("IX_Chart_Slug");
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // ---- Media ----
        modelBuilder.Entity<Media>(entity =>
        {
            entity.HasIndex(m => m.MediaTypeId).HasDatabaseName("IX_Media_MediaTypeId");
            entity.HasQueryFilter(m => !m.IsDeleted);
        });

        // ---- AlbumTrack ----
        modelBuilder.Entity<AlbumTrack>(entity =>
        {
            entity.HasIndex(at => at.AlbumId).HasDatabaseName("IX_AlbumTrack_AlbumId");
            entity.HasIndex(at => at.TrackId).HasDatabaseName("IX_AlbumTrack_TrackId");
            entity.HasIndex(at => new { at.AlbumId, at.TrackId }).IsUnique().HasDatabaseName("IX_AlbumTrack_AlbumId_TrackId");
        });

        // ---- Credit ----
        modelBuilder.Entity<Credit>(entity =>
        {
            // Restrict (not cascade): deleting a role or scope lookup must not
            // silently delete Credits. Also avoids SQL Server error 1785 —
            // RoleScopeType would otherwise reach Credit via two cascade paths
            // (direct + through CreditRole).
            entity.HasOne(c => c.CreditRole)
                .WithMany(r => r.Credits)
                .HasForeignKey(c => c.CreditRoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(c => c.RoleScopeType)
                .WithMany(r => r.Credits)
                .HasForeignKey(c => c.RoleScopeTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(c => c.CreditRoleId).HasDatabaseName("IX_Credit_CreditRoleId");
            entity.HasIndex(c => c.RoleScopeTypeId).HasDatabaseName("IX_Credit_RoleScopeTypeId");
            entity.HasIndex(c => c.PersonId).HasDatabaseName("IX_Credit_PersonId");
            entity.HasIndex(c => c.CompanyId).HasDatabaseName("IX_Credit_CompanyId");
            entity.HasIndex(c => c.InstrumentId).HasDatabaseName("IX_Credit_InstrumentId");
            entity.HasIndex(c => new { c.EntityTypeId, c.EntityId }).HasDatabaseName("IX_Credit_EntityTypeId_EntityId");
        });

        // ---- CreditRoleEntityType ----
        modelBuilder.Entity<CreditRoleEntityType>(entity =>
        {
            entity.HasIndex(c => c.CreditRoleId).HasDatabaseName("IX_CreditRoleEntityType_CreditRoleId");
            entity.HasIndex(c => c.EntityTypeId).HasDatabaseName("IX_CreditRoleEntityType_EntityTypeId");
            entity.HasIndex(c => new { c.CreditRoleId, c.EntityTypeId }).IsUnique().HasDatabaseName("IX_CreditRoleEntityType_CreditRoleId_EntityTypeId");
        });

        // ---- EntityLink ----
        modelBuilder.Entity<EntityLink>(entity =>
        {
            entity.HasIndex(el => el.LinkTypeId).HasDatabaseName("IX_EntityLink_LinkTypeId");
            entity.HasIndex(el => new { el.EntityTypeId, el.EntityId }).HasDatabaseName("IX_EntityLink_EntityTypeId_EntityId");
            entity.HasQueryFilter(el => !el.IsDeleted);
        });

        // ---- Alias ----
        modelBuilder.Entity<Alias>(entity =>
        {
            entity.HasIndex(a => a.AliasTypeId).HasDatabaseName("IX_Alias_AliasTypeId");
            entity.HasIndex(a => a.LanguageId).HasDatabaseName("IX_Alias_LanguageId");
            entity.HasIndex(a => new { a.EntityTypeId, a.EntityId }).HasDatabaseName("IX_Alias_EntityTypeId_EntityId");
        });

        // ---- Localization ----
        modelBuilder.Entity<Localization>(entity =>
        {
            entity.HasIndex(l => l.LanguageId).HasDatabaseName("IX_Localization_LanguageId");
            entity.HasIndex(l => new { l.EntityTypeId, l.EntityId }).HasDatabaseName("IX_Localization_EntityTypeId_EntityId");
            entity.HasIndex(l => new { l.EntityTypeId, l.EntityId, l.LanguageId, l.FieldName }).IsUnique().HasDatabaseName("IX_Localization_Unique");
        });

        // ---- TagAssignment ----
        modelBuilder.Entity<TagAssignment>(entity =>
        {
            entity.HasIndex(ta => ta.TagId).HasDatabaseName("IX_TagAssignment_TagId");
            entity.HasIndex(ta => new { ta.EntityTypeId, ta.EntityId }).HasDatabaseName("IX_TagAssignment_EntityTypeId_EntityId");
            entity.HasIndex(ta => new { ta.EntityTypeId, ta.EntityId, ta.TagId }).IsUnique().HasDatabaseName("IX_TagAssignment_Unique");
        });

        // ---- Citation ----
        modelBuilder.Entity<Citation>(entity =>
        {
            entity.HasIndex(c => c.SourceId).HasDatabaseName("IX_Citation_SourceId");
            entity.HasIndex(c => new { c.EntityTypeId, c.EntityId }).HasDatabaseName("IX_Citation_EntityTypeId_EntityId");
        });

        // ---- AttributeValue ----
        modelBuilder.Entity<AttributeValue>(entity =>
        {
            entity.HasIndex(av => av.AttributeDefinitionId).HasDatabaseName("IX_AttributeValue_AttributeDefinitionId");
            entity.HasIndex(av => av.LanguageId).HasDatabaseName("IX_AttributeValue_LanguageId");
            entity.HasIndex(av => new { av.EntityTypeId, av.EntityId }).HasDatabaseName("IX_AttributeValue_EntityTypeId_EntityId");
        });

        // ---- MediaAssignment ----
        modelBuilder.Entity<MediaAssignment>(entity =>
        {
            entity.HasIndex(ma => ma.MediaId).HasDatabaseName("IX_MediaAssignment_MediaId");
            entity.HasIndex(ma => ma.MediaRoleTypeId).HasDatabaseName("IX_MediaAssignment_MediaRoleTypeId");
            entity.HasIndex(ma => new { ma.EntityTypeId, ma.EntityId }).HasDatabaseName("IX_MediaAssignment_EntityTypeId_EntityId");
        });

        // ---- AwardAssignment ----
        modelBuilder.Entity<AwardAssignment>(entity =>
        {
            entity.HasIndex(aa => aa.AwardId).HasDatabaseName("IX_AwardAssignment_AwardId");
            entity.HasIndex(aa => aa.AwardResultTypeId).HasDatabaseName("IX_AwardAssignment_AwardResultTypeId");
            entity.HasIndex(aa => new { aa.EntityTypeId, aa.EntityId }).HasDatabaseName("IX_AwardAssignment_EntityTypeId_EntityId");
        });

        // ---- CertificationAssignment ----
        modelBuilder.Entity<CertificationAssignment>(entity =>
        {
            entity.HasIndex(ca => ca.CertificationId).HasDatabaseName("IX_CertificationAssignment_CertificationId");
            entity.HasIndex(ca => new { ca.EntityTypeId, ca.EntityId }).HasDatabaseName("IX_CertificationAssignment_EntityTypeId_EntityId");
        });

        // ---- ChartEntry ----
        modelBuilder.Entity<ChartEntry>(entity =>
        {
            entity.HasIndex(ce => ce.ChartId).HasDatabaseName("IX_ChartEntry_ChartId");
            entity.HasIndex(ce => new { ce.EntityTypeId, ce.EntityId }).HasDatabaseName("IX_ChartEntry_EntityTypeId_EntityId");
            entity.HasIndex(ce => new { ce.ChartId, ce.Date, ce.EntityTypeId, ce.EntityId }).IsUnique().HasDatabaseName("IX_ChartEntry_Unique");
        });

        // ---- AuditLog ----
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(al => al.Timestamp).HasDatabaseName("IX_AuditLog_Timestamp");
            entity.HasIndex(al => new { al.EntityType, al.EntityId }).HasDatabaseName("IX_AuditLog_EntityType_EntityId");
        });
    }

    private static void ConfigureJoinTables(ModelBuilder modelBuilder)
    {
        // Many-to-many join tables with unique composite indexes

        modelBuilder.Entity<AlbumGenre>(entity =>
        {
            entity.HasIndex(ag => ag.AlbumId).HasDatabaseName("IX_AlbumGenre_AlbumId");
            entity.HasIndex(ag => ag.GenreId).HasDatabaseName("IX_AlbumGenre_GenreId");
            entity.HasIndex(ag => new { ag.AlbumId, ag.GenreId }).IsUnique().HasDatabaseName("IX_AlbumGenre_Unique");
        });

        modelBuilder.Entity<TrackGenre>(entity =>
        {
            entity.HasIndex(tg => tg.TrackId).HasDatabaseName("IX_TrackGenre_TrackId");
            entity.HasIndex(tg => tg.GenreId).HasDatabaseName("IX_TrackGenre_GenreId");
            entity.HasIndex(tg => new { tg.TrackId, tg.GenreId }).IsUnique().HasDatabaseName("IX_TrackGenre_Unique");
        });

        modelBuilder.Entity<AlbumMood>(entity =>
        {
            entity.HasIndex(am => am.AlbumId).HasDatabaseName("IX_AlbumMood_AlbumId");
            entity.HasIndex(am => am.MoodId).HasDatabaseName("IX_AlbumMood_MoodId");
            entity.HasIndex(am => new { am.AlbumId, am.MoodId }).IsUnique().HasDatabaseName("IX_AlbumMood_Unique");
        });

        modelBuilder.Entity<TrackMood>(entity =>
        {
            entity.HasIndex(tm => tm.TrackId).HasDatabaseName("IX_TrackMood_TrackId");
            entity.HasIndex(tm => tm.MoodId).HasDatabaseName("IX_TrackMood_MoodId");
            entity.HasIndex(tm => new { tm.TrackId, tm.MoodId }).IsUnique().HasDatabaseName("IX_TrackMood_Unique");
        });

        modelBuilder.Entity<TrackInstrument>(entity =>
        {
            entity.HasIndex(ti => ti.TrackId).HasDatabaseName("IX_TrackInstrument_TrackId");
            entity.HasIndex(ti => ti.InstrumentId).HasDatabaseName("IX_TrackInstrument_InstrumentId");
            entity.HasIndex(ti => new { ti.TrackId, ti.InstrumentId }).IsUnique().HasDatabaseName("IX_TrackInstrument_Unique");
        });

        modelBuilder.Entity<MusicianInstrument>(entity =>
        {
            entity.HasIndex(mi => mi.PersonId).HasDatabaseName("IX_MusicianInstrument_PersonId");
            entity.HasIndex(mi => mi.InstrumentId).HasDatabaseName("IX_MusicianInstrument_InstrumentId");
            entity.HasIndex(mi => new { mi.PersonId, mi.InstrumentId }).IsUnique().HasDatabaseName("IX_MusicianInstrument_Unique");
        });

        modelBuilder.Entity<TrackSungVersion>(entity =>
        {
            entity.HasIndex(tsv => tsv.TrackId).HasDatabaseName("IX_TrackSungVersion_TrackId");
            entity.HasIndex(tsv => tsv.SungVersionId).HasDatabaseName("IX_TrackSungVersion_SungVersionId");
            entity.HasIndex(tsv => new { tsv.TrackId, tsv.SungVersionId }).IsUnique().HasDatabaseName("IX_TrackSungVersion_Unique");
        });

        modelBuilder.Entity<RecordingSessionAlbum>(entity =>
        {
            entity.HasIndex(rsa => rsa.RecordingSessionId).HasDatabaseName("IX_RecordingSessionAlbum_RecordingSessionId");
            entity.HasIndex(rsa => rsa.AlbumId).HasDatabaseName("IX_RecordingSessionAlbum_AlbumId");
            entity.HasIndex(rsa => new { rsa.RecordingSessionId, rsa.AlbumId }).IsUnique().HasDatabaseName("IX_RecordingSessionAlbum_Unique");
        });

        modelBuilder.Entity<RecordingSessionTrack>(entity =>
        {
            entity.HasIndex(rst => rst.RecordingSessionId).HasDatabaseName("IX_RecordingSessionTrack_RecordingSessionId");
            entity.HasIndex(rst => rst.TrackId).HasDatabaseName("IX_RecordingSessionTrack_TrackId");
            entity.HasIndex(rst => new { rst.RecordingSessionId, rst.TrackId }).IsUnique().HasDatabaseName("IX_RecordingSessionTrack_Unique");
        });

        modelBuilder.Entity<PerformanceEventAlbum>(entity =>
        {
            entity.HasIndex(pea => pea.PerformanceEventId).HasDatabaseName("IX_PerformanceEventAlbum_PerformanceEventId");
            entity.HasIndex(pea => pea.AlbumId).HasDatabaseName("IX_PerformanceEventAlbum_AlbumId");
            entity.HasIndex(pea => new { pea.PerformanceEventId, pea.AlbumId }).IsUnique().HasDatabaseName("IX_PerformanceEventAlbum_Unique");
        });

        modelBuilder.Entity<PerformanceEventTrack>(entity =>
        {
            entity.HasIndex(pet => pet.PerformanceEventId).HasDatabaseName("IX_PerformanceEventTrack_PerformanceEventId");
            entity.HasIndex(pet => pet.TrackId).HasDatabaseName("IX_PerformanceEventTrack_TrackId");
            entity.HasIndex(pet => new { pet.PerformanceEventId, pet.TrackId }).IsUnique().HasDatabaseName("IX_PerformanceEventTrack_Unique");
        });

        modelBuilder.Entity<TrackRelation>(entity =>
        {
            entity.HasOne(tr => tr.Track).WithMany(t => t.TrackRelations).HasForeignKey(tr => tr.TrackId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(tr => tr.RelatedTrack).WithMany(t => t.RelatedTrackRelations).HasForeignKey(tr => tr.RelatedTrackId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(tr => tr.TrackId).HasDatabaseName("IX_TrackRelation_TrackId");
            entity.HasIndex(tr => tr.RelatedTrackId).HasDatabaseName("IX_TrackRelation_RelatedTrackId");
            entity.HasIndex(tr => tr.TrackRelationTypeId).HasDatabaseName("IX_TrackRelation_TrackRelationTypeId");
            entity.HasIndex(tr => new { tr.TrackId, tr.RelatedTrackId, tr.TrackRelationTypeId }).IsUnique().HasDatabaseName("IX_TrackRelation_Unique");
        });

        modelBuilder.Entity<AlbumRelation>(entity =>
        {
            entity.HasOne(ar => ar.Album).WithMany(a => a.AlbumRelations).HasForeignKey(ar => ar.AlbumId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ar => ar.RelatedAlbum).WithMany(a => a.RelatedAlbumRelations).HasForeignKey(ar => ar.RelatedAlbumId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(ar => ar.AlbumId).HasDatabaseName("IX_AlbumRelation_AlbumId");
            entity.HasIndex(ar => ar.RelatedAlbumId).HasDatabaseName("IX_AlbumRelation_RelatedAlbumId");
            entity.HasIndex(ar => ar.AlbumRelationTypeId).HasDatabaseName("IX_AlbumRelation_AlbumRelationTypeId");
            entity.HasIndex(ar => new { ar.AlbumId, ar.RelatedAlbumId, ar.AlbumRelationTypeId }).IsUnique().HasDatabaseName("IX_AlbumRelation_Unique");
        });

        modelBuilder.Entity<TrackVersionTypeAssignment>(entity =>
        {
            entity.HasIndex(tvta => tvta.TrackId).HasDatabaseName("IX_TrackVersionTypeAssignment_TrackId");
            entity.HasIndex(tvta => tvta.TrackVersionTypeId).HasDatabaseName("IX_TrackVersionTypeAssignment_TrackVersionTypeId");
            entity.HasIndex(tvta => new { tvta.TrackId, tvta.TrackVersionTypeId }).IsUnique().HasDatabaseName("IX_TrackVersionTypeAssignment_Unique");
        });

        modelBuilder.Entity<PersonTypeAssignment>(entity =>
        {
            entity.HasIndex(pta => pta.PersonId).HasDatabaseName("IX_PersonTypeAssignment_PersonId");
            entity.HasIndex(pta => pta.PersonTypeId).HasDatabaseName("IX_PersonTypeAssignment_PersonTypeId");
            entity.HasIndex(pta => new { pta.PersonId, pta.PersonTypeId }).IsUnique().HasDatabaseName("IX_PersonTypeAssignment_Unique");
        });

        modelBuilder.Entity<AlbumLanguage>(entity =>
        {
            entity.HasIndex(al => al.AlbumId).HasDatabaseName("IX_AlbumLanguage_AlbumId");
            entity.HasIndex(al => al.LanguageId).HasDatabaseName("IX_AlbumLanguage_LanguageId");
            entity.HasIndex(al => new { al.AlbumId, al.LanguageId }).IsUnique().HasDatabaseName("IX_AlbumLanguage_Unique");
        });

        modelBuilder.Entity<AlbumCountry>(entity =>
        {
            entity.HasIndex(ac => ac.AlbumId).HasDatabaseName("IX_AlbumCountry_AlbumId");
            entity.HasIndex(ac => ac.CountryId).HasDatabaseName("IX_AlbumCountry_CountryId");
            entity.HasIndex(ac => ac.CountryRoleTypeId).HasDatabaseName("IX_AlbumCountry_CountryRoleTypeId");
            entity.HasIndex(ac => new { ac.AlbumId, ac.CountryId }).IsUnique().HasDatabaseName("IX_AlbumCountry_Unique");
        });

        modelBuilder.Entity<AlbumCompany>(entity =>
        {
            entity.HasIndex(ac => ac.AlbumId).HasDatabaseName("IX_AlbumCompany_AlbumId");
            entity.HasIndex(ac => ac.CompanyId).HasDatabaseName("IX_AlbumCompany_CompanyId");
            entity.HasIndex(ac => ac.CompanyRoleTypeId).HasDatabaseName("IX_AlbumCompany_CompanyRoleTypeId");
        });

        modelBuilder.Entity<AlbumIdentifier>(entity =>
        {
            entity.HasIndex(ai => ai.AlbumId).HasDatabaseName("IX_AlbumIdentifier_AlbumId");
            entity.HasIndex(ai => ai.IdentifierTypeId).HasDatabaseName("IX_AlbumIdentifier_IdentifierTypeId");
            entity.HasIndex(ai => new { ai.AlbumId, ai.IdentifierTypeId, ai.Value }).IsUnique().HasDatabaseName("IX_AlbumIdentifier_Unique");
        });
    }

    private static void ConfigurePolymorphicAssociations(ModelBuilder modelBuilder)
    {
        // EntityTypeId + EntityId indexes are defined per-table above.
        // Entity -> sub-type 1:1 relationships
        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Album)
            .WithOne(a => a.Entity)
            .HasForeignKey<Album>(a => a.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Track)
            .WithOne(t => t.Entity)
            .HasForeignKey<Track>(t => t.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Person)
            .WithOne(p => p.Entity)
            .HasForeignKey<Person>(p => p.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Company)
            .WithOne(c => c.Entity)
            .HasForeignKey<Company>(c => c.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Genre)
            .WithOne(g => g.Entity)
            .HasForeignKey<Genre>(g => g.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Mood)
            .WithOne(m => m.Entity)
            .HasForeignKey<Mood>(m => m.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Instrument)
            .WithOne(i => i.Entity)
            .HasForeignKey<Instrument>(i => i.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Poem)
            .WithOne(p => p.Entity)
            .HasForeignKey<Poem>(p => p.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.SungVersion)
            .WithOne(sv => sv.Entity)
            .HasForeignKey<SungVersion>(sv => sv.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Publication)
            .WithOne(p => p.Entity)
            .HasForeignKey<Publication>(p => p.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.RecordingSession)
            .WithOne(rs => rs.Entity)
            .HasForeignKey<RecordingSession>(rs => rs.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.PerformanceEvent)
            .WithOne(pe => pe.Entity)
            .HasForeignKey<PerformanceEvent>(pe => pe.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Location)
            .WithOne(l => l.Entity)
            .HasForeignKey<Location>(l => l.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Tag)
            .WithOne(t => t.Entity)
            .HasForeignKey<Tag>(t => t.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Source)
            .WithOne(s => s.Entity)
            .HasForeignKey<Source>(s => s.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Award)
            .WithOne(a => a.Entity)
            .HasForeignKey<Award>(a => a.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Certification)
            .WithOne(c => c.Entity)
            .HasForeignKey<Certification>(c => c.EntityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entities.Entity>()
            .HasOne(e => e.Chart)
            .WithOne(c => c.Entity)
            .HasForeignKey<Chart>(c => c.EntityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
