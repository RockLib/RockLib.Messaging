using FluentAssertions;
using NUnit.Framework;
using RockLib.Compression;
using System;
using System.Collections.Generic;
using System.Text;

namespace RockLib.Messaging.Tests
{
    [TestFixture]
    public class SenderMessageTests
    {
        private static readonly GZipCompressor _gzip = new GZipCompressor();

        [Test]
        public void StringConstructorNotCompressed()
        {
            var payload = "Hello, world!";

            var message = new SenderMessage(payload, compress: false);

            message.StringPayload.Should().Be(payload);
            message.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(payload));

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers.Should().NotContainKey(HeaderNames.IsBinaryPayload);
            message.IsBinary.Should().BeFalse();

            message.Headers.Should().NotContainKey(HeaderNames.IsCompressedPayload);
            message.IsCompressed.Should().BeFalse();
        }

        [Test]
        public void StringConstructorCompressed()
        {
            var payload = GetCompressablePayload("Hello, world!");

            var message = new SenderMessage(payload, compress: true);

            message.StringPayload.Should().Be(Convert.ToBase64String(_gzip.Compress(Encoding.UTF8.GetBytes(payload))));
            message.BinaryPayload.Should().BeEquivalentTo(_gzip.Compress(Encoding.UTF8.GetBytes(payload)));

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers.Should().NotContainKey(HeaderNames.IsBinaryPayload);
            message.IsBinary.Should().BeFalse();

            message.Headers[HeaderNames.IsCompressedPayload].Should().Be("true");
            message.IsCompressed.Should().BeTrue();
        }

        [Test]
        public void BinaryConstructorNotCompressed()
        {
            var payload = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var message = new SenderMessage(payload, compress: false);

            message.StringPayload.Should().Be(Convert.ToBase64String(payload));
            message.BinaryPayload.Should().BeEquivalentTo(payload);

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers[HeaderNames.IsBinaryPayload].Should().Be("true");
            message.IsBinary.Should().BeTrue();

            message.Headers.Should().NotContainKey(HeaderNames.IsCompressedPayload);
            message.IsCompressed.Should().BeFalse();
        }

        [Test]
        public void BinaryConstructorCompressed()
        {
            var payload = GetCompressablePayload(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

            var message = new SenderMessage(payload, compress: true);

            message.StringPayload.Should().Be(Convert.ToBase64String(_gzip.Compress(payload)));
            message.BinaryPayload.Should().BeEquivalentTo(_gzip.Compress(payload));

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers[HeaderNames.IsBinaryPayload].Should().Be("true");
            message.IsBinary.Should().BeTrue();

            message.Headers[HeaderNames.IsCompressedPayload].Should().Be("true");
            message.IsCompressed.Should().BeTrue();
        }

        [Test]
        public void ReceiverMessageConstructorUncompressedStringPayload()
        {
            var payload = "Hello, world!";
            var receiverMessage = new FakeReceiverMessage(payload, false);

            var message = new SenderMessage(receiverMessage);

            message.StringPayload.Should().Be(payload);
            message.BinaryPayload.Should().BeEquivalentTo(Encoding.UTF8.GetBytes(payload));

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers.Should().NotContainKey(HeaderNames.IsBinaryPayload);
            message.IsBinary.Should().BeFalse();

            message.Headers.Should().NotContainKey(HeaderNames.IsCompressedPayload);
            message.IsCompressed.Should().BeFalse();
        }

        [Test]
        public void ReceiverMessageConstructorCompressedStringPayload()
        {
            var payload = GetCompressablePayload("Hello, world!");
            var receiverMessage = new FakeReceiverMessage(payload, true);

            var message = new SenderMessage(receiverMessage);

            message.StringPayload.Should().Be(Convert.ToBase64String(_gzip.Compress(Encoding.UTF8.GetBytes(payload))));
            message.BinaryPayload.Should().BeEquivalentTo(_gzip.Compress(Encoding.UTF8.GetBytes(payload)));

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers.Should().NotContainKey(HeaderNames.IsBinaryPayload);
            message.IsBinary.Should().BeFalse();

            message.Headers[HeaderNames.IsCompressedPayload].Should().Be("true");
            message.IsCompressed.Should().BeTrue();
        }

        [Test]
        public void ReceiverMessageConstructorUncompressedBinaryPayload()
        {
            var payload = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var receiverMessage = new FakeReceiverMessage(payload, false);

            var message = new SenderMessage(receiverMessage);

            message.StringPayload.Should().Be(Convert.ToBase64String(payload));
            message.BinaryPayload.Should().BeEquivalentTo(payload);

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers[HeaderNames.IsBinaryPayload].Should().Be("true");
            message.IsBinary.Should().BeTrue();

            message.Headers.Should().NotContainKey(HeaderNames.IsCompressedPayload);
            message.IsCompressed.Should().BeFalse();
        }

        [Test]
        public void ReceiverMessageConstructorCompressedBinaryPayload()
        {
            var payload = GetCompressablePayload(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
            var receiverMessage = new FakeReceiverMessage(payload, true);

            var message = new SenderMessage(receiverMessage);

            message.StringPayload.Should().Be(Convert.ToBase64String(_gzip.Compress(payload)));
            message.BinaryPayload.Should().BeEquivalentTo(_gzip.Compress(payload));

            message.Headers.Should().ContainKey(HeaderNames.MessageId);
            message.MessageId.Should().NotBeNull();

            message.Headers[HeaderNames.IsBinaryPayload].Should().Be("true");
            message.IsBinary.Should().BeTrue();

            message.Headers[HeaderNames.IsCompressedPayload].Should().Be("true");
            message.IsCompressed.Should().BeTrue();
        }

        private static string GetCompressablePayload(string seed, int count = 128)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
                sb.Append(seed);
            return sb.ToString();
        }

        private static byte[] GetCompressablePayload(byte[] seed, int count = 128)
        {
            var payload = new byte[count * seed.Length];
            for (int i = 0; i < payload.Length; i++)
                payload[i] = seed[i % seed.Length];
            return payload;
        }

        private class FakeReceiverMessage : IReceiverMessage
        {
            private readonly Lazy<string> _stringPayload;
            private readonly Lazy<byte[]> _binaryPayload;

            public FakeReceiverMessage(string payload, bool compressed)
            {
                _stringPayload = new Lazy<string>(() => payload);
                _binaryPayload = new Lazy<byte[]>(() => Encoding.UTF8.GetBytes(payload));
                Headers = new HeaderDictionary(GetBackingHeaderDictionary(false, compressed));
            }

            public FakeReceiverMessage(byte[] payload, bool compressed)
            {
                _stringPayload = new Lazy<string>(() => Convert.ToBase64String(payload));
                _binaryPayload = new Lazy<byte[]>(() => payload);
                Headers = new HeaderDictionary(GetBackingHeaderDictionary(true, compressed));
            }

            public string StringPayload => _stringPayload.Value;
            public byte[] BinaryPayload => _binaryPayload.Value;
            public HeaderDictionary Headers { get; }
            public bool Handled => false;
            public void Acknowledge() {}
            public void Rollback() {}
            public void Reject() {}

            private static IReadOnlyDictionary<string, object> GetBackingHeaderDictionary(bool binary, bool compressed)
            {
                var headers = new Dictionary<string, object>();
                headers[HeaderNames.MessageId] = Guid.NewGuid().ToString("D");
                if (binary)
                    headers[HeaderNames.IsBinaryPayload] = "true";
                if (compressed)
                    headers[HeaderNames.IsCompressedPayload] = "true";
                return headers;
            }
        }
    }
}
