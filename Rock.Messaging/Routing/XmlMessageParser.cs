using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Rock.Messaging.Routing
{
    public class XmlMessageParser : IMessageParser
    {
        public string GetTypeName(Type type)
        {
            var xmlRootAttribute = Attribute.GetCustomAttribute(type, typeof(XmlRootAttribute)) as XmlRootAttribute;

            return xmlRootAttribute != null && !string.IsNullOrWhiteSpace(xmlRootAttribute.ElementName)
                ? xmlRootAttribute.ElementName
                : type.Name;
        }

        public string GetTypeName(string rawMessage)
        {
            var match = Regex.Match(rawMessage, @"<([a-zA-Z_][a-zA-Z0-9]*)[ >/]");
            if (!match.Success)
            {
                throw new ArgumentException("Unable to find root xml element.", "rawMessage");
            }

            return match.Groups[1].Value;
        }

        public object DeserializeMessage(string rawMessage, Type messageType)
        {
            var serializer = new XmlSerializer(messageType);
            using (var reader = new StringReader(rawMessage))
            {
                return serializer.Deserialize(reader);
            }
        }
    }
}