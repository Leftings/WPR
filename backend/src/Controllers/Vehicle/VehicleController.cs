using System.Data;

namespace WPR.Controllers.Vehicle;

using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using MySql.Data.MySqlClient;
using System;
using WPR.Database;

[Route("api/Vehicle")]
[ApiController]
public class VehicleController : ControllerBase
{

    private readonly Connector _connector;

    public VehicleController(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    [HttpGet("GetVehicleNameAsync")]
    public async Task<IActionResult> GetVehicleName(int frameNr)
    {
        try
        {
            string query1 = "SELECT Brand FROM Vehicle WHERE FrameNr = @FrameNr";
            string query2 = "SELECT Type from Vehicle WHERE FrameNr = @FrameNr";

            using (var connection = _connector.CreateDbConnection())
            {
                string brand = "";
                string type = "";

                using (var command = new MySqlCommand(query1, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@FrameNr", frameNr);

                    using (MySqlDataReader reader1 = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {

                        if (reader1.Read() && !reader1.IsDBNull(0))
                        {
                            brand = reader1.GetString(0);
                        }

                    }
                }

                using (var command2 = new MySqlCommand(query2, (MySqlConnection)connection))
                {
                    command2.Parameters.AddWithValue("@FrameNr", frameNr);

                    using (MySqlDataReader reader2 = await command2.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {
                        if (reader2.Read() && !reader2.IsDBNull(0))
                        {
                            type = reader2.GetString(0);
                            return Ok($"{brand} {type}");
                        }

                        return NotFound($"Car with frameNr {frameNr} not found.");
                    }
                }


            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while fetching the name of the car.");
        }

    }

    [HttpGet("GetVehicleImageAsync")]
    public async Task<IActionResult> GetVehicleImageAsync(int frameNr)
    {
        try
        {
            string query = "SELECT VehicleBlob FROM Vehicle WHERE FrameNr = @FrameNr";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@FrameNr", frameNr);

                using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))

                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        byte[] imageBytes = (byte[])reader["VehicleBlob"];
                        string base64Image = Convert.ToBase64String(imageBytes);

                        return Ok(base64Image);
                    }
                    else
                    {
                        return NotFound($"Image with frameNr {frameNr} not found.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while fetching the image.");
        }

    }

    [HttpGet("GetVehiclePriceAsync")]
    public async Task<IActionResult> GetVehiclePriceAsync(int frameNr)
    {
        try
        {
            string query = "SELECT Price FROM Vehicle WHERE FrameNr = @FrameNr";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@FrameNr", frameNr);

                using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))

                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        decimal priceDec = reader.GetDecimal(0);
                        string price = priceDec.ToString();
                        return Ok(price);
                    }
                    else
                    {
                        return NotFound($"Price of car with frameNr {frameNr} not found.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while fetching the price.");
        }
    }

    [HttpGet("GetAllVehicles")]
    public async Task<IActionResult> GetAllVehiclesAsync(int frameNr)
    {
        try
        {
            string query = "SELECT FrameNr, Brand, Type, Price, VehicleBlob FROM Vehicle";

            var vehicles = new List<object>();

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        vehicles.Add(new
                        {
                            FrameNr = reader.GetInt32(0),
                            Brand = reader.GetString(1),
                            Type = reader.GetString(2),
                            Price = reader.GetDecimal(3).ToString("F2"),
                            Image = !reader.IsDBNull(4)
                            ? Convert.ToBase64String((byte[])reader["VehicleBlob"]) : null
                        });
                    }
                }
            }

            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while fetching vehicles.");
        }
    }
}