using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Moves the camera about using wsad 
 */
public class CameraMover : MonoBehaviour {
    public float maxVertical, maxHorizontal;
    public float moveSpeed;
    public Vector3 gameWinPos;

	void Start () {	}
	

	void Update () {
        Vector3 deltaPos = Vector3.zero;
		if (Input.GetKey(KeyCode.W) && transform.position.z < maxVertical) {
            deltaPos += new Vector3(0, 0, moveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S) && transform.position.z > -maxVertical){
            deltaPos += new Vector3(0, 0, -moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A) && transform.position.x > -maxHorizontal) {
            deltaPos += new Vector3(- moveSpeed * Time.deltaTime, 0, 0);
        }
        else if (Input.GetKey(KeyCode.D) && transform.position.x < maxHorizontal) {
            deltaPos += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x + deltaPos.x, -maxHorizontal, maxHorizontal),
                                         transform.position.y,
                                         Mathf.Clamp(transform.position.z + deltaPos.z, -maxVertical, maxVertical));
    }


    public void ZoomOut(float duration) {
        StartCoroutine(GameWinZoomOut(duration));
    }

    private IEnumerator GameWinZoomOut(float duration) {
        float currentTime = 0f;
        Vector3 startPos = transform.position;

        while (currentTime < duration) {
            currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0, duration);
            transform.position = GameManager.SmoothStep(startPos, gameWinPos, currentTime / duration);
            yield return null;
        }
    }
}
