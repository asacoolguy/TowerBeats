using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Game Manager class that handles spawning towers at the right locations and tracking scores & progression
/// </summary>
public class GameManagerNew : MonoBehaviour {
    private UIManagerNew uiManager;
    private CameraMover cameraMover;
    private AudioSource audioSource;

    // handle current level
    public GameObject[] levels;
    public int currentLevel;
    private Scanner currentScanner;

    public GameObject[] buildableTowers;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask towerLayerMask;

    /// <summary>
    /// Enum that describes if the user is currently building a tower or not.
    /// Unselected means the player is currently
    /// </summary>
    private enum BuildState { unselected, selected };
    private BuildState bState = BuildState.unselected;
    private GameObject selectedTower = null;
    private GameObject hoveredTower = null;
    private GameObject towerWithActiveUI = null;

    // game progression variables
    public int maxHealth = 10;
    [SerializeField] private int currentHealth;
    public float currentScore = 0;
    public float totalScore = 0;
    public int startingMoney;
    public int currentMoney;
    public int currentWave = 0;

    // Audio clips used for the game
    private AudioClip[] endGameClips;
    private AudioClip youWinClip, youLoseClip;


    private void Start() {
        Time.timeScale = 1;
        currentLevel = 0;
        currentScanner = levels[currentLevel].transform.Find("Scanner").GetComponent<Scanner>();

        // set up music clips
        endGameClips = FindObjectOfType<MusicDatabase>().endGameClips;
        youWinClip = FindObjectOfType<MusicDatabase>().youWinClip;
        youLoseClip = FindObjectOfType<MusicDatabase>().youLoseClip;

        // set up some variables
        uiManager = FindObjectOfType<UIManagerNew>();
        cameraMover = FindObjectOfType<CameraMover>();
        audioSource = transform.Find("Audio").GetComponent<AudioSource>();
        currentHealth = maxHealth;
        currentMoney = startingMoney;

        // set up buildTower buttons
        for (int i = 0; i < levels.Length; i++) {
            SetUpBuildTowerButtons(i);
        }
    }


    private void Update() {
        // update the build state if there is current a selected tower 
        if (selectedTower != null) {
            bState = BuildState.selected;
        }

        // if there is a selected tower, update its location and rotation
        if (bState == BuildState.selected && selectedTower != null) {
            // snap selectedTower location and rotation to nearest ring
            Vector3 axisPos = SnapToAxisPosition(GetMousePositionInWorld());
            //print(axisPos);
            selectedTower.transform.localPosition = new Vector3(axisPos.x, selectedTower.transform.localPosition.y, axisPos.z);
            // also snap rotation
            float angle = 180f - GameManagerNew.GetAngleFromVector(-selectedTower.transform.localPosition);
            selectedTower.transform.localEulerAngles = new Vector3(0, angle, 0);
        }

        // enable buildTower buttons accordingly
        SetUpBuildTowerButtons(currentLevel);

        // highlight any towers the mouse is hovering over
        // if we're not in build mode
        if (bState == BuildState.unselected) {
            GameObject newHoveredTower = GetTowerFromMouse();
            if (hoveredTower != newHoveredTower) {
                if (hoveredTower != null) {
                    hoveredTower.GetComponent<BasicTower>().ToggleOutline(false);
                }
                if (newHoveredTower != null) {
                    newHoveredTower.GetComponent<BasicTower>().ToggleOutline(true);
                }
                hoveredTower = newHoveredTower;
            }
        }

        // if no enemy manager is spawning, then show the next spawn button
        bool finishSpawn = true;
        foreach (GameObject levelObj in levels) {
            if (levelObj.transform.Find("EnemyManager").GetComponent<EnemyManagerNew>().IsSpawning()) {
                finishSpawn = false;
            }
        }
        uiManager.ShowSpawnButton(finishSpawn);
    }

