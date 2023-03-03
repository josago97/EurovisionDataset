namespace EurovisionDataset.Data;

public abstract class Performance
{
    public int ContestantId { get; set; }
    public int? Running { get; set; }
    public int? Place { get; set; }
    public IReadOnlyList<Score> Scores { get; set; }
}
