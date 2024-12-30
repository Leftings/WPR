namespace WPR.Cryption;

using System;
using System.IO;
using System.Security.Cryptography;
using WPR.Data;

public class Crypt
{
    private readonly EnvConfig _envConfig;
    private readonly byte[]  _key;
    private readonly byte[] _IV;
    public Crypt(EnvConfig envConfig)
    {
        _envConfig = envConfig;
        _key = Convert.FromBase64String(_envConfig.Get("CRYPTION_KEY"));
        _IV = Convert.FromBase64String(_envConfig.Get("CRYPTION_IV"));
    }

    // De meegegeven text wordt geencrypt
    public string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "Nothing to encrypt";
        }

        // Er wordt een nieuw Advanced Encryption Standard (AES) aangemaakt
        using (Aes aes = Aes.Create())
        {
            aes.Key = _key; // 256-bit sleutel
            aes.IV = _IV; // 128-bit IV, waardoor dezelfde tekst op verschillend geencrypt kan worden

            // Meegegevens text wordt tijdelijk in het geheugen gebruikt en maakt van de geencrypte tekst een Base64 string
            using (MemoryStream ms = new MemoryStream())
            {
                // De Crypto Stream zorgt ervoor dat er geencrypt kan worden
                // De Stream Write zorgt ervoor dat de de tekst naar de Crypto Stream wordt geschreven
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(text);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // De meegegeven encryptie wordt gedecrypt
    public string Decrypt(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
        {
            return "Nothing to decrypt";
        }

        // Er wordt een nieuw Advanced Encryption Standard (AES) aangemaakt
        using (Aes aes = Aes.Create())
        {
            aes.Key = _key; // 256-bit sleutel
            aes.IV = _IV; // 128-bit IV, waardoor dezelfde tekst op verschillend geencrypt kan worden

            byte[] buffer = Convert.FromBase64String(encrypted); // De gencrypte tekst wordt omgezet in bytes

            // De Crypto Stream zorgt ervoor dat er gedecrypt kan worden
            // De Stream Write zorgt ervoor dat de de tekst naar de Crypto Stream wordt geschreven
            using (MemoryStream ms = new MemoryStream(buffer))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}