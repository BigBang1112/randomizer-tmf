namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class ImportantPropertyNullException : Exception
{
    public ImportantPropertyNullException() { }
    public ImportantPropertyNullException(string message) : base(message) { }
    public ImportantPropertyNullException(string message, Exception inner) : base(message, inner) { }
}
