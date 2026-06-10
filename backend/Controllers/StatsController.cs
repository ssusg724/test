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

        var byMonth = attended
            .Where(l => l.Date.Year == thisYear)
            .GroupBy(l => l.Date.Month)
            .ToDictionary(g => g.Key, g => g.Count());
        var byMonthThisYear = Enumerable.Range(1, 12)
            .Select(m => new MonthCount(m, byMonth.GetValueOrDefault(m, 0)))
            .ToList();

        var ratings = attended.Where(l => l.Rating is > 0).Select(l => l.Rating!.Value).ToList();
        double? avgRating = ratings.Count > 0 ? Math.Round(ratings.Average(), 2) : null;

        // ドリンク代はチケットとは別に会場側にあるので、参戦したライブの会場ドリンク代も加算
        var totalSpent = attended.Sum(l => (l.TicketPrice ?? 0) + (l.Venue!.DrinkFee ?? 0));

        var attendedIds = attended.Select(l => l.Id).ToHashSet();
        var entries = await _db.SetlistEntries
            .Where(e => attendedIds.Contains(e.LiveId))
            .Include(e => e.Song)
            .ToListAsync();

        var topSongs = entries
            .GroupBy(e => e.Song!.Title)
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
            .Take(5)
            .Select(g => new NameCount(g.Key, g.Count()))
            .ToList();

        return new StatsDto(
            TotalAttended: attended.Count,
            ThisYearAttended: attended.Count(l => l.Date.Year == thisYear),
            UpcomingCount: await _db.Lives.CountAsync(l =>
                l.Status == LiveStatus.Applied || l.Status == LiveStatus.Interested),
            TotalSongsPlayed: entries.Count,
            UniqueSongsHeard: entries.Select(e => e.SongId).Distinct().Count(),
            TotalSpent: totalSpent,
            AverageRating: avgRating,
            FavoriteSong: topSongs.FirstOrDefault()?.Name,
            FavoriteSongCount: topSongs.FirstOrDefault()?.Count ?? 0,
            ByYear: byYear,
            ByMonthThisYear: byMonthThisYear,
            TopVenues: topVenues,
            TopArtists: topArtists,
            TopSongs: topSongs
        );
    }
}
