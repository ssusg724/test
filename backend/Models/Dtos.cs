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
    List<SetlistItemInput>? Setlist
);

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
    List<SetlistItemDto> Setlist
);

/// <summary>曲ごとのまとめ：いつ・どこで演奏されたか</summary>
public record SongPlayDto(int LiveId, string LiveTitle, DateOnly Date, string VenueName, int Order, bool IsEncore);
public record SongSummaryDto(int SongId, string Title, string ArtistName, int PlayCount, List<SongPlayDto> Plays);
