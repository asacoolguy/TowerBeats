using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
	private Animator anim;

	public float health;
	public float distancePerMove;
	public float moveDuration;
	public float pointVal;
    public int moneyDropped;


	private void Start () {
		anim = GetComponent<Animator>();
	}
	

	private void Update () {
		// adjust to always face the center
		FaceCenter();
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "HomeBase"){
			FindObjectOfType<GameManager>().TakeDamage(1);
			StartCoroutine(SelfDestruct());
		}
	}

	public void FaceCenter(){
		float angle = 90f - GameManager.GetAngleFromVector(transform.position);
		transform.eulerAngles = new Vector3(0, angle, 0);
	}

	public IEnumerator Move(){
		float currentDuration = 0f;
		Vector3 targetLocation = new Vector3(0, transform.position.y, 0);
		float moveSpeed = distancePerMove / moveDuration;

		while(currentDuration <= moveDuration){
			float speedRatio = Mathf.Pow(1f - (currentDuration / moveDuration), 3f);
			float moveAmount = moveSpeed * speedRatio * Time.deltaTime;
			transform.position += (targetLocation - transform.position).normalized * moveAmount;
			currentDuration += Time.deltaTime;
			yield return null;
		}
	}


	// called by other functions to damage this enemy. destroys this enemy when appropriate
	public void TakeDamage(float i){
		health -= i;
		if (health <= 0f){
			// destroy
			StartCoroutine(SelfDestruct());
		}
		else{
			// play damage effect 
			anim.SetTrigger("TakeDamage");
		}

	}


	// destroy this object and play the appropriate animation
	private IEnumerator SelfDestruct(){
		FindObjectOfType<GameManager>().GetPoints(pointVal);
        FindObjectOfType<GameManager>().GetMoney(moneyDropped);

        ParticleSystem ps = transform.GetChild(0).GetComponent<ParticleSystem>();
		ps.Play();
		GetComponent<MeshRenderer>().enabled = false;
		FindObjectOfType<EnemyManager>().PlayDestroyEnemySound();

		while (ps.isPlaying){
			yield return null;
		}

		FindObjectOfType<EnemyManager>().DestroyEnemy(this.gameObject);
	}
}
