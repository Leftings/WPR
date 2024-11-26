using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using WPR.Data;
using Xunit;

/*public class LoginControllerIntegrationTests : IClassFixture<WebApplicationFactory<WPR.Program>>  // or the appropriate Startup class
{
    private readonly HttpClient _client;

    public LoginControllerIntegrationTests(WebApplicationFactory<WPR.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var loginRequest = new LoginRequest
        {
            email = "Customer", 
            Password = "Customer",      
            isEmployee = false               
        };

        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Login/login", content);

        response.EnsureSuccessStatusCode(); 

        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var loginRequest = new LoginRequest
        {
            email = "Staff", 
            Password = "Customer",
            isEmployee = true
        };

        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/Login/login", content);

        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid credentials", responseString);
    }
}
*/
