﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
	private Text textHealth, textLevel, textMoney;
	private GameObject gameOverBox, gameWinBox, pauseScreen, tutorialBox;
	private GameManager gm;
	public GameObject greenBuildButton, blueBuildButton, goldBuildButton, textMoneyObj,
                      progressionBar, healthTextObj, textLevelObj;
	public List<GameObject> thresholds;
	public List<AudioSource> pausedAudios;
	private bool paused = false;
	public bool tutorialShowing = false;
	private float progressBarMaxSize;

	// UI sound effects
	public AudioClip buttonEnable, buttonPress;

	// Use this for initialization
	void Start () {
		gm = FindObjectOfType<GameManager>();
		textHealth = healthTextObj.GetComponent<Text>();
		textHealth.text = "LIVES: " + gm.maxHealth + " / " + gm.maxHealth;
		textLevel = textLevelObj.GetComponent<Text>();
        textMoney = textMoneyObj.GetComponent<Text>();
        textMoney.text = "Total $: " + gm.startingMoney;
		gameOverBox = transform.Find("GameOverBox").gameObject;
		gameOverBox.SetActive(false);
		gameWinBox = transform.Find("GameWinBox").gameObject;
		gameWinBox.SetActive(false);
		pauseScreen = transform.Find("PauseScreen").gameObject;

		// hide elements except the start button initially
		transform.Find("Button_Start").gameObject.SetActive(true);
		transform.Find("Build Panel").gameObject.SetActive(false);
		transform.Find("Progress Panel").gameObject.SetActive(false);

		// show tutorial by default
		tutorialBox = transform.Find("TutorialBox").gameObject;
		//tutorialBox.SetActive(true);
		//tutorialShowing = true;

		// get some initial values
		progressBarMaxSize = progressionBar.transform.localScale.x;
	}	
	
	// Update is called once per frame
	void Update () {
		//tutorialShowing = tutorialBox.activeSelf;

		if (Input.GetKeyDown(KeyCode.Escape)){
			TogglePause();
		}
	}

	public void UpdateHealth(int i){
		textHealth.text = "LIVES: " + i + " / " + gm.maxHealth;
	}

	public void UpdateProgressionBar(float width){
		progressionBar.transform.localScale = new Vector3(Mathf.Clamp(width, 0f, 1f) * progressBarMaxSize,
														progressionBar.transform.localScale.y,
														progressionBar.transform.localScale.z);
	}

    public void UpdateMoney(int i)
    {
        textMoney.text = "Total $: " + gm.currentMoney;
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


	// sets up the build tower buttons accordingly to availability
	public void SetupBuildTowerButtons(bool greenOkay, bool blueOkay, bool goldOkay){
		if (transform.Find("Build Panel").gameObject.activeSelf){
			ToggleShockTowerButton(greenOkay);
			ToggleSniperTowerButton(blueOkay);
			ToggleLaserTowerButton(goldOkay);
		}
	}


	public void ToggleShockTowerButton(bool b){
		Button button = greenBuildButton.GetComponent<Button>();
		Text text = greenBuildButton.GetComponentInChildren<Text>();
		greenBuildButton.GetComponent<Animator>().SetBool("Available", b);
		if (b && button.IsInteractable() == false){
			button.interactable = true;
			text.text = "Shock";
			greenBuildButton.GetComponent<Animator>().SetTrigger("Enable");
			GetComponent<AudioSource>().PlayOneShot(buttonEnable);
		}
		else if (!b){
			button.interactable = false;
			text.text = "Unavailable";
		}
	}

	public void ToggleSniperTowerButton(bool b){
		Button button = blueBuildButton.GetComponent<Button>();
		Text text = blueBuildButton.GetComponentInChildren<Text>();
		blueBuildButton.GetComponent<Animator>().SetBool("Available", b);
		if (b && button.IsInteractable() == false){
			button.interactable = true;
			text.text = "Sniper";
			blueBuildButton.GetComponent<Animator>().SetTrigger("Enable");
			GetComponent<AudioSource>().PlayOneShot(buttonEnable);
		}
		else if (!b){
			button.interactable = false;
			text.text = "Unavailable";
		}
	}

	public void ToggleLaserTowerButton(bool b){
		Button button = goldBuildButton.GetComponent<Button>();
		Text text = goldBuildButton.GetComponentInChildren<Text>();
		goldBuildButton.GetComponent<Animator>().SetBool("Available", b);
		if (b && button.IsInteractable() == false){
			button.interactable = true;
			text.text = "Laser";
			goldBuildButton.GetComponent<Animator>().SetTrigger("Enable");
			GetComponent<AudioSource>().PlayOneShot(buttonEnable);
		}
		else if (!b){
			button.interactable = false;
			text.text = "Unavailable";
		}
	}

	public void StartGame(){
		transform.Find("Button_Start").gameObject.SetActive(false);
		transform.Find("Build Panel").gameObject.SetActive(true);
		transform.Find("Progress Panel").gameObject.SetActive(true);
		progressionBar.transform.localScale = new Vector3(0,
														progressionBar.transform.localScale.y,
														progressionBar.transform.localScale.z);
	}


	public void ShowUpgradeButton(bool b){
		transform.Find("Button_Upgrade").gameObject.SetActive(b);
	}


	public void UpgradeToLevel(int i){
		ShowUpgradeButton(false);
		textLevel.text = "LEVEL" + i;
	}

	public void TogglePause(){
		// pause button doesn't work when tutorial's showing
		if (tutorialShowing){
			return;
		}

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
