using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Game Manager class that handles spawning towers at the right locations and tracking scores & progression
/// </summary>

public class GameManager : MonoBehaviour {
	private UIManager uiManager;
    private EnemyManager enemyManager;
	private AudioSource audioSource, menuAudioSource;
    private Animator cameraAnimator;
    private Scanner scanner;

	public List<GameObject> buildableTowers;
	public GameObject homeBase, CentralPlatform;
    public LayerMask selectableLayerMask;
    
    private TowerPlatform hoveredPlatform = null;
    private TowerPlatform selectedPlatform = null;
    public GameObject towerBuildPanelPrefab;
    private BuildPanel towerBuildPanel = null;
    public GameObject buildableGrid;

	// game progression variables
	public float currentScore = 0;
	public float totalScore = 0;
	public int startingMoney, maxHealth;
	private int currentMoney, maxWave, currentWave, currentHealth;
    private bool gameOver = false;

    // Audio clips used for the game
	private AudioClip youWinClip, youLoseClip, gameStartClip, wooshClip;

    // string used for spawning
    [TextArea(3, 10)]
    public string[] spawnPatterns;
    private string[] waveSpawnPatterns;

    public enum GameState { SplashScreenDisplaying, SplashScreen, LevelScreen, GameScreen, PauseScreen, ResultScreenDisplaying, ResultScreen};
    public GameState state;

    public bool devMode;


