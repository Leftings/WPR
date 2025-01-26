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