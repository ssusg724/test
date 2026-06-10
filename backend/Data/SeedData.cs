using LiveLogApi.Models;

namespace LiveLogApi.Data;

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Artists.Any()) return;

        var elephant = new Artist { Name = "ELLEGARDEN", Genre = "ロック" };
        var asian = new Artist { Name = "ASIAN KUNG-FU GENERATION", Genre = "オルタナティブロック" };
        db.Artists.AddRange(elephant, asian);

        var zepp = new Venue { Name = "Zepp Tokyo", City = "東京", Capacity = 2700 };
        var club = new Venue { Name = "下北沢SHELTER", City = "東京", Capacity = 250 };
        db.Venues.AddRange(zepp, club);
        db.SaveChanges();

        var s1 = new Song { Title = "Supernova", ArtistId = elephant.Id };
        var s2 = new Song { Title = "Missing", ArtistId = elephant.Id };
        var s3 = new Song { Title = "リライト", ArtistId = asian.Id };
        var s4 = new Song { Title = "ソラニン", ArtistId = asian.Id };
        db.Songs.AddRange(s1, s2, s3, s4);
        db.SaveChanges();

        db.Lives.Add(new Live
        {
            Title = "ELLEGARDEN TOUR 2024",
            Date = new DateOnly(2024, 11, 3),
            ArtistId = elephant.Id,
            VenueId = zepp.Id,
            Status = LiveStatus.Attended,
            Notes = "最高だった",
            Setlist = new()
            {
                new SetlistEntry { SongId = s1.Id, Order = 1 },
                new SetlistEntry { SongId = s2.Id, Order = 2 },
                new SetlistEntry { SongId = s1.Id, Order = 3, IsEncore = true },
            }
        });
        db.Lives.Add(new Live
        {
            Title = "AKG 弾き語りツアー",
            Date = new DateOnly(2025, 8, 20),
            ArtistId = asian.Id,
            VenueId = club.Id,
            Status = LiveStatus.Interested,
        });
        db.SaveChanges();
    }
}
