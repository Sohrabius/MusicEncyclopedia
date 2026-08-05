-- ============================================================
-- Music Encyclopedia - Read Views for Public Pages
-- ============================================================

-- vAlbumPublic: Albums for public display
CREATE OR ALTER VIEW vAlbumPublic AS
SELECT
    a.AlbumId,
    a.EntityId,
    a.Slug,
    a.Title,
    a.OriginalTitle,
    a.EnglishTitle,
    ac.Name AS CategoryName,
    a.ReleaseDate,
    a.RecordingStartDate,
    a.RecordingEndDate,
    a.Description,
    a.DurationSeconds,
    a.CopyrightNotice,
    a.IsOfficial,
    a.CreatedAt,
    a.ModifiedAt,
    m.Url AS CoverUrl,
    m.ThumbnailUrl300 AS CoverThumbnailUrl
FROM dbo.Album a
LEFT JOIN dbo.AlbumCategory ac ON ac.AlbumCategoryId = a.AlbumCategoryId
LEFT JOIN dbo.Media m ON m.MediaId = a.CoverMediaId AND m.IsDeleted = 0
WHERE a.IsDeleted = 0;

-- vTrackPublic: Tracks for public display
CREATE OR ALTER VIEW vTrackPublic AS
SELECT
    t.TrackId,
    t.EntityId,
    t.Slug,
    t.Title,
    t.OriginalTitle,
    t.EnglishTitle,
    t.DurationSeconds,
    t.IsInstrumental,
    t.IsExplicit,
    t.ISRC,
    t.BPM,
    t.MusicalKeyId,
    t.VocalStyleId,
    t.LyricsAvailabilityTypeId,
    t.ReleaseDate,
    t.RecordingStartDate,
    t.Description,
    t.CopyrightNotice,
    t.CreatedAt,
    t.ModifiedAt,
    lat.Name AS LyricsAvailabilityName,
    mk.Name AS MusicalKeyName,
    vs.Name AS VocalStyleName
FROM dbo.Track t
LEFT JOIN dbo.LyricsAvailabilityType lat ON lat.LyricsAvailabilityTypeId = t.LyricsAvailabilityTypeId
LEFT JOIN dbo.MusicalKey mk ON mk.MusicalKeyId = t.MusicalKeyId
LEFT JOIN dbo.VocalStyle vs ON vs.VocalStyleId = t.VocalStyleId
WHERE t.IsDeleted = 0;

-- vPersonPublic: People for public display
CREATE OR ALTER VIEW vPersonPublic AS
SELECT
    p.PersonId,
    p.EntityId,
    p.Slug,
    p.FullName,
    p.OriginalName,
    p.EnglishName,
    pk.Name AS PersonKindName,
    p.Biography,
    p.BirthDate,
    p.BirthDatePrecision,
    p.DeathDate,
    p.DeathDatePrecision,
    p.NationalityCountryId,
    p.BirthLocationId,
    p.DeathLocationId,
    m.Url AS ImageUrl,
    m.ThumbnailUrl300 AS ImageThumbnailUrl,
    p.CreatedAt,
    p.ModifiedAt
FROM dbo.Person p
LEFT JOIN dbo.PersonKind pk ON pk.PersonKindId = p.PersonKindId
LEFT JOIN dbo.Media m ON m.MediaId = p.ImageMediaId AND m.IsDeleted = 0
WHERE p.IsDeleted = 0;

-- vCompanyPublic: Companies for public display
CREATE OR ALTER VIEW vCompanyPublic AS
SELECT
    c.CompanyId,
    c.EntityId,
    c.Slug,
    c.Name,
    c.OriginalName,
    c.EnglishName,
    ct.Name AS CompanyTypeName,
    co.Name AS CountryName,
    c.Website,
    c.History,
    m.Url AS LogoUrl,
    c.CreatedAt,
    c.ModifiedAt
FROM dbo.Company c
LEFT JOIN dbo.CompanyType ct ON ct.CompanyTypeId = c.CompanyTypeId
LEFT JOIN dbo.Country co ON co.CountryId = c.CountryId
LEFT JOIN dbo.Media m ON m.MediaId = c.EntityId AND m.IsDeleted = 0
WHERE c.IsDeleted = 0;

-- vPoemPublic: Poems for public display
CREATE OR ALTER VIEW vPoemPublic AS
SELECT
    po.PoemId,
    po.EntityId,
    po.Slug,
    po.Title,
    po.OriginalTitle,
    po.EnglishTitle,
    po.PersonId AS PoetId,
    pe.FullName AS PoetName,
    pe.Slug AS PoetSlug,
    po.PublicationId,
    pb.Title AS PublicationTitle,
    po.Book,
    po.OriginalPublicationDate,
    po.CanonicalText,
    po.Copyright,
    po.Source,
    po.ExternalReferenceUrl,
    po.CreatedAt
FROM dbo.Poem po
LEFT JOIN dbo.Person pe ON pe.PersonId = po.PersonId AND pe.IsDeleted = 0
LEFT JOIN dbo.Publication pb ON pb.PublicationId = po.PublicationId AND pb.IsDeleted = 0
WHERE po.IsDeleted = 0;

