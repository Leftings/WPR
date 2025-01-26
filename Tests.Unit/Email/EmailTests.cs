using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WPR.Database;
using WPR.Email;
using WPR.Repository;
using WPR.Services;
using Xunit;

namespace Tests.Unit.Email
{
    public class EmailTests
    {
        [Fact]
        public async Task SendBusinessReviewEmail_BusinessAccepted_EmailSentSuccessfully()
        {
            // Arrange
            var mockEmailService = new Mock<IEmailService>();
            var toEmail = "business@example.com";
            var businessName = "Test Business";
            var domain = "testbusiness.com";
            var password = "StrongPassword123!";
            var accepted = true;

            mockEmailService
                .Setup(service => service.SendBusinessReviewEmail(
                    toEmail,
                    businessName,
                    domain,
                    password,
                    accepted))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailService.Object.SendBusinessReviewEmail(toEmail, businessName, domain, password, accepted);

            // Assert
            mockEmailService.Verify(
                service => service.SendBusinessReviewEmail(
                    It.Is<string>(email => email == toEmail),
                    It.Is<string>(name => name == businessName),
                    It.Is<string>(dom => dom == domain),
                    It.Is<string>(pass => pass == password),
                    It.Is<bool>(status => status == accepted)),
                Times.Once);
        }

        [Fact]
        public async Task SendBusinessReviewEmail_BusinessDenied_EmailSentSuccessfully()
        {
            // Arrange
            var mockEmailService = new Mock<IEmailService>();
            var toEmail = "business@example.com";
            var businessName = "Test Business";
            var domain = "@testdomain"; // No domain for denied business
            var password = "testpass"; // No password for denied business
            var accepted = false;

            mockEmailService
                .Setup(service => service.SendBusinessReviewEmail(
                    toEmail,
                    businessName,
                    domain,
                    password,
                    accepted))
                .Returns(Task.CompletedTask);

            // Act
            await mockEmailService.Object.SendBusinessReviewEmail(toEmail, businessName, domain, password, accepted);

            // Assert
            mockEmailService.Verify(
                service => service.SendBusinessReviewEmail(
                    It.Is<string>(email => email == toEmail),
                    It.Is<string>(name => name == businessName),
                    It.Is<string>(dom => dom == domain),
                    It.Is<string>(pass => pass == password),
                    It.Is<bool>(status => status == accepted)),
                Times.Once);
        }

        private Reminders CreateReminders(Mock<IContractRepository> mockContractRepository,
            Mock<IEmailService> mockEmailService)
        {
            var services = new ServiceCollection();

            services.AddScoped<IEmailService>(provider => mockEmailService.Object);
            services.AddScoped<IContractRepository>(provider => mockContractRepository.Object);
            services.AddScoped<IContractDetails, Contract>();
            services.AddScoped<ICustomerDetails, Customer>();
            services.AddScoped<IVehicleDetails, Vehicle>();

            var mockUserRepository = new Mock<IUserRepository>();
            mockUserRepository.Setup(repo => repo.GetCustomerDetails(It.IsAny<int>()))
                .ReturnsAsync(new Dictionary<string, object> { { "Email", "test@domain.com" } });
            services.AddScoped<IUserRepository>(sp => mockUserRepository.Object);

            var mockConnector = new Mock<IConnector>();
            services.AddScoped<IConnector>(sp => mockConnector.Object);

            var mockVehicleRepository = new Mock<IVehicleRepository>();
            services.AddScoped<IVehicleRepository>(sp => mockVehicleRepository.Object);

            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();

            return new Reminders(scope.ServiceProvider, mockConnector.Object);
        }

        [Fact]
        public async Task Reminder24hoursContractTest()
        {
            // Arrange
            var contractIds = new List<int> { 1 };
            var mockContractRepository = new Mock<IContractRepository>();
            mockContractRepository.Setup(repo => repo.GetContractsSendEmailAsync())
                .ReturnsAsync(contractIds);

            var mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(es => es.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var reminders = CreateReminders(mockContractRepository, mockEmailService);

            var contractDetails = new Dictionary<string, object>
            {
                { "OrderId", 1 },
                { "Customer", 1 },
                { "StartDate", "2023-01-01" },
                { "EndDate", "2023-12-31" },
                { "FrameNrVehicle", 101 }
            };

            var customerDetails = new Dictionary<string, object>
            {
                { "Email", "test@domain.com" },
                { "ID", 1 }
            };

            var vehicleDetails = new Dictionary<string, object>
            {
                { "Brand", "Toyota" },
                { "Type", "Corolla" }
            };

            var mockContract = new Mock<IContractDetails>();
            mockContract.Setup(c => c.GetDetailsAsync()).ReturnsAsync(contractDetails);

            var mockCustomer = new Mock<ICustomerDetails>();
            mockCustomer.Setup(c => c.GetDetailsAsync()).ReturnsAsync(customerDetails);

            var mockVehicle = new Mock<IVehicleDetails>();
            mockVehicle.Setup(v => v.GetDetailsAsync()).ReturnsAsync(vehicleDetails);

            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();

            mockConnection.Setup(conn => conn.CreateCommand()).Returns(mockCommand.Object);
            mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);

            mockConnection.Setup(conn => conn.Open()).Callback(() => { });
            mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);
            var mockConnector = new Mock<IConnector>();
            mockConnector.Setup(connector => connector.CreateDbConnection()).Returns(mockConnection.Object);

