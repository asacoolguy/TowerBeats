using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning and destroying enemies. Interacts with GameManager. 
/// </summary>

public class EnemyManager : MonoBehaviour {
	public GameObject enemy;
	public float spawnDistance = 35f;
	public float amountPerSpawn = 2f;
	public int delaySpawn = 1; // how many rotations to delay before we start spawning
	public float enemyAltitude = 10f;
	public float enemyHealth = 2f;
	public float enemyDistancePerMove = 6f;
	public float enemyPointMultiplier = 1f;

	[SerializeField]private List<GameObject> allEnemies;

	// Use this for initialization
	void Start () {
		allEnemies = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	// spawns set number of enemies
	public void SpawnEnemies(){
		if (delaySpawn > 0){
			delaySpawn--;
			return;
		}

		// pick random vertex direction
		float angle = Random.Range(0f, 360f);

		for (int i = 0; i < amountPerSpawn; i++){
			// spawn enemy. 
			Vector3 spawnPos = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * spawnDistance,
										   enemyAltitude,
										   Mathf.Sin(angle * Mathf.Deg2Rad) * spawnDistance);
			GameObject obj = Instantiate(enemy, spawnPos, Quaternion.identity, this.transform);
			allEnemies.Add(obj);

			// adjust spawned enemy with appropriate settings
			Enemy e = obj.GetComponent<Enemy>();
			e.health = enemyHealth;
			e.distancePerMove = enemyDistancePerMove;
			e.pointVal *= enemyPointMultiplier;

			// rotate angle	
			angle += Random.Range(-20f, 20f);
		}

		// increase next wave's amount
		amountPerSpawn += 0.05f;
	}


	// move all enemies
	public void MoveEnemies(){
		for (int i = 0; i < allEnemies.Count; i++){
			if (allEnemies[i] != null){
				Enemy e = allEnemies[i].GetComponent<Enemy>();
				e.StartCoroutine(e.Move());
			}
		}
	}


	// destroy the given enemy
	public void DestroyEnemy(GameObject obj){
		allEnemies.Remove(obj);
		Destroy(obj);
	}


	public void ClearEnemyList(){
		allEnemies.Clear();
	}

	// destroy all enemies. called during level transitions.
	public void DestroyAllEnemies(){
		for (int i = 0; i < allEnemies.Count; i++){
			Destroy(allEnemies[i]);
		}
	}

}
