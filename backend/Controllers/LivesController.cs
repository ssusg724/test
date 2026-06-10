using LiveLogApi.Data;
using LiveLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LivesController : ControllerBase
{
    private readonly AppDbContext _db;
    public LivesController(AppDbContext db) => _db = db;

    private static LiveDto ToDto(Live l) => new(
        l.Id, l.Title, l.Date, l.ArtistId, l.Artist?.Name ?? "",
        l.VenueId, l.Venue?.Name ?? "", l.Status.ToString(), l.Notes,
        l.Setlist.OrderBy(e => e.Order)
            .Select(e => new SetlistItemDto(e.SongId, e.Song?.Title ?? "", e.Order, e.IsEncore))
            .ToList()
    );

    private IQueryable<Live> Query() => _db.Lives
        .Include(l => l.Artist)
        .Include(l => l.Venue)
        .Include(l => l.Setlist).ThenInclude(e => e.Song);

    /// <summary>ライブ一覧。?status=Attended|Applied|Interested、?venueId=、?artistId= で絞り込み。</summary>
    [HttpGet]
    public async Task<IEnumerable<LiveDto>> GetAll(LiveStatus? status, int? venueId, int? artistId)
    {
        var q = Query();
        if (status is not null) q = q.Where(l => l.Status == status);
        if (venueId is not null) q = q.Where(l => l.VenueId == venueId);
        if (artistId is not null) q = q.Where(l => l.ArtistId == artistId);
        var list = await q.OrderByDescending(l => l.Date).ToListAsync();
        return list.Select(ToDto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LiveDto>> Get(int id)
    {
        var live = await Query().FirstOrDefaultAsync(l => l.Id == id);
        return live is null ? NotFound() : ToDto(live);
    }

    /// <summary>FK(バンド/会場/曲)の存在チェック。問題があればエラーメッセージを返す。</summary>
    private async Task<string?> ValidateRefs(LiveInput input)
    {
        if (!await _db.Artists.AnyAsync(a => a.Id == input.ArtistId))
            return "指定されたバンドが存在しません";
        if (!await _db.Venues.AnyAsync(v => v.Id == input.VenueId))
            return "指定された会場が存在しません";
        var songIds = (input.Setlist ?? new()).Select(s => s.SongId).Distinct().ToList();
        if (songIds.Count > 0)
        {
            var existing = await _db.Songs.Where(s => songIds.Contains(s.Id)).CountAsync();
            if (existing != songIds.Count) return "セトリに存在しない曲が含まれています";
        }
        return null;
    }

    [HttpPost]
    public async Task<ActionResult<LiveDto>> Create(LiveInput input)
    {
        if (await ValidateRefs(input) is { } err) return BadRequest(new { error = err });

        var live = new Live
        {
            Title = input.Title,
            Date = input.Date,
            ArtistId = input.ArtistId,
            VenueId = input.VenueId,
            Status = input.Status,
            Notes = input.Notes,
            Setlist = (input.Setlist ?? new()).Select(s => new SetlistEntry
            {
                SongId = s.SongId, Order = s.Order, IsEncore = s.IsEncore
            }).ToList()
        };
        _db.Lives.Add(live);
        await _db.SaveChangesAsync();
        var saved = await Query().FirstAsync(l => l.Id == live.Id);
        return CreatedAtAction(nameof(Get), new { id = live.Id }, ToDto(saved));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LiveDto>> Update(int id, LiveInput input)
    {
        var live = await _db.Lives.Include(l => l.Setlist).FirstOrDefaultAsync(l => l.Id == id);
        if (live is null) return NotFound();
        if (await ValidateRefs(input) is { } err) return BadRequest(new { error = err });

        live.Title = input.Title;
        live.Date = input.Date;
        live.ArtistId = input.ArtistId;
        live.VenueId = input.VenueId;
        live.Status = input.Status;
        live.Notes = input.Notes;

        _db.SetlistEntries.RemoveRange(live.Setlist);
        live.Setlist = (input.Setlist ?? new()).Select(s => new SetlistEntry
        {
            SongId = s.SongId, Order = s.Order, IsEncore = s.IsEncore
        }).ToList();

        await _db.SaveChangesAsync();
        var saved = await Query().FirstAsync(l => l.Id == id);
        return ToDto(saved);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var live = await _db.Lives.FindAsync(id);
        if (live is null) return NotFound();
        _db.Lives.Remove(live);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
