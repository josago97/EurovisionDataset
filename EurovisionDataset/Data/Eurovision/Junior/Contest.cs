namespace EurovisionDataset.Data.Eurovision.Junior;

public class Contest : Eurovision.Contest
{
    public new IEnumerable<Contestant> Contestants { get; set; }
    public new IEnumerable<Round> Rounds { get; set; }
}
