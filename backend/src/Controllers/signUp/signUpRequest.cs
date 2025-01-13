using System.Security.Policy;

namespace WPR.Controllers.SignUp;

/// <summary>
/// Body voor SignUpRequest
/// </summary>
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