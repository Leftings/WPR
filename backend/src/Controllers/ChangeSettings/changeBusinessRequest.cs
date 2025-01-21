using System.ComponentModel.DataAnnotations;

namespace WPR.Controllers.ChangeBusinessSettings;

public class ChangeBusinessRequest
{   
    [Required]
    public ChangeVehicleManagerInfo VehicleManagerInfo { get; set; }
    [Required]
    public ChangeBusinessInfo BusinessInfo { get; set; }
}

public class ChangeVehicleManagerInfo
{
    public int ID { get; set; }
    public string? Password { get; set; }
}

public class ChangeBusinessInfo
{
    public int KvK { get; set; }
    public int Abonnement { get; set; }
}