    private void LateUpdate() {
        // handling changing levels using scrolling
        if (Input.GetAxis("Mouse ScrollWheel") != 0 && !cameraMover.IsMoving()) {
            int sign = Input.GetAxis("Mouse ScrollWheel") > 0 ? 1 : -1;
            int targetLevel = currentLevel + 1 * sign;
            // TODO: handle camera scrolling past multiple levels instead of one at a time
            if (targetLevel < levels.Length && targetLevel >= 0) {
                StartCoroutine(cameraMover.MoveToLevel(targetLevel));
                currentLevel = targetLevel;
                currentScanner = levels[currentLevel].transform.Find("Scanner").GetComponent<Scanner>();
            }
        }
        // handle deselecting/building/moving towers
        else if (Input.GetMouseButtonDown(0)) {
            // if you have a tower selected, build the tower
            if (GetMousePositionInWorld() != Vector3.zero && bState == BuildState.selected && selectedTower != null) {
                if (selectedTower.GetComponent<BasicTower>().IsBuildable()) {
                    BuildSelectedTower();
                }
                else {
                    print("Invalid build position");
                }
            }
            // if you're click on a hovering over a tower, enable its build panel
            else if (GetMousePositionInWorld() != Vector3.zero && bState == BuildState.unselected && hoveredTower != null) {
                // if there's already an active tower UI
                if (towerWithActiveUI != null) {
                    // if active tower is the same tower, just close its UI
                    if (towerWithActiveUI == hoveredTower) {
                        towerWithActiveUI.GetComponent<BasicTower>().ToggleUIPanel(false);
                        towerWithActiveUI = null;
                    }
                    // active tower is different tower, close its UI and open the new one's
                    else {
                        towerWithActiveUI.GetComponent<BasicTower>().ToggleUIPanel(false);
                        hoveredTower.GetComponent<BasicTower>().ToggleUIPanel(true);
                        towerWithActiveUI = hoveredTower;
                    }
                }
                // if there is no active UI yet
                else {
                    hoveredTower.GetComponent<BasicTower>().ToggleUIPanel(true);
                    towerWithActiveUI = hoveredTower;
                }
            }
            // if you click somewhere random, close the current build panel if it exists
            else if (towerWithActiveUI != null) {
                //towerWithActiveUI.GetComponent<BasicTower>().ToggleUIPanel(false);
                //towerWithActiveUI = null;
            }
        }
        else if (Input.GetMouseButtonDown(1) && bState == BuildState.selected) {
            bState = BuildState.unselected;
            // refund half the cost if we already paid for this 
            if (selectedTower.GetComponent<BasicTower>().refundable) {
                currentMoney += selectedTower.GetComponent<BasicTower>().cost / 2;
                uiManager.UpdateMoney(currentMoney);
            }
            Destroy(selectedTower);
            // also turn off axes
            currentScanner.EnableAllAxes(false);
        }
    }


    // select the right tower to build using index
    public void SelectTowerToBuild(int i) {
        // build buttons don't work when tutorial's showing
        if (i > buildableTowers.Length) {
            print("index out of bounds");
            return;
        }

        //bState = BuildState.selected;
        if (selectedTower != null) {
            Destroy(selectedTower);
        }

        //Vector3 startingPos = new Vector3(-20,2,20);
        selectedTower = Instantiate(buildableTowers[i], 
            buildableTowers[i].transform.localPosition + levels[currentLevel].transform.localPosition,
            Quaternion.identity) as GameObject;
        selectedTower.SetActive(true);
        selectedTower.GetComponent<BasicTower>().ToggleOutline(false);

        // also turn on axes
        currentScanner.EnableAllAxes(true);
    }


    // builds the currently selectedTower 
    private void BuildSelectedTower() {
        if (selectedTower == null) {
            print("Error: no tower selected");
            return;
        }


        BasicTower tower = selectedTower.GetComponent<BasicTower>();

        // deduct money if needed
        if (!tower.refundable) {
            currentMoney -= selectedTower.GetComponent<BasicTower>().cost;
            uiManager.UpdateMoney(currentMoney);
        }

        tower.MakeBuilt();
        tower.transform.parent = levels[currentLevel].transform;
        tower.ToggleOutline(false);
        int axisIndex = currentScanner.FindClosestAxisIndex(selectedTower.transform.localPosition);
        tower.axisIndex = axisIndex;
        currentScanner.AddTowerToList(selectedTower);


        bState = BuildState.unselected;
        selectedTower = null;

        // also turn off axes
        currentScanner.EnableAllAxes(false);
    }


    // go into planning mode with the input tower
    // this function is called by towers when they go into moving mode
    public void MoveSelectedTower(GameObject tower) {
        selectedTower = tower;
        hoveredTower = null;
        towerWithActiveUI = null;
        bState = BuildState.selected;
        currentScanner.RemoveTowerFromList(selectedTower);
        // also turn on axes
        currentScanner.EnableAllAxes(true);
    }


    public void ClearTowerUI() {
        towerWithActiveUI = null;
    }


