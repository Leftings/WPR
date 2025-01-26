using WPR.Repository;

namespace WPR.Email;

public class Vehicle : IDetails
{
    // Dictionary om de voertuigdetails op te slaan
    private Dictionary<string, object> _details { get; set; }

    // Het repository object dat gebruikt wordt om de voertuiggegevens op te halen
    private readonly IVehicleRepository _vehicleRepository;

    // Constructor die het repository object ontvangt
    public Vehicle(IVehicleRepository vehicleRepository)
    {
        // Zorg ervoor dat het repository niet null is
        _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
    }
    
    // Methode om de voertuigdetails in te stellen op basis van het frame nummer (ID)
    public async Task SetDetailsAsync(object frameNr)
    {
        // Verkrijg de voertuiggegevens uit het repository
        var result = await _vehicleRepository.GetVehicleDataAsync(frameNr);

        // Initialiseer het _details dictionary
        _details = new Dictionary<string, object>();

        // Itereer door de verkregen resultaten en vul de _details dictionary
        foreach (Dictionary<object, string> item in result)
        {
            // Voeg de gegevens toe aan de dictionary als de sleutel nog niet bestaat
            if (!_details.ContainsKey((string)item.Keys.First()))
            {
                _details[(string)item.Keys.First()] = item.Values.First();
            }
        }
    }

    // Methode om de opgeslagen voertuigdetails op te halen
    public async Task<Dictionary<string, object>> GetDetailsAsync()
    {
        return _details;
    }
}