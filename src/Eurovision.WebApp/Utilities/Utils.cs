namespace Eurovision.WebApp.Utilities;

public static class Utils
{
    public static string GetDisplayRoundName(string roundName)
    {
        return roundName.ToLower() switch
        {
            "final" => "Grand Final",
            "semifinal" => "Semifinal",
            "semifinal1" => "Semifinal 1",
            _ => "Semifinal 2",
        };
    }
}
