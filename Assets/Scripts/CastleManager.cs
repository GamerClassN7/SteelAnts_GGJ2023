using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleManager : MonoBehaviour
{
    public float health = 100;
    private float actualHealth;
    public List<GameObject> units = new List<GameObject>();
    private List<GameObject> usableUnits = new List<GameObject>();
    public int level = 1;
    public float spawnUnitsInterval = 5.0f;
    private float lastSpawnInterval = 0.0f;
    public Transform spawnPoint;

    void Start()
    {
        actualHealth = health;
        foreach (GameObject unit in units)
        {
            UnitManager unitManager = unit.GetComponent<UnitManager>();
            if (unitManager != null && unitManager.neededlevel <= level)
            {
                usableUnits.Add(unit);
            }
        }
    }

    void Update()
    {
        if (lastSpawnInterval < Time.time)
        {
            lastSpawnInterval = Time.time + spawnUnitsInterval;
            SpawnUnits();
        }
    }

    public void SpawnUnits()
    {
        if (usableUnits.Count != 0) {
            Instantiate(usableUnits[Random.Range(0, usableUnits.Count)], spawnPoint.position, spawnPoint.rotation);
        }
    }
}
