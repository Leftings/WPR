namespace WPR.Repository;

public interface IUserRepository
{
    Task<bool> ValidateUserAsync(string username, string password, bool isEmployee);
}