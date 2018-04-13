using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicalGrid : MonoBehaviour {
    // inputs
    public GameObject[] hexTowers;
    public Color[] hexTowerColors;

    // grid parameters
    public bool makeGrid, randomize;
    public int width, height;
    private List<List<GameObject>> gridObjects;
    private List<List<float>> gridRandomizeScale;

    // color parameters
    public Color defaultMainColor;
    public Color defaultTextureColor;
    public float minScale, maxScale;
    public float minTextreStr, maxTextureStr;
    public float minGlowPow, maxGlowPow;
    private float diffScale;
    private float diffGlowStr;
    private float diffGlowPow;

    // spectrum parameters
    private SpectrumData spectrum;
    private float[] scaleVals;

	void Start () {
        spectrum = FindObjectOfType<GameManagerNew>().GetComponent<SpectrumData>();
        scaleVals = new float[hexTowerColors.Length];

        diffScale = maxScale - minScale;
        diffGlowPow = maxGlowPow - minGlowPow;
        diffGlowStr = maxTextureStr - minTextreStr;

        if (makeGrid) {
            ConstructGrid();
        }
	}
	

	void Update () {
        spectrum.GetOutputSpectrum(scaleVals);

        if (makeGrid) {
            GridUpdate();
        }
        else {
            GroupUpdate();
        }
	}

    
    // helper function that instantiates the grid with towers
    // for now assume that there are only 3 towers
    private void ConstructGrid() {
        gridObjects = new List<List<GameObject>>();
        gridRandomizeScale = new List<List<float>>();

        for (int i = 0; i < hexTowerColors.Length; i++) {
            List<GameObject> towerList = new List<GameObject>();
            List<float> randomList = new List<float>();
            // make the parent object for these hexTowers
            GameObject parent = new GameObject();
            parent.transform.parent = transform;
            parent.name = "hexTowerGroup" + i;
            
            Vector3 startPos = new Vector3(width / 2 * -35, 5, height / 2 * -30);
            if (i == 1) {
                startPos += new Vector3(17.5f, 0, 10);
            }
            else if (i == 2) {
                startPos += new Vector3(17.5f, 0, -10);
            }
            for (int j = 0; j < height; j++) {
                float xOffset = j % 2 == 0 ? 0f : 17.5f;

                for (int k = 0; k < width; k++) {
                    Vector3 newPos = startPos + new Vector3(k * 35 + xOffset, 0, j * 30);
                    GameObject tower = Instantiate(hexTowers[0], newPos, Quaternion.identity, parent.transform);
                    towerList.Add(tower);
                    tower.SetActive(true);

                    if (randomize) {
                        // randomize the scale so it looks a bit more interesting
                        float factor = Random.Range(0.8f, 1.2f);
                        randomList.Add(factor);
                    }
                }
            }

            gridObjects.Add(towerList);
            gridRandomizeScale.Add(randomList);
        }
    }


    // handles updating visuals for a group
    private void GroupUpdate() {
        for (int i = 0; i < hexTowers.Length; i++) {
            float newScale = Mathf.Clamp(minScale + diffScale * scaleVals[i], minScale, maxScale);
            // change the scale and location accordingly
            hexTowers[i].transform.localScale = new Vector3(hexTowers[i].transform.localScale.x,
                                                          newScale,
                                                          hexTowers[i].transform.localScale.z);
            hexTowers[i].transform.localPosition = new Vector3(hexTowers[i].transform.localPosition.x,
                                                             hexTowers[i].transform.localScale.y,
                                                             hexTowers[i].transform.localPosition.z);
            // make the material brighter/darker accordingly
            Material mat = hexTowers[i].GetComponent<MeshRenderer>().material;
            mat.SetFloat("_MKGlowTexStrength", Mathf.Clamp(minTextreStr + diffGlowStr * scaleVals[i], minTextreStr, maxTextureStr));
            mat.SetFloat("_MKGlowPower", Mathf.Clamp(minGlowPow + diffGlowPow * scaleVals[i], minGlowPow, maxGlowPow));
            Color newMainColor = Color.Lerp(defaultMainColor, hexTowerColors[i], 0.4f + 0.6f * scaleVals[i]);
            mat.SetColor("_Color", newMainColor);
            Color newTextureColor = Color.Lerp(defaultTextureColor, hexTowerColors[i], scaleVals[i]);
            mat.SetColor("_MKGlowTexColor", newTextureColor);
        }
    }


    // handles updating visuals for a grid
    private void GridUpdate() {
        for (int i = 0; i < gridObjects.Count; i++) {
            for (int j = 0; j < gridObjects[i].Count; j++) {
                float newScale = Mathf.Clamp(minScale + diffScale * scaleVals[i], minScale, maxScale);
                if (randomize) {
                    newScale *= gridRandomizeScale[i][j];
                }
                // change the y scale and y location accordingly
                gridObjects[i][j].transform.localScale = new Vector3(gridObjects[i][j].transform.localScale.x,
                                                                     newScale,
                                                                     gridObjects[i][j].transform.localScale.z);
                gridObjects[i][j].transform.localPosition = new Vector3(gridObjects[i][j].transform.localPosition.x,
                                                                 gridObjects[i][j].transform.localScale.y,
                                                                 gridObjects[i][j].transform.localPosition.z);
                // make the material brighter/darker accordingly
                Material mat = gridObjects[i][j].GetComponent<MeshRenderer>().material;
                mat.SetFloat("_MKGlowTexStrength", Mathf.Clamp(minTextreStr + diffGlowStr * scaleVals[i], minTextreStr, maxTextureStr));
                mat.SetFloat("_MKGlowPower", Mathf.Clamp(minGlowPow + diffGlowPow * scaleVals[i], minGlowPow, maxGlowPow));
                Color newMainColor = Color.Lerp(defaultMainColor, hexTowerColors[i], 0.4f + 0.6f * scaleVals[i]);
                mat.SetColor("_Color", newMainColor);
                Color newTextureColor = Color.Lerp(defaultTextureColor, hexTowerColors[i], scaleVals[i]);
                mat.SetColor("_MKGlowTexColor", newTextureColor);
            }
        }
    }
}