-- vSungVersionPublic: Sung versions for public display
CREATE OR ALTER VIEW vSungVersionPublic AS
SELECT
    sv.SungVersionId,
    sv.EntityId,
    sv.Slug,
    sv.Title,
    sv.PoemId,
    po.Title AS PoemTitle,
    po.Slug AS PoemSlug,
    sv.VocalStyleId,
    vs.Name AS VocalStyleName,
    sv.Text,
    sv.Notes,
    sv.IsCanonical,
    sv.CreatedAt
FROM dbo.SungVersion sv
LEFT JOIN dbo.Poem po ON po.PoemId = sv.PoemId AND po.IsDeleted = 0
LEFT JOIN dbo.VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
WHERE sv.IsDeleted = 0;

-- vAlbumTrackList: Track list for album detail page
CREATE OR ALTER VIEW vAlbumTrackList AS
SELECT
    at.AlbumTrackId,
    at.AlbumId,
    at.TrackId,
    at.DiscNumber,
    at.TrackNumber,
    at.SequenceNumber,
    ISNULL(at.TrackTitleOverride, t.Title) AS DisplayTitle,
    ISNULL(at.DurationSecondsOverride, t.DurationSeconds) AS DisplayDuration,
    at.IsBonus,
    at.IsHidden,
    t.Slug AS TrackSlug,
    t.IsInstrumental,
    t.IsExplicit
FROM dbo.AlbumTrack at
INNER JOIN dbo.Track t ON t.TrackId = at.TrackId AND t.IsDeleted = 0
WHERE at.AlbumId IS NOT NULL;

-- vAlbumCreditList: Credits for album detail page
CREATE OR ALTER VIEW vAlbumCreditList AS
SELECT
    c.CreditId,
    c.EntityTypeId,
    c.EntityId,
    c.CreditRoleId,
    cr.Code AS RoleCode,
    cr.Name AS RoleName,
    cr.DisplayOrder AS RoleDisplayOrder,
    c.RoleScopeTypeId,
    c.PersonId,
    p.FullName AS PersonFullName,
    p.Slug AS PersonSlug,
    c.CompanyId,
    co.Name AS CompanyName,
    co.Slug AS CompanySlug,
    c.InstrumentId,
    i.Name AS InstrumentName,
    i.Slug AS InstrumentSlug,
    c.DisplayOrder,
    c.IsPrimary,
    c.Notes
FROM dbo.Credit c
INNER JOIN dbo.CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
LEFT JOIN dbo.Person p ON p.PersonId = c.PersonId AND p.IsDeleted = 0
LEFT JOIN dbo.Company co ON co.CompanyId = c.CompanyId AND co.IsDeleted = 0
LEFT JOIN dbo.Instrument i ON i.InstrumentId = c.InstrumentId AND i.IsDeleted = 0;

-- vTrackCreditList: Credits for track detail page
CREATE OR ALTER VIEW vTrackCreditList AS
SELECT
    c.CreditId,
    c.EntityTypeId,
    c.EntityId,
    c.CreditRoleId,
    cr.Code AS RoleCode,
    cr.Name AS RoleName,
    cr.DisplayOrder AS RoleDisplayOrder,
    c.PersonId,
    p.FullName AS PersonFullName,
    p.Slug AS PersonSlug,
    c.CompanyId,
    co.Name AS CompanyName,
    co.Slug AS CompanySlug,
    c.InstrumentId,
    i.Name AS InstrumentName,
    i.Slug AS InstrumentSlug,
    c.DisplayOrder,
    c.IsPrimary,
    c.Notes
FROM dbo.Credit c
INNER JOIN dbo.CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
LEFT JOIN dbo.Person p ON p.PersonId = c.PersonId AND p.IsDeleted = 0
LEFT JOIN dbo.Company co ON co.CompanyId = c.CompanyId AND co.IsDeleted = 0
LEFT JOIN dbo.Instrument i ON i.InstrumentId = c.InstrumentId AND i.IsDeleted = 0;

-- vAlbumMusicianInstrument: Musician + instrument per album
CREATE OR ALTER VIEW vAlbumMusicianInstrument AS
SELECT
    c.CreditId,
    c.EntityId AS AlbumId,
    c.PersonId,
    p.FullName AS PersonName,
    p.Slug AS PersonSlug,
    c.InstrumentId,
    i.Name AS InstrumentName,
    i.Slug AS InstrumentSlug,
    c.Notes
FROM dbo.Credit c
INNER JOIN dbo.CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
LEFT JOIN dbo.Person p ON p.PersonId = c.PersonId AND p.IsDeleted = 0
LEFT JOIN dbo.Instrument i ON i.InstrumentId = c.InstrumentId AND i.IsDeleted = 0
WHERE cr.Code = 'MUSICIAN';

