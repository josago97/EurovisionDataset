namespace EurovisionDataset.Data.National;

public class Contest : Data.Contest
{
    public IList<Selection> Selections { get; set; }
}
