# Music Encyclopedia Website Implementation Specification

> File: `IMPLEMENTATION_SPECIFICATION.md`  
> Purpose: Complete implementation specification for building a public-facing music encyclopedia website based on the previously designed Microsoft SQL Server database.  
> Target audience: AI coding agent, full-stack developer, database architect, UI developer, QA engineer.

---

## 1. Project Overview

### 1.1 Product Goal

Build a multilingual, encyclopedia-style website for documenting the complete works of one or more musical artists.

The website is not merely a lyrics website. It must present structured information about:

- Albums
- Tracks
- Poems
- Sung versions
- Poets
- Musicians
- Contributors
- Instruments
- Genres
- Moods
- Companies
- Recording sessions
- Live performances
- Locations
- Awards
- Certifications
- Charts
- Media
- Citations
- Tags
- Aliases
- Translations

The site must support Persian, English, Arabic, and French, with future language support possible.

---

## 2. Technology Stack

Use the following stack unless the project owner explicitly approves another stack.

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 8 LTS |
| Web UI | ASP.NET Core MVC with Razor Views |
| API | ASP.NET Core Web API controllers |
| ORM for writes | Entity Framework Core |
| Read models | Dapper or EF Core projections |
| Database | Microsoft SQL Server 2019/2022 |
| CSS | Tailwind CSS |
| JavaScript | Minimal vanilla JavaScript, optional htmx or Alpine.js |
| Validation | FluentValidation |
| Authentication | ASP.NET Core Identity |
| Authorization | Role-based and policy-based authorization |
| Search | SQL Server Full-Text Search initially, optional Meilisearch/OpenSearch later |
| Caching | IMemoryCache initially, optional Redis later |
| Logging | Serilog |
| Background jobs | Hangfire or .NET BackgroundService |
| Testing | xUnit, FluentAssertions, Testcontainers or LocalDB |
| Deployment | Docker + IIS/Linux, SQL Server database |

---

## 3. Core Product Principles

### 3.1 Encyclopedia-First

Every public page should behave like an encyclopedia article:

- Canonical title
- Alternative titles
- Translated titles
- Summary/description
- Structured metadata
- Related entities
- Citations
- Media
- Tags
- External links
- Cross-references

### 3.2 Database-Driven

The website must use the previously designed SQL Server schema.

Important tables:

```text
Entity
EntityType
Album
Track
AlbumTrack
Person
Company
Credit
CreditRole
CreditRoleEntityType
Genre
Mood
Instrument
Poem
SungVersion
TrackSungVersion
Publication
RecordingSession
PerformanceEvent
Location
Media
MediaAssignment
EntityLink
Alias
Localization
Tag
TagAssignment
AttributeDefinition
AttributeValue
Source
Citation
Award
AwardAssignment
Certification
CertificationAssignment
Chart
ChartEntry
```

Important views:

```text
vAlbumCredit
vTrackCredit
vAlbumMusicianInstrument
```

If additional read-optimized views are needed, create them without changing the base normalized schema.

---

## 4. Solution Architecture

### 4.1 High-Level Architecture

```text
Browser
  |
ASP.NET Core MVC + API
  |
Application Services
  |
Read Side ---------------- Write Side
  |                           |
Dapper/EF Core Views      EF Core DbContext
  |                           |
SQL Server Database
```

### 4.2 Recommended Projects

```text
MusicEncyclopedia.sln

src/
  MusicEncyclopedia.Core/
  MusicEncyclopedia.Data/
  MusicEncyclopedia.Services/
  MusicEncyclopedia.Web/
  MusicEncyclopedia.Search/
  MusicEncyclopedia.Media/

tests/
  MusicEncyclopedia.Core.Tests/
  MusicEncyclopedia.Services.Tests/
  MusicEncyclopedia.Web.Tests/
  MusicEncyclopedia.Data.Tests/
```

---

## 5. Project Responsibilities

### 5.1 MusicEncyclopedia.Core

Contains:

- Domain models
- Enums
- Constants
- Validation rules
- Interfaces
- DTOs
- Result types

Does not reference:

- EF Core
- ASP.NET Core
- External infrastructure libraries

---

### 5.2 MusicEncyclopedia.Data

Contains:

- EF Core `DbContext`
- Entity configurations
- Migrations
- Repositories
- Dapper query helpers
- Read-model projections

---

### 5.3 MusicEncyclopedia.Services

Contains:

- Application services
- Localization service
- Credit service
- Media service
- Search service
- Slug service
- Citation service
- Cache invalidation service
- Authorization helpers

---

### 5.4 MusicEncyclopedia.Web

Contains:

- MVC controllers
- API controllers
- Razor views
- View models
- JavaScript
- CSS
- Middleware
- Filters
- Authentication/authorization setup

---

### 5.5 MusicEncyclopedia.Search

Contains:

- Search indexing
- Search query models
- SQL Full-Text integration
- Optional external search engine integration

---

### 5.6 MusicEncyclopedia.Media

Contains:

- File upload handling
- Image resizing
- Thumbnail generation
- Storage abstraction
- CDN integration helpers

---

## 6. Database Access Rules

### 6.1 Write Operations

Use EF Core for:

- Creating albums
- Editing tracks
- Managing people
- Managing companies
- Managing poems
- Managing credits
- Managing media
- Managing users
- Managing lookup tables

Rules:

1. Never expose EF entities directly to Razor views.
2. Never expose EF entities directly in API responses.
3. Use DTOs/view models.
4. Use transactions for multi-table operations.
5. Use optimistic concurrency with `RowVersion`.
6. Use soft delete by setting `IsDeleted = 1`.
7. Populate `CreatedBy` and `ModifiedBy` from the authenticated user.

---

### 6.2 Read Operations

Use read-optimized queries for public pages.

Preferred approach:

- Use Dapper for complex public read models.
- Use EF Core for simple admin lists and forms.
- Use SQL Server views for repeated complex joins.

Recommended read views:

```text
vAlbumPublic
vTrackPublic
vPersonPublic
vCompanyPublic
vPoemPublic
vSungVersionPublic
vAlbumTrackList
vAlbumCreditList
vTrackCreditList
vAlbumMusicianInstrument
vTrackLyricDetail
vEntityLocalization
vEntityAlias
vEntityTag
vEntityMedia
vEntityLink
vEntityCitation
```

If these views do not exist, create them during database implementation.

---

## 7. Global Application Rules

### 7.1 Culture Handling

Supported cultures:

```text
fa
en
ar
fr
```

Default culture:

```text
fa
```

Fallback culture:

```text
en
```

URL pattern:

```text
/{culture}/{controller}/{action}/{slug?}
```

Examples:

```text
/fa/albums
/en/albums/khaneh-siah-ast
/ar/tracks/track-slug
/fr/people/person-slug
```

Culture must be validated:

```csharp
[Route("{culture:regex(^(fa|en|ar|fr)$)}")]
```

---

### 7.2 Right-to-Left Support

For `fa` and `ar`:

```html
<html dir="rtl" lang="fa">
```

For `en` and `fr`:

```html
<html dir="ltr" lang="en">
```

Tailwind should be configured for RTL support.

Use logical CSS properties:

```css
ms-*, me-*, ps-*, pe-*, text-start, text-end
```

Avoid hardcoded `left`/`right` spacing where possible.

---

### 7.3 Localization Strategy

There are two localization layers.

#### UI Localization

Use `.resx` files or a database-backed UI string localizer.

Examples:

```text
Albums
Tracks
Biography
Discography
Lyrics
Original Poem
Sung Version
Credits
Awards
Charts
Media
References
Tags
```

#### Content Localization

Use the database table:

```text
Localization
```

Fields commonly localized:

```text
Title
OriginalTitle
EnglishTitle
Description
Biography
History
Notes
HistoricalNotes
```

Fallback logic:

```text
1. Requested language value from Localization
2. Base entity column
3. English value if available
4. Empty string or placeholder
```

Example:

```csharp
string title = localizationService.GetString(
    entityId: album.EntityId,
    fieldName: "Title",
    culture: currentCulture,
    fallback: album.Title
);
```

---

### 7.4 Slug Handling

Every public entity has a base `Slug`.

Examples:

```text
Album.Slug
Track.Slug
Person.Slug
Company.Slug
Genre.Slug
Mood.Slug
Instrument.Slug
Poem.Slug
```

Localized slugs may be stored in:

```text
Alias
```

Use:

