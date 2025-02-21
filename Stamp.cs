namespace mintstamper;

public enum StampTypeEnum
{
    Regular,
    Control,
    Note,
    Stacked,
    Adjust,
    ContentFilter
}

public record Stamp
(
    string Text,
    List<(string text, StampTypeEnum segmentType)> Segments,
    StampTypeEnum StampType,
    bool IsBirthday,
    int TimestampSeconds
);

