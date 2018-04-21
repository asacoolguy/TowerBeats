using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the scanner that rotates 360 degrees every 2 measures. 
/// Make sure to use an audioclip of exactly 2 measures or tempo will be off. 
///
/// The scanner starts off not rotating, facing 12 o'clock. Use setRotate to start 
/// or pause rotations. Axes will be constructed upon object initiation to indicate
/// where towers can be built. 
/// </summary>

public class Scanner : MonoBehaviour {
	// variables for tracking rotations
	private float secondsPerMeasure;
	public int measurePerRotation = 1;
	private float rotationSpeed;
	public bool rotating = false;
	public bool spawnEnemies;
	public bool finishedUp = false;

	// variables for spawning axes
	public float minAxisLength = 10f;
	public float maxAxisLength = 50f;
	public int axisNumber = 8;
	private float anglePerAxis;
	[SerializeField]private List<GameObject> axes;
	// organized by axes. towerLists[1] returns all towers on axes 1.
	[SerializeField]private List<GameObject>[] towerLists; 
	public GameObject axisOBj;

	// variables for tracking current rotation amount
	private int nextAxisToPlay;
	private float totalRotateAmount;
	private float enemySpawnCounter, enemyMoveCounter;

	// variables for kepeing track of audio
	private AudioSource[] audios;
	private AudioClip[] audioClips;
	public int numAudioPlaying = 1;


	// Use this for initialization
	void Start () {
		// set up audio clips
		audioClips = FindObjectOfType<MusicDatabase>().scannerClips;

		// set up the audio sources
		audios = new AudioSource[4];
		audios[0] = transform.Find("audio0").GetComponent<AudioSource>();
		audios[0].clip = audioClips[Random.Range(0, 2)];
		audios[1] = transform.Find("audio1").GetComponent<AudioSource>();
		audios[1].clip = audioClips[Random.Range(2, 4)];
		audios[2] = transform.Find("audio2").GetComponent<AudioSource>();
		audios[2].clip = audioClips[Random.Range(4, 6)];
		audios[3] = transform.Find("audio3").GetComponent<AudioSource>();
		audios[3].clip = audioClips[Random.Range(6, 8)];


		secondsPerMeasure = audios[0].clip.length / measurePerRotation;
		anglePerAxis = 360f / axisNumber;
		rotationSpeed = 360f / (measurePerRotation * secondsPerMeasure);

		// initiate the axes and disble them
		SetupAxis();
		EnableAllAxes(false);

		// initialize towerLists
		towerLists = new List<GameObject>[axisNumber];
		for (int i = 0; i < axisNumber; i++){
			towerLists[i] = new List<GameObject>();
		}

		// initialize variables for rotation counting
		ResetRotation();

	    // start rotation
	    //SetRotate(true);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            SetRotate(!rotating);
        }

