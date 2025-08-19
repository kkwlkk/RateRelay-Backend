using System.Security.Cryptography;
using System.Text;

namespace RateRelay.Infrastructure.Utilities;

public static class HashingUtility
{
    private const string DefaultCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateSecureRandomString(int length = 32)
    {
        return new string(Enumerable.Range(0, length)
            .Select(_ => DefaultCharset[RandomNumberGenerator.GetInt32(DefaultCharset.Length)])
            .ToArray());
    }

    public static string HashString(string input)
    {
        return BCrypt.Net.BCrypt.HashPassword(input, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public static bool VerifyHash(string input, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(input, hash);
    }

    public static string HashToken(string token)
    {
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}