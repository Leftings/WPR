namespace Test.Unit.Mocks
{
    using System.Security.Cryptography;
    using WPR.Data;
    
    public class EnvConfigMock : EnvConfig
    {
        private string _base64Key;
        private string _base64IV;
        private string _salt;

        public EnvConfigMock()
        {
            SetEncryption();
            SetHash();
        }
        public override string Get(string key)
        {

            return key switch
                {
                    "CRYPTION_KEY" => _base64Key,
                    "CRYPTION_IV"  => _base64IV,
                    "SALT" => _salt,
                    _ => throw new ArgumentException("Invalid key", nameof(key)),
                };
        }

        private void SetEncryption()
        {
            using (Aes aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                _base64Key = Convert.ToBase64String(aes.Key);
                _base64IV = Convert.ToBase64String(aes.IV);
            }
        }

        private void SetHash()
        {
            _salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(128 / 8));
        }
    }
}
