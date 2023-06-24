namespace EurovisionDataset;

public static class Properties
{
    private const char ARGUMENT_PREFFIX = '-';

    private const string START_ARGUMENT = "start";
    public static int START { get; private set; } = 1956;

    private const string END_ARGUMENT = "end";
    public static int END { get; private set; } = DateTime.Now.Year;

    private const string EUROVISION_SENIOR_ARGUMENT = "senior";
    public static bool EUROVISION_SENIOR { get; private set; } = false;

    private const string EUROVISION_JUNIOR_ARGUMENT = "junior";
    public static bool EUROVISION_JUNIOR { get; private set; } = false;

    private const string HIDE_BROWSER_ARGUMENT = "hide";
    public static bool HIDE_BROWSER { get; private set; } = false;

    private const string JSON_INDENTED_ARGUMENT = "json_indented";
    public static bool JSON_INDENTED { get; private set; } = false;

    public static void ReadArguments(string[] arguments)
    {
        for (int i = 0; i < arguments.Length; i++)
        {
            string command = arguments[i];

            if (!command.StartsWith(ARGUMENT_PREFFIX))
                throw new ArgumentException($"Arguments must start with {ARGUMENT_PREFFIX}");

            switch (command.Substring(1).ToLower())
            {
                case START_ARGUMENT:
                    START = int.Parse(arguments[++i]);
                    break;

                case END_ARGUMENT:
                    END = int.Parse(arguments[++i]);
                    break;

                case HIDE_BROWSER_ARGUMENT:
                    HIDE_BROWSER = true;
                    break;

                case EUROVISION_JUNIOR_ARGUMENT:
                    EUROVISION_JUNIOR = true;
                    break;

                case EUROVISION_SENIOR_ARGUMENT:
                    EUROVISION_SENIOR = true;
                    break;

                case JSON_INDENTED_ARGUMENT:
                    JSON_INDENTED = true;
                    break;
            }
        }

        if (!EUROVISION_JUNIOR && !EUROVISION_SENIOR)
            EUROVISION_JUNIOR = EUROVISION_SENIOR = true;
    }
}
