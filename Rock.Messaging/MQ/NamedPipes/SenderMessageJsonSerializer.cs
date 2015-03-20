using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rock.Serialization;

namespace Rock.Messaging.NamedPipes
{
    internal class SenderMessageJsonSerializer : ISerializer
    {
        private const string _stringValueHeader = @"{""StringValue"":""";
        private const string _binaryValueHeader = @""",""BinaryValue"":""";
        private const string _headersHeader = @""",""Headers"":{";
        private const string _quote = @"""";
        private const string _headerSeparator = @""":""";

        // This is the default encoding that the StreamWriter class uses.
        private static readonly Encoding _defaultEncoding = new UTF8Encoding(false, true);

        public void SerializeToStream(Stream stream, object item, Type type)
        {
            using (var writer = new StreamWriter(stream, _defaultEncoding, 1024, true))
            {
                writer.Write(SerializeToString(item, type));
            }
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
            {
                return DeserializeFromChars(GetChars(reader));
            }
        }

        private static IEnumerable<char> GetChars(StreamReader reader)
        {
            int i;
            while ((i = reader.Read()) != -1)
            {
                yield return (char)i;
            }
        }

        public string SerializeToString(object item, Type type)
        {
            var message = (ISenderMessage)item;

            var sb = new StringBuilder();

            sb.Append(_stringValueHeader)
                .Append(Escape(message.StringValue))
                .Append(_binaryValueHeader)
                .Append(Convert.ToBase64String(message.BinaryValue))
                .Append(_headersHeader);

            if (message.Headers != null)
            {
                var first = true;

                foreach (var header in message.Headers)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(",");
                    }

                    sb.Append(_quote)
                        .Append(Escape(header.Key))
                        .Append(_headerSeparator)
                        .Append(Escape(header.Value))
                        .Append(_quote);
                }
            }

            sb.Append(@"}}");

            return sb.ToString();
        }

        public object DeserializeFromString(string data, Type type)
        {
            return DeserializeFromChars(data);
        }

        private static object DeserializeFromChars(IEnumerable<char> data)
        {
            var enumerator = data.GetEnumerator();

            Skip(enumerator, _stringValueHeader.Length);
            var stringValue = Unescape(GetStringValue(enumerator));
            Skip(enumerator, _binaryValueHeader.Length - 1);
            var binaryValue = Convert.FromBase64String(GetStringValue(enumerator));
            Skip(enumerator, _headersHeader.Length);
            var headers = GetHeaders(enumerator).ToDictionary(x => x.Key, x => x.Value);

            return new SentMessage
            {
                StringValue = stringValue,
                BinaryValue = binaryValue,
                Headers = headers
            };
        }

        private static IEnumerable<KeyValuePair<string, string>> GetHeaders(IEnumerator<char> enumerator)
        {
            while (true)
            {
                if (enumerator.Current == '}')
                {
                    yield break;
                }

                var key = Unescape(GetStringValue(enumerator));
                Skip(enumerator, _headerSeparator.Length - 1);
                var value = Unescape(GetStringValue(enumerator));
                Skip(enumerator, _quote.Length);

                if (enumerator.Current == ',')
                {
                    Skip(enumerator, 1);
                }

                yield return new KeyValuePair<string, string>(key, value);
            }
        }

        private static void Skip(IEnumerator enumerator, int count)
        {
            for (var i = 0; i < count; i++)
            {
                enumerator.MoveNext();
            }
        }

        private static string GetStringValue(IEnumerator<char> enumerator)
        {
            var sb = new StringBuilder();

            var prev = '\0';

            while (true)
            {
                enumerator.MoveNext();

                if (enumerator.Current == '"')
                {
                    if (prev == '\\')
                    {
                        sb.Append(@"""");
                    }
                    else
                    {
                        return sb.ToString();
                    }
                }
                else
                {
                    sb.Append(enumerator.Current);
                }

                prev = enumerator.Current;
            }
        }

        private static string Escape(string value)
        {
            return value.Replace(_quote, "\\\"");
        }

        private static string Unescape(string value)
        {
            return value.Replace("\\\"", _quote);
        }
    }
}
