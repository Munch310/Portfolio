public static class GlobalSettings
{
    private static string _currentLocale;

    // ���� ���ø����̼��� �������� �������ų� ����
    public static string CurrentLocale
    {
        get => _currentLocale;
        set
        {
            // �������� ����Ǿ��� ���� �̺�Ʈ�� �߻�
            if (_currentLocale != value)
            {
                _currentLocale = value;
                OnLocaleChanged?.Invoke(value); // ������ ���� �̺�Ʈ �߻�
            }
        }
    }

    // �������� ����� �� �߻��ϴ� �̺�Ʈ
    public static event Action<string> OnLocaleChanged;
}
