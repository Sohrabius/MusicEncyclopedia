using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Services.Services;
using MusicEncyclopedia.Services.Validators;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Step-by-step album creation wizard. One atomic POST creates the album plus all
/// of its dependencies: classification joins, companies, identifiers, tracks (with
/// genres/moods/instruments/version types, sung versions and per-track credits),
/// album credits, tags, links and related albums.
/// Route: /admin/albums/wizard
/// Requires the CanManageAlbums permission policy.
/// </summary>
[Route("/admin/albums/wizard")]
[Authorize(Policy = PermissionConstants.CanManageAlbums)]
public sealed class AlbumWizardController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<AlbumWizardController> _logger;

    public AlbumWizardController(AppDbContext db, ILogger<AlbumWizardController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  GET — render the wizard with dropdown data
    // ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var viewModel = new AlbumWizardViewModel();
        await PopulateDropdownsAsync(viewModel, cancellationToken);

        ViewData["Title"] = "ایجاد آلبوم (گام‌به‌گام)";
        ViewData["ActiveMenu"] = "Albums";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  GET — a fresh empty track card partial (used by the wizard's "add track")
    // ──────────────────────────────────────────────

    [HttpGet("track-card")]
    public async Task<IActionResult> TrackCard(
        int index = 0,
        CancellationToken cancellationToken = default)
    {
        var shared = new AlbumWizardViewModel();
        await PopulateDropdownsAsync(shared, cancellationToken);

        ViewData["TrackIndex"] = Math.Max(0, index);
        ViewData["Shared"] = shared;

        return PartialView("_WizardTrackCard", new TrackWizardItem { DiscNumber = 1 });
    }

    // ──────────────────────────────────────────────
    //  POST — validate + persist everything atomically
    // ──────────────────────────────────────────────

    // ──────────────────────────────────────────────
    //  POST — quick add a small entity / lookup row so it can be picked
    //  immediately from the dropdown or checkbox list it was opened from.
    // ──────────────────────────────────────────────

    [HttpPost("quick-add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickAdd(
        QuickAddFormModel form,
        CancellationToken cancellationToken = default)
    {
        var user = User.Identity?.Name ?? "system";
        var now = DateTime.UtcNow;
        var target = form.Target?.Trim().ToLowerInvariant() ?? "";

        var name = form.Name?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { success = false, errors = new[] { "نام الزامی است." } });
        }

        if (IsLookupTarget(target))
        {
            return await QuickAddLookupAsync(target, name, form.Code?.Trim(), cancellationToken);
        }

        return await QuickAddEntityAsync(target, name, form, user, now, cancellationToken);
    }

    /// <summary>Quick-adds a Person / Company / Poem / Genre / Mood / Instrument / Tag (Entity-backed).</summary>
    private async Task<IActionResult> QuickAddEntityAsync(
        string target,
        string name,
        QuickAddFormModel form,
        string user,
        DateTime now,
        CancellationToken ct)
    {
        var slugService = new SlugService();
        var baseSlug = slugService.GenerateSlug(name);
        var typeIds = await ResolveEntityTypeIdsAsync(ct);

        try
        {
            switch (target)
            {
                case "person":
                {
                    var slug = await EnsureUniqueSlugAsync(_db.People.Select(p => p.Slug), baseSlug, ct);
                    var entity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Person],
                        Slug = slug,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    var person = new Person
                    {
                        Entity = entity,
                        FullName = name,
                        EnglishName = form.EnglishName?.Trim(),
                        Slug = slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.People.Add(person);
                    await _db.SaveChangesAsync(ct);
                    await InvalidateEntityCacheAsync(EntityTypeConstants.Person, person.PersonId, "Created");
                    return Json(new { success = true, id = person.PersonId, name = person.FullName });
                }
                case "company":
                {
                    var slug = await EnsureUniqueSlugAsync(_db.Companies.Select(c => c.Slug), baseSlug, ct);
                    var entity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Company],
                        Slug = slug,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    var company = new Company
                    {
                        Entity = entity,
                        Name = name,
                        EnglishName = form.EnglishName?.Trim(),
                        Slug = slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.Companies.Add(company);
                    await _db.SaveChangesAsync(ct);
                    await InvalidateEntityCacheAsync(EntityTypeConstants.Company, company.CompanyId, "Created");
                    return Json(new { success = true, id = company.CompanyId, name = company.Name });
                }
                case "poem":
                {
                    var slug = await EnsureUniqueSlugAsync(_db.Poems.Select(p => p.Slug), baseSlug, ct);
                    var entity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Poem],
                        Slug = slug,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    var poem = new Poem
                    {
                        Entity = entity,
                        Title = name,
                        PersonId = form.PoetId,
                        Slug = slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.Poems.Add(poem);
                    await _db.SaveChangesAsync(ct);
                    await InvalidateEntityCacheAsync(EntityTypeConstants.Poem, poem.PoemId, "Created");
                    return Json(new { success = true, id = poem.PoemId, name = poem.Title });
                }
                case "genre":
                {
                    var slug = await EnsureUniqueSlugAsync(_db.Genres.Select(g => g.Slug), baseSlug, ct);
                    var entity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Genre],
                        Slug = slug,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    var genre = new Genre
                    {
                        Entity = entity,
                        Name = name,
                        Slug = slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.Genres.Add(genre);
                    await _db.SaveChangesAsync(ct);
                    await InvalidateEntityCacheAsync(EntityTypeConstants.Genre, genre.GenreId, "Created");
                    return Json(new { success = true, id = genre.GenreId, name = genre.Name });
                }
                case "mood":
                {
                    var slug = await EnsureUniqueSlugAsync(_db.Moods.Select(m => m.Slug), baseSlug, ct);
                    var entity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Mood],
                        Slug = slug,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    var mood = new Mood
                    {
                        Entity = entity,
                        Name = name,
                        Slug = slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.Moods.Add(mood);
                    await _db.SaveChangesAsync(ct);
                    await InvalidateEntityCacheAsync(EntityTypeConstants.Mood, mood.MoodId, "Created");
                    return Json(new { success = true, id = mood.MoodId, name = mood.Name });
                }
                case "instrument":
                {
                    var slug = await EnsureUniqueSlugAsync(_db.Instruments.Select(i => i.Slug), baseSlug, ct);
                    var entity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Instrument],
                        Slug = slug,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    var instrument = new Instrument
                    {
                        Entity = entity,
                        Name = name,
                        Slug = slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.Instruments.Add(instrument);
                    await _db.SaveChangesAsync(ct);
                    await InvalidateEntityCacheAsync(EntityTypeConstants.Instrument, instrument.InstrumentId, "Created");
                    return Json(new { success = true, id = instrument.InstrumentId, name = instrument.Name });
                }
                case "tag":
                {
                    var slug = await EnsureUniqueSlugAsync(_db.Tags.Select(t => t.Slug), baseSlug, ct);
                    var entity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Tag],
                        Slug = slug,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    var tag = new Tag
                    {
                        Entity = entity,
                        Name = name,
                        Slug = slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.Tags.Add(tag);
                    await _db.SaveChangesAsync(ct);
                    await InvalidateEntityCacheAsync(EntityTypeConstants.Tag, tag.TagId, "Created");
                    return Json(new { success = true, id = tag.TagId, name = tag.Name });
                }
                default:
                    return BadRequest(new { success = false, errors = new[] { "نوع مورد پشتیبانی نمی‌شود." } });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quick add failed: Target={Target}, Name={Name}", target, name);
            return BadRequest(new { success = false, errors = new[] { "ذخیره ناموفق بود. لطفاً دوباره تلاش کنید." } });
        }
    }

    /// <summary>Quick-adds a Code+Name lookup row (albumcategory / language / country / trackversiontype).</summary>
    private async Task<IActionResult> QuickAddLookupAsync(
        string target,
        string name,
        string? code,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(new { success = false, errors = new[] { "کد الزامی است." } });
        }
        code = code.Trim();

        try
        {
            switch (target)
            {
                case "albumcategory":
                {
                    if (await _db.AlbumCategories.AnyAsync(c => c.Code == code, ct))
                    {
                        return BadRequest(new { success = false, errors = new[] { "این کد قبلاً ثبت شده است." } });
                    }
                    var category = new AlbumCategory { Code = code, Name = name };
                    _db.AlbumCategories.Add(category);
                    await _db.SaveChangesAsync(ct);
                    return Json(new { success = true, id = category.AlbumCategoryId, name = category.Name });
                }
                case "language":
                {
                    if (await _db.Languages.AnyAsync(l => l.Code == code, ct))
                    {
                        return BadRequest(new { success = false, errors = new[] { "این کد قبلاً ثبت شده است." } });
                    }
                    var language = new Language { Code = code, Name = name };
                    _db.Languages.Add(language);
                    await _db.SaveChangesAsync(ct);
                    return Json(new { success = true, id = language.LanguageId, name = language.Name });
                }
                case "country":
                {
                    if (await _db.Countries.AnyAsync(c => c.Code == code, ct))
                    {
                        return BadRequest(new { success = false, errors = new[] { "این کد قبلاً ثبت شده است." } });
                    }
                    var country = new Country { Code = code, Name = name };
                    _db.Countries.Add(country);
                    await _db.SaveChangesAsync(ct);
                    return Json(new { success = true, id = country.CountryId, name = country.Name });
                }
                case "trackversiontype":
                {
                    if (await _db.TrackVersionTypes.AnyAsync(t => t.Code == code, ct))
                    {
                        return BadRequest(new { success = false, errors = new[] { "این کد قبلاً ثبت شده است." } });
                    }
                    var versionType = new TrackVersionType { Code = code, Name = name };
                    _db.TrackVersionTypes.Add(versionType);
                    await _db.SaveChangesAsync(ct);
                    return Json(new { success = true, id = versionType.TrackVersionTypeId, name = versionType.Name });
                }
                default:
                    return BadRequest(new { success = false, errors = new[] { "نوع مورد پشتیبانی نمی‌شود." } });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quick add lookup failed: Target={Target}, Name={Name}", target, name);
            return BadRequest(new { success = false, errors = new[] { "ذخیره ناموفق بود. لطفاً دوباره تلاش کنید." } });
        }
    }

    private static bool IsLookupTarget(string target) =>
        target is "albumcategory" or "language" or "country" or "trackversiontype";

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(
        AlbumWizardViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        var user = User.Identity?.Name ?? "system";
        var now = DateTime.UtcNow;
        var slugService = new SlugService();

        // Normalize slugs — auto-generate from titles when the client left them blank.
        if (string.IsNullOrWhiteSpace(viewModel.Slug))
        {
            viewModel.Slug = slugService.GenerateSlug(viewModel.Title);
        }
        foreach (var track in viewModel.Tracks)
        {
            if (string.IsNullOrWhiteSpace(track.Slug))
            {
                track.Slug = slugService.GenerateSlug(track.Title);
            }
        }

        // The model binder runs data-annotation validation before this action, and
        // the non-nullable Slug properties carry an implicit [Required] — so an
        // empty posted Slug (the auto-generate path) lands in ModelState as an error
        // even though the controller has now generated one. Drop those stale errors;
        // RunValidationAsync re-validates the generated slugs explicitly.
        ModelState.Remove(nameof(AlbumWizardViewModel.Slug));
        for (var i = 0; i < viewModel.Tracks.Count; i++)
        {
            ModelState.Remove($"Tracks[{i}].Slug");
        }

        await RunValidationAsync(viewModel, slugService, cancellationToken);

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel, cancellationToken);
            ViewData["Title"] = "ایجاد آلبوم (گام‌به‌گام)";
            ViewData["ActiveMenu"] = "Albums";
            return View(viewModel);
        }

        try
        {
            var (albumId, createdIds) = await PersistAsync(
                viewModel, user, now, slugService, cancellationToken);

            await InvalidateEntityCacheAsync(EntityTypeConstants.Album, albumId, "Created");
            foreach (var (typeCode, id) in createdIds)
            {
                await InvalidateEntityCacheAsync(typeCode, id, "Created");
            }
            await InvalidateBroadCacheAsync();

            _logger.LogInformation(
                "Wizard album created: AlbumId={AlbumId}, Title={Title}, Tracks={TrackCount}, Credits={CreditCount}",
                albumId, viewModel.Title, viewModel.Tracks.Count,
                viewModel.AlbumCredits.Count + viewModel.Tracks.Sum(t => t.Credits.Count));

            SetSuccessMessage($"آلبوم «{viewModel.Title}» و تمام وابستگی‌های آن با موفقیت ایجاد شد.");
            return RedirectToAction("Edit", "Albums", new { id = albumId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create album via wizard: Title={Title}", viewModel.Title);
            SetErrorMessage("ایجاد آلبوم ناموفق بود. لطفاً دوباره تلاش کنید.");
            await PopulateDropdownsAsync(viewModel, cancellationToken);
            ViewData["Title"] = "ایجاد آلبوم (گام‌به‌گام)";
            ViewData["ActiveMenu"] = "Albums";
            return View(viewModel);
        }
    }

    // ──────────────────────────────────────────────
    //  Persistence
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates the album, tracks and every dependency in one database transaction.
    /// Returns the new album id plus the ids of all created children (type code + id)
    /// so the caller can invalidate caches.
    /// </summary>
    private async Task<(int AlbumId, IReadOnlyList<(string TypeCode, int Id)> CreatedIds)> PersistAsync(
        AlbumWizardViewModel vm,
        string user,
        DateTime now,
        SlugService slugService,
        CancellationToken ct)
    {
        var typeIds = await ResolveEntityTypeIdsAsync(ct);
        var createdIds = new List<(string, int)>();

        // EF's retrying execution strategy (EnableRetryOnFailure) forbids
        // user-initiated transactions outside the strategy, so the whole save
        // runs inside CreateExecutionStrategy() as one retriable unit.
        var strategy = _db.Database.CreateExecutionStrategy();
        var album = await strategy.ExecuteAsync(
            () => PersistCoreAsync(vm, user, now, slugService, typeIds, ct));

        createdIds.Add((EntityTypeConstants.Album, album.AlbumId));
        foreach (var track in album.AlbumTracks.Select(at => at.Track))
        {
            createdIds.Add((EntityTypeConstants.Track, track.TrackId));
        }
        foreach (var sv in album.AlbumTracks.SelectMany(at => at.Track.TrackSungVersions))
        {
            createdIds.Add((EntityTypeConstants.SungVersion, sv.SungVersionId));
        }

        return (album.AlbumId, createdIds);
    }

    /// <summary>
    /// Persists the album and all its dependencies inside one user-initiated
    /// transaction, executed as a retriable unit by the configured strategy.
    /// </summary>
    private async Task<Album> PersistCoreAsync(
        AlbumWizardViewModel vm,
        string user,
        DateTime now,
        SlugService slugService,
        Dictionary<string, int> typeIds,
        CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            // ── Album + base Entity ─────────────────────────────
            var albumEntity = new Entity
            {
                EntityTypeId = typeIds[EntityTypeConstants.Album],
                Slug = await EnsureUniqueSlugAsync(_db.Albums.Select(a => a.Slug), vm.Slug, ct),
                CreatedBy = user,
                CreatedAt = now
            };
            _db.Entities.Add(albumEntity);

            var album = new Album
            {
                Entity = albumEntity,
                Title = vm.Title.Trim(),
                TitleSort = vm.TitleSort,
                OriginalTitle = vm.OriginalTitle,
                EnglishTitle = vm.EnglishTitle,
                AlbumCategoryId = vm.AlbumCategoryId,
                ReleaseDate = vm.ReleaseDate,
                ReleaseDatePrecision = vm.ReleaseDatePrecision,
                RecordingStartDate = vm.RecordingStartDate,
                RecordingEndDate = vm.RecordingEndDate,
                RecordingDatePrecision = vm.RecordingDatePrecision,
                Description = vm.Description,
                CoverMediaId = vm.CoverMediaId,
                DurationSeconds = vm.DurationSeconds,
                CopyrightNotice = vm.CopyrightNotice,
                Slug = albumEntity.Slug,
                IsOfficial = vm.IsOfficial,
                IsDeleted = false,
                CreatedBy = user,
                CreatedAt = now
            };
            _db.Albums.Add(album);

            // ── Step 2 — classification & distribution joins ────
            foreach (var id in vm.GenreIds.Distinct())
            {
                album.AlbumGenres.Add(new AlbumGenre { GenreId = id });
            }
            foreach (var id in vm.MoodIds.Distinct())
            {
                album.AlbumMoods.Add(new AlbumMood { MoodId = id });
            }
            foreach (var id in vm.LanguageIds.Distinct())
            {
                album.AlbumLanguages.Add(new AlbumLanguage { LanguageId = id });
            }
            foreach (var id in vm.CountryIds.Distinct())
            {
                album.AlbumCountries.Add(new AlbumCountry { CountryId = id });
            }
            foreach (var company in vm.Companies.Where(c => !IsEmptyCompanyRow(c)))
            {
                album.AlbumCompanies.Add(new AlbumCompany
                {
                    CompanyId = company.CompanyId,
                    CompanyRoleTypeId = company.CompanyRoleTypeId,
                    CatalogNumber = company.CatalogNumber,
                    Barcode = company.Barcode
                });
            }
            foreach (var identifier in vm.Identifiers.Where(i => !string.IsNullOrWhiteSpace(i.Value)))
            {
                album.AlbumIdentifiers.Add(new AlbumIdentifier
                {
                    IdentifierTypeId = identifier.IdentifierTypeId,
                    Value = identifier.Value.Trim()
                });
            }

            // ── Step 3 — tracks ─────────────────────────────────
            var createdTracks = new List<Track>();
            foreach (var trackItem in vm.Tracks)
            {
                var trackEntity = new Entity
                {
                    EntityTypeId = typeIds[EntityTypeConstants.Track],
                    Slug = await EnsureUniqueSlugAsync(_db.Tracks.Select(t => t.Slug), trackItem.Slug, ct),
                    CreatedBy = user,
                    CreatedAt = now
                };
                _db.Entities.Add(trackEntity);

                var track = new Track
                {
                    Entity = trackEntity,
                    Title = trackItem.Title.Trim(),
                    TitleSort = trackItem.TitleSort,
                    OriginalTitle = trackItem.OriginalTitle,
                    EnglishTitle = trackItem.EnglishTitle,
                    DurationSeconds = trackItem.DurationSeconds,
                    ReleaseDate = trackItem.ReleaseDate,
                    ReleaseDatePrecision = trackItem.ReleaseDatePrecision,
                    RecordingStartDate = trackItem.RecordingStartDate,
                    RecordingEndDate = trackItem.RecordingEndDate,
                    RecordingDatePrecision = trackItem.RecordingDatePrecision,
                    Description = trackItem.Description,
                    LyricsAvailabilityTypeId = trackItem.LyricsAvailabilityTypeId,
                    VocalStyleId = trackItem.VocalStyleId,
                    MusicalKeyId = trackItem.MusicalKeyId,
                    BPM = trackItem.BPM,
                    ISRC = trackItem.ISRC,
                    IsInstrumental = trackItem.IsInstrumental,
                    IsExplicit = trackItem.IsExplicit,
                    CopyrightNotice = trackItem.CopyrightNotice,
                    Slug = trackEntity.Slug,
                    IsDeleted = false,
                    CreatedBy = user,
                    CreatedAt = now
                };
                _db.Tracks.Add(track);
                createdTracks.Add(track);

                // Album-track row (sequence = position within disc, 1..N)
                var disc = Math.Max(1, trackItem.DiscNumber);
                var sequence = NextSequence(album, disc);
                album.AlbumTracks.Add(new AlbumTrack
                {
                    Track = track,
                    DiscNumber = disc,
                    TrackNumber = trackItem.TrackNumber > 0 ? trackItem.TrackNumber : sequence,
                    SequenceNumber = sequence,
                    TrackTitleOverride = null,
                    IsBonus = trackItem.IsBonus,
                    IsHidden = trackItem.IsHidden
                });

                foreach (var id in trackItem.GenreIds.Distinct())
                {
                    track.TrackGenres.Add(new TrackGenre { GenreId = id });
                }
                foreach (var id in trackItem.MoodIds.Distinct())
                {
                    track.TrackMoods.Add(new TrackMood { MoodId = id });
                }
                foreach (var id in trackItem.InstrumentIds.Distinct())
                {
                    track.TrackInstruments.Add(new TrackInstrument { InstrumentId = id });
                }
                foreach (var id in trackItem.VersionTypeIds.Distinct())
                {
                    track.TrackVersionTypeAssignments.Add(new TrackVersionTypeAssignment { TrackVersionTypeId = id });
                }

                // Sung versions
                var sungVersionSequence = 1;
                foreach (var sv in trackItem.SungVersions.Where(s => !IsEmptySungVersionRow(s)))
                {
                    if (!sv.IsNew)
                    {
                        track.TrackSungVersions.Add(new TrackSungVersion
                        {
                            SungVersionId = sv.SungVersionId!.Value,
                            SequenceNumber = sungVersionSequence++,
                            IsPrimary = sv.IsCanonical
                        });
                        continue;
                    }

                    var svEntity = new Entity
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.SungVersion],
                        Slug = await EnsureUniqueSlugAsync(
                            _db.SungVersions.Select(s => s.Slug),
                            sv.Title ?? $"sung-version-{Guid.NewGuid():N}"[..16],
                            ct),
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.Entities.Add(svEntity);

                    var sungVersion = new SungVersion
                    {
                        Entity = svEntity,
                        PoemId = sv.PoemId!.Value,
                        Title = sv.Title!.Trim(),
                        VocalStyleId = sv.VocalStyleId,
                        Text = sv.Text,
                        Notes = sv.Notes,
                        IsCanonical = sv.IsCanonical,
                        Slug = svEntity.Slug,
                        IsDeleted = false,
                        CreatedBy = user,
                        CreatedAt = now
                    };
                    _db.SungVersions.Add(sungVersion);

                    track.TrackSungVersions.Add(new TrackSungVersion
                    {
                        SungVersion = sungVersion,
                        SequenceNumber = sungVersionSequence++,
                        IsPrimary = sv.IsCanonical
                    });
                }
            }

            // Save FK-linked graph first so generated ids are available for the
            // polymorphic rows (Credit / TagAssignment / EntityLink) and album
            // relations, whose IDs are generated by the first save.
            await _db.SaveChangesAsync(ct);

            // ── Step 4 — album credits ──────────────────────────
            foreach (var credit in vm.AlbumCredits.Where(c => !IsEmptyCreditRow(c)))
            {
                _db.Credits.Add(new Credit
                {
                    EntityTypeId = typeIds[EntityTypeConstants.Album],
                    EntityId = album.EntityId,
                    CreditRoleId = credit.CreditRoleId,
                    RoleScopeTypeId = credit.RoleScopeTypeId,
                    PersonId = credit.PersonId,
                    CompanyId = credit.CompanyId,
                    InstrumentId = credit.InstrumentId,
                    DisplayOrder = credit.DisplayOrder,
                    IsPrimary = credit.IsPrimary,
                    Notes = credit.Notes,
                    CreatedBy = user,
                    CreatedAt = now
                });
            }

            // Per-track credits
            for (var i = 0; i < vm.Tracks.Count; i++)
            {
                var trackEntityId = createdTracks[i].EntityId;
                foreach (var credit in vm.Tracks[i].Credits.Where(c => !IsEmptyCreditRow(c)))
                {
                    _db.Credits.Add(new Credit
                    {
                        EntityTypeId = typeIds[EntityTypeConstants.Track],
                        EntityId = trackEntityId,
                        CreditRoleId = credit.CreditRoleId,
                        RoleScopeTypeId = credit.RoleScopeTypeId,
                        PersonId = credit.PersonId,
                        CompanyId = credit.CompanyId,
                        InstrumentId = credit.InstrumentId,
                        DisplayOrder = credit.DisplayOrder,
                        IsPrimary = credit.IsPrimary,
                        Notes = credit.Notes,
                        CreatedBy = user,
                        CreatedAt = now
                    });
                }
            }

            // ── Step 5 — tags, links & related albums ───────────
            foreach (var tagId in vm.TagIds.Distinct())
            {
                _db.TagAssignments.Add(new TagAssignment
                {
                    EntityTypeId = typeIds[EntityTypeConstants.Album],
                    EntityId = album.EntityId,
                    TagId = tagId
                });
            }
            foreach (var link in vm.Links.Where(l => !IsEmptyLinkRow(l)))
            {
                _db.EntityLinks.Add(new EntityLink
                {
                    EntityTypeId = typeIds[EntityTypeConstants.Album],
                    EntityId = album.EntityId,
                    LinkTypeId = link.LinkTypeId,
                    Url = link.Url.Trim(),
                    Title = link.Label,
                    IsDeleted = false
                });
            }
            foreach (var relation in vm.RelatedAlbums.Where(r => !IsEmptyRelationRow(r)))
            {
                if (relation.AlbumId == album.AlbumId)
                {
                    continue; // no self-relations
                }
                _db.AlbumRelations.Add(new AlbumRelation
                {
                    AlbumId = album.AlbumId,
                    RelatedAlbumId = relation.AlbumId,
                    AlbumRelationTypeId = relation.AlbumRelationTypeId
                });
                // Symmetric reverse row so the relation shows from either side.
                _db.AlbumRelations.Add(new AlbumRelation
                {
                    AlbumId = relation.AlbumId,
                    RelatedAlbumId = album.AlbumId,
                    AlbumRelationTypeId = relation.AlbumRelationTypeId
                });
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return album;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Next 1..N sequence number for a disc across the tracks already collected
    /// on the album (works before any ids are generated).
    /// </summary>
    private static int NextSequence(Album album, int disc)
    {
        return album.AlbumTracks.Count(at => at.DiscNumber == disc) + 1;
    }

    // ──────────────────────────────────────────────
    //  Validation
    // ──────────────────────────────────────────────

    private async Task RunValidationAsync(
        AlbumWizardViewModel vm,
        SlugService slugService,
        CancellationToken ct)
    {
        var albumValidator = new AlbumValidator();
        var albumResult = await albumValidator.ValidateAsync(
            new AlbumFormModel
            {
                Title = vm.Title,
                Slug = vm.Slug,
                ReleaseDatePrecision = vm.ReleaseDatePrecision,
                DurationSeconds = vm.DurationSeconds
            },
            ct);
        foreach (var error in albumResult.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (vm.AlbumCategoryId <= 0)
        {
            ModelState.AddModelError(nameof(vm.AlbumCategoryId), "دسته‌بندی آلبوم الزامی است.");
        }

        if (string.IsNullOrWhiteSpace(vm.Slug) || !slugService.IsValidSlug(vm.Slug))
        {
            ModelState.AddModelError(nameof(vm.Slug), "Slug معتبر نیست.");
        }

        var trackValidator = new TrackValidator();
        for (var i = 0; i < vm.Tracks.Count; i++)
        {
            var track = vm.Tracks[i];
            var trackResult = await trackValidator.ValidateAsync(
                new TrackFormModel
                {
                    Title = track.Title,
                    Isrc = track.ISRC,
                    Bpm = track.BPM,
                    DurationSeconds = track.DurationSeconds
                },
                ct);
            foreach (var error in trackResult.Errors)
            {
                ModelState.AddModelError($"Tracks[{i}].{error.PropertyName}", error.ErrorMessage);
            }

            if (string.IsNullOrWhiteSpace(track.Slug) || !slugService.IsValidSlug(track.Slug))
            {
                ModelState.AddModelError($"Tracks[{i}].Slug", "Slug معتبر نیست.");
            }

            for (var j = 0; j < track.SungVersions.Count; j++)
            {
                var sv = track.SungVersions[j];
                if (IsEmptySungVersionRow(sv))
                {
                    continue;
                }
                if (sv.IsNew && string.IsNullOrWhiteSpace(sv.Title))
                {
                    ModelState.AddModelError(
                        $"Tracks[{i}].SungVersions[{j}].Title", "عنوان نسخه خوانده‌شده الزامی است.");
                }
                if (sv.IsNew && sv.PoemId is null)
                {
                    ModelState.AddModelError(
                        $"Tracks[{i}].SungVersions[{j}].PoemId", "برای نسخه جدید، انتخاب شعر الزامی است.");
                }
            }

            for (var j = 0; j < track.Credits.Count; j++)
            {
                ValidateCreditRow($"Tracks[{i}].Credits[{j}]", track.Credits[j]);
            }
            ValidateDuplicateCredits($"Tracks[{i}].Credits", track.Credits);
        }

        for (var i = 0; i < vm.AlbumCredits.Count; i++)
        {
            ValidateCreditRow($"AlbumCredits[{i}]", vm.AlbumCredits[i]);
        }
        ValidateDuplicateCredits(nameof(vm.AlbumCredits), vm.AlbumCredits);

        for (var i = 0; i < vm.Companies.Count; i++)
        {
            var company = vm.Companies[i];
            if (IsEmptyCompanyRow(company))
            {
                continue;
            }
            if (company.CompanyId <= 0)
            {
                ModelState.AddModelError($"Companies[{i}].CompanyId", "انتخاب شرکت الزامی است.");
            }
        }

        for (var i = 0; i < vm.Identifiers.Count; i++)
        {
            var identifier = vm.Identifiers[i];
            if (string.IsNullOrWhiteSpace(identifier.Value))
            {
                continue;
            }
            if (identifier.Value.Trim().Length > 255)
            {
                ModelState.AddModelError($"Identifiers[{i}].Value", "مقدار شناسه نباید از ۲۵۵ نویسه بیشتر باشد.");
            }
        }

        for (var i = 0; i < vm.Links.Count; i++)
        {
            var link = vm.Links[i];
            if (IsEmptyLinkRow(link))
            {
                continue;
            }
            if (!Uri.TryCreate(link.Url, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError($"Links[{i}].Url", "URL باید آدرس http/https معتبر باشد.");
            }
        }

        for (var i = 0; i < vm.RelatedAlbums.Count; i++)
        {
            var relation = vm.RelatedAlbums[i];
            if (IsEmptyRelationRow(relation))
            {
                continue;
            }
            if (relation.AlbumId <= 0)
            {
                ModelState.AddModelError($"RelatedAlbums[{i}].AlbumId", "انتخاب آلبوم الزامی است.");
            }
            if (relation.AlbumRelationTypeId <= 0)
            {
                ModelState.AddModelError($"RelatedAlbums[{i}].AlbumRelationTypeId", "نوع رابطه الزامی است.");
            }
        }

        // Jump the stepper back to the first step that has errors.
        vm.ActiveStep = InferFailingStep(ModelState);
    }

    private void ValidateCreditRow(string prefix, CreditWizardItem credit)
    {
        if (IsEmptyCreditRow(credit))
        {
            return;
        }
        if (credit.CreditRoleId <= 0)
        {
            ModelState.AddModelError($"{prefix}.CreditRoleId", "نقش الزامی است.");
        }
        if (credit.RoleScopeTypeId <= 0)
        {
            ModelState.AddModelError($"{prefix}.RoleScopeTypeId", "حوزه نقش الزامی است.");
        }
        if (!(credit.PersonId.HasValue ^ credit.CompanyId.HasValue))
        {
            ModelState.AddModelError($"{prefix}.PersonId", "دقیقاً یکی از شخص یا شرکت را انتخاب کنید.");
        }
        if (credit.InstrumentId.HasValue && !credit.PersonId.HasValue)
        {
            ModelState.AddModelError($"{prefix}.InstrumentId", "برای اعتبار ساز، انتخاب شخص الزامی است.");
        }
    }

    private void ValidateDuplicateCredits(string prefix, IReadOnlyList<CreditWizardItem> credits)
    {
        var populated = credits
            .Select((credit, index) => (credit, index))
            .Where(x => !IsEmptyCreditRow(x.credit))
            .GroupBy(x => new
            {
                x.credit.CreditRoleId,
                x.credit.RoleScopeTypeId,
                x.credit.PersonId,
                x.credit.CompanyId,
                x.credit.InstrumentId
            });
        foreach (var duplicate in populated.Where(x => x.Count() > 1).SelectMany(x => x.Skip(1)))
        {
            ModelState.AddModelError($"{prefix}[{duplicate.index}].CreditRoleId", "این اعتبار دقیقاً تکراری است.");
        }
    }

    private static int InferFailingStep(ModelStateDictionary modelState)
    {
        // Only keys with actual validation errors should drive the step jump;
        // bound-but-valid keys (e.g. Tracks[0].Title) must be ignored.
        foreach (var key in modelState.Keys)
        {
            if (modelState[key]?.Errors.Count == 0)
            {
                continue;
            }
            if (key.StartsWith("Tracks", StringComparison.Ordinal)) return 3;
            if (key.StartsWith("AlbumCredits", StringComparison.Ordinal)) return 4;
            if (key.StartsWith("Links", StringComparison.Ordinal)
                || key.StartsWith("RelatedAlbums", StringComparison.Ordinal)
                || key.StartsWith("TagIds", StringComparison.Ordinal)) return 5;
            if (key.StartsWith("Companies", StringComparison.Ordinal)
                || key.StartsWith("Identifiers", StringComparison.Ordinal)) return 2;
        }
        return 1;
    }

    private static bool IsEmptyCreditRow(CreditWizardItem credit) =>
        credit.CreditRoleId == 0 && credit.RoleScopeTypeId == 0
        && credit.PersonId is null && credit.CompanyId is null && credit.InstrumentId is null
        && credit.DisplayOrder == 0 && !credit.IsPrimary && string.IsNullOrWhiteSpace(credit.Notes);

    private static bool IsEmptyCompanyRow(CompanyWizardItem company) =>
        company.CompanyId == 0 && company.CompanyRoleTypeId is null
        && string.IsNullOrWhiteSpace(company.CatalogNumber) && string.IsNullOrWhiteSpace(company.Barcode);

    private static bool IsEmptyLinkRow(LinkWizardItem link) =>
        string.IsNullOrWhiteSpace(link.Url) && link.LinkTypeId is null && string.IsNullOrWhiteSpace(link.Label);

    private static bool IsEmptyRelationRow(RelatedAlbumWizardItem relation) =>
        relation.AlbumId == 0 && relation.AlbumRelationTypeId == 0;

    private static bool IsEmptySungVersionRow(SungVersionWizardItem sv) =>
        sv.SungVersionId is null
        && string.IsNullOrWhiteSpace(sv.Title) && sv.PoemId is null
        && sv.VocalStyleId is null && !sv.IsCanonical
        && string.IsNullOrWhiteSpace(sv.Text) && string.IsNullOrWhiteSpace(sv.Notes);

    // ──────────────────────────────────────────────
    //  Dropdowns & helpers
    // ──────────────────────────────────────────────

    private async Task PopulateDropdownsAsync(
        AlbumWizardViewModel vm,
        CancellationToken ct)
    {
        vm.Categories = await _db.AlbumCategories.OrderBy(c => c.Name).ToListAsync(ct);
        vm.GenreOptions = await _db.Genres.Where(g => !g.IsDeleted).OrderBy(g => g.Name).ToListAsync(ct);
        vm.MoodOptions = await _db.Moods.OrderBy(m => m.Name).ToListAsync(ct);
        vm.LanguageOptions = await _db.Languages.OrderBy(l => l.Name).ToListAsync(ct);
        vm.CountryOptions = await _db.Countries.OrderBy(c => c.Name).ToListAsync(ct);
        vm.CompanyOptions = await _db.Companies.Where(c => !c.IsDeleted).OrderBy(c => c.Name).ToListAsync(ct);
        vm.CompanyRoleTypeOptions = await _db.CompanyRoleTypes.OrderBy(r => r.Name).ToListAsync(ct);
        vm.IdentifierTypeOptions = await _db.IdentifierTypes.OrderBy(i => i.Name).ToListAsync(ct);
        vm.InstrumentOptions = await _db.Instruments.Where(i => !i.IsDeleted).OrderBy(i => i.Name).ToListAsync(ct);
        vm.VocalStyleOptions = await _db.VocalStyles.OrderBy(v => v.Name).ToListAsync(ct);
        vm.MusicalKeyOptions = await _db.MusicalKeys.OrderBy(m => m.Name).ToListAsync(ct);
        vm.LyricsAvailabilityTypeOptions = await _db.LyricsAvailabilityTypes.OrderBy(l => l.Name).ToListAsync(ct);
        vm.TrackVersionTypeOptions = await _db.TrackVersionTypes.OrderBy(t => t.Name).ToListAsync(ct);
        vm.CreditRoleOptions = await _db.CreditRoles.OrderBy(r => r.Name).ToListAsync(ct);
        vm.RoleScopeTypeOptions = await _db.RoleScopeTypes.OrderBy(r => r.Name).ToListAsync(ct);
        vm.PersonOptions = await _db.People.Where(p => !p.IsDeleted).OrderBy(p => p.FullName).ToListAsync(ct);
        vm.PoemOptions = await _db.Poems.Where(p => !p.IsDeleted).OrderBy(p => p.Title).ToListAsync(ct);
        vm.SungVersionOptions = await _db.SungVersions.Where(s => !s.IsDeleted).OrderBy(s => s.Title).ToListAsync(ct);
        vm.TagOptions = await _db.Tags.Where(t => !t.IsDeleted).OrderBy(t => t.Name).ToListAsync(ct);
        vm.LinkTypeOptions = await _db.LinkTypes.OrderBy(l => l.Name).ToListAsync(ct);
        vm.AlbumOptions = await _db.Albums.Where(a => !a.IsDeleted).OrderBy(a => a.Title).ToListAsync(ct);
        vm.AlbumRelationTypeOptions = await _db.AlbumRelationTypes.OrderBy(r => r.Name).ToListAsync(ct);
        vm.MediaOptions = await _db.Media.Where(m => !m.IsDeleted).OrderBy(m => m.FileName).ToListAsync(ct);
    }

    /// <summary>
    /// Resolves numeric entity type ids from the EntityType table by code.
    /// </summary>
    private async Task<Dictionary<string, int>> ResolveEntityTypeIdsAsync(CancellationToken ct)
    {
        var codes = new[]
        {
            EntityTypeConstants.Album,
            EntityTypeConstants.Track,
            EntityTypeConstants.SungVersion,
            EntityTypeConstants.Person,
            EntityTypeConstants.Company,
            EntityTypeConstants.Genre,
            EntityTypeConstants.Mood,
            EntityTypeConstants.Instrument,
            EntityTypeConstants.Poem,
            EntityTypeConstants.Tag
        };
        var types = await _db.EntityTypes
            .Where(e => codes.Contains(e.Code))
            .ToListAsync(ct);

        var result = types.ToDictionary(t => t.Code, t => t.EntityTypeId);
        foreach (var code in codes)
        {
            if (!result.ContainsKey(code))
            {
                throw new InvalidOperationException($"EntityType '{code}' is missing from the lookup table.");
            }
        }
        return result;
    }

    /// <summary>
    /// Ensures a slug is unique within the given set, appending -2, -3, … on
    /// collision (mirrors the AlbumTracklistController create-inline behavior).
    /// </summary>
    private static async Task<string> EnsureUniqueSlugAsync(
        IQueryable<string> existingSlugs,
        string baseSlug,
        CancellationToken ct)
    {
        var slug = baseSlug;
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = $"item-{Guid.NewGuid():N}"[..20];
        }
        var counter = 1;
        while (await existingSlugs.AnyAsync(s => s == slug, ct))
        {
            slug = $"{baseSlug}-{counter++}";
        }
        return slug;
    }
}
