using Microsoft.Extensions.Logging;

namespace RandomizerTMF.Logic;

public class LogMessage
{
    public LogLevel LogLevel { get; }
    public string Message { get; }
    public LogScope? Scope { get; }
    public Exception? Exception { get; }
    public DateTime Timestamp { get; }
    
    public LogMessage(LogLevel logLevel, string message, LogScope? scope, Exception? exception, DateTime timestamp)
    {
        LogLevel = logLevel;
        Message = message;
        Scope = scope;
        Exception = exception;
        Timestamp = timestamp;
    }
}
