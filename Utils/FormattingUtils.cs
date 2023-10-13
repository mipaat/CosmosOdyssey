namespace Utils;

public static class FormattingUtils
{
    public static string FormatTimeSpan(TimeSpan timeSpan) => string.Format("{0:dd}d {0:hh}h {0:mm}min", timeSpan);

}