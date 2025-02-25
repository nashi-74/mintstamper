using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using mintstamper.Content_Filter_Data;
using static System.Net.Mime.MediaTypeNames;

namespace mintstamper.Services;

public interface ICensorService
{
    List<Stamp> CensorText(List<Stamp> stamps);
    List<Stamp> HighlightFilteredWords(List<Stamp> stamps);
}

public class CensorService : ICensorService
{
    public List<Stamp> CensorText(List<Stamp> stamps)
    {
        string wordToReplace = "";
        Stamp? newStamp = null;
        List<Stamp> newStamps = [];
        foreach (Stamp stamp in stamps)
        {
            newStamp = stamp;
            while (ContainsHashSetWord(newStamp.Text, ref wordToReplace))
            {
                string pattern = $@"\b{Regex.Escape(wordToReplace)}\b";
                newStamp = newStamp with { Text = Regex.Replace(newStamp.Text, pattern, match => CensorWord(match.Value), RegexOptions.IgnoreCase) };
            }
            newStamps.Add(newStamp);
        }
        return newStamps;
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

    public List<Stamp> HighlightFilteredWords(List<Stamp> stamps)
    {
        string wordToHighlight = "";
        Stamp? newStamp = null;
        Stamp? tempStamp = null;
        List<Stamp> newStamps = [];
        foreach (Stamp stamp in stamps)
        {
            newStamp = stamp;
            tempStamp = stamp;
            List<string> wordsToHighlight = new List<string>();
            while (ContainsHashSetWord(tempStamp.Text, ref wordToHighlight)) //in this loop words from the censor list are detected and saved in wordsToHighlight
            {
                wordsToHighlight.Add(wordToHighlight);
                string pattern = $@"\b{Regex.Escape(wordToHighlight)}\b";
                tempStamp = tempStamp with { Text = Regex.Replace(tempStamp.Text, pattern, match => CensorWord(match.Value), RegexOptions.IgnoreCase) };
            }
            string tempText = stamp.Text;
            foreach (string word in wordsToHighlight) //in this loop we insert tags that let us split and highlight the text from the stamp
            {
                string pattern = $@"\b({Regex.Escape(word)})\b";
                string replacement = $"|split||highlight|$1|split|";
                tempText = Regex.Replace(tempText, pattern, replacement, RegexOptions.IgnoreCase);
            }
            List<(string text, StampTypeEnum segmentType)> processedSegments = [];
            foreach (string segment in tempText.Split("|split|", StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment.IndexOf("|highlight|") == -1)
                {
                    processedSegments.Add((segment, stamp.StampType));
                }
                else
                {
                    processedSegments.Add((segment.Replace("|highlight|", ""), StampTypeEnum.ContentFilter));
                }
            }
            newStamp = newStamp with { Segments = processedSegments };
            newStamps.Add(newStamp);

        }
        return newStamps;
    }

    private static readonly char[] separators = [' ', '.', ',', '"', '\'', '!', '?', '-', '(', ')', '{', '}', '[', ']', '_', '+', '=', '*', '/', '\\', '<', '>', ':', ';', '&', '%', '#', '@', '~', '$', '^'];

    private static bool ContainsHashSetWord(string stampText, ref string returnedWord)
    {
        foreach (var word in stampText.Split(separators, StringSplitOptions.RemoveEmptyEntries))
        {
            if (ContentFilterData.FilterList.Contains(word.ToLower()))
            {
                returnedWord = word;
                return true;
            }
        }
        if (stampText.Contains("9/11")) //variations of 9/11 have to be handled on their own because the split above makes them impossible to detect, as "9" and "11" are processed as separate words
        {
            returnedWord = "9/11";
            return true;
        }
        if (stampText.Contains("9-11"))
        {
            returnedWord = "9-11";
            return true;
        }
        return false;
    }
}
