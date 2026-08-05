-- ============================================================
-- Music Encyclopedia - Seed Lookup Data
-- ============================================================

-- EntityType
INSERT INTO EntityType (EntityTypeId, Code, Name) VALUES
(1, 'Album', 'Album'),
(2, 'Track', 'Track'),
(3, 'Person', 'Person'),
(4, 'Company', 'Company'),
(5, 'Genre', 'Genre'),
(6, 'Mood', 'Mood'),
(7, 'Instrument', 'Instrument'),
(8, 'Poem', 'Poem'),
(9, 'SungVersion', 'Sung Version'),
(10, 'Publication', 'Publication'),
(11, 'RecordingSession', 'Recording Session'),
(12, 'PerformanceEvent', 'Performance Event'),
(13, 'Location', 'Location'),
(14, 'Award', 'Award'),
(15, 'Certification', 'Certification'),
(16, 'Chart', 'Chart'),
(17, 'Source', 'Source'),
(18, 'Tag', 'Tag');

-- Language
INSERT INTO Language (LanguageId, Code, Name) VALUES
(1, 'fa', 'Persian'),
(2, 'en', 'English'),
(3, 'ar', 'Arabic'),
(4, 'fr', 'French');

-- Country
INSERT INTO Country (CountryId, Code, Name) VALUES
(1, 'IR', 'Iran'),
(2, 'US', 'United States'),
(3, 'GB', 'United Kingdom'),
(4, 'FR', 'France'),
(5, 'DE', 'Germany'),
(6, 'CA', 'Canada'),
(7, 'AU', 'Australia'),
(8, 'LB', 'Lebanon'),
(9, 'EG', 'Egypt'),
(10, 'AE', 'United Arab Emirates'),
(11, 'TR', 'Turkey'),
(12, 'AF', 'Afghanistan'),
(13, 'IN', 'India'),
(14, 'PK', 'Pakistan');

-- AlbumCategory
INSERT INTO AlbumCategory (AlbumCategoryId, Code, Name) VALUES
(1, 'Studio', 'Studio Album'),
(2, 'Live', 'Live Album'),
(3, 'Compilation', 'Compilation'),
(4, 'EP', 'EP'),
(5, 'Single', 'Single'),
(6, 'BoxSet', 'Box Set'),
(7, 'Soundtrack', 'Soundtrack'),
(8, 'Demo', 'Demo'),
(9, 'Mixtape', 'Mixtape'),
(10, 'Remix', 'Remix Album');

-- CompanyType
INSERT INTO CompanyType (CompanyTypeId, Code, Name) VALUES
(1, 'Label', 'Record Label'),
(2, 'Publisher', 'Publisher'),
(3, 'Studio', 'Recording Studio'),
(4, 'Production', 'Production Company'),
(5, 'Distributor', 'Distributor'),
(6, 'Manufacturer', 'Manufacturer');

-- CompanyRoleType
INSERT INTO CompanyRoleType (CompanyRoleTypeId, Code, Name) VALUES
(1, 'Label', 'Label'),
(2, 'Publisher', 'Publisher'),
(3, 'Distributor', 'Distributor'),
(4, 'Production', 'Production Company'),
(5, 'Studio', 'Recording Studio');

-- RoleScopeType
INSERT INTO RoleScopeType (RoleScopeTypeId, Code, Name) VALUES
(1, 'Person', 'Person only'),
(2, 'Company', 'Company only'),
(3, 'Both', 'Person or Company');

-- CreditRole
INSERT INTO CreditRole (CreditRoleId, Code, Name, DisplayOrder, RoleScopeTypeId) VALUES
(1, 'PRIMARY_ARTIST', 'Primary Artist', 1, 1),
(2, 'FEATURED_ARTIST', 'Featured Artist', 2, 1),
(3, 'GUEST_ARTIST', 'Guest Artist', 3, 1),
(4, 'MUSICIAN', 'Musician', 10, 1),
(5, 'COMPOSER', 'Composer', 11, 1),
(6, 'LYRICIST', 'Lyricist', 12, 1),
(7, 'PRODUCER', 'Producer', 13, 1),
(8, 'ARRANGER', 'Arranger', 14, 1),
(9, 'CONDUCTOR', 'Conductor', 15, 1),
(10, 'ENGINEER', 'Engineer', 20, 1),
(11, 'MIXER', 'Mixing Engineer', 21, 1),
(12, 'MASTERER', 'Mastering Engineer', 22, 1),
(13, 'RECORDING', 'Recording Engineer', 23, 1),
(14, 'ARTWORK', 'Artwork/Design', 30, 1),
(15, 'PHOTOGRAPHER', 'Photographer', 31, 1),
(16, 'WRITER', 'Writer', 40, 3),
(17, 'LABEL', 'Label', 50, 2),
(18, 'PUBLISHER', 'Publisher', 51, 2),
(19, 'DISTRIBUTOR', 'Distributor', 52, 2),
(20, 'COLLABORATOR', 'Collaborator', 60, 3);

