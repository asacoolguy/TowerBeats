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

    private List<List<GameObject>> octTowers;
    private List<List<float>> octTowerScales;
    private Vector3 currentHSVColor;

    // parameters that change based on spectrum values
    public float minBumpScale, maxBumpScale;
    private float maxBrightness;
    private float diffBumpScale, diffBrightness;

    // spectrum parameters
    private SpectrumData spectrum;
    private float[] spectrumValues;


    void Awake () {
        octTowers = new List<List<GameObject>>();

        for (int i = 0; i < transform.childCount; i++) {
            List<GameObject> ringList = new List<GameObject>();

            Transform ring = transform.GetChild(i);
            for (int j = 0; j < ring.childCount;  j++) {
                ringList.Add(ring.GetChild(j).gameObject);
            }

            octTowers.Add(ringList);
        }


        octTowerScales = new List<List<float>>();

        for (int i = 0; i < octTowers.Count; i++) {
            List<float> scales = new List<float>();
            float scale = minScale + i / octTowers.Count * (maxScale - minScale);

            for (int j = 0; j < octTowers[i].Count; j++) {
                GameObject tower = octTowers[i][j];

                float actualScale = scale * Random.Range(0.85f, 1.15f);
                tower.transform.localScale = new Vector3(tower.transform.localScale.x, actualScale, tower.transform.localScale.z);
                scales.Add(actualScale);

                Vector3 localPos = tower.transform.localPosition;
                tower.transform.localPosition = new Vector3(localPos.x, localPos.y + yPosOffset, localPos.z);
            }

            octTowerScales.Add(scales);
        }


        //foreach (Transform t in transform.GetComponentInChildren<Transform>()) {

        //    float scale = GetScaleByDistance(t.position);
        //    t.localScale = new Vector3(t.localScale.x, scale, t.localScale.z);
        //    t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y + yPosOffset, t.localPosition.z);
        //    octTowerScales.Add(scale);

        //    // make each transform a random type
        //    // random
        //    //int i = Random.Range(0, bumpColors.Count);
        //    //i = ++i % bumpColors.Count;

        //    // distance based
        //    //float dist = new Vector3(t.localPosition.x, 0, t.localPosition.z).magnitude;
        //    //int i = dist < 75 ? 2 : dist < 85 ? 1 : 0;

        //    // angle based
        //    //float angle = GameManager.GetAngleFromVector(t.localPosition);
        //    //if (angle < 0) angle += 360;
        //    //angle %= 60;
        //    //i = angle < 20 ? 0 : angle < 40 ? 1 : 2;

        //    //octTowerSpectrumType.Add(i);
        //}

        Color currentColor = GetComponentInChildren<MeshRenderer>().material.GetColor("_EmissionColor");
        float h, s, v;
        Color.RGBToHSV(currentColor, out h, out s, out v);
        currentHSVColor = new Vector3(h, s, v);
        maxBrightness = v;

        diffBumpScale = maxBumpScale - minBumpScale;
        diffBrightness = maxBrightness;
    }


    private void Start() {
        spectrum = GameManager.instance.GetComponent<SpectrumData>();
        spectrumValues = new float[octTowers.Count + 1];
    }


    void Update () {
        // respond to spectrum
        spectrum.GetOutputSpectrum(spectrumValues);
        // some value tweaking


        for(int i = 0; i < octTowers.Count; i++) {
            for (int j = 0; j < octTowers[i].Count; j++) {
                GameObject tower = octTowers[i][j];

                float newScale = octTowerScales[i][j] * (1 + diffBumpScale * spectrumValues[i]);
                // change the scale and location accordingly
                tower.transform.localScale = new Vector3(tower.transform.localScale.x,
                                                         newScale,
                                                         tower.transform.localScale.z);
                // make the material brighter/darker accordingly
                Material mat = tower.GetComponent<MeshRenderer>().material;
                float newV = Mathf.Clamp(diffBrightness * spectrumValues[i] * 3, 0, maxBrightness);
                currentHSVColor = new Vector3(currentHSVColor.x, currentHSVColor.y, newV);
                mat.SetColor("_EmissionColor", Color.HSVToRGB(currentHSVColor.x, currentHSVColor.y, currentHSVColor.z));
            }
        }
    }


    // sets the scale according to distance from center
    //float GetScaleByDistance(Vector3 pos) {
    //    float finalScale = transform.localScale.y;
    //    float distanceSqrd = pos.x * pos.x + pos.z * pos.z;
    //    float minDistSqrd = minDistFromCenter * minDistFromCenter;
    //    float maxDistSqrd = maxDistFromCenter * maxDistFromCenter;

    //    if (distanceSqrd < minDistSqrd) {
    //        finalScale = minScale;
    //    }
    //    else if (distanceSqrd > maxDistSqrd) {
    //        finalScale = maxScale;
    //    }
    //    else {
    //        finalScale = minScale + (maxScale - minScale) * ((distanceSqrd - minDistSqrd) / (maxDistSqrd - minDistSqrd));
    //        finalScale *= Random.Range(0.85f, 1.15f);
    //    }

    //    return finalScale;
    //}
}