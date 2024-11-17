using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Eurovision.Dataset.Entities.Senior;
using Eurovision.Dataset.Utilities;
using Sharplus.System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace Eurovision.Dataset.Scraping.Scrapers.Senior;

public class EurovisionLod
{
    private const string ENDPOINT = "https://so-we-must-think.space/greenstone3-lod3/greenstone/query";
    private const string EUROVISION_COLLECTION = "https://so-we-must-think.space/greenstone3/eurovision-library/collection/eurovision";
    private static readonly Dictionary<string, string> PREFIXES = new Dictionary<string, string>
    {
        { "gsdlextracted", "http://greenstone.org/gsdlextracted#" },
        { "xsd", "http://www.w3.org/2001/XMLSchema#" },
        { "dc", "http://purl.org/dc/elements/1.1/" }
    };

    private SparqlQueryClient Client { get; init; }

    public EurovisionLod()
    {
        Client = new SparqlQueryClient(new HttpClient(), new Uri(ENDPOINT))
        {
            ResultsAcceptHeader = "application/sparql-results+json"
        };
    }

    public async Task ScrapContestsAsync(IReadOnlyList<Contest> contests)
    {
        int start = int.MaxValue;
        int end = int.MinValue;

        foreach (Contest contest in contests)
        {
            if (contest.Year < start) start = contest.Year;
            if (contest.Year > end) end = contest.Year;
        }

        Dictionary<string, string>[] contestantsData = await GetContestantsDataAsync(start, end);
        InsertDataToContestants(contestantsData, contests);
    }

    private void InsertDataToContestants(Dictionary<string, string>[] data, IReadOnlyList<Contest> contests)
    {
        Contestant contestant = null;

        foreach (Dictionary<string, string> row in data)
        {
            if (!row.TryGetValue("identifier", out string identifier)) continue;

            Regex regex = new Regex(@"[0-9]+");
            Match match = regex.Match(identifier);

            if (!match.Success) continue;

            string country = identifier.Substring(0, match.Index);
            int year = int.Parse(match.Value);

            IEnumerable<Contestant> contestants = contests.FirstOrDefault(c => c.Year == year)
                ?.Contestants?.Cast<Contestant>()?.Where(c =>
                {
                    string countryName = CountryCollection.GetCountryName(c.Country).Replace(" ", "");
                    return countryName.Equals(country, StringComparison.OrdinalIgnoreCase);
                });

            if (contestants.IsNullOrEmpty()) continue;

            if (year == 1956)
            {
                string song = identifier.Substring(match.Index + match.Length);

                contestant = contestants.FirstOrDefault(c =>
                    c.Song.Replace(" ", "")
                    .StartsWith(song, StringComparison.OrdinalIgnoreCase));
            }
            else
                contestant = contestants.FirstOrDefault();

            if (contestant != null) InsertDataToContestant(row, contestant);
        }
    }

    private void InsertDataToContestant(Dictionary<string, string> data, Contestant contestant)
    {
        if (data.TryGetValue("key", out string key) && !string.IsNullOrEmpty(key))
            contestant.Tone = key;

        if (data.TryGetValue("scale", out string scale) && !string.IsNullOrEmpty(scale))
        {
            contestant.Tone += $" {scale}";
            contestant.Tone = contestant.Tone.Trim();
        }

        if (data.TryGetValue("bpm", out string bpm) && double.TryParse(bpm, out double bpmValue))
            contestant.Bpm = (int)Math.Round(bpmValue);
    }

    private Task<Dictionary<string, string>[]> GetContestantsDataAsync(int start, int end)
    {
        SparqlParameterizedString query = new SparqlParameterizedString();

        query.Namespaces.AddNamespace("essentia", new Uri("http://upf.edu/essentia#"));

        query.CommandText = $@"SELECT ?identifier ?key ?scale ?bpm WHERE {{
            ?esc_entrant_uri dc:Relation.isPartOf <{EUROVISION_COLLECTION}>.

            ?esc_entrant_uri gsdlextracted:Identifier ?identifier.
            
            ?esc_entrant_uri gsdlextracted:Year ?year.
            BIND(xsd:integer(?year) AS ?year_int)
            FILTER(@start <= ?year_int && ?year_int <= @end)

            OPTIONAL {{ ?esc_entrant_uri essentia:tonal_key_edma_key ?key. }}
            OPTIONAL {{ ?esc_entrant_uri essentia:tonal_key_edma_scale ?scale. }}
            OPTIONAL {{ ?esc_entrant_uri essentia:rhythm_bpm ?bpm. }}
        }}
        ORDER BY ASC(?year_int)";

        query.SetLiteral("start", start);
        query.SetLiteral("end", end);

        return SendQueryAsync(query);
    }

    private async Task<Dictionary<string, string>[]> SendQueryAsync(SparqlParameterizedString query)
    {
        foreach (KeyValuePair<string, string> pair in PREFIXES)
            query.Namespaces.AddNamespace(pair.Key, new Uri(pair.Value));

        SparqlQueryParser parser = new SparqlQueryParser();
        SparqlQuery sparqlQuery = parser.ParseFromString(query);
        SparqlResultSet results = await Client.QueryWithResultSetAsync(sparqlQuery.ToString());
        string json = StringWriter.Write(results, new SparqlJsonWriter());
        Dictionary<string, string>[] result = GetValues(json);

        return result;
    }

    private Dictionary<string, string>[] GetValues(string sparqlResultJson)
    {
        List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();
        JsonNode jObject = JsonNode.Parse(sparqlResultJson);
        string[] headers = jObject["head"]["vars"].AsArray().Select(j => j.GetValue<string>()).ToArray();
        JsonArray body = jObject["results"]["bindings"].AsArray();

        foreach (JsonNode jItem in body)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            foreach (string header in headers)
            {
                JsonNode field = jItem[header]?["value"];

                if (field != null) values.Add(header, field.GetValue<string>());
            }

            result.Add(values);
        }

        return result.ToArray();
    }
}
