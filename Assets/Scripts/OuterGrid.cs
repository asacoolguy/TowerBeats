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
        foreach (Transform t in transform.GetComponentInChildren<Transform>()) {
            octTowers.Add(t.gameObject);
            float scale = GetScaleByDistance(t.position);
            t.localScale = new Vector3(t.localScale.x, scale, t.localScale.z);
            octTowerScales.Add(scale);

            // make each transform a random type
            int i = Random.Range(0, bumpColors.Length);
            octTowerColorType.Add(i);
        }

        spectrum = FindObjectOfType<GameManager>().GetComponent<SpectrumData>();
        spectrumValues = new float[bumpColors.Length];

        diffScale = maxScale - minScale;
        diffGlowPow = maxGlowPow - minGlowPow;
        diffGlowStr = maxTextureStr - minTextreStr;
    }

	

	void Update () {
        // respond to spectrum
        spectrum.GetOutputSpectrum(spectrumValues);
        

        for(int i = 0; i < octTowers.Count; i++) {
            float newScale = octTowerScales[i] * (1 + spectrumValues[octTowerColorType[i]]);
            // change the scale and location accordingly
            octTowers[i].transform.localScale = new Vector3(octTowers[i].transform.localScale.x,
                                                          newScale,
                                                          octTowers[i].transform.localScale.z);
            octTowers[i].transform.localPosition = new Vector3(octTowers[i].transform.localPosition.x,
                                                             octTowers[i].transform.localScale.y,
                                                             octTowers[i].transform.localPosition.z);
            // make the material brighter/darker accordingly
            Material mat = octTowers[i].GetComponent<MeshRenderer>().material;
            mat.SetFloat("_MKGlowTexStrength", Mathf.Clamp(minTextreStr + diffGlowStr * spectrumValues[octTowerColorType[i]] * 2, minTextreStr, maxTextureStr));
            mat.SetFloat("_MKGlowPower", Mathf.Clamp(minGlowPow + diffGlowPow * spectrumValues[octTowerColorType[i]], minGlowPow, maxGlowPow));
            Color newMainColor = Color.Lerp(defaultMainColor, bumpColors[octTowerColorType[i]], 5 * spectrumValues[octTowerColorType[i]]);
            mat.SetColor("_Color", newMainColor);
            Color newTextureColor = Color.Lerp(defaultTextureColor, bumpColors[octTowerColorType[i]], spectrumValues[octTowerColorType[i]] * 5);
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
            finalScale *= Random.Range(0.8f, 1.2f);
        }

        return finalScale;
    }
}
