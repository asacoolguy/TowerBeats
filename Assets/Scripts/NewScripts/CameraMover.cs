/// <summary>
/// This is a utility script that ensures moves camera smoothly from level to level
/// based on mouse wheel scrolls. It also handles rotating the camera around the tower
/// when the user click-drags on the screen
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour {
    // change switch variables
    private GameObject[] levels;
    public AnimationCurve yCurve, zCurve;
    private Vector3 positionOffset;
    private bool isMoving = false;
    public float scrollTime = 0.5f;

    // rotation variables
    public GameObject scene; // really the camera rotates the scene instead of around it
    public float angleRotateSpeed;
    private Vector3 lastMousePosition;
    public float currentAngle;
    private float targetAngle;


    void Start () {
        levels = FindObjectOfType<GameManagerNew>().levels;
        positionOffset = transform.localPosition - levels[0].transform.position;

        // initialize rotations
        currentAngle = scene.transform.eulerAngles.y;
        targetAngle = currentAngle;
	}


	void Update () {
        // --- handle click & drag rotation ---
        if (Input.GetMouseButtonDown(0)) { // record position when mouse is down
            lastMousePosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(0)) {
            // record movement and update the mouse position
            float movement = Input.mousePosition.x - lastMousePosition.x;
            lastMousePosition = Input.mousePosition;
            // calculate how much to rotate the camera
            targetAngle = targetAngle - movement / 4f;
        }
        // if we're not at targetAngle, rotate towards it
        if (Mathf.Abs(targetAngle - currentAngle) > 1) {
            float change = (targetAngle - currentAngle) * angleRotateSpeed * Time.deltaTime;
            currentAngle += change;
            CameraRotate(change);
        }
    }

    
    // helper function to move camera to a certain level
    public IEnumerator MoveToLevel(int target) {
        Vector3 start = transform.position;
        Vector3 destination = levels[target].transform.localPosition + positionOffset;
        float currentTime = 0f;
        Vector3 distance = new Vector3(0, destination.y - start.y, start.z - 40);
            ;
        isMoving = true;

        while (currentTime < scrollTime) {
            float t = currentTime / scrollTime;
            transform.position = start + new Vector3(distance.x,
                                                     distance.y * yCurve.Evaluate(t), 
                                                     distance.z * zCurve.Evaluate(t));
            currentTime += Time.deltaTime;
            yield return null;
        }

        transform.position = destination;
        isMoving = false;
    }


    Vector3 SmoothStepVector3(Vector3 start, Vector3 end, float t) {
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        return start + (end - start) * t;
    }


    // helper function to rotate towards a certain angle
    void CameraRotate(float angle) {
        scene.transform.localEulerAngles += new Vector3(0, angle, 0);
    }


    public bool IsMoving() {
        return isMoving;
    }
}
