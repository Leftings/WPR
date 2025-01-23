using System.ComponentModel;
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
        string query = "DELETE FROM Contract WHERE Customer = @C AND @D < EndDate";

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
        string query = "SELECT COUNT(*) FROM Contract WHERE Customer = @C AND @D BETWEEN StartDate AND EndDate";
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

    public (int StatusCode, string Message) DeleteBusiness(int kvk)
    {
        return (200, "Succes");
    }
}