namespace Eurovision.WebApp.Models;

public interface IRepository
{
    IDictionary<string, string> Countries { get; }
    IReadOnlyList<Contest> SeniorContests { get; }
    IReadOnlyList<Contest> JuniorContests { get; }

    Task InitAsync();
}
