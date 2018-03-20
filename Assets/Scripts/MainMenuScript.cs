using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour {
	public AudioClip buttonPressSound;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadElectro(){
		GetComponent<AudioSource>().PlayOneShot(buttonPressSound);
		SceneManager.LoadSceneAsync("Electro_New");
	}

	public void LoadSynth(){
		GetComponent<AudioSource>().PlayOneShot(buttonPressSound);
		SceneManager.LoadSceneAsync("Synth_New");
	}

	public void LoadHipHop(){
		GetComponent<AudioSource>().PlayOneShot(buttonPressSound);
		SceneManager.LoadSceneAsync("HipHop_New");
	}

	public void Quit(){
		Application.Quit();
	}
}
