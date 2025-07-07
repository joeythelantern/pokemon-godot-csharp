using System.Linq;

namespace Game.Utilities;

public static class StringModule
{
    public static string ToPascalCase(string? input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        var parts = input.Replace("-", "_").Split('_');
        return string.Concat(parts.Select(p => char.ToUpper(p[0]) + p.Substring(1)));
    }
}