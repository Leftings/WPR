using Moq;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using WPR.Email;
using WPR.Repository;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WPR.Database;

public class RemindersTests
{
    [Fact]
    public async Task CallReminderEmailTest()
    {
        // Arrange
        var mockContractRepository = new Mock<IContractRepository>();
        var mockUserRepository = new Mock<IUserRepository>();
        var mockConnector = new Mock<IConnector>();

        var mockCustomer = new Customer(mockUserRepository.Object);
        var mockContract = new Contract(mockContractRepository.Object);

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(Customer)))
            .Returns(mockCustomer);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(Contract)))
            .Returns(mockContract);
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IContractRepository)))
            .Returns(mockContractRepository.Object);

        var reminders = new Reminders(mockServiceProvider.Object, mockConnector.Object);

        var mockReminders = new Mock<Reminders>(mockServiceProvider.Object, mockConnector.Object)
        {
            CallBase = true
        };
        mockReminders.Setup(service => service.ReminderContract24Hours())
                     .ReturnsAsync(true)
                     .Verifiable();

        // Act
        var hostedService = mockReminders.Object as IHostedService;

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var task = hostedService.StartAsync(cancellationToken);

        await Task.Delay(10000);

        // Assert
        mockReminders.Verify(service => service.ReminderContract24Hours(), Times.Once);

        cts.Cancel();
        await task;

        Assert.True(cancellationToken.IsCancellationRequested);
    }
}
