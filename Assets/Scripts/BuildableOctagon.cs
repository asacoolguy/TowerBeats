using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Controls behavior of grid units where towers could be guild
    Has a selector to detect mouse hover selection
    Keeps internal states to correctly respond to mouse clicks
 */ 

public class BuildableOctagon : MonoBehaviour {
    private enum GridStatus {lowering, raising, immoble};
    private GridStatus status;

    public float pulseTime;
    private float currentPulseTime;
    private int pulseDirection;

    public float maxScale, maxYPos;
    private float minScale, minYPos;
    public float moveTime;
    private float currentMoveTime;

    private GameObject selector; // a collection of colliders that will allow this tower to be selected
    private GameObject buildPanel; // a GUI for building towers on this GridTower;
    private Material mat;


    void Awake () {
        selector = transform.Find("Selector").gameObject;
        selector.SetActive(true);

        currentPulseTime = 0f;
        pulseDirection = 1;
        minScale = transform.localScale.y;
        minYPos = transform.localPosition.y;
        currentMoveTime = 0f;

        mat = GetComponent<MeshRenderer>().material;

        status = GridStatus.immoble;
    }
	

	void Update () {
        // make the color pulsate
        currentPulseTime += pulseDirection * Time.deltaTime;
        if (currentPulseTime > pulseTime) {
            currentPulseTime = pulseTime;
            pulseDirection *= -1;
        } 
        else if(currentPulseTime < 0) {
            currentPulseTime = 0;
            pulseDirection *= -1;
        }
        float str = GameManager.SmoothStep(0, 10, currentPulseTime / pulseTime);
        mat.SetFloat("_MKGlowTexStrength", str);

        // raise or lower the tower
        if (status != GridStatus.immoble) {
            if (status == GridStatus.lowering) {
                currentMoveTime -= Time.deltaTime;
                if (currentMoveTime < 0) {
                    currentMoveTime = 0;
                    status = GridStatus.immoble;
                }
            }
            else {
                currentMoveTime += Time.deltaTime;
                if (currentMoveTime > moveTime) {
                    currentMoveTime = moveTime;
                    status = GridStatus.immoble;
                }                
            }

            float t = currentMoveTime / moveTime;
            float yScale = GameManager.SmoothStep(minScale, maxScale, t);
            float yPos = GameManager.SmoothStep(minYPos, maxYPos, t);
            transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);
        }
        
    }

    public void RaiseTower() {
        status = GridStatus.raising;
    }

    public void LowerTower() {
        status = GridStatus.lowering;
    }
}
