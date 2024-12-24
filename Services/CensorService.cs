using System.Text.RegularExpressions;

namespace mintstamper.Services;

public interface ICensorService
{
    string CensorText(string text, List<string> replacements);
}

public class CensorService : ICensorService
{
    public string CensorText(string text, List<string> replacements)
    {
        foreach (var replacement in replacements)
        {
            string pattern = $@"\b{Regex.Escape(replacement)}\b";
            text = Regex.Replace(text, pattern, match => CensorWord(match.Value), RegexOptions.IgnoreCase);
        }

        return text;
    }

    private static string CensorWord(string word)
    {
        foreach (char c in word)
        {
            if ("aeiouAEIOU".Contains(c))
            {
                int censorIndex = word.IndexOf(c);
                return word.Remove(censorIndex, 1).Insert(censorIndex, "*");
            }
        }

        return word.Remove(0, 1).Insert(0, "*");
    }
}
