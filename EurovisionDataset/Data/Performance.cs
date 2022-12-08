namespace EurovisionDataset.Data;

public abstract class Performance
{
    public int ContestantId { get; set; }
    public int Running { get; set; } = -1;
    public int Place { get; set; } = -1;
    public IEnumerable<Score> Scores { get; set; }
}
