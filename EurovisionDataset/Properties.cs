namespace EurovisionDataset;

public static class Properties
{
    private const string START_ARGUMENT = "-start";
    public static int START { get; private set; } = 1956;

    private const string END_ARGUMENT = "-end";
    public static int END { get; private set; } = DateTime.Now.Year;

    public static void ReadArguments(string[] arguments)
    {
        for (int i = 0; i < arguments.Length; i += 1)
        {
            string command = arguments[i];

            switch (command)
            {
                case START_ARGUMENT:
                    START = int.Parse(arguments[++i]);
                    break;

                case END_ARGUMENT:
                    END = int.Parse(arguments[++i]);
                    break;
            }
        }
    }
}
