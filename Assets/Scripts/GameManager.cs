using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Game Manager class that handles spawning towers at the right locations and tracking scores & progression
/// </summary>

public class GameManager : MonoBehaviour {
    // other scripts
	private UIManager uiManager;
    private EnemyManager enemyManager;
	private AudioSource audioSource, menuAudioSource;
    private Animator cameraAnimator, centralTowerAnimator;
    private Scanner scanner;
    private LevelSelector levelSelector;
    private EnemyPath enemyPath;

    // references to objs
	public List<GameObject> buildableTowers;
	public GameObject homeBase, enemyPathObj;
    public LayerMask selectableLayerMask;
    
    private TowerPlatform hoveredPlatform = null;
    private TowerPlatform selectedPlatform = null;
    public GameObject towerBuildPanelPrefab;
    private BuildPanel towerBuildPanel = null;
    public GameObject towerUpgradePanelPrefab;
    private UpgradePanel towerUpgradePanel = null;
    public GameObject towerPlatformGrid, towerPlatformPrefab;
    private LevelDatabase levelData;

	// game progression variables
	public float currentScore = 0;
	public float totalScore = 0;
	public int startingMoney, maxHealth;
	private int currentMoney, maxWave, currentWave, currentHealth;
    public int currentStage;
    private bool gameOver = false;

    // Audio clips used for the game
	private AudioClip youWinClip, youLoseClip, gameStartClip, wooshClip;


    public enum GameState { SplashScreenDisplaying, SplashScreen, LevelScreen, GameScreen, PauseScreen, ResultScreenDisplaying, ResultScreen};
    public GameState state;

    public bool devMode;

    private void Awake() {
        // set up music clips
        levelData = transform.Find("LevelDatabase").GetComponent<LevelDatabase>();
        youWinClip = levelData.youWinClip;
        youLoseClip = levelData.youLoseClip;
        gameStartClip = levelData.gameStartClip;
        wooshClip = levelData.wooshClip;
    }

    private void Start () {
        Time.timeScale = 1;

        // set up some variables
        audioSource = transform.Find("Audio").GetComponent<AudioSource>();
        menuAudioSource = transform.Find("MenuAudio").GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentMoney = startingMoney;
        currentWave = 0;
        currentStage = -1; // -1 for no currentStage

        // set up references to essential scripts
        enemyManager = FindObjectOfType<EnemyManager>();
        uiManager = FindObjectOfType<UIManager>();
        cameraAnimator = FindObjectOfType<CameraMover>().GetComponent<Animator>();
        scanner = FindObjectOfType<Scanner>();
        centralTowerAnimator = homeBase.transform.parent.GetComponent<Animator>();
        levelSelector = homeBase.transform.parent.GetComponentInChildren<LevelSelector>();
        enemyPath = enemyPathObj.GetComponent<EnemyPath>();

        // make the tower build panel and give it the correct AOEIndicators
        towerBuildPanel = Instantiate(towerBuildPanelPrefab).GetComponent<BuildPanel>();
        towerBuildPanel.gameObject.SetActive(false);
        for (int i = 0; i < buildableTowers.Count; i++) {
            // set up the appropriate AOEIndicator
            GameObject AOEIndicatorPrefab = buildableTowers[i].transform.Find("AOEIndicator").gameObject;
            GameObject AOEIndicator = Instantiate(AOEIndicatorPrefab);
            AOEIndicator.transform.localScale = AOEIndicatorPrefab.transform.lossyScale;
            AOEIndicator.transform.parent = towerBuildPanel.transform;
            AOEIndicator.transform.localPosition = Vector3.zero;
            AOEIndicator.SetActive(false);
            towerBuildPanel.AOEIndicators.Add(AOEIndicator);
            // set up the right cost
            towerBuildPanel.SetButtonCost(i, buildableTowers[i].GetComponent<BasicTower>().info.costs[0]);
        }

        // make the tower upgraePanel 
        towerUpgradePanel = Instantiate(towerUpgradePanelPrefab).GetComponent<UpgradePanel>();
        towerUpgradePanel.gameObject.SetActive(false);

        // load the level menu
        levelSelector.SetupMenu(levelData.levelData.Length);

        if (devMode) {
            state = GameState.GameScreen;
            currentMoney = 9999;

            // load info for stage 1
            int testStage = 2;
            currentStage = testStage;

            // load scannerMusic info
            scanner.SetupScanner();

            // load the spawn info
            string[] spawnPatterns = levelData.levelData[testStage].spawnPattern.Split('\n');
            maxWave = levelData.levelData[testStage].totalWaves;
            enemyManager.SetSpawnInstruction(spawnPatterns);

            // load the enemy path info
            for (int j = 0; j < levelData.levelData[testStage].enemyPaths.Length; j++) {
                enemyPath.AddNewPath(levelData.levelData[testStage].enemyPaths[j]);
            }

            // load the towerPlatform info
            for (int j = 0; j < levelData.levelData[testStage].platformData.Length; j++) {
                GameObject obj = Instantiate(towerPlatformPrefab, levelData.levelData[testStage].platformData[j], towerPlatformPrefab.transform.rotation, towerPlatformGrid.transform);
                obj.SetActive(true);
            }

            centralTowerAnimator.SetTrigger("Rise");

            uiManager.ShowGUI(true, true);
            cameraAnimator.SetTrigger("SkipToGame");

            scanner.ShowScannerLine(true);
            scanner.SetRotate(true);
            FindObjectOfType<CameraMover>().ToggleBlankScreen(false);
        }
        else {
            state = GameState.SplashScreenDisplaying;
            // initiate the camera with its splash screen
            uiManager.StartCoroutine(uiManager.DisplaySplashScreenWithDelay(true, 2f));
            StartCoroutine(PlayThemeWithDelay(2f));
        }
    }


