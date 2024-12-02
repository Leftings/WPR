using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using WPR.Data;

namespace WPR.Hashing;

public class Hash
{
    private byte[]? _salt { get; set; }

    public Hash(EnvConfig envConfig)
    {
        _salt = Convert.FromBase64String(envConfig.Get("SALT"));
    }

    public string createHash(string password)
    {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: _salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        Console.WriteLine($"Hashed: {hashed}");

        return hashed;
    }

    public bool checkHash(string password, string storedHash)
    {
        string rehashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!,
            salt: _salt!,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return rehashed == storedHash;
    }
}