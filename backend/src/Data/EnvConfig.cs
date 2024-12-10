using dotenv.net; // library to load .env files
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace WPR.Data
{
    /// <summary>
    /// Reads the .env file where database connection strings are stored.
    /// These are stored in a separate file outside the git repo,
    /// so external parties cannot connect to the database.
    /// </summary>
    public class EnvConfig
    {
        private readonly IConfiguration _configuration;

        public EnvConfig(IConfiguration configuration)
        {
            // Load .env file using dotenv.net library
            DotEnv.Load();

            // Use the provided IConfiguration instance to access environment variables
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Fetches the value of the specific environment variable.
        /// </summary>
        /// <param name="key">The environment variable key to fetch.</param>
        /// <returns>The value of the environment variable, or null if not found.</returns>
        public string Get(string key)
        {
            var value = _configuration[key]; // Fetch the value of the given key from IConfiguration
            if (string.IsNullOrEmpty(value)) 
            {
                // Log a warning if the key is not found in the configuration
                Console.WriteLine($"Warning: Environment variable {key} not found.");
            }
            return value;
        }

        /// <summary>
        /// Checks if all required environment variables are configured.
        /// </summary>
        /// <returns>True if all keys are present, otherwise false.</returns>
        public bool IsConfigured()
        {
            // List of required environment variables for database connection
            var requiredKeys = new[] { "DB_SERVER", "DB_DATABASE", "DB_USERNAME", "DB_PASSWORD" };

            // Check if all required keys are present and not empty
            return requiredKeys.All(key => !string.IsNullOrEmpty(Get(key)));
        }
    }
}
