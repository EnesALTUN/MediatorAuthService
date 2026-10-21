using System.Security.Cryptography;

namespace MediatorAuthService.Domain.Core.Extensions;

public static class HashingManager
{
    private const int SaltSize = 16; // 128 bit
    private const int HashSize = 64; // 512 bit (SHA512)
    private const int Iterations = 100_000;
    private const byte Version = 1;

    public static string HashValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(value, salt, Iterations, HashAlgorithmName.SHA512, HashSize);

        // [0] = version, [1..16] = salt, [17..80] = hash
        byte[] hashBytes = new byte[1 + SaltSize + HashSize];
        hashBytes[0] = Version;
        Buffer.BlockCopy(salt, 0, hashBytes, 1, SaltSize);
        Buffer.BlockCopy(hash, 0, hashBytes, 1 + SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyHashedValue(string? hashedValue, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrEmpty(hashedValue)) return false;

        byte[] hashBytes;

        try
        {
            hashBytes = Convert.FromBase64String(hashedValue);
        }
        catch (FormatException)
        {
            return false;
        }

        if (hashBytes.Length != 1 + SaltSize + HashSize) return false;
        if (hashBytes[0] != Version) return false;

        byte[] salt = hashBytes[1..(1 + SaltSize)];
        byte[] hash = hashBytes[(1 + SaltSize)..];

        byte[] computedHash = Rfc2898DeriveBytes.Pbkdf2(value, salt, Iterations, HashAlgorithmName.SHA512, HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}
