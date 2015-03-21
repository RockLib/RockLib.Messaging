using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private const string _jsonPattern = @"
            ^{
                ""StringValue"":""(?<stringValue>(?:[^""]|\"")*?)""
                ,
                ""BinaryValue"":""(?<binaryValue>(?:[^""]|\"")*?)""
                ,
                ""Headers"":
                {
                    (?:
                        ""(?<key>(?:[^""]|\"")*?)"":""(?<value>(?:[^""]|\"")*?)""
                        (?:
                            ,
                            ""(?<key>(?:[^""]|\"")*?)"":""(?<value>(?:[^""]|\"")*?)""
                        )*
                    )?
                }
            }$";

        private static readonly Regex _jsonRegex = new Regex(_jsonPattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

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
            var message = (ISenderMessage)item;

            var sb = new StringBuilder();

            sb.Append(_stringValueHeader)
                .Append(Escape(message.StringValue))
                .Append(_binaryValueHeader)
                .Append(message.BinaryValue == null ? null : Convert.ToBase64String(message.BinaryValue))
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
            var match = _jsonRegex.Match(data);

            if (!match.Success)
            {
                var ex = new FormatException("Invalid message format.");
                ex.Data.Add("message", data);
                throw ex;
            }

            return new SentMessage
            {
                StringValue = Unescape(match.Groups["stringValue"].Value),
                BinaryValue = Convert.FromBase64String(match.Groups["binaryValue"].Value),
                Headers = GetHeaders(match).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private static IEnumerable<KeyValuePair<string, string>> GetHeaders(Match match)
        {
            var keys = match.Groups["key"].Captures.Cast<Capture>().GetEnumerator();
            var values = match.Groups["value"].Captures.Cast<Capture>().GetEnumerator();

            while (keys.MoveNext() && values.MoveNext())
            {
                yield return new KeyValuePair<string, string>(
                    Unescape(keys.Current.Value),
                    Unescape(values.Current.Value));
            }
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
