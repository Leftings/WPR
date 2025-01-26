using System.Data;
using MySql.Data.MySqlClient;
using WPR.Database;
using WPR.Utils;
using WPR.Hashing;
using WPR.Cryption;
using WPR.Controllers.customer.Subscription;
using WPR.Controllers.General.SignUp;
using WPR.Controllers.Employee.VehicleManager.ChangeBusinessSettings;

namespace WPR.Repository;

/// <summary>
/// De UserRepository class geeft de methodes voor de interactie met de database voor gebruiker gerelateerde operaties.
/// Het implementeert de IUserRepository interface.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly Connector _connector;
    private readonly Hash _hash;
    private readonly Crypt _crypt;

    public UserRepository(Connector connector, Hash hash, Crypt crypt)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _hash = hash ?? throw new ArgumentNullException(nameof(hash));
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    /// <summary>
    /// Er wordt door middel van de meegegeven userType een query aangemaakt, om de juiste gegevens uit de juiste tabel op te halen.
    /// Vervolgens wordt er in de query het meegegeven emailadress gestopt, om zo het gehaste wachtwoord uit de database te halen
    /// Nadat er een gebruiker met wachtwoord uit de query komt, zal dit gehaste wachtwoord vergelek worden met het meegegeven gehaste wachtwoord
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="userType"></param>
    /// <returns></returns>
    public async Task<bool> ValidateUserAsync(string username, string password, string userType)
    {
        string table;
        string query;
        
        if (userType.Equals("Employee"))
        {
            table = "Staff";
            query = $@"SELECT password FROM {table} WHERE LOWER(email) = LOWER(@Email)";
        }
        else if (userType.Equals("Customer"))
        {   
            table = "Customer";
            query = $@"SELECT Password FROM {table} WHERE LOWER(email) = LOWER(@Email)";

        }
        else
        {
            table = "VehicleManager";
            query = $@"SELECT password FROM {table} WHERE LOWER(email) = LOWER(@Email)";
        }
        
        // Er wordt een connectie met de Database aangemaakt met de bovenstaande query
        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            // In de query wordt vastgesteld welke row er geselecteerd moet worden doormiddel van het invullen van het emailadres
            command.Parameters.AddWithValue("@Email", username);

            // De query wordt uitgevoerd
            using (var reader = await command.ExecuteReaderAsync())
            {   
                // De query wordt uitgelezen en gekeken of er rows gevonden zijn
                if (await reader.ReadAsync())
                {
                    // Het gehashte wachtwoord wordt opgehaald en wordt vergeleken met het gehaste meegegeven wachtwoord
                    string passwordUser = reader.GetString("Password");
                    return _hash.createHash(password).Equals(passwordUser);
                }

                return false;
            }
        }
    }

    /// <summary>
    /// Er wordt gekeken of het emailadres al bestaat in de databse
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> checkUsageEmailAsync(string email)
    {
        // Er wordt gekeken of het emailadress al ingebruik is
        try
        {
            string query = "SELECT COUNT(*) FROM Customer WHERE LOWER(Email) = LOWER(@E)";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Het emailadres wordt in de query ingevuld en er wordt gekeken of er rows zijn gevonden
                command.Parameters.AddWithValue("@E", email);
                bool inUse = Convert.ToInt32(await command.ExecuteScalarAsync()) > 0;

                // Als het aantal gevoden rows > 0, dan inUse = true, "Email detected" anders inUser = false, "No email detected"
                return (inUse, inUse ? "Email detected" : "No email detected");
            }
        }

        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Een gebruiker (klant) wordt toegevoegd aan de database, doormiddel van alle meegegeven persoondata.
    /// Deze persoondata zal vervolgens in de query geïmplementeerd worden en vervolgens worden uitgevoerd.
    /// </summary>
    /// <param name="personData"></param>
    /// <returns></returns>
    public async Task<(bool status, string message, int newUserID)> addCustomerAsync(Object[] personData)
    {
        try
        {
            string query =
                "INSERT INTO Customer (Adres, Telnum, Password, Email, FirstName, LastName) values (@A, @T, @P, @E, @F, @L)";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld met de megegeven persoons gegevens
                command.Parameters.AddWithValue("@A", personData[0]);
                command.Parameters.AddWithValue("@T", personData[1]);
                command.Parameters.AddWithValue("@P", personData[2]);
                command.Parameters.AddWithValue("@E", personData[3]);
                command.Parameters.AddWithValue("@F", personData[4]);
                command.Parameters.AddWithValue("@L", personData[5]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Als de query succesvol is uitgevoerd wordt de laatste ingevoerde userId meegegeven
                    command.CommandText = "SELECT LAST_INSERT_ID();";
                    int newUserID = Convert.ToInt32(await command.ExecuteScalarAsync());

                    return (true, "Data Inserted", newUserID);
                }

                return (false, "No Data Inserted", -1);
            }
        }

        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message, -1);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message, -1);
        }
    }

    /// <summary>
    /// Het aangemaakte account wordt gekoppeld aan de particuliere tabel, doormiddel van alle meegegeven persoondata.
    /// Deze persoondata zal vervolgens in de query geïmplementeerd worden en vervolgens worden uitgevoerd.
    /// </summary>
    /// <param name="personalData"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> addPersonalCustomerAsync(Object[] personalData)
    {
        try
        {
            string query = "INSERT INTO UserPersonal (ID, BirthDate) values (@I, @B)";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameters worden ingevuld met de persoonlijke gegevens
                command.Parameters.AddWithValue("@I", personalData[0]);
                command.Parameters.AddWithValue("@B", personalData[1]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Er wordt gekeken of de gegevens zijn ingevoert in de DataBase
                    return (true, "Data Inserted");
                }

                return (false, "No Data Inserted");
            }
        }

        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// Het aangemaakte account wordt gekoppeld aan de medewerkers tabel, doormiddel van alle meegegeven persoondata.
    /// Deze persoondata zal vervolgens in de query geïmplementeerd worden en vervolgens worden uitgevoerd.
    /// </summary>
    /// <param name="employeeData"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> addEmployeeCustomerAsync(Object[] employeeData)
    {
        try
        {
            string query = "INSERT INTO UserEmployee (ID, Business) values (@I, @B)";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld
                command.Parameters.AddWithValue("@I", employeeData[0]);
                command.Parameters.AddWithValue("@B", employeeData[1]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                    return (true, "Data Inserted");
                }

                return (false, "No Data Inserted");
            }
        }

        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, ex.Message);
        }
    }

    public async Task<string> GetUserIdAsync(string email, string table)
    {
        try
        {
            string query = $"SELECT ID FROM {table} WHERE Email = @E";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@E", email);

                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    // De user id wordt meegegeven
                    return result.ToString();
                }

                return "No user found";
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return "No user found";
        }
    }

    public async Task<string> GetUserNameAsync(string userId)
    {
        try
        {
            string query = "SELECT Firstname FROM Private WHERE ID = @I";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De paramater wordt ingevuld
                command.Parameters.AddWithValue("@I", Convert.ToInt32(userId));

                var result = await command.ExecuteScalarAsync();

                // De gebruikersnaam wordt meegegeven
                return result.ToString();
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return ex.ToString();
        }
    }

    public async Task<(bool status, string message)> EditUserInfoAsync(List<object[]> data)
    {
        try
        {
            // Er wordt een custom query aangemaakt
            var query = await CreateUserInfoQuery(data);

            if (query.goodQuery)
            {
                // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
                using (var connection = _connector.CreateDbConnection())
                using (var command = new MySqlCommand(query.message, (MySqlConnection)connection))
                {
                    var result = await command.ExecuteNonQueryAsync();

                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        // Er wordt gecontrolleerd of de gegeven in de DataBase zijn ingevoerd
                        return (true, "Data inserted");
                    }

                    return (false, "Data not inserted");
                }
            }
            else
            {
                return (false, query.message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex}");
            return (false, ex.ToString());
        }
    }

    public async Task<bool> IsKvkNumberAsync(int kvkNumber)
    {
        try
        {
            string query = "SELECT COUNT(1) FROM Business WHERE KVK = @kvkNumber";

            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@kvkNumber", kvkNumber);
                var result = Convert.ToInt32(await command.ExecuteScalarAsync());
                return result > 0;
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool status, string message)> DeleteUserAsync(string userId)
    {
        Console.WriteLine("Deleting user");
        try
        {
            string queryCustomer = "DELETE FROM Customer WHERE ID = @ID";

            using (var connection = _connector.CreateDbConnection())
            using (var customerCommand = new MySqlCommand(queryCustomer, (MySqlConnection)connection))
            {
                Console.WriteLine(userId);
                int userIdInt = Convert.ToInt32(userId);
                Console.WriteLine(userIdInt);

                customerCommand.Parameters.AddWithValue("@ID", userIdInt);

                int rowsAffected = await customerCommand.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return (true, "User deleted");
                }
                else
                {
                    return (false, "User could not be deleted");
                }
            }
        }
        catch (MySqlException ex)
        {
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, "Database error: " + ex.Message);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return (false, "Unexpected error: " + ex.Message);
        }
    }

    /// <summary>
    /// Er wordt een query aangemaakt, met daarin de meegeven gegevens die veranderd moeten worden in het account van de gebruiker.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>

    private async Task<(bool goodQuery, string message)> CreateUserInfoQuery(List<object[]> data)
    {
        int lengthList = data.Count(); // De lengte voor de loopt wordt vastgesteld
        string query = "UPDATE Customer SET "; // Het begin van de query wordt aangemaakt

        for (int i = 1; i < lengthList; i++)
        {
            // Item[0] = Colom naam
            // Item[1] = Colom data
            // Item[2] = Type colom data
            object[] item = data[i];

            // Als de 3e item in de list een integer is, worden er geen verdere checks gedaan
            if (item[2].Equals("System.Int32"))
            {
                query += $"{item[0]} = {item[1]}";
            }
            else
            {
                // Als de naam van de colomn gelijks is aan "Email" wordt er een email check gedaan
                if (item[0].ToString().Equals("Email"))
                {
                    var emailCheck = await checkUsageEmailAsync(item[1].ToString());

                    if (emailCheck.status)
                    {
                        return (false, emailCheck.message);
                    }
                }

                query += $"{item[0]} = '{item[1]}'";
            }

            if (i + 1 != lengthList)
            {
                // De query kan worden uitgebreid
                query += ",";
            }

            query += " ";
        }

        // De query wordt afgesloten
        return (true, query += $"WHERE ID = {data[0][1]}");
    }

    /// <summary>
    /// Er wordt door de query, doormiddel van de user id, vastgesteld tot wat voor soort office de medewerker van Car and All behoort
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<(bool status, string message, string officeType)> GetKindEmployeeAsync(string userId)
    {
        try
        {
            string query = "SELECT Office FROM Staff WHERE ID = @I";

            // Connect met de database
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@I", userId);

                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    // krijg het office type binnen
                    string officeType = result.ToString();
                    string message = $"Employee is assigned to {officeType}";

                    return (true, message, officeType);
                }
                else
                {
                    return (false, "No office assigned to this employee", null);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (false, "An error occurred while retrieving the employee's office", null);
        }
    }



    /// <summary>
    /// Er kunnen medewewerkers van Car and All toegevoegd worden aan de database, door middel van hun gegevens.
    /// De database voorziet de gegevens van een automatische id
    /// </summary>
    /// <param name="personData"></param>
    /// <returns></returns>
    public async Task<(bool status, string message)> AddStaff(Object[] personData)
    {
        try
        {
            string query =
                "INSERT INTO STAFF (FirstName, LastName, Password, Email, Office) VALUES (@F, @L, @P, @E, @O)";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // Alle parameters worden ingevuld
                command.Parameters.AddWithValue("@F", personData[0]);
                command.Parameters.AddWithValue("@L", personData[1]);
                command.Parameters.AddWithValue("@P", _hash.createHash(personData[2].ToString()));
                command.Parameters.AddWithValue("@E", personData[3]);
                command.Parameters.AddWithValue("@O", personData[4]);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Er wordt gekeken of de gegevens zijn ingevoerd in de DataBase
                    return (true, "Data inserted");
                }

                return (false, "Data not inserted");
            }
        }
        catch (MySqlException ex)
        {
            return (false, ex.ToString());
        }
    }

    /// <summary>
    /// Er wordt gekeken door middel van het id of de gebruiker een geldig medewerkers id heeft
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> IsUserEmployee(int id)
    {
        try
        {
            string query =
                "SELECT ID FROM Customer WHERE ID = @id AND AccountType = 'Business'"; //Geef customer met gegeven id als hij NIET een particulier is.


            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@id", id);

                var result = await command.ExecuteScalarAsync();

                if (result != null && result.ToString() == id.ToString())
                {
                    // Er wordt gekeken of de gebruiker bestaat
                    return true;
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Unexpected error: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool Status, string Message)> AddPersonalCustomer(SignUpRequest request)
{
    // Checkt of het emailadres al in gebruik is
    var emailCheck = checkUsageEmailAsync(request.Email);
    
    // Valideert het emailadres
    bool validEmail = EmailChecker.IsValidEmail(request.Email);
    
    // Valideert de geboortedatum
    bool validBirthday = BirthdayChecker.IsValidBirthday(request.BirthDate);
    
    // Valideert het telefoonnummer
    bool validPhoneNumber = TelChecker.IsValidPhoneNumber(request.TelNumber);
    
    // Valideert het wachtwoord
    (bool isValidPassword, string passwordError) validPassword = PasswordChecker.IsValidPassword(request.Password);
    
    // Wacht op het resultaat van de emailcheck
    var emailStatus = await emailCheck;

    // Als alles geldig is en het emailadres nog niet in gebruik is
    if (validEmail && validBirthday && validPhoneNumber && validPassword.isValidPassword && !emailStatus.status)
    {
        // Maakt een klant aan
        var customer = await addCustomerAsync(new object[]
        {
            request.Adres,
            request.TelNumber,
            _hash.createHash(request.Password),
            request.Email,
            request.FirstName,
            request.LastName
        });

        // Als het aanmaken van de klant mislukt, geef een foutmelding
        if (!customer.status)
        {
            return (false, customer.message);
        }

        // Voeg extra persoonlijke gegevens toe
        await addPersonalCustomerAsync(new object[]
        {
            customer.newUserID,
            request.BirthDate
        });

        // Klant succesvol toegevoegd
        return (true, "Customer added");
    }
    // Als het emailadres al in gebruik is
    else if (!validEmail)
    {
        return (false, "Email al in gebruik");
    }
    // Als de geboortedatum niet geldig is
    else if (!validBirthday)
    {
        return (false, "Ongeldige geboortedatum");
    }
    // Als het wachtwoord niet geldig is
    else if (!validPassword.isValidPassword)
    {
        return (false, validPassword.passwordError);
    }
    // Als het emailadres al in gebruik is (check via emailStatus)
    else if (emailStatus.status)
    {
        return (false, emailStatus.message);
    }
    // Onverwachte fout
    else
    {
        return (false, "Er is een onverwachte fout opgetreden");
    }
}

    /// <summary>
    /// AddCustomerChecks kijkt of alle ingevoerde velden geldig zijn. Als een check faalt falen alle checks
    /// </summary>
    /// <param name="isPrivate"></param>
    /// <param name="customer"></param>
    /// <param name="detailed"></param>
    /// <returns></returns>
    private async Task<(bool Status, string Message)> AddCustomerChecks(bool isPrivate, SignUpRequestCustomer? customer,
        SignUpRequestCustomerPrivate detailed)
    {
        if (isPrivate)
        {
            // Validatie voor private klant: geboortedatum en telefoonnummer
            if (!BirthdayChecker.IsValidBirthday(detailed.BirthDate))
            {
                return (false, "No Valid Birthday");
            }
            else if (!TelChecker.IsValidPhoneNumber(detailed.TelNumber))
            {
                return (false, "No valid Phonenumber");
            }
            else
            {
                return (true, "Valid Details");
            }
        }
        else
        {
            // Validatie voor niet-private klant: controleer of het domein van het emailadres bestaat
            DomainEmailChecker domainEmailChecker = new DomainEmailChecker(_connector);
            (bool Found, int KvK) checkDomain = await domainEmailChecker.DomainExists(customer.Email);

            if (!checkDomain.Found)
            {
                return (false, "Domain does not exists");
            }
            else
            {
                customer.KvK = checkDomain.KvK;
            }
        }

        // Controleer of het emailadres geldig is
        var emailCheck = checkUsageEmailAsync(customer.Email);
        (bool isValidPassword, string passwordError) validPassword = PasswordChecker.IsValidPassword(customer.Password);
    
        // Controleer of het wachtwoord geldig is
        if (!EmailChecker.IsValidEmail(customer.Email))
        {
            return (false, "No Valid Email Format");
        }
        else if (!validPassword.isValidPassword)
        {
            return (false, validPassword.passwordError);
        }

        // Controleer of het emailadres al in gebruik is
        (bool Status, string Message) emailStatus = await emailCheck;
        return (!emailStatus.Status, emailStatus.Message);
    }

    /// <summary>
    /// AddPrivateCustomerDetails zorgt ervoor dat alle extra details van particuliere huurders toegevoegd worden in de Private tabel.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <param name="connections"></param>
    /// <returns></returns>
    private async Task<(int StatusCode, string Message)> AddPrivateCustomerDetails(SignUpRequestCustomerPrivate request,
        int userId, MySqlConnection connections)
    {
        // Controleer de klantgegevens voordat je ze toevoegt
        (bool Status, string Message) checks = await AddCustomerChecks(true, null, request);

        if (checks.Status)
        {
            try
            {
                // SQL-query om klantgegevens in de Private tabel in te voegen
                string query =
                    "INSERT INTO Private (ID, FirstName, LastName, TelNum, Adres, BirthDate) VALUES (@I, @F, @L, @T, @A, @B)";
                using (var command = new MySqlCommand(query, connections))
                {
                    command.Parameters.AddWithValue("@I", userId);
                    command.Parameters.AddWithValue("@F", request.FirstName);
                    command.Parameters.AddWithValue("@L", request.LastName);
                    command.Parameters.AddWithValue("@T", request.TelNumber);
                    command.Parameters.AddWithValue("@A", request.Adres);
                    command.Parameters.AddWithValue("@B", request.BirthDate);

                    // Voer de query uit en check of het toevoegen gelukt is
                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (200, "Account Succesfully Added");
                    }

                    return (500, "Unexpected Error Occured");
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Z");
                return (500, ex.Message);
            }
            catch (Exception ex)
            {
                return (500, ex.Message);
            }
        }

        // Als de klantgegevens niet valide zijn, geef dan een foutmelding terug
        return (412, checks.Message);
    }


    /// <summary>
    /// AddCustomer krijgt SignUpRequestCustomer binnen en SignUpRequestCustomerPrivate.
    /// Elk account heeft als algemene registratie een email en accounttype.
    /// 
    /// Bij een particulier (Private) account geeft de gebruiker alleen een email en een accounttype door.
    /// Bij een zakelijk (Business) account geeft de gebruiker een email, accounttype en KvK nummer door
    /// (ID's worden ingevuld door de database)
    /// 
    /// Bij een particulier account wordt nog een tweede query uitgevoerd (zie AddPrivateCustomerDetails).
    /// Als deze query mocht falen wordt de gehele transactie gerolledbacked.
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="privateRequest"></param>
    /// <returns></returns>
    public async Task<(int StatusCode, string Message)> AddCustomer(SignUpRequestCustomer request,
    SignUpRequestCustomerPrivate privateRequest)
{
    // Controleer de klantgegevens voordat je ze toevoegt
    (bool Status, string Message) checks = await AddCustomerChecks(request.IsPrivate, request, privateRequest);

    if (checks.Status)
    {
        try
        {
            string query;

            // Als het een private klant is, maak dan een query zonder KvK
            if (request.AccountType.Equals("Private"))
            {
                query = "INSERT INTO Customer (Email, AccountType, Password) VALUES (@E, @A, @P)";
            }
            else
            {
                // Als het een business klant is, voeg KvK toe aan de query
                query = "INSERT INTO Customer (Email, KvK, AccountType, Password) VALUES (@E, @K, @A, @P)";
            }

            // Maak verbinding met de database en start een transactie
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var transaction = connection.BeginTransaction())
            {
                // Voeg de klantgegevens toe aan de query
                command.Parameters.AddWithValue("@E", request.Email);
                command.Parameters.AddWithValue("@A", request.AccountType);

                string hashedPassword = _hash.createHash(request.Password);
                command.Parameters.AddWithValue("@P", hashedPassword);

                // Voeg KvK toe voor business klanten
                if (request.AccountType.Equals("Business"))
                {
                    command.Parameters.AddWithValue("@K", request.KvK);
                }

                try
                {
                    // Voer de query uit en controleer of de klant is toegevoegd
                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        if (request.AccountType.Equals("Private"))
                        {
                            // Verkrijg het laatst ingevoegde klant-ID
                            command.CommandText = "SELECT LAST_INSERT_ID();";
                            int userId = Convert.ToInt32(await command.ExecuteScalarAsync());

                            // Voeg private klantdetails toe
                            (int StatusCode, string Message) response =
                                await AddPrivateCustomerDetails(privateRequest, userId,
                                    (MySqlConnection)connection);

                            // Als er een fout is, verwijder de klant en rolback de transactie
                            if (response.StatusCode == 500 || response.StatusCode == 412)
                            {
                                string deleteCustomerQuery = "DELETE FROM Customer WHERE ID = @I";
                                using (var deleteCommand = new MySqlCommand(deleteCustomerQuery,
                                           (MySqlConnection)connection))
                                {
                                    deleteCommand.Parameters.AddWithValue("@I", userId);
                                    await deleteCommand.ExecuteNonQueryAsync();
                                }

                                transaction.Rollback();
                                return response;
                            }
                        }

                        // Commit de transactie als alles succesvol is
                        transaction.Commit();
                        return (200, "Customer Account Added Successfully");
                    }

                    return (500, "Unexpected Error Occurred");
                }
                catch (Exception ex)
                {
                    // Als er een fout is, rollback de transactie en geef een foutmelding terug
                    Console.WriteLine("Error: " + ex.Message);
                    transaction.Rollback();
                    return (500, ex.Message);
                }
            }
        }
        catch (MySqlException ex)
        {
            // Fout bij databaseverbinding
            Console.WriteLine("Error: " + ex.Message);
            return (500, ex.Message);
        }
        catch (Exception ex)
        {
            // Algemene foutafhandeling
            Console.WriteLine("Error: " + ex.Message);
            return (500, ex.Message);
        }
    }

    // Als de klantgegevens niet geldig zijn, geef dan een foutmelding terug
    return (412, checks.Message);
}


    private string CreateUpdateQuery(string tabel, IList<object[]> data)
    {
        // Begin van de update query
        string query = $"UPDATE {tabel} SET";

        for (int i = 1; i < data.Count; i++)
        {
            // Als het veld een wachtwoord is, hash dan de waarde
            if (((string)data[i][0]).Equals("Password"))
            {
                query += $" Password = '{_hash.createHash((string)data[i][1])}'";
            }
            // Als het datatype van het veld een int is
            else if (data[i][2].Equals("System.Int32"))
            {
                query += $" {data[i][0]} = {data[i][1]}";
            }
            else
            {
                // Anders wordt de waarde als string ingevoegd
                query += $" {data[i][0]} = '{data[i][1]}'";
            }

            // Voeg komma toe als dit niet het laatste item is
            if (i != data.Count - 1)
            {
                query += ",";
            }
        }

        // Voeg de WHERE clausule toe
        query += $" WHERE {data[0][0]} = {data[0][1]}";

        Console.WriteLine(query);

        return query;
    }

    public async Task<(int StatusCode, string Message)> ChangeVehicleManagerInfo(ChangeVehicleManagerInfo request)
{
    try
    {
        // Logging ontvangen verzoek om voertuigmanager bij te werken
        Console.WriteLine($"Received request to update Vehicle Manager with ID: {request.ID}");
        Console.WriteLine($"New Email: {request.Email}");

        // Controleer of het opgegeven e-mailadres leeg is
        if (string.IsNullOrEmpty(request.Email))
        {
            Console.WriteLine("Error: The provided email is empty.");
            return (400, "Email cannot be empty.");
        }

        // Hash het nieuwe wachtwoord (voordat het wordt bijgewerkt)
        string hashedPassword = _hash.createHash(request.Password);
        Console.WriteLine($"Hashed Password: {hashedPassword}");

        // Valideer of het nieuwe wachtwoord aan de vereiste indeling voldoet
        var (isValid, errorMessage) = PasswordChecker.IsValidPassword(request.Password);
        if (!isValid)
        {
            Console.WriteLine($"Error: {errorMessage}");
            return (400, errorMessage);
        }

        // Controleer of het e-mailadres moet worden bijgewerkt (alleen doorvoeren van update als het e-mailadres is veranderd)
        string validateQuery = @"
        SELECT Email FROM VehicleManager
        WHERE ID = @ID";

        using (var connection = _connector.CreateDbConnection())
        {
            // Controleer of de databaseverbinding geldig is
            if (connection == null)
            {
                Console.WriteLine("Database connection is null.");
                return (500, "Database connection failed.");
            }

            // Open de databaseverbinding als deze nog niet open is
            if (connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Opening database connection...");
                connection.Open();
                Console.WriteLine("Connection opened successfully.");
            }

            using (var command = new MySqlCommand(validateQuery, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@ID", request.ID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string existingEmail = reader.GetString("Email");

                        // Als het e-mailadres niet veranderd is, geen update uitvoeren
                        if (existingEmail == request.Email)
                        {
                            Console.WriteLine("Email is the same. No update needed.");
                            return (200, "No changes to the email address.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Vehicle Manager not found.");
                        return (404, "Vehicle Manager not found.");
                    }
                }
            }
        }

        // Als het e-mailadres is veranderd, voer de update uit
        string updateQuery = @"
        UPDATE VehicleManager
        SET Email = @Email,
            Password = @Password
        WHERE ID = @ID";

        using (var connection = _connector.CreateDbConnection())
        {
            // Controleer of de databaseverbinding geldig is
            if (connection == null)
            {
                Console.WriteLine("Database connection is null.");
                return (500, "Database connection failed.");
            }

            // Open de databaseverbinding als deze nog niet open is
            if (connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Opening database connection...");
                connection.Open();
                Console.WriteLine("Connection opened successfully.");
            }

            using (var command = new MySqlCommand(updateQuery, (MySqlConnection)connection))
            {
                command.Parameters.AddWithValue("@ID", request.ID);
                command.Parameters.AddWithValue("@Email", request.Email);
                command.Parameters.AddWithValue("@Password", hashedPassword);

                try
                {
                    // Voer de update uit en controleer of er rijen zijn bijgewerkt
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        // Bijgewerkte voertuigmanager
                        Console.WriteLine("Vehicle Manager updated successfully.");
                        return (200, "Vehicle Manager updated successfully.");
                    }
                    else
                    {
                        // Geen rijen bijgewerkt, mogelijk geen voertuigmanager gevonden
                        Console.WriteLine("Error: Vehicle Manager not found.");
                        return (404, "Vehicle Manager not found.");
                    }
                }
                catch (Exception ex)
                {
                    // Fout tijdens query-executie
                    Console.WriteLine($"Error executing query: {ex.Message}");
                    return (500, $"Error executing query: {ex.Message}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Algemene foutafhandeling
        Console.WriteLine($"Error: {ex.Message}");
        return (500, $"Error: {ex.Message}");
    }
}

   public async Task<(int StatusCode, string Message)> ChangeBusinessInfo(ChangeBusinessRequest request)
{
    try
    {
        // Query om de KvK op te halen voor de opgegeven VehicleManager ID
        string query = "SELECT Business FROM VehicleManager WHERE ID = @I";

        using (var connection = (MySqlConnection)_connector.CreateDbConnection())
        using (var transaction = connection.BeginTransaction()) // Begin een transactie voor veiligheid
        using (var command = new MySqlCommand(query, connection))
        {
            // Voeg de VehicleManager ID toe aan de query
            command.Parameters.AddWithValue("@I", request.VehicleManagerInfo?.ID);

            using (var reader = await command.ExecuteReaderAsync())
            {
                // Als de voertuigmanager wordt gevonden
                if (await reader.ReadAsync())
                {
                    // Haal de KvK waarde op en zet deze in het request object
                    request.BusinessInfo.KvK = Convert.ToInt32(reader.GetValue(0));

                    await reader.CloseAsync(); // Sluit de reader

                    // Voer de wijziging van de bedrijfsinformatie uit
                    var changeBusiness = await ChangeBusinessData(request.BusinessInfo, connection);

                    // Als er een fout optreedt bij het wijzigen van de bedrijfsinformatie
                    if (changeBusiness.StatusCode != 200)
                    {
                        // Rolback de transactie bij fout
                        transaction.Rollback();
                        return (changeBusiness.StatusCode, changeBusiness.Message);
                    }
                    else
                    {
                        // Commit de transactie bij succes
                        transaction.Commit();
                        return (200, "Update Successful");
                    }
                }

                // Als de voertuigmanager niet wordt gevonden of KvK ontbreekt
                transaction.Rollback();
                return (404, "VehicleManager not found or KvK is missing");
            }
        }
    }
    catch (MySqlException ex)
    {
        // Fout bij MySQL-query
        return (500, ex.Message);
    }
    catch (OverflowException ex)
    {
        // Fout bij overloop van getal
        return (500, ex.Message);
    }
    catch (Exception ex)
    {
        // Algemene foutafhandeling
        return (500, ex.Message);
    }
}

public async Task<(int StatusCode, string Domain)> GetBusinessDomainByKvK(int KvK)
{
    try
    {
        // SQL query om het domein op te halen voor het opgegeven KvK nummer
        string query = "SELECT Domain FROM Business WHERE KvK = @KvK";

        using (var connection = (MySqlConnection)_connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, connection))
        {
            // Voeg het KvK nummer toe aan de query
            command.Parameters.AddWithValue("@KvK", KvK);

            using (var reader = await command.ExecuteReaderAsync())
            {
                // Als er een resultaat is voor de opgegeven KvK
                if (await reader.ReadAsync())
                {
                    // Haal het domein op en geef het terug
                    string domain = reader.GetString("Domain");
                    return (200, domain); // Succesvol resultaat, met het domein
                }

                // Als er geen bedrijfsinformatie gevonden is voor de opgegeven KvK
                return (404, "Business with the specified KvK not found.");
            }
        }
    }
    catch (MySqlException ex)
    {
        // Fout bij MySQL-query
        return (500, ex.Message);
    }
    catch (Exception ex)
    {
        // Algemene foutafhandeling voor andere uitzonderingen
        return (500, ex.Message);
    }
}

private async Task<(int StatusCode, string Message)> ChangeBusinessData(ChangeBusinessInfo businessInfo, MySqlConnection connection)
{
    // SQL-query voor het bijwerken van de bedrijfsgegevens
    string updateQuery = "UPDATE Business SET BusinessName = @BusinessName, Adres = @Adres, ContactEmail = @ContactEmail, Abonnement = @Abonnement WHERE KvK = @KvK";

    using (var command = new MySqlCommand(updateQuery, connection))
    {
        // Voeg de parameters toe aan de query
        command.Parameters.AddWithValue("@KvK", businessInfo.KvK);
        command.Parameters.AddWithValue("@BusinessName", businessInfo.BusinessName);
        command.Parameters.AddWithValue("@Adres", businessInfo.Adres);
        command.Parameters.AddWithValue("@ContactEmail", businessInfo.ContactEmail);
        command.Parameters.AddWithValue("@Abonnement", businessInfo.Abonnement);

        try
        {
            // Voer de query uit en krijg het aantal aangetaste rijen terug
            int rowsAffected = await command.ExecuteNonQueryAsync();
            
            // Als er een rij is aangetast, was de update succesvol
            if (rowsAffected > 0)
            {
                return (200, "Business information updated successfully");
            }
            else
            {
                // Geen rijen aangetast, dus de business met de opgegeven KvK is niet gevonden
                return (404, "Business not found with the specified KvK");
            }
        }
        catch (MySqlException ex)
        {
            // Als er een databasefout optreedt
            return (500, $"Database error: {ex.Message}");
        }
    }
}

public async Task<List<string>> GetAllSubscriptionsAsync()
{
    try
    {
        string query = "SELECT Type FROM Abonnement";

        // Open databaseverbinding en voer de query uit
        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        using (var reader = await command.ExecuteReaderAsync())
        {
            var subscriptions = new List<string>();

            // Lees resultaten en voeg ze toe aan de lijst
            while (await reader.ReadAsync())
            {
                subscriptions.Add((string)reader.GetValue(0));
            }

            return subscriptions;
        }
    }
    catch (MySqlException e)
    {
        // Fout bij databaseverbinding of query
        Console.WriteLine(e.Message);
        return null;
    }
    catch (Exception e)
    {
        // Algemene foutafhandeling
        Console.WriteLine(e);
        return null;
    }
}
public async Task<Subscription> GetSubscriptionDataAsync(int id)
{
    try
    {
        string query = "SELECT ID, Type, Description, Price FROM Abonnement WHERE ID = @Id";

        // Open databaseverbinding en voer de query uit
        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@Id", id);
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    // Haal gegevens op uit de reader
                    var type = reader["Type"].ToString();
                    var description = reader["Description"].ToString();
                    var price = reader["Price"].ToString().Equals("") ? 0 : Convert.ToDouble(reader["Price"]);
                    
                    return new Subscription
                    {
                        Id = id,
                        Type = type,
                        Description = description,
                        Price = price
                    };
                }
            }
        }

        // Als er geen resultaat is
        return null;
    }
    catch (Exception ex)
    {
        // Foutafhandeling
        Console.WriteLine(ex.Message);
        return null;
    }
}