            var services = new ServiceCollection();
            services.AddScoped<IContractDetails>(sp => mockContract.Object);
            services.AddScoped<ICustomerDetails>(sp => mockCustomer.Object);
            services.AddScoped<IVehicleDetails>(sp => mockVehicle.Object);
            services.AddScoped<IContractRepository>(sp => mockContractRepository.Object);
            services.AddScoped<IEmailService>(sp => mockEmailService.Object);
            services.AddScoped<IConnector>(sp => mockConnector.Object);

            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();

            reminders = new Reminders(scope.ServiceProvider, mockConnector.Object);

            // Act
            var result = await reminders.ReminderContract24Hours();

            // Assert
            Assert.True(result);
            mockEmailService.Verify(es => es.Send(
                "test@domain.com",
                "Herinnering Huren Voertuig",
                It.Is<string>(body => body.Contains(
                    @$"<h1>Herinnering voor voertuig </h1>Beste {customerDetails["Email"]},
        <p>Uw contract met referencie nummer {contractDetails["OrderId"]} gaat vanaf morgen in.</p>
        <p>Hier zijn de volgende details van uw lopende contract</p>
        <ul>
            <li>Referentie nummer: {contractDetails["OrderId"]}</li>
            <li>Start datum: {contractDetails["StartDate"]}</li>
            <li>Eind datum: {contractDetails["EndDate"]}</li>
            <li>Voertuig: {vehicleDetails["Brand"]} {vehicleDetails["Type"]}</li>
        </ul>
        "))
            ), Times.Once);
        }

        [Fact]
        public async Task Reminder24hoursContractTest_NoNewEmailsToSend()
        {
            // Arrange
            var contractIds = new List<int> { };
            var mockContractRepository = new Mock<IContractRepository>();
            mockContractRepository.Setup(repo => repo.GetContractsSendEmailAsync())
                .ReturnsAsync(contractIds);

            var mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(es => es.Send(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var reminders = CreateReminders(mockContractRepository, mockEmailService);

            var contractDetails = new Dictionary<string, object>
            {
            };

            var customerDetails = new Dictionary<string, object>
            {
            };

            var vehicleDetails = new Dictionary<string, object>
            {
            };

            var mockContract = new Mock<IContractDetails>();
            mockContract.Setup(c => c.GetDetailsAsync()).ReturnsAsync(contractDetails);

            var mockCustomer = new Mock<ICustomerDetails>();
            mockCustomer.Setup(c => c.GetDetailsAsync()).ReturnsAsync(customerDetails);

            var mockVehicle = new Mock<IVehicleDetails>();
            mockVehicle.Setup(v => v.GetDetailsAsync()).ReturnsAsync(vehicleDetails);

            var mockConnection = new Mock<IDbConnection>();
            var mockCommand = new Mock<IDbCommand>();

            mockConnection.Setup(conn => conn.CreateCommand()).Returns(mockCommand.Object);
            mockCommand.Setup(cmd => cmd.ExecuteNonQuery()).Returns(1);

            mockConnection.Setup(conn => conn.Open()).Callback(() => { });
            mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);
            var mockConnector = new Mock<IConnector>();
            mockConnector.Setup(connector => connector.CreateDbConnection()).Returns(mockConnection.Object);

            var services = new ServiceCollection();
            services.AddScoped<IContractDetails>(sp => mockContract.Object);
            services.AddScoped<ICustomerDetails>(sp => mockCustomer.Object);
            services.AddScoped<IVehicleDetails>(sp => mockVehicle.Object);
            services.AddScoped<IContractRepository>(sp => mockContractRepository.Object);
            services.AddScoped<IEmailService>(sp => mockEmailService.Object);
            services.AddScoped<IConnector>(sp => mockConnector.Object);

            var serviceProvider = services.BuildServiceProvider();
            var scope = serviceProvider.CreateScope();

            reminders = new Reminders(scope.ServiceProvider, mockConnector.Object);

            // Act
            var result = await reminders.ReminderContract24Hours();

            // Assert
            Assert.False(result);
        }
    }
}