```text
AliasTypeId = SLUG
LanguageId = requested language
```

Slug resolution order:

```text
1. Localized Alias slug for current culture
2. Base entity Slug
3. EntityId fallback route for admin preview
```

Slug rules:

- Lowercase
- URL-safe
- Allow Unicode/Persian characters if desired
- Maximum length: 255
- Must be unique per entity type
- Must not collide with reserved routes

Reserved routes:

```text
admin
api
auth
search
sitemap.xml
robots.txt
favicon.ico
```

---

## 8. Public Site Map

### 8.1 Public Routes

| Route | Page |
|---|---|
| `/{culture}` | Home |
| `/{culture}/albums` | Album listing |
| `/{culture}/albums/{slug}` | Album detail |
| `/{culture}/tracks` | Track listing |
| `/{culture}/tracks/{slug}` | Track detail |
| `/{culture}/people` | People listing |
| `/{culture}/people/{slug}` | Person detail |
| `/{culture}/companies` | Company listing |
| `/{culture}/companies/{slug}` | Company detail |
| `/{culture}/genres` | Genre listing |
| `/{culture}/genres/{slug}` | Genre detail |
| `/{culture}/moods` | Mood listing |
| `/{culture}/moods/{slug}` | Mood detail |
| `/{culture}/instruments` | Instrument listing |
| `/{culture}/instruments/{slug}` | Instrument detail |
| `/{culture}/poets` | Poet listing |
| `/{culture}/poets/{slug}` | Poet detail |
| `/{culture}/poems` | Poem listing |
| `/{culture}/poems/{slug}` | Poem detail |
| `/{culture}/sung-versions/{slug}` | Sung version detail |
| `/{culture}/sessions` | Recording session listing |
| `/{culture}/sessions/{slug}` | Recording session detail |
| `/{culture}/events` | Performance event listing |
| `/{culture}/events/{slug}` | Performance event detail |
| `/{culture}/locations` | Location listing |
| `/{culture}/locations/{slug}` | Location detail |
| `/{culture}/awards` | Award listing |
| `/{culture}/awards/{slug}` | Award detail |
| `/{culture}/charts` | Chart listing |
| `/{culture}/charts/{slug}` | Chart detail |
| `/{culture}/sources` | Source listing |
| `/{culture}/sources/{slug}` | Source detail |
| `/{culture}/tags/{slug}` | Tag page |
| `/{culture}/search` | Search results |

---

## 9. Public Page Specifications

---

## 9.1 Home Page

Route:

```text
/{culture}
```

Purpose:

- Introduce the encyclopedia
- Highlight main artist or artists
- Provide entry points into the database

Sections:

1. Hero section
2. Featured albums
3. Latest albums
4. Essential tracks
5. Featured poems/poets
6. Recent additions
7. Browse by genre
8. Browse by mood
9. Browse by instrument
10. Search box

Data sources:

```text
Album
Track
Person
Poem
Genre
Mood
Instrument
MediaAssignment
TagAssignment
```

Implementation rules:

- Cache home page for 5 minutes per culture.
- Use published/public items only.
- Exclude soft-deleted entities.
- Use localized titles.
- Use responsive images.

---

## 9.2 Album Listing Page

Route:

```text
/{culture}/albums
```

Query parameters:

```text
page
pageSize
category
genre
mood
language
country
year
label
artist
q
sort
```

Allowed sort fields:

```text
title
releaseDate
createdAt
duration
```

Default sort:

```text
releaseDate DESC
```

Filters:

| Filter | Source |
|---|---|
| Category | AlbumCategory |
| Genre | AlbumGenre |
| Mood | AlbumMood |
| Language | AlbumLanguage |
| Country | AlbumCountry |
| Year | Album.ReleaseDate |
| Label | AlbumCompany + CompanyRoleType |
| Artist | Credit + CreditRole |
| Search text | Album.Title, Alias, Localization |

Card fields:

- Cover image
- Title
- Original title
- English title
- Category
- Release year
- Primary artists
- Duration
- Genre badges
- Mood badges

Pagination:

```text
Default page size: 24
Maximum page size: 100
```

SEO:

- Canonical URL
- Open Graph tags
- JSON-LD `ItemList`

---

## 9.3 Album Detail Page

Route:

```text
/{culture}/albums/{slug}
```

Required data:

```text
Album
AlbumCategory
Media cover
AlbumLanguage
AlbumCountry
AlbumGenre
AlbumMood
AlbumCompany
AlbumIdentifier
AlbumTrack
Track
Credit
AwardAssignment
CertificationAssignment
ChartEntry
MediaAssignment
EntityLink
Alias
Localization
TagAssignment
Citation
Source
AlbumRelation
RecordingSessionAlbum
PerformanceEventAlbum
```

Page sections:

1. Header
   - Cover
   - Title
   - Original title
   - English title
   - Category
   - Release date
   - Recording dates
   - Duration
   - Primary artists

2. Action bar
   - Purchase links
   - Streaming links
   - Official website
   - Music video links

3. Summary
   - Localized description
   - Tags
   - Genres
   - Moods

4. Tracklist
   - Grouped by disc
   - Disc number
   - Track number
   - Track title
   - Track artists
   - Duration
   - Bonus/hidden flags
   - Track title override if present

5. Credits
   - Grouped by role
   - People
   - Companies
   - Instruments for musicians

6. Companies

   Display:

   - Label
   - Publisher
   - Distributor
   - Production company
   - Studio

   Include:

   - Catalog number
   - Barcode if available

7. Awards and nominations

8. Certifications

9. Chart history

10. Recording sessions

11. Live performance events

12. Related albums

13. Media gallery

14. Aliases

15. References/citations

16. External links

17. Metadata sidebar
   - Languages
   - Countries
   - Copyright
   - Identifiers
   - Created/modified date if public

Business rules:

- If `Album.DurationSeconds` is null, calculate from track durations.
- If `AlbumTrack.DurationSecondsOverride` exists, use it for album tracklist display.
- If `AlbumTrack.TrackTitleOverride` exists, display it instead of `Track.Title`.
- Primary artists are credits with roles:

```text
PRIMARY_ARTIST
FEATURED_ARTIST
GUEST_ARTIST
```

- Musician credits must display instrument when available.
- Do not show restricted lyrics on album page.
- Show citations as footnotes or expandable references.

JSON-LD:

```json
{
  "@type": "MusicAlbum",
  "name": "",
  "albumReleaseType": "",
  "datePublished": "",
  "image": "",
  "byArtist": [],
  "track": []
}
```

---

## 9.4 Track Listing Page

Route:

```text
/{culture}/tracks
```

Query parameters:

```text
page
pageSize
genre
mood
instrument
artist
poet
vocalStyle
language
isInstrumental
isExplicit
lyricsAvailability
q
sort
```

Allowed sort fields:

```text
title
duration
releaseDate
createdAt
```

Card fields:

- Title
- Main artists
- Duration
- Genres
- Moods
- Instrumental badge
- Explicit badge
- Lyrics availability badge
- Release year

---

## 9.5 Track Detail Page

Route:

```text
/{culture}/tracks/{slug}
```

Required data:

```text
Track
MusicalKey
VocalStyle
LyricsAvailabilityType
AlbumTrack
Album
Credit
TrackGenre
TrackMood
TrackInstrument
TrackSungVersion
SungVersion
Poem
Person
TrackRelation
TrackVersionTypeAssignment
RecordingSessionTrack
PerformanceEventTrack
MediaAssignment
EntityLink
Alias
Localization
TagAssignment
Citation
AwardAssignment
CertificationAssignment
ChartEntry
```

Page sections:

1. Header
   - Title
   - Original title
   - English title
   - Main artists
   - Duration
   - Release date
   - Recording date
   - ISRC
   - BPM
   - Musical key
   - Vocal style
   - Explicit flag
   - Instrumental flag

2. Album appearances

   Table columns:

   - Album
   - Category
   - Disc
   - Track number
   - Release year
   - Duration override

3. Artists

   Display:

   - Primary artist
   - Featured artist
   - Guest artist
   - Collaboration

4. Genres and moods

5. Instruments

6. Musicians

   Display:

   - Musician name
   - Instrument
   - Notes

7. Lyrics

   If track is instrumental:

   ```text
   Show instrumental notice.
   Hide lyrics section.
   ```

   If lyrics are public:

   - Show sung version text
   - Show original poem link
   - Show poet name
   - Show sung version title
   - Show vocal style

   If lyrics are restricted:

   - Show availability message
   - Do not show lyrics text
   - Optionally show request link

