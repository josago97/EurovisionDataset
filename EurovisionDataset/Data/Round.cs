namespace EurovisionDataset.Data;

public class Round
{
    public string Name { get; set; }
    public DateTime Date { get; set; }
    public Performance[] Performances { get; set; }
}
