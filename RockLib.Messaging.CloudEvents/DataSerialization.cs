namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines types of serialization to use when getting or setting the data of a <see cref=
    /// "CloudEvent"/> as a generic type.
    /// </summary>
    public enum DataSerialization
    {
        /// <summary>
        /// JSON serialization.
        /// </summary>
        Json,

        /// <summary>
        /// XML serialization.
        /// </summary>
        Xml
    }
}
