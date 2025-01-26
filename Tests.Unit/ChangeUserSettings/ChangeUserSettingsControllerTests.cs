using Test.Unit.Mocks;
using WPR.Controllers.Customer.ChangeUserSettings;
using WPR.Cryption;
using WPR.Database;
using WPR.Repository;
using WPR.Repository.DatabaseCheckRepository;

namespace Tests.Unit.ChangeUserSettings;

using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;


public class UserControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly ChangeUserSettingsController _userController;
    private readonly Mock<IDatabaseCheckRepository>  _mockDatabaseCheckRepository;

    public UserControllerTests()
    {
        var envConfigMock = new EnvConfigMock();

        var connector = new Connector(envConfigMock);
        var crypt = new Crypt(envConfigMock);
        
        
        _mockUserRepository = new Mock<IUserRepository>();
        _mockDatabaseCheckRepository = new Mock<IDatabaseCheckRepository>();
        _userController = new ChangeUserSettingsController(connector, _mockUserRepository.Object, crypt, _mockDatabaseCheckRepository.Object);
    }

    [Fact]
    public async Task ChangeUserInfoAsync_ReturnsOk_WhenDataIsUpdatedSuccessfully()
    {
        // Arrange
        var changeUserRequest = new ChangeUserRequest
        {
            Id = 9999,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "",
            TelNum = "",
            Adres = ""
        };

        _mockUserRepository
            .Setup(repo => repo.EditUserInfoAsync(It.IsAny<List<object[]>>()))
            .ReturnsAsync((true, "Data inserted"));

        // Act
        var result = await _userController.ChangeUserInfoAsync(changeUserRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        var response = okResult.Value as ChangeUserSettingsResponse;
        Assert.NotNull(response);
        Assert.Equal("Data Updated", response.Message);
    }

    [Fact]
    public async Task ChangeUserInfoAsync_ReturnsBadRequest_WhenBodyIsNull()
    {
        // Arrange
        ChangeUserRequest changeUserRequest = null;

        // Act
        var result = await _userController.ChangeUserInfoAsync(changeUserRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = badRequestResult.Value as string;
        Assert.Equal("Body can not be null", response);
    }

    [Fact]
    public async Task ChangeUserInfoAsync_ReturnsBadRequest_WhenUpdateFails()
    {
        // Arrange
        var changeUserRequest = new ChangeUserRequest
        {
            Id = 9999,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "",
            TelNum = "",
            Adres = ""
        };

        // Mock the repository to return a failure
        _mockUserRepository
            .Setup(repo => repo.EditUserInfoAsync(It.IsAny<List<object[]>>()))
            .ReturnsAsync((false, "Data not inserted"));

        // Act
        var result = await _userController.ChangeUserInfoAsync(changeUserRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);

        // Check the response content
        var response = badRequestResult.Value as ChangeUserSettingsErrorResponse;
        Assert.NotNull(response);
        Assert.Equal("Data not inserted", response.Message);
    }
}
