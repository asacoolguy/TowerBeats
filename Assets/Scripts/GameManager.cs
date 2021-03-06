﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
    GameManager is a singleton that oversees the goings of the entire game. 
    It handles handling inputs and cameras based on the current gamestate.
    It loads info about the stage selected into other Manager classes.
     */

public class GameManager : MonoBehaviour {
    // define singleton behavior
    public static GameManager instance;

    // other scripts
    [System.NonSerialized]
    public PrefabDatabase prefabDatabase;
    private UIManager uiManager;
    private EnemyManager enemyManager;
	private AudioSource audioSource, menuAudioSource;
    private Animator cameraAnimator, centralTowerAnimator;
    private Scanner scanner;
    private LevelDatabase levelData;
    private LevelSelector levelSelector;
    private EnemyPath enemyPath;

    // references to objs
	public GameObject homeBase, enemyPathObj;
    public LayerMask selectableLayerMask;
    
    private TowerPlatform hoveredPlatform = null;
    private TowerPlatform selectedPlatform = null;
    private BuildPanel towerBuildPanel = null;
    private UpgradePanel towerUpgradePanel = null;
    public GameObject towerPlatformGrid;
    

	// game progression variables
	public float currentScore = 0;
	public float totalScore = 0;
	public int maxHealth;
	private int currentMoney, maxWave, currentWave, currentHealth;
    public int currentStage;
    private bool gameOver = false;
    

    public enum GameState { SplashScreenDisplaying, SplashScreen, LevelScreen, GameScreen, PauseScreen, ResultScreenDisplaying, ResultScreen};
    public GameState state;

    public bool devMode;

    private void Awake() {
        // set up singleton behavior
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        levelData = GetComponent<LevelDatabase>();
    }

    private void Start () {
        Time.timeScale = 1;

        // set up some variables
        audioSource = transform.Find("Audio").GetComponent<AudioSource>();
        menuAudioSource = transform.Find("MenuAudio").GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentWave = 0;
        currentStage = -1; // -1 for no currentStage

        // set up references to essential scripts
        prefabDatabase = GetComponent<PrefabDatabase>();
        enemyManager = FindObjectOfType<EnemyManager>();
        uiManager = FindObjectOfType<UIManager>();
        cameraAnimator = Camera.main.GetComponentInParent<Animator>();
        scanner = FindObjectOfType<Scanner>();
        centralTowerAnimator = homeBase.transform.parent.GetComponent<Animator>();
        levelSelector = homeBase.transform.parent.GetComponentInChildren<LevelSelector>();
        enemyPath = enemyPathObj.GetComponent<EnemyPath>();

        // make the tower build panel and give it the correct AOEIndicators
        towerBuildPanel = Instantiate(prefabDatabase.buildPanel).GetComponent<BuildPanel>();
        towerBuildPanel.gameObject.SetActive(false);
        for (int i = 0; i < 3; i++) {
            // set up the appropriate AOEIndicator
            GameObject AOEIndicatorPrefab = prefabDatabase.GetTower(i).transform.Find("AOEIndicator").gameObject;
            GameObject AOEIndicator = Instantiate(AOEIndicatorPrefab);
            AOEIndicator.transform.localScale = AOEIndicatorPrefab.transform.lossyScale;
            AOEIndicator.transform.parent = towerBuildPanel.transform;
            AOEIndicator.transform.localPosition = Vector3.zero;
            AOEIndicator.SetActive(false);
            towerBuildPanel.AOEIndicators.Add(AOEIndicator);

            // set up the right cost
            towerBuildPanel.SetButtonCost(i, prefabDatabase.GetTower(i).GetComponent<BasicTower>().info.costs[0]);
        }

        // make the tower upgraePanel 
        towerUpgradePanel = Instantiate(prefabDatabase.upgradePanel).GetComponent<UpgradePanel>();
        towerUpgradePanel.gameObject.SetActive(false);

        // load the level menu
        levelSelector.SetupMenu(levelData.levelData.Length);

        if (devMode) {
            state = GameState.GameScreen;

            // load info for stage 1
            int testStage = 1;
            currentStage = testStage;
            currentMoney = levelData.levelData[currentStage].startingMoney;

            // load scannerMusic info
            scanner.SetupScanner();

            // load the spawn info
            string[] spawnPatterns = levelData.levelData[testStage].spawnPatterns;
            maxWave = spawnPatterns.Length;
            enemyManager.SetSpawnInstruction(spawnPatterns);

            // load the enemy path info
            for (int j = 0; j < levelData.levelData[testStage].enemyPaths.Length; j++) {
                enemyPath.AddNewPath(levelData.levelData[testStage].enemyPaths[j]);
            }

            // load the towerPlatform info
            for (int j = 0; j < levelData.levelData[testStage].platformData.Length; j++) {
                GameObject obj = Instantiate(prefabDatabase.towerPlatform, levelData.levelData[testStage].platformData[j], prefabDatabase.towerPlatform.transform.rotation, towerPlatformGrid.transform);
                obj.SetActive(true);
            }

            centralTowerAnimator.SetTrigger("Rise");

            uiManager.ShowGUI(true, true);
            uiManager.UpdateHealth(currentHealth, maxHealth);
            uiManager.UpdateMoney(currentMoney);
            cameraAnimator.SetTrigger("SkipToGame");

            scanner.ShowScannerLine(true);
            scanner.SetRotate(true);
        }
        else {
            state = GameState.SplashScreenDisplaying;
            // initiate the camera with its splash screen
            uiManager.StartCoroutine(uiManager.DisplaySplashScreenWithDelay(true, 0.5f));
            StartCoroutine(PlayThemeWithDelay(0.5f));
        }
    }


