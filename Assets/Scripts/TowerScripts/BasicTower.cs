using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicTower : MonoBehaviour {
	protected AudioSource audioSource;
	protected Animator anim;
	public int axisIndex = 0;

	// this enum determines what state the tower is in
	// TODO: combine this with the ready state and other potential states like deactivated, buffed
	public enum TowerState{planning, planningInvalid, built, functional};
	protected TowerState state;

	public Material originalMat, planningMat, planningInvalidMat;
	protected CapsuleCollider collider;
	protected MeshRenderer mRenderer;

	// Use this for initialization
	protected void Start () {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		mRenderer = GetComponent<MeshRenderer>();
		originalMat = mRenderer.material;
		collider = GetComponent<CapsuleCollider>();

		// all towers start in the planning stage
		MakePlanning();
	}
	
	// Update is called once per frame
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

	public bool IsFunctional(){
		return state == TowerState.functional;
	}

	public void MakeFunctional(){
		state = TowerState.functional;
	}

	// set the tower in its planning stage
	public virtual void MakePlanning(){
		state = TowerState.planning;
		mRenderer.material = planningMat;
		transform.Find("Area").GetComponent<MeshRenderer>().enabled = true;
	}


	// set the tower in its built stage
	public virtual void MakeBuilt(){
		// TODO should there still be planning stage?
		state = TowerState.functional;
		mRenderer.material = originalMat;
		transform.Find("Area").GetComponent<MeshRenderer>().enabled = false;
	}
}
