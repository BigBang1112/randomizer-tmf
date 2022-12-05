namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class MapValidationException : Exception
{
    public MapValidationException() { }
    public MapValidationException(string message) : base(message) { }
    public MapValidationException(string message, Exception inner) : base(message, inner) { }
    protected MapValidationException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
