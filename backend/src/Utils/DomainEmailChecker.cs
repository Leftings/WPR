using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Utils;

public class DomainEmailChecker
{
    private readonly Connector _connector;

    // Constructor voor het initialiseren van de connector (databaseverbinding)
    public DomainEmailChecker(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector)); // Controleer op null
    }

    // Methode om te controleren of een domein bestaat in de database
    public async Task<(bool Found, int KvK)> DomainExists(string email)
    {
        // Haal het domein op uit het e-mailadres (alles na de @)
        string domain = $"@{email.Split("@").Last()}";
        Console.WriteLine(domain); // Log het domein voor debuggen

        // SQL-query om de domeinen en KvK-gegevens op te halen uit de Business-tabel
        string query = "SELECT Domain, KvK FROM Business";

        // Maak verbinding met de database en voer de query uit
        using (var connection = _connector.CreateDbConnection()) // Maak de verbinding met de database
        using (var command = new MySqlCommand(query, (MySqlConnection)connection)) // Maak de query opdracht
        using (var reader = await command.ExecuteReaderAsync()) // Voer de query uit en lees het resultaat
        {
            // Lees door de resultaten
            while (await reader.ReadAsync())
            {
                // Vergelijk het domein met de waarde in de database
                if (((string)reader.GetValue(0)).Equals(domain))
                {
                    // Domein gevonden, retourneer true met de KvK-waarde
                    return (true, Convert.ToInt32(reader.GetValue(1)));
                }
            }

            // Domein niet gevonden, retourneer false met KvK 0
            return (false, 0);
        }
    }
}