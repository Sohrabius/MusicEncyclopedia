-- ============================================================
-- Music Encyclopedia - Full-Text Search Catalog and Indexes
-- ============================================================

-- Create full-text catalog
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'MusicEncyclopediaCatalog')
BEGIN
    CREATE FULLTEXT CATALOG MusicEncyclopediaCatalog AS DEFAULT;
END
GO

-- Create full-text indexes on main entity tables

-- Album
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Album'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Album (
        Title LANGUAGE 1033,
        Description LANGUAGE 1033,
        OriginalTitle LANGUAGE 1033,
        EnglishTitle LANGUAGE 1033
    )
    KEY INDEX PK_Album
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Track
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Track'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Track (
        Title LANGUAGE 1033,
        Description LANGUAGE 1033,
        OriginalTitle LANGUAGE 1033,
        EnglishTitle LANGUAGE 1033
    )
    KEY INDEX PK_Track
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Person
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Person'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Person (
        FullName LANGUAGE 1033,
        Biography LANGUAGE 1033,
        OriginalName LANGUAGE 1033,
        EnglishName LANGUAGE 1033
    )
    KEY INDEX PK_Person
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Company
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Company'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Company (
        Name LANGUAGE 1033,
        History LANGUAGE 1033,
        OriginalName LANGUAGE 1033,
        EnglishName LANGUAGE 1033
    )
    KEY INDEX PK_Company
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Poem
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Poem'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Poem (
        Title LANGUAGE 1033,
        CanonicalText LANGUAGE 1033,
        OriginalTitle LANGUAGE 1033,
        EnglishTitle LANGUAGE 1033
    )
    KEY INDEX PK_Poem
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- SungVersion
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.SungVersion'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.SungVersion (
        Title LANGUAGE 1033,
        Text LANGUAGE 1033
    )
    KEY INDEX PK_SungVersion
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Genre
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Genre'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Genre (
        Name LANGUAGE 1033,
        Description LANGUAGE 1033
    )
    KEY INDEX PK_Genre
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Mood
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Mood'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Mood (
        Name LANGUAGE 1033,
        Description LANGUAGE 1033
    )
    KEY INDEX PK_Mood
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Instrument
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Instrument'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Instrument (
        Name LANGUAGE 1033,
        Description LANGUAGE 1033
    )
    KEY INDEX PK_Instrument
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Tag
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Tag'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Tag (
        Name LANGUAGE 1033,
        Description LANGUAGE 1033
    )
    KEY INDEX PK_Tag
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Alias (for search across aliases)
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Alias'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Alias (
        AliasName LANGUAGE 1033
    )
    KEY INDEX PK_Alias
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO

-- Localization (for search across translations)
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Localization'))
BEGIN
    CREATE FULLTEXT INDEX ON dbo.Localization (
        LocalizedText LANGUAGE 1033
    )
    KEY INDEX PK_Localization
    ON MusicEncyclopediaCatalog
    WITH (CHANGE_TRACKING AUTO);
END
GO
