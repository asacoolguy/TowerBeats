using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Game Manager class that handles spawning towers at the right locations and tracking scores & progression
/// </summary>
public class GameManager : MonoBehaviour {
	private UIManager uiManager;
	private AudioSource endGameAudio;

	public List<GameObject> BuildableTowers;
	[SerializeField]private LayerMask layerMask;
	public float maxDistance = 35f;

	/// <summary>
	/// Enum that describes if the user is currently building a tower or not.
	/// Unselected means the player is currently
	/// </summary>
	private enum BuildState{unselected, selected};
	[SerializeField] private BuildState bState = BuildState.unselected;
	private GameObject selectedTower = null;

	// game progression stuff
	public int maxHealth = 10;
	[SerializeField]private int currentHealth;
	public float currentScore = 0;
	public float totalScore = 0;
	public int scoreToUpgrade = 900;   // this is the amount needed to advance to next level
	public List<int> scoresToAdvance;  // give it 7 upgrades for now, lets say that upgrade order is set
									   // start with 2 green, unlock 2 green, 1 blue, 2 green, 1 blue, and a yellow

	public int currentUnlockLevel = -1;
	public int currentLevel = 1;
	public int maxLevel = 4;
	public float attackPowerBonus = 1f;

	// tower build limit
	private int greenBuildAmount = 0;
	public int greenBuildLimitBase = 2;
	private int greenBuildLimitCurrent = 2;
	private int blueBuildAmount = 0;
	public int blueBuildLimitBase = 0;
	private int blueBuildLimitCurrent = 0;
	private int goldBuildAmount = 0;
	public int goldBuildLimitBase = 0;
	private int goldBuildLimitCurrent = 0;

	private AudioClip[] endGameClips;
	private AudioClip youWinClip, youLoseClip;

