namespace LiveLogApi.Models;

// ---- 入力(作成/更新)用 ----
public record ArtistInput(string Name, string? Genre, string? Notes, string? OfficialX, string? Website);
public record VenueInput(string Name, string? City, int? Capacity, int? DrinkFee, bool? AcceptsEMoney, string? OfficialX);
public record SongInput(string Title, int ArtistId);

public record SetlistItemInput(int SongId, int Order, bool IsEncore);
public record LiveInput(
    string Title,
    DateOnly Date,
    int ArtistId,
    int VenueId,
    LiveStatus Status,
    string? Notes,
    int? Rating,
    int? TicketPrice,
    string? Seat,
    string? Companions,
    List<SetlistItemInput>? Setlist
);
public record SongMergeInput(int TargetId);
public record AliasInput(string Alias);

// ---- 出力用 ----
public record ArtistDto(int Id, string Name, string? Genre, string? Notes, string? OfficialX, string? Website);
public record VenueDto(int Id, string Name, string? City, int? Capacity, int? DrinkFee, bool? AcceptsEMoney, string? OfficialX, int LiveCount);
public record SetlistItemDto(int SongId, string Title, int Order, bool IsEncore);

public record LiveDto(
    int Id,
    string Title,
    DateOnly Date,
    int ArtistId,
    string ArtistName,
    int VenueId,
    string VenueName,
    string Status,
    string? Notes,
    int? Rating,
    int? TicketPrice,
    string? Seat,
    string? Companions,
    List<SetlistItemDto> Setlist
);

/// <summary>曲ごとのまとめ：いつ・どこで演奏されたか</summary>
public record SongPlayDto(int LiveId, string LiveTitle, DateOnly Date, string VenueName, int Order, bool IsEncore);
public record SongSummaryDto(int SongId, string Title, string ArtistName, int PlayCount, List<SongPlayDto> Plays);

// ---- 統計 ----
public record NameCount(string Name, int Count);
public record YearCount(int Year, int Count);
public record MonthCount(int Month, int Count);
public record StatsDto(
    int TotalAttended,
    int ThisYearAttended,
    int UpcomingCount,
    int TotalSongsPlayed,
    int UniqueSongsHeard,
    int TotalSpent,
    double? AverageRating,
    string? FavoriteSong,
    int FavoriteSongCount,
    List<YearCount> ByYear,
    List<MonthCount> ByMonthThisYear,
    List<NameCount> TopVenues,
    List<NameCount> TopArtists,
    List<NameCount> TopSongs
);

// ---- 全文検索 ----
public record SearchHit(string Type, int Id, string Label, string Sub);

// ---- エクスポート/インポート ----
public record ExportBundle(
    List<Artist> Artists,
    List<Venue> Venues,
    List<Song> Songs,
    List<Live> Lives
);
