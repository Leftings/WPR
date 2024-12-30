namespace Employee.Controllers.signUpStaff;

// Body voor het aanmaken van een nieuw account
public class SignUpRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Job { get; set; }
    public int? KvK { get; set; }
}