using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerLine : MonoBehaviour {
	private List<GameObject> alreadyActiveTowers;

	// Use this for initialization
	void Start () {
		alreadyActiveTowers = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	/*
	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Tower" && other.GetComponent<BasicTower>().IsFunctional()){
			other.GetComponent<BasicTower>().PlaySound();
			alreadyActiveTowers.Add(other.gameObject);
		}
	}
	*/
}