-- AliasType
INSERT INTO AliasType (AliasTypeId, Code, Name) VALUES
(1, 'OFFICIAL', 'Official Alias'),
(2, 'TRANSLITERATION', 'Transliteration'),
(3, 'SEARCH', 'Search Alias'),
(4, 'ABBREVIATION', 'Abbreviation'),
(5, 'ORIGINAL_SCRIPT', 'Original Script'),
(6, 'SLUG', 'Slug');

-- MediaType
INSERT INTO MediaType (MediaTypeId, Code, Name) VALUES
(1, 'IMAGE', 'Image'),
(2, 'AUDIO', 'Audio'),
(3, 'VIDEO', 'Video'),
(4, 'PDF', 'Document');

-- MediaRoleType
INSERT INTO MediaRoleType (MediaRoleTypeId, Code, Name) VALUES
(1, 'COVER', 'Cover Art'),
(2, 'BACK_COVER', 'Back Cover'),
(3, 'BOOKLET', 'Booklet'),
(4, 'PROMO', 'Promotional Image'),
(5, 'LIVE', 'Live Photo'),
(6, 'STUDIO', 'Studio Photo'),
(7, 'THUMBNAIL', 'Thumbnail'),
(8, 'BANNER', 'Banner'),
(9, 'LOGO', 'Logo'),
(10, 'AUDIO_PREVIEW', 'Audio Preview'),
(11, 'MUSIC_VIDEO', 'Music Video'),
(12, 'LYRIC_SHEET', 'Lyric Sheet'),
(13, 'SCORE', 'Musical Score'),
(14, 'INTERVIEW', 'Interview');

-- LinkType
INSERT INTO LinkType (LinkTypeId, Code, Name) VALUES
(1, 'OFFICIAL', 'Official Website'),
(2, 'WIKIPEDIA', 'Wikipedia'),
(3, 'DISCOGS', 'Discogs'),
(4, 'MUSICBRAINZ', 'MusicBrainz'),
(5, 'SPOTIFY', 'Spotify'),
(6, 'APPLE_MUSIC', 'Apple Music'),
(7, 'YOUTUBE', 'YouTube'),
(8, 'YOUTUBE_MUSIC', 'YouTube Music'),
(9, 'SOUNDCLOUD', 'SoundCloud'),
(10, 'BANDCAMP', 'Bandcamp'),
(11, 'INSTAGRAM', 'Instagram'),
(12, 'TWITTER', 'Twitter/X'),
(13, 'TELEGRAM', 'Telegram'),
(14, 'FACEBOOK', 'Facebook'),
(15, 'IMDB', 'IMDb'),
(16, 'LASTFM', 'Last.fm'),
(17, 'GENIUS', 'Genius'),
(18, 'PURCHASE', 'Purchase Link'),
(19, 'STREAMING', 'Streaming Link');

-- SourceType
INSERT INTO SourceType (SourceTypeId, Code, Name) VALUES
(1, 'BOOK', 'Book'),
(2, 'MAGAZINE', 'Magazine'),
(3, 'NEWSPAPER', 'Newspaper'),
(4, 'WEBSITE', 'Website'),
(5, 'INTERVIEW', 'Interview'),
(6, 'LINER_NOTES', 'Liner Notes'),
(7, 'OFFICIAL', 'Official Source'),
(8, 'ARCHIVE', 'Archive'),
(9, 'ACADEMIC', 'Academic Publication'),
(10, 'SOCIAL_MEDIA', 'Social Media');

-- IdentifierType
INSERT INTO IdentifierType (IdentifierTypeId, Code, Name) VALUES
(1, 'BARCODE', 'Barcode'),
(2, 'CATALOG', 'Catalog Number'),
(3, 'ISRC', 'ISRC'),
(4, 'UPC', 'UPC'),
(5, 'MATRIX', 'Matrix/Runout');

