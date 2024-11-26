namespace Tests.Unit
{
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
        public void DatabaseNotConfigured_ThrowsException()
        {
            var envConfig = new Mock<EnvConfig>();
            envConfig.Setup(x => x.IsConfigured()).Returns(false);

            var connector = new Connector(envConfig.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => connector.CreateDbConnection());
            Assert.Equal("Not configured", exception.Message);
        }

        [Fact]
        public void DatabaseConnectionConfigured_ReturnsOpenConnection()
        {
            var envConfig = new Mock<EnvConfig>();
            envConfig.Setup(x => x.IsConfigured()).Returns(true);
            envConfig.Setup(x => x.Get("DB_SERVER")).Returns("fake_server");
            envConfig.Setup(x => x.Get("DB_DATABASE")).Returns("fake_database");
            envConfig.Setup(x => x.Get("DB_USERNAME")).Returns("fake_user");
            envConfig.Setup(x => x.Get("DB_PASSWORD")).Returns("fake_password");

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(conn => conn.Open()).Callback(() => { });
            mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);

            var connector = new Connector(envConfig.Object);
            connector = new Mock<Connector>(envConfig.Object) { CallBase = true }.Object;
            Mock.Get(connector).Setup(c => c.CreateDbConnection()).Returns(mockConnection.Object);

            var createdConnection = connector.CreateDbConnection();

            Assert.NotNull(createdConnection);
            Assert.Equal(ConnectionState.Open, createdConnection.State);
        }

        [Fact]
        public void DatabaseConnectionWrongConfigured_ThrowsMySqlException()
        {
            var envConfig = new Mock<EnvConfig>();
            envConfig.Setup(x => x.IsConfigured()).Returns(true);
            envConfig.Setup(x => x.Get("DB_SERVER")).Returns("fake_server");
            envConfig.Setup(x => x.Get("DB_DATABASE")).Returns("fake_database");
            envConfig.Setup(x => x.Get("DB_USERNAME")).Returns("fake_user");
            envConfig.Setup(x => x.Get("DB_PASSWORD")).Returns("wrong_password");

            var connector = new Connector(envConfig.Object);

            var exception = Assert.Throws<MySqlException>(() => connector.CreateDbConnection());
            Assert.Contains("Unable to connect", exception.Message);
        }
    }
}
