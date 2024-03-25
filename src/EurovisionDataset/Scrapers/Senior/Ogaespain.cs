namespace EurovisionDataset.Scrapers.Senior;

internal class Ogaespain : BaseOgaespain
{
    protected override string LogosFolderName => "senior";

    protected override string GetPageUrl(int year)
    {
        return $"festival-de-eurovision/eurovision-{year}";
    }
}
