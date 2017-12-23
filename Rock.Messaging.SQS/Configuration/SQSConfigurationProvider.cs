using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Messaging.SQS
{
    /// <summary>
    /// An implementation of <see cref="ISQSConfigurationProvider"/> that is backed by a
    /// collection of <see cref="SQSConfiguration"/> objects.
    /// </summary>
    public class SQSConfigurationProvider: ISQSConfigurationProvider
    {
        private readonly IReadOnlyDictionary<string, SQSConfiguration> _configurations;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQSConfigurationProvider"/> class.
        /// </summary>
        /// <param name="configurations">
        /// A collection of <see cref="SQSConfiguration"/> objects that this instance of
        /// <see cref="SQSConfigurationProvider"/> will be able to retrieve. Each instance
        /// must have a unique name.
        /// </param>
        public SQSConfigurationProvider(IEnumerable<SQSConfiguration> configurations)
        {
            _configurations = configurations?.ToDictionary(c => c.Name) ?? throw new ArgumentNullException(nameof(configurations));
        }

        /// <summary>
        /// Gets the collection of <see cref="SQSConfiguration"/> objects that this instance of
        /// <see cref="SQSConfigurationProvider"/> can retrieve.
        /// </summary>
        public IEnumerable<SQSConfiguration> Configurations { get { return _configurations.Values; } }

        /// <summary>
        /// Returns an instance of <see cref="SQSConfiguration"/> for the provided name.
        /// </summary>
        /// <param name="name">The name of the <see cref="SQSConfiguration"/> to retrieve.</param>
        /// <returns>The <see cref="SQSConfiguration"/> with the provided name.</returns>
        public SQSConfiguration GetConfiguration(string name)
        {
            return _configurations[name];
        }

        /// <summary>
        /// Returns a value indicating whether this instance of <see cref="SQSConfigurationProvider"/>
        /// can retrieve an instacne of <see cref="SQSConfiguration"/> by the provided name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True, if a configuration can be retrieved, otherwise false.</returns>
        public bool HasConfiguration(string name)
        {
            return _configurations.ContainsKey(name);
        }

        ISQSConfiguration ISQSConfigurationProvider.GetConfiguration(string name)
        {
            return GetConfiguration(name);
        }
    }
}
