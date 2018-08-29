using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
	// GUI
	public GameObject healthTextObj, moneyTextObj, waveTextObj;
	private Text healthText, moneyText, waveText;
	public GameObject spawnButton, waveTestButtons;

    // screens/panels
    public GameObject controlPanel, statusPanel, wavePanel, pauseScreen;

    // menus
    public GameObject splashScreen, gameResultScreen;

    public List<AudioSource> pausedAudios;
	private bool paused = false;

	// UI sound effects
	public AudioClip buttonEnable, buttonPress, bigThud, smallThud;


	// Use this for initialization
	private void Awake () {
		healthText = healthTextObj.GetComponent<Text>();
		waveText = waveTextObj.GetComponent<Text>();
		moneyText = moneyTextObj.GetComponent<Text>();

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

        if (waveTestButtons.activeSelf) {
            waveTestButtons.transform.Find("Text").GetComponent<Text>().text = "wave: " + (GameManager.instance.GetScanner().GetSongPhase() + 1);
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
		spawnButton.SetActive(b);

        if (b) {
            StopCoroutine(PopSpawnButton());
            StartCoroutine(PopSpawnButton(20));
        }
        else if (!b && spawnButton.activeSelf){
            StopCoroutine(PopSpawnButton());
            spawnButton.GetComponent<Animator>().SetBool("Popping", false);
        }
	}


    private IEnumerator PopSpawnButton(float delay = 0f) {
        yield return new WaitForSeconds(delay);
        if (spawnButton.activeSelf) {
            spawnButton.GetComponent<Animator>().SetBool("Popping", true);
        }
    }
    

    public void ShowGUI(bool b, bool devMode = false) {
        controlPanel.gameObject.SetActive(b);
        statusPanel.gameObject.SetActive(b);
        wavePanel.gameObject.SetActive(b);

        ShowSpawnButton(b);
        //ShowSpawnButton(!devMode);
        waveTestButtons.SetActive(devMode);
    }


    public void ChangeSongPhase(int i) {
        GameManager.instance.GetScanner().ChangeSoundPhase(i);
    }


	public IEnumerator DisplayGameResultScreen(bool display, bool winLose = true, float score = 0){
        if (display) {
            gameResultScreen.transform.Find("MainText").GetComponent<Text>().text = winLose ? "Level Complete" : "Game Over";
            gameResultScreen.transform.Find("ScoreText").GetComponent<Text>().text = "Score : " + score;

            for (int i = 0; i < gameResultScreen.transform.childCount; i++) {
                gameResultScreen.transform.GetChild(i).gameObject.SetActive(true);
                GetComponent<AudioSource>().PlayOneShot(bigThud);
                yield return new WaitForSecondsRealtime(1);
            }
            GameManager.instance.state = GameManager.GameState.ResultScreen;

        }
        else {
            for (int i = 0; i < gameResultScreen.transform.childCount; i++) {
                gameResultScreen.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
	}


    public IEnumerator DisplaySplashScreenWithDelay(bool b, float delay = 0f) {
        if (b) {
            yield return new WaitForSeconds(delay);
            splashScreen.SetActive(true);
            splashScreen.GetComponent<Animator>().SetTrigger("FadeIn");

            // sets the mode to splashScreen after the intro animation is done playing
            yield return new WaitForEndOfFrame(); // wait for a frame for the animator state to kick in
            yield return new WaitForSeconds(splashScreen.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
            GameManager.instance.state = GameManager.GameState.SplashScreen;
        }
        else {
            splashScreen.GetComponent<Animator>().SetTrigger("FadeOut");
        }
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


    public void DisplayPauseScreen(bool b) {
        paused = b;
        pauseScreen.SetActive(b);
    }

	public void PlayButtonPressSound(){
		GetComponent<AudioSource>().PlayOneShot(buttonPress);
	}
}
