using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace RandomizerTMF.Logic;

public class LoggerToFile : ILogger
{
    internal LogScope? CurrentScope { get; set; }

    public ImmutableArray<StreamWriter> Writers { get; private set; }

    public LoggerToFile(StreamWriter writer)
    {
        Writers = ImmutableArray.Create(writer);
    }

    public void SetSessionWriter(StreamWriter writer)
    {
        Writers = ImmutableArray.Create(Writers[0], writer);
    }

    public void RemoveSessionWriter()
    {
        Writers = ImmutableArray.Create(Writers[0]);
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        if (state is string stateStr)
        {
            return CurrentScope = new LogScope(stateStr, CurrentScope, this);
        }

        if (state is not IReadOnlyList<KeyValuePair<string, object>> logValues)
        {
            return new NullDisposable();
        }

        var str = logValues.FirstOrDefault(x => x.Key == "{OriginalFormat}").Value.ToString() ?? "";

        foreach (var (key, val) in logValues)
        {
            if (key == "{OriginalFormat}")
            {
                continue;
            }

            str = str.Replace($"{{{key}}}", val.ToString());
        }

        return CurrentScope = new LogScope(str, CurrentScope, this);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);

        // CurrentScope is not utilized

        foreach (var writer in Writers)
        {
            try
            {
                writer.WriteLine($"[{DateTime.Now}, {logLevel}] {message}");

                if (exception is not null)
                {
                    writer.WriteLine(exception);
                }
            }
            catch
            {
                // usually writer of ended session
            }
        }
    }
    
    internal class NullDisposable : IDisposable
    {
        public void Dispose()
        {

        }
    }
}
