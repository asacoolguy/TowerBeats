using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Game Manager class that handles spawning towers at the right locations and tracking scores & progression
/// </summary>
public class GameManager : MonoBehaviour {
	private UIManager uiManager;
	private AudioSource audioSource;

	public List<GameObject> buildableTowers;
	public GameObject homeBase;
    [SerializeField] private LayerMask octLayerMask, towerLayerMask;

    /// <summary>
    /// Enum that describes if the user is currently building a tower or not.
    /// Unselected means the player is currently
    /// </summary>
    private enum BuildState{unselected, selected};
	[SerializeField] private BuildState bState = BuildState.unselected;
	private GameObject selectedTower = null;
    [SerializeField] private GameObject hoveredGrid = null;

	// game progression variables
	public int maxHealth = 10;
	public int currentHealth;
	public float currentScore = 0;
	public float totalScore = 0;
    public int startingMoney;
    public int currentMoney;
	public int currentWave = 1;
    public int maxWave = 5;

    // Audio clips used for the game
	private AudioClip[] endGameClips;
	private AudioClip youWinClip, youLoseClip;


	private void Start () {
		Time.timeScale = 1;

		// set up music clips
		endGameClips = FindObjectOfType<MusicDatabase>().endGameClips;
		youWinClip = FindObjectOfType<MusicDatabase>().youWinClip;
		youLoseClip = FindObjectOfType<MusicDatabase>().youLoseClip;

        // set up some variables
		uiManager = FindObjectOfType<UIManager>();
		audioSource = transform.Find("Audio").GetComponent<AudioSource>();
		currentHealth = maxHealth;
        currentMoney = startingMoney;
	}


    private void Update() {
        /*
        // update the build state if there is current a selected tower 
        if (selectedTower != null)
        {
            bState = BuildState.selected;
        }

        // if there is a selected tower, update its location and rotation
        if (bState == BuildState.selected && selectedTower != null)
        {
            // snap selectedTower location and rotation to nearest ring
            Vector3 axisPos = SnapToAxisPosition(GetMousePositionInWorld());
            selectedTower.transform.position = new Vector3(axisPos.x, selectedTower.transform.position.y, axisPos.z);
            // also snap rotation
            float angle = 180f - GameManager.GetAngleFromVector(-selectedTower.transform.position);
            selectedTower.transform.eulerAngles = new Vector3(0, angle, 0);
        }
                                         
        */

        // highlight any BuildableOctagons the mouse is hovering over
        GameObject newHoveredGrid = GetOctagonFromMouse();
        if (hoveredGrid != newHoveredGrid) {
            if (hoveredGrid != null) {
                hoveredGrid.GetComponent<BuildableOctagon>().LowerTower();
            }
            if (newHoveredGrid != null) {
                newHoveredGrid.GetComponent<BuildableOctagon>().RaiseTower();
            }
            hoveredGrid = newHoveredGrid;
        }
    }

    private void LateUpdate() {
        // handle deselecting/building/moving towers
        if (Input.GetMouseButtonDown(0)){
            if (GetMousePositionInWorld() != Vector3.zero && bState == BuildState.selected && selectedTower != null) {
                if (selectedTower.GetComponent<BasicTower>().IsBuildable()) {
                    BuildSelectedTower();
                }
                else {
                    print("Invalid build position");
                }
            }
            else if(GetMousePositionInWorld() != Vector3.zero && bState == BuildState.unselected && hoveredGrid != null) {
                selectedTower = hoveredGrid;
                hoveredGrid = null;
                bState = BuildState.selected;
                selectedTower.GetComponent<BasicTower>().MakeMoving();
                selectedTower.GetComponent<BasicTower>().ToggleOutline(false);
                FindObjectOfType<Scanner>().RemoveTowerFromList(selectedTower);
                // also turn on axes
                FindObjectOfType<Scanner>().EnableAllAxes(true);
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
            FindObjectOfType<Scanner>().EnableAllAxes(false);
        }
    }


    // select the right tower to build using index
    public void SelectTowerToBuild(int i){
		// build buttons don't work when tutorial's showing
		if (uiManager.tutorialShowing){
			return;
		}

		if (i > buildableTowers.Count){
			print ("index out of bounds");
			return;
		}

		//bState = BuildState.selected;
		if (selectedTower != null){
			Destroy(selectedTower);
		}

		//Vector3 startingPos = new Vector3(-20,2,20);
		selectedTower = Instantiate(buildableTowers[i], buildableTowers[i].transform.position, Quaternion.identity) as GameObject;
		selectedTower.SetActive(true);
        selectedTower.GetComponent<BasicTower>().ToggleOutline(false);

		// also turn on axes
		FindObjectOfType<Scanner>().EnableAllAxes(true);
	}


	// builds the currently selectedTower 
	private void BuildSelectedTower(){
		if (selectedTower == null){
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
        tower.ToggleOutline(false);
        int axisIndex = FindObjectOfType<Scanner>().FindClosestAxisIndex(selectedTower.transform.position);
		tower.axisIndex = axisIndex;
		FindObjectOfType<Scanner>().AddTowerToList(selectedTower);
        

        bState = BuildState.unselected;
		selectedTower = null;

		// also turn off axes
		FindObjectOfType<Scanner>().EnableAllAxes(false);
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


    // returns the tower that the mouse is currently hovering over
    // only valid for built towers
    private GameObject GetTowerFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, towerLayerMask)) {
            // trace parents until we find the object with BasicTower script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            print("initial hit is " + current.name);
            while (current.GetComponent<BasicTower>() == null && 
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            print("final hit is " + current.name);
            if (hit.collider.gameObject.GetComponent<BasicTower>() != null
                && hit.collider.gameObject.GetComponent<BasicTower>().IsBuilt()) {
                return hit.collider.gameObject;
            }
        }

        // otherwise no tower hit
        return null;
    }


    // returns the BuildableOctagon that the mouse is currently hovering over
    // only valid for built towers
    private GameObject GetOctagonFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, octLayerMask)) {
            // trace parents until we find the object with BuildableOctagon script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            print("initial hit is " + current.name);
            while (current.GetComponent<BuildableOctagon>() == null &&
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            print("final hit is " + current.name);
            if (current.GetComponent<BuildableOctagon>() != null) {
                print("script returned");
                return current;
            }
            print("no script");
        }

        print("no hit ");
        // otherwise no tower hit
        return null;
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
		uiManager.UpdateHealth(currentHealth);
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


	public void GetPoints(float pts){
		currentScore += pts;
	}


    public void GetMoney(int money){
        currentMoney += money;
        uiManager.UpdateMoney(currentMoney);
    }

	private IEnumerator WinGame(){
		// destroy all enemies
		FindObjectOfType<EnemyManager>().DestroyAllEnemies();
		FindObjectOfType<Scanner>().spawnEnemies = false;
		// stop scanner in 4 measures
		StartCoroutine(FindObjectOfType<Scanner>().StopScannerRotation(2));
		while(FindObjectOfType<Scanner>().finishedUp == false){
			yield return null;
		}
		// play the end game sound
		audioSource.clip = endGameClips[Random.Range(0, endGameClips.Length)];
		audioSource.Play();
		while(audioSource.isPlaying){
			yield return null;
		}
		// pop the game over box
		uiManager.DisplayGameWinScreen(totalScore);
		audioSource.clip = youWinClip;
		audioSource.Play();

		Time.timeScale = 0;
	}


    public static Vector3 SmoothStep(Vector3 start, Vector3 end, float t) {
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        return start + (end - start) * t;
    }

    public static float SmoothStep(float start, float end, float t) {
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        return start + (end - start) * t;
    }
}
