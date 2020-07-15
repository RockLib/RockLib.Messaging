namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines values for the <see cref="SequentialEvent.SequenceType"/> attribute.
    /// </summary>
    public static class SequenceTypes
    {
        /// <summary>
        /// If the <see cref="SequentialEvent.SequenceType"/> attribute is set to <c>"Integer"</c>,
        /// the <see cref="SequentialEvent.Sequence"/> attribute has the following semantics:
        /// <list type="bullet">
        /// <item>The values of sequence are string-encoded signed 32-bit Integers.</item>
        /// <item>The sequence MUST start with a value of 1 and increase by 1 for each subsequent value
        /// (i.e. be contiguous and monotonically increasing).</item>
        /// <item>The sequence wraps around from 2,147,483,647 (2^31 - 1) to -2,147,483,648 (-2^31).</item>
        /// </list>
        /// </summary>
        public const string Integer = "Integer";
    }
}
