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


	void Awake () {
        octTowers = new List<GameObject>();
        octTowerScales = new List<float>();
        foreach (Transform t in transform.GetComponentInChildren<Transform>()) {
            octTowers.Add(t.gameObject);
            float scale = GetScaleByDistance(t.position);
            t.localScale = new Vector3(t.localScale.x, scale, t.localScale.z);
            octTowerScales.Add(scale);
        }
    }
	

	void Update () {
        // respond to spectrum
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
