namespace EurovisionDataset.Entities.Senior;

public class Contestant : Entities.Contestant
{
    public int? Bpm { get; set; }
    public string Broadcaster { get; set; }
    public IEnumerable<string> Commentators { get; set; }
    public string Conductor { get; set; }
    public IEnumerable<string> Jury { get; set; }
    public string Spokesperson { get; set; }
    public string StageDirector { get; set; }
    public string Tone { get; set; }
}
