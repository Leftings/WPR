using Microsoft.AspNetCore.Mvc;
using WPR.Controllers.Cookie;
using WPR.Cryption;
using WPR.Database;
using WPR.Repository;

namespace WPR.Controllers.Employee;

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly Connector _connector;
    private readonly SessionHandler _sessionHandler;
    private readonly Crypt _crypt;

    public EmployeeController(IUserRepository userRepository, Connector connector, Crypt crypt, SessionHandler sessionHandler)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    // Er wordt gekeken of de user een medewerker is
    [HttpGet("IsUserEmployee")]
    public Task<bool> IsUserEmployee()
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];
        int id;
        
        if (string.IsNullOrEmpty(loginCookie))
        {
            return _userRepository.IsUserEmployee(-1);
        }

        try
        {
            id = Convert.ToInt32(_crypt.Decrypt(loginCookie));
            return _userRepository.IsUserEmployee(id);
        }
        catch
        {
            Response.Cookies.Append("LoginSession", "Invalid cookie", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(-1)
            });

            return _userRepository.IsUserEmployee(-1);
        }
    }
}