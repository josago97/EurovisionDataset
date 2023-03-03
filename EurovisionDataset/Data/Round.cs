namespace EurovisionDataset.Data;

public abstract class Round
{
    public string Name { get; set; }
    public string Date { get; set; }
    public IEnumerable<Performance> Performances { get; set; }
}
