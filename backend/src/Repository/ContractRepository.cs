using System.Linq.Expressions;
using MySql.Data.MySqlClient;
using WPR.Controllers.Employee.Shared.viewRentalData;
using WPR.Database;

namespace WPR.Repository;

public class ContractRepository : IContractRepository
{
    private readonly IConnector _connector;
    public ContractRepository (IConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }
    public async Task<IList<int>> GetContractsSendEmailAsync()
    {
        string query = $"SELECT OrderId FROM Contract WHERE (SendEmail = 'No' AND StartDate = '{DateTime.Today.AddDays(1):yyyy-MM-dd}')";
        Console.WriteLine(query);
        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                IList<int> orders = new List<int>();

                while (await reader.ReadAsync())
                {
                    int orderId = Convert.ToInt32(reader.GetValue(0));
                    Console.WriteLine(orderId);
                    orders.Add(orderId);
                }

                return orders;
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return new List<int>();
        }
        catch (OverflowException ex)
        {
            Console.WriteLine(ex.Message);
            return new List<int>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new List<int>();
        }
    }

    public async Task<Dictionary<string, object>> GetContractInfoAsync(int orderId)
    {
        string query = $"SELECT * FROM Contract WHERE OrderId = {orderId}";
        Dictionary<string, object> info = new Dictionary<string, object>();

        try
        {
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        info[reader.GetName(i)] = reader.GetValue(i);
                    }
                }

                return info;
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new Dictionary<string, object>();
        }
    }
}