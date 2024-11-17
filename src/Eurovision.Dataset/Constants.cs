namespace Eurovision.Dataset;

internal class Constants
{
    public const string GITHUB_ASSETS_URL = "https://raw.githubusercontent.com/josago97/EurovisionDataset/main/assets/";

    private const string ROOT_PATH = "../../../../..";

    public const string ASSETS_PATH = $"{ROOT_PATH}/assets";

    private const string LOGOS_FOLDER = $"logos";
    public const string JUNIOR_LOGOS_FOLDER = $"{LOGOS_FOLDER}/junior";
    public const string SENIOR_LOGOS_FOLDER = $"{LOGOS_FOLDER}/senior";

    public const string DATASET_PATH = $"{ROOT_PATH}/dataset";

    public const string COUNTRIES_FILENAME = "countries";
    public const string SENIOR_FILENAME = "eurovision";
    public const string JUNIOR_FILENAME = "junior";
}
