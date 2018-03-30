using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicTower : MonoBehaviour {
	protected AudioSource audioSource;
	protected Animator anim;
    public GameObject UIpanel;
    public int cost;
	public int axisIndex = 0;
    public bool refundable = false;
    public int upgradeLevel = 1;

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

        ToggleOutline(false);
        ToggleUIPanel(false);

        // all towers start in the planning stage
        MakePlanning();
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
        ToggleOutline(false);
        ToggleUIPanel(false);
        refundable = true;
        MakePlanning();
        FindObjectOfType<GameManager>().MoveSelectedTower(this.gameObject);
        print("make move");
    }


	// set the tower in its built stage
	public virtual void MakeBuilt(){
		state = TowerState.built;
        refundable = false;
		mRenderer.material = originalMat;
		transform.Find("AOEIndicator").gameObject.SetActive(false);
        UIpanel.transform.eulerAngles = new Vector3(90, 0, 0);
	}

    
    // toggle on/off the outlines for this tower and its kids
    public void ToggleOutline(bool b) {
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


    public void ToggleUIPanel(bool b) {
        UIpanel.SetActive(b);
    }


    public abstract void UpgradeTower();
    
    public void CloseUIPanel() {
        ToggleUIPanel(false);
        FindObjectOfType<GameManager>().ClearTowerUI();
    }
}
