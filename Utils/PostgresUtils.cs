namespace Utils;

public class PostgresUtils
{
    public const string DefaultEscapeChar = "\\";

    public static string EscapeWildcards(string value, string escapeChar = DefaultEscapeChar)
    {
        value = value.Replace(escapeChar, escapeChar + escapeChar);

        return new[] { '%', '_', '[', ']', '^' }
            .Aggregate(value, (current, c) =>
                current.Replace(c.ToString(),
                    escapeChar + c));
    }

    public static string GetContainsPattern(string value, string escapeChar = DefaultEscapeChar) =>
        "%" + EscapeWildcards(value, escapeChar) + "%";
}