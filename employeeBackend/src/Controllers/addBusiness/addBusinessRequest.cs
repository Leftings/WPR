namespace Employee.Controllers.AddBusiness;

/// <summary>
/// Body voor het toevoegen van een bedrijf
/// </summary>
public class AddBusinessRequest()
{
    public int KvK { get; set; }
    public string? Name { get; set; }
    public string? Adress { get; set;}
}