using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls the scanner that rotates 360 degrees every 2 measures. 
///
/// The scanner starts off not rotating, facing 12 o'clock. Use setRotate to start 
/// or pause rotations. Axes will be constructed upon object initiation to indicate
/// where towers can be built. 
/// </summary>

public class Scanner : MonoBehaviour {
	// variables for tracking rotations
    public int measurePerSong;
	private int measurePerRotation;
    private int rotationPerSong;
	private float rotationSpeed;
	public bool rotating;
    private int measuresPlayed;

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
	private float fullRotationCounter;
	private float measureRotationCounter;
    private int rotationTillFinish = 0;

	// variables for kepeing track of audio
	private AudioSource[] audios;
	private AudioClip[] audioClips;
	public int numAudioPlaying = 1;


	// full rotation events
	public delegate void RotationCounter();
	public static event RotationCounter RotatedFully;  // scanner has made a full rotation
	public static event RotationCounter RotatedMeasure;  // scanner has rotated a single measure


	// Use this for initialization
	void Awake () {
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

        measurePerRotation = 2;
        rotationPerSong = measurePerSong / measurePerRotation;
		anglePerAxis = 360f / axisNumber;
		rotationSpeed = 360f / audios[0].clip.length * rotationPerSong;
        

		// initialize towerLists
		towerLists = new List<GameObject>[axisNumber];
		for (int i = 0; i < axisNumber; i++){
			towerLists[i] = new List<GameObject>();
		}

		// initialize variables for rotation counting
		ResetRotation();

        ShowScannerLine(false);
        // start rotation after some delay
        //StartCoroutine(StartScannerRotation(2f));
	}

	// Update is called once per frame
	void Update () {
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

			// after each full rotation, run the FullyRotated event 
			fullRotationCounter += angleSpun;
			if (fullRotationCounter > 360f){
				RotatedFully();
                measuresPlayed += measurePerRotation;
                if (measuresPlayed >= measurePerSong) {
                    PlayMusic(true);
                    measuresPlayed = 0;
                }
                fullRotationCounter -= 360f;
                if (rotationTillFinish > 0) rotationTillFinish--;
			}

			// move enemies after each measure
			measureRotationCounter += angleSpun;
			if (measureRotationCounter > (360f / measurePerRotation)){
				RotatedMeasure();
				measureRotationCounter -= (360f / measurePerRotation);
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
		fullRotationCounter = measureRotationCounter = 0;
	   	transform.localEulerAngles = new Vector3(-90, -90, 0);
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
	public void DestroyAllTowers(){
		for (int i = 0; i < axisNumber; i++){
            for (int j = towerLists[i].Count - 1; j >= 0; j--) {
                Destroy(towerLists[i][j]);
            }
			towerLists[i].Clear();
		}
	}


	// enables or disables rotation. 
	public void SetRotate(bool b){
		rotating = b;
		if (b){
            PlayMusic(true);
			transform.Find("ScannerLine").gameObject.SetActive(true);
            transform.Find("Spotlight").gameObject.SetActive(true);
        }
		else{
            PlayMusic(false);
			transform.Find("ScannerLine").gameObject.SetActive(false);
            transform.Find("Spotlight").gameObject.SetActive(false);
        }
	}

    
    // returns the time required for a whole rotation
    public float GetRotationTime() {
        return audios[0].clip.length;
    }

	// stops scanner from rotating in r rotations 
	public IEnumerator StopScannerRotation(int r){
		rotationTillFinish = r;
		while (rotationTillFinish > 0){
			yield return null;
		}

        // stop everything
		SetRotate(false);
	}

    private IEnumerator StartScannerRotation(float delay) {
        yield return new WaitForSeconds(delay);
        SetRotate(true);
    }


    public void ShowScannerLine(bool b) {
        transform.Find("Spotlight").gameObject.SetActive(b);
        transform.Find("ScannerLine").gameObject.SetActive(b);
    }

    private void PlayMusic(bool b) {
        if (b) {
            for (int i = 0; i < numAudioPlaying; i++) {
                audios[i].PlayOneShot(audios[i].clip);
            }
        }
        else {
            for (int i = 0; i < audios.Length; i++) {
                audios[i].Stop();
            }
        }
        
    }
}
