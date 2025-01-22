<<<<<<<< HEAD:backend/src/Controllers/customer/changeSettings/changeUserRequest.cs
namespace WPR.Controllers.Customer.ChangeUserSettings;
/// <summary>
/// Body voor het wijzigen van gebruikergegevens
/// </summary>
public class ChangeUserRequest
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string TelNum { get; set; }
    public string? Adres { get; set; }
}
========
>>>>>>>> dac20cd9d3ed25f95e8a45330a187dfe91e3cce8:backend/src/Controllers/ChangeSettings/changeUserRequest.cs
