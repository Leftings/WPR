namespace Employee.Controllers.AddVehicle;

using Microsoft.AspNetCore.Mvc;
using Employee.Database;
using System;
using MySqlX.XDevAPI.Common;
using Employee.Repository;
using MySql.Data.MySqlClient;

[Route("api/[controller]")]
[ApiController]
public class AddVehicleController : ControllerBase
{
    private readonly Connector _connector;
    private readonly IUserRepository _userRepository;

    public AddVehicleController(Connector connector, IUserRepository userRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpPost("addVehicle")]
    public async Task<(bool status, string message)> AddVehicleAsync(int yop, string brand, string type, string licensePlate, string color, string sort, double price, string description, IFormFile vehicleBlob)
    {
        try
        {
            byte[] vehicleBlobBytes = null;
            
            // Read the file into a byte array
            if (vehicleBlob != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await vehicleBlob.CopyToAsync(memoryStream);
                    vehicleBlobBytes = memoryStream.ToArray();
                }
            }

            string query = "INSERT INTO Vehicle (YoP, Brand, Type, LicensePlate, Color, Sort, Price, Description, Vehicleblob) VALUES (@Y, @B, @T, @L, @C, @S, @P, @D, @V)";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@Y", yop);
                command.Parameters.AddWithValue("@B", brand);
                command.Parameters.AddWithValue("@T", type);
                command.Parameters.AddWithValue("@L", licensePlate);
                command.Parameters.AddWithValue("@C", color);
                command.Parameters.AddWithValue("@S", sort);
                command.Parameters.AddWithValue("@P", price);
                command.Parameters.AddWithValue("@D", description);
                
                // Add the byte array of the image
                command.Parameters.AddWithValue("@V", vehicleBlobBytes);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    return (true, "Vehicle inserted");
                }
                return (false, "Something went wrong while inserting the vehicle");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }
}