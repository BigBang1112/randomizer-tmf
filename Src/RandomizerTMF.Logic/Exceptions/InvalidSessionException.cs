namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class InvalidSessionException : Exception
{
    public InvalidSessionException() { }
    public InvalidSessionException(string message) : base(message) { }
    public InvalidSessionException(string message, Exception inner) : base(message, inner) { }
}
