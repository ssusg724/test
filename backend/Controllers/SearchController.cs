using LiveLogApi.Data;
using LiveLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _db;
    public SearchController(AppDbContext db) => _db = db;

    /// <summary>ライブ・バンド・会場・曲を横断検索。</summary>
    [HttpGet]
    public async Task<IEnumerable<SearchHit>> Get(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Enumerable.Empty<SearchHit>();
        var ql = q.ToLower();
        var hits = new List<SearchHit>();

        hits.AddRange(await _db.Lives.Include(l => l.Artist).Include(l => l.Venue)
            .Where(l => l.Title.ToLower().Contains(ql))
            .OrderByDescending(l => l.Date).Take(8)
            .Select(l => new SearchHit("live", l.Id, l.Title, $"{l.Artist!.Name} ・ {l.Date}"))
            .ToListAsync());

        hits.AddRange(await _db.Artists.Where(a => a.Name.ToLower().Contains(ql)).Take(8)
            .Select(a => new SearchHit("artist", a.Id, a.Name, a.Genre ?? "バンド")).ToListAsync());

        hits.AddRange(await _db.Venues.Where(v => v.Name.ToLower().Contains(ql)).Take(8)
            .Select(v => new SearchHit("venue", v.Id, v.Name, v.City ?? "会場")).ToListAsync());

        hits.AddRange(await _db.Songs.Include(s => s.Artist)
            .Where(s => s.Title.ToLower().Contains(ql) || s.Aliases.Any(a => a.Alias.ToLower().Contains(ql)))
            .Take(8)
            .Select(s => new SearchHit("song", s.Id, s.Title, s.Artist!.Name)).ToListAsync());

        return hits;
    }
}
