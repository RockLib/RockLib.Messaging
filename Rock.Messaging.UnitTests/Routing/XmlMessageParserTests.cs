using System.Xml.Serialization;
using NUnit.Framework;
using Rock.Messaging.Routing;

// ReSharper disable once CheckNamespace
namespace XmlMessageParserTests
{
    public class XmlMessageParserTests
    {
        protected XmlMessageParser _xmlMessageParser;

        protected const string FooCommandXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<FooCommand>
  <Data>123123123123123</Data>
</FooCommand>";

        protected const string InvalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<FooCom";

        protected const string FoobarXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Foobar>
  <Data>123123123123123</Data>
</Foobar>";


        [SetUp]
        public void Setup()
        {
            _xmlMessageParser = new XmlMessageParser();
        }

        public class TheGetTypeNameMethodWithAParameterOfTypeType : XmlMessageParserTests
        {
            [Test]
            public void ReturnsTheElementNameOfTheXmlRootElementNameThatWasRegisteredWithTheType()
            {
                _xmlMessageParser.RegisterXmlRoot(typeof(FooCommand), "Foobar");

                var result = _xmlMessageParser.GetTypeName(typeof(FooCommand));

                Assert.That(result, Is.EqualTo("Foobar"));
            }

            [Test]
            public void ReturnsTheElementNameOfTheXmlRootAttributeThatDecoratesTheType()
            {
                var result = _xmlMessageParser.GetTypeName(typeof(BarEvent));

                Assert.That(result, Is.EqualTo("Bar"));
            }

            [Test]
            public void ReturnsTheNameOfTheTypeWhenItsAssociatedXmlRootAttributeHasAnEmptyElementName()
            {
                var result = _xmlMessageParser.GetTypeName(typeof(BazMessage));

                Assert.That(result, Is.EqualTo("BazMessage"));
            }

            [Test]
            public void ReturnsTheNameOfTheTypeWhenNoXmlRootAttributeIsAssociateWithIt()
            {
                var result = _xmlMessageParser.GetTypeName(typeof(FooCommand));

                Assert.That(result, Is.EqualTo("FooCommand"));
            }
        }

        public class TheGetTypeNameMethodWithAParameterOfTypeString : XmlMessageParserTests
        {
            [Test]
            public void ReturnsTheNameOfTheRootXmlElement()
            {
                var result = _xmlMessageParser.GetTypeName(FooCommandXml);

                Assert.That(result, Is.EqualTo("FooCommand"));
            }
        
            [Test]
            public void ThrowsAnExceptionWhenTheRootXmlElementCannotBeDetermined()
            {
                Assert.That(() => _xmlMessageParser.GetTypeName(InvalidXml), Throws.Exception);
            }
        }

        public class TheDeserializeMessageMethod : XmlMessageParserTests
        {
            [Test]
            public void ReturnsAnInstanceOfTheSpecifiedType()
            {
                var result = _xmlMessageParser.DeserializeMessage<FooCommand>(FooCommandXml);

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<FooCommand>());
            }

            [Test]
            public void AcceptsAnXmlDocumentWithARootElementMatchingTheRegisteredXmlRootElementNameForTheType()
            {
                _xmlMessageParser.RegisterXmlRoot(typeof(FooCommand), "Foobar");

                var result = _xmlMessageParser.DeserializeMessage<FooCommand>(FoobarXml);

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.InstanceOf<FooCommand>());
            }

            [Test]
            public void ThrowsAnExceptionWhenXmlIsInvalid()
            {
                Assert.That(() => _xmlMessageParser.DeserializeMessage<FooCommand>(InvalidXml), Throws.Exception);
            }

            [Test]
            public void ThrowsAnExceptionWhenTheRootElementIsNotExpected()
            {
                Assert.That(() => _xmlMessageParser.DeserializeMessage<FooCommand>(FoobarXml), Throws.Exception);
            }
        }

        public class FooCommand : IMessage
        {
            public string Data { get; set; }
        }

        [XmlRoot("Bar")]
        public class BarEvent : IMessage
        {
            public string Data { get; set; }
        }

        [XmlRoot]
        public class BazMessage : IMessage
        {
            public string Data { get; set; }
        }
    }
}
