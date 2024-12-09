namespace WPR.Repository;

/// <summary>
/// De UserRepository class geeft de methodes voor de interactie met de database voor gebruiker gerelateerde operaties.
/// Het implementeert de IUserRepository interface.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Connector _connector;

    public UserRepository(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }
    
    // vehicleBlob is het pad naar de afbeelding
    public Task<bool> AddVehicleAsync(int yop, string brand, string type, string licensPlate, string color, string sort, double price, string description, string vehicleBlob)
    {
        string query = "INSERT INTO Vehicle (YoP, Brand, Type, LicensPlate, Color, Sort, Price, Description, Vehicleblob) AS (@Y, @B, @T, @L, @C, @S, @P, @D, @V)";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            
        }
    }
}