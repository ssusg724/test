using LiveLogApi.Data;
using LiveLogApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ArtistsController(AppDbContext db) => _db = db;

    private static ArtistDto ToDto(Artist a) => new(a.Id, a.Name, a.Genre, a.Notes, a.OfficialX, a.Website);

    [HttpGet]
    public async Task<IEnumerable<ArtistDto>> GetAll() =>
        (await _db.Artists.OrderBy(a => a.Name).ToListAsync()).Select(ToDto);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ArtistDto>> Get(int id)
    {
        var a = await _db.Artists.FindAsync(id);
        return a is null ? NotFound() : ToDto(a);
    }

    [HttpPost]
    public async Task<ActionResult<ArtistDto>> Create(ArtistInput input)
    {
        var a = new Artist
        {
            Name = input.Name, Genre = input.Genre, Notes = input.Notes,
            OfficialX = input.OfficialX, Website = input.Website
        };
        _db.Artists.Add(a);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = a.Id }, ToDto(a));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ArtistDto>> Update(int id, ArtistInput input)
    {
        var a = await _db.Artists.FindAsync(id);
        if (a is null) return NotFound();
        a.Name = input.Name; a.Genre = input.Genre; a.Notes = input.Notes;
        a.OfficialX = input.OfficialX; a.Website = input.Website;
        await _db.SaveChangesAsync();
        return ToDto(a);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _db.Artists.FindAsync(id);
        if (a is null) return NotFound();
        _db.Artists.Remove(a);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
