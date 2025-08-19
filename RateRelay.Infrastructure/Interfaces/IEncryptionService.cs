namespace RateRelay.Infrastructure.Interfaces;

public interface IEncryptionService
{
    Task<string> Encrypt(string plainText);
    Task<string> Decrypt(string cipherText);
}