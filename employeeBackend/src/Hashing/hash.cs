using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Employee.Data;

namespace Employee.Hashing;

public class Hash
{
    private byte[]? _salt { get; set; }
    public Hash(EnvConfig envConfig)
    {
        _salt = Convert.FromBase64String(envConfig.Get("SALT"));
    }

    public byte[] createSalt()
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
        return salt;
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
}