8. Original poem

   Display:

   - Poem title
   - Poet
   - Publication
   - Book
   - Original publication date
   - Source
   - Copyright
   - Link to poem page

9. Sung versions

   If multiple sung versions exist:

   - List all sung versions
   - Show sequence
   - Show primary sung version
   - Allow switching between sung versions

10. Recording sessions

11. Live performances

   Display:

   - Event name
   - Venue
   - Date
   - Audience information
   - Improvisation notes
   - Performance notes

12. Version information

   Display assigned version types:

   - Demo
   - Alternate take
   - Radio edit
   - Acoustic
   - Live
   - Remaster
   - Remix

13. Related tracks

   Group by relationship type:

   - Cover
   - Remix
   - Live version
   - Acoustic version
   - Demo
   - Remaster
   - Re-recording

14. Awards and certifications

15. Chart history

16. Media

   - Audio preview
   - Music video
   - PDF lyric sheet
   - Score

17. Links

   - Purchase
   - Streaming
   - Music video
   - External references

18. References

19. Tags

20. Aliases

Business rules:

- Track may appear on multiple albums.
- Track may have multiple artists.
- Track may have multiple musicians with different instruments.
- Track may have no lyrics.
- Track may use multiple sung versions.
- Sung version may differ from original poem.
- Lyrics must respect `LyricsAvailabilityType`.

JSON-LD:

```json
{
  "@type": "MusicRecording",
  "name": "",
  "duration": "",
  "isrcCode": "",
  "inAlbum": [],
  "byArtist": [],
  "genre": [],
  "lyricist": [],
  "recordingOf": ""
}
```

---

## 9.6 Person Detail Page

Route:

```text
/{culture}/people/{slug}
```

Applies to:

- Main artist
- Poet
- Musician
- Composer
- Lyricist
- Producer
- Engineer
- Conductor
- Group
- Choir
- Orchestra

Required data:

```text
Person
PersonKind
PersonTypeAssignment
Credit
AlbumTrack
Album
Track
Poem
Publication
MusicianInstrument
MediaAssignment
EntityLink
Alias
Localization
TagAssignment
Citation
AwardAssignment
CertificationAssignment
ChartEntry
PerformanceEvent
RecordingSession
```

Page sections:

1. Header
   - Image
   - Full name
   - Original name
   - English name
   - Person kind
   - Nationality
   - Birth date
   - Death date
   - Birth place
   - Death place

2. Biography

3. Timeline

   Events:

   - Album releases
   - Track releases
   - Recording sessions
   - Live events
   - Awards
   - Chart entries
   - Publications

4. Discography

   Filter by:

   - Album category
   - Year
   - Role
   - Instrument

5. Track contributions

   Filter by:

   - Role
   - Instrument
   - Album
   - Genre

6. Instruments

   Display general instruments from:

   ```text
   MusicianInstrument
   ```

7. Roles

   Display all roles from:

   ```text
   Credit
   CreditRole
   ```

8. Poems

   If person is a poet:

   - List poems
   - List publications
   - List sung versions
   - List tracks based on poems

9. Media gallery

10. References

11. External links

12. Tags

13. Aliases

Business rules:

- A person may have multiple roles.
- A person may play multiple instruments.
- A person may work on the same album in multiple roles.
- A person may be an individual or group.
- Groups can be artists, choirs, orchestras, or ensembles.

---

## 9.7 Company Detail Page

Route:

```text
/{culture}/companies/{slug}
```

Required data:

```text
Company
CompanyType
AlbumCompany
Credit
Publication
MediaAssignment
EntityLink
Alias
Localization
TagAssignment
Citation
```

Page sections:

1. Header
   - Logo
   - Name
   - Type
   - Country
   - Website

2. History

3. Albums as label

4. Albums as publisher

5. Albums as distributor

6. Albums as production company

7. Albums as studio

8. Track credits

9. Publications

10. Media

11. References

12. External links

---

## 9.8 Genre Detail Page

Route:

```text
/{culture}/genres/{slug}
```

Required data:

```text
Genre
ParentGenre
GenreRelation
AlbumGenre
TrackGenre
MediaAssignment
Localization
Alias
TagAssignment
Citation
```

Page sections:

1. Header
2. Description
3. Parent genre
4. Related genres
5. Albums in genre
6. Tracks in genre
7. Artists associated with genre
8. Media
9. References

---

## 9.9 Mood Detail Page

Route:

```text
/{culture}/moods/{slug}
```

Page sections:

1. Header
2. Description
3. Albums with mood
4. Tracks with mood
5. Media
6. Tags

---

## 9.10 Instrument Detail Page

Route:

```text
/{culture}/instruments/{slug}
```

Required data:

```text
Instrument
InstrumentFamily
Country
MusicianInstrument
Credit
TrackInstrument
MediaAssignment
Localization
Alias
Citation
```

Page sections:

1. Header
2. Description
3. Family
4. Country of origin
5. Historical notes
6. Musicians who play this instrument
7. Tracks using this instrument
8. Track credits with this instrument
9. Media
10. References

Important query:

Use `Credit` to show:

```text
Musician + Instrument + Track/Album
```

Use `MusicianInstrument` to show general ability.

---

## 9.11 Poet Detail Page

Route:

```text
/{culture}/poets/{slug}
```

A poet is a `Person` with poems.

Page sections:

1. Person header
2. Biography
3. Books/publications
4. Poems
5. Sung versions
6. Tracks based on poems
7. References
8. Media

Filters:

```text
publication
originalPublicationDate
hasSungVersion
trackCount
```

---

## 9.12 Poem Detail Page

Route:

```text
/{culture}/poems/{slug}
```

Required data:

```text
Poem
Person
Publication
SungVersion
TrackSungVersion
Track
Localization
Alias
Citation
EntityLink
```

Page sections:

1. Header
   - Title
   - Poet
   - Publication
   - Book
   - Source
   - Original publication date
   - Copyright

2. Canonical original text

3. Sung versions

   For each sung version:

   - Title
   - Vocal style
   - Number of tracks
   - Link to sung version page

4. Tracks using this poem

5. References

6. External references

Business rules:

- Show canonical original poem text.
- Do not replace original poem text with sung version text.
- Clearly label differences between original poem and sung version.

---

## 9.13 Sung Version Detail Page

Route:

```text
/{culture}/sung-versions/{slug}
```

Page sections:

1. Header
   - Sung version title
   - Original poem link
   - Poet
   - Vocal style

2. Sung text

3. Differences from original poem

4. Tracks using this sung version

5. Media

6. References

---

## 9.14 Recording Session Detail Page

Route:

```text
/{culture}/sessions/{slug}
```

Required data:

```text
RecordingSession
SessionType
Location
RecordingSessionAlbum
RecordingSessionTrack
Credit
MediaAssignment
Citation
```

Page sections:

1. Header
2. Session type
3. Date range
4. Location
5. Albums recorded
6. Tracks recorded
7. Musicians and credits
8. Media
9. Notes
10. References

---

## 9.15 Performance Event Detail Page

Route:

```text
/{culture}/events/{slug}
```

Required data:

```text
PerformanceEvent
EventType
Location
PerformanceEventAlbum
PerformanceEventTrack
Credit
MediaAssignment
Citation
```

Page sections:

1. Header
2. Event type
3. Date/time
4. Venue
5. Audience information
6. Albums performed
7. Tracks performed
8. Performance notes
9. Improvisation notes
10. Media
11. References

---

## 9.16 Location Detail Page

Route:

```text
/{culture}/locations/{slug}
```

Page sections:

1. Header
2. Location type
3. Parent location
4. Country
5. Map
6. Recording sessions at this location
7. Performance events at this location
8. People born/died here
9. Media

---

## 9.17 Award Detail Page

Route:

```text
/{culture}/awards/{slug}
```

Page sections:

1. Header
2. Organization
3. Country
4. Description
5. Winners and nominees
6. Related albums/tracks/people

---

## 9.18 Chart Detail Page

Route:

```text
/{culture}/charts/{slug}
```

Page sections:

1. Header
2. Publisher
3. Country
4. Frequency
5. Chart entries
6. Timeline chart visualization

Chart entry table:

```text
Date
Entity
Position
Previous position
Weeks on chart
```

---

## 9.19 Source Detail Page

Route:

```text
/{culture}/sources/{slug}
```

