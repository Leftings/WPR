using Microsoft.AspNetCore.Http;
using Test.Unit.Mocks;
using WPR.Controllers.Employee.FrontOffice.AddIntake;

namespace Tests.Unit.AddVehicle;

using Moq;
using Xunit;
using WPR.Repository;
using WPR.Controllers.Employee.BackOffice.AddVehicle;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WPR.Database;

public class AddVehicleControllerTests
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly AddVehicleController _controller;

    public AddVehicleControllerTests()
    {
        EnvConfigMock envConfigMock = new EnvConfigMock();
        Connector connector = new Connector(envConfigMock);
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();

        _controller = new AddVehicleController(connector, _mockEmployeeRepository.Object);
    }

    [Fact]
    public async Task AddVehicleAsync_ReturnsOk_WhenAddvehicleSuccessful()
    {

        //Arrange
        var addVehicleRequest = new AddVehicleRequest
        {
            YoP = 2023,
            Brand = "Mercedes Benz",
            Type = "A-Class Sedan",
            LicensePlate = "XY-345-ZA",
            Color = "Red",
            Sort = "Car",
            Price = 50,
            Description = "Snelle auto",
            Places = 5
        };
        
        _mockEmployeeRepository.Setup(repo => repo.AddVehicleAsync(
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<double>(),
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<int>()
        )).ReturnsAsync((true, "Vehicle added successfully"));

        var vehicleImage = new FormFile(new MemoryStream(new byte[] { 1, 2, 3, 4 }), 0, 4, "vehicleBlob", "image.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        
        //Act
        var result = await _controller.AddVehicleAsync(addVehicleRequest, vehicleImage);
        
        //Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AddVehicleResponse>(okResult.Value);

        Assert.Equal("Vehicle added successfully", response.Message);
    }

}