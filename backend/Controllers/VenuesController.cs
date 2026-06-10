using LiveLogApi.Data;
using LiveLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController : ControllerBase
{
    private readonly AppDbContext _db;
    public VenuesController(AppDbContext db) => _db = db;

    private static VenueDto ToDto(Venue v) => new(
        v.Id, v.Name, v.City, v.Capacity, v.DrinkFee, v.AcceptsEMoney, v.OfficialX, v.Lives.Count);

    /// <summary>会場一覧（ライブ数つき＝会場ごとのまとめ用）</summary>
    [HttpGet]
    public async Task<IEnumerable<VenueDto>> GetAll() =>
        (await _db.Venues.Include(v => v.Lives).OrderBy(v => v.Name).ToListAsync()).Select(ToDto);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VenueDto>> Get(int id)
    {
        var v = await _db.Venues.Include(x => x.Lives).FirstOrDefaultAsync(x => x.Id == id);
        return v is null ? NotFound() : ToDto(v);
    }

    [HttpPost]
    public async Task<ActionResult<VenueDto>> Create(VenueInput input)
    {
        var v = new Venue
        {
            Name = input.Name, City = input.City, Capacity = input.Capacity,
            DrinkFee = input.DrinkFee, AcceptsEMoney = input.AcceptsEMoney, OfficialX = input.OfficialX
        };
        _db.Venues.Add(v);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = v.Id }, ToDto(v));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VenueDto>> Update(int id, VenueInput input)
    {
        var v = await _db.Venues.Include(x => x.Lives).FirstOrDefaultAsync(x => x.Id == id);
        if (v is null) return NotFound();
        v.Name = input.Name; v.City = input.City; v.Capacity = input.Capacity;
        v.DrinkFee = input.DrinkFee; v.AcceptsEMoney = input.AcceptsEMoney; v.OfficialX = input.OfficialX;
        await _db.SaveChangesAsync();
        return ToDto(v);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _db.Venues.FindAsync(id);
        if (v is null) return NotFound();
        if (await _db.Lives.AnyAsync(l => l.VenueId == id))
            return Conflict(new { error = "この会場のライブ記録があるため削除できません" });
        _db.Venues.Remove(v);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
