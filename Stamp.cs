namespace mintstamper;

public enum StampTypeEnum
{
    Regular,
    Control,
    Note,
    Stacked,
    Adjust
}

public record Stamp
(
    string Text,
    StampTypeEnum StampType,
    bool IsBirthday,
    int TimestampSeconds
);