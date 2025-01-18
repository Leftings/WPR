using System.Data;
using System.Runtime.CompilerServices;
using WPR.Controllers.viewRentalData;
using WPR.Database;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509.SigI;
using Org.BouncyCastle.Utilities;

namespace WPR.Repository;

public class BackOfficeRepository(Connector connector) : IBackOfficeRepository
{
    private readonly Connector _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    private Exception? _exception;

    /// <summary>
    /// Alle benodigde gegevens van Contract tabel worden opgehaald
    /// (Dit kan in 1 keer gedaan worden, omdat er geen grote gegevens verstuurd worden)
    /// </summary>
    /// <returns></returns>
    private (bool status, Dictionary<string, object> row) GetFromDB(int id, bool fullInfo)
    {
        try
        {
            Dictionary<string, object> row = new Dictionary<string, object>();

            string query = "SELECT * FROM Contract WHERE OrderId = @I";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))         
            {
                command.Parameters.AddWithValue("@I", id);
                using (var reader = command.ExecuteReader())
                {
                    int columns = reader.FieldCount;

                    // Zolang er rijen zijn, blijft de loop doorgaan
                    while (reader.Read())
                    {
                        // Alle gegevens per kolom worden opgeslagen

                        for (int col = 0; col < columns; col++)
                        {
                            string columnName = reader.GetName(col);
                            object columnData = reader.GetValue(col);

                            row[columnName] = columnData;

                            // Extra gegevens worden vanuit andere tabellen opgehaald
                            if (columnName.Equals("Customer"))
                            {   
                                if (fullInfo)
                                {
                                    foreach (var item in GetFullPerson(columnData))
                                    {
                                        if (!row.ContainsKey(item.Key))
                                        {
                                            row.Add(item.Key, item.Value);
                                        }
                                    }
                                }
                                else
                                {
                                    row["NameCustomer"] = GetName(columnData, "UserCustomer");
                                }
                            }
                            else if (columnName.Equals("ReviewedBy"))
                            {
                                row["NameEmployee"] = GetName(columnData, "Staff");
                            }
                            else if (columnName.Equals("FrameNrVehicle"))
                            {
                                if (fullInfo)
                                {
                                    foreach (var item in GetFullVehicleData(columnData))
                                    {
                                        if (!row.ContainsKey(item.Key))
                                        {
                                            row.Add(item.Key, item.Value);
                                        }
                                    }
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

            return (true, row);

        }
        catch (MySqlException ex)
        {
            _exception = ex;
            return (false, null);
        }
        catch (OverflowException ex)
        {
            _exception = ex;
            return (false, null);
        }
        catch (Exception ex)
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
            string query = $"SELECT LastName, FirstName From {table} WHERE ID = @I";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", id);
                using (var reader = command.ExecuteReader())
                {
                    // Als de id niet is ingevuld, wordt er niks verzonden
                    if (reader.Read())
                    {
                        return $"{reader.GetValue(0)}, {reader.GetValue(1)}";
                    }
                    return null;
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex);
            _exception = ex;
            return null;
        }
        catch (Exception ex)
        {
            _exception = ex;
            return null;
        }
    }

    private Dictionary<string, object> GetBusinessInfo(object employee)
    {
        try
        {
            string query = $"SELECT Business From UserEmployee WHERE ID = @I";
            object business = "";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", employee);
                using (var reader = command.ExecuteReader())
                {
                    Console.WriteLine("Y");
                    while (reader.Read())
                    {
                        business = reader.GetValue(0).ToString();
                    }
                }
            }

            string query2 = "SELECT * FROM Business WHERE KvK = @K";
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query2, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@K", business);
                using (var reader = command.ExecuteReader())
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.WriteLine(reader.GetName(i));
                            if (reader.GetName(i).Equals("Adres"))
                            {
                                data["AdresBusiness"] = reader.GetValue(i);
                            }
                            else
                            {
                                data[reader.GetName(i)] = reader.GetValue(i);
                            }
                        }
                    }

                    return data;
                }
            }

        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex);
            _exception = ex;
            return null;
        }
        catch (Exception ex)
        {
            _exception = ex;
            Console.WriteLine(ex);
            return null;
        }
    }

    private Dictionary<string, object> GetFullPerson(object id)
    {
        try
        {
            string query = $"SELECT * From UserCustomer WHERE ID = @I";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", id);
                using (var reader = command.ExecuteReader())
                {
                    Dictionary<string, object> person = new Dictionary<string, object>();
                    // Als de id niet is ingevuld, wordt er niks verzonden
                    if (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string columnName = reader.GetName(i);
                            object columnData = reader.GetValue(i);

                            person[reader.GetName(i)] = reader.GetValue(i);

                            Console.WriteLine(columnName);

                            if (columnName.Equals("ID"))
                            {
                                foreach (var item in GetBusinessInfo(columnData))
                                {
                                    Console.WriteLine(item.Key);
                                    if (!person.ContainsKey(item.Key))
                                    {
                                        person[item.Key] = item.Value;
                                    }
                                }
                            }
                        }
                    }
                    return person;
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex);
            _exception = ex;
            return null;
        }
        catch (Exception ex)
        {
            _exception = ex;
            return null;
        }
    }

    private Dictionary<string, object> GetFullVehicleData(object frameNr)
    {
        try
        {
            string query = "SELECT * FROM Vehicle WHERE FrameNR = @F";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@F", frameNr);

                // Er wordt een lijst aangemaakt met alle gegevens van het voertuig
                var data = new Dictionary<string, object>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            // Van elke row worden colom namen met gegevens vastgesteld 
                            if (reader.GetName(i).ToString().Equals("VehicleBlob"))
                            {
                                data[reader.GetName(i)] = Convert.ToBase64String((byte[])reader.GetValue(i));
                            }
                            else
                            {
                                data[reader.GetName(i)] = reader.GetValue(i).ToString();
                            }
                        }
                    }
                }

                return data;
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
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
            string query = $"SELECT Brand, Type, YoP, Sort From Vehicle WHERE FrameNr = @F";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@F", frameNr);
                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    return $"{reader.GetValue(0)}, {reader.GetValue(1)}, {reader.GetValue(2)}, {reader.GetValue(3)}";
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex);
            _exception = ex;
            return null;
        }
        catch (Exception ex)
        {
            _exception = ex;
            return null;
        }
    }

    public (bool Status, string Message, Dictionary<string, object> Data) GetFullDataReview(int id)
    {
        (bool Status, Dictionary<string, object> row) data = GetFromDB(id, true);
        
        if (!data.Status)
        {
            return (false, _exception.Message, null);
        }
        return (true, null, data.row);
    }

    public (bool Status, string Message, Dictionary<string, object> Data) GetDataReview(int id)
    {
        (bool Status, Dictionary<string, object> row) data = GetFromDB(id, false);

        if (!data.Status)
        {
            return (false, _exception.Message, null);
        }
        return (true, null, data.row);
    }

    public (bool Status, string Message, int[] Ids) GetDataReviewIds()
    {
        try
        {
            string size = "SELECT COUNT(*) FROM Contract";
            int rows = 0;
            using(var connection = _connector.CreateDbConnection())
            using(var rowsCommand = new MySqlCommand(size, (MySqlConnection)connection))
            using(var rowsReader = rowsCommand.ExecuteReader())
            {
                rowsReader.Read();
                rows = Convert.ToInt32(rowsReader.GetValue(0));
            }

            string query = "SELECT OrderId FROM Contract";

            using(var connection = _connector.CreateDbConnection())
            using(var command = new MySqlCommand(query, (MySqlConnection)connection))
            using(var reader = command.ExecuteReader())
            {
                int[] ids = new int[rows];
                int place = 0;

                while(reader.Read())
                {
                    ids[place++] = (int)reader.GetValue(0);
                }

                return (true, "Ids", ids);
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message, null);

        }
    }
}