Page sections:

1. Header
2. Source type
3. Author
4. Publisher
5. Publication date
6. URL
7. Cited facts/entities

---

## 9.20 Tag Page

Route:

```text
/{culture}/tags/{slug}
```

Page sections:

1. Header
2. Description
3. Albums
4. Tracks
5. People
6. Poems
7. Other tagged entities

---

## 9.21 Search Page

Route:

```text
/{culture}/search
```

Query parameters:

```text
q
type
genre
mood
instrument
language
page
pageSize
```

Searchable entities:

```text
Album
Track
Person
Company
Genre
Mood
Instrument
Poem
SungVersion
Publication
RecordingSession
PerformanceEvent
Location
Tag
```

Search fields:

```text
Title
Name
FullName
OriginalTitle
EnglishTitle
Description
Biography
Alias
Localization
Tag
```

Implementation:

Initial implementation:

```sql
CONTAINS / FREETEXT
```

Optional later:

```text
Meilisearch
OpenSearch
Elasticsearch
```

Search result card:

- Entity type badge
- Title
- Subtitle
- Image
- Short description
- Culture/language
- Link

---

## 10. Admin Area Specification

Admin route prefix:

```text
/admin
```

All admin pages require authentication.

---

## 10.1 Admin Roles

| Role | Description |
|---|---|
| `Administrator` | Full access |
| `Editor` | Create/edit encyclopedia content |
| `Contributor` | Create drafts, edit own content |
| `Reviewer` | Review and approve content |
| `MediaManager` | Manage media only |
| `UserManager` | Manage users and roles |

---

## 10.2 Admin Policies

```text
CanManageAlbums
CanManageTracks
CanManagePeople
CanManageCompanies
CanManagePoems
CanManageGenres
CanManageMoods
CanManageInstruments
CanManageSessions
CanManageEvents
CanManageLocations
CanManageAwards
CanManageCharts
CanManageMedia
CanManageUsers
CanManageLookupTables
CanManageCitations
CanManageLocalizations
CanPublishContent
CanDeleteContent
```

---

## 10.3 Admin Navigation

```text
Dashboard
Albums
Tracks
People
Companies
Poems
Sung Versions
Publications
Genres
Moods
Instruments
Recording Sessions
Performance Events
Locations
Awards
Certifications
Charts
Media
Sources
Citations
Tags
Attributes
Localizations
Lookup Tables
Users
Roles
Settings
Audit Log
```

---

## 10.4 Admin List Pages

Every admin list page must support:

- Pagination
- Search
- Sorting
- Filtering
- Bulk soft delete
- Bulk restore
- CSV export optional
- Created/modified display
- Active/deleted status

Default page size:

```text
25
```

Allowed page sizes:

```text
10, 25, 50, 100
```

---

## 10.5 Album Admin Form

Fields:

```text
Title
TitleSort
OriginalTitle
EnglishTitle
AlbumCategory
ReleaseDate
ReleaseDatePrecision
RecordingStartDate
RecordingEndDate
RecordingDatePrecision
Description
CoverMedia
DurationSeconds
CopyrightNotice
Slug
IsOfficial
IsDeleted
```

Related editors:

- Tracklist editor
- Primary artist editor
- Genre editor
- Mood editor
- Language editor
- Country editor
- Company editor
- Identifier editor
- Credit editor
- Alias editor
- Localization editor
- Tag editor
- Link editor
- Media gallery editor
- Citation editor
- Attribute editor
- Related album editor

Validation:

```text
Title required, max 500
Slug required, max 255, unique
ReleaseDate optional
RecordingEndDate >= RecordingStartDate
DurationSeconds >= 0
```

Tracklist editor rules:

- Add existing track
- Create new track inline
- Set disc number
- Set track number
- Set sequence number
- Set title override
- Set duration override
- Reorder by drag and drop
- Prevent duplicate disc/sequence

---

## 10.6 Track Admin Form

Fields:

```text
Title
TitleSort
OriginalTitle
EnglishTitle
DurationSeconds
RecordingStartDate
RecordingEndDate
RecordingDatePrecision
ReleaseDate
ReleaseDatePrecision
Description
LyricsAvailabilityType
VocalStyle
MusicalKey
BPM
ISRC
IsInstrumental
IsExplicit
CopyrightNotice
Slug
IsDeleted
```

Related editors:

- Album appearance editor
- Artist credit editor
- Genre editor
- Mood editor
- Instrument editor
- Musician credit editor
- Sung version editor
- Original poem selector
- Version type editor
- Related track editor
- Recording session editor
- Live performance editor
- Alias editor
- Localization editor
- Tag editor
- Link editor
- Media editor
- Citation editor
- Attribute editor

Validation:

```text
Title required
ISRC length 12 if provided
BPM between 0 and 1000
DurationSeconds >= 0
If IsInstrumental = true, sung versions optional but allowed only if logically consistent
```

---

## 10.7 Credit Editor

The credit editor is one of the most important admin components.

It must support:

- Person credit
- Company credit
- Role selection
- Instrument selection for musician roles
- Display order
- Primary flag
- Notes
- Multiple roles for same person/company
- Duplicate prevention

Credit form fields:

```text
EntityType
Entity
CreditRole
RoleScopeType
Person
Company
Instrument
DisplayOrder
IsPrimary
Notes
```

UI rules:

- If role scope is `PERSON`, show person selector only.
- If role scope is `COMPANY`, show company selector only.
- If role scope is `BOTH`, allow choosing either person or company.
- If role is `MUSICIAN`, show instrument selector.
- If instrument is selected, person must be selected.
- Prevent exact duplicate credit rows.

---

## 10.8 Person Admin Form

Fields:

```text
FullName
FullNameSort
OriginalName
EnglishName
PersonKind
Biography
BirthDate
BirthDatePrecision
BirthLocation
DeathDate
DeathDatePrecision
DeathLocation
NationalityCountry
ImageMedia
Slug
IsDeleted
```

Related editors:

- Person type editor
- Credit editor
- Musician instrument editor
- Alias editor
- Localization editor
- Tag editor
- Link editor
- Media editor
- Citation editor
- Attribute editor

---

## 10.9 Poem Admin Form

Fields:

```text
Title
OriginalTitle
EnglishTitle
Poet
Publication
Source
Book
OriginalPublicationDate
OriginalPublicationDatePrecision
ExternalReferenceUrl
Copyright
Notes
CanonicalText
Slug
```

Related editors:

- Sung version editor
- Alias editor
- Localization editor
- Citation editor
- Tag editor

---

## 10.10 Sung Version Admin Form

Fields:

```text
Title
Poem
VocalStyle
Text
Notes
IsCanonical
Slug
```

Related editors:

- Track assignment editor
- Alias editor
- Localization editor
- Citation editor

---

## 10.11 Media Manager

Media types:

```text
IMAGE
PDF
VIDEO
AUDIO
```

Features:

- Upload files
- Drag-and-drop upload
- Replace file
- Edit metadata
- Assign to entity
- Set media role
- Set primary media
- Delete/soft delete
- Search media
- Filter by media type
- Filter by assigned entity

Upload limits:

| Type | Max size |
|---|---:|
| Image | 10 MB |
| PDF | 50 MB |
| Audio | 100 MB |
| Video | 500 MB |

Allowed image formats:

```text
jpg
jpeg
png
webp
avif
```

Allowed audio formats:

```text
mp3
m4a
ogg
wav
flac
```

Allowed video formats:

```text
mp4
webm
```

Allowed document formats:

```text
pdf
```

Image processing:

Generate thumbnails:

```text
150px
300px
600px
1200px
```

Store original file separately.

---

## 10.12 Localization Editor

Every major admin edit page must include a localization tab.

Fields:

```text
Language
FieldName
LocalizedText
```

Supported fields:

```text
Title
OriginalTitle
EnglishTitle
Description
Biography
History
Notes
HistoricalNotes
```

Rules:

- One value per entity/language/field.
- If no translation exists, fallback to base field.
- Show base value beside translation editor.
- Allow copying base value into translation.

---

## 10.13 Alias Editor

Fields:

```text
AliasType
Language
AliasName
IsPrimary
Notes
```

Alias types:

```text
OFFICIAL
TRANSLITERATION
SEARCH
ABBREVIATION
ORIGINAL_SCRIPT
SLUG
```

Rules:

- Only one primary alias per entity/type/language.
- Alias names are searchable.
- Slug aliases can be used for localized URLs.

