using System.Collections.Generic;
using RockLib.Configuration;

namespace RockLib.Messaging.Configuration
{
    public class ScenarioFactorySection
    {
        public List<LateBoundConfigurationSection<IMessagingScenarioFactory>> ScenarioFactories { get; set; }

    }
}