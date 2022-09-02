namespace EurovisionDataset.Data;

public class Contestant
{
    public int Id { get; set; }
    public string Country { get; set; }
    public string Artist { get; set; }
    public string Song { get; set; }
    public string[] Composers { get; set; }
    public string[] Writers { get; set; }
    public string Lyrics { get; set; }
    public string VideoUrl { get; set; }
    public string Broadcaster { get; set; }
}
