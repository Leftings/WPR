using dotenv.net;
using System;

namespace WPR.Data;

public class Environment
{
    private readonly IConfiguration _configuration;
    
    public Environment()
    {
        DotEnv.Load();

        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();
        
        _configuration = builder.Build();
    }

    public string Get(string key)
    {
        var value = _configuration[key];
        if (string.IsNullOrEmpty(value)) 
        {
            Console.WriteLine($"Warning {key} not found in environment");
        }
        return value;
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(Get("DB_SERVER")) &&
               !string.IsNullOrEmpty(Get("DB_DATABASE")) &&
               !string.IsNullOrEmpty(Get("DB_USERNAME")) &&
               !string.IsNullOrEmpty(Get("DB_PASSWORD"));
    }
}