    private void Update() {
        if (state == GameState.GameScreen) {
            // if enemyManager is done with the current wave, advance to the next wave
            if (enemyManager.waveDone) {
                currentWave++;
                scanner.ChangeSoundPhase(+1);

                if (currentWave >= maxWave) {
                    if (!gameOver) {
                        gameOver = true;
                        StartCoroutine(WinGame());
                    }
                }
                else {
                    SpawnWave();
                }
            }

            // highlight any TowerPlatforms the mouse is hovering over that's not already built on
            TowerPlatform newHoveredPlatform = GetPlatformFromMouse();
            if (hoveredPlatform != newHoveredPlatform) {
                if (hoveredPlatform != null) {
                    hoveredPlatform.LowerPlatform();
                }
                if (newHoveredPlatform != null) {
                    newHoveredPlatform.RaisePlatform();
                }
                hoveredPlatform = newHoveredPlatform;
            }


            if (towerBuildPanel.gameObject.activeSelf) {
                // highlight any BuildPanelButtons the mouse is hovering over
                towerBuildPanel.HighlightButton(GetBuildPanelFromMouse());

                // enable/disable BuildPanelButtons based on money
                for (int i = 0; i < 3; i++) {
                    if (currentMoney >= prefabDatabase.GetTower(i).GetComponent<BasicTower>().info.costs[0]) {
                        towerBuildPanel.EnableButton(i, true);
                    }
                    else {
                        towerBuildPanel.EnableButton(i, false);
                    }
                }
            }

            if (towerUpgradePanel.gameObject.activeSelf) {
                // highlight any upgradePanelButtons the mouse is hovering over
                towerUpgradePanel.HighlightButton(GetUpgradePanelFromMouse());

                // enable/disable upgradePanelButtons based on money
                towerUpgradePanel.EnableButton(0, currentMoney >= towerUpgradePanel.GetUpgradeCost());
                towerUpgradePanel.EnableButton(1, true);
            }
        }
    }

