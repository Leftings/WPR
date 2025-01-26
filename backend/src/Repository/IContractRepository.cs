namespace WPR.Repository;

public interface IContractRepository
{
    public Task<IList<int>> GetContractsSendEmailAsync();
    public Task<Dictionary<string, object>> GetContractInfoAsync(int OrderId);
}