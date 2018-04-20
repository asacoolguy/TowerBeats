using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Controls behavior of grid units where towers could be guild
    Has a selector to detect mouse hover selection
    Keeps internal states to correctly respond to mouse clicks
 */ 

public class BuildableOctagon : MonoBehaviour {
    private enum OctagonStatus {lowering, raising, immoble, builtOn};
    private OctagonStatus status;
    private bool selected = false;

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

        pulseTime = FindObjectOfType<MusicDatabase>().scannerClips[0].length;
        currentPulseTime = 0f;
        pulseDirection = 1;


        minScale = transform.localScale.y;
        minYPos = transform.localPosition.y;
        currentMoveTime = 0f;

        mat = GetComponent<MeshRenderer>().material;

        status = OctagonStatus.immoble;
    }
	

	void Update () {
        // make the color pulsate if the tower is not selected
        if (!selected) {
            currentPulseTime += pulseDirection * Time.deltaTime;
            if (currentPulseTime > pulseTime) {
                currentPulseTime = pulseTime;
                pulseDirection *= -1;
            }
            else if (currentPulseTime < 0) {
                currentPulseTime = 0;
                pulseDirection *= -1;
            }
            float t = currentPulseTime / pulseTime;
            float str = GameManager.SmoothStep(2, 6, t);
            float glow = GameManager.SmoothStep(0f, 0.3f, t);
            mat.SetFloat("_MKGlowTexStrength", str);
            mat.SetFloat("_MKGlowPower", glow);
        }

        // raise or lower the tower
        if (status != OctagonStatus.immoble) {
            if (status == OctagonStatus.lowering) {
                currentMoveTime -= Time.deltaTime;
                if (currentMoveTime < 0) {
                    currentMoveTime = 0;
                    status = OctagonStatus.immoble;
                }
            }
            else if (status == OctagonStatus.raising) {
                currentMoveTime += Time.deltaTime;
                if (currentMoveTime > moveTime) {
                    currentMoveTime = moveTime;
                    if (status == OctagonStatus.raising) {
                        status = OctagonStatus.immoble;
                    }
                }                
            }

            float t = currentMoveTime / moveTime;
            float yScale = GameManager.SmoothStep(minScale, maxScale, t);
            float yPos = GameManager.SmoothStep(minYPos, maxYPos, t);
            transform.localScale = new Vector3(transform.localScale.x, yScale, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);
        }
        
    }

    public void RaiseOctagon() {
        status = OctagonStatus.raising;
    }

    public void LowerOctagon() {
        if (!selected && !IsBuiltOn()) {
            status = OctagonStatus.lowering;
        }
    }

    public void BuiltOnOctagon() {
        status = OctagonStatus.builtOn;
    }

    public bool IsBuiltOn() {
        return status == OctagonStatus.builtOn;
    }

    public void SelectOctagon(bool b) {
        if (b) {
            selected = true;
            mat.SetFloat("_MKGlowTexStrength", 10);
            mat.SetFloat("_MKGlowPower", 0.6f);
            RaiseOctagon();
        }
        else {
            selected = false;
            mat.SetFloat("_MKGlowTexStrength", 6);
            mat.SetFloat("_MKGlowPower", 0.3f);
            LowerOctagon();
        }
    }
}
