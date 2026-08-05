using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin dashboard controller (spec 10.4).
/// Route: /admin
/// </summary>
[Route("/admin")]
public sealed class DashboardController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly IDbConnection _connection;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AppDbContext db,
        IDbConnection connection,
        ILogger<DashboardController> logger)
    {
        _db = db;
        _connection = connection;
        _logger = logger;
    }

    /// <summary>
    /// Admin dashboard — shows aggregated counts and recent additions.
    /// Uses Dapper for fast aggregate queries against the database.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Loading admin dashboard");

        var viewModel = new AdminDashboardViewModel
        {
            AlbumCount = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Album WHERE IsDeleted = 0"),
            TrackCount = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Track WHERE IsDeleted = 0"),
            PersonCount = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Person WHERE IsDeleted = 0"),
            CompanyCount = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Company WHERE IsDeleted = 0"),
            PoemCount = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Poem WHERE IsDeleted = 0"),
            RecentAlbums = (await _connection.QueryAsync<Core.DTOs.AlbumListItemDto>(
                @"SELECT TOP 10 a.AlbumId, a.Slug, a.Title, a.OriginalTitle, a.EnglishTitle,
                         ac.Name AS CategoryName, a.ReleaseDate, a.DurationSeconds, NULL AS CoverUrl
                  FROM Album a
                  LEFT JOIN AlbumCategory ac ON a.AlbumCategoryId = ac.AlbumCategoryId
                  WHERE a.IsDeleted = 0
                  ORDER BY a.CreatedAt DESC"))
                .ToList().AsReadOnly(),
            RecentTracks = (await _connection.QueryAsync<TrackListItemDto>(
                @"SELECT TOP 10 t.TrackId, t.Title, t.Slug,
                         a.Title AS AlbumTitle, t.DurationSeconds
                  FROM Track t
                  LEFT JOIN AlbumTrack at ON t.TrackId = at.TrackId
                  LEFT JOIN Album a ON at.AlbumId = a.AlbumId
                  WHERE t.IsDeleted = 0
                  ORDER BY t.CreatedAt DESC"))
                .ToList().AsReadOnly()
        };

        ViewData["Title"] = "Dashboard";
        ViewData["ActiveMenu"] = "Dashboard";

        return View(viewModel);
    }
}
