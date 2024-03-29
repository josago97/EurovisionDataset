namespace Eurovision.WebApp.Models;

public class Lyrics
{
    public IReadOnlyList<string> Languages { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}
