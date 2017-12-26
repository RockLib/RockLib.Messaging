#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    /// <summary>
    /// Defineds methods for looking up instances of <see cref="ISQSConfiguration"/> by name.
    /// </summary>
    public interface ISQSConfigurationProvider
    {
        /// <summary>
        /// Returns an instance of <see cref="ISQSConfiguration"/> for the provided name.
        /// </summary>
        /// <param name="name">The name of the <see cref="ISQSConfiguration"/> to retrieve.</param>
        /// <returns>The <see cref="ISQSConfiguration"/> with the provided name.</returns>
        ISQSConfiguration GetConfiguration(string name);

        /// <summary>
        /// Returns a value indicating whether this instance of <see cref="ISQSConfigurationProvider"/>
        /// can retrieve an instacne of <see cref="ISQSConfiguration"/> by the provided name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True, if a configuration can be retrieved, otherwise false.</returns>
        bool HasConfiguration(string name);
    }
}