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
    private List<int> octTowerSpectrumType;
    private Vector3 currentHSVColor;

    // color parameters
    public float minBumpScale, maxBumpScale;
    private float maxBrightness;
    private float diffScale, diffBrightness;

    // spectrum parameters
    public int spectrumNumber;
    private SpectrumData spectrum;
    private float[] spectrumValues;


    void Awake () {
        octTowers = new List<GameObject>();
        octTowerScales = new List<float>();
        octTowerSpectrumType = new List<int>();

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
            int i = dist < 75 ? 2 : dist < 85 ? 1 : 0;

            // angle based
            //float angle = GameManager.GetAngleFromVector(t.localPosition);
            //if (angle < 0) angle += 360;
            //angle %= 60;
            //i = angle < 20 ? 0 : angle < 40 ? 1 : 2;

            octTowerSpectrumType.Add(i);
        }

        Color currentColor = GetComponentInChildren<MeshRenderer>().material.GetColor("_EmissionColor");
        float h, s, v;
        Color.RGBToHSV(currentColor, out h, out s, out v);
        currentHSVColor = new Vector3(h, s, v);
        maxBrightness = v;

        spectrum = FindObjectOfType<GameManager>().GetComponent<SpectrumData>();
        spectrumValues = new float[spectrumNumber];

        diffScale = maxBumpScale - minBumpScale;
        diffBrightness = maxBrightness;
    }

	

	void Update () {
        // respond to spectrum
        spectrum.GetOutputSpectrum(spectrumValues);

        for(int i = 0; i < octTowers.Count; i++) {
            float newScale = octTowerScales[i] * (1 + diffScale * spectrumValues[octTowerSpectrumType[i]]);
            // change the scale and location accordingly
            octTowers[i].transform.localScale = new Vector3(octTowers[i].transform.localScale.x,
                                                          newScale,
                                                          octTowers[i].transform.localScale.z);
            // make the material brighter/darker accordingly
            Material mat = octTowers[i].GetComponent<MeshRenderer>().material;
            float newV = Mathf.Clamp(diffBrightness * spectrumValues[octTowerSpectrumType[i]] * 3, 0, maxBrightness);
            currentHSVColor = new Vector3(currentHSVColor.x, currentHSVColor.y, newV);
            mat.SetColor("_EmissionColor", Color.HSVToRGB(currentHSVColor.x, currentHSVColor.y, currentHSVColor.z));
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
            finalScale *= Random.Range(0.85f, 1.15f);
        }

        return finalScale;
    }
}
