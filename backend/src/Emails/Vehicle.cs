using WPR.Repository;

namespace WPR.Email;

public interface IVehicleDetails : IDetails {}
public class Vehicle : IVehicleDetails
{
    private Dictionary<string, object> _details { get; set; }
    private readonly IVehicleRepository _vehicleRepository;

    /// <summary>
    /// In de constructor worden alle benodigde klasses geset.
    /// </summary>
    /// <param name="vehicleRepository"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Vehicle(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    }
    
    /// <summary>
    /// Alle gegevens worden in de dictionary van het voertuig geset.
    /// </summary>
    /// <param name="frameNr"></param>
    /// <returns></returns>
    public async Task SetDetailsAsync(object frameNr)
    {
        var result = await _vehicleRepository.GetVehicleDataAsync(frameNr);

        _details = new Dictionary<string, object>();

        foreach (Dictionary<object, string> item in result)
        {

            if (!_details.ContainsKey((string)item.Keys.First()))
            {
                _details[(string)item.Keys.First()] = item.Values.First();
            }
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