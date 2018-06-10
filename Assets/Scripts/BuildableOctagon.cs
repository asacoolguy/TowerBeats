using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Controls behavior of grid units where towers could be guild
    Has a selector to detect mouse hover selection
    Keeps internal states to correctly respond to mouse clicks
 */
 

public class BuildableOctagon : Octagon {
    private GameObject builtTower;
    
    private float texStrDecaySpeed, glowPowDecaySpeed;

    public float raisedTexStr, raisedGlowPow;
    private float loweredTexStr, loweredGlowPow, tempTexStr, tempGlowPow;

    public AnimationCurve flyInCurve;
    public float flyInHeight;
    public AudioClip flyInSound;

    private GameObject selector; // a collection of colliders that will allow this tower to be selected
    private GameObject buildPanel; // a GUI for building towers on this BuildableOct;
    private Material mat;


    private new void Awake () {
        base.Awake();
        mat = GetComponent<MeshRenderer>().material;

        selector = transform.Find("Selector").gameObject;
        selector.SetActive(true);
        
        loweredTexStr = mat.GetFloat("_MKGlowTexStrength");
        loweredGlowPow = mat.GetFloat("_MKGlowPower");
    }


    private void Start() {
        float rotationTime = FindObjectOfType<Scanner>().GetRotationTime();
        texStrDecaySpeed = (raisedTexStr - loweredTexStr) / rotationTime * 2;
        glowPowDecaySpeed = (raisedGlowPow - loweredGlowPow) / rotationTime * 2;
    }


    private new void Update () {
        base.Update();

        float t = currentMoveTime / moveTime;

        // show AOEIndicator if necessary
        if (IsBuiltOn()){
            bool highlighted = (status == OctagonStatus.raising || status == OctagonStatus.raised);
            builtTower.transform.Find("AOEIndicator").gameObject.SetActive(highlighted);

            if (highlighted) {
                // otherwise link color to height using tempTexStr and tempGlowPow
                float texStr = GameManager.SmoothStep(tempTexStr, raisedTexStr, t);
                float glowPow = GameManager.SmoothStep(tempGlowPow, raisedGlowPow, t);
                mat.SetFloat("_MKGlowTexStrength", texStr);
                mat.SetFloat("_MKGlowPower", glowPow);
            }
            else {
                // decay color if platform is built on and not selected
                float newTexStr = mat.GetFloat("_MKGlowTexStrength") - (Time.deltaTime * texStrDecaySpeed);
                float newGlowPow = mat.GetFloat("_MKGlowPower") - (Time.deltaTime * glowPowDecaySpeed);
                mat.SetFloat("_MKGlowTexStrength", Mathf.Clamp(newTexStr, loweredTexStr, raisedTexStr));
                mat.SetFloat("_MKGlowPower", Mathf.Clamp(newGlowPow, loweredGlowPow, raisedGlowPow));
            }
        }
        else {
            // link color to height
            float texStr = GameManager.SmoothStep(loweredTexStr, raisedTexStr, t);
            float glowPow = GameManager.SmoothStep(loweredGlowPow, raisedGlowPow, t);
            mat.SetFloat("_MKGlowTexStrength", texStr);
            mat.SetFloat("_MKGlowPower", glowPow);
        }
    }


    public override void RaiseOctagon() {
        status = OctagonStatus.raising;
        if (IsBuiltOn()) {
            tempTexStr = mat.GetFloat("_MKGlowTexStrength");
            tempGlowPow = mat.GetFloat("_MKGlowPower");
        }
    }

    

    public bool IsBuiltOn() {
        return builtTower != null;
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


    public IEnumerator FlyIn(float duration) {
        GetComponent<AudioSource>().PlayOneShot(flyInSound);

        float t = 0;
        float endingPos = transform.position.y;
        float startingPos = endingPos + flyInHeight;
        while (t < duration) {
            float newY = startingPos - flyInHeight * flyInCurve.Evaluate(t / duration);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, endingPos, transform.position.z);
    }

    public void SetBuiltTower(GameObject tower) {
        if (IsBuiltOn()) {
            Destroy(builtTower);
        }

        builtTower = tower;
    }
}