public async Task<List<int>> GetSubscriptionIdsAsync()
{
    try
    {
        string query = "SELECT ID FROM Abonnement";

        // Open databaseverbinding en voer de query uit
        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        using (var reader = await command.ExecuteReaderAsync())
        {
            var ids = new List<int>();
            while (await reader.ReadAsync())
            {
                // Voeg ID toe aan de lijst
                ids.Add(Convert.ToInt32(reader.GetValue(0)));
            }

            return ids;
        }
    }
    catch (MySqlException ex)
    {
        // Fout bij databaseverbinding of query
        Console.WriteLine(ex.Message);
        return null;
    }
    catch (Exception ex)
    {
        // Algemene foutafhandeling
        Console.WriteLine(ex.Message);
        return null;
    }
}

public async Task<Dictionary<string, object>> GetCustomerDetails(int id)
{
    string query = $"SELECT * FROM Customer WHERE ID = {id}";
    Dictionary<string, object> info = new Dictionary<string, object>();

    try
    {
        // Open databaseverbinding en voer de query uit
        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        using (var reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                // Voeg elk veld toe aan de dictionary
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
        // Fout bij databaseverbinding of query
        Console.WriteLine(ex.Message);
        throw;
    }
    catch (Exception ex)
    {
        // Algemene foutafhandeling
        Console.WriteLine(ex.Message);
        throw;
    }
}

public async Task<VehicleManager> GetVehicleManagerInfoAsync(int id)
{
    try
    {
        string query = "SELECT ID, Email, Password, Business FROM VehicleManager WHERE ID = @Id";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@Id", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    // Gegevens uit de database lezen en teruggeven als een VehicleManager-object
                    return new VehicleManager
                    {
                        Id = reader.GetInt32("ID"),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        Business = reader["Business"].ToString()
                    };
                }
            }
        }

        return null;  // Geen gegevens gevonden
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        return null;  // Foutafhandeling bij uitzondering
    }
}

