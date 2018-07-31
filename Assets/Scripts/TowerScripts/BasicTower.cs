using System;
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

	// this enum determines what state the tower is in
	// TODO: combine this with the ready state and other potential states like deactivated, buffed
	public enum TowerState{planning, planningInvalid, built, moving};
	protected TowerState state;

	public Material originalMat, planningMat, planningInvalidMat;
	protected MeshRenderer mRenderer;

    public float popYScale, popTime, lightBoostTime;
    protected float defaultYScale;
    public float lightMinIntensity, lightMaxIntensity;
    protected Light spotlight;

    protected void Start () {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		mRenderer = GetComponent<MeshRenderer>();
		originalMat = mRenderer.material;
        spotlight = transform.Find("Spotlight").GetComponent<Light>();
        spotlight.intensity = lightMinIntensity;
        defaultYScale = transform.localScale.y;

        powerFactor = FindObjectOfType<CentralPlatform>().GetPowerFactor();
	}
	

	void Update () {
		
	}


	void OnTriggerStay(Collider other){
		if (state == TowerState.planning && other.gameObject.tag == "Tower"){
			// bypasses the bug where your AoE collider also prevents you from constructing a tower
			// check the additional factor of distance to the other tower
			float dist = Vector3.Distance(other.transform.position, this.transform.position);
			if (dist < this.GetComponent<CapsuleCollider>().radius * transform.lossyScale.x){
				state = TowerState.planningInvalid;
				mRenderer.material = planningInvalidMat;
			}
		}
	}

	void OnTriggerExit(Collider other){
		if (state == TowerState.planningInvalid && other.gameObject.tag == "Tower"){
			state = TowerState.planning;
			mRenderer.material = planningMat;
		}
	}

    public virtual void PlaySound() {
        // enable the lights
        StartCoroutine(BoostLight());
        // boost the color and size of the octagon
        transform.parent.GetComponent<TowerPlatform>().BoostColor();
        transform.parent.GetComponent<TowerPlatform>().BoostSize(popYScale, popTime);
    }


    public virtual void SetupSound() { }


	public bool IsBuildable(){
		return state == TowerState.planning;
	}

	public bool IsBuilt(){
		return state == TowerState.built;
	}

    public bool IsMoving() {
        return state == TowerState.moving;
    }

	// set the tower in its planning stage
	public virtual void MakePlanning(){
		state = TowerState.planning;
		mRenderer.material = planningMat;
		transform.Find("AOEIndicator").gameObject.SetActive(true);
	}


    // set the tower in its moving stage
    // really just do planning stage, but enable a refund if you cancel the build
    public virtual void MakeMoving() {
        MakePlanning();
    }


	// set the tower in its built stage
	public virtual void MakeBuilt(){
		// TODO should there still be planning stage?
		state = TowerState.built;
        //mRenderer.material = originalMat;
		transform.Find("AOEIndicator").gameObject.SetActive(false);
	}

    
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