    private void LateUpdate() {
        if (state == GameState.SplashScreen && Input.GetMouseButtonDown(0)) {
            state = GameState.LevelScreen;
            uiManager.StartCoroutine(uiManager.DisplaySplashScreenWithDelay(false));
            cameraAnimator.SetTrigger("SplashToLevel");
            centralTowerAnimator.SetTrigger("Rise");
            levelSelector.ShowLevelSelection(true);
        }
        else if (state == GameState.GameScreen) {
            // handle clicking events
            if (Input.GetMouseButtonDown(0)) {
                int buildPanelButtonClicked = GetBuildPanelFromMouse();
                int upgradePanelButtonClicked = GetUpgradePanelFromMouse();
                // if we clicked on a BuildablePanel that's enabled, build that tower
                if (buildPanelButtonClicked >= 0) {
                    if (towerBuildPanel.IsButtonEnabled(buildPanelButtonClicked)) {
                        BuildTower(buildPanelButtonClicked);
                        towerBuildPanel.ActivatePanel(false);

                        // deselect any selectedOctagons
                        if (selectedPlatform) {
                            selectedPlatform.SelectPlatform(false);
                            selectedPlatform = null;
                        }
                    }
                }
                // if we clicked on a UpgradePanel that's enabled, do that upgrade
                else if (upgradePanelButtonClicked >= 0) {
                    if (towerUpgradePanel.IsButtonEnabled(upgradePanelButtonClicked)) {
                        towerUpgradePanel.HandleButtonClick(upgradePanelButtonClicked);
                        towerUpgradePanel.ActivatePanel(false);

                        // deselect any selectedOctagons
                        if (selectedPlatform) {
                            selectedPlatform.SelectPlatform(false);
                            selectedPlatform = null;
                        }
                    }
                }
                // if you clicked somewhere random or on the selected Octagon, deselect the selectedPlatform
                else if (hoveredPlatform == null || hoveredPlatform == selectedPlatform) {
                    //towerBuildPanel.transform.parent = null;
                    towerBuildPanel.ActivatePanel(false);
                    towerUpgradePanel.ActivatePanel(false);
                    // deselect any selectedOctagons
                    if (selectedPlatform) {
                        selectedPlatform.SelectPlatform(false);
                        selectedPlatform = null;
                    }
                }
                // if the clicked on hoverOctagon 
                else if (hoveredPlatform && hoveredPlatform != selectedPlatform) {
                    if (hoveredPlatform.IsBuiltOn()) {
                        // this one is already built on, select it and show the upgrade Panel
                        towerUpgradePanel.ActivatePanel(true);
                        towerBuildPanel.ActivatePanel(false);
                        towerUpgradePanel.transform.SetParent(hoveredPlatform.transform, true);
                        towerUpgradePanel.SetButtonInfo(hoveredPlatform.getBuiltTower());
                        towerUpgradePanel.transform.localPosition = new Vector3(0, 1.2f, 0);
                        // TODO: get info from this platform's tower
                    }
                    else {
                        // this hoverOctagon is not yet selected or built on, select it and show the build panel
                        towerBuildPanel.ActivatePanel(true);
                        towerUpgradePanel.ActivatePanel(false);
                        towerBuildPanel.transform.SetParent(hoveredPlatform.transform, true);
                        //towerBuildPanel.transform.parent = hoveredPlatform.transform;
                        towerBuildPanel.transform.localPosition = new Vector3(0, 1.2f, 0);
                    }

                    // set the new selectedPlatform
                    if (selectedPlatform) {
                        selectedPlatform.SelectPlatform(false);
                    }
                    selectedPlatform = hoveredPlatform;
                    selectedPlatform.SelectPlatform(true);
                }
            }

            if (devMode) {
                if (state == GameState.GameScreen && Input.GetKeyDown(KeyCode.Space)) {
                    StartCoroutine(WinGame());
                }
                else if (Input.GetKeyDown(KeyCode.F)) {
                    Time.timeScale += 0.5f;
                }
                else if (Input.GetKeyDown(KeyCode.V)) {
                    Time.timeScale -= 0.5f;
                }
            }
        }
        else if (state == GameState.ResultScreen && Input.GetMouseButtonDown(0)) {
            StartCoroutine(ResetGame());
        }
    }


