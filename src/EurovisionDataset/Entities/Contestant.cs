namespace EurovisionDataset.Entities;

public class Contestant : IEquatable<Contestant>
{
    public int Id { get; set; }
    public string Country { get; set; }
    public string Artist { get; set; }
    public string Song { get; set; }
    public IEnumerable<Lyrics> Lyrics { get; set; }
    public IEnumerable<string> VideoUrls { get; set; }
    public IEnumerable<string> Dancers { get; set; }
    public IEnumerable<string> Backings { get; set; }
    public IEnumerable<string> Composers { get; set; }
    public IEnumerable<string> Lyricists { get; set; }
    public IEnumerable<string> Writers { get; set; }

    public bool Equals(Contestant other)
    {
        return ReferenceEquals(this, other)
            || other != null
            && Artist.Equals(other.Artist, StringComparison.OrdinalIgnoreCase)
            && Song.Equals(other.Song, StringComparison.OrdinalIgnoreCase);
    }
}
