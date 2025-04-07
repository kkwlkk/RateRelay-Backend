using RateRelay.Infrastructure.Constants;

namespace RateRelay.Infrastructure.Extensions;

public static class AnsiColorExtensions
{
    public static string GetAnsiCode(this ConsoleColor color)
    {
        return $"\u001b[{(int)color}m";
    }

    public static string ColorText(this ConsoleColor color, string text)
    {
        return $"{color.GetAnsiCode()}{text}\u001b[0m";
    }
}