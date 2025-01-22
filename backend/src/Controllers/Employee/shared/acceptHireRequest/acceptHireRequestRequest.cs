namespace WPR.Controllers.Employee.Shared.acceptHireRequest;

/// <summary>
/// Body voor het accepteren / weigeren van een huuraanvraag
/// </summary>
public class acceptHireRequestRequest
{
    int VehcileId { get; set; }
    int CustomerId { get; set; }
}