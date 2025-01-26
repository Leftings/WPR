using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Test.Unit.Mocks;
using WPR.Controllers.General.Vehicle;
using WPR.Database;
using WPR.Repository;
using Xunit;

public class VehicleControllerTests
{
    private readonly Mock<IVehicleRepository> _mockVehicleRepository;
    private readonly VehicleController _vehicleController;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpRequest> _mockHttpRequest;

    public VehicleControllerTests()
    {
        var envConfigMock = new EnvConfigMock();

        var connector = new Connector(envConfigMock);
        
        
        _mockVehicleRepository = new Mock<IVehicleRepository>();
        _vehicleController = new VehicleController(connector, _mockVehicleRepository.Object);

        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpRequest = new Mock<HttpRequest>();

        _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
        _vehicleController.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }

    [Fact]
    public async Task GetVehicleData_ReturnsSuccess_WhenDataIsFound()
    {
        // Arrange
        var frameNr = "12345";
        var vehicleData = new List<Dictionary<object, string>>
        {
            new Dictionary<object, string> { { "FrameNR", "12345" }, { "Make", "Toyota" }, { "Model", "Camry" } }
        };

        _mockVehicleRepository
            .Setup(repo => repo.GetVehicleDataAsync(frameNr))
            .ReturnsAsync(vehicleData);

        // Act
        var result = await _vehicleController.GetVehicleData(frameNr);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultValue = okResult.Value;
        var resultType = resultValue.GetType();
        Assert.Equal(200, okResult.StatusCode);
    }


    [Fact]
    public async Task GetVehicleData_ReturnsNotFound_WhenDataIsEmpty()
    {
        // Arrange
        var frameNr = "12345";
        _mockVehicleRepository
            .Setup(repo => repo.GetVehicleDataAsync(frameNr))
            .ReturnsAsync(new List<Dictionary<object, string>>());

        // Act
        var result = await _vehicleController.GetVehicleData(frameNr);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);

        var response = Assert.IsType<VehicleErrorResponse>(notFoundResult.Value); // Assert that Value is a VehicleErrorResponse
        Assert.False(response.Status); // Assert the Status property of the VehicleErrorResponse
        Assert.Equal("No vehicle data found", response.Message); // Assert the Message property of the VehicleErrorResponse
    }


    [Fact]
    public async Task GetVehicleData_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var frameNr = "12345";
        _mockVehicleRepository
            .Setup(repo => repo.GetVehicleDataAsync(frameNr))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _vehicleController.GetVehicleData(frameNr);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        var response = Assert.IsType<VehicleErrorResponse>(statusCodeResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Database connection failed", response.Message);
    }

}