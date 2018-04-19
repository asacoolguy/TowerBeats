﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning and destroying enemies. Interacts with GameManagerNew. 
/// </summary>

public class EnemyManagerNew : MonoBehaviour {
    public GameObject enemy;
    public float spawnDistance = 40f;
    public float enemyAltitude = 10f;
    public float enemyHealth = 2f;
    public float enemyDistancePerMove = 6f;

    private List<GameObject> allEnemies;
    private List<Vector3> spawnPositions;
    private List<int> enemiesNumberToSpawn;
    private bool isSpawning;

    // enemy destroy audio
    private int enemyDestroySoundCounter;
    public AudioClip enemyDestroySound;

    void OnEnable() {
        Scanner.RotatedFully += SpawnEnemies;
        Scanner.RotatedMeasure += MoveEnemies;
    }


    void OnDisable() {
        Scanner.RotatedFully -= SpawnEnemies;
        Scanner.RotatedMeasure -= MoveEnemies;
    }
    

    void Start() {
        allEnemies = new List<GameObject>();
        spawnPositions = new List<Vector3>();
        enemiesNumberToSpawn = new List<int>();
    }

    void LateUpdate() {
        // destory enemies and play the destruction sound at the right volume
        if (enemyDestroySoundCounter > 0) {
            float volume = Mathf.Clamp(0.9f + enemyDestroySoundCounter * 0.1f, 1f, 1.5f);
            GetComponent<AudioSource>().PlayOneShot(enemyDestroySound, volume);
            enemyDestroySoundCounter = 0;
        }
    }


    // set up the correct spawn info for the current wave
    // ith wave will have 1 + i spawn points
    // each spawn point will spawn 3 + i enemies
    public void SetupWave(int wave) {
        isSpawning = true;

        // generate new spawn points
        int newPointsToGenerate = wave - spawnPositions.Count;
        for (int i = 0; i < newPointsToGenerate; i++) {
            float angle = Random.Range(0f, 360f);
            Vector3 spawnPos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * spawnDistance,
                                            enemyAltitude + transform.position.y,
                                            Mathf.Sin(angle * Mathf.Deg2Rad) * spawnDistance);
            spawnPositions.Add(spawnPos);
        }

        enemiesNumberToSpawn.Clear();
        for (int i = 0; i < wave; i++) {
            enemiesNumberToSpawn.Add(3 + wave);
        }
    }


    // spawns the appropriate amount of enemies for the current wave
    public void SpawnEnemies() {
        bool justSpawned = false;

       for (int i = 0; i < enemiesNumberToSpawn.Count; i++) {
            if (enemiesNumberToSpawn[i] > 0) {
                GameObject enemyObj = Instantiate(enemy, spawnPositions[i], Quaternion.identity, this.transform);
                allEnemies.Add(enemyObj);
                enemiesNumberToSpawn[i]--;
                justSpawned = true;
                enemyObj.GetComponent<Enemy>().manager = this;
            }
        }

        isSpawning = justSpawned;
    }


    // move all enemies
    public void MoveEnemies() {
        for (int i = 0; i < allEnemies.Count; i++) {
            if (allEnemies[i] != null) {
                Enemy e = allEnemies[i].GetComponent<Enemy>();
                e.StartCoroutine(e.Move());
            }
        }
    }


    // destroy the given enemy. add it to the enemyTodestory list which will destory them
    // all at the same time and also play th destory sound at the right volume
    public void DestroyEnemy(GameObject obj) {
        allEnemies.Remove(obj);
        Destroy(obj);
    }

    public void PlayDestroyEnemySound() {
        enemyDestroySoundCounter++;
    }


    public void ClearEnemyList() {
        allEnemies.Clear();
    }

    // destroy all enemies. called during level transitions.
    public void DestroyAllEnemies() {
        for (int i = 0; i < allEnemies.Count; i++) {
            Destroy(allEnemies[i]);
        }
    }

    public bool IsSpawning() {
        return isSpawning;
    }

}
