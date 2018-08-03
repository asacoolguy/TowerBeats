﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicTower : MonoBehaviour {
	protected AudioSource audioSource;
	protected Animator anim;
    public int cost;
    public int towerType = 0;
	public int axisIndex = 0;
    public float powerFactor = 1f;
    public TowerInfo info;
    protected int randomClipIndex = -1;

    public float popYScale, popTime, lightBoostTime;
    public float lightMinIntensity, lightMaxIntensity;
    protected Light spotlight;

    protected void Awake () {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
        spotlight = transform.Find("Spotlight").GetComponent<Light>();
        spotlight.intensity = lightMinIntensity;
	}
	

    public virtual void PlaySound() {
        // enable the lights
        StartCoroutine(BoostLight());
        // boost the color and size of the octagon
        transform.parent.GetComponent<TowerPlatform>().BoostColor();
        transform.parent.GetComponent<TowerPlatform>().BoostSize(popYScale, popTime);
    }


    public virtual void SetupSound() { }
    
    
    // toggle on/off the outlines for this tower and its kids
    public void ToggleOutline(bool b) {
        //print("toggled outline to " + b);

        if (GetComponent<cakeslice.Outline>() != null) {
            GetComponent<cakeslice.Outline>().enabled = b;
        }

        foreach (Transform t in GetComponentsInChildren<Transform>()) {
            cakeslice.Outline outline = t.gameObject.GetComponent<cakeslice.Outline>();
            if (outline != null) {
                outline.enabled = b;
            }
        }
    }
    

    private IEnumerator BoostLight() {
        float currentTime = 0f;
        while (currentTime < lightBoostTime) {
            spotlight.intensity = GameManager.EaseOut(lightMinIntensity, lightMaxIntensity, currentTime / lightBoostTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentTime = lightBoostTime;
        while (currentTime > 0) {
            spotlight.intensity = GameManager.EaseOut(lightMinIntensity, lightMaxIntensity, currentTime / lightBoostTime);
            currentTime -= Time.deltaTime;
            yield return null;
        }
    }


    public void UpgradeTower() {
        if (info.currentLevel < info.maxLevel - 1) {
            info.currentLevel++;
            SetupSound();

            // TODO: increase power level and change models
        }   
    }
}


[Serializable]
public class TowerInfo {
    public Color color;
    public int maxLevel, currentLevel;
    [TextArea]
    public string[] descriptions;
    public int[] costs;
}