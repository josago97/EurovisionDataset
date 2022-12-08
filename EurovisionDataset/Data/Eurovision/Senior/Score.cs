namespace EurovisionDataset.Data.Eurovision.Senior;

public class Score : Data.Score
{
    public Dictionary<string, int> Votes { get; set; }
}
