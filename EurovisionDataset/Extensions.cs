using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace EurovisionDataset;

public static class Extensions
{
    public static async Task<string> InnerTextFromHTMLAsync(this IElementHandle element, string lineBreak = "\n")
    {
        string text = await element.InnerHTMLAsync();
        text = Regex.Replace(text, @"< *br *\/*>", lineBreak); // Replace <br>

        return text;
    }
}
