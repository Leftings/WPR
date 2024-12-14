using System.Data;
using System.Diagnostics;

namespace Employee.Repository;

/// <summary>
/// Interface defining methods voor user gerelateerde database operaties,
/// validatie, user management, data retrieval.
///
/// Het Repository Pattern abstraheert de logica voor data toegang en zorgt voor gecentraliseerde
/// en consitente interface voor interactie met de database.
/// Dit is goed voor: testbaarheid, onderhoudbaarheid en scheiding van verantwoordelijkheden binnen de applicatie.
/// </summary>
public interface IUserRepository
{
    Task<(bool status, string message)> AddVehicleAsync(int yop, string brand, string type, string licensPlate, string color, string sort, double price, string description, byte[] vehicleBlob);
    Task<(bool status, string message)> AddStaff(Object[] personData);
    Task<(bool status, string message)> checkUsageEmailAsync(string email);
    Task<(bool status, List<string> ids)> GetReviewIdsAsync(string user, string userId);
    Task<(bool status, List<Dictionary<string, object>> data)> GetReviewAsync(string id);
    Task<(bool status, string message)> SetStatusAsync(string id, string status, string employee, string userType);
}