namespace Tests.Unit;

using System;
using System.Data;
using Xunit;
using Moq;
using WPR.Data;
using WPR.Database;
using MySql.Data.MySqlClient;

public class ConnectorTests
{
    [Fact]
    public void DatabaseNotConffigured()
    {
        var envConfig = new Mock<EnvConfig>();
        envConfig.Setup(x => x.IsConfigured()).Returns(false);
        var connector = new Connector(envConfig.Object);

        Assert.Throws<InvalidOperationException>(() => connector.CreateDbConnection());
    }

    [Fact]
    public void DatabaseConnectionConffigured()
    {
        var envConfig = new Mock<EnvConfig>();
        envConfig.Setup(x => x.IsConfigured()).Returns(true);
        envConfig.Setup(x => x.Get("DB_SERVER")).Returns("95.99.30.110");
        envConfig.Setup(x => x.Get("DB_DATABASE")).Returns("WPR");
        envConfig.Setup(x => x.Get("DB_USERNAME")).Returns("root");
        envConfig.Setup(x => x.Get("DB_PASSWORD")).Returns("WPR");

        var connector = new Connector(envConfig.Object);

        using var connection = connector.CreateDbConnection();

        Assert.NotNull(connection);
        Assert.Equal(ConnectionState.Open, connection.State);
    }

    [Fact]
    public void DatabaseConnectionWrongConffigured()
    {
        var envConfig = new Mock<EnvConfig>();
        envConfig.Setup(x => x.IsConfigured()).Returns(true);
        envConfig.Setup(x => x.Get("DB_SERVER")).Returns("95.99.30.110");
        envConfig.Setup(x => x.Get("DB_DATABASE")).Returns("WPR");
        envConfig.Setup(x => x.Get("DB_USERNAME")).Returns("incorrect_user"); 
        envConfig.Setup(x => x.Get("DB_PASSWORD")).Returns("WPR");

        var connector = new Connector(envConfig.Object);

        var exception = Assert.Throws<MySqlException>(() => connector.CreateDbConnection());
        Assert.Contains("Access denied for user 'incorrect_user'@", exception.Message);
    }
}
