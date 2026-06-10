using LiveLogApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LiveLogApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Live> Lives => Set<Live>();
    public DbSet<SetlistEntry> SetlistEntries => Set<SetlistEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Live>()
            .HasOne(l => l.Artist).WithMany(a => a.Lives)
            .HasForeignKey(l => l.ArtistId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Live>()
            .HasOne(l => l.Venue).WithMany(v => v.Lives)
            .HasForeignKey(l => l.VenueId).OnDelete(DeleteBehavior.Restrict);

        b.Entity<Song>()
            .HasOne(s => s.Artist).WithMany(a => a.Songs)
            .HasForeignKey(s => s.ArtistId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<SetlistEntry>()
            .HasOne(e => e.Live).WithMany(l => l.Setlist)
            .HasForeignKey(e => e.LiveId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<SetlistEntry>()
            .HasOne(e => e.Song).WithMany(s => s.SetlistEntries)
            .HasForeignKey(e => e.SongId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Live>().Property(l => l.Status).HasConversion<string>();
    }
}
