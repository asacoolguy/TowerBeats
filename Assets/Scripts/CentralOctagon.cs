﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO: this entire class is borrowed functions. refactor it to extend other classes instead
public class CentralOctagon : Octagon {
    public Color mainColor;
    private int level;
    public LayerMask hitLayers;
    private bool highlighted;
    private GameObject upgradeButton;
    private bool buttonHighlighted;
    public float highlightedButtonSize;
    public int[] cost;
    public float[] powerFactors;

    private new void Awake() {
        base.Awake();

        highlighted = false;
        buttonHighlighted = false;

        // set up the central build panel
        upgradeButton = transform.Find("CentralBuildPanel").Find("Upgrade").gameObject;
        upgradeButton.GetComponent<Image>().color = mainColor;
        upgradeButton.transform.Find("Text").GetComponent<Text>().color = mainColor;
        upgradeButton.transform.Find("Cost").GetComponent<Text>().color = mainColor;
        upgradeButton.transform.Find("Level").GetComponent<Text>().color = mainColor;
        upgradeButton.transform.Find("Cost").GetComponent<Text>().text = "" + cost[0];
        upgradeButton.transform.Find("Description").GetComponent<Image>().color = mainColor;
        upgradeButton.transform.Find("Description").GetComponentInChildren<Text>().color = mainColor;

        // set up initial states
        level = 0;
    }
    
	
	private new void Update () {
        highlighted = IsHighlighted();
        buttonHighlighted = IsButtonHighlighted();

        if (Input.GetMouseButtonDown(0)) {
           
            if (buttonHighlighted) {
                UpgradeCentralTower();
                ActivatePanel(false);
            }
            else {
                ActivatePanel(highlighted);
            }
        }
        else {
            if (highlighted) {
                RaiseOctagon();
            }
            else {
                LowerOctagon();
            }

            upgradeButton.transform.Find("Description").gameObject.SetActive(buttonHighlighted);
        }
        base.Update();
    }


    private bool IsHighlighted() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, hitLayers)) {
            // trace parents until we find the object with BuildableOctagon script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            //print("initial hit is " + current.name);
            while (current.tag != "HomeBase" && current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
            if (current.tag == "HomeBase") {
                //print("script returned");
                return true;
            }
            //print("no script");
        }

        // print("no hit ");
        // otherwise no tower hit
        return false;
    }


    private bool IsButtonHighlighted() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, hitLayers)) {
            // trace parents until we find the object with BuildPanel script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            //print("initial hit is " + current.name);
            while (current.name != "Upgrade" && current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
            if (current.name == "Upgrade") {
                return true;
            }
            //print("no script");
        }

        // print("no hit ");
        // otherwise no tower hit
        return false;
    }


    // sets the gameobject to active and plays a little animation
    public void ActivatePanel(bool b) {
        if (b) {
            transform.Find("CentralBuildPanel").gameObject.SetActive(b);
            StopCoroutine(PlayActivateAnimation());
            StartCoroutine(PlayActivateAnimation());
        }
        else {
            transform.Find("CentralBuildPanel").gameObject.SetActive(b);
        }
    }

    private IEnumerator PlayActivateAnimation() {
        float counter, t, limit;
        counter = t = 0;
        limit = 0.2f;
        Vector3 startPos = new Vector3(0, 0, 120);

        while (counter <= limit) {
            counter += Time.deltaTime;
            t = Mathf.Clamp(counter / limit, 0f, 1f);

            upgradeButton.transform.localPosition = GameManager.SmoothStep(startPos, new Vector3(-170, -170, 0), t);
            yield return null;
        }
    }


    public float GetPowerFactor() {
        return powerFactors[level];
    }

    private void UpgradeCentralTower() {
        if (level < 3) {
            FindObjectOfType<GameManager>().SpendMoney(cost[level]);

            level++;
            FindObjectOfType<Scanner>().numAudioPlaying++;

            // set powerfactor for all towers
            float factor = powerFactors[level];
            foreach (BasicTower tower in FindObjectsOfType<BasicTower>()) {
                tower.powerFactor = factor;
            }

            // update the text
            int percent = (level+1) * 20;
            string temp = "Increase Tower Power By " + percent + "%";
            upgradeButton.transform.Find("Description").GetComponentInChildren<Text>().text = temp;
            upgradeButton.transform.Find("Cost").GetComponent<Text>().text = "" + cost[level];
            upgradeButton.transform.Find("Level").GetComponent<Text>().text = "level " + (level+1);
            upgradeButton.transform.Find("Description").GetComponentInChildren<Text>().color = mainColor;

            if (level == 3) {
                upgradeButton.transform.Find("Description").GetComponentInChildren<Text>().text = "Fully Upgraded";
                upgradeButton.transform.Find("Cost").GetComponent<Text>().text = "";
            }
            
        }
    }
}
