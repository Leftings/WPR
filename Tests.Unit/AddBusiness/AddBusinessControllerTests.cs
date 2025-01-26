using Moq;
using Xunit;
using WPR.Repository;
using System.Threading.Tasks;
using WPR.Services;
using Microsoft.AspNetCore.Mvc;
using WPR.Database;
using Microsoft.AspNetCore.Http;
using Test.Unit.Mocks;
using WPR.Controllers.Customer.AddBusiness;

namespace Tests.Unit.AddBusiness;

public class AddBusinessControllerTests
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepo;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly AddBusinessController _controller;

    public AddBusinessControllerTests()
    {
        _mockEmployeeRepo = new Mock<IEmployeeRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _controller = new AddBusinessController(_mockEmployeeRepo.Object, _mockEmailService.Object);
    }

    [Fact]
    public async Task AddBusiness_ReturnsOk_WhenAddVehicleSuccessful()
    {
        //Arrange
        var addBusinessRequest = new AddBusinessRequest()
        {
            Subscription = "1",
            KvK = 78978978,
            Name = "Samsung Corp.",
            Adress = "Samsungstraat 1",
            Domain = "@samsung.com",
            ContactEmail = "stevejobs@samsung.com"
        };

        var mockResult = (true, "Succesfull added business");
        _mockEmployeeRepo.Setup(repo => repo.AddBusiness((It.IsAny<AddBusinessRequest>())))
            .ReturnsAsync(mockResult);

        //Act
        var result = await _controller.AddBusiness(addBusinessRequest);

        //Asssert
        var actionResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<addBusinessResponse>(actionResult.Value);
        Assert.Equal("Succesfull added business", returnValue.Message);
    }
    
    [Fact]
    public async Task AddBusiness_InvalidRepositoryResponse_ReturnBadReturns()
    {
        //Arrange
        var addBusinessRequest = new AddBusinessRequest()
        {
            Subscription = "1",
            KvK = 78978978,
            Name = "Samsung Corp.",
            Adress = "Samsungstraat 1",
            Domain = "@samsung.com",
            ContactEmail = "stevejobs@samsung.com"
        };

        var mockResult = (false, "Error occured adding business");
        _mockEmployeeRepo.Setup(repo => repo.AddBusiness((It.IsAny<AddBusinessRequest>())))
            .ReturnsAsync(mockResult);

        //Act
        var result = await _controller.AddBusiness(addBusinessRequest);

        //Assert
        var actionResult = Assert.IsType<BadRequestObjectResult>(result);
        var returnValue = Assert.IsType<addBusinessResponse>(actionResult.Value);
        Assert.Equal("Error occured adding business", returnValue.Message);
    }
}