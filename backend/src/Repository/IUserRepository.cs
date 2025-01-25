using System.Data;
using WPR.Controllers.customer.Subscription;
using WPR.Controllers.Employee.VehicleManager.ChangeBusinessSettings;
using WPR.Controllers.General.SignUp;

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
    Task<(bool status, string message, string officeType)> GetKindEmployeeAsync(string userId);
    Task<(bool Status, string Message)> AddPersonalCustomer(SignUpRequest request);
    Task<(int StatusCode, string Message)> AddCustomer(SignUpRequestCustomer request, SignUpRequestCustomerPrivate privateRequest);
    Task<(int StatusCode, string Message)> ChangeBusinessInfo(ChangeBusinessRequest request);
    Task<List<string>> GetAllSubscriptionsAsync();
    Task<Subscription>GetSubscriptionDataAsync(int id);
    Task<List<int>> GetSubscriptionIdsAsync();
    Task<UserRepository.VehicleManager> GetVehicleManagerInfoAsync(int id);
    Task<List<UserRepository.Customer>> GetCustomersByBusinessNumberAsync(string Business);
    Task<bool> UpdateCustomerAsync(int id, string email, string encryptedPassword);
    Task<(int StatusCode, string Message)> ChangeVehicleManagerInfo(ChangeVehicleManagerInfo request);

}