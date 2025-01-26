using MySql.Data.MySqlClient;
using WPR.Database;
using WPR.Repository;
using WPR.Services;

namespace WPR.Email;

public class Reminders : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Connector _connector;

    // Constructor om de benodigde services te initialiseren
    public Reminders(IServiceProvider serviceProvider, Connector connector)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    // Methode die de herinneringsmail genereert voor een contract
    private string CreateReminderContract(Dictionary<string, object> Customer, Dictionary<string, object> Contract, Dictionary<string, object> Vehicle)
    {
        return @$"<h1>Herinnering voor voertuig </h1>Beste {Customer["Email"]},
        <p>Uw contract met referencie nummer {Contract["OrderId"]} gaat vanaf morgen in.</p>
        <p>Hier zijn de volgende details van uw lopende contract</p>
        <ul>
            <li>Referentie nummer: {Contract["OrderId"]}</li>
            <li>Start datum: {Contract["StartDate"]}</li>
            <li>Eind datum: {Contract["EndDate"]}</li>
            <li>Voertuig: {Vehicle["Brand"]} {Vehicle["Type"]}</li>
        </ul>
        ";
    }

    // Haalt de klantgegevens op voor een specifiek klant-ID
    private async Task<Dictionary<string, object>> GetCustomerInfo(int id, IServiceScope scope)
    {
        var customer = scope.ServiceProvider.GetRequiredService<Customer>();

        // Zet de klantgegevens
        await customer.SetDetailsAsync(id);
        
        // Haal de gegevens op en retourneer ze
        return await customer.GetDetailsAsync();
    }

    // Haalt de contractgegevens op voor een specifiek contract-ID
    private async Task<Dictionary<string, object>> GetContractInfo(int id, IServiceScope scope)
    {
        var contract = scope.ServiceProvider.GetRequiredService<Contract>();

        // Zet de contractgegevens
        await contract.SetDetailsAsync(id);
        
        // Haal de gegevens op en retourneer ze
        return await contract.GetDetailsAsync();
    }

    // Haalt de voertuiggegevens op voor een specifiek voertuig-ID
    private async Task<Dictionary<string, object>> GetVehicleInfo(int id, IServiceScope scope)
    {
        var vehicle = scope.ServiceProvider.GetRequiredService<Vehicle>();

        // Zet de voertuiggegevens
        await vehicle.SetDetailsAsync(id);
        
        // Haal de gegevens op en retourneer ze
        return await vehicle.GetDetailsAsync();
    }

    // Verstuurt de herinneringsmail naar de klant
    private async Task SendEmail(string body, string subject, string toEmail, object orderId, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
            string query = $"UPDATE Contract SET SendEmail = 'Yes' WHERE OrderId = {orderId}";

            // Voer de update query uit in de database
            using (var connection = _connector.CreateDbConnection())
            using (var command = new MySqlCommand(query, (MySqlConnection)connection))
            {
                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    // Stuur de e-mail als de update succesvol was
                    await emailService.Send(toEmail, subject, body);
                }
            }
        }
        catch (MySqlException ex)
        {
            // Fout bij MySQL database verbinding
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            // Algemeen fout
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    // Herinnering voor contracten die in de komende 24 uur beginnen
    private async Task<bool> ReminderContract24Hours()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var customer = scope.ServiceProvider.GetRequiredService<Customer>();
            var contract = scope.ServiceProvider.GetRequiredService<Contract>();
            var vehicle = scope.ServiceProvider.GetRequiredService<Vehicle>();
            var contracts = scope.ServiceProvider.GetRequiredService<IContractRepository>();

            // Haal de lijst van contracten op waarvoor een e-mail moet worden verzonden
            IList<int> ids = await contracts.GetContractsSendEmailAsync();

            foreach (int orderId in ids)
            {
                // Haal de gegevens voor elk contract, klant en voertuig op
                Dictionary<string, object> contractDic = new Dictionary<string, object>();
                Dictionary<string, object> customerDic = new Dictionary<string, object>();
                Dictionary<string, object> vehicleDic = new Dictionary<string, object>();

                contractDic = await GetContractInfo(orderId, scope);
                customerDic = await GetCustomerInfo(Convert.ToInt32(contractDic["Customer"]), scope);
                vehicleDic = await GetVehicleInfo(Convert.ToInt32(contractDic["FrameNrVehicle"]), scope);

                // Log de voertuig gegevens (ter controle)
                foreach (var item in vehicleDic)
                {
                    Console.WriteLine(item.Key);
                }

                // Maak de e-mail inhoud aan
                string email = CreateReminderContract(customerDic, contractDic, vehicleDic);

                // Verstuur de e-mail naar de klant
                await SendEmail(email, "Herinnering Huren Voertuig", customerDic["Email"].ToString(), contractDic["OrderId"], scope);
            }
            
            return true;
        }
    }

    // Voert de achtergrondtaak uit die de herinneringen periodiek verzendt
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Herinneringen voor contracten die in de komende 24 uur beginnen
                await ReminderContract24Hours();
            }
            catch (Exception ex)
            {
                // Fout in de herinneringen service
                Console.WriteLine($"Error in Reminders service: {ex.Message}");
            }

            // Wacht 1 minuut voordat de taak opnieuw wordt uitgevoerd
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