public async Task<List<Customer>> GetCustomersByBusinessNumberAsync(string Kvk)
{
    try
    {
        string query = "SELECT ID, Email, Kvk FROM Customer WHERE Kvk = @Kvk";

        using (var connection = _connector.CreateDbConnection())
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@Kvk", Kvk.Trim()); // Verwijder eventuele leidende of achterwaartse spaties

            using (var reader = await command.ExecuteReaderAsync())
            {
                var customers = new List<Customer>();

                while (await reader.ReadAsync())
                {
                    customers.Add(new Customer
                    {
                        Id = reader.GetInt32("ID"),
                        Email = reader["Email"].ToString(),
                        Kvk = reader["Kvk"].ToString()
                    });
                }

                return customers; // Retourneer de lijst van klanten
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in GetCustomersByBusinessNumberAsync: {ex.Message}"); // Foutmelding loggen
        return null; // Retourneer null in geval van een fout
    }
}

   public async Task<bool> UpdateCustomerAsync(int id, string email, string password)
{
    try
    {
        // Controleer of het ID geldig is
        if (id <= 0)
        {
            Console.WriteLine("Error: Invalid ID.");
            return false;
        }

        // Controleer of email en wachtwoord niet leeg zijn
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine("Error: Email or Password cannot be empty.");
            return false;
        }

        // Hash het nieuwe wachtwoord (voor de update)
        string hashedPassword = _hash.createHash(password);

        // Controleer de validiteit van het wachtwoord
        var (isValid, errorMessage) = PasswordChecker.IsValidPassword(password);
        if (!isValid)
        {
            Console.WriteLine($"Error: {errorMessage}");
            return false;
        }

        // SQL-query voor het bijwerken van klantgegevens
        string updateQuery = "UPDATE Customer SET Email = @Email, Password = @Password WHERE ID = @ID";

        using (var connection = _connector.CreateDbConnection())
        {
            try
            {
                // Open de verbinding indien nodig
                if (connection?.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var command = new MySqlCommand(updateQuery, (MySqlConnection)connection))
                {
                    // Voeg de parameters toe aan de query
                    command.Parameters.AddWithValue("@ID", id);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", hashedPassword);

                    // Voer de query uit
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    // Controleer of de klant is gevonden en bijgewerkt
                    if (rowsAffected > 0)
                    {
                        return true;  // Succesvol bijgewerkt
                    }
                    else
                    {
                        Console.WriteLine("Error: Customer not found.");
                        return false;
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
            finally
            {
                // Zorg ervoor dat de verbinding wordt gesloten
                if (connection?.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
        return false;
    }
}

    public class VehicleManager
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Business { get; set; }
    }
    public class Customer
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Kvk { get; set; }
    }

}
