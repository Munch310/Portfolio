using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerData towerData;
    public GameObject dotPrefab;

    public float currentAtkDamage;
    public float currentAtkSpeed;

    private SpriteRenderer spriteRenderer;

    public int level = 1;
    private List<Dot> dots = new List<Dot>();

    public GameObject bulletPrefab;
    public Transform firePoint;

    public float range = 5f;
    private float nextFireTime = 0f;

    private const string EnemyTag = "Enemy";


    private void Start()
    {
        string towerName = towerData.towerName;
        gameObject.name = towerName;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && towerData.sprite != null)
        {
            spriteRenderer.sprite = towerData.sprite;
        }

        GenerateDots();
        UpdateDots();

        StartCoroutine(FireEnemies());
    }

    IEnumerator FireEnemies()
    {
        while (true)
        {
            if (Time.time >= nextFireTime)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range);
                Transform enemyInRange = null;

                foreach (var collider in colliders)
                {
                    if (collider.CompareTag(EnemyTag))
                    {
                        enemyInRange = collider.transform; break;
                    }
                }

                if (enemyInRange != null)
                {
                    FireBullet(enemyInRange);
                    nextFireTime = Time.time + 1f / currentAtkSpeed;
                }
            }

            yield return null;
        }
    }

    void FireBullet(Transform enemy)
    {
        Vector2[] positions = CalculateDotPositions(level);

        for (int i = 0; i < level; i++)
        {
            Vector3 firePointPosition = firePoint.position + (Vector3)positions[i];

            GameObject bulletGO = ObjectPoolingManager.instance.GetFromPool(bulletPrefab.name);

            if (bulletGO == null)
            {
                bulletGO = Instantiate(bulletPrefab, firePointPosition, Quaternion.identity);
            }
            else
            {
                bulletGO.transform.position = firePointPosition;
                bulletGO.transform.rotation = Quaternion.identity;
                bulletGO.SetActive(true);
            }

            Bullet bullet = bulletGO.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.SetTarget(enemy);
                bullet.tower = this;
                bullet.damage = currentAtkDamage;

                SpriteRenderer sr_bullet = bulletGO.GetComponent<SpriteRenderer>();
                if (sr_bullet != null)
                {
                    sr_bullet.color = towerData.dotColor;
                }
            }
        }
    }



    void GenerateDots()
    {
        Vector2[] positions = CalculateDotPositions(level);

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject dotObject =
               Instantiate(dotPrefab);
            dotObject.transform.SetParent(transform);

            Dot dot = dotObject.GetComponent<Dot>();
            if (dot != null)
            {
                dot.SetColor(towerData.dotColor);
                dot.SetLocalPosition(positions[i]);
                dots.Add(dot);
            }
        }
    }

    void UpdateDots()
    {
        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].SetActive(i < level);
        }
    }

    public void UpgradeTower()
    {
        if(level < 6)
        {
            level++;
            UpdateDots();
        }
    }

    public void UpgradeAtkDamage(int level)
    {
        currentAtkDamage = towerData.towerAtkDamage + level * 2f;
    }

    public void UpgradeAtkSpeed(int level)
    {
        currentAtkSpeed = towerData.towerAtkSpeed + (level * 0.2f);
    }

    private Vector2[] CalculateDotPositions(int towerLevel)
    {
        Vector2[] positions = null;

        switch (towerLevel)
        {
            case 1:
                positions = new Vector2[] { Vector2.zero };
                break;
            case 2:
                positions = new Vector2[] { new Vector2(-0.3f, -0.3f), new Vector2(0.3f, 0.3f) };
                break;
            case 3:
                positions = new Vector2[] { new Vector2(-0.3f, -0.3f), Vector2.zero, new Vector2(0.3f, 0.3f) };
                break;
            case 4:
                positions = new Vector2[] { new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f), new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f) };
                break;
            case 5:
                positions = new Vector2[] { new Vector2(-0.3f, -0.3f), new Vector2(0.3f, -0.3f), Vector2.zero, new Vector2(-0.3f, 0.3f), new Vector2(0.3f, 0.3f) };
                break;
            case 6:
                positions = new Vector2[] { new Vector2(-0.3f, -0.3f), new Vector2(-0.3f, 0f), new Vector2(-0.3f, 0.3f), new Vector2(0.3f, -0.3f), new Vector2(0.3f, 0f), new Vector2(0.3f, 0.3f) };
                break;
            default:
                break;
        }

        return positions;
    }
}
 