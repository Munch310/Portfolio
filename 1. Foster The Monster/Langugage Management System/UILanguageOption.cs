using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class UILanguageOption : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown languageDropdown;

    void Start()
    {
        InitializeDropdown();
    }

    void InitializeDropdown()
    {
        languageDropdown.onValueChanged.AddListener(OnSelectionChanged);
        languageDropdown.ClearOptions();
        languageDropdown.options.Add(new TMP_Dropdown.OptionData("Loading..."));
        languageDropdown.interactable = false;

        var initializeOperation = LocalizationSettings.SelectedLocaleAsync;
        if (initializeOperation.IsDone)
        {
            PopulateLanguageOptions();
        }
        else
        {
            initializeOperation.Completed += (operation) => PopulateLanguageOptions();
        }
    }

    void PopulateLanguageOptions()
    {
        languageDropdown.ClearOptions();

        var options = new List<string>();
        int selectedOption = GetLocaleOptions(options);

        if (options.Count == 0)
        {
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("No Locales Available"));
            languageDropdown.interactable = false;
        }
        else
        {
            languageDropdown.AddOptions(options);
            languageDropdown.SetValueWithoutNotify(selectedOption);
            languageDropdown.interactable = true;
        }
    }

    int GetLocaleOptions(List<string> options)
    {
        var locales = LocalizationSettings.AvailableLocales.Locales;
        int selectedOption = 0;

        for (int i = 0; i < locales.Count; ++i)
        {
            var locale = locales[i];
            var displayName = locale.Identifier.CultureInfo.NativeName;
            options.Add(displayName);

            if (LocalizationSettings.SelectedLocale == locale)
                selectedOption = i;
        }

        return selectedOption;
    }

    void OnSelectionChanged(int index)
    {
        if (index >= 0 && index < LocalizationSettings.AvailableLocales.Locales.Count)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[index];
            LocalizationSettings.SelectedLocale = locale;

            PlayerPrefs.SetString("SelectedLanguage", locale.Identifier.Code);
            PlayerPrefs.Save();

            GlobalSettings.CurrentLocale = locale.Identifier.Code;

            int selectedIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(locale);
            languageDropdown.SetValueWithoutNotify(selectedIndex);
        }
    }
}
