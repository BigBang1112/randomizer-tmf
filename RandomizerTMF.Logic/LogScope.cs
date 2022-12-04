namespace RandomizerTMF.Logic;

public class LogScope : IDisposable
{
    private readonly LoggerToFile logger;

    public string Name { get; }
    public LogScope? Parent { get; }

    public LogScope(string name, LogScope? parent, LoggerToFile logger)
    {
        Name = name;
        Parent = parent;
        this.logger = logger;
    }

    public override string ToString()
    {
        return Name;
    }

    public void Dispose()
    {
        logger.CurrentScope = Parent;
    }
}
