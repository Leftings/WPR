using Microsoft.AspNetCore.Mvc;
using WPR.Controllers.Cookie;
using WPR.Cryption;
using WPR.Database;
using WPR.Repository;
using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using System;
using WPR.Repository;
using MySqlX.XDevAPI.Common;
using WPR.Cryption;


namespace WPR.Controllers.Staff;

[Route("api/[controller]")]
[ApiController]
public class StaffController
{
    private readonly IUserRepository _userRepository;
    private readonly Connector _connector;
    private readonly SessionHandler _sessionHandler;
    private readonly Crypt _crypt;

    public StaffController(IUserRepository userRepository, Connector connector, SessionHandler sessionHandler, Crypt crypt)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _sessionHandler = sessionHandler ?? throw new ArgumentNullException(nameof(sessionHandler));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    [HttpGet("ValidateStaffId")]
    public Task<IActionResult> ValidateStaffIdAsync(string id)
    {
        string loginCookie = HttpContext.Request.Cookies["LoginSession"];



        using (var connection = _connector.CreateDbConnection())
        {
            return _userRepository.IsExistingStaffId(id);
        }
    }
}