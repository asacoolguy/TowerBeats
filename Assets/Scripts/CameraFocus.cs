/// <summary>
/// This is a utility script that ensures the camera is focusing on a certain object.
/// also allows the camera to be dragged and zoomed 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocus : MonoBehaviour {
	// variables for zooming
	public GameObject target;
    public float maxZoomOut;
	public float maxZoomIn;
	public float zoomSpeed;
	private float targetZoom;

	// variables for rotating
	public float maxAngle = 30f;
	public float angleRotateSpeed;
	private Vector3 lastMousePosition;
	private float currentAngle;
	private float targetAngle;

	// variables for changing positions
	//public GameObject defaultLocation;
	//public GameObject pauseLocation;


	// Use this for initialization
	void Start () {
        // make sure camera is zoomedOut
        maxZoomOut = transform.position.y;
        maxZoomIn = maxZoomOut - 40f;
		targetZoom = maxZoomOut;

		// make sure the camera is at default rotation
		currentAngle = targetAngle = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		// --- handle zooming ---
		// update targetZoom if user scrolled
		if (Input.GetAxis("Mouse ScrollWheel") != 0){
			targetZoom = Mathf.Clamp(targetZoom + Input.GetAxis("Mouse ScrollWheel") * -zoomSpeed, 
				maxZoomIn, maxZoomOut);
		}
		// if we're not currently at targetZoom, go towards it
		if (Mathf.Abs(targetZoom - transform.position.y) > 1){
			float sign = Mathf.Sign(targetZoom - transform.position.y);
			cameraZoom(transform.position.y + sign * zoomSpeed * Time.deltaTime);
		}

		// --- handle click & drag ---
		if (Input.GetMouseButtonDown(0)){ // record position when mouse is down
			lastMousePosition = Input.mousePosition;
		}
		if (Input.GetMouseButton(0)){
			// record movement and update the mouse position
			float movement = Input.mousePosition.x - lastMousePosition.x;
			lastMousePosition = Input.mousePosition;
			// calculate how much to rotate the camera
			targetAngle = Mathf.Clamp(targetAngle + movement / 4f, 
				-maxAngle, maxAngle);
		}
		// if we're not at targetAngle, rotate towards it
		if (Mathf.Abs(targetAngle - currentAngle) > 1){
			float change = (targetAngle - currentAngle) * angleRotateSpeed * Time.deltaTime;
			currentAngle += change;
			cameraRotate(change);
		}

		// make sure we're looking at the target
		Vector3 difference = target.transform.position - transform.position;
		Vector3 upDirection = new Vector3(difference.x, 0, difference.z);
		transform.LookAt(target.transform, upDirection);
	}


	// helper function to zoom in and out the camera
	void cameraZoom(float zoom){
		transform.position = new Vector3(transform.position.x, zoom, transform.position.z);
	}


	// helper function to rotate towards a certain angle
	void cameraRotate(float angle){
		Vector3 rotation = new Vector3(transform.localEulerAngles.x, angle, transform.localEulerAngles.z);
		transform.position = Quaternion.Euler(rotation) * 
			(transform.position - target.transform.position) + target.transform.position;
	}
}
