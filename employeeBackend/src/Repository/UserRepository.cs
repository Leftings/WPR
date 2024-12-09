using MySql.Data.MySqlClient;

namespace Employee.Repository;

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
    public async Task<(bool status, string message)> AddVehicleAsync(int yop, string brand, string type, string licensPlate, string color, string sort, double price, string description, string vehicleBlob)
    {
        try
        {
            string query = "INSERT INTO Vehicle (YoP, Brand, Type, LicensPlate, Color, Sort, Price, Description, Vehicleblob) AS (@Y, @B, @T, @L, @C, @S, @P, @D, @V)";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@Y", yop);
                command.Parameters.AddWithValue("@B", brand);
                command.Parameters.AddWithValue("@T", type);
                command.Parameters.AddWithValue("@L", licensPlate);
                command.Parameters.AddWithValue("@C", color);
                command.Parameters.AddWithValue("@S", sort);
                command.Parameters.AddWithValue("@P", price);
                command.Parameters.AddWithValue("@D", description);
                command.Parameters.AddWithValue("@V", vehicleBlob);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    return (true, "Vehicle inserted");
                }
                return (false, "Something went wrong while inserting the vehicle");
            }
        }
        catch ()
    }
}