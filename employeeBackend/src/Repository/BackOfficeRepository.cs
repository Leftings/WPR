using System.Data;
using System.Runtime.CompilerServices;
using Employee.Controllers.viewRentalData;
using Employee.Database;
using MySql.Data.MySqlClient;

namespace Employee.Repository;

public class BackOfficeRepository(Connector connector) : IBackOfficeRepository
{
    private readonly Connector _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    private Exception? _exception;

    /// <summary>
    /// Alle benodigde gegevens van Abonnements tabel worden opgehaald
    /// (Dit kan in 1 keer gedaan worden, omdat er geen grote gegevens verstuurd worden)
    /// </summary>
    /// <returns></returns>
    private (bool status, IList<Dictionary<string, object>> rows) GetFromDB()
    {
        try
        {
            IList<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            string query = "SELECT * FROM Abonnement";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = command.ExecuteReader())
            {
                int columns = reader.FieldCount;

                // Zolang er rijen zijn, blijft de loop doorgaan
                while (reader.Read())
                {
                    // Alle gegevens per kolom worden opgeslagen
                    Dictionary<string, object> column = new Dictionary<string, object>();

                    for (int col = 0; col < columns; col++)
                    {
                        string columnName = reader.GetName(col);
                        object columnData = reader.GetValue(col);

                        column[columnName] = columnData;

                        // Extra gegevens worden vanuit andere tabellen opgehaald
                        if (columnName.Equals("Customer"))
                        {
                            column["NameCustomer"] = GetName(columnData, "UserCustomer");
                        }
                        else if (columnName.Equals("ReviewedBy"))
                        {
                            column["NameEmployee"] = GetName(columnData, "Staff");
                        }
                        else if (columnName.Equals("FrameNrCar"))
                        {
                            column["Vehicle"] = GetVehicleName(columnData);
                        }
                    }
                    rows.Add(column);
                }
            }

            return (true, rows);
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

    /// <summary>
    /// De benodigde gegevens van het voertuig worden opgehaald
    /// </summary>
    /// <param name="frameNr"></param>
    /// <returns></returns>
    private object GetVehicleName(object frameNr)
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

    /// <summary>
    /// De rijen worden gesorteerd op basis van de kolom waarop de gebruiker wilt sorteren (Double).
    /// rows: Lijst met alle rijen erin als Dictionary vorm
    /// how: Van laag naar hoog of van hoog naar laag
    /// column: Op welke kolom gesorteerd moet worden
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="how"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    private IList<Dictionary<string, object>> SortNumber(IList<Dictionary<string, object>> rows, string how, string column)
    {
        for (int row = 1; row < rows.Count; row++)
        {
            Dictionary<string, object> key = rows[row];
            int j = row - 1;

            while (j >= 0 &&
                (how.Equals("Low") 
                    ? Convert.ToDouble(key[column]) <= Convert.ToDouble(rows[j][column])
                    :  Convert.ToDouble(key[column]) >= Convert.ToDouble(rows[j][column])
            ))
            {
                rows[j + 1] = rows[j];
                j = j - 1;
            }
            rows[j + 1] = key;
        }

        return rows;
    }

    /// <summary>
    /// De rijen worden gesorteerd op basis van de kolom waarop de gebruiker wilt sorteren (String).
    /// rows: Lijst met alle rijen erin als Dictionary vorm
    /// how: Van laag naar hoog of van hoog naar laag
    /// column: Op welke kolom gesorteerd moet worden
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="how"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    private IList<Dictionary<string, object>> SortName(IList<Dictionary<string, object>> rows, string how, string column)
    {
        for (int row = 1; row < rows.Count; row++)
        {
            Dictionary<string, object> key = rows[row];
            int j = row - 1;

            while (j >= 0 &&
                (how.Equals("Low") 
                    ? String.Compare((String)key[column], (String)rows[j][column]) > 0
                    : String.Compare((String)key[column], (String)rows[j][column]) <= 0
            ))
            {
                rows[j + 1] = rows[j];
                j = j - 1;
            }
            rows[j + 1] = key;
        }

        return rows;
    }

    /// <summary>
    /// De rijen worden gesorteerd op basis van de kolom waarop de gebruiker wilt sorteren (DateTime).
    /// rows: Lijst met alle rijen erin als Dictionary vorm
    /// how: Van laag naar hoog of van hoog naar laag
    /// column: Op welke kolom gesorteerd moet worden
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="how"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    private IList<Dictionary<string, object>> SortDate(IList<Dictionary<string, object>> rows, string how, string column)
    {
        for (int row = 1; row < rows.Count; row++)
        {
            Dictionary<string, object> key = rows[row];
            int j = row - 1;

            while (j >= 0 &&
                (how.Equals("Low") 
                    ? DateTime.Compare((DateTime)key[column], (DateTime)rows[j][column]) <= 0
                    : DateTime.Compare((DateTime)key[column], (DateTime)rows[j][column]) > 0
            ))
            {
                rows[j + 1] = rows[j];
                j = j - 1;
            }
            rows[j + 1] = key;
        }

        return rows;
    }

    /// <summary>
    /// Vanuit GetDataReviews worden de gegevens verzameld en gesorteerd
    /// sort: Op welke kolom gesorteerd moet worden
    /// how: Hoe er gesorteerd moet worden (laag - hoog, hoog - laag)
    /// </summary>
    /// <param name="sort"></param>
    /// <param name="how"></param>
    /// <returns></returns>
    public (bool Status, string Message, IList<Dictionary<string, object>> Data) GetDataReviews(string sort, string how)
    {
        (bool Status, IList<Dictionary<string, object>> rows) data = GetFromDB();

        if (!data.Status)
        {
            return (false, _exception.Message, null);
        }

        try
        {
            Convert.ToDouble(data.rows[0][sort]);
            return (true, sort, SortNumber(data.rows, how, sort));
        }
        catch (Exception ex)
        {
            try
            {
                Convert.ToString(data.rows[0][sort]);
                return (true, sort, SortName(data.rows, how, sort));
            }
            catch (Exception ex2)
            {
                return (true, sort, SortDate(data.rows, how, sort));
            }
        }
    }
}