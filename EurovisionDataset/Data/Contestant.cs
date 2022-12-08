namespace EurovisionDataset.Data;

public abstract class Contestant : IEquatable<Contestant>
{
    public int Id { get; set; }
    public string Artist { get; set; }
    public string Song { get; set; }
    public IList<Lyrics> Lyrics { get; set; }
    public IList<string> VideoUrls { get; set; }
    public IList<string> Dancers { get; set; }
    public IList<string> Backings { get; set; }
    public IList<string> Composers { get; set; }
    public IList<string> Lyricists { get; set; }
    public IList<string> Writers { get; set; }
    public string Conductor { get; set; }
    public string StageDirector { get; set; }

    public bool Equals(Contestant other)
    {
        return ReferenceEquals(this, other)
            || other != null
            && Artist.Equals(other.Artist, StringComparison.OrdinalIgnoreCase)
            && Song.Equals(other.Song, StringComparison.OrdinalIgnoreCase);
    }
}
