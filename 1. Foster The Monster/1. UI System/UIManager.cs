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
    // UI 기본 설정
    private const string UiPath = "UI";
    private Dictionary<Type, string> uiDefaultPaths = new Dictionary<Type, string>();

    // UI 요소 관리
    private Dictionary<string, GameObject> UIElements = new Dictionary<string, GameObject>();
    private Transform uiContents;
    public Transform UIContents => uiContents;
    private Stack<UIElementContainer> uiStack = new Stack<UIElementContainer>();

    // UI 정렬 순서
    public int CurrentSortingOrder { get; private set; } = 0;

    // Serialized Object 참조 (Unity 인스펙터 설정)
    [Header("Scriptable Objects")]
    [SerializeField] private AssetReference PopupSO;
    [SerializeField] private AssetReference SinglePopupSO;

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // UI 요소를 동적으로 생성 및 관리
    private T CreateUI<T>(string name) where T : Component
    {
        // T 타입에 따라 UI 프리팹의 경로를 결정하고, 해당 경로의 프리팹을 로드하여 인스턴스화합니다.
        string path = GetUIPath<T>();
        string resourcePath = $"{path}/{name}";
        GameObject prefab = ResourceManager.Instance.LoadPrefab<GameObject>(resourcePath, name);
        if (prefab == null)
        {
            return null;
        }

        // 프리팹으로부터 UI 인스턴스를 생성하고, UIContents의 자식으로 설정합니다.
        GameObject uiInstance = Instantiate(prefab, UIContents);
        return uiInstance.GetComponent<T>();
    }

    // UI 요소를 열거나 활성화
    public T OpenUI<T>() where T : Component
    {
        string prefabName = typeof(T).Name;

        // prefabName에 해당하는 UI 요소가 이미 존재하면, 해당 요소를 활성화하고 정렬 순서를 업데이트합니다.
        if (UIElements.TryGetValue(prefabName, out GameObject uiElement) && uiElement != null)
        {
            uiElement.SetActive(true);
            var canvas = uiElement.GetComponent<Canvas>();
            if (canvas != null)
            {
                // 캔버스의 정렬 순서를 업데이트하고, UI 스택을 관리합니다.
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
            // 존재하지 않는 UI 요소는 새로 생성하고, UIElements에 추가합니다.
            T newComponent = CreateUI<T>(prefabName);
            if (newComponent != null)
            {
                UIElements.Add(prefabName, newComponent.gameObject);
                newComponent.gameObject.SetActive(true);

                // 새로 생성된 UI의 캔버스 정렬 순서를 설정합니다.
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
}
