using UnityEngine;

public class Dot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
