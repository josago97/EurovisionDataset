namespace EurovisionDataset.Data.National
{
    public class Selection
    {
        public string EventName { get; set; }
        public string Country { get; set; }
        public bool IsInternal { get; set; }
        public int[] WinnersId { get; set; }
        public IList<Contestant> Contestants { get; set; }
        public IList<Round> Rounds { get; set; }
    }
}
