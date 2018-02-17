using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandingWave : MonoBehaviour {
	private float initialSize, finalSize;
	private float expansionSpeed;
	private List<GameObject> alreadyActiveTowers;

	public float initialDelay = 0.4f;
	private float currentDelay = 0;

	// Use this for initialization
	void Start () {
		initialSize = 6;
		finalSize = 145;

		transform.localScale = new Vector3(initialSize, 5f, initialSize);

		expansionSpeed = (finalSize - initialSize) / 3.5f;
	}
	
	// Update is called once per frame
	void Update () {
		if (currentDelay < initialDelay){
			currentDelay += Time.deltaTime;
			return;
		}
		if (transform.localScale.x < finalSize + 5){
			transform.localScale = transform.localScale + new Vector3(expansionSpeed * Time.deltaTime, 0, expansionSpeed * Time.deltaTime);
		}
		else{
			transform.localScale = new Vector3(initialSize, 5f, initialSize);
			currentDelay = 0f;
			gameObject.SetActive(false);
		}
	}

	void OnTriggerEnter(Collider other){
		// destroy all towers and enmies it hits
		if (other.tag == "Tower" || other.tag == "Enemy"){
			Destroy(other.gameObject);
		}
	}
}
