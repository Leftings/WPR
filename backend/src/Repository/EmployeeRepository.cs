﻿using WPR.Database;
using WPR.Hashing;
using MySql.Data.MySqlClient;
using WPR.Services;
using WPR.Controllers.Employee.BackOffice.signUpStaff;
using WPR.Controllers.Customer.AddBusiness;

namespace WPR.Repository;

/// <summary>
/// De UserRepository class geeft de methodes voor de interactie met de database voor gebruiker gerelateerde operaties.
/// Het implementeert de IUserRepository interface.
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly IConnector _connector;
    private readonly Hash _hash;
    private readonly EmailService _emailService;

    public EmployeeRepository(IConnector connector, Hash hash, EmailService emailService)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _hash = hash ?? throw new ArgumentNullException(nameof (hash));
        _emailService = emailService ?? throw new ArgumentNullException(nameof (emailService));
    }
    
    /// <summary>
    /// Er wordt een query aangemaakt met de meegegeven gegevens voor een voertuig.
    /// Nadat de query volledig is, wordt deze uitgevoerd.
    /// Frame nummers worden automatisch toegepast op het voertuig.
    /// </summary>
    /// <param name="yop"></param>
    /// <param name="brand"></param>
    /// <param name="type"></param>
    /// <param name="licensePlate"></param>
    /// <param name="color"></param>
    /// <param name="sort"></param>
    /// <param name="price"></param>
    /// <param name="description"></param>
    /// <param name="vehicleBlob"></param>
    /// <returns></returns>
    // vehicleBlob is het pad naar de afbeelding
    public async Task<(bool status, string message)> AddVehicleAsync(int yop, string brand, string type, string licensePlate, string color, string sort, double price, string description, byte[] vehicleBlob, int places)
    {
        try
        {
            string query = "INSERT INTO Vehicle (YoP, Brand, Type, LicensePlate, Color, Sort, Price, Description, Vehicleblob, Seats) VALUES (@Y, @B, @T, @L, @C, @S, @P, @D, @V, @SE)";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld
                command.Parameters.AddWithValue("@Y", yop);
                command.Parameters.AddWithValue("@B", brand);
                command.Parameters.AddWithValue("@T", type);
                command.Parameters.AddWithValue("@L", licensePlate);
                command.Parameters.AddWithValue("@C", color);
                command.Parameters.AddWithValue("@S", sort);
                command.Parameters.AddWithValue("@P", price);
                command.Parameters.AddWithValue("@D", description);
                command.Parameters.AddWithValue("@V", vehicleBlob);
                command.Parameters.AddWithValue("@SE", places);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                    return (true, "Vehicle inserted");
                }
                return (false, "Something went wrong while inserting the vehicle");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Er kunnen medewerkers toegevoegd worden aan het systeem.
    /// Als er in de meegegeven data naar voren komt dat een er wagenparkbeheerder toegevoegd wordt, wordt daarvoor een aparte query gemaakt.
    /// Als er in de meegegeven data naar voren komt dat er een Car and All medewerker toegevoegd wordt, wordt daarvoor een aparte query gemaakt.
    /// De mee gegeven gegvens worden in de query geïmplementeerd en vervolgens uitgevoerd.
    /// </summary>
    /// <param name="personData"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> AddStaff(SignUpStaffRequest request)
    {
        try
        {
            // Er wordt een specifieke query aangewezen tussen VehicleManagers en Offices
            string query;

            if (request.Job.Equals("Wagen"))
            {
                query = "INSERT INTO VehicleManager (FirstName, LastName, Password, Email, Business) VALUES (@F, @L, @P, @E, @B)";
            }
            else
            {
                query = "INSERT INTO Staff (FirstName, LastName, Password, Email, Office) VALUES (@F, @L, @P, @E, @O)";
            }

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld
                command.Parameters.AddWithValue("@F", request.FirstName);
                command.Parameters.AddWithValue("@L", request.LastName);
                command.Parameters.AddWithValue("@P", _hash.createHash(request.Password));
                command.Parameters.AddWithValue("@E", request.Email);

                if (request.Job.Equals("Wagen"))
                {
                    command.Parameters.AddWithValue("@B", request.KvK);
                }
                else
                {
                    command.Parameters.AddWithValue("@O", request.Job);
                }

                if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                        return (true, "Data inserted");
                    }
                    
                    return (false, "Data not inserted");
            }
        }
        catch(MySqlException ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Er wordt query aangemaakt die zoekt in de Staff tabel naar het ingevoerde emailadress, zodat de emailadressen altijd uniek blijven.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> checkUsageEmailAsync(string email)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM Staff WHERE LOWER(Email) = LOWER(@E)";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De paramater wordt ingevuld
                command.Parameters.AddWithValue("@E", email);
                bool inUse = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                // Als de uitvoer van de query > 0, dan inUse = true, "Email detected", anders inUse = false, "No email detected"
                return (inUse, inUse ? "Email detected" : "No email detected");
            }
        }

        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool status, string message)> checkUsageEmaiVehicleManagerlAsync(string email)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM VehicleManager WHERE LOWER(Email) = LOWER(@E)";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De paramater wordt ingevuld
                command.Parameters.AddWithValue("@E", email);
                bool inUse = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                // Als de uitvoer van de query > 0, dan inUse = true, "Email detected", anders inUse = false, "No email detected"
                return (inUse, inUse ? "Email detected" : "No email detected");
            }
        }

        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// De voornaam, achternaam, adres, emailadres en telefoonnummer wordt uit de UserCustomer tabel gehaald, om deze te laten tonen bij de huuraanvragen.
    /// De gegevens van de klant worden verzameld doormiddel van hun id.
    /// </summary>
    /// <param name="userid"></param>
    /// <returns></returns>
    private async Task<(bool status, Dictionary<string, object> data)> GetUserDataAsync(string userid)
    {
        try
        {
            var row = new Dictionary<string, object>();
            string queryCustomer = "SELECT Email FROM Customer WHERE ID = @I";
            string queryPrivate = "SELECT FirstName, LastName, Adres, TelNum FROM Private WHERE ID = @I";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var commandCustomer = new MySqlCommand(queryCustomer, (MySqlConnection)connection))
            using (var commandPrivate = new MySqlCommand(queryPrivate, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                commandCustomer.Parameters.AddWithValue("@I", userid);
                commandPrivate.Parameters.AddWithValue("@I", userid);

                using (var readerCustomer = await commandCustomer.ExecuteReaderAsync())
                {
                    // Alle gegevens in de row worden verzameld <Kolom naam : Kolom data>
                    await readerCustomer.ReadAsync();

                    for (int i = 0; i < readerCustomer.FieldCount; i++)
                    {
                        row[readerCustomer.GetName(i)] = readerCustomer.GetValue(i);
                    }

                    await readerCustomer.CloseAsync();
                }

                using (var readerPrivate = await commandPrivate.ExecuteReaderAsync())
                {
                    await readerPrivate.ReadAsync();

                    for (int j = 0; j < readerPrivate.FieldCount; j++)
                    {
                        row[readerPrivate.GetName(j)] = readerPrivate.GetValue(j);
                    }

                    return (true, row);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message} User");
            return (false, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, new Dictionary<string, object>());
        }
    }

    /// <summary>
    /// Het voertuigmerk, type en nummerbord worden uit de tabel Vehicle getrokken, doormiddel van hun frame nummer
    /// </summary>
    /// <param name="carId"></param>
    /// <returns></returns>
    private async Task<(bool status, Dictionary<string, object> data)> GetVehicleData(string carId)
    {
        try
        {
            string query = "SELECT Brand, Type, LicensePlate FROM Vehicle WHERE FrameNr = @I";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", carId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Alle gegevens in de row worden verzameld <Kolom naam : Kolom data>
                    await reader.ReadAsync();
                    
                    var row = new Dictionary<string, object>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }

                    return (true, row);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message} Vehicle");
            return (false, new Dictionary<string, object>());
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, new Dictionary<string, object>());
        }
    }

    /// <summary>
    /// Voor een zakelijke huuraanvraag moet deze eerst geaccepteerd worden door de wagenparkbeheerdeer van het bedrijf.
    /// Deze aanvragen worden geselecteerd door middel van de status van de aanvraag op te vragen en te kijken of deze overeen komt met 'requested', de VMstatus overeen komt met 'requested'en of het KvK nummer van gebruiker gelijk is aan het KvK nummer van de wagenparkbeheerder.
    /// 
    /// Voor een particuliere huuraanvraag of geaccepteerde zakelijke huuraanvraag worden deze geaccepteerd door een frontoffice medewerker van Car and All.
    /// Hierbij wordt gekekenof de status nog steeds 'requested' is en of de VMstatus gelijk is aan 'X' of 'accepted'.
    /// 
    /// Alle ids van deze aanvragen worden in een list gestopt een gereturnt.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<(bool status, List<string> ids)> GetReviewIdsAsync(string user, string userId)
    {
        try
        {
            bool isVehicleManager = user.Equals("vehicleManager");
            string query = "QUERY";

            if (isVehicleManager)
            {
                query = @"SELECT OrderId FROM Contract 
                        JOIN Customer C on C.ID = Contract.Customer
                        WHERE Status = 'requested' AND VMStatus = 'requested' AND C.KvK = @K";

            }
            else if (user.Equals("frontOffice"))
            {
                query = "SELECT OrderId FROM Contract WHERE Status = 'requested' AND (VMStatus = 'X' OR VMStatus = 'accepted')";
            }

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            
            {
                if (isVehicleManager)
                {
                    // De parameter wordt ingevuld
                    command.Parameters.AddWithValue("@K", GetKvK(userId));
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    List<string> ids = new List<string>();
                    while (await reader.ReadAsync())
                    {
                        // Alle ids worden in een list gestopt
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            ids.Add(reader.GetValue(i).ToString());
                        }
                    }

                    return (true, ids);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return (false, new List<string>());
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(ex.Message);
            return (false, new List<string>());
        }
    }

    /// <summary>
    /// Er wordt een specifieke id opgevraagd en deze wordt uit de tabel Contract opgevraagd met alle data in de juiste rij.
    /// 
    /// Vervogelens worden de gegevens huurder en het voertuig async opgevraagd.
    /// Daarna worden door alle kolomen heen gegegaan om de gegevens in een dictonary te stoppen.
    /// In de dictonary wordt de kolom naam als key gebruikt en de kolom waarde als waarde en vervolgens in een list gestopt.
    /// Vervolgens wordt in hetzelfde gedaan met de verzamelde gegevens van de gebruiker en het voertuig.
    /// 
    /// Uiteindelijk wordt de list met alle dictonaries met gegevens gereturnt.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<(bool status, List<Dictionary<string, object>> data)> GetReviewAsync(string id)
    {
        try
        {
            string query = "SELECT * FROM Contract WHERE OrderId = @I";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", id);

                // De query wordt uitgevoerd
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        // Er wordt een lijst gemaakt met alle waarders van elke row van de uitgevoerde query
                        var rows = new List<Dictionary<string, object>>();
                        // De gegevens in de row worden opgeslagen in een dictonary <Kolom naam : Kolom data>
                        var row = new Dictionary<string, object>();

                        var getUserData = GetUserDataAsync(reader.GetValue(5).ToString());
                        var getVehiceData = GetVehicleData(reader.GetValue(4).ToString());

                        // alle gegvens in de row worden verzameld
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row = new Dictionary<string, object>();
                            row[reader.GetName(i)] = reader.GetValue(i);
                            rows.Add(row);
                        }

                        var userData = await getUserData;

                        // Alle gebruikers gegevens worden verzameld
                        foreach (var userField in userData.data)
                        {
                            row = new Dictionary<string, object>();
                            row[userField.Key] = userField.Value;
                            rows.Add(row);
                        }

                        var vehicleData = await getVehiceData;

                        // Alle voertuig gegevens worden verzameld
                        foreach (var vehicelField in vehicleData.data)
                        {
                            row = new Dictionary<string, object>();
                            row[vehicelField.Key] = vehicelField.Value;
                            rows.Add(row);
                        }

                        return (true, rows);
                    }

                    return (false, null);
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message} Review");
            return (false, new List<Dictionary<string, object>>());
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, new List<Dictionary<string, object>>());
        }
    }

    /// <summary>
    /// Het KvK-nummer van de wagenparkbeheerder wordt opgehaald uit de database en gereturnd.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private string GetKvK(string id)
    {
        try
        {
            string query = "SELECT Business FROM VehicleManager WHERE ID = @I";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", id);

                return command.ExecuteScalar().ToString();
            }
        }
        catch (MySqlException ex)
        {
            return ex.Message;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// De status van de frontoffice medewerker of wagenparkbeheerder wordt geupdate in de huuraanvraag.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <param name="employee"></param>
    /// <param name="userType"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> SetStatusAsync(string id, string status, string employee, string userType)
    {
        try
        {
            bool isOfficeType = userType.Equals("frontOffice");
            string query = "QUERY";

            if (isOfficeType)
            {
                query = "UPDATE Contract SET Status = @S, ReviewedBy = @E WHERE OrderId = @I";
            }
            else if (userType.Equals("vehicleManager"))
            {
                query = "UPDATE Contract SET VMStatus = @S WHERE OrderId = @I";
            }

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                Console.WriteLine(id);
                // De parameters worden ingevuld
                command.Parameters.AddWithValue("@S", status);
                command.Parameters.AddWithValue("@I", id);

                if (isOfficeType)
                {
                    command.Parameters.AddWithValue("@E", employee);
                }

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                    return (true, "Status updated");
                }
                
                return (false, "Status not updated");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// CheckDomain bekijkt of het domain niet al in gebruik ik in de tabel Customer en in de tabel Business
    /// </summary>
    /// <param name="table"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    private async Task<(bool Status, string Message)> CheckDomain(string table, string domain)
    {
        try
        {
            string query;

            if (table.Equals("Customer"))
            {
                query = "SELECT Email FROM Customer";
            }
            else
            {
                query = "SELECT Domain FROM Business";
            }   

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                bool domainFound = false;
                while (await reader.ReadAsync())
                {
                    string[] split = reader.GetValue(0).ToString().Split("@");
                    if (split[1].ToLower().Equals(domain[1..]))
                    {
                        domainFound = true;
                        break;
                    }
                }
                
                if (domainFound)
                {
                    return (false, "Domain Detected");
                }
                return (true, "Unique Domain");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Maakt een connectie met de database door middel van de connector en de query.
    /// De gegevens worden uit de AddBusinessRequest getrokken en verwerkt in de query.
    /// De query wordt uitgevoerd en checkt of deze ook succesvol is uitgevoerd
    /// </summary>
    /// <param name="table"></param>
    /// <param name="domain"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> AddBusiness(AddBusinessRequest request)
    {
        var customer = CheckDomain("Customer", request.Domain);
        var business = CheckDomain("Business", request.Domain);

        await Task.WhenAll(customer, business);
        if (customer.Result.Status && business.Result.Status)
        {
            try
            {
                string query = "INSERT INTO Business (KvK, BusinessName, Adres, Domain, ContactEmail, Abonnement) VALUES (@K, @B, @A, @D, @C,@Ab)";

                // Er wordt een connectie met de database gemaakt
                using (var connection = _connector.CreateDbConnection())
                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                {
                    // Parameters van de query worden ingevuld
                    command.Parameters.AddWithValue("@K", request.KvK);
                    command.Parameters.AddWithValue("@B", request.Name);
                    command.Parameters.AddWithValue("@A", request.Adress);
                    command.Parameters.AddWithValue("@D", request.Domain);
                    command.Parameters.AddWithValue("@C", request.ContactEmail);
                    command.Parameters.AddWithValue("@Ab", request.Subscription);


                    // Er wordt gekeken of de query succesvol is uitgevoerd
                    if (command.ExecuteNonQuery() > 0)
                    {
                        await _emailService.SendConfirmationEmailBusiness(request.ContactEmail, request.Name, request.KvK, request.Domain, request.Adress);
                        return (true, "Succesfull added business");
                    }
                    return (false, "Error occured adding business");
                }
            }
            catch(MySqlException ex)
            {
                return (false, ex.Message);
            }
            catch(Exception ex)
            {
                return (false, ex.Message);
            }
        }

        return (false, $"Domain Detected");
    }

    /// <summary>
    /// Haalt KvK-nummers op van bedrijven die gedeactiveerd zijn.
    /// </summary>
    /// <returns>
    /// Een tuple met de statuscode (200 voor succes, 500 voor mislukking), een bericht en een lijst met KvK-nummers.
    /// </returns>
public (int StatusCode, string Message, IList<int> KvK) ViewBusinessRequests()
{
    try
    {
        string query = "SELECT KvK FROM Business WHERE Activated = 'Deactive'";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        using (var reader = command.ExecuteReader())
        {
            IList<int> kvk = new List<int>();

            // Leest de KvK-nummers en voegt ze toe aan de lijst
            while (reader.Read())
            {
                kvk.Add(Convert.ToInt32(reader.GetValue(0)));
            }

            // Retourneert de KvK-nummers als een succesvolle response
            return (200, "Succes", kvk);
        }
    }
    catch (MySqlException ex)
    {
        return (500, ex.Message, new List<int>());
    }
    catch (OverflowException ex)
    {
        return (500, ex.Message, new List<int>());
    }
    catch (Exception ex)
    {
        return (500, ex.Message, new List<int>());
    }
}

    /// <summary>
    /// Haalt gedetailleerde informatie op voor een specifiek bedrijf op basis van KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf.</param>
    /// <returns>
    /// Een tuple met de statuscode (200 voor succes, 500 voor mislukking), een bericht en een woordenboek met bedrijfsgegevens.
    /// </returns>
public async Task<(int StatusCode, string Message, Dictionary<string, object> data)> ViewBusinessRequestDetailed(int kvk)
{
    try
    {
        string query = "SELECT KvK, BusinessName, Adres, Domain, ContactEmail FROM Business WHERE KvK = @K";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@K", kvk);

            using (var reader = await command.ExecuteReaderAsync())
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                // Leest de gegevens van de query en slaat ze op in een dictionary
                while (await reader.ReadAsync())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        data[reader.GetName(i)] = reader.GetValue(i);
                    }
                }

                // Retourneert de gegevens als succesvolle response
                return (200, "Succes", data);
            }
        }
    }
    catch (MySqlException ex)
    {
        return (500, ex.Message, new Dictionary<string, object>());
    }
    catch (Exception ex)
    {
        return (500, ex.Message, new Dictionary<string, object>());
    }
}

    /// <summary>
    /// Zet de status van een bedrijf naar 'Actief'.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf dat geactiveerd moet worden.</param>
    /// <returns>
    /// Een tuple met de statuscode (200 voor succes, 500 voor mislukking) en een bericht dat het resultaat aangeeft.
    /// </returns>
