using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning and destroying enemies. Interacts with GameManager. 
/// </summary>

public class EnemyManager : MonoBehaviour {
	public GameObject[] enemyPrefabs;
    public GameObject[] spawnPoints;
	public float spawnDistance = 40f;
	public float enemyAltitude = 10f;
	public float enemyHealth = 2f;
	public float enemyDistancePerMove = 6f;

	private List<GameObject> allEnemies;
    private string[] spawnInstruction;
    private int instructionIndex;
    private int waitCounter;
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
        enemyPath = FindObjectOfType<EnemyPath>();
        instructionIndex = -1;
        waitCounter = 0;
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
	public void SetupWave(string pattern) {
		isSpawning = true;

        if (pattern.Contains("A")) {
            enemyPath.TogglePath(0, true);
        }
        if (pattern.Contains("B")) {
            enemyPath.TogglePath(1, true);
        }
        
        spawnInstruction = pattern.Split(';');
        instructionIndex = 0;
        waitCounter = 1; // wait a round before actually spawning
    }


	// spawns the appropriate amount of enemies for the current wave
	public void SpawnEnemies() {
        // guard against null
        if (spawnInstruction == null || spawnInstruction.Length == 0) {
            return;
        }

        // if we've read all instructions, then we are done spawning.
        if (instructionIndex >= spawnInstruction.Length) {
            isSpawning = false;
        }
        // if we're supposed to wait this round, wait and decrement the counter
        else if(waitCounter > 0) {
            waitCounter--;
        }
        else{
            // parse the spawnPattern
            // A and B denotes which spawn point
            // s# = small enemy
            // l# = large enemy
            // w# = wait some amount of rotations
            // so As3,Bs3;w2;Al1,Bs2 means spawn 3 small enemies at A and 3 small enemies at B, 
            // wait 2 rotations, then spawn 1 large enemy at A and 2 small enemies at B
            string[] currentInstruction = spawnInstruction[instructionIndex++].Split(',');
            foreach (string instruction in currentInstruction) {
                if (instruction == "") continue;
                string char1 = instruction.Substring(0, 1);
                if (char1 == "w") {
                    waitCounter = int.Parse(instruction.Substring(1, 1));
                }
                else{
                    int spawnPointIndex = 0;
                    if (char1 == "A") {
                        spawnPointIndex = 0;
                    } else if (char1 == "B") {
                        spawnPointIndex = 1;
                    }

                    int enemyType = 0;
                    string char2 = instruction.Substring(1, 1);
                    if (char2 == "s") {
                        enemyType = 0;
                    }
                    else if (char2 == "l") {
                        enemyType = 1;
                    }

                    int spawnAmount = int.Parse(instruction.Substring(2, 1));

                    StartCoroutine(SpawnEnemies(spawnPointIndex, enemyType, spawnAmount));
                }
            }
        }
	}


    // spawns enemies with some random delay
    private IEnumerator SpawnEnemies(int spawnPointIndex, int enemyType, int spawnAmount) {
        Vector3 offset = Vector3.zero;
        for (int i = 0; i < spawnAmount; i++) {
            Vector3 position = spawnPoints[spawnPointIndex].transform.position + offset;
            GameObject enemyObj = Instantiate(enemyPrefabs[enemyType], 
                                              position, 
                                              Quaternion.identity, 
                                              this.transform);
            enemyObj.GetComponent<Enemy>().SetPath(enemyPath.GetPath(spawnPointIndex));
            allEnemies.Add(enemyObj);
            
            //offset -= new Vector3(0, Random.Range(3f, 7f), 0);
            yield return null;
        }
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
