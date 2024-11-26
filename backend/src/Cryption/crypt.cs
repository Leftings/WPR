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
    public string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "Nothing to encrypt";
        }

        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _IV;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(text);
                }

                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    public string Decrypt(string encrypted)
    {
        if (string.IsNullOrEmpty(encrypted))
        {
            return "Nothing to decrypt";
        }

        using (Aes aes = Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _IV;

            byte[] buffer = Convert.FromBase64String(encrypted);

            using (MemoryStream ms = new MemoryStream(buffer))
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
            using (StreamReader sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }
}