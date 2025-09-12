using System.Globalization;

namespace Uptime.Workflows.Core.Common;

public static class DomainIdParser
{
    public static T Parse<T>(string? text, string typeName, Func<int, T> factory)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentNullException(nameof(text));

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            throw new FormatException($"'{text}' is not a valid {typeName}.");

        return value <= 0
            ? throw new ArgumentOutOfRangeException(nameof(text), value, $"{typeName} must be a positive integer.")
            : factory(value);
    }

    public static bool TryParse<T>(string? text, Func<int, T> factory, out T id)
    {
        id = default!;
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) || value <= 0)
            return false;
        id = factory(value);
        return true;
    }
}