using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WPR.Repository;

namespace WPR.Email;

public interface IVehicleDetails : IDetails {}
public class Vehicle : IVehicleDetails
{
    private Dictionary<string, object> _details { get; set; }
    private readonly IVehicleRepository _vehicleRepository;

    public Vehicle(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    }
    
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
    public async Task<Dictionary<string, object>> GetDetailsAsync()
    {
        return _details;
    }
}