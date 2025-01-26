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

public class Reminders : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConnector _connector;

    public Reminders(IServiceProvider serviceProvider, IConnector connector)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

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

    private async Task<Dictionary<string, object>> GetCustomerInfo (int id, IServiceScope scope)
    {
        var customer = scope.ServiceProvider.GetRequiredService<ICustomerDetails>();

        await customer.SetDetailsAsync(id);
        
        return await customer.GetDetailsAsync();
    }

    private async Task<Dictionary<string, object>> GetContractInfo (int id, IServiceScope scope)
    {
        var contract = scope.ServiceProvider.GetRequiredService<IContractDetails>();

        await contract.SetDetailsAsync(id);
        
        return await contract.GetDetailsAsync();
    }

    private async Task<Dictionary<string, object>> GetVehicleInfo (int id, IServiceScope scope)
    {
        var vehicle = scope.ServiceProvider.GetRequiredService<IVehicleDetails>();

        await vehicle.SetDetailsAsync(id);
        
        return await vehicle.GetDetailsAsync();
    }

    private async Task SendEmail(string body, string subject, string toEmail, object orderId, IServiceScope scope)
    {
        try
        {
            var emailService = scope.ServiceProvider.GetService<IEmailService>();

            if (emailService == null)
            {
                throw new InvalidOperationException("IEmailService not found in service provider.");
            }

            string query = $"UPDATE Contract SET SendEmail = 'Yes' WHERE OrderId = {orderId}";

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
                    await emailService.Send(toEmail, subject, body);
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

    public virtual async Task<bool> ReminderContract24Hours()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var customer = scope.ServiceProvider.GetRequiredService<ICustomerDetails>();
            var contract = scope.ServiceProvider.GetRequiredService<IContractDetails>();
            var vehicle = scope.ServiceProvider.GetRequiredService<IVehicleDetails>();
            var contracts = scope.ServiceProvider.GetRequiredService<IContractRepository>();

            if (customer == null || contract == null || vehicle == null || contracts == null)
            {
                Console.WriteLine("One or more required services are not available.");
                return false; // Exit early if any service is not resolved
            }

            IList<int> ids = await contracts.GetContractsSendEmailAsync();

            if (ids == null || ids.Count == 0)
            {
                return false;
            }

            foreach (int orderId in ids)
            {
                Dictionary<string, object> contractDic = new Dictionary<string, object>();
                Dictionary<string, object> customerDic = new Dictionary<string, object>();
                Dictionary<string, object> vehicleDic = new Dictionary<string, object>();

                contractDic = await GetContractInfo(orderId, scope);
                customerDic = await GetCustomerInfo(Convert.ToInt32(contractDic["Customer"]), scope);
                vehicleDic = await GetVehicleInfo(Convert.ToInt32(contractDic["FrameNrVehicle"]), scope);

                string email = CreateReminderContract(customerDic, contractDic, vehicleDic);

                await SendEmail(email, "Herinnering Huren Voertuig", customerDic["Email"].ToString(), contractDic["OrderId"], scope);
            }
            
            return true;
        }
    }

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
