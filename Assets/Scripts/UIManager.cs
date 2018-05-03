using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
	// GUI
	public GameObject healthTextObj, moneyTextObj, waveTextObj;
	private Text healthText, moneyText, waveText;
	public GameObject spawnButton;

	// pop up windows
	private GameObject gameOverBox, gameWinBox, pauseScreen, tutorialBox;

	public List<AudioSource> pausedAudios;
	private bool paused = false;

	// UI sound effects
	public AudioClip buttonEnable, buttonPress;


	// Use this for initialization
	private void Awake () {
		healthText = healthTextObj.GetComponent<Text>();
		waveText = waveTextObj.GetComponent<Text>();
		moneyText = moneyTextObj.GetComponent<Text>();
		
		gameOverBox = transform.Find("GameOverBox").gameObject;
		gameOverBox.SetActive(false);
		gameWinBox = transform.Find("GameWinBox").gameObject;
		gameWinBox.SetActive(false);
		//pauseScreen = transform.Find("PauseScreen").gameObject;

		// show the game start button and reset the progress bar
		//transform.Find("StartButton").gameObject.SetActive(true);

        // show tutorial by default
        //tutorialBox = transform.Find("TutorialBox").gameObject;
		//tutorialBox.SetActive(true);
		//tutorialShowing = true;
	}	
	
	// Update is called once per frame
	private void Update () {
		//tutorialShowing = tutorialBox.activeSelf;

		if (Input.GetKeyDown(KeyCode.Escape)){
			TogglePause();
		}
	}

	public void UpdateHealth(int current, int max){
		healthText.text = "" + current + "/" + max;
	}

    public void UpdateMoney(int current){
        moneyText.text = "" + current;
    }

	public void UpdateWave(int current, int max){
		waveText.text = "Wave: " + (current +1) + "/" + max;
        waveText.GetComponent<Animator>().SetTrigger("Pop");
	}

	public void ShowSpawnButton(bool b){
		spawnButton.gameObject.SetActive(b);
	}

	public void DisplayGameOverScreen(){
		gameOverBox.SetActive(true);
	}


    public void ShowGUI(bool b) {
        transform.Find("ControlPanel").gameObject.SetActive(b);
        transform.Find("StatusPanel").gameObject.SetActive(b);
        transform.Find("WavePanel").gameObject.SetActive(b);
    }


	public void DisplayGameWinScreen(){
		gameWinBox.SetActive(true);
	}

	public void RestartGame(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}


	public void LoadMainMenu(){
		SceneManager.LoadScene("Main Menu");
	}


	public void StartMusic(){
		transform.Find("StartButton").gameObject.SetActive(false);		
	}
		

	public void TogglePause(){
		// pause button doesn't work when tutorial's showing
		//if (tutorialShowing){
		//	return;
		//}

		PlayButtonPressSound();
		if (paused){
			paused = false;
			Time.timeScale = 1;
			pauseScreen.SetActive(false);

			// unpause all audiosources
			foreach(AudioSource au in pausedAudios){
				au.UnPause();
			}
		}
		else if (!paused){
			paused = true;
			Time.timeScale = 0;
			pauseScreen.SetActive(true);

			// pause all audiosources
			pausedAudios = new List<AudioSource>();
			foreach(AudioSource au in FindObjectsOfType<AudioSource>()){
				if (au != this.GetComponent<AudioSource>() && au.isPlaying){
					pausedAudios.Add(au);
					au.Pause();
				}
			}
		}
	}

	public void PlayButtonPressSound(){
		GetComponent<AudioSource>().PlayOneShot(buttonPress);
	}
}
