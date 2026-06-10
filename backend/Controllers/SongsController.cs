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

    /// <summary>曲一覧。?q= でタイトル/別表記の部分一致検索、?artistId= で絞り込み。</summary>
    [HttpGet]
    public async Task<IEnumerable<object>> Search(string? q, int? artistId)
    {
        var query = _db.Songs.Include(s => s.Artist).Include(s => s.Aliases).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.ToLower();
            query = query.Where(s => s.Title.ToLower().Contains(ql)
                || s.Aliases.Any(a => a.Alias.ToLower().Contains(ql)));
        }
        if (artistId is not null)
            query = query.Where(s => s.ArtistId == artistId);

        return await query.OrderBy(s => s.Title)
            .Select(s => new
            {
                s.Id, s.Title, s.ArtistId, ArtistName = s.Artist!.Name,
                Aliases = s.Aliases.Select(a => a.Alias).ToList(),
                PlayCount = s.SetlistEntries.Count
            })
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

    /// <summary>曲に別表記(エイリアス)を追加。名寄せ用。</summary>
    [HttpPost("{id:int}/aliases")]
    public async Task<IActionResult> AddAlias(int id, AliasInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Alias))
            return BadRequest(new { error = "別表記が空です" });
        if (!await _db.Songs.AnyAsync(s => s.Id == id)) return NotFound();
        _db.SongAliases.Add(new SongAlias { SongId = id, Alias = input.Alias.Trim() });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>名寄せ：曲 id を target に統合する。
    /// id のセトリ出演を全て target に付け替え、id のタイトルを target の別表記に追加して id を削除。</summary>
    [HttpPost("{id:int}/merge")]
    public async Task<IActionResult> Merge(int id, SongMergeInput input)
    {
        if (id == input.TargetId) return BadRequest(new { error = "同じ曲には統合できません" });
        var source = await _db.Songs.Include(s => s.Aliases).FirstOrDefaultAsync(s => s.Id == id);
        var target = await _db.Songs.Include(s => s.Aliases).FirstOrDefaultAsync(s => s.Id == input.TargetId);
        if (source is null || target is null) return NotFound();

        // セトリの付け替え
        var entries = await _db.SetlistEntries.Where(e => e.SongId == id).ToListAsync();
        foreach (var e in entries) e.SongId = target.Id;

        // 別表記の引き継ぎ（重複は除く）
        var existing = target.Aliases.Select(a => a.Alias).Append(target.Title).ToHashSet();
        foreach (var name in source.Aliases.Select(a => a.Alias).Append(source.Title))
            if (existing.Add(name))
                _db.SongAliases.Add(new SongAlias { SongId = target.Id, Alias = name });

        _db.SongAliases.RemoveRange(source.Aliases); // sourceのエイリアスは削除（cascadeでも消えるが明示）
        _db.Songs.Remove(source);
        await _db.SaveChangesAsync();
        return Ok(new { merged = id, into = target.Id });
    }
}
