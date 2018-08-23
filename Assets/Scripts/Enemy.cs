using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private int slowCounter;
    private float travelDist;

    public bool ascending; // this is true when enemy is still rising. enemy is untargetable in this phase

    public bool regenerate;
    public int regenerateStartDelay;
    private int currentRegenerateDelay;
    public float regenerateMeasure;
    private float regenerateSpeed;
    

	private void Awake () {
		anim = GetComponent<Animator>();
        healthBar = transform.Find("HealthBar").GetComponent<LineRenderer>();
        initialHealth = health;

        transform.Find("MoneyText").GetComponentInChildren<Text>().text = "+" + moneyDropped;
        transform.Find("MoneyText").gameObject.SetActive(false);

        print("spawned at position " + transform.position);

        travelDist = 0;
        regenerateSpeed = health / (regenerateMeasure * FindObjectOfType<Scanner>().getTimePerMeasure());
    }
	

	private void Update () {
        if (regenerate && currentRegenerateDelay == 0 && health < initialHealth) {
            // regain health
            health += regenerateSpeed * Time.deltaTime;
            if (health > initialHealth) {
                health = initialHealth;
            }
        }

        healthBar.SetPosition(0, transform.position + new Vector3(-initialHealth / 4f, 0f, 4f));
        float fill = Mathf.Max(0f, health / 2f);
        healthBar.SetPosition(1, healthBar.GetPosition(0) + new Vector3(fill, 0, 0));
    }

	void OnTriggerEnter(Collider other){
        if (nextTarget == path.Count - 1 && other.gameObject.tag == "HomeBase"){
			FindObjectOfType<GameManager>().TakeDamage(1);
            FindObjectOfType<GameManager>().GainPoints(-pointVal); // no points gained if self destructed
            StartCoroutine(SelfDestruct());
		}
	}


	public void FaceDirection(Vector3 direction){
        //float angle = GameManager.GetAngleFromVector(direction) + 90;
        //transform.eulerAngles = new Vector3(0, angle, 0);
        Vector3 lookPos = new Vector3(direction.x, transform.position.y, direction.z);
        transform.LookAt(lookPos);
	}


	public IEnumerator Move(){
        if (nextTarget < path.Count && GetComponent<MeshRenderer>().enabled) {
            float currentDuration = 0f;
            Vector3 targetLocation = path[nextTarget];
            float moveSpeed = distancePerMove / moveDuration;
            Vector3 moveDirection = (targetLocation - transform.position);

            if (regenerate && currentRegenerateDelay > 0) {
                currentRegenerateDelay--;
            }

            if (slowCounter > 0) {
                slowCounter--;
                moveSpeed = moveSpeed * 2f / 3f;
            }

            // if we're still ascending out of the spawn point, use the set speed and face up
            if (nextTarget == 0) {
                float angle = 90f + GameManager.GetAngleFromVector(transform.position);
                transform.eulerAngles = new Vector3(-90, 0, angle);
                ascending = true;
            }
            else {
                FaceDirection(moveDirection);
                ascending = false;
            }

            while (currentDuration <= moveDuration) {
                float speedRatio = Mathf.Pow(1f - (currentDuration / moveDuration), 3f);
                float moveAmount = (ascending ? 60 : moveSpeed) * speedRatio * Time.deltaTime;
                travelDist += moveAmount;

                transform.position += moveDirection.normalized * moveAmount;
                if (Vector3.Distance(transform.position, targetLocation) < 0.5f && (nextTarget + 1) < path.Count) {
                    print("transform is at " + transform.position + " and target is at " + targetLocation + " and distance is " + Vector3.Distance(transform.position, targetLocation));
                    targetLocation = path[++nextTarget];
                }

                currentDuration += Time.deltaTime;
                yield return null;
            }
        }
	}


	// called by other functions to damage this enemy. destroys this enemy when appropriate
	public void TakeDamage(float i){
        if (!ascending) {
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


    // slows the enemy for 2 movement cycles
    public void Slow() {
        slowCounter = 2;
    }


	// destroy this object and play the appropriate animation
	private IEnumerator SelfDestruct(){
		FindObjectOfType<GameManager>().GainPoints(pointVal);
        FindObjectOfType<GameManager>().GainMoney(moneyDropped);

        // show moneyText
        transform.Find("MoneyText").localEulerAngles = new Vector3(90, -transform.localEulerAngles.y, 0);
        transform.Find("MoneyText").gameObject.SetActive(true);

        ParticleSystem ps = transform.GetChild(0).GetComponent<ParticleSystem>();
		ps.Play();
		GetComponent<MeshRenderer>().enabled = false;
        healthBar.enabled = false;
        FindObjectOfType<EnemyManager>().PlayDestroyEnemySound();

		while (ps.isPlaying){
			yield return null;
		}

		FindObjectOfType<EnemyManager>().DestroyEnemy(this.gameObject);
	}


    public void SetPath(List<Vector3> input, float heightOffset) {
        nextTarget = 0;
        ascending = true;
        path = new List<Vector3>();

        // randomize the path a little
        float offsetRange = 1f;
        Vector3 offset = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange));
        for (int i = 0; i < input.Count; i++) {
            path.Add(input[i] + offset + new Vector3(0, heightOffset, 0));
        }
    }

    public float GetTravelDist() {
        return travelDist;
    }
}
