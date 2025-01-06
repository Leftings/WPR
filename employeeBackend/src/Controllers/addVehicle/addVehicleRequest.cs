namespace Employee.Controllers.AddVehicle;

/// <summary>
/// Body voor het toevoegen van een voertuig
/// </summary>
public class AddVehicleRequest
{
    public int YoP { get; set; }
    public string? Brand { get; set;}
    public string? Type { get; set; }
    public string? LicensePlate { get; set; }
    public string? Color { get; set; }
    public string? Sort { get; set; }
    public double Price { get; set; }
    public string? Description { get; set; }
    public int Places { get; set; }

}