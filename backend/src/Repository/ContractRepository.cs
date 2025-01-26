using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Repository;

public class ContractRepository : IContractRepository
{
    private readonly Connector _connector;

    // Constructor: initialiseer de connector voor databaseverbinding
    public ContractRepository(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    // Asynchrone methode om contracten op te halen die een e-mail moeten ontvangen
    public async Task<IList<int>> GetContractsSendEmailAsync()
    {
        // SQL-query om contracten op te halen die een e-mail moeten ontvangen en waarvoor de startdatum morgen is
        string query = $"SELECT OrderId FROM Contract WHERE (SendEmail = 'No' AND StartDate = '{DateTime.Today.AddDays(1):yyyy-MM-dd}')";

        Console.WriteLine(query);  // Log de query naar de console voor debugging

        try
        {
            // Maak verbinding met de database en voer de query uit
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                IList<int> orders = new List<int>();  // Lijst om de orderId's op te slaan

                // Lees de resultaten van de query
                while (await reader.ReadAsync())
                {
                    int orderId = Convert.ToInt32(reader.GetValue(0));  // Haal de OrderId op
                    Console.WriteLine(orderId);  // Log de orderId naar de console
                    orders.Add(orderId);  // Voeg de orderId toe aan de lijst
                }

                return orders;  // Retourneer de lijst met orders
            }
        }
        catch (MySqlException ex)
        {
            // Log de foutmelding bij een MySQL-specifieke fout
            Console.WriteLine(ex.Message);
            return new List<int>();  // Retourneer een lege lijst bij een fout
        }
        catch (OverflowException ex)
        {
            // Log de foutmelding bij een overflow-exceptie
            Console.WriteLine(ex.Message);
            return new List<int>();  // Retourneer een lege lijst bij een overflow
        }
        catch (Exception ex)
        {
            // Log de foutmelding bij een algemene fout
            Console.WriteLine(ex.Message);
            return new List<int>();  // Retourneer een lege lijst bij een andere fout
        }
    }

    // Asynchrone methode om gedetailleerde informatie op te halen van een specifiek contract
    public async Task<Dictionary<string, object>> GetContractInfoAsync(int orderId)
    {
        // SQL-query om contractinformatie op te halen op basis van OrderId
        string query = $"SELECT * FROM Contract WHERE OrderId = {orderId}";
        
        Dictionary<string, object> info = new Dictionary<string, object>();  // Dictionary om contractinformatie op te slaan

        try
        {
            // Maak verbinding met de database en voer de query uit
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                // Lees de resultaten van de query
                while (await reader.ReadAsync())
                {
                    // Loop door alle kolommen en voeg de naam en waarde toe aan de info-dictionary
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        info[reader.GetName(i)] = reader.GetValue(i);  // Sla de kolomnaam en waarde op in de dictionary
                    }
                }

                return info;  // Retourneer de verzamelde informatie
            }
        }
        catch (MySqlException ex)
        {
            // Log de foutmelding bij een MySQL-specifieke fout
            Console.WriteLine(ex.Message);
            return new Dictionary<string, object>();  // Retourneer een lege dictionary bij een fout
        }
        catch (Exception ex)
        {
            // Log de foutmelding bij een andere fout
            Console.WriteLine(ex.Message);
            return new Dictionary<string, object>();  // Retourneer een lege dictionary bij een andere fout
        }
    }
}
