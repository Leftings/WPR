using System.Data;

namespace WPR.Controllers.General.Vehicle;

using Microsoft.AspNetCore.Mvc;
using WPR.Repository;
using MySql.Data.MySqlClient;
using System;
using WPR.Database;

/// <summary>
/// VehicleController zorgt ervoor dat voertuiggegevens uit de backend gehaald kunnen worden
/// </summary>
[Route("api/Vehicle")]
[ApiController]
public class VehicleController : ControllerBase
{

    private readonly IConnector _connector;
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleController(IConnector connector, IVehicleRepository vehicleRepository)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    }

    
    /// <summary>
    /// Haalt de merk en type op van een voertuig met het opgegeven FrameNr.
    /// </summary>
    /// <param name="frameNr">Het frame nummer van het voertuig.</param>
    /// <returns>Het merk en type van het voertuig of een foutmelding als het voertuig niet gevonden kan worden.</returns>
    [HttpGet("GetVehicleNameAsync")]
    public async Task<IActionResult> GetVehicleName(int frameNr)
    {
        try
        {
            // Query's om het merk en type van het voertuig op te halen.
            string query1 = "SELECT Brand FROM Vehicle WHERE FrameNr = @FrameNr";
            string query2 = "SELECT Type from Vehicle WHERE FrameNr = @FrameNr";

            // Verbinding maken met de database.
            using (var connection = _connector.CreateDbConnection())
            {
                string brand = "";
                string type = "";

                // Eerste query om het merk van het voertuig op te halen.
                using (var command = new MySqlCommand(query1, (MySqlConnection)connection))
                {
                    command.Parameters.AddWithValue("@FrameNr", frameNr);

                    using (MySqlDataReader reader1 = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {
                        // Als het merk gevonden wordt, sla het op in de 'brand' variabele.
                        if (reader1.Read() && !reader1.IsDBNull(0))
                        {
                            brand = reader1.GetString(0);
                        }
                    }
                }

                // Tweede query om het type van het voertuig op te halen.
                using (var command2 = new MySqlCommand(query2, (MySqlConnection)connection))
                {
                    command2.Parameters.AddWithValue("@FrameNr", frameNr);

                    using (MySqlDataReader reader2 = await command2.ExecuteReaderAsync(CommandBehavior.SingleRow))
                    {
                        // Als het type gevonden wordt, sla het op in de 'type' variabele en geef een succesvolle response.
                        if (reader2.Read() && !reader2.IsDBNull(0))
                        {
                            type = reader2.GetString(0);
                            return Ok($"{brand} {type}");
                        }

                        // Als geen type is gevonden, geef een foutmelding terug.
                        return NotFound($"Car with frameNr {frameNr} not found.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Foutafhandeling, geef een serverfoutmelding terug als er iets misgaat.
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while fetching the name of the car.");
        }
    }
    
    /// <summary>
    /// Haalt de afbeelding (in Base64) op van een voertuig met het opgegeven FrameNr.
    /// </summary>
    /// <param name="frameNr">Het frame nummer van het voertuig.</param>
    /// <returns>De afbeelding van het voertuig in Base64-indeling of een foutmelding als de afbeelding niet gevonden kan worden.</returns>
    [HttpGet("GetVehicleImageAsync")]
    public async Task<IActionResult> GetVehicleImageAsync(int frameNr)
    {
        try
        {
            // Query om de afbeelding (VehicleBlob) van het voertuig op te halen.
            string query = "SELECT VehicleBlob FROM Vehicle WHERE FrameNr = @FrameNr";

            // Verbinding maken met de database.
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@FrameNr", frameNr);

                using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    // Als de afbeelding is gevonden, converteer het naar Base64 en geef het terug.
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        byte[] imageBytes = (byte[])reader["VehicleBlob"];
                        string base64Image = Convert.ToBase64String(imageBytes);

                        return Ok(base64Image);
                    }
                    else
                    {
                        // Als geen afbeelding is gevonden, geef een foutmelding terug.
                        return NotFound($"Image with frameNr {frameNr} not found.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Foutafhandeling, geef een serverfoutmelding terug als er iets misgaat.
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while fetching the image.");
        }
    }

    /// <summary>
    /// Haalt de prijs op van een voertuig met het opgegeven FrameNr.
    /// </summary>
    /// <param name="frameNr">Het frame nummer van het voertuig.</param>
    /// <returns>De prijs van het voertuig in een formaat met twee decimalen of een foutmelding als de prijs niet gevonden kan worden.</returns>
    [HttpGet("GetVehiclePriceAsync")]
    public async Task<IActionResult> GetVehiclePriceAsync(int frameNr)
    {
        try
        {
            // Query om de prijs van het voertuig op te halen.
            string query = "SELECT Price FROM Vehicle WHERE FrameNr = @FrameNr";

            // Verbinding maken met de database.
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@FrameNr", frameNr);

                using (MySqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    // Als de prijs gevonden wordt, converteer het naar een formaat en geef het terug.
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        decimal priceDec = reader.GetDecimal(0);

                        // Prijs formatteren naar 2 decimalen
                        string price = priceDec.ToString("F2");

                        return Ok(price);
                    }
                    else
                    {
                        // Als de prijs niet gevonden is, geef een foutmelding terug.
                        return NotFound($"Price of car with frameNr {frameNr} not found.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Foutafhandeling, geef een serverfoutmelding terug als er iets misgaat.
            Console.WriteLine(ex);
            return StatusCode(500, "An error occurred while fetching the price.");
        }
    }

    /// <summary>
    /// Haalt alle voertuigen op uit de database en retourneert de gegevens in een lijst.
    /// </summary>
    /// <returns>Een lijst van voertuigen met details zoals FrameNr, Merk, Type, Prijs, etc.</returns>
    [HttpGet("GetAllVehicles")]
    public async Task<IActionResult> GetAllVehiclesAsync()
    {
        try
        {
            string query = @"
        SELECT FrameNr, YoP, Brand, Type, LicensePlate, Color, Sort, Price, VehicleBlob, Description, Seats
        FROM Vehicle";
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
                            FrameNr = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            YoP = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                            Brand = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Type = reader.IsDBNull(3) ? null : reader.GetString(3),
                            LicensePlate = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Color = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Sort = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Price = reader.IsDBNull(7) ? "0.00" : reader.GetDecimal(7).ToString("F2"),
                            Image = reader.IsDBNull(8) ? null : Convert.ToBase64String((byte[])reader["VehicleBlob"]),
                            Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Seats = reader.GetInt32(10)
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

    /// <summary>
    /// Haalt voertuigen op van een specifiek type (of alle types als "ALL" wordt opgegeven).
    /// </summary>
    /// <param name="vehicleType">Het type van voertuigen om op te filteren, zoals "SUV", "Sedan" etc.</param>
    /// <returns>Een lijst van voertuigen van het opgegeven type.</returns>
    [HttpGet("GetTypeOfVehicles")]
    public async Task<IActionResult> GetTypeOfVehiclesAsync(string vehicleType)
    {
        try
        {
            string query = @"
            SELECT FrameNr, YoP, Brand, Type, LicensePlate, Color, Sort, Price, VehicleBlob, Description, Seats 
            FROM Vehicle 
            WHERE LOWER(Sort) = LOWER(@Sort)";

            var vehicles = new List<object>();

            using (var connection = _connector.CreateDbConnection())
            {
                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    if (!string.IsNullOrWhiteSpace(vehicleType) &&
                        !vehicleType.Equals("ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        command.Parameters.AddWithValue("@Sort", vehicleType);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            vehicles.Add(new
                            {
                                FrameNr = reader.GetInt32(0),
                                YoP = reader.GetInt32(1),
                                Brand = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Type = reader.IsDBNull(3) ? null : reader.GetString(3),
                                LicensePlate = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Color = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Sort = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Price = reader.IsDBNull(7) ? null : reader.GetDecimal(7).ToString("F2"),
                                Image = reader.IsDBNull(8)
                                    ? null
                                    : Convert.ToBase64String((byte[])reader["VehicleBlob"]),
                                Description = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Seats = reader.GetInt32(10)
                            });
                        }
                    }
                }
            }

            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return StatusCode(500, "An error occurred while fetching vehicles.");
        }
    }

    /// <summary>
    /// Haalt alle framenummers van voertuigen op uit de database.
    /// </summary>
    /// <returns>Een lijst van framenummers van alle voertuigen.</returns>
    [HttpGet("GetFrameNumbers")]
    public async Task<IActionResult> GetFrameNumbersAsync()
    {
        var ids = await _vehicleRepository.GetFrameNumbersAsync();
        return Ok(new { message = ids });
    }

    /// <summary>
    /// Haalt de framenummers op van voertuigen van een specifiek type.
    /// </summary>
    /// <param name="type">Het type voertuig waarvoor de framenummers opgehaald moeten worden.</param>
    /// <returns>Een lijst van framenummers van voertuigen van het opgegeven type.</returns>
    [HttpGet("GetFrameNumbersSpecificType")]
    public async Task<IActionResult> GetFrameNumbersSpecificTypeAsync(string type)
    {
        var ids = await _vehicleRepository.GetFrameNumberSpecifiekTypeAsync(type);
        return Ok(new { message = ids });
    }

    /// <summary>
    /// Haalt alle gegevens van een specifiek voertuig op basis van het FrameNr.
    /// </summary>
    /// <param name="frameNr">Het frame nummer van het voertuig.</param>
    /// <returns>De gegevens van het voertuig, zoals merk, type, prijs, etc.</returns>
    [HttpGet("GetVehicelData")]
    public async Task<IActionResult> GetVehicleData(string frameNr)
    {
        try
        {
            var data = await _vehicleRepository.GetVehicleDataAsync(frameNr);

            if (data == null || !data.Any())
            {
                return NotFound(new VehicleErrorResponse
                {
                    Status = false,
                    Message = "No vehicle data found"
                });
            }
                
            return Ok(new VehicleDataResponse { Message = data });
        }
        catch (Exception e)
        {
            return StatusCode(500, new VehicleErrorResponse()
            {
                Status = false,
                Message = e.Message
            });
        }
    }

    /// <summary>
    /// Verwijdert een specifiek voertuig uit de database op basis van het FrameNr.
    /// </summary>
    /// <param name="frameNr">Het frame nummer van het voertuig dat verwijderd moet worden.</param>
    /// <returns>Een bericht over de status van het verwijderen van het voertuig.</returns>
    [HttpDelete("DeleteVehicle")]
    public async Task<IActionResult> DeleteVehicleAsync(string frameNr)
    {
        try
        {
            var result = await _vehicleRepository.DeleteVehicleAsync(frameNr);

            if (result.Status)
            {
                return Ok(new { Status = true, Message = result.Message });
            }
            else
            {
                return BadRequest(new { Status = false, Message = result.Message });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}