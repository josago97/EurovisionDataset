namespace EurovisionDataset.Scrapers.Junior;

internal class Ogaespain : BaseOgaespain
{
    protected override string LogosFolderName => "junior";

    protected override string GetPageUrl(int year)
    {
        return $"eurovision-junior/junior-{year}";
    }
}
