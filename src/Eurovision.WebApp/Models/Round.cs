namespace Eurovision.WebApp.Models;

public class Round
{
    public string Name { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly? Time { get; set; }
    public IReadOnlyList<Performance> Performances { get; set; }
}
