namespace WPR;
using WPR.Data;
using WPR.Database;

public class Program
{
    private static void configure(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register the CORS policy
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", policy =>
            {
                // Make sure this matches the URL of your React app
                policy.WithOrigins("http://localhost:5173") // Replace with your frontend URL
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();  // Allow credentials if needed (for cookies/sessions)
            });
        });

        // Add services to the container
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Enable Swagger UI in development environment
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
        }

        // Apply CORS policy globally
        app.UseCors("AllowLocalhost");

        // Configure the HTTP request pipeline
        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers(); // Map controller routes

        app.Run();
    }

    public static void Main(string[] args)
    {
        var envConfig = new EnvConfig();
        var dbConnector = new Connector(envConfig);

        try
        {
            using (var connection = dbConnector.CreateDbConnection())
            {
                Console.WriteLine("Connection established");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to establish connection");
        }

        configure(args);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins", builder =>
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Ensure this line uses the correct policy
        app.UseCors("AllowLocalhost");

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
