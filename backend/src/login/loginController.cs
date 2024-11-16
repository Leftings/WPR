using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using WPR.Data;
using MySql.Data.MySqlClient;

namespace WPR.Login
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private Connector _connector;

        public LoginController()
        {
            _connector = new Connector(new EnvConfig());
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            using(var connection = _connector.CreateDbConnection())
            {
                string table = "";

                if (loginRequest.isEmployee)
                {
                    table = "Staff";
                }
                else
                {
                    table = "User_Customer";
                }
                

                string query = $"SELECT * FROM {table} WHERE email = @email AND password = @password";

                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@email", loginRequest.email);
                    command.Parameters.AddWithValue("@password", loginRequest.Password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            return Ok(new { message = "Login successful" });
                        }
                        else
                        {
                            return Unauthorized(new { message = "Invalid credentials" });
                        }
                    }
                }
            }
        }
    }

    public class LoginRequest
    {
        public string email { get; set; }
        public string Password { get; set; }
        public bool isEmployee { get; set; }
    }
}
