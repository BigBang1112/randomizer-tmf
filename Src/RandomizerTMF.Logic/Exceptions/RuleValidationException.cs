namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class RuleValidationException : Exception
{
    public RuleValidationException() { }
    public RuleValidationException(string message) : base(message) { }
    public RuleValidationException(string message, Exception inner) : base(message, inner) { }
    protected RuleValidationException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
