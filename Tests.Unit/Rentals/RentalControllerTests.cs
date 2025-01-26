using Moq;
using WPR.Controllers.Customer.Rental;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WPR.Repository;

namespace Tests.Unit.Rentals;

public class RentalControllerTests
{
    private readonly Mock<IVehicleRepository> _mockVehicleRepo;
    private readonly RentalController _controller;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpRequest> _mockRequest;
    
    public RentalControllerTests()
    {
        
        _mockVehicleRepo = new Mock<IVehicleRepository>();
        _controller = new RentalController(_mockVehicleRepo.Object);
        
        _mockHttpContext = new Mock<HttpContext>();
        _mockRequest = new Mock<HttpRequest>();

        _mockRequest.Setup(r => r.Cookies["LoginSession"]).Returns("validLoginCookie");
        _mockHttpContext.Setup(c => c.Request).Returns(_mockRequest.Object);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }
    
    [Fact]
    public async Task CancelRentalAsync_ReturnsCorrectResponse_WhenRentalIsCanceled()
    {
        // Arrange
        int rentalId = 123;
        string loginCookie = "validLoginCookie";

        _mockVehicleRepo
            .Setup(repo => repo.CancelRental(rentalId, loginCookie))
            .ReturnsAsync((true, 200, "Rental canceled successfully"));

        // Act
        var result = await _controller.CancelRentalAsync(rentalId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<RentalResponse>(statusCodeResult.Value);
        Assert.Equal("Rental canceled successfully", response.Message);
        Assert.Equal(200, statusCodeResult.StatusCode);
    }
    
    [Fact]
    public async Task GetAllUserRentalsAsync_ReturnsRentals_WhenUserHasRentals()
    {
        // Arrange
        string loginCookie = "validLoginCookie";
        var rentals = new List<object> { new { Id = 1, Vehicle = "Car", StartDate = DateTime.Now } };
        _mockVehicleRepo
            .Setup(repo => repo.GetAllUserRentals(loginCookie))
            .ReturnsAsync((true, 200, "Rentals fetched successfully", rentals));

        // Act
        var result = await _controller.GetAllUserRentalsAsync();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        var responseRentals = Assert.IsType<List<object>>(statusCodeResult.Value);
        Assert.NotEmpty(responseRentals);
        Assert.Equal(200, statusCodeResult.StatusCode);
    }
    
    [Fact]
    public async Task GetAllUserRentalsAsync_ReturnsError_WhenUserHasNoRentals()
    {
        // Arrange
        string loginCookie = "validLoginCookie";
        _mockVehicleRepo
            .Setup(repo => repo.GetAllUserRentals(loginCookie))
            .ReturnsAsync((false, 404, "No rentals found", new List<object>()));

        // Act
        var result = await _controller.GetAllUserRentalsAsync();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        var response = Assert.IsType<RentalResponse>(statusCodeResult.Value);
        Assert.Equal("No rentals found", response.Message);
        Assert.Equal(404, statusCodeResult.StatusCode);
    }
    
    [Fact]
    public async Task CreateRental_ReturnsSuccess_WhenRentalIsCreated()
    {
        // Arrange
        var rentalRequest = new RentalRequest
        {
            Email = "saadzk@outlook.com",
            FrameNrVehicle = "ABC123",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(5),
            Price = 200
        };
        _mockVehicleRepo
            .Setup(repo => repo.HireVehicle(rentalRequest, "validLoginCookie"))
            .ReturnsAsync((true, "Rental created successfully"));

        // Act
        var result = await _controller.CreateRental(rentalRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RentalResponse>(okResult.Value);
        Assert.Equal("Rental created successfully", response.Message);
        Assert.Equal(200, okResult.StatusCode);
    }
    
    [Fact]
    public async Task CreateRental_ReturnsError_WhenRentalCreationFails()
    {
        // Arrange
        var rentalRequest = new RentalRequest
        {
            Email = "customer@example.com",
            FrameNrVehicle = "ABC123",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(5),
            Price = 200
        };
        _mockVehicleRepo
            .Setup(repo => repo.HireVehicle(rentalRequest, "validLoginCookie"))
            .ReturnsAsync((false, "Failed to create rental"));

        // Act
        var result = await _controller.CreateRental(rentalRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<RentalResponse>(badRequestResult.Value);
        Assert.Equal("Failed to create rental", response.Message);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}