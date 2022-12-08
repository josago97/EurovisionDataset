namespace EurovisionDataset.Data.Eurovision.Junior;

public class Round : Data.Round
{
    public new IEnumerable<Performance> Performances { get; set; }
}