-- PersonKind
INSERT INTO PersonKind (PersonKindId, Code, Name) VALUES
(1, 'INDIVIDUAL', 'Individual'),
(2, 'GROUP', 'Group'),
(3, 'CHOIR', 'Choir'),
(4, 'ORCHESTRA', 'Orchestra'),
(5, 'ENSEMBLE', 'Ensemble');

-- PersonType
INSERT INTO PersonType (PersonTypeId, Code, Name) VALUES
(1, 'MAIN_ARTIST', 'Main Artist'),
(2, 'POET', 'Poet'),
(3, 'MUSICIAN', 'Musician'),
(4, 'COMPOSER', 'Composer'),
(5, 'LYRICIST', 'Lyricist'),
(6, 'PRODUCER', 'Producer'),
(7, 'ENGINEER', 'Engineer'),
(8, 'CONDUCTOR', 'Conductor');

-- LocationType
INSERT INTO LocationType (LocationTypeId, Code, Name) VALUES
(1, 'CITY', 'City'),
(2, 'COUNTRY', 'Country'),
(3, 'VENUE', 'Venue'),
(4, 'STUDIO', 'Recording Studio'),
(5, 'REGION', 'Region'),
(6, 'PROVINCE', 'Province/State');

-- EventType
INSERT INTO EventType (EventTypeId, Code, Name) VALUES
(1, 'CONCERT', 'Concert'),
(2, 'FESTIVAL', 'Festival'),
(3, 'TOUR', 'Tour'),
(4, 'TV_PERFORMANCE', 'TV Performance'),
(5, 'RADIO_PERFORMANCE', 'Radio Performance'),
(6, 'AWARD_CEREMONY', 'Award Ceremony'),
(7, 'BOOK_SIGNING', 'Book Signing'),
(8, 'MEET_AND_GREET', 'Meet and Greet');

-- SessionType
INSERT INTO SessionType (SessionTypeId, Code, Name) VALUES
(1, 'ALBUM', 'Album Recording Session'),
(2, 'SINGLE', 'Single Recording Session'),
(3, 'DEMO', 'Demo Session'),
(4, 'REHEARSAL', 'Rehearsal'),
(5, 'MIXING', 'Mixing Session'),
(6, 'MASTERING', 'Mastering Session'),
(7, 'LIVE_RECORDING', 'Live Recording Session');

-- PublicationType
INSERT INTO PublicationType (PublicationTypeId, Code, Name) VALUES
(1, 'BOOK', 'Book'),
(2, 'MAGAZINE', 'Magazine'),
(3, 'JOURNAL', 'Journal'),
(4, 'DIGITAL', 'Digital Publication'),
(5, 'COLLECTION', 'Collection');

-- AwardResultType
INSERT INTO AwardResultType (AwardResultTypeId, Code, Name) VALUES
(1, 'WON', 'Won'),
(2, 'NOMINATED', 'Nominated'),
(3, 'PLACE_1', 'First Place'),
(4, 'PLACE_2', 'Second Place'),
(5, 'PLACE_3', 'Third Place'),
(6, 'HONORABLE', 'Honorable Mention');

-- TrackRelationType
INSERT INTO TrackRelationType (TrackRelationTypeId, Code, Name) VALUES
(1, 'COVER', 'Cover'),
(2, 'REMIX', 'Remix'),
(3, 'LIVE_VERSION', 'Live Version'),
(4, 'ACOUSTIC', 'Acoustic Version'),
(5, 'DEMO', 'Demo'),
(6, 'REMASTER', 'Remaster'),
(7, 'RE_RECORDING', 'Re-recording'),
(8, 'ORIGINAL', 'Original'),
(9, 'SAMPLED_IN', 'Sampled In'),
(10, 'SAMPLES', 'Samples');

-- AlbumRelationType
INSERT INTO AlbumRelationType (AlbumRelationTypeId, Code, Name) VALUES
(1, 'REISSUE', 'Reissue'),
(2, 'REMASTER', 'Remaster'),
(3, 'BOX_SET', 'Box Set Contains'),
(4, 'PART_OF', 'Part Of'),
(5, 'FOLLOW_UP', 'Follow-up'),
(6, 'COMPILATION', 'Compilation Contains');

