using Eurovision.WebApp.Models;

namespace Eurovision.WebApp.Views.Pages;

public partial class ContestList
{
    private enum Order { Ascending, Descending };

    private const Order DEAFULT_ORDER = Order.Ascending;

    private string Title { get; set; }
    private IEnumerable<ContestData> AllContests { get; set; }
    private new IEnumerable<ContestData> Contests { get; set; }
    private Order ContestsOrder { get; set; }
    private string ContestOrderArrow { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        switch (Page)
        {
            case PageType.Junior:
                Title = "Junior Eurovision";
                break;

            case PageType.Senior:
                Title = "Eurovision";
                break;
        }

        Contests = AllContests = GetContests(base.Contests);
        ContestsOrder = DEAFULT_ORDER;
        UpdateContestList(false);
    }

    private IEnumerable<ContestData> GetContests(IReadOnlyList<Contest> contests)
    {
        List<ContestData> result = new List<ContestData>();

        for (int i = 0; i < contests.Count; i++)
        {
            Contest contest = contests[i];
            string countryCode = contest.IntendedCountry;

            result.Add(new ContestData()
            {
                Id = i,
                CountryCode = countryCode,
                CountryName = Repository.Countries[countryCode],
                City = contest.City,
                Year = contest.Year
            });
        }

        return result;
    }

    private void Search(string query)
    {
        IEnumerable<ContestData> result = AllContests;

        if (!string.IsNullOrEmpty(query))
        {
            result = AllContests.Where(c =>
                c.CountryName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || c.City.Contains(query, StringComparison.OrdinalIgnoreCase)
                || c.Year.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        Contests = result;

        UpdateContestList(true);
    }

    private void ChangeOrder()
    {
        ContestsOrder = ContestsOrder == Order.Ascending
            ? Order.Descending
            : Order.Ascending;

        UpdateContestList(false);
    }

    private void UpdateContestList(bool queryChanged)
    {
        if (ContestsOrder == Order.Ascending)
        {
            Contests = Contests.OrderBy(c => c.Year);
            ContestOrderArrow = "down";
        }
        else
        {
            Contests = Contests.OrderByDescending(c => c.Year);
            ContestOrderArrow = "up";
        }
    }

    private class ContestData
    {
        public int Id { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string City { get; set; }
        public int Year { get; set; }
    }
}