---

## 10.14 Citation Editor

Fields:

```text
Source
FieldName
Quote
PageNumber
Url
AccessedDate
Notes
```

UI:

- Select existing source
- Create new source inline
- Show citations attached to entity
- Group citations by field

---

## 10.15 Attribute Editor

Fields:

```text
AttributeDefinition
Language
ValueString
ValueInt
ValueDecimal
ValueDate
ValueDateTime
ValueBit
Notes
```

Rules:

- Only show attributes allowed for the current entity type.
- Validate value type against `AttributeDefinition.DataTypeCode`.
- Prevent duplicate values for same entity/attribute/language.

---

## 11. API Specification

All API routes are versioned:

```text
/api/v1
```

Public API is read-only.

Admin API requires authentication and authorization.

---

## 11.1 Public API Endpoints

### Albums

```http
GET /api/v1/albums
GET /api/v1/albums/{slug}
GET /api/v1/albums/{slug}/tracks
GET /api/v1/albums/{slug}/credits
GET /api/v1/albums/{slug}/media
GET /api/v1/albums/{slug}/links
GET /api/v1/albums/{slug}/awards
GET /api/v1/albums/{slug}/certifications
GET /api/v1/albums/{slug}/charts
GET /api/v1/albums/{slug}/related
```

### Tracks

```http
GET /api/v1/tracks
GET /api/v1/tracks/{slug}
GET /api/v1/tracks/{slug}/albums
GET /api/v1/tracks/{slug}/credits
GET /api/v1/tracks/{slug}/musicians
GET /api/v1/tracks/{slug}/lyrics
GET /api/v1/tracks/{slug}/poems
GET /api/v1/tracks/{slug}/related
GET /api/v1/tracks/{slug}/media
GET /api/v1/tracks/{slug}/links
```

### People

```http
GET /api/v1/people
GET /api/v1/people/{slug}
GET /api/v1/people/{slug}/albums
GET /api/v1/people/{slug}/tracks
GET /api/v1/people/{slug}/credits
GET /api/v1/people/{slug}/instruments
GET /api/v1/people/{slug}/poems
GET /api/v1/people/{slug}/media
```

### Companies

```http
GET /api/v1/companies
GET /api/v1/companies/{slug}
GET /api/v1/companies/{slug}/albums
GET /api/v1/companies/{slug}/credits
```

### Genres, Moods, Instruments

```http
GET /api/v1/genres
GET /api/v1/genres/{slug}
GET /api/v1/genres/{slug}/albums
GET /api/v1/genres/{slug}/tracks

GET /api/v1/moods
GET /api/v1/moods/{slug}
GET /api/v1/moods/{slug}/albums
GET /api/v1/moods/{slug}/tracks

GET /api/v1/instruments
GET /api/v1/instruments/{slug}
GET /api/v1/instruments/{slug}/musicians
GET /api/v1/instruments/{slug}/tracks
```

### Poems and Sung Versions

```http
GET /api/v1/poems
GET /api/v1/poems/{slug}
GET /api/v1/poems/{slug}/sung-versions
GET /api/v1/poems/{slug}/tracks

GET /api/v1/sung-versions/{slug}
GET /api/v1/sung-versions/{slug}/tracks
```

### Sessions, Events, Locations

```http
GET /api/v1/sessions
GET /api/v1/sessions/{slug}
GET /api/v1/events
GET /api/v1/events/{slug}
GET /api/v1/locations
GET /api/v1/locations/{slug}
```

### Awards, Certifications, Charts

```http
GET /api/v1/awards
GET /api/v1/awards/{slug}
GET /api/v1/certifications
GET /api/v1/charts
GET /api/v1/charts/{slug}
GET /api/v1/charts/{slug}/entries
```

### Search

```http
GET /api/v1/search?q=term&type=album&page=1&pageSize=20
```

---

## 11.2 Admin API Endpoints

Examples:

```http
POST   /api/v1/admin/albums
PUT    /api/v1/admin/albums/{id}
DELETE /api/v1/admin/albums/{id}
POST   /api/v1/admin/albums/{id}/restore

POST   /api/v1/admin/tracks
PUT    /api/v1/admin/tracks/{id}
DELETE /api/v1/admin/tracks/{id}

POST   /api/v1/admin/people
PUT    /api/v1/admin/people/{id}

POST   /api/v1/admin/credits
PUT    /api/v1/admin/credits/{id}
DELETE /api/v1/admin/credits/{id}

POST   /api/v1/admin/media/upload
PUT    /api/v1/admin/media/{id}
DELETE /api/v1/admin/media/{id}

POST   /api/v1/admin/localizations
PUT    /api/v1/admin/localizations/{id}
DELETE /api/v1/admin/localizations/{id}

POST   /api/v1/admin/aliases
PUT    /api/v1/admin/aliases/{id}
DELETE /api/v1/admin/aliases/{id}

POST   /api/v1/admin/citations
PUT    /api/v1/admin/citations/{id}
DELETE /api/v1/admin/citations/{id}
```

---

## 11.3 API Response Envelope

Success response:

```json
{
  "success": true,
  "data": {},
  "message": null,
  "errors": []
}
```

List response:

```json
{
  "success": true,
  "data": {
    "items": [],
    "page": 1,
    "pageSize": 20,
    "totalItems": 100,
    "totalPages": 5
  },
  "message": null,
  "errors": []
}
```

