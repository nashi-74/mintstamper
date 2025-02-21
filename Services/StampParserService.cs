namespace mintstamper.Services;

public interface IStampParserService
{
    List<Stamp> ParseStamps(string stampsText, int stackHighlightInterval);
}

public class StampParserService : IStampParserService
{
    public List<Stamp> ParseStamps(string stampsText, int stackHighlightInterval)
    {
        if (stampsText == "") return [];
        var stamps = stampsText.Split("\n")
            .Select(ParseLine)
            .ToList();

        for (int i = 1; i < stamps.Count; i++)
        {
            if (stamps[i].TimestampSeconds - stamps[i - 1].TimestampSeconds < stackHighlightInterval && !(stamps[i - 1].IsBirthday || stamps[i].IsBirthday))
            {
                if (stamps[i - 1].StampType == StampTypeEnum.Regular) stamps[i - 1] = stamps[i - 1] with { StampType = StampTypeEnum.Stacked };
                if (stamps[i].StampType == StampTypeEnum.Regular) stamps[i] = stamps[i] with { StampType = StampTypeEnum.Stacked };
            }
        }

        return stamps;
    }

    private static Stamp ParseLine(string line)
    {
        var lineLower = line.ToLower();

        try
        {
            var timestampSeconds = lineLower[..lineLower.IndexOf(' ')]
                .Split(":")
                .Select(int.Parse)
                .Reverse()
                .Zip([1, 60, 3600], (t, m) => t * m)
                .Sum();

            var lineType = StampTypeEnum.Regular;
            if (lineLower.Contains("note: ")) lineType = StampTypeEnum.Note;
            if (lineLower.Contains("control stamp") || lineLower.Contains("control tag") || lineLower.Contains("control timestamp") || 
                lineLower.Contains("anshor stamp") || lineLower.Contains("anchor tag") || lineLower.Contains("anchor timestamp")) lineType = StampTypeEnum.Control;
            if (lineLower.Contains("adjust") || lineLower.Contains("retime")) lineType = StampTypeEnum.Adjust;

            return new(
                Text: line,
                Segments: [],
                StampType: lineType,
                IsBirthday: lineLower.Contains("hbd"),
                TimestampSeconds: timestampSeconds
            );
        }
        catch
        {
            return new("ERROR PARSING LINE", [], StampTypeEnum.Note, false, 0);
        }
    }
}
