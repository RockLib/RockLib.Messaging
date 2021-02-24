using FluentAssertions;
using Moq;
using RockLib.Messaging;
using RockLib.Messaging.Testing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Example.Tests
{
    public class ExampleServiceTests
    {
        [Fact(DisplayName = "Constructor sets properties from parameters")]
        public void ConstructorHappyPath()
        {
            var receiver = new Mock<IReceiver>().Object;
            var database = new Mock<IDatabase>().Object;

            var service = new ExampleService(receiver, database);

            service.Receiver.Should().BeSameAs(receiver);
            service.Database.Should().BeSameAs(database);
        }

        [Fact(DisplayName = "Constructor throws when receiver parameter is null")]
        public void ConstructorSadPath1()
        {
            var database = new Mock<IDatabase>().Object;

            Action act = () => new ExampleService(null, database);

            act.Should().ThrowExactly<ArgumentNullException>()
                .WithMessage("*receiver*");
        }

        [Fact(DisplayName = "Constructor throws when database parameter is null")]
        public void ConstructorSadPath2()
        {
            var receiver = new Mock<IReceiver>().Object;

            Action act = () => new ExampleService(receiver, null);

            act.Should().ThrowExactly<ArgumentNullException>()
                .WithMessage("*database*");
        }

        [Fact(DisplayName = "Start starts the receiver")]
        public async Task StartAsyncHappyPath()
        {
            var mockReceiver = new Mock<IReceiver>()
                .SetupProperty(m => m.MessageHandler);
            var mockDatabase = new Mock<IDatabase>();

            var receiver = mockReceiver.Object;

            var service = new ExampleService(receiver, mockDatabase.Object);

            receiver.MessageHandler.Should().BeNull();

            await service.StartAsync(CancellationToken.None);

            receiver.MessageHandler.Should().NotBeNull();

            mockReceiver.Verify(m => m.Dispose(), Times.Never());
        }

        [Fact(DisplayName = "Stop disposes the receiver")]
        public async Task StopAsyncHappyPath()
        {
            var mockReceiver = new Mock<IReceiver>()
                .SetupProperty(m => m.MessageHandler);
            var database = new Mock<IDatabase>().Object;

            var service = new ExampleService(mockReceiver.Object, database);

            await service.StartAsync(CancellationToken.None);

            await service.StopAsync(CancellationToken.None);

            mockReceiver.Verify(m => m.Dispose(), Times.Once());
        }

        [Fact(DisplayName = "OnMessageReceived handles 'create' messages with Database.CreateAsync")]
        public async Task OnMessageReceivedHappyPath1()
        {
            var mockReceiver = new Mock<IReceiver>()
                .SetupProperty(m => m.MessageHandler);
            var mockDatabase = new Mock<IDatabase>();

            var service = new ExampleService(mockReceiver.Object, mockDatabase.Object);

            await service.StartAsync(CancellationToken.None);

            var fakeMessage = new FakeReceiverMessage("MyPayload")
            {
                Headers = { ["operation"] = "create" }
            };

            await service.Receiver.MessageHandler.OnMessageReceivedAsync(mockReceiver.Object, fakeMessage);

            mockDatabase.Verify(m => m.CreateAsync("MyPayload"), Times.Once());
            fakeMessage.HandledBy.Should().Be(nameof(fakeMessage.AcknowledgeAsync));

            mockDatabase.Verify(m => m.UpdateAsync("MyPayload"), Times.Never());
            mockDatabase.Verify(m => m.DeleteAsync("MyPayload"), Times.Never());
        }

        [Fact(DisplayName = "OnMessageReceived handles 'update' messages with Database.UpdateAsync")]
        public async Task OnMessageReceivedHappyPath2()
        {
            var mockReceiver = new Mock<IReceiver>()
                .SetupProperty(m => m.MessageHandler);
            var mockDatabase = new Mock<IDatabase>();

            var service = new ExampleService(mockReceiver.Object, mockDatabase.Object);

            await service.StartAsync(CancellationToken.None);

            var fakeMessage = new FakeReceiverMessage("MyPayload")
            {
                Headers = { ["operation"] = "update" }
            };

            await service.Receiver.MessageHandler.OnMessageReceivedAsync(mockReceiver.Object, fakeMessage);

            mockDatabase.Verify(m => m.UpdateAsync("MyPayload"), Times.Once());
            fakeMessage.HandledBy.Should().Be(nameof(fakeMessage.AcknowledgeAsync));

            mockDatabase.Verify(m => m.CreateAsync("MyPayload"), Times.Never());
            mockDatabase.Verify(m => m.DeleteAsync("MyPayload"), Times.Never());
        }

        [Fact(DisplayName = "OnMessageReceived handles 'delete' messages with Database.DeleteAsync")]
        public async Task OnMessageReceivedHappyPath3()
        {
            var mockReceiver = new Mock<IReceiver>()
                .SetupProperty(m => m.MessageHandler);
            var mockDatabase = new Mock<IDatabase>();

            var service = new ExampleService(mockReceiver.Object, mockDatabase.Object);

            await service.StartAsync(CancellationToken.None);

            var fakeMessage = new FakeReceiverMessage("MyPayload")
            {
                Headers = { ["operation"] = "delete" }
            };

            await service.Receiver.MessageHandler.OnMessageReceivedAsync(mockReceiver.Object, fakeMessage);

            mockDatabase.Verify(m => m.DeleteAsync("MyPayload"), Times.Once());
            fakeMessage.HandledBy.Should().Be(nameof(fakeMessage.AcknowledgeAsync));

            mockDatabase.Verify(m => m.CreateAsync("MyPayload"), Times.Never());
            mockDatabase.Verify(m => m.UpdateAsync("MyPayload"), Times.Never());
        }

        [Fact(DisplayName = "OnMessageReceived rejects a messages without an 'operation' header")]
        public async Task OnMessageReceivedSadPath1()
        {
            var mockReceiver = new Mock<IReceiver>()
                .SetupProperty(m => m.MessageHandler);
            var mockDatabase = new Mock<IDatabase>();

            var service = new ExampleService(mockReceiver.Object, mockDatabase.Object);

            await service.StartAsync(CancellationToken.None);

            var fakeMessage = new FakeReceiverMessage("MyPayload");

            await service.Receiver.MessageHandler.OnMessageReceivedAsync(mockReceiver.Object, fakeMessage);

            fakeMessage.HandledBy.Should().Be(nameof(fakeMessage.RejectAsync));

            mockDatabase.Verify(m => m.CreateAsync("MyPayload"), Times.Never());
            mockDatabase.Verify(m => m.UpdateAsync("MyPayload"), Times.Never());
            mockDatabase.Verify(m => m.DeleteAsync("MyPayload"), Times.Never());

            // TODO: Verify that the error log was sent
        }

        [Fact(DisplayName = "OnMessageReceived rejects a messages with an invalid 'operation' header")]
        public async Task OnMessageReceivedSadPath2()
        {
            var mockReceiver = new Mock<IReceiver>()
                .SetupProperty(m => m.MessageHandler);
            var mockDatabase = new Mock<IDatabase>();

            var service = new ExampleService(mockReceiver.Object, mockDatabase.Object);

            await service.StartAsync(CancellationToken.None);

            var fakeMessage = new FakeReceiverMessage("MyPayload")
            {
                Headers = { ["operation"] = "invalid" }
            };

            await service.Receiver.MessageHandler.OnMessageReceivedAsync(mockReceiver.Object, fakeMessage);

            fakeMessage.HandledBy.Should().Be(nameof(fakeMessage.RejectAsync));

            mockDatabase.Verify(m => m.CreateAsync("MyPayload"), Times.Never());
            mockDatabase.Verify(m => m.UpdateAsync("MyPayload"), Times.Never());
            mockDatabase.Verify(m => m.DeleteAsync("MyPayload"), Times.Never());

            // TODO: Verify that the error log was sent
        }
    }
}