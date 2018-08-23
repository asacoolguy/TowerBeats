using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Controls behavior of OctPlatform where towers could be guild
    Has a selector to detect mouse hover selection
    Keeps internal states to correctly respond to mouse clicks
 */
 

public class TowerPlatform : OctPlatform {
    private GameObject builtTower;
    [ColorUsageAttribute(true,true)]
    public Color builtColor;
    private Color originalColor;
    private float brightnessDecaySpeed;

    public float raisedTexStr, raisedGlowPow;
    //private float loweredEmissionStr, tempTexStr, tempGlowPow;
    private Vector3 currentHSVColor;
    private float loweredBrightness, raisedBrightness;

    public AnimationCurve flyInCurve;
    public float flyInHeight, fallOffHeight;
    public AudioClip flyInSound;

    private GameObject selector; // a collection of colliders that will allow this tower to be selected
    private GameObject buildPanel; // a GUI for building towers on this BuildableOct;
    private Material mat;


    private new void Awake () {
        base.Awake();
        mat = GetComponent<MeshRenderer>().material;

        selector = transform.Find("Selector").gameObject;
        selector.SetActive(true);

        originalColor = mat.GetColor("_EmissionColor");
        float h, s, v;
        Color.RGBToHSV(originalColor, out h, out s, out v);
        loweredBrightness = v;
        raisedBrightness = v * 1.5f;
    }


    private void Start() {
        float rotationTime = FindObjectOfType<Scanner>().GetRotationTime();
        brightnessDecaySpeed = (raisedBrightness - loweredBrightness) / rotationTime;
    }


    private new void Update () {
        base.Update();

        float newV = GameManager.SmoothStep(loweredBrightness, raisedBrightness, currentMoveTime / moveTime);

        if (IsBuiltOn()){
            bool highlighted = (status == PlatformStatus.raising || status == PlatformStatus.raised);

            // show AOEIndicator if necessary
            builtTower.transform.Find("AOEIndicator").gameObject.SetActive(highlighted);

            if (!highlighted) {
                newV = currentHSVColor.z - (brightnessDecaySpeed * Time.deltaTime);
                newV = Mathf.Clamp(newV, loweredBrightness, raisedBrightness);
            }
        }

        currentHSVColor = new Vector3(currentHSVColor.x, currentHSVColor.y, newV);
        mat.SetColor("_EmissionColor", Color.HSVToRGB(currentHSVColor.x, currentHSVColor.y, currentHSVColor.z));
    }


    public override void RaisePlatform() {
        status = PlatformStatus.raising;
    }

    

    public bool IsBuiltOn() {
        return builtTower != null;
    }


    public void SetColor(Color c) {
        mat.SetColor("_EmissionColor", c);
        float h, s, v;
        Color.RGBToHSV(c, out h, out s, out v);
        currentHSVColor = new Vector3(h, s, v);
    }


    public void BoostColor() {
        if (IsBuiltOn()) {
            currentHSVColor = new Vector3(currentHSVColor.x, currentHSVColor.y, raisedBrightness);
            mat.SetColor("_EmissionColor", Color.HSVToRGB(currentHSVColor.x, currentHSVColor.y, currentHSVColor.z));
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
        if (!IsBuiltOn()) {
            builtTower = tower;
            SetColor(builtColor);
        }
    }


    public void RemoveBuiltTower() {
        if (IsBuiltOn()) {
            builtTower = null;
            SetColor(originalColor);
        }
    }


    public IEnumerator FallOff() {
        SetColor(Color.gray);

        Vector2 r = Random.insideUnitCircle;
        Vector3 randomAxis = new Vector3(r.x, 0, r.y);
        Vector3 initialPos = transform.position;
        float t = 0;
        Vector3 speed = new Vector3(0, -4.9f, 0);
        while (t < 3) {
            t += Time.deltaTime;
            transform.position = initialPos + speed * t * t * t;
            transform.rotation *= Quaternion.AngleAxis(20 * Time.deltaTime, randomAxis);
            yield return null;
        }

        //this.gameObject.SetActive(false);
        //transform.position = new Vector3(transform.position.x, flyInHeight, transform.position.z);
        //transform.localEulerAngles = new Vector3(0, 22.5f, 0);
        Destroy(this.gameObject);
    }

    
    public BasicTower getBuiltTower() {
        if (IsBuiltOn()) {
            return builtTower.GetComponent<BasicTower>();
        }

        return null;
    }
}
