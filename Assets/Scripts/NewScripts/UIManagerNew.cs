/*
    General UI manager that oversees UI activities for the whole game
 */ 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManagerNew : MonoBehaviour {
    public GameObject textMoneyObj, healthTextObj, textWaveObj;
    public List<AudioSource> pausedAudios;
    private bool paused = false;

    private Text textHealth, textWave, textMoney;
    private GameObject gameOverBox, gameWinBox, pauseScreen;
    private GameManagerNew gameManager;

    // UI sound effects
    public AudioClip buttonEnable, buttonPress;


    void Start() {
        gameManager = FindObjectOfType<GameManagerNew>();
        textHealth = healthTextObj.GetComponent<Text>();
        textHealth.text = "LIVES: " + gameManager.maxHealth + " / " + gameManager.maxHealth;
        textWave = textWaveObj.GetComponent<Text>();
        textMoney = textMoneyObj.GetComponent<Text>();
        textMoney.text = "Total $: " + gameManager.startingMoney;
        gameOverBox = transform.Find("GameOverBox").gameObject;
        gameOverBox.SetActive(false);
        gameWinBox = transform.Find("GameWinBox").gameObject;
        gameWinBox.SetActive(false);
        pauseScreen = transform.Find("PauseScreen").gameObject;

        // show the game start button and reset the progress bar
        transform.Find("StartButton").gameObject.SetActive(true);
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePause();
        }
    }

    public void UpdateHealth(int i) {
        textHealth.text = "LIVES: " + i + " / " + gameManager.maxHealth;
    }

    public void UpdateMoney(int i) {
        textMoney.text = "Total $: " + gameManager.currentMoney;
    }

    public void DisplayGameOverScreen() {
        gameOverBox.SetActive(true);
    }

    public void DisplayGameWinScreen(float score) {
        gameWinBox.SetActive(true);
        gameWinBox.transform.Find("Text_Score").GetComponent<Text>().text = "SCORE: " + (int)score;
    }

    public void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void LoadMainMenu() {
        SceneManager.LoadScene("Main Menu");
    }


    public void StartMusic() {
        transform.Find("StartButton").gameObject.SetActive(false);
    }


    public void ShowSpawnButton(bool b) {
        transform.Find("SpawnButton").gameObject.SetActive(b);
    }


    public void UpdateWaveText(int i) {
        textWave.text = "Wave: " + i;
    }

    public void TogglePause() {
        PlayButtonPressSound();
        if (paused) {
            paused = false;
            Time.timeScale = 1;
            pauseScreen.SetActive(false);

            // unpause all audiosources
            foreach (AudioSource au in pausedAudios) {
                au.UnPause();
            }
        }
        else if (!paused) {
            paused = true;
            Time.timeScale = 0;
            pauseScreen.SetActive(true);

            // pause all audiosources
            pausedAudios = new List<AudioSource>();
            foreach (AudioSource au in FindObjectsOfType<AudioSource>()) {
                if (au != this.GetComponent<AudioSource>() && au.isPlaying) {
                    pausedAudios.Add(au);
                    au.Pause();
                }
            }
        }
    }

    public void PlayButtonEnableSound() {
        GetComponent<AudioSource>().PlayOneShot(buttonEnable);
    }

    public void PlayButtonPressSound() {
        GetComponent<AudioSource>().PlayOneShot(buttonPress);
    }
}
