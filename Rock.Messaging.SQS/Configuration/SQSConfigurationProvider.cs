using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Messaging.SQS
{
    public class SQSConfigurationProvider: ISQSConfigurationProvider
    {
        private readonly IReadOnlyDictionary<string, SQSConfiguration> _configurations;

        public SQSConfigurationProvider(IEnumerable<SQSConfiguration> configurations)
        {
            _configurations = configurations?.ToDictionary(c => c.Name) ?? throw new ArgumentNullException(nameof(configurations));
        }

        public IEnumerable<SQSConfiguration> Configurations { get { return _configurations.Values; } }

        public ISQSConfiguration GetConfiguration(string name)
        {
            return _configurations[name];
        }

        public bool HasConfiguration(string name)
        {
            return _configurations.ContainsKey(name);
        }
    }
}
