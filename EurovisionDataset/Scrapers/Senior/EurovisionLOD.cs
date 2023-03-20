using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using EurovisionDataset.Data.Senior;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace EurovisionDataset.Scrapers.Senior;

public class EurovisionLOD
{
    private const string ENDPOINT = "https://so-we-must-think.space/greenstone3-lod3/greenstone/query";
    private const string EUROVISION_COLLECTION = "https://so-we-must-think.space/greenstone3/eurovision-library/collection/eurovision";
    private static readonly Dictionary<string, string> PREFIXES = new Dictionary<string, string>
    {
        { "gsdlextracted", "http://greenstone.org/gsdlextracted#" },
        { "xsd", "http://www.w3.org/2001/XMLSchema#" },
        { "dc", "http://purl.org/dc/elements/1.1/" }
    };

    public void GetContests(IList<Contest> contests)
    {
        int start = int.MaxValue;
        int end = int.MinValue;

        foreach (Contest contest in contests)
        {
            if (contest.Year < start) start = contest.Year;
            if (contest.Year > end) end = contest.Year;
        }

        Dictionary<string, string>[] contestsData = GetContestData(start, end);
        Dictionary<string, string>[] contestantsData = GetContestantsData(start, end);

        InsertDataToContests(contestsData, contests);
        InsertDataToContestants(contestantsData, contests);
    }

    private void InsertDataToContests(Dictionary<string, string>[] data, IList<Contest> contests)
    {
        foreach (Dictionary<string, string> row in data)
        {
            int year = int.Parse(row["year"]);
            Contest contest = contests.FirstOrDefault(c => c.Year == year);

            if (contest != null)
            {
                contest.LogoUrl = row["logo"];
            }
        }
    }

    private void InsertDataToContestants(Dictionary<string, string>[] data, IList<Contest> contests)
    {
        foreach (Dictionary<string, string> row in data)
        {
            Contestant contestant = null;

            if (row.TryGetValue("identifier", out string identifier))
            {
                Regex regex = new Regex(@"[0-9]+");
                Match match = regex.Match(identifier);

                if (match.Success)
                {
                    string country = identifier.Substring(0, match.Index);
                    int year = int.Parse(match.Value);

                    IEnumerable<Contestant> contestants = contests.FirstOrDefault(c => c.Year == year)
                        ?.Contestants?.Cast<Contestant>()?.Where(c =>
                        {
                            string countryName = Utils.GetCountryName(c.Country).Replace(" ", "");
                            return countryName.Equals(country, StringComparison.OrdinalIgnoreCase);
                        });

                    if (year == 1956)
                    {
                        string song = identifier.Substring(match.Index + match.Length);

                        contestant = contestants.FirstOrDefault(c =>
                            c.Song.Replace(" ", "").StartsWith(song, StringComparison.OrdinalIgnoreCase));
                    }
                    else
                        contestant = contestants.FirstOrDefault();
                }
            }

            if (contestant != null)
            {
                if (row.TryGetValue("key", out string key) && !string.IsNullOrEmpty(key))
                    contestant.Tone = key;

                if (row.TryGetValue("scale", out string scale) && !string.IsNullOrEmpty(scale))
                {
                    contestant.Tone += $" {scale}";
                    contestant.Tone = contestant.Tone.Trim();
                }

                if (row.TryGetValue("bpm", out string bpm) && double.TryParse(bpm, out double bpmValue))
                    contestant.Bpm = (int)Math.Round(bpmValue);
            }
        }
    }

    private Dictionary<string, string>[] GetContestData(int start, int end)
    {
        SparqlParameterizedString query = new SparqlParameterizedString();

        query.CommandText = $@"SELECT DISTINCT ?year ?logo WHERE {{
                
            ?esc_contest_uri dc:Relation.isPartOf <{EUROVISION_COLLECTION}>.

            ?esc_contest_uri gsdlextracted:Year ?year.
            ?esc_contest_uri gsdlextracted:YearLogoImg ?logo.

            BIND(xsd:integer(?year) AS ?year_int)
            FILTER(@start <= ?year_int && ?year_int <= @end)
        }}
        ORDER BY ASC(?year_int)";

        query.SetLiteral("start", start);
        query.SetLiteral("end", end);

        Dictionary<string, string>[] contestsData = SendQuery(query);

        foreach (Dictionary<string, string> data in contestsData)
        {
            if (data.TryGetValue("logo", out string logo) && !string.IsNullOrEmpty(logo))
            {
                string pattern = "src=\"";
                int patternIndex = logo.IndexOf(pattern);
                int startIndex = patternIndex + pattern.Length;
                int quoteIndex = logo.IndexOf('\"', startIndex);
                logo = logo.Substring(startIndex, quoteIndex - startIndex);

                data["logo"] = $"https:{logo}";
            }
        }

        return contestsData;
    }

    private Dictionary<string, string>[] GetContestantsData(int start, int end)
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

        return SendQuery(query);
    }

    private Dictionary<string, string>[] SendQuery(SparqlParameterizedString query)
    {
        SparqlRemoteEndpoint endpoint = new FederatedSparqlRemoteEndpoint(new Uri(ENDPOINT));
        //endpoint.DefaultGraphs.Add("https://so-we-must-think.space/greenstone3/eurovision-lod-foo-library/collection/eurovision");
        endpoint.ResultsAcceptHeader = "application/sparql-results+json";

        foreach (var pair in PREFIXES)
            query.Namespaces.AddNamespace(pair.Key, new Uri(pair.Value));

        SparqlQueryParser parser = new SparqlQueryParser();
        SparqlQuery sparqlQuery = parser.ParseFromString(query);
        SparqlResultSet results = endpoint.QueryWithResultSet(sparqlQuery.ToString());
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
