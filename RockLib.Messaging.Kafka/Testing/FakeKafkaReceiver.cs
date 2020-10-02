using RockLib.Messaging.Kafka;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RockLib.Messaging.Testing.Kafka
{
    /// <summary>
    /// Defines an implementation of <see cref="IReceiver"/> that is meant to be used when testing
    /// objects that use a <see cref="KafkaReceiver"/> in "real" or "production" scenarios.
    /// </summary>
    public class FakeKafkaReceiver : Receiver, IKafkaReceiver
    {
        private readonly List<DateTime> _seekInvocations = new List<DateTime>();
        private readonly List<ReplayInvocation> _replayInvocations = new List<ReplayInvocation>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeKafkaReceiver"/> class.
        /// </summary>
        /// <param name="name">The name of the receiver.</param>
        public FakeKafkaReceiver(string name = nameof(FakeKafkaReceiver))
            : base(name)
        {
        }

        /// <summary>
        /// The timestamp of the stream at which to start listening.
        /// </summary>
        public DateTime? StartTimestamp { get; set; }

        /// <summary>
        /// The list of the parameters that were passed to the <see cref="Seek"/> method.
        /// </summary>
        public IReadOnlyList<DateTime> SeekInvocations => _seekInvocations;

        /// <summary>
        /// The list of the parameters that were passed to the <see cref="ReplayAsync"/> method.
        /// </summary>
        public IReadOnlyList<ReplayInvocation> ReplayInvocations => _replayInvocations;

        /// <summary>
        /// The number of times the <see cref="Pause"/> method has been called.
        /// </summary>
        public int PauseInvocations { get; private set; }

        /// <summary>
        /// The number of times the <see cref="Resume"/> method has been called.
        /// </summary>
        public int ResumeInvocations { get; private set; }

        /// <summary>
        /// Adds the <paramref name="timestamp"/> to the <see cref="SeekInvocations"/> list.
        /// </summary>
        /// <param name="timestamp">The timestamp to seek to.</param>
        public void Seek(DateTime timestamp)
        {
            _seekInvocations.Add(timestamp);
        }

        /// <summary>
        /// Add a <see cref="ReplayInvocation"/> to the <see cref="ReplayInvocations"/> list.
        /// </summary>
        /// <param name="start">The start time.</param>
        /// <param name="end">
        /// The end time, or <see langword="null"/> to indicate that the the current UTC time
        /// should be used.
        /// </param>
        /// <param name="callback">
        /// The delegate to invoke for each replayed message, or <see langword="null"/> to indicate
        /// that <see cref="Receiver.MessageHandler"/> should handle replayed messages.
        /// </param>
        /// <param name="pauseDuringReplay">
        /// Whether to pause the consumer while replaying, then resume after replaying is finished.
        /// </param>
        public Task ReplayAsync(DateTime start, DateTime? end, Func<IReceiverMessage, Task> callback = null, bool pauseDuringReplay = false)
        {
            _replayInvocations.Add(new ReplayInvocation(start, end, callback, pauseDuringReplay));
            return Tasks.CompletedTask;
        }

        /// <summary>
        /// Increments <see cref="PauseInvocations"/>.
        /// </summary>
        public void Pause()
        {
            PauseInvocations++;
        }

        /// <summary>
        /// Increments <see cref="ResumeInvocations"/>.
        /// </summary>
        public void Resume()
        {
            ResumeInvocations++;
        }

        /// <summary>
        /// Resets the invocations of the receiver.
        /// </summary>
        public void Reset()
        {
            _seekInvocations.Clear();
            _replayInvocations.Clear();
            PauseInvocations = 0;
            ResumeInvocations = 0;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        protected override void Start()
        {
        }
    }
}
