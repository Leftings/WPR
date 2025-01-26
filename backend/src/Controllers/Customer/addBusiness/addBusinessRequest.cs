using System.ComponentModel.DataAnnotations;

namespace WPR.Controllers.Customer.AddBusiness;

/// <summary>
/// Body voor het toevoegen van een bedrijf
/// </summary>
public class AddBusinessRequest()
{
    [Required] public string? Subscription { get; set; }
    [Required]
    public int KvK { get; set; }
    [Required]
    public string? Name { get; set; }
    [Required]
    public string? Adress { get; set;}
    [Required]
    public string? Domain { get; set; }
    [Required]
    public string? ContactEmail { get; set; }
}