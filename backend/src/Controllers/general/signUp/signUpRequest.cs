using System.ComponentModel.DataAnnotations;

namespace WPR.Controllers.General.SignUp;

/// <summary>
/// Body voor SignUpRequest
/// </summary>
public class SignUpRequest()
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string AccountType { get; set; }
    public int KvK { get; set; }
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

public class CombinedSignUpRequest()
{
    public SignUpRequestCustomer SignUpRequestCustomer { get; set; }
    public SignUpRequestCustomerPrivate? SignUpRequestCustomerPrivate { get; set; }
}

public class SignUpRequestCustomer()
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string AccountType { get; set; }
    public int? KvK { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public bool IsPrivate { get; set; }
}

public class SignUpRequestCustomerPrivate()
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? TelNumber { get; set; }
    public string? Adres { get; set; }
    public DateTime? BirthDate { get; set; }
}