﻿namespace WinUICommunity.Common.Tools;
public class LanguageChangedEventArgs : EventArgs
{
    public LanguageChangedEventArgs(string language)
    {
        Language = language;
    }

    public string Language { get; }
}