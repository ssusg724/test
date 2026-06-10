using System.ComponentModel.DataAnnotations;

namespace LiveLogApi.Models;

/// <summary>バンド／アーティスト</summary>
public class Artist
{
    public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";
    public string? Genre { get; set; }
    public string? Notes { get; set; }

    /// <summary>公式X(Twitter)のURLまたは@ハンドル</summary>
    public string? OfficialX { get; set; }
    /// <summary>公式サイトURL</summary>
    public string? Website { get; set; }

    public List<Live> Lives { get; set; } = new();
    public List<Song> Songs { get; set; } = new();
}

/// <summary>会場（ライブハウス・ホールなど）</summary>
public class Venue
{
    public int Id { get; set; }
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";
    public string? City { get; set; }
    public int? Capacity { get; set; }

    /// <summary>ドリンク代(円)</summary>
    public int? DrinkFee { get; set; }
    /// <summary>電子マネー・キャッシュレス対応か</summary>
    public bool? AcceptsEMoney { get; set; }
    public string? OfficialX { get; set; }

    public List<Live> Lives { get; set; } = new();
}

/// <summary>曲</summary>
public class Song
{
    public int Id { get; set; }
    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    public int ArtistId { get; set; }
    public Artist? Artist { get; set; }

    public List<SetlistEntry> SetlistEntries { get; set; } = new();
}

/// <summary>ライブ公演。行った記録にも、これから行きたい(応募)予定にも使う。</summary>
public class Live
{
    public int Id { get; set; }
    [Required, MaxLength(300)]
    public string Title { get; set; } = "";
    public DateOnly Date { get; set; }

    public int ArtistId { get; set; }
    public Artist? Artist { get; set; }

    public int VenueId { get; set; }
    public Venue? Venue { get; set; }

    /// <summary>参加ステータス</summary>
    public LiveStatus Status { get; set; } = LiveStatus.Attended;

    public string? Notes { get; set; }

    public List<SetlistEntry> Setlist { get; set; } = new();
}

public enum LiveStatus
{
    /// <summary>気になる（候補）</summary>
    Interested,
    /// <summary>応募済み・チケット確保</summary>
    Applied,
    /// <summary>参加済み</summary>
    Attended
}

/// <summary>セットリストの1曲（ライブと曲を結ぶ。演奏順を持つ）</summary>
public class SetlistEntry
{
    public int Id { get; set; }

    public int LiveId { get; set; }
    public Live? Live { get; set; }

    public int SongId { get; set; }
    public Song? Song { get; set; }

    /// <summary>演奏順（1始まり）</summary>
    public int Order { get; set; }

    /// <summary>アンコールか</summary>
    public bool IsEncore { get; set; }
}
