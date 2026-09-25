-- ============================================================
-- Music Encyclopedia - Full-Text Search verification
-- ============================================================
-- Run with sqlcmd after FullTextSearch.sql. Pass a term that is known to
-- occur in at least one Album title/description in the target database.
-- Example:
--   sqlcmd ... -i VerifyFullTextSearch.sql -v SearchTerm="known term"

:setvar SearchTerm "Music"

SET NOCOUNT ON;

IF COALESCE(FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'), 0) <> 1
    THROW 51000, 'SQL Server Full-Text Search is not installed.', 1;

IF COALESCE(FULLTEXTSERVICEPROPERTY('IsFullTextEnabled'), 0) <> 1
    THROW 51001, 'SQL Server Full-Text Search is not enabled.', 1;

DECLARE @requiredTables TABLE (TableName sysname NOT NULL PRIMARY KEY);
INSERT INTO @requiredTables (TableName)
VALUES
    (N'Album'), (N'Track'), (N'Person'), (N'Company'), (N'Poem'),
    (N'SungVersion'), (N'Genre'), (N'Mood'), (N'Instrument'), (N'Source'),
    (N'Location'), (N'Publication'), (N'RecordingSession'),
    (N'PerformanceEvent');

IF EXISTS (
    SELECT 1
    FROM @requiredTables AS required
    LEFT JOIN sys.fulltext_indexes AS fti
        ON fti.object_id = OBJECT_ID(N'dbo.' + required.TableName)
       AND fti.is_enabled = 1
       AND fti.change_tracking_state_desc = N'AUTO'
    WHERE fti.object_id IS NULL)
BEGIN
    SELECT
        required.TableName,
        fti.is_enabled,
        fti.change_tracking_state_desc
    FROM @requiredTables AS required
    LEFT JOIN sys.fulltext_indexes AS fti
        ON fti.object_id = OBJECT_ID(N'dbo.' + required.TableName)
    WHERE fti.object_id IS NULL
       OR fti.is_enabled <> 1
       OR fti.change_tracking_state_desc <> N'AUTO';

    THROW 51002, 'A required full-text index is missing, disabled, or not using automatic change tracking.', 1;
END;

SELECT
    OBJECT_SCHEMA_NAME(fti.object_id) AS SchemaName,
    OBJECT_NAME(fti.object_id) AS TableName,
    fti.is_enabled,
    fti.change_tracking_state_desc,
    FULLTEXTCATALOGPROPERTY(N'MusicEncyclopediaCatalog', N'PopulateStatus') AS CatalogPopulateStatus
FROM sys.fulltext_indexes AS fti
JOIN @requiredTables AS required
    ON fti.object_id = OBJECT_ID(N'dbo.' + required.TableName)
ORDER BY TableName;

IF NOT EXISTS (
    SELECT 1
    FROM FREETEXTTABLE(dbo.Album,
        (Title, Description, OriginalTitle, EnglishTitle), N'$(SearchTerm)') AS matches)
    THROW 51003, 'The supplied SearchTerm returned no Album FTS results. Seed a known term and retry.', 1;

SELECT TOP (20)
    album.AlbumId,
    album.Title,
    matches.RANK
FROM FREETEXTTABLE(dbo.Album,
    (Title, Description, OriginalTitle, EnglishTitle), N'$(SearchTerm)') AS matches
JOIN dbo.Album AS album ON album.AlbumId = matches.[KEY]
ORDER BY matches.RANK DESC, album.Title ASC;

PRINT 'Full-text service, required indexes, automatic change tracking, and ranked Album query verified.';
