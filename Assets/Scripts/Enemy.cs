﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
	private Animator anim;

	public float health = 2f;
	public float distancePerMove = 5f;
	public float moveDuration = 0.2f;
	public float thresholdDist = 5f; // how close before it will start attack home base
	public float pointVal = 10f;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		// adjust to always face the center
		FaceCenter();
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "HomeBase"){
			FindObjectOfType<GameManager>().TakeDamage(1);
			SelfDestruct();
		}
	}

	public void FaceCenter(){
		float angle = 90f - GameManager.GetAngleFromVector(transform.position);
		transform.eulerAngles = new Vector3(0, angle, 0);
	}

	public IEnumerator Move(){
		float currentDuration = 0f;
		Vector3 targetLocation = new Vector3(0, transform.position.y, 0);
		// if we're not close enough to homebase yet, just move forward
		if ((targetLocation - transform.position).magnitude > thresholdDist){
			float moveSpeed = distancePerMove / moveDuration;

			while(currentDuration <= moveDuration){
				float speedRatio = Mathf.Pow(1f - (currentDuration / moveDuration), 3f);
				float moveAmount = moveSpeed * speedRatio * Time.deltaTime;
				transform.position += (targetLocation - transform.position).normalized * moveAmount;
				currentDuration += Time.deltaTime;
				yield return null;
			}
		}
		else{
			// we're close enough to attack. dive into the homebase
			float distanceToBase = transform.position.magnitude;
			float moveSpeed = distanceToBase / moveDuration;

			while(currentDuration <= moveDuration){
				float speedRatio = Mathf.Pow(1f - (currentDuration / moveDuration), 3f);
				float moveAmount = moveSpeed * speedRatio * Time.deltaTime;
				transform.position += - transform.position.normalized * moveAmount;
				currentDuration += Time.deltaTime;
				yield return null;
			}
		}
	}


	// called by other functions to damage this enemy. destroys this enemy when appropriate
	public void TakeDamage(float i){
		health -= i;
		if (health <= 0f){
			// destroy
			SelfDestruct();
		}
		else{
			// play damage effect 
			anim.SetTrigger("TakeDamage");
		}

	}


	// destroys this object and plays the appropriate animation
	public void SelfDestruct(){
		// TODO: need FX/animation for this
		FindObjectOfType<GameManager>().GetPoints(pointVal);
		FindObjectOfType<EnemyManager>().DestroyEnemy(this.gameObject);
	}
}
