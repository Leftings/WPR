namespace Tests.Unit;

using System.Data;
using Xunit;
using Moq;
using WPR.Data;
using WPR.Database;
using MySql.Data.MySqlClient;

public class ConnectorTests
{
    [Fact]
    public void CreateDbConnection_ShouldThrowException_WhenNotConfigured()
    {
        // Arrange
        var envConfig = new Mock<EnvConfig>();
        envConfig.Setup(x => x.IsConfigured()).Returns(false);
        var connector = new Connector(envConfig.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => connector.CreateDbConnection());
    }

    [Fact]
    public void CreateDbConnection_ShouldConnect_WhenProperlyConfigured()
    {
        // Arrange
        var envConfig = new Mock<EnvConfig>();
        envConfig.Setup(x => x.IsConfigured()).Returns(true);
        envConfig.Setup(x => x.Get("DB_SERVER")).Returns("95.99.30.110");
        envConfig.Setup(x => x.Get("DB_DATABASE")).Returns("WPR");
        envConfig.Setup(x => x.Get("DB_USERNAME")).Returns("root");
        envConfig.Setup(x => x.Get("DB_PASSWORD")).Returns("WPR");

        var connector = new Connector(envConfig.Object);

        // Act
        using var connection = connector.CreateDbConnection();

        // Assert
        Assert.NotNull(connection);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public void CreateDbConnection_ShouldThrowMySqlException_WhenCredentialsAreInvalid()
    {
        // Arrange
        var envConfig = new Mock<EnvConfig>();
        envConfig.Setup(x => x.IsConfigured()).Returns(true);
        envConfig.Setup(x => x.Get("DB_SERVER")).Returns("95.99.30.110");
        envConfig.Setup(x => x.Get("DB_DATABASE")).Returns("WPR");
        envConfig.Setup(x => x.Get("DB_USERNAME")).Returns("incorrect_user");
        envConfig.Setup(x => x.Get("DB_PASSWORD")).Returns("WPR");

        var connector = new Connector(envConfig.Object);

        // Act & Assert
        Assert.Throws<MySqlException>(() => connector.CreateDbConnection());
    }
}
