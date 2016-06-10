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
        private const string _messageFormatHeader = @""",""MessageFormat"":""";
        private const string _priorityHeader = @""",""Priority"":";
        private const string _headersHeader = @",""Headers"":{";
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
                return DeserializeFromString(reader.ReadToEnd(), type);
            }
        }

        public string SerializeToString(object item, Type type)
        {
            var message = (SentMessage)item;

            var sb = new StringBuilder();

            sb.Append(_stringValueHeader)
                .Append(Escape(message.StringValue))
                .Append(_messageFormatHeader)
                .Append(message.MessageFormat)
                .Append(_priorityHeader)
                .Append(message.Priority == null ? "null" : message.Priority.ToString())
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
            var enumerator = data.GetEnumerator();

            Skip(enumerator, _stringValueHeader.Length);
            var stringValue = Unescape(GetStringValue(enumerator));
            Skip(enumerator, _messageFormatHeader.Length - 1);
            var messageFormat = (MessageFormat)Enum.Parse(typeof(MessageFormat), GetStringValue(enumerator));
            Skip(enumerator, _priorityHeader.Length);
            var priority = GetNullableByteValue(enumerator);
            Skip(enumerator, _headersHeader.Length);
            var headers = GetHeaders(enumerator).ToDictionary(x => x.Key, x => x.Value);

            return new SentMessage
            {
                StringValue = stringValue,
                MessageFormat = messageFormat,
                Priority = priority,
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

            var wasPrevBackslash = false;

            while (true)
            {
                enumerator.MoveNext();

                if (enumerator.Current == '"')
                {
                    if (wasPrevBackslash)
                    {
                        sb.Append(@"""");
                        wasPrevBackslash = false;
                    }
                    else
                    {
                        return sb.ToString();
                    }
                }
                else if (enumerator.Current == '\\')
                {
                    sb.Append('\\');
                    wasPrevBackslash = !wasPrevBackslash;
                }
                else
                {
                    sb.Append(enumerator.Current);
                    wasPrevBackslash = false;
                }
            }
        }

        private static byte? GetNullableByteValue(IEnumerator<char> enumerator)
        {
            if (enumerator.Current == 'n')
            {
                enumerator.MoveNext(); // u
                enumerator.MoveNext(); // l
                enumerator.MoveNext(); // l
                enumerator.MoveNext(); // Move past last char (to match how we parse the number below)
                return null;
            }

            var sb = new StringBuilder();
            sb.Append(enumerator.Current);
            enumerator.MoveNext(); // Move past first digit (to second digit, if present)
            if (enumerator.Current != ',')
            {
                sb.Append(enumerator.Current);
                enumerator.MoveNext(); // Move past second digit (to third digit, if present)
                if (enumerator.Current != ',')
                {
                    sb.Append(enumerator.Current); // Move past third digit (because we *always* move past the last digit)
                    enumerator.MoveNext();
                }
            }

            return byte.Parse(sb.ToString());
        }

        private static string Escape(string value)
        {
            var sb = new StringBuilder();

            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append(@"\\");
                        break;
                    case '"':
                        sb.Append(@"\""");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            return sb.ToString();
        }

        private static string Unescape(string value)
        {
            var sb = new StringBuilder();

            bool wasPrevBackslash = false;

            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\':
                        if (wasPrevBackslash)
                        {
                            sb.Append('\\');
                            wasPrevBackslash = false;
                        }
                        else
                        {
                            wasPrevBackslash = true;
                        }
                        break;
                    case '"':
                        sb.Append('"');
                        wasPrevBackslash = false;
                        break;
                    default:
                        if (wasPrevBackslash)
                        {
                            sb.Append('\\');
                        }
                        sb.Append(c);
                        wasPrevBackslash = false;
                        break;
                }
            }

            return sb.ToString();
        }
    }
}
