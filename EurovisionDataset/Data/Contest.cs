namespace EurovisionDataset.Data;

public class Contest
{
    private Contestant[] _contestants;

    public int Year { get; set; }
    public string Arena { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string[] Broadcasters { get; set; }
    public string[] Presenters { get; set; }
    public string Slogan { get; set; }
    public Contestant[] Contestants 
    { 
        get => _contestants; 
        set
        {
            _contestants = value;
            _contestants.ForEach((i, c) => c.Id = i + 1);
        }
    }
    public Round[] Rounds { get; set; }
}
