using EurovisionDataset.Data.Eurovision;

namespace EurovisionDataset.Scrapers.Eurovision;

public abstract class EurovisionWorld : Scrapers.EurovisionWorld
{
    protected virtual void SetContestInfo(Contest contest, Dictionary<string, string> data)
    {
        if (data.TryGetValue("hosts", out string hosts))
            contest.Presenters = hosts.Split('\n');

        if (data.TryGetValue("slogan", out string slogan))
            contest.Slogan = slogan;

        if (data.TryGetValue("voting", out string voting))
            contest.Voting = voting;
    }
}
