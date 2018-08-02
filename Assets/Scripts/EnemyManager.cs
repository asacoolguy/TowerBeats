﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning and destroying enemies. Interacts with GameManager. 
/// </summary>

public class EnemyManager : MonoBehaviour {
	public GameObject[] enemyPrefabs;
    public float heightOffset;
    public float spawnMeasureCount;
    private float currentMeasureCount;

	private List<GameObject> allEnemies;
    private string[] waveSpawnInstructions; // spawn instructions for the entire stage
    private string[] spawnInstruction; // specifc instructions for a certain wave
    private int instructionIndex;
    private int waitCounter;
	private bool isSpawning;
    public bool waveDone;

    private EnemyPath enemyPath;
    private int pathsUsed = 0;

	// enemy destroy audio
	private int enemyDestroySoundCounter;
    private AudioClip[] enemyDeathClips;
    private int enemyDeathClipIndex = 0;


	void OnEnable() {
        //Scanner.RotatedFully += SpawnEnemies;
        Scanner.RotatedMeasure += IncreaseSpawnEnemyCounter;
		Scanner.RotatedMeasure += MoveEnemies;
	}


	void OnDisable() {
        //Scanner.RotatedFully -= SpawnEnemies;
        Scanner.RotatedMeasure -= IncreaseSpawnEnemyCounter;
        Scanner.RotatedMeasure -= MoveEnemies;
	}


	void Start() {
		allEnemies = new List<GameObject>();
        enemyPath = FindObjectOfType<EnemyPath>();
        enemyDeathClips = FindObjectOfType<GameManager>().GetEnemyAudioClips();
	}

	void LateUpdate() {
		// destory enemies and play the destruction sound at the right volume
		if (enemyDestroySoundCounter > 0) {
			float volume = Mathf.Clamp(0.9f + enemyDestroySoundCounter * 0.1f, 1f, 1.5f);
			GetComponent<AudioSource>().PlayOneShot(enemyDeathClips[enemyDeathClipIndex], volume);
			enemyDestroySoundCounter = 0;
            enemyDeathClipIndex = ++enemyDeathClipIndex % enemyDeathClips.Length;

            // shake camera
            float temp = 1f + enemyDestroySoundCounter / 5f;
            FindObjectOfType<CameraShakeScript>().ShakeCamera(temp, temp);
        }
        
	}


    public void SetSpawnInstruction(string[] instruction) {
        waveSpawnInstructions = instruction;
        instructionIndex = -1;
        waitCounter = 0;
        currentMeasureCount = 0;
        isSpawning = waveDone = false;
    }


	// set up the correct spawn info for the current wave
	public void SetupWave(int waveNum) {
        if (waveNum < waveSpawnInstructions.Length) {

            isSpawning = true;
            waveDone = false;

            if (waveSpawnInstructions[waveNum].Contains("B")) {
                pathsUsed = 2;
            }
            else if (waveSpawnInstructions[waveNum].Contains("A")) {
                pathsUsed = 1;
            }


            spawnInstruction = waveSpawnInstructions[waveNum].Split(';');
            instructionIndex = 0;
            waitCounter = 1; // wait a round before actually spawning
        }
    }


    public void IncreaseSpawnEnemyCounter() {
        currentMeasureCount++;
        if (currentMeasureCount >= spawnMeasureCount) {
            SpawnEnemies();
            currentMeasureCount = 0;
        }
    }


	// spawns the appropriate amount of enemies for the current wave
	public void SpawnEnemies() {
        // guard against null
        if (!isSpawning || spawnInstruction == null || spawnInstruction.Length == 0) {
            return;
        }

        // turn on the paths we're using
        //for (int i = 0; i < pathsUsed; i++) {
        //    enemyPath.TogglePath(i, true);
        //}

        // if we've read all instructions, then we are done spawning.
        if (instructionIndex >= spawnInstruction.Length) {
            // if all enemies are dead, the current wave is done
            if (allEnemies.Count == 0) {
                waveDone = true;
            }
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
        for (int i = 0; i < spawnAmount; i++) {
            Vector3 position = enemyPath.GetSpawnPoint(spawnPointIndex);
            GameObject enemyObj = Instantiate(enemyPrefabs[enemyType], 
                                              position, 
                                              Quaternion.identity, 
                                              this.transform);
            enemyObj.GetComponent<Enemy>().SetPath(enemyPath.GetPath(spawnPointIndex), heightOffset);
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
}