    // select the right tower to build using index
    public void BuildTower(int input) {
        if (input > 3) {
            print("index out of bounds");
            return;
        }

        if (selectedPlatform == null) {
            print("no octagon selected");
            return;
        }

        // build the tower
        GameObject towerObj = Instantiate(prefabDatabase.GetTower(input)) as GameObject;
        towerObj.SetActive(true);

        // parent it to selectedPlatform and set its position
        towerObj.transform.SetParent(selectedPlatform.transform, true);
        towerObj.transform.localPosition = Vector3.zero;

        selectedPlatform.SetBuiltTower(towerObj);

        BasicTower tower = towerObj.GetComponent<BasicTower>();

        GainMoney(-tower.info.costs[0]);

        // do some stuff about finding the right list to add tower to
        int axisIndex = scanner.FindClosestAxisIndex(tower.transform.position);
        tower.axisIndex = axisIndex;
        scanner.AddTowerToList(towerObj);
    }


    // returns the TowerPlatform that the mouse is currently hovering over
    // only valid for built towers
    private TowerPlatform GetPlatformFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, selectableLayerMask)) {
            // trace parents until we find the object with TowerPlatform script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            //print("initial hit is " + current.name);
            while (current.GetComponent<TowerPlatform>() == null &&
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
            if (current.GetComponent<TowerPlatform>() != null) {
                //print("script returned");
                return current.GetComponent<TowerPlatform>();
            }
            //print("no script");
        }

        // print("no hit ");
        // otherwise no tower hit
        return null;
    }


    // returns the buildPanel button the mouse is currently hovering over
    // -1 if no buildPanel
    private int GetBuildPanelFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, selectableLayerMask)) {
            // trace parents until we find the object with BuildPanel script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            //print("initial hit is " + current.name);
            while (current.GetComponent<BuildPanel>() == null &&
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
            if (current.GetComponent<BuildPanel>() != null) {
                //print("script returned");
                GameObject button = hit.collider.transform.parent.gameObject;
                string name = button.name.Substring(button.name.Length - 1);
                return int.Parse(name) - 1;
            }
            //print("no script");
        }

        // print("no hit ");
        // otherwise no tower hit
        return -1;
    }


    // returns the upgradePanel button the mouse is currently hovering over
    // -1 if no upgradePanel
    private int GetUpgradePanelFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, selectableLayerMask)) {
            // trace parents until we find the object with UpgradePanel script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            //print("initial hit is " + current.name);
            while (current.GetComponent<UpgradePanel>() == null &&
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
            if (current.GetComponent<UpgradePanel>() != null) {
                //print("script returned");
                GameObject button = hit.collider.transform.parent.gameObject;
                return button.name == "Upgrade" ? 0 : 1;
            }
            //print("no script");
        }

        // print("no hit ");
        // otherwise no tower hit
        return -1;
    }

    // called when the homeBase takes damage. spawns a restart button when game over.
    // passes the UI stuff to the UIManager
    public void TakeDamage(int i) {
        currentHealth -= i;
        uiManager.UpdateHealth(currentHealth, maxHealth);
        homeBase.GetComponent<AudioSource>().Play();
        homeBase.GetComponent<Animator>().SetTrigger("TakeDamage");
        centralTowerAnimator.SetTrigger("TakeDamage");

        if (currentHealth <= 0) {
            // game over!
            foreach (AudioSource a in FindObjectsOfType<AudioSource>()) {
                a.Stop();
            }

            audioSource.clip = levelData.takeDamageClip;
            audioSource.Play();
            state = GameState.ResultScreenDisplaying;
            StartCoroutine(uiManager.DisplayGameResultScreen(true, false, totalScore));
            uiManager.ShowGUI(false);
            Time.timeScale = 0;
        }
    }

    // starts spawning enemies 
    public void SpawnWave() {
		uiManager.ShowSpawnButton(false);

        enemyManager.SetupWave(currentWave);

		uiManager.UpdateWave(currentWave, maxWave);
	}


    public void GainPoints(float pts){
		currentScore += pts;
	}


    public void GainMoney(int money){
        currentMoney += money;
        uiManager.UpdateMoney(currentMoney);
    }


    public int GetMoney() {
        return currentMoney;
    }

    public void SpendMoney(int spent) {
        currentMoney -= spent;
        uiManager.UpdateMoney(currentMoney);
    }


    // load relevant stage info and set up the scene
    public void LoadStage(int i) {
        currentStage = i;

        // reset all scores and stuff
        currentHealth = maxHealth;
        currentMoney = levelData.levelData[currentStage].startingMoney;
        currentScore = totalScore = 0;
        currentWave = 0;

        // load scannerMusic info
        scanner.SetupScanner();

        // load the spawn info 
        string[] spawnPatterns = levelData.levelData[i].spawnPatterns;
        maxWave = spawnPatterns.Length;
        enemyManager.SetSpawnInstruction(spawnPatterns);

        // load the enemy path info
        for (int j = 0; j < levelData.levelData[i].enemyPaths.Length; j++) {
            enemyPath.AddNewPath(levelData.levelData[i].enemyPaths[j]);
        }
        enemyPath.ToggleAllPaths(false);

        // load the towerPlatform info
        for (int j = 0; j < levelData.levelData[i].platformData.Length; j++) {
            Instantiate(prefabDatabase.towerPlatform, levelData.levelData[i].platformData[j], prefabDatabase.towerPlatform.transform.rotation, towerPlatformGrid.transform);
        }

        // start the camera movement and get panels to fly in
        StartCoroutine(StartGame());
    }


    private IEnumerator StartGame() {
        // play the game starting sound
        menuAudioSource.Stop();
        audioSource.PlayOneShot(levelData.gameStartClip);
        yield return new WaitForSeconds(levelData.gameStartClip.length - 1);

        // pull the camera up
        audioSource.PlayOneShot(levelData.cameraPullUpClip);
        cameraAnimator.SetTrigger("LevelToGame");
        levelSelector.ShowLevelSelection(false);
        yield return new WaitForSeconds(levelData.cameraPullUpClip.length + 1);

        // fly the panels in and show the GUI
        float flyInDuration = 1f;
        for (int i = 0; i < towerPlatformGrid.transform.childCount; i++) {
            TowerPlatform oct = towerPlatformGrid.transform.GetChild(i).GetComponent<TowerPlatform>();
            oct.gameObject.SetActive(true);
            StartCoroutine(oct.FlyIn(flyInDuration));
            yield return new WaitForSeconds(0.2f);
        }

        // show the GUI when all panels have flown in
        yield return new WaitForSeconds(flyInDuration);
        uiManager.ShowGUI(true);
        uiManager.UpdateHealth(currentHealth, maxHealth);
        uiManager.UpdateMoney(currentMoney);
        uiManager.UpdateWave(currentWave, maxWave);

        yield return new WaitForSeconds(1.5f);
        enemyPath.ToggleAllPaths(true);

        yield return new WaitForSeconds(1.5f);

        if (currentStage == 0) {
            uiManager.DisplayTutorialScreen(true);

            while (uiManager.IsTutorialShowing()) {
                yield return null;
            }

            yield return new WaitForSeconds(1);
        }

        scanner.ShowScannerLine(true);
        scanner.SetRotate(true);
        state = GameState.GameScreen;
        enemyPath.ToggleAllPaths(true);
    }


    private IEnumerator WinGame() {
        // stop scanner in 1 rotation
        StartCoroutine(scanner.StopScannerRotation(1));
        while (scanner.rotating == true) {
            yield return null;
        }

        // add remaining money and health to score
        currentScore += currentMoney;
        currentScore *= 1f + currentHealth / maxHealth;

        // play the end game sound and zoom out
        state = GameState.ResultScreenDisplaying;
        uiManager.ShowGUI(false);
        cameraAnimator.SetTrigger("GameToResult");
        audioSource.PlayOneShot(levelData.levelData[currentStage].musicData.ending);

        while (audioSource.isPlaying) {
            yield return null;
        }

        // show the game win screen
        StartCoroutine(uiManager.DisplayGameResultScreen(true, true, currentScore));

        Time.timeScale = 0;
    }


    private IEnumerator PlayThemeWithDelay(float delay) {
        Camera.main.transform.parent.GetChild(1).gameObject.SetActive(true);

        yield return new WaitForSeconds(delay);
        menuAudioSource.Play();
        //FindObjectOfType<CameraMover>().ToggleBlankScreen(false);
        Camera.main.transform.parent.GetChild(1).gameObject.SetActive(false);
    }


    private IEnumerator ResetGame() {
        Time.timeScale = 1;
        gameOver = false;
        scanner.SetRotate(false);
        state = GameState.LevelScreen;

        enemyManager.DestroyAllEnemies();
        enemyPath.Reset();
        enemyPath.ToggleAllPaths(false);
        towerBuildPanel.ActivatePanel(false);
        towerUpgradePanel.ActivatePanel(false);
        uiManager.StartCoroutine(uiManager.DisplayGameResultScreen(false));

        // make all octagons fall and lower the central tower
        centralTowerAnimator.SetTrigger("Lower");
        audioSource.volume = 0.6f;
        audioSource.PlayOneShot(levelData.powerdownClip);
        foreach (TowerPlatform oct in FindObjectsOfType<TowerPlatform>()) {
            StartCoroutine(oct.FallOff());
        }
        yield return new WaitForSeconds(2.8f);
        audioSource.volume = 1;

        cameraAnimator.enabled = true;
        cameraAnimator.SetTrigger("ResultToLevel");
        centralTowerAnimator.SetTrigger("Rise");
        levelSelector.ShowLevelSelection(true, currentStage);

        // clear all towers
        scanner.DestroyAllTowers();

        // wait for the camera to go back to level and play the main theme
        yield return new WaitForSeconds(2.6f);
        menuAudioSource.Play();
    }


    public void QuitGame() {
        print("quitting game");
        Application.Quit();
    }


    public void ReturnToMenu() {
        uiManager.ShowGUI(false);
        uiManager.DisplayPauseScreen(false);        
        StartCoroutine(ResetGame());
    }

    public MusicDatabase GetMusicDatabase() {
        if (currentStage >= 0) {
            return levelData.levelData[currentStage].musicData;
        }
        else {
            return null;
        }
    }


    public AudioClip[] GetEnemyAudioClips() {
        return levelData.levelData[currentStage].musicData.enemyDeathClips;
    }


    public Scanner GetScanner() {
        return scanner;
    }


    public int GetCurrentWave() {
        return currentWave;
    }

	public static float GetAngleFromVector(Vector3 pos) {
		float angle = 0f;

		if (pos.x == 0) {
			if (pos.z > 0) {
				angle = 90f;
			}
			else {
				angle = -90f;
			}
		}
		else if(pos.z == 0) {
			if (pos.x > 0) {
				angle = 0f;
			}
			else {
				angle = 180f;
			}
		}
		else {
			angle = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
		}
		if (angle < 0) {
			angle += 360f;
		}

		return angle;
	}


    public static Vector3 SmoothStep(Vector3 start, Vector3 end, float t) {
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        return start + (end - start) * t;
    }

    public static float SmoothStep(float start, float end, float t) {
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        return start + (end - start) * t;
    }

    public static float EaseOut(float start, float end, float t) {
        t--;
        t = t * t * t + 1;
        return start + (end - start) * t;
    }

}
