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
    Task<(bool status, string message)> checkUsageEmailAsync(string email);
    Task<(bool status, string message, int newUserID)> addCustomerAsync(Object[] personData);
    Task<(bool status, string message)> addPersonalCustomerAsync(Object[] personalData);
    Task<(bool status, string message)> addEmployeeCustomerAsync(Object[] employeeData);
    Task<int> GetUserIdAsync(string email);
    Task<string> GetUserNameAsync(string userId);
    Task<bool> EditUserInfoAsync(List<object[]> data);

}