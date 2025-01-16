using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace WPR.Controllers.SignUp;

/// <summary>
/// Body voor SignUpRequest
/// </summary>
public class SignUpRequest()
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string TelNumber { get; set; }
    [Required]
    public string Adres { get; set; }
    [Required]
    public DateTime BirthDate { get; set; }
}