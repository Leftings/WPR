using System.Data;

namespace WPR.Repository;

public interface IUserRepository
{
    Task<bool> ValidateUserAsync(string username, string password, bool isEmployee);
    Task<(bool status, string message)> checkUsageEmailAsync(IDbConnection connection, string email);
    Task<(bool status, string message, int newUserID)> addCustomerAsync(IDbConnection connection, Object[] personData);
    Task<(bool status, string message)> addPersonalCustomerAsync(IDbConnection connection, Object[] personalData);
    Task<(bool status, string message)> addEmployeeCustomerAsync(IDbConnection conenction, Object[] employeeData);
    Task<int> GetUserIdAsync(IDbConnection connection, string email);

}