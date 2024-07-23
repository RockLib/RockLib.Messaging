using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RockLib.Messaging.SQS;

/// <summary>
/// An implementation of <see cref="IReceiver"/> that processes messages from SQS in parallel.
/// </summary>
public class SQSConcurrentReceiver : SQSReceiver
{
    private ulong _messagesBeingProcessedCount;
    private readonly ManualResetEventSlim _noMessagesBeingProcessed = new(initialState:true);
    private readonly ManualResetEventSlim _doneReceiving = new(initialState:false);

    /// <inheritdoc />
    public SQSConcurrentReceiver(string name, Uri queueUrl, string? region = null, int maxMessages = DefaultMaxMessages, bool autoAcknowledge = true, int waitTimeSeconds = DefaultWaitTimeSeconds, bool unpackSNS = false, bool terminateMessageVisibilityTimeoutOnRollback = false)
        : base(name, queueUrl, region, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS, terminateMessageVisibilityTimeoutOnRollback)
    {
    }

    /// <inheritdoc />
    public SQSConcurrentReceiver(string name, Uri queueUrl, string region, int maxMessages, bool autoAcknowledge, int waitTimeSeconds, bool unpackSNS)
        : base(name, queueUrl, region, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS)
    {
    }

    /// <inheritdoc />
    public SQSConcurrentReceiver(IAmazonSQS sqs, string name, Uri queueUrl, int maxMessages = DefaultMaxMessages, bool autoAcknowledge = true, int waitTimeSeconds = DefaultWaitTimeSeconds, bool unpackSNS = false, bool terminateMessageVisibilityTimeoutOnRollback = false) : base(sqs, name, queueUrl, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS, terminateMessageVisibilityTimeoutOnRollback)
    {
    }

    /// <inheritdoc />
    public SQSConcurrentReceiver(IAmazonSQS sqs, string name, Uri queueUrl, int maxMessages, bool autoAcknowledge, int waitTimeSeconds, bool unpackSNS) : base(sqs, name, queueUrl, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS)
    {
    }

    /// <inheritdoc />
    protected override Task ProcessMessagesAsync(IEnumerable<Message> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        foreach (var message in messages)
        {
            _ = HandleMessageAsync();

            async Task HandleMessageAsync()
            {
                if (Interlocked.Increment(ref _messagesBeingProcessedCount) == 1)
                {
                    _noMessagesBeingProcessed.Reset();
                }

                try
                {
                    await HandleAsync(message).ConfigureAwait(false);
                }
                finally
                {
                    if (Interlocked.Decrement(ref _messagesBeingProcessedCount) == 0)
                    {
                        _noMessagesBeingProcessed.Set();
                    }
                }
            }
        };
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override void OnDoneReceiving()
    {
        _doneReceiving.Set();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            OnStopRequested();
            _doneReceiving.Wait();
            _noMessagesBeingProcessed.Wait();

            _doneReceiving.Dispose();
            _noMessagesBeingProcessed.Dispose();
        }
        base.Dispose(disposing);
    }
}
