namespace Rock.Messaging.RabbitMQ
{
    public interface IRabbitSessionConfiguration
    {
        /// <summary>
        /// Colloquial name of the queue. Use this to fetch yonder queue.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Where the message is going. Form: HostName:PortNum
        /// </summary>
        string QueueUrl { get; set; }
        string vHost { get; set; }
        string RoutingKey { get; set; }
        /// <summary>
        /// User name for the queue.
        /// </summary>
        string UserName { get; set; }
        /// <summary>
        /// Password for the queue.
        /// </summary>
        string Password { get; set; }
        
        string QueueName { get; set; }
        string Exchange { get; set; }
        string ExchangeType { get; set; }

        bool AutoAcknowledge { get; set; }

        ushort MaxRequests { get; set; }
       
        //TODO: Add Validate()?
    }
}