using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/TowerData", fileName = "Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Dice")]
    public string towerName = "Tower";
    public float towerAtkDamage = 5f;
    public float towerAtkSpeed = 1f;
    public Sprite sprite;

    [Header("Dot")]
    public Color dotColor = Color.white;
}