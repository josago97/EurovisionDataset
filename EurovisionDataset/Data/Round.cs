namespace EurovisionDataset.Data;

public abstract class Round
{
    public string Name { get; set; }
    public string Date { get; set; }
    public virtual IEnumerable<Performance> Performances { get; set; }
}