    private void Update() {
        if (state == GameState.GameScreen) {
            // if enemyManager is done with the current wave, advance to the next wave
            if (enemyManager.waveDone) {
                currentWave++;
                scanner.changeSoundPhase(+1);

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
                for (int i = 0; i < buildableTowers.Count; i++) {
                    if (currentMoney >= buildableTowers[i].GetComponent<BasicTower>().info.costs[0]) {
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
                    }
                }
                // if we clicked on a UpgradePanel that's enabled, do that upgrade
                else if (upgradePanelButtonClicked >= 0) {
                    if (towerUpgradePanel.IsButtonEnabled(upgradePanelButtonClicked)) {
                        towerUpgradePanel.HandleButtonClick(upgradePanelButtonClicked);
                        towerUpgradePanel.ActivatePanel(false);
                    }
                    // deselect any selectedOctagons
                    if (selectedPlatform) {
                        selectedPlatform.SelectPlatform(false);
                        selectedPlatform = null;
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
            }
        }
        else if (state == GameState.ResultScreen && Input.GetMouseButtonDown(0)) {
            StartCoroutine(ResetGame());
        }
    }


    // select the right tower to build using index
    public void BuildTower(int input) {
        if (input > buildableTowers.Count) {
            print("index out of bounds");
            return;
        }

        if (selectedPlatform == null) {
            print("no octagon selected");
            return;
        }

        // build the tower
        GameObject towerObj = Instantiate(buildableTowers[input]) as GameObject;
        towerObj.SetActive(true);

        // parent it to selectedPlatform and set its position and rotation accordingly
        // TODO: need better way of handling tower positioning
        Vector3 pos = Vector3.zero;
        if (input == 0) pos = new Vector3(0, 4.4f, 0);
        else if (input == 1) pos = new Vector3(0, 3.6f, 0);
        else if (input == 2) pos = new Vector3(0, 5f, 0);
        float angle = 180f - GameManager.GetAngleFromVectorSpecial(-selectedPlatform.transform.position);
        towerObj.transform.eulerAngles = new Vector3(0, angle, 0);
        towerObj.transform.SetParent(selectedPlatform.transform, true);
        towerObj.transform.localPosition = pos;

        // link tower to selectedPlatform and deselect it
        selectedPlatform.SetBuiltTower(towerObj);
        selectedPlatform.SelectPlatform(false);
        selectedPlatform = null;

        BasicTower tower = towerObj.GetComponent<BasicTower>();
        tower.ToggleOutline(false);

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

        if (currentHealth <= 0) {
            // game over!
            foreach (AudioSource a in FindObjectsOfType<AudioSource>()) {
                a.Stop();
            }

            audioSource.clip = youLoseClip;
            audioSource.Play();
            state = GameState.ResultScreenDisplaying;
            StartCoroutine(uiManager.DisplayGameResultScreen(true, false, totalScore));
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
        currentMoney = startingMoney;
        currentScore = totalScore = 0;
        currentWave = 0;

        // load scannerMusic info
        scanner.SetupScanner();

        // load the spawn info 
        string[] spawnPatterns = levelData.levelData[i].spawnPattern.Split('\n');
        maxWave = levelData.levelData[i].totalWaves;
        enemyManager.SetSpawnInstruction(spawnPatterns);

        // load the enemy path info
        for (int j = 0; j < levelData.levelData[i].enemyPaths.Length; j++) {
            enemyPath.AddNewPath(levelData.levelData[i].enemyPaths[j]);
        }
        enemyPath.ToggleAllPaths(false);

        // load the towerPlatform info
        for (int j = 0; j < levelData.levelData[i].platformData.Length; j++) {
            Instantiate(towerPlatformPrefab, levelData.levelData[i].platformData[j], towerPlatformPrefab.transform.rotation, towerPlatformGrid.transform);
        }

        // start the camera movement and get panels to fly in
        StartCoroutine(StartGame());
    }


    private IEnumerator StartGame() {
        // play the game starting sound
        menuAudioSource.Stop();
        audioSource.PlayOneShot(gameStartClip);
        yield return new WaitForSeconds(gameStartClip.length + 1);

        // pull the camera up
        audioSource.PlayOneShot(wooshClip);
        cameraAnimator.SetTrigger("LevelToGame");
        levelSelector.ShowLevelSelection(false);
        yield return new WaitForSeconds(wooshClip.length + 1);

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

        // add remaining money to score
        currentScore += currentMoney;

        // play the end game sound and zoom out
        uiManager.ShowGUI(false);
        cameraAnimator.SetTrigger("GameToResult");
        audioSource.PlayOneShot(youWinClip);

        while (audioSource.isPlaying) {
            yield return null;
        }

        // show the game win screen
        state = GameState.ResultScreenDisplaying;
        StartCoroutine(uiManager.DisplayGameResultScreen(true, true, currentScore));

        Time.timeScale = 0;
    }


    private IEnumerator PlayThemeWithDelay(float delay) {
        yield return new WaitForSeconds(delay);
        menuAudioSource.Play();
        FindObjectOfType<CameraMover>().ToggleBlankScreen(false);
    }


    private IEnumerator ResetGame() {
        Time.timeScale = 1;
        gameOver = false;
        state = GameState.LevelScreen;

        // make all octagons fall and lower the central tower
        centralTowerAnimator.SetTrigger("Lower");
        foreach (TowerPlatform oct in FindObjectsOfType<TowerPlatform>()) {
            StartCoroutine(oct.FallOff());
        }
        yield return new WaitForSeconds(2);

        uiManager.StartCoroutine(uiManager.DisplayGameResultScreen(false));
        cameraAnimator.SetTrigger("ResultToLevel");
        centralTowerAnimator.SetTrigger("Rise");
        levelSelector.ShowLevelSelection(true);

        // clear all towers, paths and towerplatforms
        scanner.DestroyAllTowers();
        enemyPath.Reset();
        enemyPath.ToggleAllPaths(false);
        //for (int i = towerPlatformGrid.transform.childCount - 1; i >= 0; i--){
        //    Destroy(towerPlatformGrid.transform.GetChild(i).gameObject);
        //}
    }


    public void QuitGame() {
        print("quitting game");
        Application.Quit();
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
        return levelData.enemyDeathClips;
    }


    public static float GetAngleFromVector(Vector3 pos){
		float angle = 0f;

		if (pos.x == 0){
			if (pos.z == 0){
				angle = 0f;
			}
			else{
				angle = 180f;
			}
		}
		else{
			angle = Mathf.Atan2(pos.z, pos.x) * Mathf.Rad2Deg;
		}
		if (angle < 0){
			angle += 360f;
		}

		return angle;
	}

	public static float GetAngleFromVectorSpecial(Vector3 pos) {
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

    public static Color SmoothStep(Color start, Color end, float t) {
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        return Color.Lerp(start, end, t);
    }

    public static float EaseOut(float start, float end, float t) {
        t--;
        t = t * t * t + 1;
        return start + (end - start) * t;
    }

}
