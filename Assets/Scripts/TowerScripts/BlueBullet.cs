using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Upon creation, travels towards the target location
/// </summary>
public class BlueBullet : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine(FadeBulletTrail(0.6f));
	}

	private IEnumerator FadeBulletTrail(float time){
		Material mat = this.GetComponent<LineRenderer>().material;
		float t = 0f;
		float decaySpeed = mat.color.a / time;
		while (t < time){
			Color c = mat.color;
			mat.color = new Color(c.r, c.g, c.b, c.a -= decaySpeed * Time.deltaTime);
			t+= Time.deltaTime;
			yield return null;
		}

		Destroy(this.gameObject);
		yield break;
	}
}
