namespace RateRelay.Infrastructure.Helpers;

public static class RandomHelper
{
    public static readonly Random Random = Random.Shared;

    public static int GetRandomNumberInRange(int min, int max)
        => Random.Next(min, max + 1);

    public static double GetRandomDoubleInRange(double min, double max)
        => Random.NextDouble() * (max - min) + min;

    public static T GetRandomEnumValue<T>() where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        return values[Random.Next(values.Length)];
    }

    public static string GetRandomString(int length,
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")
    {
        var result = new char[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = chars[Random.Next(chars.Length)];
        }

        return new string(result);
    }
}