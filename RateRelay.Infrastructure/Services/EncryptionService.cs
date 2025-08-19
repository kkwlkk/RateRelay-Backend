using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using RateRelay.Infrastructure.Configuration;
using RateRelay.Infrastructure.Interfaces;

namespace RateRelay.Infrastructure.Services;

public class EncryptionService(IOptions<EncryptionOptions> options) : IEncryptionService
{
    public async Task<string> Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        var key = Convert.FromBase64String(options.Value.Key);
        aes.Key = key;
        aes.GenerateIV();

        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();

        ms.Write(aes.IV, 0, aes.IV.Length);

        await using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        await cs.WriteAsync(plainTextBytes);
        await cs.FlushFinalBlockAsync();

        return Convert.ToBase64String(ms.ToArray());
    }

    public async Task<string> Decrypt(string cipherText)
    {
        var cipherTextBytes = Convert.FromBase64String(cipherText);
    
        using var aes = Aes.Create();
        var key = Convert.FromBase64String(options.Value.Key);
        aes.Key = key;
    
        var iv = new byte[aes.IV.Length];
        var encryptedData = new byte[cipherTextBytes.Length - iv.Length];
    
        Buffer.BlockCopy(cipherTextBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(cipherTextBytes, iv.Length, encryptedData, 0, encryptedData.Length);

        aes.IV = iv;
    
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(encryptedData);
        await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var reader = new StreamReader(cs);
    
        return await reader.ReadToEndAsync();
    }
}