    // returns the mouse position in world coordinates if mouse is within the ground plane
    private Vector3 GetMousePositionInWorld() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, groundLayerMask)) {
            return new Vector3(hit.point.x, 0, hit.point.z);
        }
        else {
            //print("Error: cannot get mouse position");
            return Vector3.zero;
        }
    }


    // returns the tower that the mouse is currently hovering over
    // only valid for built towers
    private GameObject GetTowerFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, towerLayerMask)) {
            // trace parents until we find the object with BasicTower script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            while (current.GetComponent<BasicTower>() == null &&
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }

            if (hit.collider.gameObject.GetComponent<BasicTower>() != null
                && hit.collider.gameObject.GetComponent<BasicTower>().IsBuilt()) {
                return hit.collider.gameObject;
            }
        }

        // otherwise no tower hit
        return null;
    }


    // given a position, return the point nearest ring to that position
    private Vector3 SnapToAxisPosition(Vector3 pos) {
        Vector3 newPos = currentScanner.FindPointOnAxis(pos);
        return new Vector3(newPos.x, 0, newPos.z);
    }


    // called when the homeBase takes damage. summons a restart button when game over.
    // passes the UI stuff to the UIManagerNew
    public void TakeDamage(int i) {
        currentHealth -= i;
        uiManager.UpdateHealth(currentHealth);
        // TODO: make new tower react to taking damage
        //homeBase.GetComponent<AudioSource>().Play();
        //homeBase.GetComponent<Animator>().SetTrigger("TakeDamage");

        if (currentHealth <= 0) {
            // game over!
            foreach (AudioSource a in FindObjectsOfType<AudioSource>()) {
                a.Stop();
            }

            audioSource.clip = youLoseClip;
            audioSource.Play();
            uiManager.DisplayGameOverScreen();
            Time.timeScale = 0;
        }
    }

    public static float GetAngleFromVector(Vector3 pos) {
        float angle = 0f;

        if (pos.x == 0) {
            if (pos.z == 0) {
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


    public void GetPoints(float pts) {
        currentScore += pts;
    }


    public void GetMoney(int money) {
        currentMoney += money;
        uiManager.UpdateMoney(currentMoney);
    }


    // sets up the buildTower buttons for a certain level
    private void SetUpBuildTowerButtons(int level) {
        LevelUI levelUI = levels[level].transform.Find("HUD").GetComponent<LevelUI>();
        levelUI.SetupBuildTowerButtons(currentMoney >= buildableTowers[0].GetComponent<BasicTower>().cost,
                                         currentMoney >= buildableTowers[1].GetComponent<BasicTower>().cost,
                                         currentMoney >= buildableTowers[2].GetComponent<BasicTower>().cost);
    }


    // starts the music playing on all levels
    public void StartMusic() {
        foreach(GameObject levelObj in levels) {
            levelObj.transform.Find("Scanner").GetComponent<Scanner>().SetRotate(true);
        }
    }


    // starts spawning enemies on all levels
    public void SpawnWave() {
        currentWave++;
        uiManager.ShowSpawnButton(false);

        foreach (GameObject levelObj in levels) {
            levelObj.transform.Find("EnemyManager").GetComponent<EnemyManagerNew>().SetupWave(currentWave);
        }

        uiManager.UpdateWaveText(currentWave);
    }


    // upgrades to the next level
    public void UpgradeLevel() {
        //currentLevel++;

        // send out wave
        transform.Find("UpgradeWave").GetComponent<ParticleSystem>().Emit(1);
        transform.Find("WaveSphere").gameObject.SetActive(true);

        // play the upgrade song while pausing all other songs
        transform.Find("UpgradeWave").GetComponent<AudioSource>().Play();
        currentScanner.SetRotate(false);

        // update ui
        //uiManager.UpgradeToLevel(currentLevel);

        // clear enemies and tower lists

        FindObjectOfType<EnemyManager>().ClearEnemyList();
        currentScanner.ClearTowerList();

        StartCoroutine(SetupLevel());
    }

    // helper coroutine to reset states for new level
    private IEnumerator SetupLevel() {
        // destroy all bullets
        GameObject[] SniperBullets = GameObject.FindGameObjectsWithTag("SniperBullet");
        for (int i = 0; i < SniperBullets.Length; i++) {
            Destroy(SniperBullets[i]);
        }

        // wait for the wave to pass over
        while (transform.Find("UpgradeWave").GetComponent<AudioSource>().isPlaying) {
            yield return null;
        }

        currentScanner.ResetRotation();
        currentScanner.numAudioPlaying = Mathf.Min(currentLevel, 4);
        currentScanner.SetRotate(true);
        currentScanner.spawnEnemies = true;
        totalScore += currentScore;
        currentScore = 0;
    }

    private IEnumerator WinGame() {
        // destroy all enemies
        FindObjectOfType<EnemyManager>().DestroyAllEnemies();
        currentScanner.spawnEnemies = false;
        // stop scanner in 4 measures
        StartCoroutine(currentScanner.StopScannerRotation(2));
        while (currentScanner.finishedUp == false) {
            yield return null;
        }
        // play the end game sound
        audioSource.clip = endGameClips[Random.Range(0, endGameClips.Length)];
        audioSource.Play();
        while (audioSource.isPlaying) {
            yield return null;
        }
        // pop the game over box
        uiManager.DisplayGameWinScreen(totalScore);
        audioSource.clip = youWinClip;
        audioSource.Play();

        Time.timeScale = 0;
    }

}
