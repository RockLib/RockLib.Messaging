using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using RockLib.Configuration;

namespace RockLib.Messaging.Example.Core
{
    class Program
    {
        // using project reference for now, will change to nuget once we get Messaging Stable
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var messagingSection = Config.Root.GetSection("RockLib.Messaging").Get<ScenarioFactorySection>();

            var scenarioFactoriesCount = messagingSection.ScenarioFactories.Count;
            var messagingScenarioFactory = messagingSection.ScenarioFactories[0].CreateInstance();
            

            var queueProducer = MessagingScenarioFactory.CreateQueueProducer("");
        }
    }
    
    public class ScenarioFactorySection
    {
        public List<LateBoundConfigurationSection<IMessagingScenarioFactory>> ScenarioFactories { get; set; }
        
    }
}
