using System.Dynamic;
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
    public async Task AddIntakeAsyncWithoutDamage_ReturnsOk_WhenAddIntakeIsSuccessful()
    {

        //Arrange
        var request = new AddIntakeRequest
        {
            Damage = null,
            FrameNrVehicle = 99,
            ReviewedBy = "caa-1",
            Date = DateTime.Now,
            Contract = 100,
            IsDamaged = false
        };

        _mockEmployeeRepository
            .Setup(repo => repo.AddIntakeAsync(It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .ReturnsAsync((true, "Intake added successfully"));
        
        _mockVehicleRepository
            .Setup(repo => repo.ChangeRepairStatus(It.IsAny<int>(), It.IsAny<bool>()))
            .Verifiable();
        
        //Act
        var result = await _controller.AddIntakeAsync(request);
        
        //Assert
        _mockVehicleRepository.Verify(repo => repo.ChangeRepairStatus(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AddIntakeResponse>(okResult.Value);

        Assert.Equal("Intake added successfully", response.Message);
    }

    [Fact]
    public async Task AddIntakeAsync_ReturnsBadRequest_WhenAddIntakeFails()
    {
        //Arrange
        var request = new AddIntakeRequest
        {
            Damage = null,
            FrameNrVehicle = 99,
            ReviewedBy = "caa-1",
            Date = DateTime.Now,
            Contract = 100,
            IsDamaged = false
        };

        _mockEmployeeRepository
            .Setup(repo => repo.AddIntakeAsync(It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .ReturnsAsync((false, "Failure"));
        
        //Act
        var result = await _controller.AddIntakeAsync(request);
        
        //Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<AddIntakeErrorResponse>(badRequestResult.Value);

        Assert.False(response.Status);
        Assert.Equal("Failure", response.Message);
    }
    
    [Fact]
    public async Task AddIntakeAsync_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var request = new AddIntakeRequest
        {
            Damage = "Whole car is flat",
            FrameNrVehicle = 12345,
            ReviewedBy = "caa-1",
            Date = DateTime.Now,
            Contract = 69420,
            IsDamaged = false
        };
        
        _mockEmployeeRepository
            .Setup(repo => repo.AddIntakeAsync(It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<int>(), 
                It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.AddIntakeAsync(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        
        var response = Assert.IsType<AddIntakeErrorResponse>(statusCodeResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Database connection failed", response.Message);
    }



    
    [Fact]
    public async Task AddIntakeAsync_UpdatesRepairStatus_WhenDamageIsReported()
    {
        //Arrange
        var request = new AddIntakeRequest
        {
            Damage = "Half the car is gone",
            FrameNrVehicle = 108,
            ReviewedBy = "caa-1",
            Date = DateTime.Now,
            Contract = 107,
            IsDamaged = true
        };

        _mockEmployeeRepository
            .Setup(repo => repo.AddIntakeAsync(It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .ReturnsAsync((true, "Success"));
        
        _mockVehicleRepository
            .Setup(repo => repo.ChangeRepairStatus(It.IsAny<int>(), It.IsAny<bool>()))
            .Verifiable();
        
        //Act
        var result = await _controller.AddIntakeAsync(request);
        
        //Assert
        _mockVehicleRepository.Verify(repo => repo.ChangeRepairStatus(request.FrameNrVehicle, true), Times.Once);
        
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AddIntakeResponse>(okResult.Value);
        
        Assert.Equal("Success", response.Message);
    }
}