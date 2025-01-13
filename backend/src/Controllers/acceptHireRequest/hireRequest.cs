using System.ComponentModel.DataAnnotations;

namespace WPR.Controllers.acceptHireRequest;

/// <summary>
/// Body voor de HireRequest
/// </summary>
public class HireRequest
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string? Status { get; set; }
    public string userType { get; set; }
}