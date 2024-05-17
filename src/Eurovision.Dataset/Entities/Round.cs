namespace Eurovision.Dataset.Entities;

public class Round
{
    public string Name { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? Time { get; set; }
    public IEnumerable<Performance> Performances { get; set; }
}