	private void Start () {
		Time.timeScale = 1;

		// set up music clips
		youWinClip = FindObjectOfType<MusicDatabase>().youWinClip;
		youLoseClip = FindObjectOfType<MusicDatabase>().youLoseClip;
        gameStartClip = FindObjectOfType<MusicDatabase>().gameStartClip;
        wooshClip = FindObjectOfType<MusicDatabase>().wooshClip;

        // set up some variables
        audioSource = transform.Find("Audio").GetComponent<AudioSource>();
        menuAudioSource = transform.Find("MenuAudio").GetComponent<AudioSource>();
		currentHealth = maxHealth;
        currentMoney = startingMoney;
		currentWave = 0;

        // set up references to essential scripts
        enemyManager = FindObjectOfType<EnemyManager>();
        uiManager = FindObjectOfType<UIManager>();
        cameraAnimator = FindObjectOfType<CameraMover>().GetComponent<Animator>();
        scanner = FindObjectOfType<Scanner>();

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
            towerBuildPanel.SetButtonCost(i, buildableTowers[i].GetComponent<BasicTower>().cost);
        }

        
        if (devMode) {
            state = GameState.GameScreen;
            currentMoney = 9999;

            waveSpawnPatterns = spawnPatterns[0].Split('\n');
            maxWave = waveSpawnPatterns.Length;

            for (int i = 0; i < buildableGrid.transform.childCount; i++) {
                TowerPlatform oct = buildableGrid.transform.GetChild(i).GetComponent<TowerPlatform>();
                oct.gameObject.SetActive(true);
            }

            FindObjectOfType<CentralPlatform>().GetComponent<Animator>().SetTrigger("Rise");
            FindObjectOfType<CentralPlatform>().interactable = true;

            uiManager.ShowGUI(true);
            cameraAnimator.SetTrigger("SkipToGame");

            scanner.ShowScannerLine(true);
            scanner.SetRotate(true);
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
                    if (currentMoney >= buildableTowers[i].GetComponent<BasicTower>().cost) {
                        towerBuildPanel.EnableButton(i, true);
                    }
                    else {
                        towerBuildPanel.EnableButton(i, false);
                    }
                }
            }
        }
    }

    private void LateUpdate() {
        if (state == GameState.SplashScreen && Input.GetMouseButtonDown(0)) {
            state = GameState.LevelScreen;
            uiManager.StartCoroutine(uiManager.DisplaySplashScreenWithDelay(false));
            cameraAnimator.SetTrigger("SplashToLevel");
            FindObjectOfType<CentralPlatform>().GetComponent<Animator>().SetTrigger("Rise");
            FindObjectOfType<LevelSelector>().ShowLevelSelection(true);
        }
        else if (state == GameState.GameScreen) {
            // handle clicking events
            if (Input.GetMouseButtonDown(0)) {
                int buttonClicked = GetBuildPanelFromMouse();
                // if we clicked on a BuildablePanel that's enabled, build that tower
                if (buttonClicked >= 0) {
                    if (towerBuildPanel.IsButtonEnabled(buttonClicked)) {
                        BuildTower(buttonClicked);
                        towerBuildPanel.ActivatePanel(false);
                    }
                }
                // if you clicked somewhere random or on the selected Octagon, deselect the selectedPlatform
                else if (hoveredPlatform == null || hoveredPlatform == selectedPlatform) {
                    //towerBuildPanel.transform.parent = null;
                    towerBuildPanel.ActivatePanel(false);
                    // deselect any selectedOctagons
                    if (selectedPlatform) {
                        selectedPlatform.SelectPlatform(false);
                        selectedPlatform = null;
                    }

                }
                // if the clicked on hoverOctagon is not yet selected or built on, select it
                else if (hoveredPlatform && hoveredPlatform != selectedPlatform && !hoveredPlatform.IsBuiltOn()) {
                    towerBuildPanel.ActivatePanel(true);
                    towerBuildPanel.transform.SetParent(hoveredPlatform.transform, true);
                    //towerBuildPanel.transform.parent = hoveredPlatform.transform;
                    towerBuildPanel.transform.localPosition = new Vector3(0, 1.2f, 0);
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
    public void BuildTower(int input){
		if (input > buildableTowers.Count){
			print ("index out of bounds");
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
        if (input == 0)  pos = new Vector3(0, 4.4f, 0);
        else if (input == 1) pos = new Vector3(0, 3.6f, 0);
        else if (input == 2) pos = new Vector3(0, 5f, 0);
        float angle = 180f - GameManager.GetAngleFromVectorSpecial(-selectedPlatform.transform.position);
        towerObj.transform.eulerAngles = new Vector3(0, angle, 0);
        towerObj.transform.SetParent(selectedPlatform.transform, true);
        towerObj.transform.localPosition = pos;

        // link tower to selectedPlatform, change its color and deselect it
        selectedPlatform.SetBuiltTower(towerObj);
        selectedPlatform.SetColor(towerBuildPanel.towerColors[input]);
        selectedPlatform.SelectPlatform(false);
        selectedPlatform = null;

        BasicTower tower = towerObj.GetComponent<BasicTower>();
        tower.ToggleOutline(false);
        tower.MakeBuilt();
        
        // deduct money if needed
        if (!tower.refundable) {
            currentMoney -= tower.cost;
            uiManager.UpdateMoney(currentMoney);
        }

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


	// called when the homeBase takes damage. spawns a restart button when game over.
	// passes the UI stuff to the UIManager
	public void TakeDamage(int i){
		currentHealth -= i;
		uiManager.UpdateHealth(currentHealth, maxHealth);
		homeBase.GetComponent<AudioSource>().Play();
		homeBase.GetComponent<Animator>().SetTrigger("TakeDamage");

		if (currentHealth <= 0){
			// game over!
			foreach (AudioSource a in FindObjectsOfType<AudioSource>()){
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

        enemyManager.SetupWave(waveSpawnPatterns[currentWave]);

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


    public void LoadStage(int i) {
        // load the spawn info 
        waveSpawnPatterns = spawnPatterns[i].Split('\n');
        maxWave = waveSpawnPatterns.Length;

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
        FindObjectOfType<LevelSelector>().ShowLevelSelection(false);
        yield return new WaitForSeconds(wooshClip.length + 1);

        // fly the panels in and show the GUI
        float flyInDuration = 1f;
        for (int i = 0; i < buildableGrid.transform.childCount; i++) {
            TowerPlatform oct = buildableGrid.transform.GetChild(i).GetComponent<TowerPlatform>();
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

        yield return new WaitForSeconds(1);
        scanner.ShowScannerLine(true);
        scanner.SetRotate(true);
        state = GameState.GameScreen;
    }
    

	private IEnumerator WinGame(){
		// stop scanner in 1 rotation
		StartCoroutine(scanner.StopScannerRotation(1));
		while(scanner.rotating == true){
			yield return null;
		}

		// play the end game sound and zoom out
        uiManager.ShowGUI(false);
        cameraAnimator.SetTrigger("GameToResult");
        audioSource.PlayOneShot(youWinClip);

        while (audioSource.isPlaying){
			yield return null;
		}

        // show the game win screen
        state = GameState.ResultScreenDisplaying;
        StartCoroutine(uiManager.DisplayGameResultScreen(true, true, totalScore));

		Time.timeScale = 0;
	}


    private IEnumerator PlayThemeWithDelay(float delay) {
        yield return new WaitForSeconds(delay);
        menuAudioSource.Play();
        FindObjectOfType<CameraMover>().ToggleBlankScreen(false);
    }


    private IEnumerator ResetGame() {
        Time.timeScale = 1;
        state = GameState.LevelScreen;

        // make all octagons fall and lower the central tower
        FindObjectOfType<CentralPlatform>().GetComponent<Animator>().SetTrigger("Lower");
        FindObjectOfType<CentralPlatform>().interactable = true;
        foreach (TowerPlatform oct in FindObjectsOfType<TowerPlatform>()) {
            StartCoroutine(oct.FallOff());
        }
        yield return new WaitForSeconds(2);
        

        uiManager.StartCoroutine(uiManager.DisplayGameResultScreen(false));
        cameraAnimator.SetTrigger("ResultToLevel");
        FindObjectOfType<CentralPlatform>().GetComponent<Animator>().SetTrigger("Rise");
        FindObjectOfType<CentralPlatform>().interactable = false;
        FindObjectOfType<LevelSelector>().ShowLevelSelection(true);

        scanner.DestroyAllTowers();
        FindObjectOfType<EnemyPath>().ToggleAllPaths(false);
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

    public static float EaseOut(float start, float end, float t) {
        t--;
        t = t * t * t + 1;
        return start + (end - start) * t;
    }
}
