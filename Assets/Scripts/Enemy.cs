using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
	private Animator anim;
    private List<Vector3> path;
    private int nextTarget;
    private LineRenderer healthBar;

	public float health;
    private float initialHealth;
	public float distancePerMove;
	public float moveDuration;
	public float pointVal;
    public int moneyDropped;

    public bool ascending; // this is true when enemy is still rising. enemy is untargetable in this phase


	private void Start () {
		anim = GetComponent<Animator>();
        healthBar = transform.Find("HealthBar").GetComponent<LineRenderer>();
        initialHealth = health;
    }
	

	private void Update () {
        healthBar.SetPosition(0, transform.position + new Vector3(-initialHealth / 4f, 0f, 4f));
        healthBar.SetPosition(1, healthBar.GetPosition(0) + new Vector3(health / 2f, 0, 0));
    }

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "HomeBase"){
			FindObjectOfType<GameManager>().TakeDamage(1);
			StartCoroutine(SelfDestruct());
		}
	}


	public void FaceDirection(Vector3 direction){
		float angle = 90f - GameManager.GetAngleFromVectorSpecial(direction);
		transform.eulerAngles = new Vector3(0, angle, 0);
	}


	public IEnumerator Move(){
		float currentDuration = 0f;
        Vector3 targetLocation = path[nextTarget];
		float moveSpeed = distancePerMove / moveDuration;

		while(currentDuration <= moveDuration){
			float speedRatio = Mathf.Pow(1f - (currentDuration / moveDuration), 3f);
			float moveAmount = moveSpeed * speedRatio * Time.deltaTime;
            Vector3 moveDirection = (targetLocation - transform.position);

            // if we're still ascending out of the spawn point, double the speed and face up
            if (nextTarget == 1) {
                moveAmount *= 2;
                float angle = 90f + GameManager.GetAngleFromVectorSpecial(transform.position);
                transform.eulerAngles = new Vector3(90, 0, angle);
                ascending = true;
            }
            else {
                FaceDirection(-moveDirection);
                ascending = false;
            }

			transform.position += moveDirection.normalized * moveAmount;
            if (Vector3.Distance(transform.position, targetLocation) < 0.1f) {
                targetLocation = path[++nextTarget];
            }

			currentDuration += Time.deltaTime;
			yield return null;
		}
	}


	// called by other functions to damage this enemy. destroys this enemy when appropriate
	public void TakeDamage(float i){
        if (!ascending) {
            health -= i;
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


    public void SetPath(List<Vector3> input) {
        path = input;
        nextTarget = 1;
        ascending = true;
    }
}
