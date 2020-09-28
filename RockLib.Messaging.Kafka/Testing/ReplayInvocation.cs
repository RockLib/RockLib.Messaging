using System;
using System.Threading.Tasks;

namespace RockLib.Messaging.Testing.Kafka
{
    /// <summary>
    /// Represents an invocation of the <see cref="FakeKafkaReceiver.ReplayAsync"/> method.
    /// </summary>
    public struct ReplayInvocation : IEquatable<ReplayInvocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayInvocation"/> struct.
        /// </summary>
        /// <param name="start">
        /// The value of the 'start' parameter when the <see cref="FakeKafkaReceiver.ReplayAsync"/>
        /// method was invoked.
        /// </param>
        /// <param name="end">
        /// The value of the 'end' parameter when the <see cref="FakeKafkaReceiver.ReplayAsync"/>
        /// method was invoked.
        /// </param>
        /// <param name="callback">
        /// The value of the 'callback' parameter when the <see cref="FakeKafkaReceiver.ReplayAsync"/>
        /// method was invoked.
        /// </param>
        /// <param name="pauseDuringReplay">
        /// Whether to pause the consumer while replaying, then resume after replaying is finished.
        /// </param>
        public ReplayInvocation(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback, bool pauseDuringReplay)
        {
            Start = start;
            End = end;
            Callback = callback;
            PauseDuringReplay = pauseDuringReplay;
        }

        /// <summary>
        /// The value of the 'start' parameter when the <see cref="FakeKafkaReceiver.ReplayAsync"/>
        /// method was invoked.
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// The value of the 'end' parameter when the <see cref="FakeKafkaReceiver.ReplayAsync"/>
        /// method was invoked.
        /// </summary>
        public DateTime? End { get; }

        /// <summary>
        /// The value of the 'callback' parameter when the <see cref="FakeKafkaReceiver.ReplayAsync"/>
        /// method was invoked.
        /// </summary>
        public Func<IReceiverMessage, Task> Callback { get; }

        /// <summary>
        /// Whether to pause the consumer while replaying, then resume after replaying is finished.
        /// </summary>
        public bool PauseDuringReplay { get; }

        /// <summary>
        /// Deconstructs the <see cref="ReplayInvocation"/>.
        /// </summary>
        /// <param name="start">
        /// When this method returns, the value of the <see cref="Start"/> property.
        /// </param>
        /// <param name="end">
        /// When this method returns, the value of the <see cref="End"/> property.
        /// </param>
        /// <param name="callback">
        /// When this method returns, the value of the <see cref="Callback"/> property.
        /// </param>
        /// <param name="pauseDuringReplay">
        /// When this method returns, the value of the <see cref="PauseDuringReplay"/> property.
        /// </param>
        public void Deconstruct(out DateTime start, out DateTime? end, out Func<IReceiverMessage, Task> callback, out bool pauseDuringReplay)
        {
            start = Start;
            end = End;
            callback = Callback;
            pauseDuringReplay = PauseDuringReplay;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) =>
            obj is ReplayInvocation invocation && Equals(invocation);

        /// <inheritdoc/>
        public bool Equals(ReplayInvocation other) =>
            Start == other.Start
                && End == other.End
                && ReferenceEquals(Callback, other.Callback);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 544682223;
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            hashCode = hashCode * -1521134295 + Callback?.GetHashCode() ?? 0;
            return hashCode;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="ReplayInvocation"/> are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// represent the same invocation of the <see cref="FakeKafkaReceiver.ReplayAsync"/> method;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(ReplayInvocation left, ReplayInvocation right) =>
            left.Equals(right);

        /// <summary>
        /// Determines whether two specified instances of <see cref="ReplayInvocation"/> are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> do not
        /// represent the same invocation of the <see cref="FakeKafkaReceiver.ReplayAsync"/> method;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(ReplayInvocation left, ReplayInvocation right) =>
            !(left == right);
    }
}
