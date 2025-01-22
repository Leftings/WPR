namespace WPR.Controllers.Employee.BackOffice.signUpStaff;

/// <summary>
/// Body voor het aanmaken van een nieuw account
/// </summary>
public class SignUpStaffRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Job { get; set; }
    public int? KvK { get; set; }
}