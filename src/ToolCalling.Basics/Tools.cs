namespace ToolCalling.Basics;

public static class Tools
{
    public static DateTime CurrentDataAndTime(TimeType type)
    {
        return type switch
        {
            TimeType.Local => DateTime.Now,
            TimeType.Utc => DateTime.UtcNow,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static string CurrentTimezone()
    {
        return TimeZoneInfo.Local.DisplayName;
    }

    public enum TimeType
    {
        Local,
        Utc
    }
}