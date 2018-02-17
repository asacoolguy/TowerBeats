using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles tracking which enemies are currently damageable by this Tower
/// </summary>
public class TowerAoE : MonoBehaviour {
	public List<Enemy> enemiesInRange;

	// Use this for initialization
	void Start () {
		enemiesInRange = new List<Enemy>();
	}
	
	// Update is called once per frame
	void Update () {
		// clear null enemies out of the list
		for (int i = enemiesInRange.Count - 1; i > -1; i--){
			if (enemiesInRange[i] == null){
				enemiesInRange.RemoveAt(i);
			}
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject != null && other.gameObject.tag == "Enemy"){
			enemiesInRange.Add(other.GetComponent<Enemy>());
		}
	}

	void OnTriggerExit(Collider other){
		if(other.gameObject != null && other.gameObject.tag == "Enemy"){
			enemiesInRange.Remove(other.GetComponent<Enemy>());
		}
	}
}
