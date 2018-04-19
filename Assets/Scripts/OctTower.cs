using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Script attached to all OctGridTowers.
    GridType determines behavior:
        BuildableOct: where towers can be build. 
                      rises when hovered over.
                      shows build grid when clicked on.
        InnerGridOct: octagons in the inner grid. doesn't do anything 
        OuterGridOct: octagons in the outer grid. 
                      has varied height depending on distance from center
                      rises depending on spectrum data
 */ 

public enum OctGridType { BuildableOct, InnerGirdOct, OuterGridOct };

public class OctGridTower : MonoBehaviour {
    public OctGridType type;

    private GameObject selector; // a collection of colliders that will allow this tower to be selected


	void Awake () {
        selector = transform.Find("Selector").gameObject;
        selector.tag = "OctSelector";
        if (type == OctGridType.BuildableOct) {
            selector.SetActive(true);
        }
        else if (type == OctGridType.InnerGirdOct) {
            selector.SetActive(false);
        }
        else if (type == OctGridType.OuterGridOct) {
            selector.SetActive(false);
        }
    }
	

	void Update () {
		if (type == OctGridType.BuildableOct) {

        } 
        else if (type == OctGridType.InnerGirdOct) {

        }
        else if (type == OctGridType.OuterGridOct) {

        }
	}
}
