using UnityEngine;

public class PrefabDatabase : MonoBehaviour{
    [Header("Enemies")]
    public GameObject enemy;
    public GameObject enemyFast;
    public GameObject enemyRegen;
    public GameObject enemyHeavy;

    [Header("Towers")]
    public GameObject shockTower;
    public GameObject sniperTower;
    public GameObject laserTower;
    private GameObject[] towerPrefabs;

    [Header("Panels")]
    public GameObject buildPanel;
    public GameObject upgradePanel;

    [Header("Other")]
    public GameObject towerPlatform;


    public GameObject GetTower(int i) {
        if (i == 0) {
            return shockTower;
        }
        else if (i == 1) {
            return sniperTower;
        }
        else if (i == 2){
            return laserTower;
        }
        else {
            return null;
        }
    }
}
