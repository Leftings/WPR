using Microsoft.AspNetCore.Mvc;
using Moq;
using WPR.Controllers.Employee.Shared.viewRentalData;
using WPR.Repository;

namespace Tests.Unit.ViewRentalData;

public class ViewRentalDataControllerTests
{
    private readonly Mock<IBackOfficeRepository> _mockBackOfficeRepository;
    private readonly viewRentalDataController _viewRentalDataController;

    public ViewRentalDataControllerTests()
    {
        _mockBackOfficeRepository = new Mock<IBackOfficeRepository>();
        _viewRentalDataController = new viewRentalDataController(_mockBackOfficeRepository.Object);
    }
[Fact]
    public async Task GetFullReview_ReturnsOk_WhenDataIsFound()
    {
        // Arrange
        var reviewId = 1;
        var data = new Dictionary<string, object>
        {
            { "OrderId", reviewId },
            { "CustomerName", "John Doe" },
            { "Vehicle", "Toyota Camry" }
        };

        _mockBackOfficeRepository
            .Setup(repo => repo.GetFullDataReview(reviewId))
            .Returns((true, "Success", data));

        // Act
        var result = await _viewRentalDataController.GetFullReview(reviewId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);

        var response = okResult.Value as dynamic;
        Assert.NotNull(response);
        Assert.Equal(data, response.Message);
    }
    
    [Fact]
    public async Task GetFullReview_ReturnsBadRequest_WhenDataNotFound()
    {
        // Arrange
        var reviewId = 1;
        var errorMessage = "No data found";

        _mockBackOfficeRepository
            .Setup(repo => repo.GetFullDataReview(reviewId))
            .Returns((false, errorMessage, new Dictionary<string, object>()));

        // Act
        var result = await _viewRentalDataController.GetFullReview(reviewId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);

        var response = badRequestResult.Value as dynamic;
        Assert.NotNull(response);
        Assert.Equal(errorMessage, response.Message);
    }


    [Fact]
    public async Task GetFullReview_ReturnsBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var reviewId = 1;
        var exceptionMessage = "Database connection failed";

        _mockBackOfficeRepository
            .Setup(repo => repo.GetFullDataReview(reviewId))
            .Throws(new Exception(exceptionMessage));

        // Act
        var result = await _viewRentalDataController.GetFullReview(reviewId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);

        var response = badRequestResult.Value as dynamic;
        Assert.NotNull(response);
        Assert.Equal(exceptionMessage, response.Message);
    }
}
