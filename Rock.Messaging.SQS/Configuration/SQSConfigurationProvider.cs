using System;
using System.Collections.Generic;
using System.Linq;

namespace RockLib.Messaging.SQS
{
    public class SQSConfigurationProvider: ISQSConfigurationProvider
    {
        private readonly List<SQSConfiguration> _configurations;

        public SQSConfigurationProvider(List<SQSConfiguration> configuration)
        {
            _configurations = configuration;
        }

        public ISQSConfiguration GetConfiguration(string name)
        {
            return _configurations.First(c => c.Name == name);
        }

        public bool HasConfiguration(string name)
        {
            return _configurations.Any(c => c.Name == name);
        }

        public void Validate()
        {
            foreach (var configuration in _configurations)
            {
                configuration.Validate();
            }

            if (_configurations.Select(c => c.Name).Distinct().Count() != _configurations.Count)
            {
                throw new Exception("Each configuration Name must be unique.");
            }
        }
    }
}
