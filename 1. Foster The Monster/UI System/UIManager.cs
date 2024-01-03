using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Enums;

public class UIManager : Singleton<UIManager>
{
    #region Color
    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");
        byte a = 255;

        byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

        if(hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
        }

        return new Color32(r, g, b, a);
    }
    #endregion
    #region Path
    private const string UiPath = "UI";
    private Dictionary<Type, string> uiDefaultPaths = new Dictionary<Type, string>();
    #endregion

    #region HoverEffect
    private Dictionary<Button, TMP_Text> buttonToTextMapping = new Dictionary<Button, TMP_Text>();
    private Dictionary<Button, Color> originalButtonColors = new Dictionary<Button, Color>();
    #endregion

    private Dictionary<string, GameObject> UIElements = new Dictionary<string, GameObject>();
    private Transform uiContents;
    public Transform UIContents => uiContents;
    private Stack<UIElementContainer> uiStack = new Stack<UIElementContainer>();
    public int CurrentSortingOrder { get; private set; } = 0;

    [Header("SO")]
    [SerializeField] private AssetReference PopupSO;
    [SerializeField] private AssetReference SinglePopupSO;

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void ResetUIManager()
    {
        foreach (var uiElement in UIElements.Values)
        {
            if (uiElement != null)
            {
                Destroy(uiElement);
            }
        }
        UIElements.Clear();
        uiStack.Clear();
    }

    private string GetUIPath<T>() where T : Component
    {
        if(uiDefaultPaths.TryGetValue(typeof(T), out var customPath))
        {
            return customPath;
        }
        return UiPath;
    }

    private T CreateUI<T>(string name) where T : Component
    {
        string path = GetUIPath<T>();
        string resourcePath = $"{path}/{name}";
        GameObject prefab = ResourceManager.Instance.LoadPrefab<GameObject>(resourcePath, name);
        if (prefab == null)
        {
            return null;
        }

        GameObject uiInstance = Instantiate(prefab, UIContents);
        return uiInstance.GetComponent<T>();
    }

    public T OpenUI<T>() where T : Component
    {
        string prefabName = typeof(T).Name;

        if (UIElements.TryGetValue(prefabName, out GameObject uiElement) && uiElement != null)
        {
            uiElement.SetActive(true);
            var canvas = uiElement.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = ++CurrentSortingOrder;
                if (uiStack.Count > 0 && uiStack.Peek().GameObject == uiElement)
                {
                    uiStack.Pop();
                }
                uiStack.Push(new UIElementContainer(uiElement, CurrentSortingOrder));
            }
            return uiElement.GetComponent<T>();
        }
        else
        {
            T newComponent = CreateUI<T>(prefabName);
            if (newComponent != null)
            {
                UIElements.Add(prefabName, newComponent.gameObject);
                newComponent.gameObject.SetActive(true);

                var canvas = newComponent.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = ++CurrentSortingOrder;
                    uiStack.Push(new UIElementContainer(newComponent.gameObject, CurrentSortingOrder));
                }
            }

            return newComponent;
        }
    }

    public void CloseUI<T>() where T : Component
    {
        string prefabName = typeof(T).Name;
        if (UIElements.TryGetValue(prefabName, out GameObject uiElement))
        {
            uiElement.SetActive(false);
            if (uiStack.Count > 0 && uiStack.Peek().GameObject == uiElement)
            {
                uiStack.Pop();
            }
        }
    }

    public void ClearUI<T>() where T : Component
    {
        string prefabName = typeof(T).Name;
        if (UIElements.TryGetValue(prefabName, out GameObject uiElement))
        {
            Destroy(uiElement);
            UIElements.Remove(prefabName);
        }
    }

    public void ClearUIManager()
    {
        uiStack.Clear();
    }

    #region Popup
    public async void OpenConfirmationPopup(int popupKey, Action leftButtonAction, Action rightButtonAction)
    {

        PopupSO foundPopupDataSO = await ResourceManager.Instance.LoadResource<PopupSO>(PopupSO);
        if (foundPopupDataSO != null)
        {
            foreach (var popupData in foundPopupDataSO.popups)
            {
                if (popupData.popupKey == popupKey)
                {
                    ConfirmationPopup popup = OpenUI<ConfirmationPopup>();
                    popup.SetTitle(popupData.title);
                    popup.SetMessage(popupData.message);
                    popup.SetLeftButtonText(popupData.leftButtonText);
                    popup.SetRightButtonText(popupData.rightButtonText);
                    popup.SetLeftButtonAction(leftButtonAction);
                    popup.SetRightButtonAction(rightButtonAction);
                    break;
                }
            }
        }
    }

    public async void OpenSingleConfirmationPopup(int popupKey, Action buttonAction)
    {
        SinglePopupSO foundSinglePopupDataSO = await ResourceManager.Instance.LoadResource<SinglePopupSO>(SinglePopupSO);
        if (foundSinglePopupDataSO != null)
        {
            foreach(var singlePopupData in foundSinglePopupDataSO.popups)
            {
                if(singlePopupData.popupKey == popupKey)
                {
                    SingleConfirmationPopup popup = OpenUI<SingleConfirmationPopup>();
                    popup.SetTitle(singlePopupData.title);
                    popup.SetMessage(singlePopupData.message);
                    popup.SetButtonText(singlePopupData.buttonText);
                    popup.SetButtonAction(buttonAction);
                    break;
                }
            }
        }
    }
    #endregion

    #region Hover
    public void InitializeButtonHoverEffect(Button button, TMP_Text btnText, string hexColor)
    {
        Color hoverColor = HexToColor(hexColor);
        if (!buttonToTextMapping.ContainsKey(button))
        {
            buttonToTextMapping.Add(button, btnText);
            originalButtonColors.Add(button, button.image.color);
        }

        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((e) => OnHoverEnter(button, hoverColor));
        trigger.triggers.Add(pointerEnter);

        var pointerExit = new EventTrigger.Entry { eventID= EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((e) => OnHoverExit(button));
        trigger.triggers.Add(pointerExit);
    }

    private void OnHoverEnter(Button button, Color hoverColor)
    {
        button.image.color = hoverColor;
        if(buttonToTextMapping.TryGetValue(button, out TMP_Text text))
        {
            text.gameObject.SetActive(true);
        }
        AudioManager.Instance.PlaySFX(SFXCategory.Button, SFXClips.Hover);
    }

    private void OnHoverExit(Button button)
    {
        if(originalButtonColors.TryGetValue(button, out Color originColor))
        {
            button.image.color = originColor;
        }
        
        if(buttonToTextMapping.TryGetValue(button, out TMP_Text text))
        {
            text.gameObject.SetActive(false);
        }
    }
    #endregion

    #region NoTextHover
    public void NoTextInitializeButtonHoverEffect(Button button, string hexColor)
    {
        Color hoverColor = HexToColor(hexColor);
        if (!originalButtonColors.ContainsKey(button))
        {
            originalButtonColors.Add(button, button.image.color);
        }

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>() ?? button.gameObject.AddComponent<EventTrigger>();

        var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((e) => OnHoverEnterNoText(button, hoverColor));
        trigger.triggers.Add(pointerEnter);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((e) => OnHoverExitNoText(button));
        trigger.triggers.Add(pointerExit);
    }

    private void OnHoverEnterNoText(Button button, Color hoverColor)
    {
        button.image.color = hoverColor;
        AudioManager.Instance.PlaySFX(SFXCategory.Button, SFXClips.Hover);
    }

    private void OnHoverExitNoText(Button button)
    {
        if (originalButtonColors.TryGetValue(button, out Color originColor))
        {
            button.image.color = originColor;
        }
    }

    #endregion
}
