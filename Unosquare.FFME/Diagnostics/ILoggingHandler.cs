namespace Unosquare.FFME.Diagnostics
{
    /// <summary>
    /// Defines interface methods for logging message handlers.
    /// </summary>
    public interface ILoggingHandler
    {
        /// <summary>
        /// Handles a log message.
        /// </summary>
        /// <param name="message">The message object containing the data.</param>
        void HandleLogMessage(LoggingMessage message);
    }
}
