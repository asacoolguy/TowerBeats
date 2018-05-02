using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Controls behavior of grid units where towers could be guild
    Has a selector to detect mouse hover selection
    Keeps internal states to correctly respond to mouse clicks
 */ 

public class BuildableOctagon : MonoBehaviour {
    private enum OctagonStatus {lowering, raising, lowered, raised};
    private GameObject builtTower;
    private OctagonStatus status;
    private bool selected = false;

    private float texStrDecaySpeed, glowPowDecaySpeed;

    public float raisedYPos, idleFloatRange, raisedTexStr, raisedGlowPow;
    private float loweredYPos, loweredTexStr, loweredGlowPow;
    public float moveTime, idleFloatTime;
    private float currentMoveTime, currentIdleFloatTime;
    private int idleFloatDirection;


    private GameObject selector; // a collection of colliders that will allow this tower to be selected
    private GameObject buildPanel; // a GUI for building towers on this BuildableOct;
    private Material mat;


    private void Awake () {
        mat = GetComponent<MeshRenderer>().material;

        selector = transform.Find("Selector").gameObject;
        selector.SetActive(true);

        currentIdleFloatTime = Random.Range(-idleFloatTime, idleFloatTime);
        idleFloatDirection = 1;

        loweredYPos = transform.localPosition.y;
        loweredTexStr = mat.GetFloat("_MKGlowTexStrength");
        loweredGlowPow = mat.GetFloat("_MKGlowPower");
        currentMoveTime = 0f;

        status = OctagonStatus.lowered;
    }

    private void Start() {
        float rotationTime = FindObjectOfType<Scanner>().GetRotationTime();
        texStrDecaySpeed = (raisedTexStr - loweredTexStr) / rotationTime * 2;
        glowPowDecaySpeed = (raisedGlowPow - loweredGlowPow) / rotationTime * 2;
    }

    private void Update () {
        // if there's a tower on this platform, decay its color
        if (builtTower != null) {
            float newTexStr = mat.GetFloat("_MKGlowTexStrength") - (Time.deltaTime * texStrDecaySpeed);
            float newGlowPow = mat.GetFloat("_MKGlowPower") - (Time.deltaTime * glowPowDecaySpeed);

            mat.SetFloat("_MKGlowTexStrength", Mathf.Clamp(newTexStr , loweredTexStr, raisedTexStr));
            mat.SetFloat("_MKGlowPower", Mathf.Clamp(newGlowPow , loweredGlowPow, raisedGlowPow));
        }

        // raise or lower the tower
        if (status == OctagonStatus.lowering) {
            currentMoveTime -= Time.deltaTime;
            if (currentMoveTime < 0) {
                currentMoveTime = 0;
                status = OctagonStatus.lowered;
            }
        }
        else if (status == OctagonStatus.raising) {
            currentMoveTime += Time.deltaTime;
            if (currentMoveTime > moveTime) {
                currentMoveTime = moveTime;
                if (status == OctagonStatus.raising) {
                    status = OctagonStatus.raised;
                }
            }    
        }

        float t = currentMoveTime / moveTime;
        float yPos = GameManager.SmoothStep(loweredYPos, raisedYPos, t);
        float texStr = GameManager.SmoothStep(loweredTexStr, raisedTexStr, t);
        float glowPow = GameManager.SmoothStep(loweredGlowPow, raisedGlowPow, t);
        transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);
        mat.SetFloat("_MKGlowTexStrength", texStr);
        mat.SetFloat("_MKGlowPower", glowPow);

        // show AOEIndicator if necessary
        if (IsBuiltOn()){
            bool showAOE = (status == OctagonStatus.raising || status == OctagonStatus.raised);
            builtTower.transform.Find("AOEIndicator").gameObject.SetActive(showAOE);
        }
    }

    public void RaiseOctagon() {
        status = OctagonStatus.raising;
    }

    public void LowerOctagon() {
        if (!selected) {
            status = OctagonStatus.lowering;
        }
    }

    public bool IsBuiltOn() {
        return builtTower != null;
    }

    public void SelectOctagon(bool b) {
        if (b) {
            selected = true;
            RaiseOctagon();
        }
        else {
            selected = false;
            LowerOctagon();
        }
    }

    public void SetColor(Color c) {
        mat.SetColor("_Color", c);
        mat.SetColor("_MKGlowColor", c);
        mat.SetColor("_MKGlowTexColor", c);
    }

    public void BoostColor() {
        if (IsBuiltOn()) {
            mat.SetFloat("_MKGlowTexStrength", raisedTexStr);
            mat.SetFloat("_MKGlowPower", raisedGlowPow);
        }
    }


    public void BoostSize(float maxYScale, float popTime) {
        StartCoroutine(PopSize(maxYScale, popTime));
    }


    public IEnumerator PopSize(float maxYScale, float popTime) {
        float currentTime = 0f;
        float currentYPos = transform.localPosition.y;
        while (currentTime < popTime) {
            float newYScale = GameManager.EaseOut(1, maxYScale, currentTime / popTime);
            float newYPos = GameManager.EaseOut(currentYPos, currentYPos + maxYScale - 1, currentTime / popTime);
            transform.localScale = new Vector3(transform.localScale.x, newYScale, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, newYPos, transform.localPosition.z);
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentTime = popTime;
        while (currentTime > 0) {
            float newYScale = GameManager.EaseOut(1, maxYScale, currentTime / popTime);
            float newYPos = GameManager.EaseOut(currentYPos, currentYPos + maxYScale - 1, currentTime / popTime);
            transform.localScale = new Vector3(transform.localScale.x, newYScale, transform.localScale.z);
            transform.localPosition = new Vector3(transform.localPosition.x, newYPos, transform.localPosition.z);
            currentTime -= Time.deltaTime;
            yield return null;
        }
    }


    public void SetBuiltTower(GameObject tower) {
        if (IsBuiltOn()) {
            Destroy(builtTower);
        }

        builtTower = tower;
    }
}
