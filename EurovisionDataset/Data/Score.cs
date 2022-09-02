namespace EurovisionDataset.Data;

public class Score
{
    public string Name { get; set; }
    public int Points { get; set; }
    public Dictionary<string, int> Votes { get; set; }
}
