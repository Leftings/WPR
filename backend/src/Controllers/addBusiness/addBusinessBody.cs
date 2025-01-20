namespace Employee.Controllers.AddBusiness;

public class AddBusinessRequest()
{
    public int KvK { get; set; }
    public string? Name { get; set; }
    public string? Adress { get; set;}
}