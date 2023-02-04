namespace WinUICommunity.Common.Tools;

public class LocalizerException : Exception
{
    public LocalizerException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}