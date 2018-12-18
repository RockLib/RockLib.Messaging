using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class EncodingTests
    {
        [Test]
        public void UncompressedStringSenderMessageToStringReceiverMessageAsStringPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedStringSenderMessageToStringReceiverMessageAsStringPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void UncompressedStringSenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedStringSenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void UncompressedStringSenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedStringSenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void UncompressedStringSenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedStringSenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void UncompressedBinarySenderMessageToStringReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedBinarySenderMessageToStringReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void UncompressedBinarySenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedBinarySenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void UncompressedBinarySenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedBinarySenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void UncompressedBinarySenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        [Test]
        public void CompressedBinarySenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
            var senderCopy = new SenderMessage(receiverMessage);
            senderCopy.StringPayload.Should().Be(senderMessage.StringPayload);
            senderCopy.BinaryPayload.Should().BeEquivalentTo(senderMessage.BinaryPayload);
        }

        private static byte[] GetBinaryExample()
        {
            var data = new byte[1024];

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(i % 8);

            return data;
        }

        private static string GetStringExample() => "abcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh";

        private class TestReceiverMessage : ReceiverMessage
        {
            private readonly IDictionary<string, object> _headers;

            public TestReceiverMessage(string rawStringPayload, IDictionary<string, object> headers)
                : base(() => rawStringPayload)
            {
                _headers = headers;
            }

            public TestReceiverMessage(byte[] rawBinaryPayload, IDictionary<string, object> headers)
                : base(() => rawBinaryPayload)
            {
                _headers = headers;
            }

            protected override Task AcknowledgeMessageAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

            protected override Task RollbackMessageAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

            protected override Task RejectMessageAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

            protected override void InitializeHeaders(IDictionary<string, object> headers)
            {
                foreach (var header in _headers)
                    headers.Add(header);
            }
        }
    }
}
