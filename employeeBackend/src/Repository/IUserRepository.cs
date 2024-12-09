using System.Data;

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
    Task<(bool status, string message)> AddVehicleAsync(int yop, string brand, string type, string licensPlate, string color, string sort, double price, string description, string vehicleBlob);
}