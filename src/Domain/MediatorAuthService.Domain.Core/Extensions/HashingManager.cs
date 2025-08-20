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

        using var rng = RandomNumberGenerator.Create();
        byte[] salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(value, salt, Iterations, HashAlgorithmName.SHA512);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // [0] = version, [1..16] = salt, [17..80] = hash
        byte[] hashBytes = new byte[1 + SaltSize + HashSize];
        hashBytes[0] = Version;
        Buffer.BlockCopy(salt, 0, hashBytes, 1, SaltSize);
        Buffer.BlockCopy(hash, 0, hashBytes, 1 + SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyHashedValue(string hashedValue, string value)
    {
        if (hashedValue is null) return false;
        ArgumentNullException.ThrowIfNull(value);

        byte[] hashBytes = Convert.FromBase64String(hashedValue);

        if (hashBytes.Length != 1 + SaltSize + HashSize) return false;
        if (hashBytes[0] != Version) return false;

        byte[] salt = new byte[SaltSize];
        Buffer.BlockCopy(hashBytes, 1, salt, 0, SaltSize);

        byte[] hash = new byte[HashSize];
        Buffer.BlockCopy(hashBytes, 1 + SaltSize, hash, 0, HashSize);

        using var pbkdf2 = new Rfc2898DeriveBytes(value, salt, Iterations, HashAlgorithmName.SHA512);
        byte[] computedHash = pbkdf2.GetBytes(HashSize);

        return CryptographicOperations.FixedTimeEquals(hash, computedHash);
    }
}