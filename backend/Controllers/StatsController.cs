using LiveLogApi.Data;
using LiveLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatsController(AppDbContext db) => _db = db;

    /// <summary>参戦サマリー：通算/今年の参戦数、年別、よく行く会場・バンド、生で聴いた曲数。</summary>
    [HttpGet("summary")]
    public async Task<StatsDto> Summary()
    {
        var attended = await _db.Lives
            .Where(l => l.Status == LiveStatus.Attended)
            .Include(l => l.Artist).Include(l => l.Venue)
            .ToListAsync();

        var thisYear = DateOnly.FromDateTime(DateTime.Today).Year;

        var byYear = attended
            .GroupBy(l => l.Date.Year)
            .OrderByDescending(g => g.Key)
            .Select(g => new YearCount(g.Key, g.Count()))
            .ToList();

        var topVenues = attended
            .GroupBy(l => l.Venue!.Name)
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
            .Take(5)
            .Select(g => new NameCount(g.Key, g.Count()))
            .ToList();

        var topArtists = attended
            .GroupBy(l => l.Artist!.Name)
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
            .Take(5)
            .Select(g => new NameCount(g.Key, g.Count()))
            .ToList();

        var attendedIds = attended.Select(l => l.Id).ToHashSet();
        var entries = await _db.SetlistEntries
            .Where(e => attendedIds.Contains(e.LiveId))
            .ToListAsync();

        return new StatsDto(
            TotalAttended: attended.Count,
            ThisYearAttended: attended.Count(l => l.Date.Year == thisYear),
            UpcomingCount: await _db.Lives.CountAsync(l =>
                l.Status == LiveStatus.Applied || l.Status == LiveStatus.Interested),
            TotalSongsPlayed: entries.Count,
            UniqueSongsHeard: entries.Select(e => e.SongId).Distinct().Count(),
            ByYear: byYear,
            TopVenues: topVenues,
            TopArtists: topArtists
        );
    }
}
