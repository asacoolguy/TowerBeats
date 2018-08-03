using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Controls the behavior of outer grid towers. 
    Outergrid towers respond to spectrum data and changes height based on distance from center
 */ 


public class OuterGrid : MonoBehaviour {
    public float minDistFromCenter, maxDistFromCenter;
    public float minScale, maxScale;
    public float yPosOffset;

    private List<GameObject> octTowers;
    private List<float> octTowerScales;
    private List<int> octTowerColorType;


    // color parameters
    public Color defaultMainColor;
    public Color defaultTextureColor;
    public Color[] bumpColors;
    public float minBumpScale, maxBumpScale;
    public float minTextreStr, maxTextureStr;
    public float minGlowPow, maxGlowPow;
    private float diffScale;
    private float diffGlowStr;
    private float diffGlowPow;

    // spectrum parameters
    private SpectrumData spectrum;
    private float[] spectrumValues;


    void Awake () {
        octTowers = new List<GameObject>();
        octTowerScales = new List<float>();
        octTowerColorType = new List<int>();
        int i = 0;
        foreach (Transform t in transform.GetComponentInChildren<Transform>()) {
            octTowers.Add(t.gameObject);

            float scale = GetScaleByDistance(t.position);
            t.localScale = new Vector3(t.localScale.x, scale, t.localScale.z);
            t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y + yPosOffset, t.localPosition.z);
            octTowerScales.Add(scale);

            // make each transform a random type
            // random
            //int i = Random.Range(0, bumpColors.Length);
            //i = ++i % bumpColors.Length;

            // distance based
            float dist = new Vector3(t.localPosition.x, 0, t.localPosition.z).magnitude;
            i = dist < 75 ? 0 : dist < 85 ? 1 : 2;

            // angle based
            //float angle = GameManager.GetAngleFromVector(t.localPosition);
            //if (angle < 0) angle += 360;
            //angle %= 60;
            //i = angle < 20 ? 0 : angle < 40 ? 1 : 2;

            octTowerColorType.Add(i);
        }

        spectrum = FindObjectOfType<GameManager>().GetComponent<SpectrumData>();
        spectrumValues = new float[bumpColors.Length];

        diffScale = maxBumpScale - minBumpScale;
        diffGlowPow = maxGlowPow - minGlowPow;
        diffGlowStr = maxTextureStr - minTextreStr;
    }

	

	void Update () {
        // respond to spectrum
        spectrum.GetOutputSpectrum(spectrumValues);
        

        for(int i = 0; i < octTowers.Count; i++) {
            //float newScale = octTowerScales[i] * (1 + spectrumValues[octTowerColorType[i]]);
            float newScale = octTowerScales[i] * (1 + diffScale * spectrumValues[octTowerColorType[i]]);
            // change the scale and location accordingly
            octTowers[i].transform.localScale = new Vector3(octTowers[i].transform.localScale.x,
                                                          newScale,
                                                          octTowers[i].transform.localScale.z);
            // make the material brighter/darker accordingly
            Material mat = octTowers[i].GetComponent<MeshRenderer>().material;
            mat.SetFloat("_MKGlowTexStrength", Mathf.Clamp(minTextreStr + diffGlowStr * spectrumValues[octTowerColorType[i]] * 2, minTextreStr, maxTextureStr));
            mat.SetFloat("_MKGlowPower", Mathf.Clamp(minGlowPow + diffGlowPow * spectrumValues[octTowerColorType[i]], minGlowPow, maxGlowPow));

            float t = spectrumValues[octTowerColorType[i]] * 5;
            //Color newMainColor = Color.Lerp(defaultMainColor, bumpColors[octTowerColorType[i]], t);
            Color newMainColor = GameManager.SmoothStep(defaultMainColor, bumpColors[octTowerColorType[i]], t);
            mat.SetColor("_Color", newMainColor);
            //Color newTextureColor = Color.Lerp(defaultTextureColor, bumpColors[octTowerColorType[i]], t);
            Color newTextureColor = GameManager.SmoothStep(defaultMainColor, bumpColors[octTowerColorType[i]], t);
            mat.SetColor("_MKGlowTexColor", newTextureColor);
        }
    }


    // sets the scale according to distance from center
    float GetScaleByDistance(Vector3 pos) {
        float finalScale = transform.localScale.y;
        float distanceSqrd = pos.x * pos.x + pos.z * pos.z;
        float minDistSqrd = minDistFromCenter * minDistFromCenter;
        float maxDistSqrd = maxDistFromCenter * maxDistFromCenter;

        if (distanceSqrd < minDistSqrd) {
            finalScale = minScale;
        }
        else if (distanceSqrd > maxDistSqrd) {
            finalScale = maxScale;
        }
        else {
            finalScale = minScale + (maxScale - minScale) * ((distanceSqrd - minDistSqrd) / (maxDistSqrd - minDistSqrd));
            //finalScale = GameManager.SmoothStep(minScale, maxScale, (distanceSqrd - minDistSqrd) / (maxDistSqrd - minDistSqrd));
            finalScale *= Random.Range(0.85f, 1.15f);
        }

        return finalScale;
    }
}
