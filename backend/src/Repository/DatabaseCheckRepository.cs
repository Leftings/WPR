using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Repository.DatabaseCheckRepository;

public class DatabaseCheckRepository : IDatabaseCheckRepository
{
    private readonly Connector _connector;

    // Constructor: initialiseer de connector voor databaseverbinding
    public DatabaseCheckRepository(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    // Methode voor het afhandelen van DELETE-operaties
    private (int StatusCode, string Message) HandleDelete(string query)
    {
        try
        {
            // Maak verbinding met de database en voer de query uit
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                Console.WriteLine(query);  // Log de query naar de console
                // Controleer of het verwijderen succesvol is
                if (command.ExecuteNonQuery() > 0)
                {
                    return (200, "Succesfully deleted");
                }
                return (404, "Verwijderen van gegevens gefaald. Opgegeven gegevens bestaan niet");
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);  // Fout bij database-operatie
        }
        catch (Exception ex)
        {
            return (500, ex.Message);  // Algemene fout
        }
    }

    // Methode voor het verwijderen van toekomstige contracten van een gebruiker
    private (int StatusCode, string Message) DeleteFutereContracts(int id)
    {
        string query = "DELETE FROM Contract WHERE (Customer = @C AND @D < EndDate)";

        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Voeg de parameters toe voor de query
                command.Parameters.AddWithValue("@C", id);
                command.Parameters.AddWithValue("@D", DateTime.Today);
                command.ExecuteNonQuery();

                return (200, "Verwijderen toekomstige contracten was succesvol");
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);  // Fout bij database-operatie
        }
        catch (Exception ex)
        {
            return (500, ex.Message);  // Algemene fout
        }
    }

    // Methode die controleert of een gebruiker een actief contract heeft
    private (int StatusCode, string Message) UserHasActiveContract(int id)
    {
        string query = "SELECT COUNT(*) FROM Contract WHERE (Customer = @C AND @D BETWEEN StartDate AND EndDate AND Status = 'accepted')";

        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Voeg de parameters toe voor de query
                command.Parameters.AddWithValue("@C", id);
                command.Parameters.AddWithValue("@D", DateTime.Today);

                int count = Convert.ToInt32(command.ExecuteScalar());

                if (count > 0)
                {
                    return (424, "Gebruiker heeft momenteel een lopend contract");
                }

                var deleteFutereContracts = DeleteFutereContracts(id);  // Verwijder toekomstige contracten

                return (deleteFutereContracts.StatusCode, deleteFutereContracts.Message);
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);  // Fout bij database-operatie
        }
        catch (OverflowException ex)
        {
            return (500, ex.Message);  // Overflow-fout
        }
        catch (Exception ex)
        {
            return (500, ex.Message);  // Algemene fout
        }
    }

    // Verwijdert een gebruiker uit de database
    public (int StatusCode, string Message) DeleteUser(int id)
    {
        var activeContract = UserHasActiveContract(id);  // Controleer of de gebruiker een actief contract heeft

        if (activeContract.StatusCode == 200)
        {
            string query = $"DELETE FROM Customer WHERE ID = {id}";  // Verwijder de gebruiker

            var deleteUser = HandleDelete(query);

            return (deleteUser.StatusCode, deleteUser.Message);
        }

        return (activeContract.StatusCode, activeContract.Message);  // Retourneer foutmelding als gebruiker een actief contract heeft
    }

    // Verwijdert medewerkers van een bedrijf op basis van KvK-nummer
    private (int StatusCode, string Message, Dictionary<int, bool> employeeData) DeleteEmployees(int kvk)
    {
        string query = "SELECT ID From Customer WHERE KvK = @B";

        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@B", kvk);

                using (var reader = command.ExecuteReader())
                {
                    Dictionary<int, bool> deletedEmployees = new Dictionary<int, bool>();

                    if (reader.Read())
                    {
                        // Loop door alle medewerkers en verwijder ze
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            int employee = Convert.ToInt32(reader.GetValue(i));

                            var deleted = DeleteUser(employee);

                            if (deleted.StatusCode == 200)
                            {
                                deletedEmployees[employee] = true;  // Succesvol verwijderd
                            }
                            else
                            {
                                deletedEmployees[employee] = false;  // Verwijdering mislukt
                            }
                        }
                    }

                    return (200, "", deletedEmployees);
                }
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message, new Dictionary<int, bool>());  // Fout bij database-operatie
        }
        catch (OverflowException ex)
        {
            return (500, ex.Message, new Dictionary<int, bool>());  // Overflow-fout
        }
        catch (Exception ex)
        {
            return (500, ex.Message, new Dictionary<int, bool>());  // Algemene fout
        }
    }

    // Controleert of het bedrijf toestemming heeft om te worden verwijderd
    private bool DeleteBusinessAllowed(Dictionary<int, bool> employees)
    {
        foreach (var item in employees)
        {
            if (!item.Value)
            {
                return false;  // Als een van de medewerkers niet succesvol is verwijderd, wordt het bedrijf niet verwijderd
            }
        }

        return true;  // Alle medewerkers zijn succesvol verwijderd
    }

    // Verwijdert een bedrijf op basis van KvK-nummer
    public (int StatusCode, string Message) DeleteBusiness(int kvk)
    {
        var employees = DeleteEmployees(kvk);

        if (employees.StatusCode != 200)
        {
            return (employees.StatusCode, employees.Message);  // Foutmelding als het verwijderen van medewerkers niet succesvol was
        }

        if (!DeleteBusinessAllowed(employees.employeeData))
        {
            return (417, "Sommige medewerkers zitten nog in een contract");  // Als niet alle medewerkers geen contract meer hebben, wordt het bedrijf niet verwijderd
        }
        
        string query = $"DELETE FROM Business WHERE KvK = {kvk}";  // Verwijder het bedrijf
        var deleteBusiness = HandleDelete(query);

        return (deleteBusiness.StatusCode, deleteBusiness.Message);
    }

    // Controleert of er minimaal 1 wagenparkbeheerder beschikbaar is voor het bedrijf
    private (int StatusCode, string Message) MinimumVehicleManagers(int kvk)
    {
        string query = "SELECT COUNT(*) FROM VehicleManager WHERE Business = @B";

        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@B", kvk);
                Console.WriteLine(query + $" | {kvk}");
                int count = Convert.ToInt32(command.ExecuteScalar());
                Console.WriteLine(count);

                if (count > 1)
                {
                    return (200, "Allowed");  // Er zijn genoeg wagenparkbeheerders beschikbaar
                }
                return (422, "Er moet minimaal 1 wagenparkbeheerder beschikbaar zijn");  // Niet genoeg wagenparkbeheerders
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);  // Fout bij database-operatie
        }
        catch (OverflowException ex)
        {
            return (500, ex.Message);  // Overflow-fout
        }
        catch (Exception ex)
        {
            return (500, ex.Message);  // Algemene fout
        }
    }

    // Verwijdert een wagenparkbeheerder op basis van ID en KvK-nummer
    public (int StatusCode, string Message) DeleteVehicleManager(int id, int kvk)
    {
        var allowed = MinimumVehicleManagers(kvk);

        if (allowed.StatusCode == 200)
        {
            string query = $"DELETE FROM VehicleManager WHERE ID = {id}";  // Verwijder de wagenparkbeheerder

            var deleteUser = HandleDelete(query);

            return (deleteUser.StatusCode, deleteUser.Message);
        }

        return (allowed.StatusCode, allowed.Message);  // Foutmelding als niet genoeg wagenparkbeheerders beschikbaar zijn
    }
}
