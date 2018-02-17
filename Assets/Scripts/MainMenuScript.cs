using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void LoadElectro(){
		SceneManager.LoadSceneAsync("Electro");
	}

	public void LoadSynth(){
		SceneManager.LoadSceneAsync("Synth");
	}

	public void LoadHipHop(){
		SceneManager.LoadSceneAsync("Hip Hop");
	}

	public void Quit(){
		Application.Quit();
	}
}
