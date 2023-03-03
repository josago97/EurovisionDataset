namespace EurovisionDataset.Data;

public abstract class Contest
{
    public int Year { get; set; }
    public string Arena { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Slogan { get; set; }
    public string LogoUrl { get; set; }
    public string Voting { get; set; }
    public IEnumerable<string> Presenters { get; set; }
    public IEnumerable<Contestant> Contestants { get; set; }
    public IEnumerable<Round> Rounds { get; set; }
}
