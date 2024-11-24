using System.Data;

namespace WPR.Repository;

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

    Task<bool> ValidateUserAsync(string username, string password, bool isEmployee);
    Task<(bool status, string message)> checkUsageEmailAsync(IDbConnection connection, string email);
    Task<(bool status, string message, int newUserID)> addCustomerAsync(IDbConnection connection, Object[] personData);
    Task<(bool status, string message)> addPersonalCustomerAsync(IDbConnection connection, Object[] personalData);
    Task<(bool status, string message)> addEmployeeCustomerAsync(IDbConnection conenction, Object[] employeeData);
    Task<int> GetUserIdAsync(IDbConnection connection, string email);
    Task<string> GetUserNameAsync(IDbConnection connection, string userId);
    Task<bool> EditUserInfoAsync(IDbConnection connection, List<object[]> data);

}