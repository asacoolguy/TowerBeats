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


    public IEnumerator DisplaySplashScreenWithDelay(bool b, float delay = 0f) {
        if (b) {
            yield return new WaitForSeconds(delay);
            splashScreen.SetActive(true);
            splashScreen.GetComponent<Animator>().SetTrigger("FadeIn");

            // sets the mode to splashScreen after the intro animation is done playing
            yield return new WaitForEndOfFrame(); // wait for a frame for the animator state to kick in
            yield return new WaitForSeconds(splashScreen.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
            FindObjectOfType<GameManager>().state = GameManager.GameState.SplashScreen;
        }
        else {
            splashScreen.GetComponent<Animator>().SetTrigger("FadeOut");
        }
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
