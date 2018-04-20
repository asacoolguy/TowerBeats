using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicTower : MonoBehaviour {
	protected AudioSource audioSource;
	protected Animator anim;
    public int cost;
	public int axisIndex = 0;
    public bool refundable = false;

	// this enum determines what state the tower is in
	// TODO: combine this with the ready state and other potential states like deactivated, buffed
	public enum TowerState{planning, planningInvalid, built, moving};
	protected TowerState state;

	public Material originalMat, planningMat, planningInvalidMat;
	protected MeshRenderer mRenderer;


	protected void Start () {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		mRenderer = GetComponent<MeshRenderer>();
		originalMat = mRenderer.material;
        
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

	public abstract void PlaySound();


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
        refundable = true;
        MakePlanning();
    }


	// set the tower in its built stage
	public virtual void MakeBuilt(){
		// TODO should there still be planning stage?
		state = TowerState.built;
        refundable = false;
        //mRenderer.material = originalMat;
		transform.Find("AOEIndicator").gameObject.SetActive(false);
	}

    
    // toggle on/off the outlines for this tower and its kids
    public void ToggleOutline(bool b) {
        print("toggled outline to " + b);

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
    
}
