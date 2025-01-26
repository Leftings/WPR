using WPR.Repository;

namespace WPR.Email;

public interface IContractDetails : IDetails {}

/// <summary>
/// In de Contract klasse worden alle gegevens van een specifieke klant verzameld voor het aanmaken van een email.
/// </summary>
public class Contract : IContractDetails
{
    private Dictionary<string, object> _details { get; set; }
    private readonly IContractRepository _contractRepository;

    /// <summary>
    /// In de constructor worden alle benodigde klasses geset.
    /// </summary>
    /// <param name="contractRepository"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Contract(IContractRepository contractRepository)
    {
        _contractRepository = contractRepository ?? throw new ArgumentNullException(nameof(contractRepository));
    }

    /// <summary>
    /// Alle gegevens worden in de dictionary van het contract geset.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Alle gegevens van de klant worden via deze methode opgehaald.
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, object>> GetDetailsAsync()
    {
        return _details;
    }
}