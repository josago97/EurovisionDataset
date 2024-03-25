namespace EurovisionDataset.Utilities;

internal static class CountryCollection
{
    public static readonly Dictionary<string, string> COUNTRY_CODES = new Dictionary<string, string>()
    {
        { "AL", "Albania" }, { "AD", "Andorra" },
        { "AM", "Armenia" }, { "AU", "Australia" },
        { "AT", "Austria" }, { "AZ", "Azerbaijan" },
        { "BY", "Belarus" }, { "BE", "Belgium" },
        { "BA", "Bosnia and Herzegovina" }, { "BG", "Bulgaria" },
        { "HR", "Croatia" }, { "CY", "Cyprus" },
        { "CZ", "Czechia" }, { "DK", "Denmark" },
        { "EE", "Estonia" }, { "FI", "Finland" },
        { "FR", "France" }, { "GE", "Georgia" },
        { "DE", "Germany" }, { "GR", "Greece" },
        { "HU", "Hungary" }, { "IS", "Iceland" },
        { "IE", "Ireland" }, { "IL", "Israel" },
        { "IT", "Italy" }, {"KZ", "Kazakhstan"},
        { "LV", "Latvia" }, { "LT", "Lithuania" },
        { "LU", "Luxembourg" },
        { "MT", "Malta" }, { "MD", "Moldova" },
        { "MC", "Monaco" }, { "ME", "Montenegro" },
        { "MA", "Morocco" }, { "NL", "Netherlands" },
        { "MK", "North Macedonia" }, { "NO", "Norway" },
        { "PL", "Poland" }, { "PT", "Portugal" },
        { "RO", "Romania" }, { "RU", "Russia" },
        { "SM", "San Marino" }, { "RS", "Serbia" },
        { "CS", "Serbia and Montenegro" }, { "SK", "Slovakia" },
        { "SI", "Slovenia" }, { "ES", "Spain" },
        { "SE", "Sweden" }, { "CH", "Switzerland" },
        { "TR", "Turkey" }, { "UA", "Ukraine" },
        { "GB", "United Kingdom" }, { "GB-WLS", "Wales" },
        { "YU", "Yugoslavia" },
    };

    public static string GetCountryCode(string countryName)
    {
        string result = null;
        countryName = countryName.Replace("The ", "", StringComparison.OrdinalIgnoreCase)
            .Replace("&", "and").Trim();

        try
        {
            result = COUNTRY_CODES.First(p =>
                p.Value.Equals(countryName, StringComparison.OrdinalIgnoreCase))
                .Key;
        }
        catch
        {
            Console.WriteLine($"No country code: {countryName}");
        }

        return result;
    }

    public static string GetCountryName(string countryCode)
    {
        return COUNTRY_CODES[countryCode.ToUpper()];
    }
}
