using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
	// GUI
	public GameObject healthTextObj, moneyTextObj, waveTextObj;
	private Text healthText, moneyText, waveText;

	// pop up windows
	private GameObject gameOverBox, gameWinBox, pauseScreen, tutorialBox;

	private GameManager gm;

	public List<AudioSource> pausedAudios;
	private bool paused = false;

	// UI sound effects
	public AudioClip buttonEnable, buttonPress;


	// Use this for initialization
	private void Start () {
		gm = FindObjectOfType<GameManager>();
		healthText = healthTextObj.GetComponent<Text>();
		healthText.text = "" + gm.maxHealth + " / " + gm.maxHealth;
		waveText = waveTextObj.GetComponent<Text>();
		moneyText = moneyTextObj.GetComponent<Text>();
		moneyText.text = "" + gm.startingMoney;
		/*
		gameOverBox = transform.Find("GameOverBox").gameObject;
		gameOverBox.SetActive(false);
		gameWinBox = transform.Find("GameWinBox").gameObject;
		gameWinBox.SetActive(false);
		pauseScreen = transform.Find("PauseScreen").gameObject;
*/
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

	public void UpdateHealth(int i){
		healthText.text = "" + i + " / " + gm.maxHealth;
	}

    public void UpdateMoney(int i){
        moneyText.text = "" + gm.currentMoney;
    }

	public void UpdateWave(int i){
		// todo
		return; 
	}

	public void DisplayGameOverScreen(){
		gameOverBox.SetActive(true);
	}

	public void DisplayGameWinScreen(float score){
		gameWinBox.SetActive(true);
		gameWinBox.transform.Find("Text_Score").GetComponent<Text>().text = "SCORE: " + (int)score;
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

    public void StartWave() {
        transform.Find("SpawnButton").gameObject.SetActive(false);
        FindObjectOfType<Scanner>().spawnEnemies = true;
    }


    public void ShowUpgradeButton(bool b){
		transform.Find("Button_Upgrade").gameObject.SetActive(b);
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
