namespace EurovisionDataset.Data.Eurovision.Senior;

public class Contest : Eurovision.Contest
{
    public string[] Broadcasters { get; set; }
    public new IEnumerable<Contestant> Contestants { get; set; }
    public new IEnumerable<Round> Rounds { get; set; }
}
