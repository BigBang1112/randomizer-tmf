namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class RuleValidationException : Exception
{
    public RuleValidationException() { }
    public RuleValidationException(string message) : base(message) { }
    public RuleValidationException(string message, Exception inner) : base(message, inner) { }
}
