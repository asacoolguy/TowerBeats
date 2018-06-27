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

	public List<GameObject> buildableTowers;
	public GameObject homeBase;
    public LayerMask selectableLayerMask;
    
    private BuildableOctagon hoveredOctagon = null;
    private BuildableOctagon selectedOctagon = null;
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
	private AudioClip youWinClip, youLoseClip, displayBoxClip, gameStartClip, wooshClip;

    // string used for spawning
    [TextArea(3, 10)]
    public string[] spawnPatterns;
    private string[] waveSpawnPatterns;

    public bool devMode;


	private void Start () {
		Time.timeScale = 1;

		// set up music clips
		youWinClip = FindObjectOfType<MusicDatabase>().youWinClip;
		youLoseClip = FindObjectOfType<MusicDatabase>().youLoseClip;
        displayBoxClip = FindObjectOfType<MusicDatabase>().displayBoxClip;
        gameStartClip = FindObjectOfType<MusicDatabase>().gameStartClip;
        wooshClip = FindObjectOfType<MusicDatabase>().wooshClip;

        // set up some variables
        audioSource = transform.Find("Audio").GetComponent<AudioSource>();
        menuAudioSource = transform.Find("MenuAudio").GetComponent<AudioSource>();
		currentHealth = maxHealth;
        currentMoney = startingMoney;
		currentWave = 0;

        // set up enemy manager and parse the spawn pattern
        enemyManager = FindObjectOfType<EnemyManager>();
        

        // initialize the UI with some values
        uiManager = FindObjectOfType<UIManager>();

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
            towerBuildPanel.GetComponent<BuildPanel>().AOEIndicators.Add(AOEIndicator);
            // set up the right cost
            towerBuildPanel.GetComponent<BuildPanel>().SetButtonCost(i, buildableTowers[i].GetComponent<BasicTower>().cost);
        }

        if (devMode) {
            waveSpawnPatterns = spawnPatterns[0].Split('\n');
            maxWave = waveSpawnPatterns.Length;

            for (int i = 0; i < buildableGrid.transform.childCount; i++) {
                BuildableOctagon oct = buildableGrid.transform.GetChild(i).GetComponent<BuildableOctagon>();
                oct.gameObject.SetActive(true);
            }

            FindObjectOfType<CentralOctagon>().GetComponent<Animator>().SetTrigger("Rise");
            FindObjectOfType<CentralOctagon>().interactable = true;

            uiManager.ShowGUI(true);
            FindObjectOfType<CameraMover>().MoveToGame(0.1f);
            FindObjectOfType<CameraMover>().ToggleBlankScreen(false);

            FindObjectOfType<Scanner>().ShowScannerLine(true);
            FindObjectOfType<Scanner>().SetRotate(true);
        }
        else {
            // initiate the camera with its splash screen
            uiManager.DisplaySplashScreen(true, 2.2f);
            StartCoroutine(PlayThemeWithDelay(2f));
        }
    }


    private void Update() {
        if (gameOver) return;

        if (currentHealth <= 0) {
            gameOver = true;
            Time.timeScale = 0;
            uiManager.DisplayGameOverScreen();
            audioSource.PlayOneShot(displayBoxClip);
        }

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
        
        // highlight any BuildableOctagons the mouse is hovering over that's not already built on
        BuildableOctagon newHoveredOctagon = GetOctagonFromMouse();
        if (hoveredOctagon != newHoveredOctagon) {
            if (hoveredOctagon != null) {
                hoveredOctagon.LowerOctagon();
            }
            if (newHoveredOctagon != null) {
                newHoveredOctagon.RaiseOctagon();
            }
            hoveredOctagon = newHoveredOctagon;
        }

		
		if (towerBuildPanel.gameObject.activeSelf){
            // highlight any BuildPanelButtons the mouse is hovering over
            towerBuildPanel.HighlightButton(GetBuildPanelFromMouse());

            // enable/disable BuildPanelButtons based on money
            for (int i = 0; i < buildableTowers.Count; i++) {
                if (currentMoney >= buildableTowers[i].GetComponent<BasicTower>().cost) {
                    towerBuildPanel.EnableButton(i, true);
                } else {
                    towerBuildPanel.EnableButton(i, false);
                }
            }
        }

    }

    private void LateUpdate() {
        if (gameOver) return;

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
            // if you clicked somewhere random or on the selected Octagon, deselect the selectedOctagon
            else if (hoveredOctagon == null || hoveredOctagon == selectedOctagon) {
                //towerBuildPanel.transform.parent = null;
                towerBuildPanel.ActivatePanel(false);
                // deselect any selectedOctagons
                if (selectedOctagon) {
                    selectedOctagon.SelectOctagon(false);
                    selectedOctagon = null;
                }
                
            }
            // if the clicked on hoverOctagon is not yet selected or built on, select it
            else if (hoveredOctagon && hoveredOctagon != selectedOctagon && !hoveredOctagon.IsBuiltOn()) {
				towerBuildPanel.ActivatePanel(true);
                towerBuildPanel.transform.SetParent(hoveredOctagon.transform, true);
                //towerBuildPanel.transform.parent = hoveredOctagon.transform;
                towerBuildPanel.transform.localPosition = new Vector3(0, 1.2f, 0);
                // set the new selectedOctagon
                if (selectedOctagon) {
                    selectedOctagon.SelectOctagon(false);
                }
                selectedOctagon = hoveredOctagon;
                selectedOctagon.SelectOctagon(true);
            }
        }

        if (devMode) {
            if (Input.GetKeyDown(KeyCode.Space)) {
                StartCoroutine(WinGame());
            }
        }
    }


    // select the right tower to build using index
    public void BuildTower(int input){
		if (input > buildableTowers.Count){
			print ("index out of bounds");
			return;
		}

        if (selectedOctagon == null) {
            print("no octagon selected");
            return;
        }

        // build the tower
        GameObject towerObj = Instantiate(buildableTowers[input]) as GameObject;
		towerObj.SetActive(true);

        // parent it to selectedOctagon and set its position and rotation accordingly
        // TODO: need better way of handling tower positioning
        Vector3 pos = Vector3.zero;
        if (input == 0)  pos = new Vector3(0, 4.4f, 0);
        else if (input == 1) pos = new Vector3(0, 3.6f, 0);
        else if (input == 2) pos = new Vector3(0, 5f, 0);
        float angle = 180f - GameManager.GetAngleFromVectorSpecial(-selectedOctagon.transform.position);
        towerObj.transform.eulerAngles = new Vector3(0, angle, 0);
        towerObj.transform.SetParent(selectedOctagon.transform, true);
        towerObj.transform.localPosition = pos;

        // link tower to selectedOctagon, change its color and deselect it
        selectedOctagon.SetBuiltTower(towerObj);
        selectedOctagon.SetColor(towerBuildPanel.towerColors[input]);
        selectedOctagon.SelectOctagon(false);
        selectedOctagon = null;

        BasicTower tower = towerObj.GetComponent<BasicTower>();
        tower.ToggleOutline(false);
        tower.MakeBuilt();
        
        // deduct money if needed
        if (!tower.refundable) {
            currentMoney -= tower.cost;
            uiManager.UpdateMoney(currentMoney);
        }

        // do some stuff about finding the right list to add tower to
        int axisIndex = FindObjectOfType<Scanner>().FindClosestAxisIndex(tower.transform.position);
        tower.axisIndex = axisIndex;
        FindObjectOfType<Scanner>().AddTowerToList(towerObj);
    }


	// returns the mouse position in world coordinates if mouse is within the ground plane
	private Vector3 GetMousePositionInWorld(){
        /*
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100, groundLayerMask)){
        	return new Vector3(hit.point.x, 0, hit.point.z);
        }
        else{
        	//print("Error: cannot get mouse position");
        	return Vector3.zero;
        }*/
        return Vector3.zero;
	}


    // returns the BuildableOctagon that the mouse is currently hovering over
    // only valid for built towers
    private BuildableOctagon GetOctagonFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 1000, selectableLayerMask)) {
            // trace parents until we find the object with BuildableOctagon script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            //print("initial hit is " + current.name);
            while (current.GetComponent<BuildableOctagon>() == null &&
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
            if (current.GetComponent<BuildableOctagon>() != null) {
                //print("script returned");
                return current.GetComponent<BuildableOctagon>();
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


    // given a position, return the point nearest ring to that position
    private Vector3 SnapToAxisPosition(Vector3 pos){
		Vector3 newPos = FindObjectOfType<Scanner>().FindPointOnAxis(pos);
		return new Vector3(newPos.x, 0, newPos.z);
	}


	// called when the homeBase takes damage. summons a restart button when game over.
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
			uiManager.DisplayGameOverScreen();
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
        FindObjectOfType<CameraMover>().MoveToGame(wooshClip.length);
        FindObjectOfType<LevelSelector>().ShowLevelSelection(false);
        yield return new WaitForSeconds(wooshClip.length + 1);

        // fly the panels in and show the GUI
        float flyInDuration = 1f;
        for (int i = 0; i < buildableGrid.transform.childCount; i++) {
            BuildableOctagon oct = buildableGrid.transform.GetChild(i).GetComponent<BuildableOctagon>();
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
        FindObjectOfType<Scanner>().ShowScannerLine(true);
        FindObjectOfType<Scanner>().SetRotate(true);
    }
    

	private IEnumerator WinGame(){
		// stop scanner in 1 rotation
		StartCoroutine(FindObjectOfType<Scanner>().StopScannerRotation(1));
		while(FindObjectOfType<Scanner>().rotating == true){
			yield return null;
		}

		// play the end game sound and zoom out
        uiManager.ShowGUI(false);
        FindObjectOfType<CameraMover>().MoveToGameWin(youWinClip.length);
        audioSource.PlayOneShot(youWinClip);

        while (audioSource.isPlaying){
			yield return null;
		}

		// show the game win screen
		uiManager.DisplayGameWinScreen(true);
        audioSource.PlayOneShot(displayBoxClip);

		Time.timeScale = 0;
	}


    private IEnumerator PlayThemeWithDelay(float delay) {
        yield return new WaitForSeconds(delay);
        menuAudioSource.Play();
        FindObjectOfType<CameraMover>().ToggleBlankScreen(false);
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
