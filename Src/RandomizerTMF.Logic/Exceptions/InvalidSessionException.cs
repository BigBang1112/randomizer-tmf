namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class InvalidSessionException : Exception
{
    public InvalidSessionException() { }
    public InvalidSessionException(string message) : base(message) { }
    public InvalidSessionException(string message, Exception inner) : base(message, inner) { }
    protected InvalidSessionException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
