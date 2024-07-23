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
    private ulong _messagesBeingHandledCount;
    private readonly ManualResetEventSlim _zeroMessagesAreCurrentlyBeingHandled = new(initialState: true);
    private readonly ManualResetEventSlim _doneReceiving = new(initialState: false);

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
    public SQSConcurrentReceiver(IAmazonSQS sqs, string name, Uri queueUrl, int maxMessages = DefaultMaxMessages, bool autoAcknowledge = true, int waitTimeSeconds = DefaultWaitTimeSeconds, bool unpackSNS = false, bool terminateMessageVisibilityTimeoutOnRollback = false)
        : base(sqs, name, queueUrl, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS, terminateMessageVisibilityTimeoutOnRollback)
    {
    }

    /// <inheritdoc />
    public SQSConcurrentReceiver(IAmazonSQS sqs, string name, Uri queueUrl, int maxMessages, bool autoAcknowledge, int waitTimeSeconds, bool unpackSNS)
        : base(sqs, name, queueUrl, maxMessages, autoAcknowledge, waitTimeSeconds, unpackSNS)
    {
    }

    /// <inheritdoc />
    protected override Task ProcessMessagesAsync(IEnumerable<Message> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        // Process each message without waiting for them to finish
        // (but wait for all messages to finish while disposing)
        foreach (var message in messages)
        {
            _ = HandleMessageAsync();

            async Task HandleMessageAsync()
            {
                // If any messages are being processed, set the event so that Dispose will wait
                if (Interlocked.Increment(ref _messagesBeingHandledCount) == 1)
                {
                    _zeroMessagesAreCurrentlyBeingHandled.Reset();
                }

                try
                {
                    await HandleAsync(message).ConfigureAwait(false);
                }
                finally
                {
                    // If no messages are being processed, reset the event so that Dispose will not wait
                    if (Interlocked.Decrement(ref _messagesBeingHandledCount) == 0)
                    {
                        _zeroMessagesAreCurrentlyBeingHandled.Set();
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

            // Wait for the `ReceiveMessages` loop to finish, indicating that no new messages will begin to be handled
            _doneReceiving.Wait();
            _doneReceiving.Dispose();

            // Wait for all messages to finish being handled
            _zeroMessagesAreCurrentlyBeingHandled.Wait();
            _zeroMessagesAreCurrentlyBeingHandled.Dispose();

        }
        base.Dispose(disposing);
    }
}