-- TrackVersionType
INSERT INTO TrackVersionType (TrackVersionTypeId, Code, Name) VALUES
(1, 'DEMO', 'Demo'),
(2, 'ALTERNATE', 'Alternate Take'),
(3, 'RADIO_EDIT', 'Radio Edit'),
(4, 'ACOUSTIC', 'Acoustic'),
(5, 'LIVE', 'Live'),
(6, 'REMASTER', 'Remaster'),
(7, 'REMIX', 'Remix'),
(8, 'INSTRUMENTAL', 'Instrumental'),
(9, 'A_CAPPELLA', 'A Cappella'),
(10, 'EXTENDED', 'Extended Mix'),
(11, 'ORIGINAL', 'Original Version');

-- VocalStyle
INSERT INTO VocalStyle (VocalStyleId, Code, Name) VALUES
(1, 'SOLO', 'Solo'),
(2, 'DUET', 'Duet'),
(3, 'GROUP', 'Group'),
(4, 'CHORUS', 'Chorus'),
(5, 'BACKING', 'Backing Vocals'),
(6, 'NARRATION', 'Narration'),
(7, 'SPOKEN', 'Spoken Word'),
(8, 'SCAT', 'Scat'),
(9, 'FALSETTO', 'Falsetto'),
(10, 'RAP', 'Rap'),
(11, 'SCREAM', 'Scream'),
(12, 'WHISPER', 'Whisper');

-- MusicalKey
INSERT INTO MusicalKey (MusicalKeyId, Code, Name) VALUES
(1, 'C', 'C Major'),
(2, 'Cm', 'C Minor'),
(3, 'C#', 'C# Major'),
(4, 'C#m', 'C# Minor'),
(5, 'Db', 'Db Major'),
(6, 'Dbm', 'Db Minor'),
(7, 'D', 'D Major'),
(8, 'Dm', 'D Minor'),
(9, 'Eb', 'Eb Major'),
(10, 'Ebm', 'Eb Minor'),
(11, 'E', 'E Major'),
(12, 'Em', 'E Minor'),
(13, 'F', 'F Major'),
(14, 'Fm', 'F Minor'),
(15, 'F#', 'F# Major'),
(16, 'F#m', 'F# Minor'),
(17, 'Gb', 'Gb Major'),
(18, 'Gbm', 'Gb Minor'),
(19, 'G', 'G Major'),
(20, 'Gm', 'G Minor'),
(21, 'Ab', 'Ab Major'),
(22, 'Abm', 'Ab Minor'),
(23, 'A', 'A Major'),
(24, 'Am', 'A Minor'),
(25, 'Bb', 'Bb Major'),
(26, 'Bbm', 'Bb Minor'),
(27, 'B', 'B Major'),
(28, 'Bm', 'B Minor');

-- LyricsAvailabilityType
INSERT INTO LyricsAvailabilityType (LyricsAvailabilityTypeId, Code, Name) VALUES
(1, 'NONE', 'No Lyrics'),
(2, 'PUBLIC', 'Public'),
(3, 'REGISTERED', 'Registered Users Only'),
(4, 'REQUEST', 'Available Upon Request'),
(5, 'RESTRICTED', 'Restricted Access');

-- CountryRoleType
INSERT INTO CountryRoleType (CountryRoleTypeId, Code, Name) VALUES
(1, 'NATIONALITY', 'Nationality'),
(2, 'ORIGIN', 'Country of Origin'),
(3, 'OPERATION', 'Country of Operation'),
(4, 'REGISTRATION', 'Country of Registration');

-- InstrumentFamily
INSERT INTO InstrumentFamily (InstrumentFamilyId, Code, Name) VALUES
(1, 'STRING', 'String Instruments'),
(2, 'WOODWIND', 'Woodwind Instruments'),
(3, 'BRASS', 'Brass Instruments'),
(4, 'PERCUSSION', 'Percussion Instruments'),
(5, 'KEYBOARD', 'Keyboard Instruments'),
(6, 'ELECTRONIC', 'Electronic Instruments'),
(7, 'VOCAL', 'Vocal'),
(8, 'FOLK', 'Folk/Traditional Instruments'),
(9, 'OTHER', 'Other Instruments');

-- Seed admin user and role
-- Password: must be set via application registration
-- INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES (NEWID(), 'Administrator', 'ADMINISTRATOR');
-- INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES (NEWID(), 'Editor', 'EDITOR');
GO
