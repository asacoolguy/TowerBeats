using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanel : MonoBehaviour {
    public Color[] towerColors;


	private void Awake () {
        for (int i = 0; i < towerColors.Length; i++) {
            string name = "Tower" + (i + 1);
            GameObject towerButton = transform.Find(name).gameObject;
            Material mat = towerButton.transform.Find("Octagon").GetComponent<MeshRenderer>().material;
            Text towerName = towerButton.transform.Find("Text").GetComponent<Text>();
            Text towerCost = towerButton.transform.Find("Cost").GetComponent<Text>();
            mat.SetColor("_Color", new Color(towerColors[i].r, towerColors[i].g, towerColors[i].b, 0.75f));
            mat.SetColor("_MKGlowColor", towerColors[i]);
            mat.SetColor("_MKGlowTexColor", towerColors[i]);
            towerName.color = towerColors[i];
            towerCost.color = towerColors[i];
        }
    }
}
