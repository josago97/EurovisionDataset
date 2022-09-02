namespace EurovisionDataset.Data;

public class Performance
{
    public int ContestantId { get; set; }
    public int Running { get; set; }
    public int Place { get; set; }
    public Score[] Scores { get; set; }
}
