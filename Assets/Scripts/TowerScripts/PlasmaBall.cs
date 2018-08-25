using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaBall : MonoBehaviour {
    public AnimationCurve moveSpeedCurve;
    private Vector3 originalPosition, targetPosition;
    private GameObject target;
    private ParticleSystem explosion;
    private TowerAoE area;
    private bool moving;
    private float moveDuration, currentMoveDuration;
    private float attackPower;

	private void Awake () {
        area = transform.Find("Area").GetComponent<TowerAoE>();
        explosion = transform.Find("Explosion").GetComponent<ParticleSystem>();
        moving = false;
        currentMoveDuration = 0;
	}
	

	private void Update () {
		if (moving) {
            if (target != null) {
                targetPosition = target.transform.position;
            }

            if (currentMoveDuration > moveDuration) {
                StartCoroutine(Explode());
                return;
            }

            currentMoveDuration += Time.deltaTime;
            float t = Mathf.Clamp(currentMoveDuration / moveDuration, 0, 1);
            transform.position = Vector3.Lerp(originalPosition, targetPosition, moveSpeedCurve.Evaluate(t));
        }
	}


    void OnTriggerEnter(Collider other) {
        if (moving && other.gameObject == target) {
            StartCoroutine(Explode());
        }
    }


    public void SetTarget(GameObject enemy, float duration, float power) {
        target = enemy;
        targetPosition = target.transform.position;
        originalPosition = transform.position;
        moveDuration = duration;
        attackPower = power;
        moving = true;
    }


    private IEnumerator Explode() {
        moving = false;
        GetComponent<MeshRenderer>().enabled = false;

        // damage enemies in area
        foreach (Enemy e in area.enemiesInRange) {
            if (e != null && e.IsVulnerable()) {
                e.TakeDamage(attackPower);
            }
        }

        // play explosion effect
        explosion.gameObject.SetActive(true);
        explosion.Play();

        yield return new WaitForSeconds(2);

        Destroy(gameObject);
    }
}
