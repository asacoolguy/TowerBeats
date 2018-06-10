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
    public GameObject gameOverBox, gameWinBox, controlPanel, statusPanel, wavePanel;

    // menus
    public GameObject splashScreen, levelSelect;

    public enum MenuState { splashScreen, splashScreenClickable, levelMenu, duringGame, pauseGame, gameWin };
    private MenuState state;
    public float splashToLevelDuration, levelToGameDuration;

    public List<AudioSource> pausedAudios;
	private bool paused = false;

	// UI sound effects
	public AudioClip buttonEnable, buttonPress;


	// Use this for initialization
	private void Awake () {
		healthText = healthTextObj.GetComponent<Text>();
		waveText = waveTextObj.GetComponent<Text>();
		moneyText = moneyTextObj.GetComponent<Text>();
		
		gameOverBox.SetActive(false);
		gameWinBox.SetActive(false);
        ShowGUI(false);
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

        if (state == MenuState.splashScreenClickable && Input.GetMouseButtonDown(0)) {
            state = MenuState.levelMenu;
            splashScreen.transform.GetChild(2).gameObject.SetActive(false);
            FindObjectOfType<CameraMover>().MoveSplashToLevel(splashToLevelDuration);
            FindObjectOfType<CentralOctagon>().GetComponent<Animator>().SetTrigger("Rise");
            FindObjectOfType<LevelSelector>().ShowLevelSelection(true);
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
        controlPanel.gameObject.SetActive(b);
        statusPanel.gameObject.SetActive(b);
        wavePanel.gameObject.SetActive(b);
    }


	public void DisplayGameWinScreen(bool b){
		gameWinBox.SetActive(true);
	}

    public void DisplaySplashScreen(bool b, float delay = 0f) {
        if (b && delay > 0f) {
            StartCoroutine(DisplaySplashScreenTimed(delay));
        }

        splashScreen.SetActive(b);

        if (b) {
            state = MenuState.splashScreen;
        }
    }

    private IEnumerator DisplaySplashScreenTimed(float delay) {
        splashScreen.SetActive(true);
        splashScreen.transform.GetChild(0).gameObject.SetActive(false);
        splashScreen.transform.GetChild(1).gameObject.SetActive(false);
        splashScreen.transform.GetChild(2).gameObject.SetActive(false);

        float wait = 1.64f;

        yield return new WaitForSeconds(delay + wait - 0.1f);
        splashScreen.transform.GetChild(0).gameObject.SetActive(true);

        yield return new WaitForSeconds(wait);
        splashScreen.transform.GetChild(1).gameObject.SetActive(true);

        yield return new WaitForSeconds(wait);
        splashScreen.transform.GetChild(2).gameObject.SetActive(true);
        state = MenuState.splashScreenClickable;

        yield return new WaitForSeconds(wait);
        splashScreen.transform.GetChild(2).GetComponent<Animator>().SetTrigger("Blink");
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
			//pauseScreen.SetActive(false);

			// unpause all audiosources
			foreach(AudioSource au in pausedAudios){
				au.UnPause();
			}
		}
		else if (!paused){
			paused = true;
			Time.timeScale = 0;
			//pauseScreen.SetActive(true);

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
