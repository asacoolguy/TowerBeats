﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour {
	private Animator anim;
    private List<Vector3> path;
    private int nextTarget;
    private LineRenderer healthBar;
    private EnemyManager enemyManager;
    private GameObject lightning;

	public float health, healthDisplayFactor, healthBonusFactor, slowFactor;
    private float initialHealth;
	public float distancePerMove, distancePerMoveOnSpawn;
	public float moveDuration;
	public float pointVal;
    public int moneyDropped;
    private int slowCounter;
    private float travelDist;
    private float maxHeight;

    public bool regenerate;
    public int regenerateStartDelay;
    private int currentRegenerateDelay;
    public float regenerateMeasure;
    private float regenerateSpeed;
    

	private void Awake () {
		anim = GetComponent<Animator>();
        healthBar = transform.Find("HealthBar").GetComponent<LineRenderer>();

        lightning = transform.Find("Lightning").gameObject;
        lightning.SetActive(false);

        moneyDropped += Random.Range(-moneyDropped / 4, 2);
        distancePerMove *= (1f + 0.15f * GameManager.instance.GetCurrentWave());
        health *= (1f + healthBonusFactor * GameManager.instance.GetCurrentWave());
        initialHealth = health;
        transform.Find("MoneyText").GetComponentInChildren<Text>().text = "+" + moneyDropped;
        transform.Find("MoneyText").gameObject.SetActive(false);

        travelDist = 0;
        regenerateSpeed = health / (regenerateMeasure * GameManager.instance.GetScanner().GetTimePerMeasure());
    }
	

	private void Update () {
        if (transform.position.y > maxHeight) {
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
        }

        if (regenerate && currentRegenerateDelay == 0 && health < initialHealth) {
            // regain health
            health += regenerateSpeed * Time.deltaTime;
            if (health > initialHealth) {
                health = initialHealth;
            }
        }

        healthBar.SetPosition(0, transform.position + new Vector3(-initialHealth * healthDisplayFactor / 4f, 0f, 4f));
        float fill = Mathf.Max(0f, health * healthDisplayFactor / 2);
        healthBar.SetPosition(1, healthBar.GetPosition(0) + new Vector3(fill, 0, 0));
    }


	void OnTriggerEnter(Collider other){
        if (nextTarget == path.Count - 1 && other.gameObject.tag == "HomeBase"){
			GameManager.instance.TakeDamage(1);
            GameManager.instance.GainPoints(-pointVal); // no points gained if self destructed
            StartCoroutine(SelfDestruct());
		}
	}


	public void FaceDirection(Vector3 direction){
        Vector3 dir = new Vector3(Mathf.Abs(direction.x) > 0.2f ? direction.x : 0,
                                  0,
                                  Mathf.Abs(direction.z) > 0.2f ? direction.z : 0);

        float angle;
        if (dir.x == 0) {
            if (dir.z > 0) {
                angle = 0;
            }
            else {
                angle = 180f;
            }
        }
        else {
            angle = 90 * Mathf.Sign(dir.x) - Mathf.Atan(dir.z/dir.x) * Mathf.Rad2Deg;
        }
        
        transform.eulerAngles = new Vector3(0, angle, 0);
	}


	public IEnumerator Move(){
        if (nextTarget < path.Count && GetComponent<MeshRenderer>().enabled) {
            float currentDuration = 0f;
            Vector3 targetLocation = path[nextTarget];

            if (regenerate && currentRegenerateDelay > 0) {
                currentRegenerateDelay--;
            }

            if (slowCounter > 0) {
                slowCounter--;
                lightning.SetActive(true);
            }
            else {
                lightning.SetActive(false);
            }
            
            while (currentDuration <= moveDuration) {
                float speedRatio = Mathf.Pow(1f - (currentDuration / moveDuration), 3f);
                float moveSpeed = distancePerMove / moveDuration * (slowCounter > 0 ?  slowFactor : 1);
                if (nextTarget == 0){
                    if (transform.position.y < 0) {
                        moveSpeed = distancePerMoveOnSpawn;
                    }
                    else {
                        moveSpeed = Mathf.Lerp(distancePerMoveOnSpawn, distancePerMove, transform.position.y / maxHeight);
                    }
                }

                float moveAmount = moveSpeed * speedRatio * Time.deltaTime;
                Vector3 moveDirection = path[nextTarget] - transform.position;
                travelDist += moveAmount;

                transform.position += moveDirection.normalized * moveAmount;
                if (Vector3.Distance(transform.position, path[nextTarget]) < 0.5f && (nextTarget + 1) < path.Count) {
                    ++nextTarget;
                    FaceDirection(path[nextTarget] - path[nextTarget - 1]);
                }
                
                currentDuration += Time.deltaTime;
                yield return null;
            }
        }
	}


	// called by other functions to damage this enemy. destroys this enemy when appropriate
	public void TakeDamage(float i){
        if (IsVulnerable()) {
            health -= i;
            currentRegenerateDelay = regenerateStartDelay;

            if (health <= 0f) {
                // destroy
                StartCoroutine(SelfDestruct());
            }
            else {
                // play damage effect 
                anim.SetTrigger("TakeDamage");
            }
        }

	}


    // slows the enemy
    public void Slow() {
        slowCounter = 4;
    }


	// destroy this object and play the appropriate animation
	private IEnumerator SelfDestruct(){
		GameManager.instance.GainPoints(pointVal);
        GameManager.instance.GainMoney(moneyDropped);

        // show moneyText
        transform.Find("MoneyText").localEulerAngles = new Vector3(90, -transform.localEulerAngles.y, 0);
        transform.Find("MoneyText").gameObject.SetActive(true);

        ParticleSystem ps = transform.GetChild(0).GetComponent<ParticleSystem>();
		ps.Play();
		GetComponent<MeshRenderer>().enabled = false;
        healthBar.enabled = false;
        enemyManager.PlayDestroyEnemySound();

		while (ps.isPlaying){
			yield return null;
		}

		enemyManager.DestroyEnemy(this.gameObject);
	}


    public void SetEnemyManager(EnemyManager manager) {
        enemyManager = manager;
    }


    public void SetPath(List<Vector3> input, float heightOffset) {
        nextTarget = 0;
        path = new List<Vector3>();

        // randomize the path a little
        float offsetRange = 0.5f;
        Vector3 offset = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange));
        for (int i = 0; i < input.Count; i++) {
            path.Add(input[i] + offset + new Vector3(0, heightOffset, 0));
        }

        maxHeight = offset.y + heightOffset;

        // face up
        float angle = 90f + GameManager.GetAngleFromVector(transform.position);
        transform.eulerAngles = new Vector3(-90, 0, angle);
    }


    public float GetTravelDist() {
        return travelDist;
    }

    
    public bool IsVulnerable() {
        return nextTarget != 0;
    }
}
