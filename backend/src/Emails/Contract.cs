using WPR.Repository;

namespace WPR.Email;

public interface IContractDetails : IDetails {}
public class Contract : IContractDetails
{
    private Dictionary<string, object> _details { get; set; }
    private readonly IContractRepository _contractRepository;

    public Contract(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
    }

    public async Task SetDetailsAsync(object orderId)
    {
        try
        {
            _details = await _contractRepository.GetContractInfoAsync(Convert.ToInt32(orderId));
        }
        catch (OverflowException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<Dictionary<string, object>> GetDetailsAsync()
    {
        return _details;
    }
}