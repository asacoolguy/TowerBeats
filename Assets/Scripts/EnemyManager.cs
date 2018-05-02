using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning and destroying enemies. Interacts with GameManager. 
/// </summary>

public class EnemyManager : MonoBehaviour {
	public GameObject enemyPrefab;
    public GameObject[] spawnPoints;
	public float spawnDistance = 40f;
	public float enemyAltitude = 10f;
	public float enemyHealth = 2f;
	public float enemyDistancePerMove = 6f;

	private List<GameObject> allEnemies;
	private List<Vector3> spawnPositions;
	private List<int> enemiesNumberToSpawn;
	private bool isSpawning;

    private EnemyPath enemyPath;

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
        enemyPath = FindObjectOfType<EnemyPath>();
	}

	void LateUpdate() {
        // if all enemies are done ascending, turn off the light there
        // TODO: can do this better by keeping track of number of enemies done ascending
        if (!isSpawning) {
            bool doneAscending = true;
            foreach (GameObject e in allEnemies) {
                if (e.GetComponent<Enemy>().ascending) {
                    doneAscending = false;
                }
            }
            if (doneAscending) {
                enemyPath.ToggleLight(0, false);
            }
        }
       

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

		enemiesNumberToSpawn.Clear();
		for (int i = 0; i < wave; i++) {
			enemiesNumberToSpawn.Add(3 + wave);
		}

        // TODO: make the light turn on after a short delay
        enemyPath.ToggleLight(0, true);
    }


	// spawns the appropriate amount of enemies for the current wave
	public void SpawnEnemies() {
		bool justSpawned = false;

        for (int i = 0; i < enemiesNumberToSpawn.Count; i++) {
            if (enemiesNumberToSpawn[i] > 0) {
                GameObject enemyObj = Instantiate(enemyPrefab, spawnPoints[0].transform.position, Quaternion.identity, this.transform);
                enemyObj.GetComponent<Enemy>().SetPath(enemyPath.GetPath(0));
                allEnemies.Add(enemyObj);
                enemiesNumberToSpawn[i]--;
                justSpawned = true;
                
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
