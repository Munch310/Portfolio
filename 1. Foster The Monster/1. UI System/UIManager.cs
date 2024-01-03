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
    // UI �⺻ ����
    private const string UiPath = "UI";
    private Dictionary<Type, string> uiDefaultPaths = new Dictionary<Type, string>();

    // UI ��� ����
    private Dictionary<string, GameObject> UIElements = new Dictionary<string, GameObject>();
    private Transform uiContents;
    public Transform UIContents => uiContents;
    private Stack<UIElementContainer> uiStack = new Stack<UIElementContainer>();

    // UI ���� ����
    public int CurrentSortingOrder { get; private set; } = 0;

    // Serialized Object ���� (Unity �ν����� ����)
    [Header("Scriptable Objects")]
    [SerializeField] private AssetReference PopupSO;
    [SerializeField] private AssetReference SinglePopupSO;

    protected override void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // UI ��Ҹ� �������� ���� �� ����
    private T CreateUI<T>(string name) where T : Component
    {
        // T Ÿ�Կ� ���� UI �������� ��θ� �����ϰ�, �ش� ����� �������� �ε��Ͽ� �ν��Ͻ�ȭ�մϴ�.
        string path = GetUIPath<T>();
        string resourcePath = $"{path}/{name}";
        GameObject prefab = ResourceManager.Instance.LoadPrefab<GameObject>(resourcePath, name);
        if (prefab == null)
        {
            return null;
        }

        // ���������κ��� UI �ν��Ͻ��� �����ϰ�, UIContents�� �ڽ����� �����մϴ�.
        GameObject uiInstance = Instantiate(prefab, UIContents);
        return uiInstance.GetComponent<T>();
    }

    // UI ��Ҹ� ���ų� Ȱ��ȭ
    public T OpenUI<T>() where T : Component
    {
        string prefabName = typeof(T).Name;

        // prefabName�� �ش��ϴ� UI ��Ұ� �̹� �����ϸ�, �ش� ��Ҹ� Ȱ��ȭ�ϰ� ���� ������ ������Ʈ�մϴ�.
        if (UIElements.TryGetValue(prefabName, out GameObject uiElement) && uiElement != null)
        {
            uiElement.SetActive(true);
            var canvas = uiElement.GetComponent<Canvas>();
            if (canvas != null)
            {
                // ĵ������ ���� ������ ������Ʈ�ϰ�, UI ������ �����մϴ�.
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
            // �������� �ʴ� UI ��Ҵ� ���� �����ϰ�, UIElements�� �߰��մϴ�.
            T newComponent = CreateUI<T>(prefabName);
            if (newComponent != null)
            {
                UIElements.Add(prefabName, newComponent.gameObject);
                newComponent.gameObject.SetActive(true);

                // ���� ������ UI�� ĵ���� ���� ������ �����մϴ�.
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
