namespace mintstamper;

public enum StampTypeEnum
{
    Regular,
    Control,
    Note,
    Stacked
}

public record Stamp
(
    string Text,
    StampTypeEnum StampType,
    bool IsBirthday,
    int TimestampSeconds
);