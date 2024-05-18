using System.Reflection;

namespace Eurovision.Dataset.Utilities;

internal static class Assets
{
    private const string EMBED_FOLDER_PATH = "Eurovision.Dataset.Assets";

    public static Stream OpenEmbedResource(string path)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        return assembly.GetManifestResourceStream(GetEmbedAbsolutePath(path));
    }

    public static byte[] ReadEmbedResource(string path)
    {
        using Stream stream = OpenEmbedResource(path);
        using MemoryStream memoryStream = new MemoryStream();

        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    public static string ReadEmbedTextResource(string path)
    {
        using StreamReader reader = new StreamReader(OpenEmbedResource(path));

        return reader.ReadToEnd();
    }

    private static string GetEmbedAbsolutePath(string relativePath)
    {
        return $"{EMBED_FOLDER_PATH}/{relativePath}".Replace('/', '.');
    }
}
