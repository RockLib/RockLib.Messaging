using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Rock.Messaging.Routing
{
    public class XmlMessageParser : IMessageParser
    {
        private readonly Regex _tagRegex;
        private readonly ConcurrentDictionary<Type, XmlRootAttribute> _xmlRootAttributes = new ConcurrentDictionary<Type, XmlRootAttribute>();

        public XmlMessageParser()
        {
            // From XML Specifications: http://www.w3.org/TR/REC-xml/#sec-starttags
            const string nameStartCharacters = @":A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD"; //\u10000-\uEFFFF"; // .net regex doesn't support high-value unicode pointcodes. :(
            const string nameCharacters = @"-.0-9\xB7\u0300-\u036F\u203F-\u2040" + nameStartCharacters;
            var namePattern = string.Format("[{0}][{1}]*", nameStartCharacters, nameCharacters);
            var tagPattern = string.Format(@"<({0})[ \r\n\t>/]", namePattern);
            _tagRegex = new Regex(tagPattern, RegexOptions.Compiled);
        }

        public void RegisterXmlRoot(Type messageType, string xmlRootElementName)
        {
            _xmlRootAttributes.AddOrUpdate(
                    messageType,
                    t => new XmlRootAttribute(xmlRootElementName),
                    (t, a) => new XmlRootAttribute(xmlRootElementName));
        }

        public void DeregisterXmlRoot(Type messageType)
        {
            XmlRootAttribute dummy;
            _xmlRootAttributes.TryRemove(messageType, out dummy);
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
            var match = _tagRegex.Match(rawMessage);
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