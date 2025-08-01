namespace RateRelay.Infrastructure.Logging;

public static class AnsiColor
{
    public const string Black = "30";
    public const string DarkRed = "31";
    public const string DarkGreen = "32";
    public const string DarkYellow = "33";
    public const string DarkBlue = "34";
    public const string DarkMagenta = "35";
    public const string DarkCyan = "36";
    public const string Gray = "37";

    public const string DarkGray = "90";
    public const string Red = "91";
    public const string Green = "92";
    public const string Yellow = "93";
    public const string Blue = "94";
    public const string Magenta = "95";
    public const string Cyan = "96";
    public const string White = "97";

    public static class Background
    {
        public const string Black = "40";
        public const string DarkRed = "41";
        public const string DarkGreen = "42";
        public const string DarkYellow = "43";
        public const string DarkBlue = "44";
        public const string DarkMagenta = "45";
        public const string DarkCyan = "46";
        public const string Gray = "47";

        public const string DarkGray = "100";
        public const string Red = "101";
        public const string Green = "102";
        public const string Yellow = "103";
        public const string Blue = "104";
        public const string Magenta = "105";
        public const string Cyan = "106";
        public const string White = "107";
    }

    public static class Style
    {
        public const string Bold = "1";
        public const string Dim = "2";
        public const string Italic = "3";
        public const string Underline = "4";
        public const string Blink = "5";
        public const string Reverse = "7";
        public const string Strikethrough = "9";
    }

    public static string Format(string text, string color, string? backgroundColor = null, string? style = null)
    {
        var codes = new List<string> { color };
        if (!string.IsNullOrEmpty(backgroundColor)) codes.Add(backgroundColor);
        if (!string.IsNullOrEmpty(style)) codes.Add(style);

        return $"\u001b[{string.Join(";", codes)}m{text}\u001b[0m";
    }
}