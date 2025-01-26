using WPR.Database;
using MySql.Data.MySqlClient;

namespace WPR.Repository;

public class BackOfficeRepository(Connector connector) : IBackOfficeRepository
{
    private readonly Connector _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    private Exception? _exception;

    /// <summary>
    /// Alle benodigde gegevens van Abonnements tabel worden opgehaald
    /// (Dit kan in 1 keer gedaan worden, omdat er geen grote gegevens verstuurd worden)
    /// </summary>
    /// <returns></returns>
    private (bool status, Dictionary<string, object> row) GetFromDB(int id, bool fullInfo)
    {
        try
        {
            Dictionary<string, object> row = new Dictionary<string, object>();

            string query = "SELECT * FROM Contract WHERE OrderId = @I"; // SQL-query om gegevens op te halen

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", id); // Voeg de OrderId parameter toe

                using (var reader = command.ExecuteReader()) // Voer de query uit
                {
                    int columns = reader.FieldCount; // Aantal kolommen in het resultaat

                    while (reader.Read()) // Loop door de rijen
                    {
                        for (int col = 0; col < columns; col++) // Loop door de kolommen
                        {
                            string columnName = reader.GetName(col);
                            object columnData = reader.GetValue(col);

                            row[columnName] = columnData; // Voeg de kolom en waarde toe aan de dictionary

                            // Haal extra gegevens op, afhankelijk van de kolomnaam
                            if (columnName.Equals("Customer"))
                            {
                                if (fullInfo) // Haal gedetailleerde klantgegevens op als fullInfo waar is
                                {
                                    foreach (var item in GetFullPerson(columnData))
                                        row[item.Key] = item.Value;

                                    foreach (var item in GetBusinessInfo(columnData))
                                        row[item.Key] = item.Value;
                                }
                                else
                                {
                                    row["NameCustomer"] = GetName(columnData, "Private");
                                }
                            }
                            else if (columnName.Equals("ReviewedBy"))
                            {
                                row["NameEmployee"] = GetName(columnData, "Staff");
                            }
                            else if (columnName.Equals("FrameNrVehicle"))
                            {
                                if (fullInfo) // Haal volledige voertuiggegevens op als fullInfo waar is
                                {
                                    foreach (var item in GetFullVehicleData(columnData))
                                        row[item.Key] = item.Value;
                                }
                                else
                                {
                                    row["Vehicle"] = GetVehicleInfo(columnData);
                                }
                            }
                        }
                    }
                }
            }

            return (true, row); // Retourneer de gegevens als succesvol opgehaald

        }
        catch (MySqlException ex) // Fout bij databaseverbinding of query
        {
            _exception = ex;
            return (false, null);
        }
        catch (OverflowException ex) // Fout bij gegevensoverloop
        {
            _exception = ex;
            return (false, null);
        }
        catch (Exception ex) // Algemene foutafhandeling
        {
            _exception = ex;
            return (false, null);
        }
    }


    /// <summary>
    /// De naam van de medewerker of klant wordt via hun id opgevraagd
    /// </summary>
    /// <param name="id"></param>
    /// <param name="table"></param>
    /// <returns></returns>
    private object GetName(object id, string table)
    {
        try
        {
            // SQL-query om achternaam en voornaam op te halen op basis van de ID en de tabelnaam
            string query = $"SELECT LastName, FirstName From {table} WHERE ID = @I";

            using (var connection = _connector.CreateDbConnection()) // Maak verbinding met de database
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", id); // Voeg de ID-parameter toe

                using (var reader = command.ExecuteReader()) // Voer de query uit
                {
                    // Als er een resultaat is voor de gegeven ID, retourneer de naam
                    if (reader.Read())
                    {
                        return $"{reader.GetValue(0)}, {reader.GetValue(1)}"; // Formatteer als "Achternaam, Voornaam"
                    }

                    return null; // Geen resultaat gevonden
                }
            }
        }
        catch (MySqlException ex) // Fout bij databaseverbinding of query
        {
            _exception = ex;
            return null;
        }
        catch (Exception ex) // Algemene foutafhandeling
        {
            _exception = ex;
            return null;
        }
    }


    private Dictionary<string, object> GetBusinessInfo(object employee)
    {
        try
        {
            // Query om KvK-nummer van de klant op te halen op basis van de medewerker-ID
            string query = $"SELECT KvK From Customer WHERE ID = @I";
            object business = "";

            using (var connection = _connector.CreateDbConnection()) // Maak verbinding met de database
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", employee); // Voeg de medewerker-ID parameter toe

                using (var reader = command.ExecuteReader()) // Voer de query uit
                {
                    while (reader.Read())
                    {
                        business = reader.GetValue(0).ToString(); // Verkrijg het KvK-nummer
                    }
                }
            }

            // Query om bedrijfsgegevens op te halen op basis van KvK-nummer
            string query2 = "SELECT * FROM Business WHERE KvK = @K";
            using (var connection = _connector.CreateDbConnection()) // Maak verbinding met de database
            using (var command = new MySqlCommand(query2, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@K", business); // Voeg het KvK-nummer als parameter toe

                using (var reader = command.ExecuteReader()) // Voer de query uit
                {
                    Dictionary<string, object>
                        data = new Dictionary<string, object>(); // Maak een dictionary voor de bedrijfsgegevens

                    while (reader.Read()) // Loop door de resultaten van de bedrijfsgegevens
                    {
                        for (int i = 0; i < reader.FieldCount; i++) // Loop door de kolommen
                        {
                            // Als de kolomnaam "Adres" is, geef het een specifieke naam
                            if (reader.GetName(i).Equals("Adres"))
                            {
                                data["AdresBusiness"] = reader.GetValue(i);
                            }
                            else
                            {
                                data[reader.GetName(i)] = reader.GetValue(i); // Voeg de andere gegevens toe
                            }
                        }
                    }

                    return data; // Retourneer de verzamelde bedrijfsgegevens
                }
            }

        }
        catch (MySqlException ex) // Fout bij databaseverbinding of query
        {
            _exception = ex;
            return new Dictionary<string, object>(); // Retourneer een lege dictionary bij fout
        }
        catch (Exception ex) // Algemene foutafhandeling
        {
            _exception = ex;
            return new Dictionary<string, object>(); // Retourneer een lege dictionary bij fout
        }
    }

    private Dictionary<string, object> GetFullPerson(object id)
    {
        try
        {
            Dictionary<string, object> person = new Dictionary<string, object>();

            // SQL-query om gegevens op te halen uit de 'Private' en 'Customer' tabellen op basis van ID
            string queryPrivate = $"SELECT * From Private WHERE ID = @I";
            string queryCustomer = $"SELECT * FROM Customer WHERE ID = @I";

            using (var connection = _connector.CreateDbConnection()) // Maak verbinding met de database
            using (var commandPrivate = new MySqlCommand(queryPrivate, (MySqlConnection)connection))
            using (var commandCustomer = new MySqlCommand(queryCustomer, (MySqlConnection)connection))
            {
                commandPrivate.Parameters.AddWithValue("@I", id); // Voeg de ID-parameter voor 'Private' toe
                commandCustomer.Parameters.AddWithValue("@I", id); // Voeg de ID-parameter voor 'Customer' toe

                using (var reader = commandPrivate.ExecuteReader()) // Voer de 'Private' query uit
                {
                    // Als er gegevens zijn voor de opgegeven ID, voeg deze toe aan de dictionary
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++) // Loop door de kolommen
                        {
                            string columnName = reader.GetName(i);
                            object columnData = reader.GetValue(i);

                            person[columnName] = columnData; // Voeg de kolomnaam en gegevens toe aan de dictionary

                            // Als het de ID is, haal dan extra bedrijfsgegevens op
                            if (columnName.Equals("ID"))
                            {
                                foreach (var item in GetBusinessInfo(columnData)) // Haal bedrijfsinformatie op
                                {
                                    if (!person.ContainsKey(item
                                            .Key)) // Voeg bedrijfsinformatie toe als deze nog niet bestaat
                                    {
                                        person[item.Key] = item.Value;
                                    }
                                }
                            }
                        }
                    }
                }

                using (var reader = commandCustomer.ExecuteReader()) // Voer de 'Customer' query uit
                {
                    if (reader.Read()) // Als er gegevens zijn voor de klant
                    {
                        for (int j = 0; j < reader.FieldCount; j++) // Loop door de kolommen van 'Customer'
                        {
                            person[reader.GetName(j)] =
                                reader.GetValue(j); // Voeg de klantgegevens toe aan de dictionary
                        }
                    }

                    return person; // Retourneer de verzamelde gegevens
                }
            }
        }
        catch (MySqlException ex) // Fout bij databaseverbinding of query
        {
            _exception = ex;
            return null; // Retourneer null bij fout
        }
        catch (Exception ex) // Algemene foutafhandeling
        {
            _exception = ex;
            return null; // Retourneer null bij fout
        }
    }

    private Dictionary<string, object> GetFullVehicleData(object frameNr)
    {
        try
        {
            // SQL-query om gegevens op te halen uit de 'Vehicle' tabel op basis van het frameNummer
            string query = "SELECT * FROM Vehicle WHERE FrameNr = @F";

            // Maak verbinding met de database en voer de query uit
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Voeg de 'FrameNr' parameter toe aan de query
                command.Parameters.AddWithValue("@F", frameNr);

                // Maak een dictionary om alle voertuiggegevens op te slaan
                var data = new Dictionary<string, object>();

                // Voer de query uit en verwerk de resultaten
                using (var reader = command.ExecuteReader())
                {
                    // Loop door alle rijen van de resultaten
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++) // Loop door de kolommen van de rij
                        {
                            // Controleer of de kolom de naam 'VehicleBlob' heeft en converteer de gegevens indien nodig
                            if (reader.GetName(i).ToString().Equals("VehicleBlob"))
                            {
                                // Converteer het byte-array naar een Base64-string
                                data[reader.GetName(i)] = Convert.ToBase64String((byte[])reader.GetValue(i));
                            }
                            else
                            {
                                // Voeg de andere gegevens toe aan de dictionary
                                data[reader.GetName(i)] = reader.GetValue(i).ToString();
                            }
                        }
                    }
                }

                return data; // Retourneer de verzamelde voertuiggegevens
            }
        }
        catch (MySqlException ex) // Fout bij databaseverbinding of query
        {
            _exception = ex;
            return new Dictionary<string, object>(); // Retourneer een lege dictionary bij fout
        }
        catch (Exception ex) // Algemene foutafhandeling
        {
            _exception = ex;
            return new Dictionary<string, object>(); // Retourneer een lege dictionary bij fout
        }
    }


    /// <summary>
    /// De benodigde gegevens van het voertuig worden opgehaald
    /// </summary>
    /// <param name="frameNr"></param>
    /// <returns></returns>
    private object GetVehicleInfo(object frameNr)
    {
        try
        {
            // SQL-query om basisvoertuiginformatie op te halen op basis van het frameNummer
            string query = $"SELECT Brand, Type, YoP, Sort From Vehicle WHERE FrameNr = @F";

            // Maak verbinding met de database en voer de query uit
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Voeg de 'FrameNr' parameter toe aan de query
                command.Parameters.AddWithValue("@F", frameNr);

                // Voer de query uit en verwerk de resultaten
                using (var reader = command.ExecuteReader())
                {
                    // Lees de eerste (en enige) rij van de resultaten
                    reader.Read();

                    // Retourneer de samengevoegde voertuiginformatie in één string
                    return $"{reader.GetValue(0)}, {reader.GetValue(1)}, {reader.GetValue(2)}, {reader.GetValue(3)}";
                }
            }
        }
        catch (MySqlException ex) // Fout bij databaseverbinding of query
        {
            _exception = ex;
            return ex.Message; // Retourneer foutmelding bij MySQL-exceptie
        }
        catch (Exception ex) // Algemene foutafhandeling
        {
            _exception = ex;
            return ex.Message; // Retourneer foutmelding bij andere uitzonderingen
        }
    }

    public (bool Status, string Message, Dictionary<string, object> Data) GetFullDataReview(int id)
    {
        // Haal alle gegevens op met fullInfo (true)
        (bool Status, Dictionary<string, object> row) data = GetFromDB(id, true);

        if (!data.Status)
        {
            // Als er een fout optreedt, retourneer de foutmelding
            return (false, _exception.Message, new Dictionary<string, object>());
        }

        return (true, "Succes", data.row); // Gegevens zijn succesvol opgehaald
    }

    public (bool Status, string Message, Dictionary<string, object> Data) GetDataReview(int id)
    {
        // Haal de gegevens op zonder extra informatie (false)
        (bool Status, Dictionary<string, object> row) data = GetFromDB(id, false);

        if (!data.Status)
        {
            // Als er een fout optreedt, retourneer de foutmelding
            return (false, _exception.Message, new Dictionary<string, object>());
        }

        return (true, "Succes", data.row); // Gegevens zijn succesvol opgehaald
    }

    public (bool Status, string Message, int[] Ids) GetDataReviewIds()
    {
        try
        {
            // Bepaal het aantal rijen in de Contract tabel
            string size = "SELECT COUNT(*) FROM Contract";
            int rows = 0;
            using (var connection = _connector.CreateDbConnection())
            using (var rowsCommand = new MySqlCommand(size, (MySqlConnection)connection))
            using (var rowsReader = rowsCommand.ExecuteReader())
            {
                rowsReader.Read();
                rows = Convert.ToInt32(rowsReader.GetValue(0));
            }

            // Haal alle OrderIds op uit de Contract tabel
            string query = "SELECT OrderId FROM Contract";
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = command.ExecuteReader())
            {
                int[] ids = new int[rows];
                int place = 0;

                // Voeg elke OrderId toe aan de array
                while (reader.Read())
                {
                    ids[place++] = (int)reader.GetValue(0);
                }

                return (true, "Ids", ids); // Retourneer de lijst van Ids
            }
        }
        catch (MySqlException ex)
        {
            // Fout in de databasequery
            return (false, ex.Message, new int[0]);
        }
        catch (Exception ex)
        {
            // Algemene fout
            return (false, ex.Message, new int[0]);
        }
    }

    public async Task<(bool status, string message)> AddSubscriptionAsync(string type, string description,
        double discount, double price)
    {
        try
        {
            // Voeg een nieuw abonnement toe aan de Abonnement tabel
            string query =
                "INSERT INTO Abonnement (Type, Description, Discount, Price) VALUES (@Type, @Description, @Discount, @Price)";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Voeg de parameters voor het abonnement toe
                command.Parameters.AddWithValue("@Type", type);
                command.Parameters.AddWithValue("@Description", description);
                command.Parameters.AddWithValue("@Discount", discount);
                command.Parameters.AddWithValue("@Price", price);

                // Voer de query uit en controleer of het toevoegen gelukt is
                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    return (true, "Subscription added");
                }

                return (false, "Error during adding of subscription");
            }
        }
        catch (MySqlException ex)
        {
            // Database fout
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message)> DeleteSubscriptionAsync(int id)
    {
        try
        {
            // Verwijder een abonnement op basis van het ID
            string query = "DELETE FROM Abonnement WHERE ID = @ID";

            using (var connection = _connector.CreateDbConnection())
            using (var customerCommand = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Voeg het ID parameter toe
                customerCommand.Parameters.AddWithValue("@ID", id);

                // Voer de query uit en controleer of er rijen zijn verwijderd
                int rowsAffected = await customerCommand.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return (true, "Subscription deleted");
                }

                return (false, "Subscription could not be deleted");
            }
        }
        catch (MySqlException ex)
        {
            // Fout bij databaseoperatie
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, "Database error: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Algemene fout
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, "Unexpected error: " + ex.Message);
        }
    }
}