Error response:

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed.",
  "errors": [
    {
      "field": "Title",
      "code": "Required",
      "message": "Title is required."
    }
  ]
}
```

---

## 11.4 API Error Status Codes

| Status | Meaning |
|---|---|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad request/validation error |
| 401 | Unauthenticated |
| 403 | Forbidden |
| 404 | Not found |
| 409 | Conflict/duplicate/concurrency |
| 422 | Unprocessable entity |
| 429 | Too many requests |
| 500 | Server error |

---

## 12. Read Models and DTOs

Do not return EF entities.

Use DTOs such as:

```csharp
public sealed class AlbumListItemDto
{
    public int AlbumId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public string? CategoryName { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? DurationSeconds { get; init; }
    public string? CoverUrl { get; init; }
    public IReadOnlyList<string> Genres { get; init; } = [];
    public IReadOnlyList<string> Moods { get; init; } = [];
    public IReadOnlyList<string> PrimaryArtists { get; init; } = [];
}
```

```csharp
public sealed class AlbumDetailDto
{
    public int AlbumId { get; init; }
    public int EntityId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public string? Description { get; init; }
    public string? CategoryName { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public DateOnly? RecordingStartDate { get; init; }
    public DateOnly? RecordingEndDate { get; init; }
    public int? DurationSeconds { get; init; }
    public string? CoverUrl { get; init; }
    public string? CopyrightNotice { get; init; }

    public IReadOnlyList<AlbumTrackDto> Tracks { get; init; } = [];
    public IReadOnlyList<CreditDto> Credits { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Genres { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Moods { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Languages { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Countries { get; init; } = [];
    public IReadOnlyList<AlbumCompanyDto> Companies { get; init; } = [];
    public IReadOnlyList<IdentifierDto> Identifiers { get; init; } = [];
    public IReadOnlyList<MediaDto> Media { get; init; } = [];
    public IReadOnlyList<EntityLinkDto> Links { get; init; } = [];
    public IReadOnlyList<AliasDto> Aliases { get; init; } = [];
    public IReadOnlyList<TagDto> Tags { get; init; } = [];
    public IReadOnlyList<CitationDto> Citations { get; init; } = [];
    public IReadOnlyList<AwardAssignmentDto> Awards { get; init; } = [];
    public IReadOnlyList<CertificationAssignmentDto> Certifications { get; init; } = [];
    public IReadOnlyList<ChartEntryDto> ChartEntries { get; init; } = [];
}
```

```csharp
public sealed class TrackDetailDto
{
    public int TrackId { get; init; }
    public int EntityId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public int? DurationSeconds { get; init; }
    public bool IsInstrumental { get; init; }
    public bool IsExplicit { get; init; }
    public string? Isrc { get; init; }
    public short? Bpm { get; init; }
    public string? MusicalKeyName { get; init; }
    public string? VocalStyleName { get; init; }
    public string? LyricsAvailabilityName { get; init; }

    public IReadOnlyList<TrackAlbumAppearanceDto> Albums { get; init; } = [];
    public IReadOnlyList<CreditDto> Credits { get; init; } = [];
    public IReadOnlyList<MusicianCreditDto> Musicians { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Genres { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Moods { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Instruments { get; init; } = [];
    public IReadOnlyList<TrackLyricDto> Lyrics { get; init; } = [];
    public IReadOnlyList<TrackRelationDto> RelatedTracks { get; init; } = [];
    public IReadOnlyList<MediaDto> Media { get; init; } = [];
    public IReadOnlyList<EntityLinkDto> Links { get; init; } = [];
    public IReadOnlyList<CitationDto> Citations { get; init; } = [];
}
```

---

## 13. Specific Business Rules

### 13.1 Album Rules

1. An album must have one category.
2. An album may have multiple primary artists.
3. An album may contain multiple discs.
4. A track may appear on multiple albums.
5. Album track order is determined by:

```text
DiscNumber, SequenceNumber
```

6. If `AlbumTrack.TrackTitleOverride` exists, display it on that album page.
7. If `AlbumTrack.DurationSecondsOverride` exists, use it on that album page.
8. Album duration may be calculated as:

```sql
SUM(ISNULL(at.DurationSecondsOverride, t.DurationSeconds))
```

---

### 13.2 Track Rules

1. A track may have multiple artists.
2. A track may have multiple genres.
3. A track may have multiple moods.
4. A track may have multiple instruments.
5. A track may have multiple musicians.
6. A musician may play multiple instruments on one track.
7. A track may be instrumental.
8. Instrumental tracks must not require lyrics.
9. A track may use one or more sung versions.
10. A sung version may be used by multiple tracks.
11. Lyrics display must respect `LyricsAvailabilityType`.

---

### 13.3 Credit Rules

1. A credit must have either a person or a company, not both.
2. A credit role determines whether person/company is allowed.
3. A person may have multiple roles on the same entity.
4. A company may have multiple roles on the same entity.
5. A musician credit may include an instrument.
6. If `InstrumentId` is present, `PersonId` must be present.
7. Credits should be grouped by role on public pages.
8. Primary artist display should prioritize:

```text
PRIMARY_ARTIST
FEATURED_ARTIST
GUEST_ARTIST
```

---

### 13.4 Poem and Lyrics Rules

1. A poem has exactly one canonical original version.
2. A poem may have many sung versions.
3. A sung version may modify, omit, repeat, or rearrange text.
4. Track pages must show sung version text, not original poem text, unless explicitly comparing.
5. Poem pages must show canonical original text.
6. Sung version pages must link back to original poem.
7. If lyrics are restricted, do not expose sung version text in public API or UI.

---

### 13.5 Localization Rules

1. Every public page must render in the selected culture.
2. UI strings must be localized.
3. Database content must use `Localization` where available.
4. Fallback must be deterministic.
5. RTL must be applied for Persian and Arabic.
6. Language switcher must preserve current entity/page.

---

### 13.6 Citation Rules

1. Citations may be attached to any major entity.
2. Citations should be displayed near the fact they support.
3. If field-level citation exists, show it beside that field.
4. If entity-level citation exists, show it in references section.
5. Source pages should list cited entities.

---

## 14. Search Implementation

### 14.1 Initial SQL Full-Text Search

Create full-text catalog:

```sql
CREATE FULLTEXT CATALOG MusicEncyclopediaCatalog;
```

Index columns:

```text
Album.Title
Album.Description
Track.Title
Track.Description
Person.FullName
Person.Biography
Company.Name
Company.History
Poem.Title
Poem.CanonicalText
SungVersion.Text
Genre.Name
Genre.Description
Mood.Name
Instrument.Name
Instrument.Description
Tag.Name
```

Also include:

```text
Alias.AliasName
Localization.LocalizedText
```

Search service should query by culture.

---

### 14.2 Search Query Model

```csharp
public sealed class SearchQuery
{
    public string? Q { get; init; }
    public string? EntityType { get; init; }
    public string? Culture { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
```

---

### 14.3 Search Result Model

```csharp
public sealed class SearchResultDto
{
    public string EntityType { get; init; } = "";
    public int EntityId { get; init; }
    public string Title { get; init; } = "";
    public string? Subtitle { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string Url { get; init; } = "";
    public string Culture { get; init; } = "";
}
```

---

## 15. Caching Specification

### 15.1 Cache Keys

Use culture-aware cache keys.

Examples:

```text
home:{culture}
album:list:{culture}:{hash(filters)}
album:detail:{culture}:{slug}
track:detail:{culture}:{slug}
person:detail:{culture}:{slug}
company:detail:{culture}:{slug}
genre:detail:{culture}:{slug}
mood:detail:{culture}:{slug}
instrument:detail:{culture}:{slug}
poem:detail:{culture}:{slug}
search:{culture}:{hash(query)}
```

---

### 15.2 Cache Durations

| Page | Duration |
|---|---:|
| Home | 5 minutes |
| Public detail pages | 10 minutes |
| Public list pages | 5 minutes |
| Search results | 1 minute |
| Admin pages | No cache |
| API public detail | 10 minutes |
| API public list | 5 minutes |

---

### 15.3 Cache Invalidation

Invalidate cache when:

- Entity is created
- Entity is updated
- Entity is soft deleted
- Entity is restored
- Related entity changes
- Credit changes
- Localization changes
- Alias changes
- Media changes
- Tag changes
- Citation changes
- Award/certification/chart changes

Recommended approach:

Use entity change events.

Example:

```csharp
public sealed record EntityChangedEvent(
    string EntityTypeCode,
    int EntityId,
    string ChangeType
);
```

Change types:

```text
Created
Updated
Deleted
Restored
```

Cache invalidation service should remove related cache keys.

---

## 16. SEO Specification

### 16.1 Meta Tags

Every public page must include:

```html
<title>
<meta name="description">
<link rel="canonical">
<meta name="robots">
<meta property="og:title">
<meta property="og:description">
<meta property="og:image">
<meta property="og:url">
<meta property="og:type">
<meta name="twitter:card">
```

---

### 16.2 Hreflang

For multilingual pages:

```html
<link rel="alternate" hreflang="fa" href="..." />
<link rel="alternate" hreflang="en" href="..." />
<link rel="alternate" hreflang="ar" href="..." />
<link rel="alternate" hreflang="fr" href="..." />
<link rel="alternate" hreflang="x-default" href="..." />
```

---

### 16.3 Structured Data

Use JSON-LD for:

```text
MusicAlbum
MusicRecording
Person
Organization
MusicGroup
Event
Place
Award
CreativeWork
```

---

### 16.4 Sitemap

Generate:

```text
/sitemap.xml
```

Include:

- Albums
- Tracks
- People
- Companies
- Genres
- Moods
- Instruments
- Poems
- Sung versions
- Sessions
- Events
- Locations
- Awards
- Charts
- Sources
- Tags

Respect culture URLs.

Example:

```xml
<url>
  <loc>https://example.com/fa/albums/album-slug</loc>
  <lastmod>2026-07-22</lastmod>
  <changefreq>weekly</changefreq>
  <priority>0.8</priority>
</url>
```

---

### 16.5 Robots

```text
User-agent: *
Allow: /
Disallow: /admin
Disallow: /api
Disallow: /auth

Sitemap: https://example.com/sitemap.xml
```

---

## 17. Security Specification

### 17.1 Authentication

Use ASP.NET Core Identity.

Login route:

```text
/auth/login
```

Logout route:

```text
/auth/logout
```

Password rules:

```text
Minimum length: 10
Require digit
Require lowercase
Require uppercase
Require non-alphanumeric
Lockout after 5 failed attempts for 15 minutes
```

---

### 17.2 Authorization

Use policies.

Example:

```csharp
[Authorize(Policy = "CanManageAlbums")]
```

Never rely only on UI hiding.

Always enforce authorization on:

- Controllers
- API endpoints
- Service methods

---

### 17.3 Input Security

Requirements:

- Encode all output
- Sanitize rich text
- Reject dangerous HTML
- Use anti-forgery tokens on all forms
- Validate file uploads by MIME and extension
- Store uploaded files outside web root or use secure file provider
- Prevent path traversal
- Use parameterized queries only
- Never concatenate SQL

Allowed rich text tags:

```text
p
br
strong
em
u
blockquote
ul
ol
li
a
h2
h3
h4
span
```

Allowed link protocols:

```text
http
https
mailto
```

---

### 17.4 Security Headers

```text
Content-Security-Policy
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Referrer-Policy: strict-origin-when-cross-origin
Permissions-Policy
Strict-Transport-Security
```

---

### 17.5 Rate Limiting

Recommended limits:

| Endpoint | Limit |
|---|---:|
| Anonymous public API | 120 requests/minute |
| Search | 30 requests/minute |
| Login | 5 requests/minute/IP |
| Password reset | 3 requests/minute/IP |
| Admin API | 300 requests/minute/user |

---

### 17.6 Lyrics Access Control

If `LyricsAvailabilityType` is:

```text
PUBLIC
```

Show lyrics to everyone.

If:

```text
REGISTERED
```

Show lyrics only to authenticated users.

If:

```text
REQUEST
```

Show request/access message.

If:

```text
RESTRICTED
```

Show only to users with `CanViewRestrictedLyrics`.

If:

```text
NONE
```

Do not show lyrics.

---

## 18. Performance Specification

### 18.1 Database Rules

- Always paginate lists.
- Avoid `SELECT *`.
- Use projections.
- Avoid N+1 queries.
- Use indexes on foreign keys.
- Use filtered indexes for active entities.
- Use full-text search for text search.
- Use views for complex public pages.

---

### 18.2 Page Performance Targets

| Page | Target |
|---|---:|
| Server response time | < 300 ms |
| Largest Contentful Paint | < 2.5 s |
| Cumulative Layout Shift | < 0.1 |
| Total page weight | < 2 MB |
| Image formats | WebP/AVIF preferred |

---

### 18.3 Image Optimization

- Serve responsive images
- Use `srcset`
- Lazy-load below-the-fold images
- Generate thumbnails
- Use CDN
- Cache static assets

---

### 18.4 Query Guidance

For album detail, avoid separate queries for every track credit.

Prefer batched queries:

```text
1. Load album
2. Load album tracks
3. Load track IDs
4. Load all track credits for those track IDs
5. Load all musicians for those track IDs
6. Load all lyrics/sung versions for those track IDs
7. Assemble in memory
```

---

## 19. Logging and Monitoring

Use Serilog.

Log:

- Unhandled exceptions
- Authorization failures
- Validation failures in admin
- File upload failures
- Search failures
- Cache failures
- Background job failures
- Slow queries optional

Do not log:

- Passwords
- Tokens
- Full request bodies containing credentials
- Personal sensitive data

Structured log example:

```json
{
  "Timestamp": "2026-07-22T10:00:00Z",
  "Level": "Error",
  "Message": "Failed to load album detail",
  "AlbumSlug": "example-album",
  "Culture": "fa",
  "Exception": "..."
}
```

---

## 20. Background Jobs

Use background jobs for:

- Image thumbnail generation
- Search index updates
- Sitemap regeneration
- Cache warming
- Email notifications
- Database integrity checks optional

Recommended jobs:

| Job | Frequency |
|---|---|
| Rebuild search index | Daily |
| Warm popular page cache | Hourly |
| Generate sitemap | Daily |
| Cleanup expired upload temp files | Hourly |
| Cleanup soft-deleted temp data | Weekly |

---

## 21. Testing Specification

### 21.1 Unit Tests

Test:

- Slug generation
- Localization fallback
- Credit validation
- Lyrics availability rules
- Pagination logic
- Search query parsing
- DTO mapping
- Date precision display

---

### 21.2 Integration Tests

Test:

- Album list loads
- Album detail loads
- Track detail loads
- Person detail loads
- Search returns results
- Admin login required
- Admin authorization enforced
- API returns correct DTO shape
- Soft delete hides public entity
- Localization returns correct culture
- Restricted lyrics are not exposed

---

### 21.3 Example Test Cases

#### Album Tracklist Test

Given:

```text
Album has 2 discs
Disc 1 has tracks 1-5
Disc 2 has tracks 1-3
Track 3 on disc 1 has title override
Track 2 on disc 2 has duration override
```

Expect:

```text
Tracks grouped by disc
Override title shown
Override duration shown
Order correct by disc and sequence
```

---

#### Musician Credit Test

Given:

```text
Person A is credited as MUSICIAN on Track X with Instrument Piano
Person A is credited as MUSICIAN on Track Y with Instrument Guitar
```

Expect:

```text
Track X shows Person A: Piano
Track Y shows Person A: Guitar
Person page shows both instruments
Instrument pages list relevant tracks
```

---

#### Lyrics Restriction Test

Given:

```text
Track has sung version text
LyricsAvailabilityType = RESTRICTED
User is anonymous
```

Expect:

```text
Lyrics text not rendered
API does not return lyrics text
UI shows restricted notice
```

---

## 22. Deployment Specification

### 22.1 Environments

```text
Development
Staging
Production
```

---

### 22.2 Configuration

Use `appsettings.json` and environment variables.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MusicEncyclopedia;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Site": {
    "BaseUrl": "https://example.com",
    "DefaultCulture": "fa",
    "SupportedCultures": ["fa", "en", "ar", "fr"]
  },
  "Media": {
    "StoragePath": "/var/media",
    "CdnBaseUrl": "https://cdn.example.com",
    "MaxImageMb": 10,
    "MaxAudioMb": 100,
    "MaxVideoMb": 500,
    "MaxPdfMb": 50
  },
  "Search": {
    "Provider": "SqlServerFullText",
    "PageSize": 20
  },
  "Cache": {
    "Provider": "Memory",
    "DefaultPublicPageMinutes": 10
  }
}
```

---

### 22.3 Database Deployment

Use EF Core migrations or SQL scripts.

Recommended:

```text
EF Core migrations for application schema
Separate SQL scripts for views, full-text indexes, seed data
```

Seed required lookup data:

```text
EntityType
Language
Country
DatePrecision
AlbumCategory
CompanyType
CompanyRoleType
CreditRole
RoleScopeType
CreditRoleEntityType
LinkType
MediaType
MediaRoleType
AliasType
SourceType
AttributeDataType
PersonKind
PersonType
LocationType
EventType
SessionType
PublicationType
IdentifierType
CountryRoleType
AwardResultType
TrackRelationType
AlbumRelationType
TrackVersionType
VocalStyle
MusicalKey
LyricsAvailabilityType
```

---

### 22.4 Production Checklist

```text
HTTPS enabled
HSTS enabled
Security headers configured
Database backups enabled
SQL Server Agent jobs configured
Full-text catalog created
Indexes created
Admin account created
Roles seeded
Error pages configured
Logging configured
Rate limiting enabled
CDN configured
Media storage secured
Sitemap generated
Robots.txt deployed
Health checks enabled
Monitoring enabled
```

---

## 23. Health Checks

Expose:

```text
/health
/health/ready
/health/live
```

Checks:

```text
Database connection
Search availability
Media storage availability
Cache availability
Background job queue optional
```

---

## 24. Implementation Milestones

### Milestone 1: Foundation

Tasks:

```text
Create solution
Configure ASP.NET Core
Configure EF Core
Configure Identity
Configure localization
Configure routing
Configure Tailwind
Create layout
Create culture middleware
Create error pages
Create health checks
```

Definition of done:

```text
Site runs locally
/fa and /en render
Admin login works
Database connection works
```

---

### Milestone 2: Public Read Pages

Tasks:

```text
Implement Album list
Implement Album detail
Implement Track list
Implement Track detail
Implement Person list
Implement Person detail
Implement Company list/detail
Implement Genre/Mood/Instrument pages
Implement Poem/SungVersion pages
Implement Search page
```

Definition of done:

```text
All public pages render from database
Localization works
Pagination works
SEO tags present
```

---

### Milestone 3: Admin CRUD

Tasks:

```text
Admin layout
Album CRUD
Track CRUD
Person CRUD
Company CRUD
Credit editor
AlbumTrack editor
Genre/Mood/Instrument admin
Poem/SungVersion admin
Alias editor
Localization editor
Tag editor
Link editor
Citation editor
Attribute editor
```

Definition of done:

```text
Authorized users can manage core entities
Validation works
Soft delete works
Concurrency handled
```

---

### Milestone 4: Media and Files

Tasks:

```text
Media upload
Image thumbnails
Media assignment UI
Media library
File validation
CDN integration optional
```

Definition of done:

```text
Images display on public pages
Media can be assigned to entities
Uploads are secure
```

---

### Milestone 5: Advanced Encyclopedia Features

Tasks:

```text
Recording sessions
Performance events
Locations
Awards
Certifications
Charts
Sources
Citations UI
Related tracks/albums
Timeline on person page
Discography filters
```

Definition of done:

```text
Advanced entities are fully browsable and editable
```

---

### Milestone 6: Performance and Search

Tasks:

```text
Full-text search
Caching
Query optimization
Sitemap
Robots
JSON-LD
Hreflang
Performance profiling
```

Definition of done:

```text
Search works
Pages load fast
SEO requirements pass
```

---

### Milestone 7: Hardening and Deployment

Tasks:

```text
Security headers
Rate limiting
Logging
Monitoring
Backup validation
Deployment pipeline
Production configuration
Load testing optional
```

Definition of done:

```text
Production deployment successful
Admin and public site stable
```

---

## 25. Definition of Done for Every Feature

A feature is complete only when:

```text
It works in all supported cultures
RTL works for fa and ar
Database queries are parameterized
No EF entities are exposed to UI/API
Validation works server-side
Authorization is enforced
Soft delete is respected
Localization fallback works
SEO tags are correct
No console errors
No broken links
Tests pass
Code reviewed
Documentation updated
```

---

## 26. Coding Standards

### 26.1 C# Standards

- Use nullable reference types.
- Use DTOs for input/output.
- Use async/await for I/O.
- Avoid `Task.Result` and `.Wait()`.
- Use FluentValidation for view models.
- Use strongly typed IDs optional but recommended.
- Use sealed DTOs where appropriate.
- Avoid magic strings; use constants.

---

### 26.2 EF Core Standards

- Use `AsNoTracking()` for read queries.
- Use projections.
- Use explicit transactions for multi-entity writes.
- Use concurrency tokens.
- Avoid lazy loading.
- Use compiled queries for hot paths optional.

---

### 26.3 SQL Standards

- Use views for complex reads.
- Use indexes on FK columns.
- Avoid scalar UDFs in hot queries.
- Use `NVARCHAR` for multilingual text.
- Use `DATE` for dates, `DATETIME2` for timestamps.
- Use filtered indexes for active/public rows.

---

### 26.4 Razor View Standards

- Use view models only.
- Do not write complex LINQ in views.
- Use partials for repeated components.
- Use localization service for database content.
- Use tag helpers for links.
- Encode user content.

---

## 27. Recommended Folder Structure

```text
src/MusicEncyclopedia.Web/
  Controllers/
    Public/
    Admin/
    Api/
  ViewModels/
    Public/
    Admin/
    Api/
  Views/
    Shared/
    Home/
    Albums/
    Tracks/
    People/
    Companies/
    Genres/
    Moods/
    Instruments/
    Poems/
    SungVersions/
    Sessions/
    Events/
    Locations/
    Awards/
    Charts/
    Sources/
    Tags/
    Search/
    Admin/
  wwwroot/
    css/
    js/
    images/
  Middleware/
  Filters/
  Extensions/
```

---

## 28. Example Route Configuration

```csharp
app.MapControllerRoute(
    name: "localized-default",
    pattern: "{culture=fa}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

Culture constraint:

```csharp
constraints: new { culture = @"^(fa|en|ar|fr)$" }
```

---

## 29. Example Album Controller

```csharp
public sealed class AlbumsController : Controller
{
    private readonly IAlbumQueryService _albumQueryService;

    public AlbumsController(IAlbumQueryService albumQueryService)
    {
        _albumQueryService = albumQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string culture,
        AlbumListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _albumQueryService.GetAlbumsAsync(query, cancellationToken);
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, cancellationToken);

        if (album is null)
            return NotFound();

        return View(album);
    }
}
```

---

## 30. Example Query Service Interface

```csharp
public interface IAlbumQueryService
{
    Task<PagedResult<AlbumListItemDto>> GetAlbumsAsync(
        AlbumListQuery query,
        CancellationToken cancellationToken);

    Task<AlbumDetailDto?> GetAlbumBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AlbumTrackDto>> GetAlbumTracksAsync(
        int albumId,
        CancellationToken cancellationToken);
}
```

---

## 31. Example Dapper Query Pattern

```csharp
public async Task<AlbumDetailDto?> GetAlbumBySlugAsync(
    string slug,
    CancellationToken cancellationToken)
{
    const string sql = """
        SELECT
            a.AlbumId,
            a.EntityId,
            a.Slug,
            a.Title,
            a.OriginalTitle,
            a.EnglishTitle,
            a.Description,
            a.ReleaseDate,
            a.RecordingStartDate,
            a.RecordingEndDate,
            a.DurationSeconds,
            a.CopyrightNotice,
            ac.Name AS CategoryName,
            m.Url AS CoverUrl
        FROM dbo.Album AS a
        JOIN dbo.AlbumCategory AS ac
            ON ac.AlbumCategoryId = a.AlbumCategoryId
        LEFT JOIN dbo.Media AS m
            ON m.MediaId = a.CoverMediaId
        WHERE a.Slug = @Slug
          AND a.IsDeleted = 0;
        """;

    using var connection = _connectionFactory.Create();

    return await connection.QuerySingleOrDefaultAsync<AlbumDetailDto>(
        sql,
        new { Slug = slug });
}
```

---

## 32. Example Localization Service

```csharp
public interface IContentLocalizationService
{
    Task<string> GetLocalizedValueAsync(
        int entityId,
        string fieldName,
        string culture,
        string fallback,
        CancellationToken cancellationToken);
}
```

Fallback logic:

```csharp
var localized = await GetFromLocalizationTableAsync(entityId, fieldName, culture);

if (!string.IsNullOrWhiteSpace(localized))
    return localized;

if (!string.IsNullOrWhiteSpace(fallback))
    return fallback;

return string.Empty;
```

---

## 33. Example Credit Display Logic

```csharp
var groupedCredits = credits
    .OrderBy(c => c.RoleDisplayOrder)
    .ThenBy(c => c.DisplayOrder)
    .ThenBy(c => c.PersonFullName)
    .ThenBy(c => c.CompanyName)
    .GroupBy(c => c.RoleName)
    .Select(g => new CreditGroupDto
    {
        RoleName = g.Key,
        Credits = g.ToList()
    })
    .ToList();
```

Musician display:

```csharp
if (credit.RoleCode == "MUSICIAN" && !string.IsNullOrWhiteSpace(credit.InstrumentName))
{
    display = $"{credit.PersonFullName} — {credit.InstrumentName}";
}
else
{
    display = credit.PersonFullName ?? credit.CompanyName;
}
```

---

## 34. Example Lyrics Display Logic

```csharp
bool canViewLyrics = lyricsAvailability switch
{
    "PUBLIC" => true,
    "REGISTERED" => user.IsAuthenticated,
    "REQUEST" => false,
    "RESTRICTED" => user.HasPermission("CanViewRestrictedLyrics"),
    "NONE" => false,
    _ => false
};

if (track.IsInstrumental)
{
    model.LyricsSectionVisible = false;
    model.LyricsMessage = Localizer["Instrumental track"];
}
else if (!canViewLyrics)
{
    model.LyricsSectionVisible = false;
    model.LyricsMessage = Localizer["Lyrics are not publicly available."];
}
else
{
    model.LyricsSectionVisible = true;
}
```

---

## 35. Optional Future Enhancements

These are not required for MVP but should be considered:

- User accounts with favorites
- User ratings
- User corrections/submissions
- Editorial workflow
- Version history
- Advanced timeline visualization
- Interactive relationship graph
- Music player
- Audio waveform previews
- IIIF image support
- Public API keys
- GraphQL API
- External music service metadata imports
- Duplicate detection
- AI-assisted search
- Recommendation engine

---

## 36. Final Acceptance Criteria

The website is considered complete when:

```text
1. All public encyclopedia pages render correctly.
2. All admin CRUD pages work.
3. Multilingual content works for fa, en, ar, fr.
4. RTL layout works for fa and ar.
5. Album-track many-to-many relationships work.
6. Credits support people, companies, roles, and instruments.
7. Musicians can be linked to instruments on tracks/albums.
8. Poems and sung versions are clearly separated.
9. Lyrics display respects availability rules.
10. Media can be uploaded and assigned.
11. Citations are displayed.
12. Tags, aliases, and localizations work.
13. Search returns relevant results.
14. SEO tags, sitemap, and structured data are present.
15. Admin authorization is enforced.
16. Soft delete hides content from public site.
17. Performance targets are met.
18. Security requirements are met.
19. Tests pass.
20. Production deployment succeeds.
```