using dotenv.net;
using System;

namespace WPR.Data;

public class EnvConfig
{
    private readonly IConfiguration _configuration;
    
    public EnvConfig()
    {
        DotEnv.Load();

        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        
        _configuration = builder.Build();
    }

    public virtual string Get(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrEmpty(value)) 
        {
            Console.WriteLine($"Warning {key} not found in environment");
        }
        return value;
    }

    public virtual bool IsConfigured()
    {
        var requiredKeys = new[] { "DB_SERVER", "DB_DATABASE", "DB_USERNAME", "DB_PASSWORD" };
        return requiredKeys.All(key => !string.IsNullOrEmpty(Get(key)));
    }
}