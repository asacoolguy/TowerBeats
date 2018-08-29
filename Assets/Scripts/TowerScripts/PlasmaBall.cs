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
    private float attackPower, splashDamageFactor;

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


    public void SetTarget(GameObject enemy, float duration, float power, float splashFactor) {
        target = enemy;
        targetPosition = target.transform.position;
        originalPosition = transform.position;
        moveDuration = duration;
        attackPower = power;
        splashDamageFactor = splashFactor;
        moving = true;
    }


    private IEnumerator Explode() {
        moving = false;
        GetComponent<MeshRenderer>().enabled = false;

        // damage target and splash damage enemies in area
        Enemy mainEnemy = null;
        if (target == null) {
            float minDist = float.PositiveInfinity;
            foreach (Enemy e in area.enemiesInRange) {
                if (e != null && e.IsVulnerable()) {
                    float tempDist = (e.transform.position - transform.position).magnitude;
                    if (tempDist < minDist) {
                        minDist = tempDist;
                        mainEnemy = e;
                    }
                }
            }
        }
        else {
            mainEnemy = target.GetComponent<Enemy>();
        }

        if (mainEnemy != null && mainEnemy.IsVulnerable()) {
            mainEnemy.TakeDamage(attackPower);
        }
        
        foreach (Enemy e in area.enemiesInRange) {
            if (e != null && e!= mainEnemy && e.IsVulnerable()) {
                e.TakeDamage(attackPower * splashDamageFactor);
            }
        }

        // play explosion effect
        explosion.gameObject.SetActive(true);
        explosion.Play();

        yield return new WaitForSeconds(2);

        Destroy(gameObject);
    }
}