		if (rotating){
			// rotate the scanner (and its underlying line)
			transform.Rotate(new Vector3(0, 0, - rotationSpeed * Time.deltaTime));

			float angleSpun = Mathf.Abs(- rotationSpeed * Time.deltaTime);
			// when the angle's right, play everything on the axis
			totalRotateAmount += angleSpun;
			if (totalRotateAmount > anglePerAxis){
				for (int i = 0; i < towerLists[nextAxisToPlay].Count; i++){
					if (towerLists[nextAxisToPlay][i] != null){
						towerLists[nextAxisToPlay][i].GetComponent<BasicTower>().PlaySound();
					}
				}
				nextAxisToPlay++;
				totalRotateAmount -= anglePerAxis;
				nextAxisToPlay %= axisNumber;
			}

			// spawn enemies after each full rotation
			enemySpawnCounter += angleSpun;
			if (enemySpawnCounter > 360f){
				if (spawnEnemies){
					FindObjectOfType<EnemyManager>().SpawnEnemies();
				}
				enemySpawnCounter -= 360f;
			}

			// move enemies after each measure
			enemyMoveCounter += angleSpun;
			if (enemyMoveCounter > (360f / measurePerRotation)){
				FindObjectOfType<EnemyManager>().MoveEnemies();
				enemyMoveCounter -= 360f / measurePerRotation;
			}
		}
	}


	// sets up the correct number of axis based on axisNumber
	private void SetupAxis(){
		axes = new List<GameObject>();

		float sectionAngle = anglePerAxis * Mathf.Deg2Rad;
		for (int i = 0; i < axisNumber; i++){
			float angle = sectionAngle * i;
			GameObject line = Instantiate(axisOBj, this.transform) as GameObject;
			Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
			Vector3 newEndPoint = direction.normalized * maxAxisLength;
			Vector3 newStartPoint = direction.normalized * minAxisLength;
			line.GetComponent<LineRenderer>().SetPosition(0, newStartPoint);
			line.GetComponent<LineRenderer>().SetPosition(1, newEndPoint);
			axes.Add(line);
		}
	}


	// set the rotation of the scanner back to 12 o'clock
	// reset all rotation-based counters
	public void ResetRotation(){
		nextAxisToPlay = 4;
	    totalRotateAmount = anglePerAxis;
		enemySpawnCounter = enemyMoveCounter = 0;
	   	transform.localEulerAngles = new Vector3(-90, -90, 0);
	   	spawnEnemies = false;
	}


	// given a point in space, find the closest point on the closest axis
	public Vector3 FindPointOnAxis(Vector3 pos){
		int axisIndex = FindClosestAxisIndex(pos);
		Vector3 cloestAxis = axes[axisIndex].GetComponent<LineRenderer>().GetPosition(1);

		// return the cloest axis's unit vector scaled by pos's magnitude
		Vector3 result = cloestAxis.normalized * Mathf.Clamp(pos.magnitude, minAxisLength, maxAxisLength);
		return result;
	}


	// given a point in 3D world space, find the index of the axis cloest to that point
	public int FindClosestAxisIndex(Vector3 pos){
        // first find the angle of the pos vector
        float angle = 180 + GameManager.GetAngleFromVectorSpecial(-pos);
        // then find the vector of the cloest axis
        int axisIndex = (int)(angle / (anglePerAxis / 2f));
		axisIndex = (axisIndex + 1) / 2;
		axisIndex = axisIndex % axisNumber; // prevent overflow

		return axisIndex;
	}


	// add a tower to the list of towers of index
	public void AddTowerToList(GameObject tower){
		int index = tower.GetComponent<BasicTower>().axisIndex;
		towerLists[index].Add(tower);
	}


    // remove the tower from the list of towers
    public void RemoveTowerFromList(GameObject tower) {
        int index = tower.GetComponent<BasicTower>().axisIndex;
        towerLists[index].Remove(tower);
    }


	// show or hide all axes
	public void EnableAllAxes(bool b){
		foreach (GameObject obj in axes){
			obj.SetActive(b);
		}
	}


	// destory all towers. usually called during level transitions. 
	public void ClearTowerList(){
		for (int i = 0; i < axisNumber; i++){
			towerLists[0].Clear();
		}
	}


	// enables or disables rotation. 
	public void SetRotate(bool b){
		rotating = b;
		if (b){
			for(int i = 0; i < numAudioPlaying; i++){
				audios[i].Play();
			}
			transform.Find("ScannerLine").gameObject.SetActive(true);
		}
		else{
			for(int i = 0; i < numAudioPlaying; i++){
				audios[i].Stop();
			}
			transform.Find("ScannerLine").gameObject.SetActive(false);
		}
	}


	// stops scanner from rotating in r rotations 
	public IEnumerator StopScannerRotation(int r){
		int rotationsLeft = r;
		while (rotationsLeft > 0){
			if (nextAxisToPlay == 5){
				rotationsLeft--;
				yield return new WaitForSeconds(0.4f);
			}
			yield return null;
		}

		// stop rotation ?

		// stop music
		for(int i = 0; i < numAudioPlaying; i++){
			audios[i].Stop();
		}

		// delete all towers
		for (int i = 0; i < axisNumber; i++){
			for (int j = 0; j < towerLists[i].Count; j++){
				Destroy(towerLists[i][j]);
			}
		}

		SetRotate(false);
		finishedUp = true;

		yield break;
	}
}
