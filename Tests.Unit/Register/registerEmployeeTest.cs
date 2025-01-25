using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WPR.Controllers.General.SignUp;
using WPR.Database;
using WPR.Repository;
using WPR.Utils;

namespace Tests.Unit.Register;

public class RegisterTest
{

    [Fact]
    public async Task CreateBusinessAccount_WithValidDomain()
    {
        // Arrange
        var mockConnector = Mock.Of<IConnector>();
        var mockDomainEmailChecker = new Mock<DomainEmailChecker>(mockConnector);

        mockDomainEmailChecker
            .Setup(checker => checker.DomainExists(It.Is<string>(email => email == "user@testdomain.com")))
            .ReturnsAsync((true, 12345678));

        var mockSignUpRequestCustomer = new SignUpRequestCustomer
        {
            Email = "user@testdomain.com",
            AccountType = "Business",
            KvK = 0,
            Password = "Test123!?",
            IsPrivate = false
        };

        var mockCombinedSignupRequest = new CombinedSignUpRequest
        {
            SignUpRequestCustomer = mockSignUpRequestCustomer,
            SignUpRequestCustomerPrivate = null
        };

        var mockUserRepository = new Mock<IUserRepository>();


        mockUserRepository
            .Setup(repo => repo.AddCustomer(It.IsAny<SignUpRequestCustomer>(), It.IsAny<SignUpRequestCustomerPrivate>()))
            .Callback<SignUpRequestCustomer, SignUpRequestCustomerPrivate>((customer, privateCustomer) =>
            {
                if (customer.AccountType == "Business" && customer.Email.Contains("@testdomain.com"))
                {
                    var (domainExists, kvk) = mockDomainEmailChecker.Object.DomainExists(customer.Email).Result;
                    if (domainExists)
                    {
                        customer.KvK = kvk;
                    }
                }
            })
            .ReturnsAsync((200, "Customer Account Added Successfully"));

        var controller = new SignUpController(mockUserRepository.Object);

        // Act
        var result = await controller.SignUp(mockCombinedSignupRequest);

        // Assert
        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, response.StatusCode);

        var responseValue = response.Value;
        Assert.NotNull(responseValue);

        var messageProperty = responseValue.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);

        var messageValue = messageProperty.GetValue(responseValue)?.ToString();
        Assert.Equal("Customer Account Added Successfully", messageValue);
    }

    [Fact]
    public async Task CreateBusinessAccount_WithInvalidDomain()
    {
        // Arrange
        var mockConnector = Mock.Of<IConnector>();
        var mockDomainEmailChecker = new Mock<DomainEmailChecker>(mockConnector); 

        mockDomainEmailChecker
            .Setup(checker => checker.DomainExists(It.Is<string>(email => email == "user@invalid.com")))
            .ReturnsAsync((false, 0)); 

        var mockSignUpRequestCustomerInvalid = new SignUpRequestCustomer
        {
            Email = "user@invalid.com",
            AccountType = "Business",
            KvK = 0,
            Password = "Test123!?",
            IsPrivate = false
        };

        var mockCombinedSignupRequestInvalid = new CombinedSignUpRequest
        {
            SignUpRequestCustomer = mockSignUpRequestCustomerInvalid,
            SignUpRequestCustomerPrivate = null
        };

        var mockUserRepository = new Mock<IUserRepository>();

        mockUserRepository
            .Setup(repo => repo.AddCustomer(It.IsAny<SignUpRequestCustomer>(), It.IsAny<SignUpRequestCustomerPrivate>()))
            .Callback<SignUpRequestCustomer, SignUpRequestCustomerPrivate>((customer, privateCustomer) =>
            {
                if (customer.AccountType == "Business" && customer.Email.Contains("@invalid.com"))
                {
                    var (domainExists, _) = mockDomainEmailChecker.Object.DomainExists(customer.Email).Result;
                    if (!domainExists)
                    {
                        customer.KvK = 0;
                    }
                }
            })
            .ReturnsAsync((400, "Domain does not exist"));

        var controller = new SignUpController(mockUserRepository.Object);

        // Act
        var result = await controller.SignUp(mockCombinedSignupRequestInvalid);

        // Assert
        var response = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, response.StatusCode);

        var responseValue = response.Value;
        Assert.NotNull(responseValue);

        var messageProperty = responseValue.GetType().GetProperty("message");
        Assert.NotNull(messageProperty);

        var messageValue = messageProperty.GetValue(responseValue)?.ToString();
        Assert.Equal("Domain does not exist", messageValue);

    }

}