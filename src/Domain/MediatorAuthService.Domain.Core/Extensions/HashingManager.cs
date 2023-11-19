using System.Security.Cryptography;

namespace MediatorAuthService.Domain.Core.Extensions;

public static class HashingManager
{
    public static string HashValue(string value)
    {
        byte[] salt;
        byte[] buffer2;

        ArgumentNullException.ThrowIfNull(value);

        using (Rfc2898DeriveBytes bytes = new(value, 0x10, 0x3e8))
        {
            salt = bytes.Salt;
            buffer2 = bytes.GetBytes(0x20);
        }
        byte[] dst = new byte[0x31];
        Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
        Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
        return Convert.ToBase64String(dst);
    }

    public static bool VerifyHashedValue(string hashedValue, string value)
    {
        byte[] buffer4;
        if (hashedValue is null) return false;

        ArgumentNullException.ThrowIfNull(value);

        byte[] src = Convert.FromBase64String(hashedValue);

        if ((src.Length != 0x31) || (src[0] != 0)) return false;

        byte[] dst = new byte[0x10];
        Buffer.BlockCopy(src, 1, dst, 0, 0x10);
        byte[] buffer3 = new byte[0x20];
        Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
        using (Rfc2898DeriveBytes bytes = new(value, dst, 0x3e8))
        {
            buffer4 = bytes.GetBytes(0x20);
        }
        return buffer3.SequenceEqual(buffer4);
    }
}