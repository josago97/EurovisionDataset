namespace EurovisionDataset.Entities.Senior;

public class Score : Entities.Score
{
    public Dictionary<string, int> Votes { get; set; }
}
