namespace WPR.SignUp;

using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using WPR.Data;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Microsoft.VisualBasic;
using System.Diagnostics.Tracing;

[Route("api/[controller]")]
[ApiController]
public class SignUpController : ControllerBase
{
    private readonly Connector _connector;

    public SignUpController(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    private (bool status, string message) checkUsageEmail(IDbConnection connection, string email)
    {
        try
        {
            string query = "SELECT LOWER(Email) = LOWER(@Email)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@Email", email);

                using (var reader = command.ExecuteReader())
                {
                    return (reader.HasRows, "Succesfull query");
                }

            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    private (bool status, string message) addCustomer(IDbConnection connection, Object[] personData)
    {
        try
        {
            string query = "INSERT INTO User_Customer (Adres, Telnum, Password, Email) values (@A, @T, @P, @E)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@A", personData[0]);
                command.Parameters.AddWithValue("@T", personData[1]);
                command.Parameters.AddWithValue("@P", personData[2]);
                command.Parameters.AddWithValue("@E", personData[3]);

                if (command.ExecuteNonQuery() > 0)
                {
                    return (true, "Data Inserted");
                }
                return (false, "No Data Inserted");
            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    private (bool satus, string message) personalCustomer(IDbConnection connection, Object[] personalData)
    {
        return (false, "X");
    }
    
    private (bool status, string message) employeeCustomer(IDbConnection connection, Object[] employeeData)
    {
        return (false, "X");
    }


    [HttpPost("signUp")]
    public IActionResult signUpPersonal([FromBody] SignUpRequest signUpRequest)
    {
        if (signUpRequest == null || string.IsNullOrEmpty(signUpRequest.Email) || string.IsNullOrEmpty(signUpRequest.Password)
        || string.IsNullOrEmpty(signUpRequest.FirstName) || string.IsNullOrEmpty(signUpRequest.LastName) || signUpRequest.TelNumber == null
        || signUpRequest.BirthDate == null)
        {
            return BadRequest(new { message = "Not all elements are filled in" });
        }

        var customer = addCustomer(_connector.CreateDbConnection(), new object[] {signUpRequest.Adres, signUpRequest.TelNumber, signUpRequest.Password, signUpRequest.Email});

        if (customer.status)
        {
            return Ok( new {message = customer.message});
        }
        else
        {
            return BadRequest( new {message = customer.message});
        }
    }
}

public class SignUpRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? TelNumber { get; set; }
    public string? Adres { get; set; }
    public DateTime? BirthDate { get; set; }
    public int? KvK { get; set; }
}

