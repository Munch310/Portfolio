public static class GlobalSettings
{
    private static string _currentLocale;

    // 현재 애플리케이션의 로케일을 가져오거나 설정
    public static string CurrentLocale
    {
        get => _currentLocale;
        set
        {
            // 로케일이 변경되었을 때만 이벤트를 발생
            if (_currentLocale != value)
            {
                _currentLocale = value;
                OnLocaleChanged?.Invoke(value); // 로케일 변경 이벤트 발생
            }
        }
    }

    // 로케일이 변경될 때 발생하는 이벤트
    public static event Action<string> OnLocaleChanged;
}
