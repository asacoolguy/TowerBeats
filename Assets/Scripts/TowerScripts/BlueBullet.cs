using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Upon creation, travels towards the target location
/// </summary>
public class BlueBullet : MonoBehaviour {
	public GameObject target;
	public float attackPower = 1;
	public float speed = 5;
	public float rotationSpeed = 5;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (target == null){
			print("error: no target found for blue bullet");
			// fly towards the target destination first
			Destroy(this.gameObject);
		}
		else{
			// move towards the target direction
			Vector3 direction = (target.transform.position - transform.position).normalized;
			transform.position += direction * speed * Time.deltaTime;

			// rotate to face the target direction
			Vector3 facingDirection = Vector3.RotateTowards(transform.forward, direction, rotationSpeed * Time.deltaTime, 0f);
			transform.rotation = Quaternion.LookRotation(facingDirection);
		}
	}

	void OnTriggerEnter(Collider other){
		if (other.gameObject.tag == "Enemy"){
			other.gameObject.GetComponent<Enemy>().TakeDamage(attackPower * FindObjectOfType<GameManager>().attackPowerBonus);
			Destroy(this.gameObject);
		}
	}
}
