using Test.Unit.Mocks;

namespace Tests.Unit.AddIntakeControllerTests;

using Moq;
using Xunit;
using WPR.Repository;
using WPR.Controllers.Employee.FrontOffice.AddIntake;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WPR.Database;

public class AddIntakeControllerTests
{

    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IVehicleRepository> _mockVehicleRepository;
    private readonly AddIntakeController _controller;

    public AddIntakeControllerTests()
    {

        var envConfigMock = new EnvConfigMock();

        var connector = new Connector(envConfigMock);
        
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockVehicleRepository = new Mock<IVehicleRepository>();

        _controller = new AddIntakeController(connector, _mockEmployeeRepository.Object, _mockVehicleRepository.Object);
    }

    [Fact]
    public async Task AddIntakeAsync_ReturnsOk_WhenAddIntakeIsSuccessful()
    {

        //Arrange
        var request = new AddIntakeRequest
        {
            Damage = "No damage present.",
            FrameNrVehicle = 99,
            ReviewedBy = "caa-1",
            Date = DateTime.Now,
            Contract = 100
        };

        _mockEmployeeRepository
            .Setup(repo => repo.AddIntakeAsync(It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<int>()))
            .ReturnsAsync((true, "Success"));
        
        //Act
        var result = await _controller.AddIntakeAsync(request);
        
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AddIntakeResponse>(okResult.Value);

        Assert.Equal("Success", response.Message);
    }
}