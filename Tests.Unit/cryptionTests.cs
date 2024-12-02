namespace Tests.Unit
{
    using WPR.Cryption;
    using Xunit;
    using Test.Unit.Mocks;

    public class CryptionTest
    {
        private readonly EnvConfigMock _env;
        private readonly Crypt _crypt;
        public CryptionTest()
        {
            // Initialize the mocked EnvConfig
            _env = new EnvConfigMock();  // The mock should now return valid base64 strings for keys and IVs

            // Initialize Crypt with the mock EnvConfig
            _crypt = new Crypt(_env);
        }

        private string CreateTestData()
        {
            return "Test data";
        }

        private string CreateEncryption(string data)
        {
            return _crypt.Encrypt(data);
        }

        [Fact]
        public void RightEncryption()
        {
            string data = CreateTestData();

            // Encrypt the data
            string encrypted = CreateEncryption(data);

            // Assert that the encrypted value is different from the original data
            Assert.DoesNotMatch(data, encrypted);
        }

        [Fact]
        public void RightDecryption()
        {
            string data = CreateTestData();

            // Encrypt the data
            string encrypted = CreateEncryption(data);

            Assert.Equal(data, _crypt.Decrypt(encrypted));
        }

        [Fact]
        public void WrongDecryption()
        {
            string data = CreateTestData();


            string encrypted = CreateEncryption(data);
            Assert.DoesNotMatch("Wrong data", _crypt.Decrypt(encrypted));
        }
    }
}
