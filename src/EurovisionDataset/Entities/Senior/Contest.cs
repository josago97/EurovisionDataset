namespace EurovisionDataset.Entities.Senior;

public class Contest : Entities.Contest
{
    public IEnumerable<string> Broadcasters { get; set; }
}
