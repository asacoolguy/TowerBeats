using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MK.Glow;

public class MusicalGrid : MonoBehaviour {
    public GameObject[] gridObj;
    public Color[] objColors;
    public Color defaultMainColor;
    public Color defaultTextureColor;
    public float minScale, maxScale;
    public float minTextreStr, maxTextureStr;
    public float minGlowPow, maxGlowPow;

    private SpectrumData spectrum;
    private float[] scaleVals;
    private float diffScale;
    private float diffGlowStr;
    private float diffGlowPow;


	void Start () {
        spectrum = FindObjectOfType<GameManagerNew>().GetComponent<SpectrumData>();
        scaleVals = new float[gridObj.Length];

        diffScale = maxScale - minScale;
        diffGlowPow = maxGlowPow - minGlowPow;
        diffGlowStr = maxTextureStr - minTextreStr;
	}
	

	void Update () {
        spectrum.GetOutputSpectrum(scaleVals);

        for (int i = 0; i < gridObj.Length; i++) {
            float newScale = Mathf.Clamp(minScale + diffScale * scaleVals[i], minScale, maxScale);
            // change the scale and location accordingly
            gridObj[i].transform.localScale = new Vector3(gridObj[i].transform.localScale.x,
                                                          newScale,
                                                          gridObj[i].transform.localScale.z);
            gridObj[i].transform.localPosition = new Vector3(gridObj[i].transform.localPosition.x, 
                                                             gridObj[i].transform.localScale.y, 
                                                             gridObj[i].transform.localPosition.z);
            // make the material brighter/darker accordingly
            Material mat = gridObj[i].GetComponent<MeshRenderer>().material;
            mat.SetFloat("_MKGlowTexStrength", Mathf.Clamp(minTextreStr + diffGlowStr * scaleVals[i], minTextreStr, maxTextureStr));
            mat.SetFloat("_MKGlowPower", Mathf.Clamp(minGlowPow + diffGlowPow * scaleVals[i], minGlowPow, maxGlowPow));
            Color newMainColor = Color.Lerp(defaultMainColor, objColors[i], 0.4f + 0.6f * scaleVals[i]);
            mat.SetColor("_Color", newMainColor);
            Color newTextureColor = Color.Lerp(defaultTextureColor, objColors[i], scaleVals[i]);
            mat.SetColor("_MKGlowTexColor", newTextureColor);
        }
	}
}
