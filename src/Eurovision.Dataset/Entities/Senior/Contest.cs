namespace Eurovision.Dataset.Entities.Senior;

public class Contest : Entities.Contest
{
    public IEnumerable<string> Broadcasters { get; set; }
}
