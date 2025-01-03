namespace WPR.Controllers.Login;

/// <summary>
/// Body voor LoginRequest
/// </summary>
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string UserType { get; set; }
}
