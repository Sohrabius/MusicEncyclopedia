using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Services.Services;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Album tracklist editor (spec 10.5 "Tracklist" tab).
/// AJAX editor for AlbumTrack rows: add existing track, create inline, set disc/track/
/// sequence numbers, title/duration overrides, bonus/hidden flags, reorder, remove.
/// Route: /admin/album-tracklist
/// </summary>
[Route("/admin/album-tracklist")]
[Authorize(Policy = PermissionConstants.CanManageAlbums)]
public sealed class AlbumTracklistController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<AlbumTracklistController> _logger;

    public AlbumTracklistController(AppDbContext db, ILogger<AlbumTracklistController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET: Returns the tracklist editor partial for an album.
    /// GET /admin/album-tracklist/{albumId}
    /// </summary>
    [HttpGet]
    [Route("{albumId:int}")]
    public async Task<IActionResult> Index(
        int albumId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.AlbumTracks
            .Where(at => at.AlbumId == albumId)
            .Include(at => at.Track)
            .OrderBy(at => at.DiscNumber)
            .ThenBy(at => at.SequenceNumber)
            .ThenBy(at => at.TrackNumber)
            .ToListAsync(cancellationToken);

        // Tracks already on this album, plus any soft-deleted tracks
        var assignedTrackIds = rows.Select(r => r.TrackId).ToHashSet();
        var available = await _db.Tracks
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Title)
            .Select(t => new AlbumTrackOption { TrackId = t.TrackId, Title = t.Title })
            .ToListAsync(cancellationToken);

        var vm = new AlbumTracklistViewModel
        {
            AlbumId = albumId,
            Rows = rows.Select(r => new AlbumTrackRow
            {
                AlbumTrackId = r.AlbumTrackId,
                TrackId = r.TrackId,
                TrackTitle = r.Track?.Title ?? $"Track {r.TrackId}",
                DiscNumber = r.DiscNumber,
                TrackNumber = r.TrackNumber,
                SequenceNumber = r.SequenceNumber,
                TitleOverride = r.TrackTitleOverride,
                DurationOverrideSeconds = r.DurationSecondsOverride,
                IsBonus = r.IsBonus,
                IsHidden = r.IsHidden
            }).ToList(),
            AvailableTracks = available.Where(t => !assignedTrackIds.Contains(t.TrackId)).ToList()
        };

        return PartialView("_AlbumTracklistEditor", vm);
    }

    /// <summary>
    /// POST: Adds an existing track to the album (with disc/track/sequence numbers).
    /// </summary>
    [HttpPost]
    [Route("add-existing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExisting(
        CancellationToken cancellationToken = default)
    {
        var albumId = int.TryParse(Request.Form["albumId"], out var aId) ? aId : 0;
        var trackId = int.TryParse(Request.Form["trackId"], out var tId) ? tId : 0;
        var disc = int.TryParse(Request.Form["discNumber"], out var d) ? d : 1;
        var trackNo = int.TryParse(Request.Form["trackNumber"], out var tn) ? tn : 0;
        var isBonus = Request.Form["isBonus"].ToString() == "true";

        if (albumId == 0 || trackId == 0)
        {
            return Json(new { success = false, errors = new[] { "Album and track are required." } });
        }

        var exists = await _db.AlbumTracks.AnyAsync(at => at.AlbumId == albumId && at.TrackId == trackId, cancellationToken);
        if (exists)
        {
            return Json(new { success = false, errors = new[] { "This track is already on the album." } });
        }

        var sequence = await NextSequenceAsync(albumId, disc, cancellationToken);

        _db.AlbumTracks.Add(new AlbumTrack
        {
            AlbumId = albumId,
            TrackId = trackId,
            DiscNumber = Math.Max(1, disc),
            TrackNumber = trackNo > 0 ? trackNo : sequence,
            SequenceNumber = sequence,
            IsBonus = isBonus
        });

        await _db.SaveChangesAsync(cancellationToken);
        await InvalidateEntityCacheAsync(EntityTypeConstants.Album, albumId, "Updated");
        await InvalidateBroadCacheAsync();
        return Json(new { success = true });
    }

    /// <summary>
    /// POST: Creates a brand new track inline and adds it to the album.
    /// </summary>
    [HttpPost]
    [Route("create-inline")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInline(
        CancellationToken cancellationToken = default)
    {
        var albumId = int.TryParse(Request.Form["albumId"], out var aId) ? aId : 0;
        var title = Request.Form["title"].ToString().Trim();
        var disc = int.TryParse(Request.Form["discNumber"], out var d) ? d : 1;
        var trackNo = int.TryParse(Request.Form["trackNumber"], out var tn) ? tn : 0;
        var duration = int.TryParse(Request.Form["durationSeconds"], out var dur) ? dur : (int?)null;
        var isBonus = Request.Form["isBonus"].ToString() == "true";

        if (albumId == 0 || string.IsNullOrWhiteSpace(title))
        {
            return Json(new { success = false, errors = new[] { "Album and track title are required." } });
        }

        var slugService = new SlugService();
        var slug = slugService.GenerateSlug(title);
        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = $"track-{Guid.NewGuid():N}"[..20];
        }

        // Ensure unique slug
        var baseSlug = slug;
        var counter = 1;
        while (await _db.Tracks.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter++}";
        }

        var entity = new Entity
        {
            EntityTypeId = 2, // Track
            Slug = slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var track = new Track
        {
            EntityId = entity.EntityId,
            Title = title,
            Slug = slug,
            DurationSeconds = duration,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Tracks.Add(track);
        await _db.SaveChangesAsync(cancellationToken);

        var sequence = await NextSequenceAsync(albumId, disc, cancellationToken);

        _db.AlbumTracks.Add(new AlbumTrack
        {
            AlbumId = albumId,
            TrackId = track.TrackId,
            DiscNumber = Math.Max(1, disc),
            TrackNumber = trackNo > 0 ? trackNo : sequence,
            SequenceNumber = sequence,
            IsBonus = isBonus
        });

        await _db.SaveChangesAsync(cancellationToken);
        await InvalidateEntityCacheAsync(EntityTypeConstants.Album, albumId, "Updated");
        await InvalidateEntityCacheAsync(EntityTypeConstants.Track, track.TrackId, "Created");
        await InvalidateBroadCacheAsync();
        return Json(new { success = true, trackId = track.TrackId });
    }

    /// <summary>
    /// POST: Updates a tracklist row (disc/track/sequence, overrides, flags).
    /// </summary>
    [HttpPost]
    [Route("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        CancellationToken cancellationToken = default)
    {
        var id = int.TryParse(Request.Form["albumTrackId"], out var i) ? i : 0;
        var disc = int.TryParse(Request.Form["discNumber"], out var d) ? d : 1;
        var trackNo = int.TryParse(Request.Form["trackNumber"], out var tn) ? tn : 0;
        var duration = int.TryParse(Request.Form["durationOverrideSeconds"], out var dur) ? dur : (int?)null;
        var titleOverride = Request.Form["titleOverride"].ToString().Trim();

        var row = await _db.AlbumTracks.FirstOrDefaultAsync(at => at.AlbumTrackId == id, cancellationToken);
        if (row is null)
        {
            return Json(new { success = false, errors = new[] { "Tracklist row not found." } });
        }

        var oldDisc = row.DiscNumber;
        row.DiscNumber = Math.Max(1, disc);
        row.TrackNumber = trackNo;
        row.DurationSecondsOverride = duration;
        row.TrackTitleOverride = string.IsNullOrWhiteSpace(titleOverride) ? null : titleOverride;
        row.IsBonus = Request.Form["isBonus"].ToString() == "true";
        row.IsHidden = Request.Form["isHidden"].ToString() == "true";

        await _db.SaveChangesAsync(cancellationToken);

        // Renumber sequences by position within the disc so they stay 1..N and gap-free
        await RenumberSequenceAsync(row.AlbumId, row.DiscNumber, cancellationToken);
        if (oldDisc != row.DiscNumber)
        {
            await RenumberSequenceAsync(row.AlbumId, oldDisc, cancellationToken);
        }

        await InvalidateEntityCacheAsync(EntityTypeConstants.Album, row.AlbumId, "Updated");
        await InvalidateBroadCacheAsync();
        return Json(new { success = true });
    }

    /// <summary>
    /// POST: Removes a track from the album tracklist.
    /// </summary>
    [HttpPost]
    [Route("remove/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Remove(
        int id,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.AlbumTracks.FirstOrDefaultAsync(at => at.AlbumTrackId == id, cancellationToken);
        if (row is null)
        {
            return Json(new { success = false, errors = new[] { "Tracklist row not found." } });
        }

        var albumId = row.AlbumId;
        _db.AlbumTracks.Remove(row);
        await _db.SaveChangesAsync(cancellationToken);
        await InvalidateEntityCacheAsync(EntityTypeConstants.Album, albumId, "Updated");
        await InvalidateBroadCacheAsync();
        return Json(new { success = true });
    }

    /// <summary>
    /// POST: Moves a row up or down (reorder). Direction: up|down.
    /// </summary>
    [HttpPost]
    [Route("move/{id:int}/{direction}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Move(
        int id,
        string direction,
        CancellationToken cancellationToken = default)
    {
        var row = await _db.AlbumTracks.FirstOrDefaultAsync(at => at.AlbumTrackId == id, cancellationToken);
        if (row is null)
        {
            return Json(new { success = false, errors = new[] { "Tracklist row not found." } });
        }

        var siblings = await _db.AlbumTracks
            .Where(at => at.AlbumId == row.AlbumId && at.DiscNumber == row.DiscNumber)
            .OrderBy(at => at.SequenceNumber)
            .ThenBy(at => at.TrackNumber)
            .ToListAsync(cancellationToken);

        var index = siblings.FindIndex(at => at.AlbumTrackId == id);
        var swapIndex = direction == "up" ? index - 1 : index + 1;
        if (index < 0 || swapIndex < 0 || swapIndex >= siblings.Count)
        {
            return Json(new { success = false, errors = new[] { "Cannot move further." } });
        }

        (siblings[index].SequenceNumber, siblings[swapIndex].SequenceNumber) =
            (siblings[swapIndex].SequenceNumber, siblings[index].SequenceNumber);

        await _db.SaveChangesAsync(cancellationToken);

        // Renumber to remove any gaps
        await RenumberSequenceAsync(row.AlbumId, row.DiscNumber, cancellationToken);

        await InvalidateEntityCacheAsync(EntityTypeConstants.Album, row.AlbumId, "Updated");
        await InvalidateBroadCacheAsync();
        return Json(new { success = true });
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    private async Task<int> NextSequenceAsync(int albumId, int disc, CancellationToken ct)
    {
        var maxSeq = await _db.AlbumTracks
            .Where(at => at.AlbumId == albumId && at.DiscNumber == disc)
            .Select(at => (int?)at.SequenceNumber)
            .MaxAsync(ct);
        return (maxSeq ?? 0) + 1;
    }

    /// <summary>
    /// Recomputes 1..N sequence numbers for all rows on a given album/disc,
    /// preserving their current relative order.
    /// </summary>
    private async Task RenumberSequenceAsync(int albumId, int disc, CancellationToken ct)
    {
        var rows = await _db.AlbumTracks
            .Where(at => at.AlbumId == albumId && at.DiscNumber == disc)
            .OrderBy(at => at.SequenceNumber)
            .ThenBy(at => at.TrackNumber)
            .ToListAsync(ct);

        var seq = 1;
        foreach (var row in rows)
        {
            row.SequenceNumber = seq++;
        }

        await _db.SaveChangesAsync(ct);
    }
}
