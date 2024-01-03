using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    public static TowerManager instance;
    public List<Tower> spawnedTowers = new List<Tower>();
    public Dictionary<string, int> levelTowers = new Dictionary<string, int>();

    public Transform towerSpawnPosition;

    private void Awake()
    {
        instance = this;
    }

    public void MergeTowers(Tower tower1, Tower tower2)
    {
        if (tower1.transform.parent == null)
            return;

        Transform parentTransform = tower2.transform.parent;
        Vector3 newTowerPosition = parentTransform.position;

        Destroy(tower1.gameObject);
        Destroy(tower2.gameObject);
        RemoveSpawnedTower(tower1);
        RemoveSpawnedTower(tower2);

        int randomIndex = UnityEngine.Random.Range(0, TowerSpawnManager.instance.towerPrefabs.Length);
        GameObject newTowerObject = Instantiate(TowerSpawnManager.instance.towerPrefabs[randomIndex], newTowerPosition, Quaternion.identity, parentTransform);

        AudioManager.instance.PlaySfx(AudioManager.Sfx.TowerMerge);

        Tower newTower = newTowerObject.GetComponent<Tower>();

        if (newTower != null)
        {
            TowerData selectedTowerData = newTowerObject.GetComponent<Tower>().towerData;
            newTower.towerData = selectedTowerData;
            AddSpawnedTower(newTower);

            int upgradesNeeded = Math.Max(tower1.level, tower2.level);

            for (int i = 0; i < upgradesNeeded && i < 6; i++)
                newTower.UpgradeTower();
        }
    }

    public void AddSpawnedTower(Tower tower)
    {
        spawnedTowers.Add(tower);
        UpgradeTowerAtk();
    }

    public void RemoveSpawnedTower(Tower tower)
    {
        spawnedTowers.Remove(tower);
    }

    public List<Tower> GetSpawnedTowers()
    {
        return spawnedTowers;
    }

    public void AddLevelTower(Tower tower, int level)
    {
        if (!string.IsNullOrEmpty(tower.towerData.towerName))
        {
            levelTowers[tower.towerData.towerName] = level;
        }
    }

    public void UpgradeTowerAtk()
    {
        foreach(var tower in spawnedTowers)
        {
            string towerName = tower.towerData.towerName;
            int towerLevel = levelTowers.ContainsKey(towerName) ? levelTowers[towerName] : 0;
            tower.UpgradeAtkDamage(towerLevel);
            tower.UpgradeAtkSpeed(towerLevel);
        }
    }
}
