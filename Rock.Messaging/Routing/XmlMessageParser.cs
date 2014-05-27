using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Rock.Messaging.Routing
{
    public class XmlMessageParser : IMessageParser
    {
        private readonly ConcurrentDictionary<Type, XmlRootAttribute> _xmlRootAttributes = new ConcurrentDictionary<Type, XmlRootAttribute>();

        public void RegisterXmlRoot(Type messageType, string xmlRootElementName)
        {
            if (string.IsNullOrWhiteSpace(xmlRootElementName))
            {
                XmlRootAttribute dummy;
                _xmlRootAttributes.TryRemove(messageType, out dummy);
            }
            else
            {
                _xmlRootAttributes.AddOrUpdate(
                    messageType,
                    t => new XmlRootAttribute(xmlRootElementName),
                    (t, a) => new XmlRootAttribute(xmlRootElementName));
            }
        }

        public string GetTypeName(Type messageType)
        {
            XmlRootAttribute xmlRootAttribute;
            if (!_xmlRootAttributes.TryGetValue(messageType, out xmlRootAttribute))
            {
                xmlRootAttribute = Attribute.GetCustomAttribute(messageType, typeof(XmlRootAttribute)) as XmlRootAttribute;
            }

            return xmlRootAttribute != null && !string.IsNullOrWhiteSpace(xmlRootAttribute.ElementName)
                ? xmlRootAttribute.ElementName
                : messageType.Name;
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

        public TMessage DeserializeMessage<TMessage>(string rawMessage)
        {
            using (var reader = new StringReader(rawMessage))
            {
                return (TMessage)GetXmlSerializer(typeof(TMessage)).Deserialize(reader);
            }
        }

        private XmlSerializer GetXmlSerializer(Type messageType)
        {
            XmlRootAttribute xmlRootAttribute;
            return _xmlRootAttributes.TryGetValue(messageType, out xmlRootAttribute)
                ? new XmlSerializer(messageType, xmlRootAttribute)
                : new XmlSerializer(messageType);
        }
    }
}