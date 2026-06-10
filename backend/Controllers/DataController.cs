using LiveLogApi.Data;
using LiveLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly AppDbContext _db;
    public DataController(AppDbContext db) => _db = db;

    /// <summary>全データをJSONでエクスポート（バックアップ用）。</summary>
    [HttpGet("export")]
    public async Task<ExportBundle> Export()
    {
        return new ExportBundle(
            await _db.Artists.AsNoTracking().ToListAsync(),
            await _db.Venues.AsNoTracking().ToListAsync(),
            await _db.Songs.Include(s => s.Aliases).AsNoTracking().ToListAsync(),
            await _db.Lives.Include(l => l.Setlist).AsNoTracking().ToListAsync()
        );
    }

    /// <summary>エクスポートしたJSONを取り込む。既存データは全削除してから置き換える。
    /// IDはDBが再採番するため、旧IDで参照を辿りナビゲーションプロパティで関連を張り直す。</summary>
    [HttpPost("import")]
    public async Task<IActionResult> Import(ExportBundle? bundle)
    {
        if (bundle?.Artists is null || bundle.Venues is null || bundle.Songs is null || bundle.Lives is null)
            return BadRequest(new { error = "不正なバックアップ形式です（artists/venues/songs/lives が必要）" });

        // 全工程を1つのトランザクションで実行。途中で失敗したら削除ごとロールバックしてデータ消失を防ぐ。
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // 既存を全削除（依存順）
            _db.SetlistEntries.RemoveRange(_db.SetlistEntries);
            _db.SongAliases.RemoveRange(_db.SongAliases);
            _db.Lives.RemoveRange(_db.Lives);
            _db.Songs.RemoveRange(_db.Songs);
            _db.Venues.RemoveRange(_db.Venues);
            _db.Artists.RemoveRange(_db.Artists);
            await _db.SaveChangesAsync();

            // 旧ID→新エンティティのマップを作り、FKではなくナビゲーションで関連付け（DBが再採番しても壊れない）
            var artistMap = new Dictionary<int, Artist>();
            foreach (var a in bundle.Artists)
            {
                var na = new Artist { Name = a.Name, Genre = a.Genre, Notes = a.Notes, OfficialX = a.OfficialX, Website = a.Website };
                artistMap[a.Id] = na; _db.Artists.Add(na);
            }
            var venueMap = new Dictionary<int, Venue>();
            foreach (var v in bundle.Venues)
            {
                var nv = new Venue { Name = v.Name, City = v.City, Capacity = v.Capacity, DrinkFee = v.DrinkFee, AcceptsEMoney = v.AcceptsEMoney, OfficialX = v.OfficialX };
                venueMap[v.Id] = nv; _db.Venues.Add(nv);
            }
            var songMap = new Dictionary<int, Song>();
            foreach (var s in bundle.Songs)
            {
                if (!artistMap.TryGetValue(s.ArtistId, out var owner)) continue;
                var ns = new Song
                {
                    Title = s.Title, Artist = owner,
                    Aliases = (s.Aliases ?? new()).Select(al => new SongAlias { Alias = al.Alias }).ToList()
                };
                songMap[s.Id] = ns; _db.Songs.Add(ns);
            }
            foreach (var l in bundle.Lives)
            {
                if (!artistMap.TryGetValue(l.ArtistId, out var art)) continue;
                if (!venueMap.TryGetValue(l.VenueId, out var ven)) continue;
                var nl = new Live
                {
                    Title = l.Title, Date = l.Date, Artist = art, Venue = ven, Status = l.Status,
                    Notes = l.Notes, Rating = l.Rating, TicketPrice = l.TicketPrice, Seat = l.Seat, Companions = l.Companions,
                    Setlist = (l.Setlist ?? new())
                        .Where(e => songMap.ContainsKey(e.SongId))
                        .Select(e => new SetlistEntry { Song = songMap[e.SongId], Order = e.Order, IsEncore = e.IsEncore })
                        .ToList()
                };
                _db.Lives.Add(nl);
            }
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                artists = artistMap.Count, venues = venueMap.Count,
                songs = songMap.Count, lives = bundle.Lives.Count
            });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(new { error = $"インポートに失敗しました（変更は取り消されました）: {ex.Message}" });
        }
    }
}