public (int StatusCode, string Message) BusinessAccepted(int kvk)
{
    try
    {
        string query = "UPDATE Business SET Activated = 'Active' WHERE KvK = @K";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@K", kvk);

            // Voert de update uit en controleert of het succesvol was
            if (command.ExecuteNonQuery() > 0)
            {
                return (200, "Succes");
            }
            return (500, "Error Occured");
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
    /// Verwijdert een bedrijf uit het systeem op basis van het KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf dat verwijderd moet worden.</param>
    /// <returns>
    /// Een tuple met de statuscode (200 voor succes, 500 voor mislukking) en een bericht dat het resultaat aangeeft.
    /// </returns>
public (int StatusCode, string Message) BusinessDenied(int kvk)
{
    try
    {
        string query = "DELETE FROM Business WHERE KvK = @K";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@K", kvk);

            // Voert de delete-operatie uit en controleert of het succesvol was
            if (command.ExecuteNonQuery() > 0)
            {
                return (200, "Succes");
            }
            return (500, "Error Occured");
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
    /// Haalt gedetailleerde informatie op van een bedrijf op basis van het KvK-nummer.
    /// </summary>
    /// <param name="kvk">Het KvK-nummer van het bedrijf om op te halen.</param>
    /// <returns>
    /// Een tuple met een boolean die aangeeft of het succesvol is, een bericht en een woordenboek met bedrijfsgegevens.
    /// </returns>
public (bool Status, string Message, Dictionary<string, object> Data) GetBusinessInfo(int kvk)
{
    try
    {
        string query = "SELECT * FROM Business WHERE KvK = @K";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@K", kvk);

            using (var reader = command.ExecuteReader())
            {
                // Controleert of er gegevens voor het opgegeven KvK-nummer zijn
                if (!reader.HasRows)
                {
                    return (false, "No data found for the given KvK", new Dictionary<string, object>());
                }

                Dictionary<string, object> data = new Dictionary<string, object>();

                // Leest de gegevens van de database en slaat ze op in de dictionary
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                        data[reader.GetName(i)] = value;
                    }
                }

                return (true, "Success", data);
            }
        }
    }
    catch (MySqlException ex)
    {
        return (false, $"MySQL Error: {ex.Message}", new Dictionary<string, object>());
    }
    catch (Exception ex)
    {
        return (false, $"General Error: {ex.Message}", new Dictionary<string, object>());
    }
}

    /// <summary>
    /// Haalt het KvK-nummer op van een voertuigbeheerder op basis van hun ID.
    /// </summary>
    /// <param name="vehicleManagerId">Het ID van de voertuigbeheerder.</param>
    /// <returns>
    /// Een tuple met de statuscode (200 voor succes, 404 voor niet gevonden, 500 voor fout), een bericht en het KvK-nummer.
    /// </returns>
public (int StatusCode, string Message, int KvK) GetKvK(int vehicleManagerId)
{
    try
    {
        string query = $"SELECT Business FROM VehicleManager WHERE ID = @I";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@I", vehicleManagerId);

            using (var reader = command.ExecuteReader())
            {
                // Leest het KvK-nummer en retourneert het als een succesresponse
                if (reader.Read())
                {
                    int kvk = Convert.ToInt32(reader.GetValue(0));
                    return (200, "Succes", kvk);
                }
                return (404, "KvK Number Not Found", -1);
            }
        }
    }
    catch (MySqlException ex)
    {
        return (500, ex.Message, -1);
    }
    catch (OverflowException ex)
    {
        return (500, ex.Message, -1);
    }
    catch (Exception ex)
    {
        return (500, ex.Message, -1);
    }
}

    /// <summary>
    /// Haalt abonnementsinformatie op op basis van het abonnement-ID.
    /// </summary>
    /// <param name="abonnementId">Het ID van het abonnement.</param>
    /// <returns>
    /// Een tuple met de statuscode (200 voor succes, 404 voor niet gevonden, 500 voor fout), een bericht en een woordenboek met abonnementsgegevens.
    /// </returns>
