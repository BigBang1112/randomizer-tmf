namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class ConfigCorruptedException : Exception
{
    public ConfigCorruptedException() { }
    public ConfigCorruptedException(string message) : base(message) { }
    public ConfigCorruptedException(string message, Exception inner) : base(message, inner) { }
}