	// Use this for initialization
	void Start () {
		Time.timeScale = 1;

		// set up music clips
		endGameClips = FindObjectOfType<MusicDatabase>().endGameClips;
		youWinClip = FindObjectOfType<MusicDatabase>().youWinClip;
		youLoseClip = FindObjectOfType<MusicDatabase>().youLoseClip;

		uiManager = FindObjectOfType<UIManager>();
		endGameAudio = transform.Find("EndGameAudio").GetComponent<AudioSource>();
		currentHealth = maxHealth;

		// sets up the scoresToAdvance
		scoresToAdvance = new List<int>();
		scoresToAdvance.Add(30);
		scoresToAdvance.Add(70);
		scoresToAdvance.Add(120);
		scoresToAdvance.Add(190);
		scoresToAdvance.Add(300);
		scoresToAdvance.Add(450);
		scoresToAdvance.Add(600);

		// call UIManager to configure the progression bar
		uiManager.SetupProgressionBar(scoresToAdvance, scoreToUpgrade);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0) && bState == BuildState.selected && selectedTower != null){
			if (selectedTower.GetComponent<BasicTower>().IsBuildable()){
				BuildSelectedTower();
			}
			else{
				print("Invalid build position");
			}

		}
		else if (Input.GetMouseButtonDown(1) && bState == BuildState.selected){
			bState = BuildState.unselected;
			Destroy(selectedTower);
			// also turn off axes
			FindObjectOfType<Scanner>().EnableAllAxes(false);
		}

		if (selectedTower != null){
			bState = BuildState.selected;
		}

		if (bState == BuildState.selected && selectedTower != null){
			// snap selectedTower location and rotation to nearest ring
			Vector3 axisPos = SnapToAxisPosition(GetMousePositionInWorld());
			selectedTower.transform.position = new Vector3(axisPos.x, selectedTower.transform.position.y, axisPos.z);
			// also snap rotation
			float angle = 180f - GameManager.GetAngleFromVector(-selectedTower.transform.position);
			selectedTower.transform.eulerAngles = new Vector3(0, angle, 0);
		}

		// update build limits and call UIManager to toggle tower build buttons accordingly
		SetupBuildLimits();
		uiManager.SetupBuildTowerButtons(greenBuildAmount < greenBuildLimitCurrent,
										 blueBuildAmount < blueBuildLimitCurrent,
										 goldBuildAmount < goldBuildLimitCurrent);
	}

	public void SelectTowerToBuild(int i){
		// build buttons don't work when tutorial's showing
		if (FindObjectOfType<UIManager>().tutorialShowing){
			return;
		}

		if (i > BuildableTowers.Count){
			print ("index out of bounds");
			return;
		}

		//bState = BuildState.selected;
		if (selectedTower != null){
			Destroy(selectedTower);
		}

		//Vector3 startingPos = new Vector3(-20,2,20);
		selectedTower = Instantiate(BuildableTowers[i], BuildableTowers[i].transform.position, Quaternion.identity) as GameObject;
		selectedTower.SetActive(true);

		// also turn on axes
		FindObjectOfType<Scanner>().EnableAllAxes(true);
	}


	// builds the currently selectedTower 
	private void BuildSelectedTower(){
		if (selectedTower == null){
			print("Error: no tower selected");
			return;
		}

		selectedTower.GetComponent<BasicTower>().MakeBuilt();
		int axisIndex = FindObjectOfType<Scanner>().FindClosestAxisIndex(selectedTower.transform.position);
		selectedTower.GetComponent<BasicTower>().axisIndex = axisIndex;
		FindObjectOfType<Scanner>().AddTowerToList(selectedTower);

		// increment the tower count accordingly
		if (selectedTower.GetComponent<GreenTower>() != null){
			greenBuildAmount++;
		}
		else if(selectedTower.GetComponent<BlueTower>() != null){
			blueBuildAmount++;
		}
		else if (selectedTower.GetComponent<GoldTower>() != null){
			goldBuildAmount++;
		}

		bState = BuildState.unselected;
		selectedTower = null;

		// also turn off axes
		FindObjectOfType<Scanner>().EnableAllAxes(false);
	}


	// returns the mouse position in world coordinates
	private Vector3 GetMousePositionInWorld(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 100, layerMask)){
        	return new Vector3(hit.point.x, 0, hit.point.z);
        }
        else{
        	//print("Error: cannot get mouse position");
        	return Vector3.zero;
        }
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
		if (currentHealth <= 0){
			// game over!
			foreach (AudioSource a in FindObjectsOfType<AudioSource>()){
				a.Stop();
			}

			endGameAudio.clip = youLoseClip;
			endGameAudio.Play();
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
		uiManager.UpdateScore(currentScore);

		if (currentScore > scoreToUpgrade && transform.Find("WaveSphere").gameObject.activeSelf == false){
			if (currentLevel < 4){
				uiManager.ShowUpgradeButton(true);
			}
			else{
				// the game is won!
				StartCoroutine(WinGame());
			}
		}
		else if (currentUnlockLevel < 6 && currentScore >= scoresToAdvance[currentUnlockLevel + 1]){
			currentUnlockLevel++;
		}
	}


	// upgrades to the next level
	public void UpgradeLevel(){
		currentLevel++;
		currentUnlockLevel = -1;

		// send out wave
		transform.Find("UpgradeWave").GetComponent<ParticleSystem>().Emit(1);
		transform.Find("WaveSphere").gameObject.SetActive(true);

		// play the upgrade song while pausing all other songs
		transform.Find("UpgradeWave").GetComponent<AudioSource>().Play();
		FindObjectOfType<Scanner>().SetRotate(false);

		// update ui
		uiManager.UpgradeToLevel(currentLevel);

		// clear enemies and tower lists

		FindObjectOfType<EnemyManager>().ClearEnemyList();
		FindObjectOfType<Scanner>().ClearTowerList();

		StartCoroutine(SetupLevel());
	}

	// helper coroutine to reset states for new level
	private IEnumerator SetupLevel(){
		// destroy all bullets
		GameObject[] blueBullets = GameObject.FindGameObjectsWithTag("BlueBullet");
		for(int i = 0; i < blueBullets.Length; i++){
			Destroy(blueBullets[i]);
		}

		// wait for the wave to pass over
		while(transform.Find("UpgradeWave").GetComponent<AudioSource>().isPlaying){
			yield return null;
		}

		FindObjectOfType<Scanner>().ResetRotation();
		FindObjectOfType<Scanner>().numAudioPlaying = Mathf.Min(currentLevel, 4);
		FindObjectOfType<Scanner>().SetRotate(true);
		greenBuildAmount = 0;
		greenBuildLimitBase = 2;
		blueBuildAmount = 0;
		blueBuildLimitBase = 0;
		goldBuildAmount = 0;
		goldBuildLimitBase = 0;
		totalScore += currentScore;
		currentScore = 0;
		uiManager.UpdateScore(currentScore);
		uiManager.ToggleGreenTowerButton(true);
		uiManager.ToggleBlueTowerButton(false);
		uiManager.ToggleGoldTowerButton(false);


		if (currentLevel == 2){
			greenBuildLimitBase = 3;
			FindObjectOfType<EnemyManager>().enemyHealth = 3;
			FindObjectOfType<EnemyManager>().enemyDistancePerMove = 8f;
			FindObjectOfType<EnemyManager>().amountPerSpawn = 3;
			FindObjectOfType<EnemyManager>().enemyPointMultiplier = .9f;

			attackPowerBonus = 1.3f;
		}
		else if (currentLevel == 3){
			greenBuildLimitBase = 3;
			blueBuildLimitBase = 1;
			uiManager.ToggleBlueTowerButton(true);
			FindObjectOfType<EnemyManager>().enemyHealth = 4;
			FindObjectOfType<EnemyManager>().enemyDistancePerMove = 10f;
			FindObjectOfType<EnemyManager>().amountPerSpawn = 4;
			FindObjectOfType<EnemyManager>().enemyPointMultiplier = .8f;

			attackPowerBonus = 1.6f;
		}
		else if (currentLevel == 4){
			greenBuildLimitBase = 3;
			blueBuildLimitBase = 2;
			uiManager.ToggleBlueTowerButton(true);
			FindObjectOfType<EnemyManager>().enemyHealth = 5;
			FindObjectOfType<EnemyManager>().enemyDistancePerMove = 12f;
			FindObjectOfType<EnemyManager>().amountPerSpawn = 4;
			FindObjectOfType<EnemyManager>().enemyPointMultiplier = .75f;

			attackPowerBonus = 2f;
		}
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
		endGameAudio.clip = endGameClips[Random.Range(0, endGameClips.Length)];
		endGameAudio.Play();
		while(endGameAudio.isPlaying){
			yield return null;
		}
		// pop the game over box
		uiManager.DisplayGameWinScreen(totalScore);
		endGameAudio.clip = youWinClip;
		endGameAudio.Play();

		Time.timeScale = 0;
	}


	private void SetupBuildLimits(){
		switch(currentUnlockLevel){
			default:
				greenBuildLimitCurrent = greenBuildLimitBase;
				blueBuildLimitCurrent = blueBuildLimitBase;
				goldBuildLimitCurrent = goldBuildLimitBase;
				break;
			case 0:
				greenBuildLimitCurrent = greenBuildLimitBase + 1;
				blueBuildLimitCurrent = blueBuildLimitBase;
				goldBuildLimitCurrent = goldBuildLimitBase;
				break;
			case 1:
				greenBuildLimitCurrent = greenBuildLimitBase + 2;
				blueBuildLimitCurrent = blueBuildLimitBase;
				goldBuildLimitCurrent = goldBuildLimitBase;
				break;
			case 2:
				greenBuildLimitCurrent = greenBuildLimitBase + 2;
				blueBuildLimitCurrent = blueBuildLimitBase + 1;
				goldBuildLimitCurrent = goldBuildLimitBase;
				break;
			case 3:
				greenBuildLimitCurrent = greenBuildLimitBase + 3;
				blueBuildLimitCurrent = blueBuildLimitBase + 1;
				goldBuildLimitCurrent = goldBuildLimitBase;
				break;
			case 4:
				greenBuildLimitCurrent = greenBuildLimitBase + 4;
				blueBuildLimitCurrent = blueBuildLimitBase + 1;
				goldBuildLimitCurrent = goldBuildLimitBase;
				break;
			case 5:
				greenBuildLimitCurrent = greenBuildLimitBase + 4;
				blueBuildLimitCurrent = blueBuildLimitBase + 2;
				goldBuildLimitCurrent = goldBuildLimitBase;
				break;
			case 6:
				greenBuildLimitCurrent = greenBuildLimitBase + 4;
				blueBuildLimitCurrent = blueBuildLimitBase + 2;
				goldBuildLimitCurrent = goldBuildLimitBase + 1;
				break;
		}
	}
}
