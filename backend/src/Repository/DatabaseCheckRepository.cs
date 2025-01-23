using System.ComponentModel;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using MySql.Data.MySqlClient;
using WPR.Database;

namespace WPR.Repository.DatabaseCheckRepository;

public class DatabaseCheckRepository : IDatabaseCheckRepository
{
    private readonly Connector _connector;

    public DatabaseCheckRepository(Connector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

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