namespace WPR.Controllers.Employee.FrontOffice.AddIntake;

/// <summary>
/// Body voor het toevoegen van een inname
/// </summary>
public class AddIntakeRequest
{
    public string? Damage { get; set;}
    public int FrameNrVehicle { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime Date { get; set; }
    public int Contract { get; set; }
    public bool IsDamaged { get; set; }
}