using System;
using System.Collections.Generic;
using System.Text;

namespace RockLib.Messaging
{
    /// <summary>
    /// Provides data for the <see cref="IReceiver.Disconnected"/> event.
    /// </summary>
    public class DisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedEventArgs"/> class.
        /// </summary>
        /// <param name="errorMessage">The error message that describes the reason for the disconnection.</param>
        public DisconnectedEventArgs(string errorMessage) => ErrorMessage = errorMessage;

        /// <summary>
        /// Gets the error message that describes the reason for the disconnection.
        /// </summary>
        public string ErrorMessage { get; }
    }
}
