namespace Utils;

public class PostgresUtils
{
    public static string EscapeWildcards(string value, char escapeChar = '\\')
    {
        value = value.Replace(escapeChar.ToString(), escapeChar.ToString() + escapeChar);

        return new[] { '%', '_', '[', ']', '^' }
            .Aggregate(value, (current, c) =>
                current.Replace(c.ToString(),
                    escapeChar.ToString() + c));
    }

    public static string GetContainsPattern(string value, char escapeChar = '\\') =>
        "%" + EscapeWildcards(value, escapeChar) + "%";
}