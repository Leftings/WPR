namespace WPR.Repository.DatabaseCheckRepository;

public interface IDatabaseCheckRepository
{
    public (int StatusCode, string Message) DeleteUser(int id);
    public (int StatusCode, string Message) DeleteBusiness(int kvk);
}