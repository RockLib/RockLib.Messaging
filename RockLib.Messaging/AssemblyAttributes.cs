using RockLib.Configuration.ObjectFactory;
using RockLib.Messaging;
using System.Collections.Generic;

[assembly: ConfigSection("RockLib.Messaging:Senders", typeof(List<ISender>))]
[assembly: ConfigSection("RockLib.Messaging:Receivers", typeof(List<IReceiver>))]
