using Moq;
using System;
using System.Text.RegularExpressions;

namespace RockLib.Messaging.CloudEvents.Tests
{
    public static class MockExtensions
    {
        public static Mock<IProtocolBinding> SetupTestProtocolBinding(this Mock<IProtocolBinding> mockProtocolBinding)
        {
            if (mockProtocolBinding is null)
            {
                throw new ArgumentNullException(nameof(mockProtocolBinding));
            }
            mockProtocolBinding.Setup(m => m.GetHeaderName(It.IsAny<string>())).Returns<string>(header => "test-" + header);

            mockProtocolBinding.Setup(m => m.GetAttributeName(It.IsAny<string>(), out It.Ref<bool>.IsAny))
                .Callback(new GetAttributeNameCallback(TestGetAttributeNameCallback))
                .Returns(new GetAttributeName(TestGetAttributeName));

            return mockProtocolBinding;
        }

        public delegate void GetAttributeNameCallback(string headerName, out bool isCloudEventAttribute);

        public delegate string GetAttributeName(string headerName, out bool isCloudEventAttribute);

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
