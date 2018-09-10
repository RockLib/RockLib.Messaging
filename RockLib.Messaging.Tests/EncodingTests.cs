using FluentAssertions;
using NUnit.Framework;
using RockLib.Messaging.ImplementationHelpers;
using System;
using System.Collections.Generic;
using System.Text;

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
        }

        [Test]
        public void CompressedStringSenderMessageToStringReceiverMessageAsStringPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(originalPayload);
        }

        [Test]
        public void UncompressedStringSenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
        }

        [Test]
        public void CompressedStringSenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
        }

        [Test]
        public void UncompressedStringSenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(originalPayload);
        }

        [Test]
        public void CompressedStringSenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(originalPayload);
        }

        [Test]
        public void UncompressedStringSenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
        }

        [Test]
        public void CompressedStringSenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetStringExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(originalPayload));
        }

        [Test]
        public void UncompressedBinarySenderMessageToStringReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
        }

        [Test]
        public void CompressedBinarySenderMessageToStringReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
        }

        [Test]
        public void UncompressedBinarySenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
        }

        [Test]
        public void CompressedBinarySenderMessageToStringReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.StringPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
        }

        [Test]
        public void UncompressedBinarySenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
        }

        [Test]
        public void CompressedBinarySenderMessageToBinaryReceiverMessageAsStringPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.StringPayload.Should().Be(Convert.ToBase64String(originalPayload));
        }

        [Test]
        public void UncompressedBinarySenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
        }

        [Test]
        public void CompressedBinarySenderMessageToBinaryReceiverMessageAsBinaryPayload()
        {
            var originalPayload = GetBinaryExample();
            var senderMessage = new SenderMessage(originalPayload, compress: true);
            var receiverMessage = new TestReceiverMessage(senderMessage.BinaryPayload, senderMessage.Headers);
            receiverMessage.BinaryPayload.Should().BeEquivalentTo(originalPayload);
        }

        private static byte[] GetBinaryExample()
        {
            var data = new byte[1024];

            for (int i = 0; i < data.Length; i++)
                data[i] = (byte)(i % 8);

            return data;
        }

        private static string GetStringExample() => "abcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh";

        private class TestReceiverMessage : IReceiverMessage
        {
            private readonly Lazy<string> _stringPayload;
            private readonly Lazy<byte[]> _binaryPayload;

            public TestReceiverMessage(string rawStringPayload, IDictionary<string, object> headers)
            {
                Headers = new HeaderDictionary(new Dictionary<string, object>(headers));
                this.SetLazyPayloadFields(rawStringPayload, out _stringPayload, out _binaryPayload);
            }

            public TestReceiverMessage(byte[] rawBinaryPayload, IDictionary<string, object> headers)
            {
                Headers = new HeaderDictionary(new Dictionary<string, object>(headers));
                this.SetLazyPayloadFields(rawBinaryPayload, out _stringPayload, out _binaryPayload);
            }

            public string StringPayload => _stringPayload.Value;

            public byte[] BinaryPayload => _binaryPayload.Value;

            public HeaderDictionary Headers { get; }

            public byte? Priority => throw new NotImplementedException();

            public bool IsTransactional => throw new NotImplementedException();

            public void Acknowledge() => throw new NotImplementedException();

            public void Rollback() => throw new NotImplementedException();
        }
    }
}
