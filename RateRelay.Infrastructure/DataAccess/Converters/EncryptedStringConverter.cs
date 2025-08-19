using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RateRelay.Infrastructure.Interfaces;

namespace RateRelay.Infrastructure.DataAccess.Converters;

public class EncryptedStringConverter() : ValueConverter<string, string>(
    v => _encryptionService!.Encrypt(v).GetAwaiter().GetResult(),
    v => _encryptionService!.Decrypt(v).GetAwaiter().GetResult())
{
    private static IEncryptionService? _encryptionService;

    public static void Configure(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }
}