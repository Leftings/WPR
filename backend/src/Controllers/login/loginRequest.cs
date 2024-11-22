namespace WPR.Controllers.Login;

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsEmployee { get; set; }
}
