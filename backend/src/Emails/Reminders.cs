using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Writers;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities;
using WPR.Database;
using WPR.Repository;
using WPR.Services;

namespace WPR.Email;

/// <summary>
/// De klasse Reminders draaid op de achtergrond om elke 24 uur herrinering mails van contracten te sturen naar de benodigde gebruikers.
/// </summary>

public class Reminders : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnector _connector;

    /// <summary>
    /// Constructor die de benodigde classes initialiseerd
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="connector"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Reminders(IServiceProvider serviceProvider, IConnector connector)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    /// <summary>
    /// CreateReminderContract creeërt een standaard template voor het laten herinneren van gebruikers dat hun zij hun voertuig binnen 24 uur kunnen ophalen
    /// </summary>
    /// <param name="Customer"></param>
    /// <param name="Contract"></param>
    /// <param name="Vehicle"></param>
    /// <returns></returns>
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
    
    /// <summary>
    /// Alle gegevens van de klant worden verzameld
    /// </summary>
    /// <param name="id"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    private async Task<Dictionary<string, object>> GetCustomerInfo (int id, IServiceScope scope)
    {
        var customer = scope.ServiceProvider.GetRequiredService<ICustomerDetails>();

        await customer.SetDetailsAsync(id);
        
        return await customer.GetDetailsAsync();
    }

    /// <summary>
    /// Alle gegevens van het contract worden verzameld
    /// </summary>
    /// <param name="id"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    private async Task<Dictionary<string, object>> GetContractInfo (int id, IServiceScope scope)
    {
        var contract = scope.ServiceProvider.GetRequiredService<IContractDetails>();

        await contract.SetDetailsAsync(id);
        
        return await contract.GetDetailsAsync();
    }

    /// <summary>
    /// Alle gegegevens van het voertuig worden verzameld
    /// </summary>
    /// <param name="id"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    private async Task<Dictionary<string, object>> GetVehicleInfo (int id, IServiceScope scope)
    {
        var vehicle = scope.ServiceProvider.GetRequiredService<IVehicleDetails>();

        await vehicle.SetDetailsAsync(id);
        
        return await vehicle.GetDetailsAsync();
    }

    /// <summary>
    /// SendEmail zorgt voor de voorbereidingen van het maken van de email.
    /// Terwijl de email klaar wordt gemaakt om te verzenden wordt in de database geregistreerd dat er voor het huidige contract een herinnerings maal verzonden is.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="subject"></param>
    /// <param name="toEmail"></param>
    /// <param name="orderId"></param>
    /// <param name="scope"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task SendEmail(string body, string subject, string toEmail, object orderId, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetService<IEmailService>(); // Emailservices wordt beschikbaargesteld

            if (emailService == null)
            {
                throw new InvalidOperationException("IEmailService not found in service provider.");
            }

            string query = $"UPDATE Contract SET SendEmail = 'Yes' WHERE OrderId = {orderId}";

            // Er wordt vastgesteld in de database dat er een email verstuurd is
            using (var connection = _connector.CreateDbConnection())
            using (var command = connection.CreateCommand())
            {
                if (command == null)
                {
                    throw new InvalidOperationException("Failed to create a database command.");
                }
                
                command.CommandText = query;

                if (command.ExecuteNonQuery() > 0)
                {
                    await emailService.Send(toEmail, subject, body); // Email aanvraag wordt verstuurd
                }
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Alle gegevens worden opgehaald en verzameld om verstuurd te worden.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<bool> ReminderContract24Hours()
    {
        using (var scope = _serviceProvider.CreateScope()) // Er wordt een scope opgezet, zodat alle gegevens bereikbaar zijn
        {
            var customer = scope.ServiceProvider.GetRequiredService<ICustomerDetails>();
            var contract = scope.ServiceProvider.GetRequiredService<IContractDetails>();
            var vehicle = scope.ServiceProvider.GetRequiredService<IVehicleDetails>();
            var contracts = scope.ServiceProvider.GetRequiredService<IContractRepository>();

            if (customer == null || contract == null || vehicle == null || contracts == null)
            {
                Console.WriteLine("One or more required services are not available.");
                return false; 
            }

            IList<int> ids = await contracts.GetContractsSendEmailAsync();

            if (ids == null || ids.Count == 0)
            {
                return false;
            }

            foreach (int orderId in ids)
            {
                // Alle gegevens worden verzameld in dictionaries
                Dictionary<string, object> contractDic = new Dictionary<string, object>();
                Dictionary<string, object> customerDic = new Dictionary<string, object>();
                Dictionary<string, object> vehicleDic = new Dictionary<string, object>();

                // Alle gegevens worden in de dictionaries gezet
                contractDic = await GetContractInfo(orderId, scope);
                customerDic = await GetCustomerInfo(Convert.ToInt32(contractDic["Customer"]), scope);
                vehicleDic = await GetVehicleInfo(Convert.ToInt32(contractDic["FrameNrVehicle"]), scope);

                // Email wordt aangemaakt
                string email = CreateReminderContract(customerDic, contractDic, vehicleDic);

                // Email wordt naar de mailservice gestuurd
                await SendEmail(email, "Herinnering Huren Voertuig", customerDic["Email"].ToString(), contractDic["OrderId"], scope);
            }
            
            return true;
        }
    }

    /// <summary>
    /// ExecuteAsync zorgt ervoor dat er de herinneringsmails elke x tijd verstuurd worden.
    /// Voor de realiteit is het per 24 uur.
    /// Voor het presenteren is het lke 10 seconden.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReminderContract24Hours();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Reminders service: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
