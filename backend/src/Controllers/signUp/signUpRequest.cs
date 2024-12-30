using System.Security.Policy;

namespace WPR.Controllers.SignUp;

// Er wordt een body aangemaakt voor alle gegevens voor het aanmaken van een account
public class SignUpRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string TelNumber { get; set; }
    public string? Adres { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? KvK { get; set; }
}