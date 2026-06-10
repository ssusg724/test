using LiveLogApi.Data;
using LiveLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    private readonly AppDbContext _db;
    public SongsController(AppDbContext db) => _db = db;

    /// <summary>曲一覧。?q= でタイトル部分一致検索、?artistId= で絞り込み。</summary>
    [HttpGet]
    public async Task<IEnumerable<object>> Search(string? q, int? artistId)
    {
        var query = _db.Songs.Include(s => s.Artist).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.Title.ToLower().Contains(q.ToLower()));
        if (artistId is not null)
            query = query.Where(s => s.ArtistId == artistId);

        return await query.OrderBy(s => s.Title)
            .Select(s => new { s.Id, s.Title, s.ArtistId, ArtistName = s.Artist!.Name })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create(SongInput input)
    {
        if (!await _db.Artists.AnyAsync(a => a.Id == input.ArtistId))
            return BadRequest(new { error = "指定されたバンドが存在しません" });
        var s = new Song { Title = input.Title, ArtistId = input.ArtistId };
        _db.Songs.Add(s);
        await _db.SaveChangesAsync();
        return Created($"/api/songs/{s.Id}", new { s.Id, s.Title, s.ArtistId });
    }

    /// <summary>曲ごとのまとめ：その曲がいつ・どの会場で演奏されたかの履歴。
    /// ?from=2024-01-01&amp;to=2024-12-31 で「いつからいつまでに何回」を集計。</summary>
    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<SongSummaryDto>> History(int id, DateOnly? from, DateOnly? to)
    {
        var song = await _db.Songs.Include(s => s.Artist).FirstOrDefaultAsync(s => s.Id == id);
        if (song is null) return NotFound();

        var q = _db.SetlistEntries.Where(e => e.SongId == id);
        if (from is not null) q = q.Where(e => e.Live!.Date >= from);
        if (to is not null) q = q.Where(e => e.Live!.Date <= to);

        var plays = await q
            .Include(e => e.Live).ThenInclude(l => l!.Venue)
            .OrderByDescending(e => e.Live!.Date)
            .Select(e => new SongPlayDto(
                e.LiveId, e.Live!.Title, e.Live.Date, e.Live.Venue!.Name, e.Order, e.IsEncore))
            .ToListAsync();

        return new SongSummaryDto(song.Id, song.Title, song.Artist?.Name ?? "", plays.Count, plays);
    }
}
