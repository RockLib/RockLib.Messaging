using FluentAssertions;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RockLib.Messaging.RabbitMQ.Tests
{
    public static class RabbitReceiverTests
    {
        [Fact]
        public static void Create()
        {
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());
            model.Setup(_ => _.QueueBind("queue", "exchange", string.Empty, null));

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var receiver = new RabbitReceiver("name", connectionFactory.Object, "queue", "exchange",
                new List<string>(), 20, true))
            {
                receiver.Name.Should().Be("name");
                receiver.Queue.Should().Be("queue");
                receiver.Exchange.Should().Be("exchange");
                receiver.PrefetchCount.Should().Be(20);
                receiver.AutoAck.Should().Be(true);
                receiver.Connection.Should().Be(connection.Object);
                receiver.Channel.Should().Be(model.Object);
            }

            connectionFactory.VerifyAll();
            connection.VerifyAll();
            model.VerifyAll();
        }

        [Fact]
        public static void CreateWithRoutingKeys()
        {
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());
            model.Setup(_ => _.QueueBind("queue", "exchange", "routingKey", null));

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var receiver = new RabbitReceiver("name", connectionFactory.Object, "queue", "exchange",
                new List<string>() { "routingKey" }, 20, true))
            {
                receiver.Name.Should().Be("name");
                receiver.Queue.Should().Be("queue");
                receiver.Exchange.Should().Be("exchange");
                receiver.PrefetchCount.Should().Be(20);
                receiver.AutoAck.Should().Be(true);
                receiver.Connection.Should().Be(connection.Object);
                receiver.Channel.Should().Be(model.Object);
            }

            connectionFactory.VerifyAll();
            connection.VerifyAll();
            model.VerifyAll();
        }

        [Fact]
        public static void CreateWithNulLExchange()
        {
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.Dispose());

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var receiver = new RabbitReceiver("name", connectionFactory.Object, "queue", null,
                new List<string>(), 20, true))
            {
                receiver.Name.Should().Be("name");
                receiver.Queue.Should().Be("queue");
                receiver.Exchange.Should().BeNull();
                receiver.PrefetchCount.Should().Be(20);
                receiver.AutoAck.Should().Be(true);
                receiver.Connection.Should().Be(connection.Object);
                receiver.Channel.Should().Be(model.Object);
            }

            connectionFactory.VerifyAll();
            connection.VerifyAll();
            model.VerifyAll();
        }

        [Fact]
        public static void StartWithPrefetchCount()
        {
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.QueueBind("queue", "exchange", string.Empty, null));
            model.Setup(_ => _.BasicQos(0, 20, false));
            model.Setup(_ => _.BasicConsume("queue", true, string.Empty, false, false, null, It.IsAny<EventingBasicConsumer>())).Returns(string.Empty);
            model.Setup(_ => _.Dispose());

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var receiver = new RabbitReceiver("name", connectionFactory.Object, "queue", "exchange",
                new List<string>(), 20, true))
            {
                receiver.Start(new Func<IReceiverMessage, Task>(message => Task.CompletedTask));
            }

            connectionFactory.VerifyAll();
            connection.VerifyAll();
            model.VerifyAll();
        }

        [Fact]
        public static void StartWithoutPrefetchCount()
        {
            var model = new Mock<IModel>(MockBehavior.Strict);
            model.Setup(_ => _.QueueBind("queue", "exchange", string.Empty, null));
            model.Setup(_ => _.BasicConsume("queue", true, string.Empty, false, false, null, It.IsAny<EventingBasicConsumer>())).Returns(string.Empty);
            model.Setup(_ => _.Dispose());

            var connection = new Mock<IConnection>(MockBehavior.Strict);
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            connection.Setup(_ => _.Dispose());

            var connectionFactory = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactory.Setup(_ => _.CreateConnection()).Returns(connection.Object);

            using (var receiver = new RabbitReceiver("name", connectionFactory.Object, "queue", "exchange",
                new List<string>(), null, true))
            {
                receiver.Start(new Func<IReceiverMessage, Task>(message => Task.CompletedTask));
            }

            connectionFactory.VerifyAll();
            connection.VerifyAll();
            model.VerifyAll();
        }
    }
}