-- vTrackLyricDetail: Lyrics for track detail page
CREATE OR ALTER VIEW vTrackLyricDetail AS
SELECT
    tsv.TrackSungVersionId,
    tsv.TrackId,
    tsv.SungVersionId,
    tsv.SequenceNumber,
    tsv.IsPrimary,
    sv.Title AS SungVersionTitle,
    sv.Slug AS SungVersionSlug,
    sv.Text AS SungText,
    sv.VocalStyleId,
    vs.Name AS VocalStyleName,
    sv.PoemId,
    po.Title AS PoemTitle,
    po.Slug AS PoemSlug,
    po.CanonicalText AS PoemText,
    pe.PersonId AS PoetId,
    pe.FullName AS PoetName,
    pe.Slug AS PoetSlug
FROM dbo.TrackSungVersion tsv
INNER JOIN dbo.SungVersion sv ON sv.SungVersionId = tsv.SungVersionId AND sv.IsDeleted = 0
LEFT JOIN dbo.VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
LEFT JOIN dbo.Poem po ON po.PoemId = sv.PoemId AND po.IsDeleted = 0
LEFT JOIN dbo.Person pe ON pe.PersonId = po.PersonId AND pe.IsDeleted = 0;

-- vEntityLocalization: Localized values for entities
CREATE OR ALTER VIEW vEntityLocalization AS
SELECT
    l.LocalizationId,
    l.EntityTypeId,
    l.EntityId,
    l.LanguageId,
    lg.Code AS LanguageCode,
    l.FieldName,
    l.LocalizedText
FROM dbo.Localization l
LEFT JOIN dbo.Language lg ON lg.LanguageId = l.LanguageId;

-- vEntityAlias: Aliases for entities
CREATE OR ALTER VIEW vEntityAlias AS
SELECT
    a.AliasId,
    a.EntityTypeId,
    a.EntityId,
    a.AliasTypeId,
    at.Code AS AliasTypeCode,
    at.Name AS AliasTypeName,
    a.LanguageId,
    lg.Code AS LanguageCode,
    a.AliasName,
    a.IsPrimary,
    a.Notes
FROM dbo.Alias a
LEFT JOIN dbo.AliasType at ON at.AliasTypeId = a.AliasTypeId
LEFT JOIN dbo.Language lg ON lg.LanguageId = a.LanguageId;

-- vEntityTag: Tags for entities
CREATE OR ALTER VIEW vEntityTag AS
SELECT
    ta.TagAssignmentId,
    ta.EntityTypeId,
    ta.EntityId,
    ta.TagId,
    t.Name AS TagName,
    t.Slug AS TagSlug,
    t.Description AS TagDescription
FROM dbo.TagAssignment ta
INNER JOIN dbo.Tag t ON t.TagId = ta.TagId AND t.IsDeleted = 0;

-- vEntityMedia: Media for entities
CREATE OR ALTER VIEW vEntityMedia AS
SELECT
    ma.MediaAssignmentId,
    ma.EntityTypeId,
    ma.EntityId,
    ma.MediaId,
    m.FileName,
    m.Url,
    m.ThumbnailUrl150,
    m.ThumbnailUrl300,
    m.ThumbnailUrl600,
    m.ThumbnailUrl1200,
    m.Width,
    m.Height,
    m.FileSize,
    m.MimeType,
    m.MediaTypeId,
    mt.Code AS MediaTypeCode,
    ma.MediaRoleTypeId,
    mrt.Code AS MediaRoleCode,
    ma.DisplayOrder,
    ma.IsPrimary,
    m.CreatedAt
FROM dbo.MediaAssignment ma
INNER JOIN dbo.Media m ON m.MediaId = ma.MediaId AND m.IsDeleted = 0
LEFT JOIN dbo.MediaType mt ON mt.MediaTypeId = m.MediaTypeId
LEFT JOIN dbo.MediaRoleType mrt ON mrt.MediaRoleTypeId = ma.MediaRoleTypeId;

-- vEntityLink: External links for entities
CREATE OR ALTER VIEW vEntityLink AS
SELECT
    el.EntityLinkId,
    el.EntityTypeId,
    el.EntityId,
    el.LinkTypeId,
    lt.Code AS LinkTypeCode,
    lt.Name AS LinkTypeName,
    el.Url,
    el.Title
FROM dbo.EntityLink el
LEFT JOIN dbo.LinkType lt ON lt.LinkTypeId = el.LinkTypeId
WHERE el.IsDeleted = 0;

-- vEntityCitation: Citations for entities
CREATE OR ALTER VIEW vEntityCitation AS
SELECT
    ci.CitationId,
    ci.EntityTypeId,
    ci.EntityId,
    ci.SourceId,
    s.Title AS SourceTitle,
    s.Slug AS SourceSlug,
    s.Url AS SourceUrl,
    s.Author AS SourceAuthor,
    ci.FieldName,
    ci.Quote,
    ci.PageNumber,
    ci.Url,
    ci.AccessedDate,
    ci.Notes,
    ci.CreatedAt
FROM dbo.Citation ci
LEFT JOIN dbo.Source s ON s.SourceId = ci.SourceId AND s.IsDeleted = 0;
GO
