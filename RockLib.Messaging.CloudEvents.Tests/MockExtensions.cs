using Moq;
using System.Text.RegularExpressions;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public static class MockExtensions
    {
        public static Mock<IProtocolBinding> SetupTestProtocolBinding(this Mock<IProtocolBinding> mockProtocolBinding)
        {
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            mockProtocolBinding.Setup(m => m.GetAttributeName(It.IsAny<string>(), out It.Ref<bool>.IsAny))
                .Callback(new GetAttributeNameCallback(TestGetAttributeNameCallback))
                .Returns(new GetAttributeNameDelegate(TestGetAttributeName));

            return mockProtocolBinding;
        }

        public delegate void GetAttributeNameCallback(string headerName, out bool isCloudEventAttribute);

        public delegate string GetAttributeNameDelegate(string headerName, out bool isCloudEventAttribute);

        public static void TestGetAttributeNameCallback(string headerName, out bool isCloudEventAttribute)
        {
            var attributeName = Regex.Replace(headerName, "^test-", "");
            isCloudEventAttribute = attributeName != headerName;
        }

        public static string TestGetAttributeName(string headerName, out bool isCloudEventAttribute)
        {
            var attributeName = Regex.Replace(headerName, "^test-", "");
            isCloudEventAttribute = attributeName != headerName;
            return attributeName;
        }
    }
}
