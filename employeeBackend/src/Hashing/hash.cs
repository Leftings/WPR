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

    // Het creëren van een salt
    public byte[] createSalt()
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // de bites worden naar bytes omgezet
        return salt;
    }
    
    // Er wordt een hash toegepast op het meegegeven wachtwoord
    public string createHash(string password)
    {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password!, // Het wachtwoord
            salt: _salt, // Salt wordt toegevoegd aan de hash
            prf: KeyDerivationPrf.HMACSHA256, // Er wordt gebruik gemaakt van het HMACSHA256 hash algoritme
            iterationCount: 100000, // Hoeveel itteraties het algoritme draaid
            numBytesRequested: 256 / 8)); // zet de 256 bites om naar 32 bytes

        return hashed;
    }
}