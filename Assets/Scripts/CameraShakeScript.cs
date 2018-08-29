using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeScript : MonoBehaviour {
	public float shakeIntensity;
	public float shakeDuration;

	public void ShakeCamera(){
		StartCoroutine(Shake(shakeIntensity, shakeDuration));
	}

    public void ShakeCamera(float intensity, float duration) {
        StartCoroutine(Shake(shakeIntensity * intensity, shakeDuration * duration));
    }

    public void ShakeCameraCustom(float intensity, float duration){
		StartCoroutine(Shake(intensity, duration));
	}

	IEnumerator Shake(float intensity, float duration){
		float countdown = Time.realtimeSinceStartup + duration;
		while (Time.realtimeSinceStartup < countdown){
			Vector2 rotation = Random.insideUnitCircle * intensity;
			//countdown = Mathf.Lerp(countdown, 0, Time.deltaTime);
			//countdown -= Time.deltaTime;

			transform.localRotation = Quaternion.Euler(new Vector3(rotation.x, rotation.y, 0));
			yield return null;
		}
		transform.localRotation = Quaternion.identity;
	}
}
