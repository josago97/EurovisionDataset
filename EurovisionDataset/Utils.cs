using System.Reflection;

namespace EurovisionDataset
{
    public static class Utils
    {
        public static readonly Dictionary<string, string> COUNTRY_CODES = new Dictionary<string, string>() 
        {
            { "AL", "Albania" }, { "AD", "Andorra" },
            { "AM", "Armenia" }, { "AU", "Australia" },
            { "AT", "Austria" }, { "AZ", "Azerbaijan" },
            { "BY", "Belarus" }, { "BE", "Belgium" },
            { "BA", "Bosnia and Herzegovina" }, { "BG", "Bulgaria" },
            { "HR", "Croatia" }, { "CY", "Cyprus" },
            { "CZ", "Czech Republic" }, { "DK", "Denmark" },
            { "EE", "Estonia" }, { "FI", "Finland" },
            { "FR", "France" }, { "GE", "Georgia" },
            { "DE", "Germany" }, { "GR", "Greece" },
            { "HU", "Hungary" }, { "IS", "Iceland" },
            { "IE", "Ireland" }, { "IL", "Israel" },
            { "IT", "Italy" }, { "LV", "Latvia" },
            { "LT", "Lithuania" }, { "LU", "Luxembourg" },
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
            { "GB", "United Kingdom" }, { "YU", "Yugoslavia" },
        };
        
        public static string GetCountryCode(string countryName)
        {
            string result = null;
            countryName = countryName.Replace("&", "and");

            try
            {
                result = COUNTRY_CODES.First(p => p.Value == countryName).Key;
            }
            catch
            {
                Console.WriteLine($"No country code: {countryName}");
            }

            return result;
        }

        public static string GetCountryName(string countryCode)
        {
            return COUNTRY_CODES[countryCode];
        }

        public static Stream OpenEmbeddedResource(string path)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            return assembly.GetManifestResourceStream(path);
        }

        public static byte[] ReadEmbeddedResource(string path)
        {
            using Stream stream = OpenEmbeddedResource(path);
            using MemoryStream memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        public static string ReadEmbeddedTextResource(string path)
        {
            using StreamReader reader = new StreamReader(OpenEmbeddedResource(path));

            return reader.ReadToEnd();
        }
    }
}
