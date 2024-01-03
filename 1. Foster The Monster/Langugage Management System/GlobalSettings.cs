using System;

public static class GlobalSettings
{
    private static string _currentLocale;

    public static string CurrentLocale
    {
        get => _currentLocale;
        set
        {
            if (_currentLocale != value)
            {
                _currentLocale = value;
                OnLocaleChanged?.Invoke(value);
            }
        }
    }

    public static event Action<string> OnLocaleChanged;
}
