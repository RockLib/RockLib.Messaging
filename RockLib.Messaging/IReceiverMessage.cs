using System;

namespace RockLib.Messaging
{
    /// <summary>
    /// Defines the interface for a received message.
    /// </summary>
    public interface IReceiverMessage
    {
        /// <summary>
        /// Gets the payload of the message as a string.
        /// </summary>
        string StringPayload { get; }

        /// <summary>
        /// Gets the payload of the message as a byte array.
        /// </summary>
        byte[] BinaryPayload { get; }

        /// <summary>
        /// Gets the headers of the message.
        /// </summary>
        HeaderDictionary Headers { get; }

        /// <summary>
        /// Gets a value indicating whether this message has been handled by one of the
        /// <see cref="Acknowledge"/>, <see cref="Rollback"/> or <see cref="Reject"/>
        /// methods.
        /// </summary>
        bool Handled { get; }

        /// <summary>
        /// Indicates that the message was successfully processed and should not
        /// be redelivered.
        /// </summary>
        /// <remarks>
        /// Notes for implementators of this method:
        /// <para>When called, this method should immediately throw if the <see cref="Handled"/>
        /// property is true.</para>
        /// <para>After successfully calling this method, the <see cref="Handled"/> property
        /// should return true, preventing the message from being handled multiple times.</para>
        /// <para>If the concept of acknowledging a message doesn't make sense for an implementation,
        /// it should not throw a <see cref="NotImplementedException"/> or similar exception.</para>
        /// </remarks>
        void Acknowledge();

        /// <summary>
        /// Indicates that the message was not successfully processed but should be
        /// (or should be allowed to be) redelivered.
        /// </summary>
        /// <remarks>
        /// Notes for implementators of this method:
        /// <para>When called, this method should immediately throw if the <see cref="Handled"/>
        /// property is true.</para>
        /// <para>After successfully calling this method, the <see cref="Handled"/> property
        /// should return true, preventing the message from being handled multiple times.</para>
        /// <para>If the concept of rolling back a message doesn't make sense for an implementation,
        /// it should not throw a <see cref="NotImplementedException"/> or similar exception.</para>
        /// </remarks>
        void Rollback();

        /// <summary>
        /// Indicates that the message could not be successfully processed and should
        /// not be redelivered.
        /// </summary>
        /// <remarks>
        /// Notes for implementators of this method:
        /// <para>When called, this method should immediately throw if the <see cref="Handled"/>
        /// property is true.</para>
        /// <para>After successfully calling this method, the <see cref="Handled"/> property
        /// should return true, preventing the message from being handled multiple times.</para>
        /// <para>If the concept of rejecting a message doesn't make sense for an implementation,
        /// it should not throw a <see cref="NotImplementedException"/> or similar exception.</para>
        /// </remarks>
        void Reject();
    }
}