using System.Data;
using WPR.Controllers.SignUp;

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

    Task<bool> ValidateUserAsync(string username, string password, string userType);
    Task<(bool status, string message)> checkUsageEmailAsync(string email);
    Task<(bool status, string message, int newUserID)> addCustomerAsync(Object[] personData);
    Task<(bool status, string message)> addPersonalCustomerAsync(Object[] personalData);
    Task<(bool status, string message)> addEmployeeCustomerAsync(Object[] employeeData);
    Task<string> GetUserIdAsync(string email, string table);
    Task<string> GetUserNameAsync(string userId);
    Task<(bool status, string message)> EditUserInfoAsync(List<object[]> data);
    Task<bool> IsKvkNumberAsync(int kvkNumber);
    Task<bool> IsUserEmployee(int id);
    Task<(bool status, string message)> DeleteUserAsync(string userId);
    Task<(bool status, string message)> GetKindEmployeeAsync(string userId);
    Task<(bool Status, string Message)> AddPersonalCustomer(SignUpRequest request);
}