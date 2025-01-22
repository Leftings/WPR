using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WPR.Database;
using Microsoft.AspNetCore.Http.HttpResults;
using WPR.Utils;
using WPR.Hashing;
using Org.BouncyCastle.Crypto.Prng;
using WPR.Cryption;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualBasic;
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

    /*private async Task<bool> CheckPassword(string username, string password, string table)
    {
        string query = $@"SELECT password FROM {table} WHERE LOWER(email) = LOWER(@Email)";

        using (var connection = _connector.CreateDbConnection())
        
        using (var command = new MySqlCommand(query, (MySqlConnection)connection))
        {
            command.Parameters.AddWithValue("@Email", username);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    string passwordUser = reader.GetString("Password");

                    return _hash.(password, passwordUser);
                }

                return false;
            }
        }
    }
    */

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
            string query = "INSERT INTO Customer (Adres, Telnum, Password, Email, FirstName, LastName) values (@A, @T, @P, @E, @F, @L)";

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
            using (var command = new MySqlCommand(query, (MySqlConnection) connection))
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
            // Handle database errors
            await Console.Error.WriteLineAsync($"Database error: {ex.Message}");
            return (false, "Database error: " + ex.Message);
        }
        catch (Exception ex)
        {
            // Handle other errors
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
    public async Task<(bool status, string message)> GetKindEmployeeAsync(string userId)
    {
        try
        {
            string query = "SELECT Office FROM Staff WHERE ID = @I";

            // Er wordt een connectie aangemaakt met de DataBase met bovenstaande query 
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                // De parameter wordt ingevuld
                command.Parameters.AddWithValue("@I", userId);

                var result = await command.ExecuteScalarAsync();

                // De office wordt meegegeven
                return (true, result.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return (false, ex.ToString());
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
            string query = "INSERT INTO STAFF (FirstName, LastName, Password, Email, Office) VALUES (@F, @L, @P, @E, @O)";

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
        catch(MySqlException ex)
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
            string query = "SELECT ID FROM Customer WHERE ID = @id AND AccountType = 'Business'"; //Geef customer met gegeven id als hij NIET een particulier is.


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
        var emailCheck = checkUsageEmailAsync(request.Email);
        bool validEmail = EmailChecker.IsValidEmail(request.Email);
        bool validBirthday = BirthdayChecker.IsValidBirthday(request.BirthDate);
        bool validPhoneNumber = TelChecker.IsValidPhoneNumber(request.TelNumber);
        (bool isValidPassword, string passwordError) validPassword = PasswordChecker.IsValidPassword(request.Password);
        var emailStatus = await emailCheck;

        Console.WriteLine(validBirthday);

        if (validEmail && validBirthday && validPhoneNumber && validPassword.isValidPassword && !emailStatus.status)
        {
            var customer = await addCustomerAsync(new object[]
            {
                request.Adres,
                request.TelNumber,
                _hash.createHash(request.Password),
                request.Email,
                request.FirstName,
                request.LastName
            });

            if (!customer.status)
            {
                return (false, customer.message);
            }

            await addPersonalCustomerAsync(new object[]
            {
                customer.newUserID,
                request.BirthDate
            });

            return (true, "Customer added");
        }
        else if (!validEmail)
        {
            return (false, "Email allready in use");
        }
        else if (!validBirthday)
        {
            return (false, "Invalid birthday");
        }
        else if (!validPassword.isValidPassword)
        {
            return (false, validPassword.passwordError);
        }
        else if (emailStatus.status)
        {
            return (false, emailStatus.message);
        }
        else
        {
            return (false, "An unexpected error occured");
        }
    }

    /// <summary>
    /// AddCustomerChecks kijkt of alle ingevoerde velden geldig zijn. Als een check faalt falen alle checks
    /// </summary>
    /// <param name="isPrivate"></param>
    /// <param name="customer"></param>
    /// <param name="detailed"></param>
    /// <returns></returns>
    private async Task<(bool Status, string Message)> AddCustomerChecks(bool isPrivate, SignUpRequestCustomer? customer, SignUpRequestCustomerPrivate detailed)
    {
        if (isPrivate)
        {
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
        
        var emailCheck = checkUsageEmailAsync(customer.Email);
        (bool isValidPassword, string passwordError) validPassword = PasswordChecker.IsValidPassword(customer.Password);
        if (!EmailChecker.IsValidEmail(customer.Email))
        {
            return (false, "No Valid Email Format");
        }
        else if (!validPassword.isValidPassword)
        {
            return (false, validPassword.passwordError);
        }
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
    private async Task<(int StatusCode, string Message)> AddPrivateCustomerDetails(SignUpRequestCustomerPrivate request, int userId, MySqlConnection connections)
    {
        (bool Status, string Message) checks = await AddCustomerChecks(true, null, request);

        if (checks.Status)
        {
            try
            {
                string query = "INSERT INTO Private (ID, FirstName, LastName, TelNum, Adres, BirthDate) VALUES (@I, @F, @L, @T, @A, @B)";
                using (var command = new MySqlCommand(query, connections))
                {
                    command.Parameters.AddWithValue("@I", userId);
                    command.Parameters.AddWithValue("@F", request.FirstName);
                    command.Parameters.AddWithValue("@L", request.LastName);
                    command.Parameters.AddWithValue("@T", request.TelNumber);
                    command.Parameters.AddWithValue("@A", request.Adres);
                    command.Parameters.AddWithValue("@B", request.BirthDate);

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
    public async Task<(int StatusCode, string Message)> AddCustomer(SignUpRequestCustomer request, SignUpRequestCustomerPrivate privateRequest)
    {
        (bool Status, string Message) checks = await AddCustomerChecks(false, request, privateRequest);

        if (checks.Status)
        {
            try
            {
                string query;

                if (request.AccountType.Equals("Private"))
                {
                    query = "INSERT INTO Customer (Email, AccountType, Password) VALUES (@E, @A, @P)";
                }
                else
                {
                    query = "INSERT INTO Customer (Email, KvK, AccountType, Password) VALUES (@E, @K, @A, @P)";
                }

                using (var connection = _connector.CreateDbConnection())
                using (var command = new MySqlCommand(query, (MySqlConnection)connection))
                using (var transaction = connection.BeginTransaction())
                {
                    command.Parameters.AddWithValue("@E", request.Email);
                    command.Parameters.AddWithValue("@A", request.AccountType);
                    command.Parameters.AddWithValue("@P", _hash.createHash(request.Password));

                    if (request.AccountType.Equals("Business"))
                    {
                        command.Parameters.AddWithValue("@K", request.KvK);
                    }

                    try
                    {
                        if (await command.ExecuteNonQueryAsync() > 0)
                        {

                            if (request.AccountType.Equals("Private"))
                            {
                                // UserId van het account verkrijgen
                                command.CommandText = "SELECT LAST_INSERT_ID();";
                                int userId = Convert.ToInt32(await command.ExecuteScalarAsync());

                                (int StatusCode, string Message) response = await AddPrivateCustomerDetails(privateRequest, userId, (MySqlConnection)connection);

                                // Bij falen van query van toevoegen particulier account wordt een rollback van de transactie uitgevoerd
                                if (response.StatusCode == 500 || response.StatusCode == 412)
                                {
                                    string deleteCustomerQuery = "DELETE FROM Customer WHERE ID = @I";
                                    using (var deleteCommand = new MySqlCommand(deleteCustomerQuery, (MySqlConnection)connection))
                                    {
                                        deleteCommand.Parameters.AddWithValue("@I", userId);
                                        await deleteCommand.ExecuteNonQueryAsync();
                                    }

                                    transaction.Rollback();
                                    return response; 
                                }
                            }

                            transaction.Commit();
                            return (200, "Customer Account Added Successfully");
                        }

                        return (500, "Unexpected Error Occurred");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("X");
                        transaction.Rollback();
                        return (500, ex.Message);
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Y");
                return (500, ex.Message);
            }
            catch (Exception ex)
            {
                return (500, ex.Message);
            }
        }
        
        return (412, checks.Message);
    }

    private string CreateUpdateQuery (string tabel, IList<object[]> data)
    {
        string query = $"UPDATE {tabel} SET";

        for (int i = 1; i < data.Count; i++)
        {
            if (((string)data[i][0]).Equals("Password"))
            {
                query += $" Password = '{_hash.createHash((string)data[i][1])}'";
            }
            else if (data[i][2].Equals("System.Int32"))
            {
                query += $" {data[i][0]} = {data[i][1]}";
            }
            else
            {
                query += $" {data[i][0]} = '{data[i][1]}'";
            }

            if (i != data.Count - 1)
            {
                query += ",";
            }
        }

        query += $" WHERE {data[0][0]} = {data[0][1]}";

        Console.WriteLine(query);

        return query;
    }

    private async Task<(int StatusCode, string Message)> ChangeVehicleManagerInfo(ChangeVehicleManagerInfo request, MySqlConnection connection)
    {
        try
        {
            List<object[]> data = new List<object[]>();

            // gegevens worden uit de lijst gehaald (naam van de collom, de nieuwe waarde, soort waarde)
            foreach (var propertyInfo in typeof(ChangeVehicleManagerInfo).GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(request);
                var propertyType = propertyInfo.PropertyType;

                if (!propertyValue.Equals(""))
                {
                    data.Add(new object[] {propertyName, propertyValue, propertyType});
                }
            }    
            
            if (data.Count > 1)
            {
                using (var command = new MySqlCommand(CreateUpdateQuery("VehicleManager", data), connection))
                {
                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (200, "VehicleManager Updated");
                    }
                    return (417, "VehicleManager Not Updated");
                }
            }

            return (200, "No Data To Update");
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

    private async Task<(int StatusCode, string Message)> ChangeBusinessInfo(ChangeBusinessInfo request, MySqlConnection connection)
    {
        try
        {
            List<object[]> data = new List<object[]>();

            // gegevens worden uit de lijst gehaald (naam van de collom, de nieuwe waarde, soort waarde)
            foreach (var propertyInfo in typeof(ChangeBusinessInfo).GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(request);
                var propertyType = propertyInfo.PropertyType;

                if (!propertyValue.Equals(""))
                {
                    data.Add(new object[] {propertyName, propertyValue, propertyType});
                }
            }

            if (data.Count > 1)
            {
                using (var command = new MySqlCommand(CreateUpdateQuery("Business", data), connection))
                {
                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (200, "Business Updated");
                    }
                    return (417, "Business Not Updated");
                }
            }
            
            return (200, "No Data To Update");
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

    public async Task<(int StatusCode, string Message)> ChangeBusinessInfo(ChangeBusinessRequest request)
    {
        try
        {
            string query = "SELECT Business FROM VehicleManager WHERE ID = @I";
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var transaction = connection.BeginTransaction())
            {
                command.Parameters.AddWithValue("@I", request.VehicleManagerInfo.ID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        request.BusinessInfo.KvK = Convert.ToInt32(reader.GetValue(0));
                        await reader.CloseAsync();
                        var changeVehicleManager = ChangeVehicleManagerInfo(request.VehicleManagerInfo, (MySqlConnection)_connector.CreateDbConnection());
                        var changeBusiness = ChangeBusinessInfo(request.BusinessInfo, (MySqlConnection)_connector.CreateDbConnection());

                        Task.WaitAll(changeVehicleManager, changeBusiness);

                        if (changeVehicleManager.Result.StatusCode != 200)
                        {
                            return (changeVehicleManager.Result.StatusCode, changeVehicleManager.Result.Message);
                        }
                        else if (changeBusiness.Result.StatusCode != 200)
                        {
                            return (changeBusiness.Result.StatusCode, changeBusiness.Result.Message);
                        }
                        else
                        {
                            return (200, "Update Succesfull");
                        }
                    }
                    
                    transaction.Rollback();
                    return (500, "Unexpected Error Detected");
                }
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

    

    /*private string CreateUpdateQuery (string tabel, IList<object[]> data)
    {
        string query = $"UPDATE {tabel}";

        for (int i = 1; i < data.Count; i++)
        {
            if (((string)data[i][0]).Equals("Password"))
            {
                query += $" SET Password = '{_hash.createHash((string)data[i][1])}'";
            }
            else if (data[i][2].Equals("System.Int32"))
            {
                query += $" SET {data[i][0]} = {data[i][1]}";
            }
            else
            {
                query += $" SET {data[i][0]} = '{data[i][1]}'";
            }
        }

        query += $" WHERE {data[0][0]} = {data[0][1]}";

        return query;
    }

    private async Task<(int StatusCode, string Message)> ChangeVehicleManagerInfo(ChangeVehicleManagerInfo request, MySqlConnection connection)
    {
        try
        {
            List<object[]> data = new List<object[]>();

            // gegevens worden uit de lijst gehaald (naam van de collom, de nieuwe waarde, soort waarde)
            foreach (var propertyInfo in typeof(ChangeVehicleManagerInfo).GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(request);
                var propertyType = propertyInfo.PropertyType;

                if (!propertyValue.Equals(""))
                {
                    data.Add(new object[] {propertyName, propertyValue, propertyType});
                }
            }    
            
            if (data.Count > 1)
            {
                using (var command = new MySqlCommand(CreateUpdateQuery("VehicleManager", data), connection))
                {
                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (200, "VehicleManager Updated");
                    }
                    return (417, "VehicleManager Not Updated");
                }
            }

            return (200, "No Data To Update");
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

    private async Task<(int StatusCode, string Message)> ChangeBusinessInfo(ChangeBusinessInfo request, MySqlConnection connection)
    {
        try
        {
            List<object[]> data = new List<object[]>();

            // gegevens worden uit de lijst gehaald (naam van de collom, de nieuwe waarde, soort waarde)
            foreach (var propertyInfo in typeof(ChangeBusinessInfo).GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var propertyValue = propertyInfo.GetValue(request);
                var propertyType = propertyInfo.PropertyType;

                if (!propertyValue.Equals(""))
                {
                    data.Add(new object[] {propertyName, propertyValue, propertyType});
                }
            }

            if (data.Count > 1)
            {
                using (var command = new MySqlCommand(CreateUpdateQuery("Business", data), connection))
                {
                    if (await command.ExecuteNonQueryAsync() > 0)
                    {
                        return (200, "Business Updated");
                    }
                    return (417, "Business Not Updated");
                }
            }
            
            return (200, "No Data To Update");
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

    public async Task<(int StatusCode, string Message)> ChangeBusinessInfo(ChangeBusinessRequest request)
    {
        try
        {
            string query = "SELECT Business FROM VehicleManager WHERE ID = @I";
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            using (var transaction = connection.BeginTransaction())
            {
                command.Parameters.AddWithValue("@I", request.VehicleManagerInfo.ID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        request.BusinessInfo.KvK = Convert.ToInt32(reader.GetValue(0));
                        await reader.CloseAsync();
                        var changeVehicleManager = ChangeVehicleManagerInfo(request.VehicleManagerInfo, (MySqlConnection)_connector.CreateDbConnection());
                        var changeBusiness = ChangeBusinessInfo(request.BusinessInfo, (MySqlConnection)_connector.CreateDbConnection());

                        Task.WaitAll(changeVehicleManager, changeBusiness);

                        if (changeVehicleManager.Result.StatusCode != 200)
                        {
                            return (changeVehicleManager.Result.StatusCode, changeVehicleManager.Result.Message);
                        }
                        else if (changeBusiness.Result.StatusCode != 200)
                        {
                            return (changeBusiness.Result.StatusCode, changeBusiness.Result.Message);
                        }
                        else
                        {
                            return (200, "Update Succesfull");
                        }
                    }
                    
                    transaction.Rollback();
                    return (500, "Unexpected Error Detected");
                }
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
    */
}