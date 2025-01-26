using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Repository.DatabaseCheckRepository;

public class DatabaseCheckRepository : IDatabaseCheckRepository
{
    private readonly IConnector _connector;

    public DatabaseCheckRepository(IConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    /// <summary>
    /// Verwerkt een delete-operatie voor een opgegeven query.
    /// </summary>
    /// <param name="query">De SQL-query die moet worden uitgevoerd.</param>
    /// <returns>
    /// Een tuple met een statuscode en een bericht.
    /// </returns>
    private (int StatusCode, string Message) HandleDelete(string query)
    {
        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                Console.WriteLine(query);
                if (command.ExecuteNonQuery() > 0)
                {
                    return (200, "Succesfully deleted");
                }
                return (404, "Verwijderen van gegevens gefaald. Opgegeven gegevens bestaan niet");
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);
        }
        catch (Exception ex)
        {
            return (500, ex.Message);
        }
    }

    /// <summary>
    /// Verwijdert toekomstige contracten voor een opgegeven klant-ID.
    /// </summary>
    /// <param name="id">Het ID van de klant waarvan de toekomstige contracten moeten worden verwijderd.</param>
    /// <returns>
    /// Een tuple met een statuscode en een bericht.
    /// </returns>
    private (int StatusCode, string Message) DeleteFutereContracts(int id)
    {
        string query = "DELETE FROM Contract WHERE (Customer = @C AND @D < EndDate)";

        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@C", id);
                command.Parameters.AddWithValue("@D", DateTime.Today);
                command.ExecuteNonQuery();

                return (200, "Verwijderen toekomstige contracten was succesvol");
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);
        }
        catch (Exception ex)
        {
            return (500, ex.Message);
        }
    }

    /// <summary>
    /// Controleert of een gebruiker een actief contract heeft.
    /// </summary>
    /// <param name="id">Het ID van de gebruiker die gecontroleerd moet worden.</param>
    /// <returns>
    /// Een tuple met een statuscode en een bericht.
    /// </returns>
    private (int StatusCode, string Message) UserHasActiveContract(int id)
    {
        string query = "SELECT COUNT(*) FROM Contract WHERE (Customer = @C AND @D BETWEEN StartDate AND EndDate AND Status = 'accepted')";

        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@C", id);
                command.Parameters.AddWithValue("@D", DateTime.Today);

                int count = Convert.ToInt32(command.ExecuteScalar());

                if (count > 0)
                {
                    return (424, "Gebruiker heeft momenteel een lopend contract");
                }

                var deleteFutereContracts = DeleteFutereContracts(id);

                return (deleteFutereContracts.StatusCode, deleteFutereContracts.Message);
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);
        }
        catch (OverflowException ex)
        {
            return (500, ex.Message);
        }
        catch (Exception ex)
        {
            return (500, ex.Message);
        }
    }

    /// <summary>
    /// Verwijdert een gebruiker op basis van het opgegeven ID.
    /// </summary>
    /// <param name="id">Het ID van de gebruiker die verwijderd moet worden.</param>
    /// <returns>
    /// Een tuple met een statuscode en een bericht.
    /// </returns>
    public (int StatusCode, string Message) DeleteUser(int id)
    {
        var activeContract = UserHasActiveContract(id);

        if (activeContract.StatusCode == 200)
        {
            string query = $"DELETE FROM Customer WHERE ID = {id}";

            var deleteUser = HandleDelete(query);

            return (deleteUser.StatusCode, deleteUser.Message);
        }

        return (activeContract.StatusCode, activeContract.Message);
    }

    /// <summary>
    /// Verwijdert werknemers op basis van een KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf waarvan de werknemers verwijderd moeten worden.</param>
    /// <returns>
    /// Een tuple met een statuscode, bericht en een dictionary van werknemer-ID's en de bijbehorende verwijderstatus.
    /// </returns>
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
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            int employee = Convert.ToInt32(reader.GetValue(i));

                            var deleted = DeleteUser(employee);

                            if (deleted.StatusCode == 200)
                            {
                                deletedEmployees[employee] = true;
                            }
                            else
                            {
                                deletedEmployees[employee] = false;
                            }
                        }
                    }

                    return (200, "", deletedEmployees);
                }
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message, new Dictionary<int, bool>());
        }
        catch (OverflowException ex)
        {
            return (500, ex.Message, new Dictionary<int, bool>());
        }
        catch (Exception ex)
        {
            return (500, ex.Message, new Dictionary<int, bool>());
        }
    }

    /// <summary>
    /// Controleert of alle werknemers van een bedrijf succesvol zijn verwijderd.
    /// </summary>
    /// <param name="employees">Een dictionary van werknemer-ID's en hun verwijderstatus.</param>
    /// <returns>True als alle werknemers succesvol zijn verwijderd, anders false.</returns>
    private bool DeleteBusinessAllowed(Dictionary<int, bool> employees)
    {
        foreach (var item in employees)
        {
            if (!item.Value)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Verwijdert een bedrijf op basis van het KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf dat verwijderd moet worden.</param>
    /// <returns>
    /// Een tuple met een statuscode en een bericht.
    /// </returns>
    public (int StatusCode, string Message) DeleteBusiness(int kvk)
    {
        var employees = DeleteEmployees(kvk);

        if (employees.StatusCode != 200)
        {
            return (employees.StatusCode, employees.Message);
        }

        if (!DeleteBusinessAllowed(employees.employeeData))
        {
            return (417, "Sommige medewerkers zitten nog in een contract");
        }
        
        string query = $"DELETE FROM Business WHERE KvK = {kvk}";
        var deleteBusiness = HandleDelete(query);

        return (deleteBusiness.StatusCode, deleteBusiness.Message);
    }

    /// <summary>
    /// Controleert of er minimaal één voertuigmanager beschikbaar is voor een bepaald KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf waarvoor de controle uitgevoerd moet worden.</param>
    /// <returns>
    /// Een tuple met een statuscode en een bericht.
    /// </returns>
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
                    return (200, "Allowed");
                }
                return (422, "Er moet minimaal 1 wagenparkbeheerder beschikbaar zijn");
            }
        }
        catch (MySqlException ex)
        {
            return (500, ex.Message);
        }
        catch (OverflowException ex)
        {
            return (500, ex.Message);
        }
        catch (Exception ex)
        {
            return (500, ex.Message);
        }

    }

    /// <summary>
    /// Verwijdert een voertuigmanager op basis van het opgegeven ID en KvK-nummer.
    /// </summary>
    /// <param name="id">Het ID van de voertuigmanager die verwijderd moet worden.</param>
    /// <param name="kvk">Het KvK-nummer van het bedrijf waarvoor de voertuigmanager moet worden verwijderd.</param>
    /// <returns>
    /// Een tuple met een statuscode en een bericht.
    /// </returns>
    public (int StatusCode, string Message) DeleteVehicleManager(int id, int kvk)
    {
        var allowed = MinimumVehicleManagers(kvk);

        if (allowed.StatusCode == 200)
        {
            string query = $"DELETE FROM VehicleManager WHERE ID = {id}";

            var deleteUser = HandleDelete(query);

            return (deleteUser.StatusCode, deleteUser.Message);
        }

        return (allowed.StatusCode, allowed.Message);
    }
}