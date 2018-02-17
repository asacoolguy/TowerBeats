using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
	private Text textHealth, textLevel;
	private GameObject gameOverBox, gameWinBox, pauseScreen, tutorialBox;
	private GameManager gm;
	public GameObject greenBuildButton, blueBuildButton, goldBuildButton, progressionBar, healthTextObj, textLevelObj;
	public List<GameObject> thresholds;
	public List<AudioSource> pausedAudios;
	private bool paused = false;
	public bool tutorialShowing = false;

	// Use this for initialization
	void Start () {
		gm = FindObjectOfType<GameManager>();
		textHealth = healthTextObj.GetComponent<Text>();
		textHealth.text = "Base Health: " + gm.maxHealth + " / " + gm.maxHealth;
		textLevel = textLevelObj.GetComponent<Text>();
		gameOverBox = transform.Find("GameOverBox").gameObject;
		gameOverBox.SetActive(false);
		gameWinBox = transform.Find("GameWinBox").gameObject;
		gameWinBox.SetActive(false);
		pauseScreen = transform.Find("PauseScreen").gameObject;

		// hide elements initially
		transform.Find("Button_Start").gameObject.SetActive(true);
		transform.Find("Build Panel").gameObject.SetActive(false);
		transform.Find("Bottom Panel").gameObject.SetActive(false);

		// show tutorial by default
		tutorialBox = transform.Find("TutorialBox").gameObject;
		tutorialBox.SetActive(true);
		tutorialShowing = true;
	}	
	
	// Update is called once per frame
	void Update () {
		tutorialShowing = tutorialBox.activeSelf;
		if (Input.GetKeyDown(KeyCode.Escape)){
			TogglePause();
		}
	}

	public void UpdateHealth(int i){
		textHealth.text = "Base Health: " + i + " / " + gm.maxHealth;
	}

	public void UpdateScore(float newScore){
		progressionBar.GetComponent<Slider>().value = newScore;
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

	// sets progression bar slider's max score and positions its unlock thresholds
	public void SetupProgressionBar(List<int> thresholdsValues, int scoreToUpgrade){
		Slider slider = progressionBar.GetComponent<Slider>();
		slider.maxValue = scoreToUpgrade;
		slider.value = 0;
		float totalWidth = slider.GetComponent<RectTransform>().rect.width;
		for (int i = 0; i < thresholds.Count; i++){
			float ratio = (float)thresholdsValues[i] / (float) scoreToUpgrade;
			float xPos = totalWidth * ratio;
			xPos -= totalWidth / 2f;
			thresholds[i].GetComponent<RectTransform>().localPosition = new Vector3(xPos, 0, 0);
		}
	}


	// sets up the build tower buttons accordingly to availability
	public void SetupBuildTowerButtons(bool greenOkay, bool blueOkay, bool goldOkay){
		ToggleGreenTowerButton(greenOkay);
		ToggleBlueTowerButton(blueOkay);
		ToggleGoldTowerButton(goldOkay);
	}

	public void ToggleGreenTowerButton(bool b){
		Button button = greenBuildButton.GetComponent<Button>();
		Text text = greenBuildButton.GetComponentInChildren<Text>();
		if (b && button.IsInteractable() == false){
			button.interactable = true;
			text.text = "Build Shockwave Tower";
			greenBuildButton.GetComponent<Animator>().SetTrigger("Enable");
		}
		else if (!b){
			button.interactable = false;
			text.text = "Tower Unavailable";
		}
	}

	public void ToggleBlueTowerButton(bool b){
		Button button = blueBuildButton.GetComponent<Button>();
		Text text = blueBuildButton.GetComponentInChildren<Text>();
		if (b && button.IsInteractable() == false){
			button.interactable = true;
			text.text = "Build Sniper Tower";
			blueBuildButton.GetComponent<Animator>().SetTrigger("Enable");
		}
		else if (!b){
			button.interactable = false;
			text.text = "Tower Unavailable";
		}
	}

	public void ToggleGoldTowerButton(bool b){
		Button button = goldBuildButton.GetComponent<Button>();
		Text text = goldBuildButton.GetComponentInChildren<Text>();
		if (b && button.IsInteractable() == false){
			button.interactable = true;
			text.text = "Build Laser Tower";
			goldBuildButton.GetComponent<Animator>().SetTrigger("Enable");
		}
		else if (!b){
			button.interactable = false;
			text.text = "Tower Unavailable";
		}
	}

	public void StartGame(){
		transform.Find("Button_Start").gameObject.SetActive(false);
		transform.Find("Build Panel").gameObject.SetActive(true);
		transform.Find("Bottom Panel").gameObject.SetActive(true);
	}

	public void ShowUpgradeButton(bool b){
		transform.Find("Button_Upgrade").gameObject.SetActive(b);
	}

	public void UpgradeToLevel(int i){
		ShowUpgradeButton(false);
		textLevel.text = "HEAT LEVEL" + i;
	}

	public void TogglePause(){
		// pause button doesn't work when tutorial's showing
		if (tutorialShowing){
			return;
		}

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
				if (au.isPlaying){
					pausedAudios.Add(au);
					au.Pause();
				}
			}
		}
	}
}
