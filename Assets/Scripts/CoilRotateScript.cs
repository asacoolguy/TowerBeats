using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoilRotateScript : MonoBehaviour {
	public float rotationSpeed;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		// rotate the scanner (and its underlying line)
		transform.Rotate(new Vector3(0, - rotationSpeed * Time.deltaTime, 0));

	}
}
