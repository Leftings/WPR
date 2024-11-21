using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Crypto.Prng;
using WPR.Data;
using WPR.Database;
using WPR.Repository;

namespace WPR;

public class AppConfigure
{
    public static void InitDatabase(IServiceProvider services)
    {
        var dbConnector = services.GetRequiredService<Connector>();
        
        try
        {
            using (var connection = dbConnector.CreateDbConnection())
            {
                Console.WriteLine("Database connection established");
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to establish database connection");
            Console.WriteLine(e.StackTrace);
        }
        
    }
    
    public static WebApplication ConfigureApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var cookiePolicyOptions = new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Strict
        };

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowLocalhost", policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .AllowAnyMethod();
            });
        });


        builder.Services.AddSingleton<EnvConfig>();
        builder.Services.AddTransient<Connector>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IResponseCookies>();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                options.SlidingExpiration = true;
                options.AccessDeniedPath = "/Forbidden/";
            });
        
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler =
                    ReferenceHandler.IgnoreCycles;
            });
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });
        }
        
        app.UseCors("AllowLocalhost");
        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseAuthorization();
        app.UseAuthentication();
        app.UseCookiePolicy(cookiePolicyOptions);

        app.MapRazorPages();
        app.MapDefaultControllerRoute();

        return app;
    }
}