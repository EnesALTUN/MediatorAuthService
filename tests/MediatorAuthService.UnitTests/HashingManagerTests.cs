using MediatorAuthService.Domain.Core.Extensions;
using System.Security.Cryptography;

namespace MediatorAuthService.UnitTests;

public class HashingManagerTests
{
    [Fact]
    public void VerifyHashedValue_ReturnsTrue_ForTheOriginalValue()
    {
        string hash = HashingManager.HashValue("Passw0rd!");

        Assert.True(HashingManager.VerifyHashedValue(hash, "Passw0rd!"));
        Assert.False(HashingManager.VerifyHashedValue(hash, "passw0rd!"));
    }

    [Fact]
    public void HashValue_UsesARandomSalt()
    {
        Assert.NotEqual(HashingManager.HashValue("same"), HashingManager.HashValue("same"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-base64!")]
    [InlineData("AAAA")]
    public void VerifyHashedValue_ReturnsFalse_ForMalformedHashes(string? hashedValue)
    {
        Assert.False(HashingManager.VerifyHashedValue(hashedValue, "value"));
    }

    [Fact]
    public void VerifyHashedValue_AcceptsHashesCreatedByThePreviousImplementation()
    {
        // The previous implementation used the Rfc2898DeriveBytes instance API; stored passwords must keep working.
        byte[] salt = RandomNumberGenerator.GetBytes(16);
#pragma warning disable SYSLIB0060
        using Rfc2898DeriveBytes pbkdf2 = new("Legacy1!", salt, 100_000, HashAlgorithmName.SHA512);
#pragma warning restore SYSLIB0060
        byte[] hash = pbkdf2.GetBytes(64);

        byte[] hashBytes = [1, .. salt, .. hash];

        Assert.True(HashingManager.VerifyHashedValue(Convert.ToBase64String(hashBytes), "Legacy1!"));
    }
}
