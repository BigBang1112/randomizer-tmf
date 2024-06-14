namespace RandomizerTMF.Logic.Exceptions;

[Serializable]
public class AutosaveScannerException : Exception
{
    public AutosaveScannerException() { }
    public AutosaveScannerException(string message) : base(message) { }
    public AutosaveScannerException(string message, Exception inner) : base(message, inner) { }
}
