using System.ComponentModel.DataAnnotations;

namespace WPR.Controllers.Employee.VehicleManager.ChangeBusinessSettings;

public class ChangeBusinessRequest
{   
    [Required]
    public ChangeVehicleManagerInfo? VehicleManagerInfo { get; set; }
    public ChangeBusinessInfo? BusinessInfo { get; set; }
}

public class ChangeVehicleManagerInfo
{
    public int ID { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
}

public class ChangeBusinessInfo
{
    public int KvK { get; set; }
    public int Abonnement { get; set; }
    public string? ContactEmail { get; set; }
    public string? Adres { get; set; }
    public string? BusinessName { get; set; }
}