public (int StatusCode, string Message, Dictionary<string, object> Data) GetAbonnementType(int abonnementId)
{
    try
    {
        string query = $"SELECT * FROM Abonnement WHERE ID = @I";
        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@I", abonnementId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();

                    // Leest de velden van de database en slaat ze op in de dictionary
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        if (!data.ContainsKey(reader.GetName(i)))
                        {
                            data[reader.GetName(i)] = reader.GetValue(i);
                        }
                    }

                    return (200, "Succes", data);
                }

                return (404, "Abonnement Not Found", new Dictionary<string, object>());
            }
        }
    }
    catch (MySqlException ex)
    {
        return (500, ex.Message, new Dictionary<string, object>());
    }
    catch (Exception ex)
    {
        return (500, ex.Message, new Dictionary<string, object>());
    }
}

    /// <summary>
    /// Haalt gedetailleerde informatie op van een voertuigbeheerder, inclusief het wachtwoord.
    /// </summary>
    /// <param name="id">Het ID van de voertuigbeheerder.</param>
    /// <returns>
    /// Een tuple met de statuscode (200 voor succes, 404 voor niet gevonden, 500 voor fout), een bericht en een woordenboek met voertuigbeheerdergegevens.
    /// </returns>
public (int StatusCode, string Message, Dictionary<string, object> Data) GetVehicleManagerInfo(int id)
{
    try
    {
        // Query die expliciet het wachtwoord opvraagt
        string query = $"SELECT ID, Email, Password, Business FROM VehicleManager WHERE ID = @I";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@I", id);

            using (var reader = command.ExecuteReader())
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                
                // Leest de gegevens van de database
                if (reader.Read())
                {
                    data["ID"] = reader.GetInt32(reader.GetOrdinal("ID"));
                    data["Email"] = reader.GetString(reader.GetOrdinal("Email"));
                    data["Password"] = reader.IsDBNull(reader.GetOrdinal("Password")) ? null : reader.GetString(reader.GetOrdinal("Password"));
                    data["Business"] = reader.GetInt32(reader.GetOrdinal("Business"));

                    return (200, "Success", data);
                }

                return (404, "Vehicle Manager Not Found", data);
            }
        }
    }
    catch (MySqlException ex)
    {
        return (500, ex.Message, new Dictionary<string, object>());
    }
    catch (Exception ex)
    {
        return (500, ex.Message, new Dictionary<string, object>());
    }
}

    
    /// <summary>
    /// Er wordt een query aangemaakt met de meegegeven gegevens voor de inname.
    /// Nadat de query volledig is, wordt deze uitgevoerd.
    /// IDs wordt automatisch toegepast op de inname.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="frameNrVehicle"></param>
    /// <param name="reviewedBy"></param>
    /// <param name="date"></param>
    /// <param name="contract"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> AddIntakeAsync(string damage, int frameNrVehicle, string reviewedBy, DateTime date, int contract, bool isDamaged)
    {
        try
        {
            string query = "INSERT INTO Intake (Damage, FrameNrVehicle, ReviewedBy, Date, Contract, IsDamaged) VALUES (@D, @F, @R, @DT, @C, @isD)";

            // Er wordt een connectie met de DataBase gemaakt met de bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld
                command.Parameters.AddWithValue("@D", damage);
                command.Parameters.AddWithValue("@F", frameNrVehicle);
                command.Parameters.AddWithValue("@R", reviewedBy);
                command.Parameters.AddWithValue("@DT", date);
                command.Parameters.AddWithValue("@C", contract);
                command.Parameters.AddWithValue("@isD", isDamaged);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                    return (true, "Intake inserted");
                }
                return (false, "Something went wrong while inserting the intake");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.Message);
        }
    }
}
