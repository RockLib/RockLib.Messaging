using System;
using System.Linq;

namespace RockLib.Messaging.SQS.Configuration
{
    public class SQSConfigurationProvider: ISQSConfigurationProvider
    {
        private ISQSConfiguration[] _configurations = new ISQSConfiguration[0];
        
        public ISQSConfiguration[] Configurations
        {
            get => _configurations;
            set => _configurations = value ?? throw new ArgumentNullException(nameof(value));
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

            if (_configurations.Select(c => c.Name).Distinct().Count() != _configurations.Length)
            {
                throw new Exception("Each configuration Name must be unique.");
            }
        }
    }
}
