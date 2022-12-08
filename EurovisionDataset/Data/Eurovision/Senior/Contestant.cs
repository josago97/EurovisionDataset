namespace EurovisionDataset.Data.Eurovision.Senior;

public class Contestant : Eurovision.Contestant
{
    public string Tone { get; set; }
    public int Bpm { get; set; } = -1;
    public string Broadcaster { get; set; }
    public string Spokesperson { get; set; }
    public IList<string> Commentators { get; set; }
}
