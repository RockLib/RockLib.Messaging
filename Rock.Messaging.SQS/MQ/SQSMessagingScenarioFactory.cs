using Amazon.SQS;
using System;
using System.Collections.Generic;

#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    /// <summary>
    /// An implementation of <see cref="IMessagingScenarioFactory"/> that creates
    /// objects that use SQS to send and receive messages.
    /// </summary>
    public class SQSMessagingScenarioFactory : IMessagingScenarioFactory
    {
        private readonly ISQSConfigurationProvider _configurationProvider;

#if ROCKLIB
        /// <summary>
        /// Initializes a new instance of the <see cref="SQSMessagingScenarioFactory"/> class.
        /// </summary>
        /// <param name="sqsSettings">
        /// A collection of <see cref="SQSConfiguration"/> objects that define the objects that
        /// this instance of <see cref="SQSMessagingScenarioFactory"/> will be able to create.
        /// Each <see cref="SQSConfiguration"/> instance in <paramref name="sqsSettings"/> must
        /// have a unique name.
        /// </param>
        public SQSMessagingScenarioFactory(IEnumerable<SQSConfiguration> sqsSettings)
            : this(new SQSConfigurationProvider(sqsSettings))
        {
        }
#else
        public SQSMessagingScenarioFactory(XmlDeserializingSQSConfigurationProvider sqsSettings)
            : this((ISQSConfigurationProvider)sqsSettings)
        {
        }
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="SQSMessagingScenarioFactory"/> class.
        /// </summary>
        /// <param name="configurationProvider">
        /// An object that provides the configurations used by this instance to create objects.
        /// </param>
        public SQSMessagingScenarioFactory(ISQSConfigurationProvider configurationProvider)
        {
            if (configurationProvider == null) throw new ArgumentNullException(nameof(configurationProvider));
            _configurationProvider = configurationProvider;
        }

        /// <summary>
        /// Creates an instance of <see cref="IReceiver"/> that uses the queue consumer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="IReceiver"/> that uses the queue consumer scenario.</returns>
        /// <remarks>
        /// Note to implementors of this interface: If this scenario is not applicable,
        /// return the result of a call to <see cref="CreateTopicSubscriber"/> instead of
        /// throwing an exception.
        /// </remarks>
        public IReceiver CreateQueueConsumer(string name)
        {
            var configuration = _configurationProvider.GetConfiguration(name);
            return new SQSQueueReceiver(configuration, CreateSqsClient());
        }

        /// <summary>
        /// Creates an instance of <see cref="ISender"/> that uses the queue producer scenario.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>An instance of <see cref="ISender"/> that uses the queue producer scenario.</returns>
        /// <remarks>
        /// Note to implementors of this interface: If this scenario is not applicable,
        /// return the result of a call to <see cref="CreateTopicPublisher"/> instead of
        /// throwing an exception.
        /// </remarks>
        public ISender CreateQueueProducer(string name)
        {
            var configuration = _configurationProvider.GetConfiguration(name);
            return new SQSQueueSender(configuration, CreateSqsClient());
        }

        /// <summary>
        /// Not implemented, throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="name">Not used.</param>
        /// <exception cref="NotImplementedException">This method is not implemented.</exception>
        public ISender CreateTopicPublisher(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, throws a <see cref="NotImplementedException"/>.
        /// </summary>
        /// <param name="name">Not used.</param>
        /// <exception cref="NotImplementedException">This method is not implemented.</exception>
        public IReceiver CreateTopicSubscriber(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a value indicating whether a scenario by the given name can be created by this
        /// instance of <see cref="SQSMessagingScenarioFactory"/>.
        /// </summary>
        /// <param name="name">The name of the scenario.</param>
        /// <returns>True, if the scenario can be created. Otherwise, false.</returns>
        public bool HasScenario(string name)
        {
            return _configurationProvider.HasConfiguration(name);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private static IAmazonSQS CreateSqsClient()
        {
            return new AmazonSQSClient();
        }
    }
}
