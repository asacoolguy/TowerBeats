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
    [SerializeField] private LayerMask octLayerMask, buildPanelLayerMask, towerLayerMask;

    /// <summary>
    /// Enum that describes if the user is currently building a tower or not.
    /// Unselected means the player is currently
    /// </summary>
    private enum BuildState{unselected, selected};
	[SerializeField] private BuildState bState = BuildState.unselected;
	private GameObject selectedTower = null;
    private GameObject hoveredOctagon = null;
    private GameObject selectedOctagon = null;
    public GameObject towerBuildPanelPrefab;
    private GameObject towerBuildPanel = null;

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

        // make the tower build panel
        towerBuildPanel = Instantiate(towerBuildPanelPrefab);
        towerBuildPanel.SetActive(false);
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

        // highlight any BuildableOctagons the mouse is hovering over that's not already built on
        GameObject newHoveredOctagon = GetOctagonFromMouse();
        if (hoveredOctagon != newHoveredOctagon) {
            if (hoveredOctagon != null) {
                hoveredOctagon.GetComponent<BuildableOctagon>().LowerOctagon();
            }
            if (newHoveredOctagon != null && !newHoveredOctagon.GetComponent<BuildableOctagon>().IsBuiltOn()) {
                newHoveredOctagon.GetComponent<BuildableOctagon>().RaiseOctagon();
            }
            hoveredOctagon = newHoveredOctagon;
        }
    }

    private void LateUpdate() {
        // handle deselecting/building/moving towers
        /*
        if (Input.GetMouseButtonDown(0)){
            if (GetMousePositionInWorld() != Vector3.zero && bState == BuildState.selected && selectedTower != null) {
                if (selectedTower.GetComponent<BasicTower>().IsBuildable()) {
                    BuildSelectedTower();
                }
                else {
                    print("Invalid build position");
                }
            }
            else if(GetMousePositionInWorld() != Vector3.zero && bState == BuildState.unselected && hoveredOctagon != null) {
                selectedTower = hoveredOctagon;
                hoveredOctagon = null;
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
        }*/

        // handle clicking events
        if (Input.GetMouseButtonDown(0)) {
            // if we clicked on a BuildablePanel
            int panelClicked = GetBuildPanelFromMouse();

            if (panelClicked >= 0) {
                BuildTower(panelClicked);
                towerBuildPanel.transform.SetParent(null, true);
                towerBuildPanel.SetActive(false);
            }
            // if you clicked somewhere random or on the selected Octagon, deselect the selectedOctagon
            else if (hoveredOctagon == null || hoveredOctagon == selectedOctagon) {
                //towerBuildPanel.transform.parent = null;
                towerBuildPanel.transform.SetParent(null, true);
                towerBuildPanel.SetActive(false);
                // deselect any selectedOctagons
                if (selectedOctagon) {
                    selectedOctagon.GetComponent<BuildableOctagon>().SelectOctagon(false);
                    selectedOctagon = null;
                }
                
            }
            // if the clicked on hoverOctagon is not yet selected, select it
            else if (hoveredOctagon && hoveredOctagon != selectedOctagon) {
                towerBuildPanel.SetActive(true);
                towerBuildPanel.transform.SetParent(hoveredOctagon.transform, true);
                //towerBuildPanel.transform.parent = hoveredOctagon.transform;
                towerBuildPanel.transform.localPosition = new Vector3(0, 1.2f, 0);
                // set the new selectedOctagon
                if (selectedOctagon) {
                    selectedOctagon.GetComponent<BuildableOctagon>().SelectOctagon(false);
                }
                selectedOctagon = hoveredOctagon;
                selectedOctagon.GetComponent<BuildableOctagon>().SelectOctagon(true);
            }
        }
    }


    // select the right tower to build using index
    public void BuildTower(int i){
		if (i > buildableTowers.Count){
			print ("index out of bounds");
			return;
		}

        if (selectedOctagon == null) {
            print("no octagon selected");
            return;
        }

        //print("building tower " + i + " in gamemanager");

        // build the tower
        GameObject towerObj = Instantiate(buildableTowers[i]) as GameObject;
		towerObj.SetActive(true);

        // parent it to selectedOctagon and set its position and rotation accordingly
        // TODO: need better way of handling tower positioning
        Vector3 pos = Vector3.zero;
        if (i == 0)  pos = new Vector3(0, 1.14f, 0);
        else if (i == 1) pos = new Vector3(0, 1.11f, 0);
        else if (i == 2) pos = new Vector3(0, 1.176f, 0);
        float angle = 180f - GameManager.GetAngleFromVectorSpecial(-selectedOctagon.transform.position);
        towerObj.transform.eulerAngles = new Vector3(0, angle, 0);
        towerObj.transform.SetParent(selectedOctagon.transform, true);
        towerObj.transform.localPosition = pos;

        // change selectedOctagon to built 
        selectedOctagon.GetComponent<BuildableOctagon>().BuiltOnOctagon();

        BasicTower tower = towerObj.GetComponent<BasicTower>();
        tower.ToggleOutline(false);
        tower.MakeBuilt();
        
        // deduct money if needed
        if (!tower.refundable) {
            currentMoney -= tower.cost;
            //uiManager.UpdateMoney(currentMoney);
        }

        // do some stuff about finding the right list to add tower to
        int axisIndex = FindObjectOfType<Scanner>().FindClosestAxisIndex(tower.transform.position);
        tower.axisIndex = axisIndex;
        print("index is " + axisIndex);
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


    // returns the tower that the mouse is currently hovering over
    // only valid for built towers
    private GameObject GetTowerFromMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, towerLayerMask)) {
            // trace parents until we find the object with BasicTower script on it
            // in case ray tracing hit a child component of a tower
            GameObject current = hit.collider.gameObject;
            //print("initial hit is " + current.name);
            while (current.GetComponent<BasicTower>() == null && 
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
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
            //print("initial hit is " + current.name);
            while (current.GetComponent<BuildableOctagon>() == null &&
                current.transform.parent != null) {
                current = current.transform.parent.gameObject;
            }
            //print("final hit is " + current.name);
            if (current.GetComponent<BuildableOctagon>() != null) {
                //print("script returned");
                return current;
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
        if (Physics.Raycast(ray, out hit, 1000, buildPanelLayerMask)) {
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
