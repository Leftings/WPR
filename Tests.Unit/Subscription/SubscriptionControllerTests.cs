using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WPR.Controllers.customer.Subscription;
using WPR.Repository;
namespace Tests.Unit.Subscription;
public class SubscriptionControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IBackOfficeRepository> _mockBackOfficeRepository;
    private readonly SubscriptionController _subscriptionController;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpRequest> _mockHttpRequest;
    public SubscriptionControllerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockBackOfficeRepository = new Mock<IBackOfficeRepository>();
        _subscriptionController = new SubscriptionController(_mockUserRepository.Object, _mockBackOfficeRepository.Object);
        
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpRequest = new Mock<HttpRequest>();
        
        _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
        _subscriptionController.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }
    [Fact]
    public async Task AddSubscription_ReturnsSuccess_WhenSubscriptionIsCreated()
    {
        // Arrange
        var request = new SubscriptionRequest
        {
            Type = "MockAbbonement",
            Description = "MockDescription",
            Discount = 0.5,
            Price = 20.00,
        };
        _mockBackOfficeRepository
            .Setup(repo => repo.AddSubscriptionAsync(request.Type, request.Description, request.Discount, request.Price))
            .ReturnsAsync((true, "Subscription added"));
        // Act
        var result = await _subscriptionController.AddSubscription(request);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<SubscriptionResponse>(okResult.Value);
        Assert.Equal("Subscription added", response.Message);
        Assert.Equal(200, okResult.StatusCode);
    }
    
    [Fact]
    public async Task AddSubscription_ReturnsBadRequest_WhenSubscriptionFails()
    {
        // Arrange
        var request = new SubscriptionRequest
        {
            Type = "MockAbbonement",
            Description = "MockDescription",
            Discount = 0.5,
            Price = 20.00,
        };
        _mockBackOfficeRepository
            .Setup(repo => repo.AddSubscriptionAsync(request.Type, request.Description, request.Discount, request.Price))
            .ReturnsAsync((false, "Error during adding of subscription"));
        // Act
        var result = await _subscriptionController.AddSubscription(request);
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<SubscriptionErrorResponse>(badRequestResult.Value);
        Assert.Equal("Error during adding of subscription", response.Message);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
    
    [Fact]
    public async Task AddSubscription_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var request = new SubscriptionRequest
        {
            Type = "MockAbbonement",
            Description = "MockDescription",
            Discount = 0.5,
            Price = 20.00,
        };
        _mockBackOfficeRepository
            .Setup(repo => repo.AddSubscriptionAsync(request.Type, request.Description, request.Discount, request.Price))
            .ThrowsAsync(new Exception("Database connection failed"));
        // Act
        var result = await _subscriptionController.AddSubscription(request);
        // Assert
        
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
            
        var response = Assert.IsType<SubscriptionErrorResponse>(statusCodeResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Database connection failed", response.Message);
    }
    
    
    [Fact]
    public async Task DeleteSubscription_ReturnsSuccess_WhenDeletionIsSuccessful()
    {
        // Arrange
        var subscriptionId = 1;
        _mockBackOfficeRepository
            .Setup(repo => repo.DeleteSubscriptionAsync(subscriptionId))
            .ReturnsAsync((true, "Subscription deleted successfully"));
        // Act
        var result = await _subscriptionController.DeleteSubscriptionAsync(subscriptionId);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<SubscriptionResponse>(okResult.Value);
        Assert.Equal("Subscription deleted successfully", response.Message);
        Assert.Equal(200, okResult.StatusCode);
    }
    
    [Fact]
    public async Task DeleteSubscription_ReturnsBadRequest_WhenDeletionFails()
    {
        // Arrange
        var subscriptionId = 1;
        _mockBackOfficeRepository
            .Setup(repo => repo.DeleteSubscriptionAsync(subscriptionId))
            .ReturnsAsync((false, "Error deleting subscription"));
        // Act
        var result = await _subscriptionController.DeleteSubscriptionAsync(subscriptionId);
        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<SubscriptionErrorResponse>(badRequestResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Error deleting subscription", response.Message);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
    
    [Fact]
    public async Task DeleteSubscription_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var subscriptionId = 1;
        _mockBackOfficeRepository
            .Setup(repo => repo.DeleteSubscriptionAsync(subscriptionId))
            .ThrowsAsync(new Exception("Database connection failed"));
        // Act
        var result = await _subscriptionController.DeleteSubscriptionAsync(subscriptionId);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var response = Assert.IsType<SubscriptionErrorResponse>(statusCodeResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Database connection failed", response.Message);
    }
    
    [Fact]
    public async Task GetSubscriptionData_ReturnsSuccess_WhenDataIsFound()
    {
        // Arrange
        var subscriptionId = 1;
        var subscriptionData = new SubscriptionRequest
        {
            Id = subscriptionId,
            Type = "Premium",
            Description = "Premium Subscription",
            Price = 29.99
        };
        // Mock the repository method to return the subscription data
        _mockUserRepository
            .Setup(repo => repo.GetSubscriptionDataAsync(subscriptionId))
            .ReturnsAsync(subscriptionData);
        // Act
        var result = await _subscriptionController.GetSubscriptionData(subscriptionId);
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
    
        // Inspect the actual structure
        var resultValue = okResult.Value;
        var resultType = resultValue.GetType();
    
        // Debug: Print out properties
        foreach (var prop in resultType.GetProperties())
        {
            Console.WriteLine($"Property: {prop.Name}");
        }
        // Adjust assertion based on actual structure
        Assert.Equal(200, okResult.StatusCode);
    }
    [Fact]
    public async Task GetSubscriptionData_ReturnsNotFound_WhenDataIsNull()
    {
        // Arrange
        var subscriptionId = 1;
        _mockUserRepository
            .Setup(repo => repo.GetSubscriptionDataAsync(subscriptionId))!
            .ReturnsAsync((SubscriptionRequest)null!);
        // Act
        var result = await _subscriptionController.GetSubscriptionData(subscriptionId);
        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<SubscriptionErrorResponse>(notFoundResult.Value);
        Assert.False(response.Status);
        Assert.Equal("No subscription data found", response.Message);
        Assert.Equal(404, notFoundResult.StatusCode);
    }
    [Fact]
    public async Task GetSubscriptionData_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var subscriptionId = 1;
        _mockUserRepository
            .Setup(repo => repo.GetSubscriptionDataAsync(subscriptionId))
            .ThrowsAsync(new Exception("Database connection failed"));
        // Act
        var result = await _subscriptionController.GetSubscriptionData(subscriptionId);
        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        var response = Assert.IsType<SubscriptionErrorResponse>(statusCodeResult.Value);
        Assert.False(response.Status);
        Assert.Equal("Database connection failed", response.Message);
    }
}