using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.RabbitMQ.Tests
{
    public static class RabbitSenderTests
    {
        [Fact]
        public static void Create()
        {
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var sender = new RabbitSender("name", connectionFactory.Object,
                "exchange", "routingKey", "routingKeyHeaderName", false))
            {
                sender.Name.Should().Be("name");
                sender.Exchange.Should().Be("exchange");
                sender.RoutingKey.Should().Be("routingKey");
                sender.RoutingKeyHeaderName.Should().Be("routingKeyHeaderName");
                sender.Persistent.Should().Be(false);
                sender.Connection.Should().Be(connection.Object);
                sender.Channel.Should().Be(model.Object);
            }

            connectionFactory.VerifyAll();
            connection.VerifyAll();
            model.VerifyAll();
        }

        [Fact]
        public static void DisposeWhenChannelAndConnectionAreNotCreated()
        {
            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);

            using (var sender = new RabbitSender("name", connectionFactory.Object,
                "exchange", "routingKey", "routingKeyHeaderName", false))
            { }

            connectionFactory.VerifyAll();
        }

        [Fact]
        public static void DisposeWhenChannelIsNotCreated()
        {
            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var sender = new RabbitSender("name", connectionFactory.Object,
                "exchange", "routingKey", "routingKeyHeaderName", false))
            {
                _ = sender.Connection;
            }

            connectionFactory.VerifyAll();
        }

        [Fact]
        public static void DisposeWhenConnectionIsNotCreated()
        {
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var sender = new RabbitSender("name", connectionFactory.Object,
                "exchange", "routingKey", "routingKeyHeaderName", false))
            {
                _ = sender.Channel;
            }

            connectionFactory.VerifyAll();
            connection.VerifyAll();
            model.VerifyAll();
        }

        [Fact]
        public static async Task SendAsyncWhenOriginatingSystemIsNullAndNoPersistence()
        {
            var message = new SenderMessage("payload");

            var properties = new Mock<IBasicProperties>(MockBehavior.Strict);
            properties.SetupSet(_ => _.Headers = message.Headers);
            
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());
            model.Setup(_ => _.CreateBasicProperties()).Returns(properties.Object);
            model.Setup(_ => _.BasicPublish("exchange", It.IsAny<string>(), false, properties.Object, message.BinaryPayload));

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var sender = new RabbitSender("name", connectionFactory.Object,
                "exchange", "routingKey", "routingKeyHeaderName", false))
            {
                await sender.SendAsync(message, default);
                message.OriginatingSystem.Should().Be("RabbitMQ");
            }

            properties.VerifyAll();
            model.VerifyAll();
            connection.VerifyAll();
            connectionFactory.VerifyAll();
        }

        [Fact]
        public static async Task SendAsyncWhenOriginatingSystemIsNotNullAndNoPersistence()
        {
            var message = new SenderMessage("payload");
            message.OriginatingSystem = "OriginatingSystem";

            var properties = new Mock<IBasicProperties>(MockBehavior.Strict);
            properties.SetupSet(_ => _.Headers = message.Headers);

            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());
            model.Setup(_ => _.CreateBasicProperties()).Returns(properties.Object);
            model.Setup(_ => _.BasicPublish("exchange", It.IsAny<string>(), false, properties.Object, message.BinaryPayload));

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var sender = new RabbitSender("name", connectionFactory.Object,
                "exchange", "routingKey", "routingKeyHeaderName", false))
            {
                await sender.SendAsync(message, default);
                message.OriginatingSystem.Should().Be("OriginatingSystem");
            }

            properties.VerifyAll();
            model.VerifyAll();
            connection.VerifyAll();
            connectionFactory.VerifyAll();
        }

        [Fact]
        public static async Task SendAsyncWhenOriginatingSystemIsNotNullAndPersistence()
        {
            var message = new SenderMessage("payload");
            message.OriginatingSystem = "OriginatingSystem";

            var properties = new Mock<IBasicProperties>(MockBehavior.Strict);
            properties.SetupSet(_ => _.Headers = message.Headers);
            properties.SetupSet(_ => _.Persistent = true);

            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());
            model.Setup(_ => _.CreateBasicProperties()).Returns(properties.Object);
            model.Setup(_ => _.BasicPublish("exchange", It.IsAny<string>(), false, properties.Object, message.BinaryPayload));

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var sender = new RabbitSender("name", connectionFactory.Object,
                "exchange", "routingKey", "routingKeyHeaderName", true))
            {
                await sender.SendAsync(message, default);
                message.OriginatingSystem.Should().Be("OriginatingSystem");
            }

            properties.VerifyAll();
            model.VerifyAll();
            connection.VerifyAll();
            connectionFactory.VerifyAll();
        }

        [Fact]
        public static async Task SendAsyncWhenMessageIsNull()
        {
            using var sender = new RabbitSender("name", Mock.Of<IConnectionFactory>(),
                "exchange", "routingKey", "routingKeyHeaderName", false);
            Func<Task> send = async () => await sender.SendAsync(null!, default);
            await send.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}