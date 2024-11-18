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
            string query = "SELECT LOWER(Email) = LOWER(@E) FROM User_Customer";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@E", email);

                using (var reader = command.ExecuteReader())
                {
                    return (!reader.HasRows, "Succesfull query");
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

    private (bool status, string message, int newUserID) addCustomer(IDbConnection connection, Object[] personData)
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
                    command.CommandText = "SELECT LAST_INSERT_ID();";
                    int newUserID = Convert.ToInt32(command.ExecuteScalar());
                    return (true, "Data Inserted", newUserID);
                }
                return (false, "No Data Inserted", -1);
            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (false, ex.Message, -1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (false, ex.Message, -1);
        }
    }

    private (int id, string message) getIdUser(IDbConnection connection, string email)
    {
        try
        {
            string query = "SELECT ID FROM User_Customer WHERE LOWER(Email) = LOWER(@E)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@E", email);

                using (var reader = command.ExecuteReader())
                {
                    return (reader.GetInt32(0), "Succesfull obtained ID");
                }

            }
        }

        catch (MySqlException ex)
        {
            Console.Error.WriteLine($"Database error: {ex.Message}");
            return (-1, ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            return (-1, ex.Message);
        }
    }

    private (bool status, string message) addPersonalCustomer(IDbConnection connection, Object[] personalData)
    {
        try
        {
            string query = "INSERT INTO Personal (ID, BirthDate) values (@I, @B)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", personalData[0]);
                command.Parameters.AddWithValue("@B", personalData[1]);

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
    
    private (bool status, string message) addEmployeeCustomer(IDbConnection connection, Object[] employeeData)
    {
        try
        {
            string query = "INSERT INTO Employee (ID, Business) values (@I, @B)";

            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", employeeData[0]);
                command.Parameters.AddWithValue("@B", employeeData[1]);

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


    [HttpPost("signUpPersonal")]
    public IActionResult signUpPersonal([FromBody] SignUpRequest signUpRequest)
    {
        var connection = _connector.CreateDbConnection();
        bool commit = true;

        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                if (signUpRequest == null
                || string.IsNullOrEmpty(signUpRequest.Email)
                || string.IsNullOrEmpty(signUpRequest.Password)
                || string.IsNullOrEmpty(signUpRequest.FirstName)
                || string.IsNullOrEmpty(signUpRequest.LastName)
                || signUpRequest.TelNumber == null
                || signUpRequest.BirthDate == null)
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                else if (checkUsageEmail(connection, signUpRequest.Email).status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email allready existing"} );
                }
                else if (signUpRequest != null 
                && !string.IsNullOrEmpty(signUpRequest.Email) 
                && !string.IsNullOrEmpty(signUpRequest.Password)
                && !string.IsNullOrEmpty(signUpRequest.FirstName) 
                && !string.IsNullOrEmpty(signUpRequest.LastName) 
                && signUpRequest.TelNumber != null
                && signUpRequest.BirthDate != null)
                {
                    var customer = addCustomer(connection, new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        signUpRequest.Password,
                        signUpRequest.Email
                    });

                    var personal = addPersonalCustomer(connection, new object[] 
                    {
                        customer.newUserID, 
                        signUpRequest.BirthDate
                    });

                    if (!personal.status)
                    {
                        transaction.Rollback();
                        commit = false;

                        return BadRequest(new { personal.message });
                    }
                    return Ok(new { personal.message });
                }
                else
                {
                    return BadRequest(new { message = "Something went wrong (Nothing mathced)" });
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                commit = false;

                return StatusCode(500, ex);
            }
            finally
            {
                if (commit)
                {
                    transaction.Commit();
                }

                connection.Close();
            }
        }
    }

    [HttpPost("signUpEmployee")]
    public IActionResult signUpEmployee([FromBody] SignUpRequest signUpRequest)
    {
        var connection = _connector.CreateDbConnection();
        bool commit = true;

        using (var transaction = connection.BeginTransaction())
        {
            try
            {
                if (signUpRequest == null
                || string.IsNullOrEmpty(signUpRequest.Email)
                || string.IsNullOrEmpty(signUpRequest.Password)
                || string.IsNullOrEmpty(signUpRequest.FirstName)
                || string.IsNullOrEmpty(signUpRequest.LastName)
                || signUpRequest.TelNumber == null
                || signUpRequest.KvK == null)
                {
                    return BadRequest(new { message = "Not all elements are filled in" });
                }
                else if (checkUsageEmail(connection, signUpRequest.Email).status)
                {
                    transaction.Rollback();
                    commit = false;

                    return BadRequest( new { message = "Email allready existing"} );
                }
                else if (signUpRequest != null
                && !string.IsNullOrEmpty(signUpRequest.Email)
                && !string.IsNullOrEmpty(signUpRequest.Password)
                && !string.IsNullOrEmpty(signUpRequest.FirstName)
                && !string.IsNullOrEmpty(signUpRequest.LastName)
                && signUpRequest.TelNumber != null
                && signUpRequest.KvK != null)
                {
                    var customer = addCustomer(connection, new object[] 
                    {
                        signUpRequest.Adres,
                        signUpRequest.TelNumber,
                        signUpRequest.Password,
                        signUpRequest.Email
                    });
                    var employee = addEmployeeCustomer(connection,new object[] 
                    {
                        customer.newUserID,
                        signUpRequest.KvK
                    });

                    if (!employee.status)
                    {
                        transaction.Rollback();
                        commit = false;

                        return BadRequest(new { employee.status });
                    }


                    return Ok(new { employee.status });
                }
                else
                {
                    return BadRequest(new { message = "Something went wrong (Nothing mathced)" });
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                commit = false;
                
                return StatusCode(500, ex);
            }
            finally
            {
                if (commit)
                {
                    transaction.Commit();
                }

                connection.Close();
            }
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

