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
    // variables for song info
    private int songPhasesTotal, songPhasesCurrent;
    public int axisNumber;
    private float anglePerAxis;
    public float measurePerSong;
    private float timePerSong, timePerRotation, timePerMeasure;
    private float rotationPerSong;
	private float rotationSpeed;
	public bool rotating;
    private float measuresPlayed;
	

	// organized by axes. towerLists[1] returns all towers on axes 1.
	[SerializeField]private List<GameObject>[] towerLists; 

    // variables for tracking current rotation amount
    private float rotationTimeCounter, measureTimeCounter;
    private int rotationTillFinish = 0;

	// variables for kepeing track of audio
	private AudioSource myAudio;
	private AudioClip[] audioClips;
	public int numAudioPlaying = 1;

    // data for ScannerLineObjects
    public ScannerLineData[] lineData;
    private GameObject scannerLineObj;
    private List<ScannerLine> scannerLines;

    // full rotation events
    public delegate void RotationCounter();
	public static event RotationCounter RotatedFully;  // scanner has made a full rotation
	public static event RotationCounter RotatedMeasure;  // scanner has rotated a single measure


	// Use this for initialization
	void Awake () {
        SetupScanner();
        ResetScannerLines();
        ShowScannerLine(false);
	}

	// Update is called once per frame
	void Update () {
		if (rotating){
            // rotate the scannerLines
            foreach (ScannerLine line in scannerLines) {
                line.Rotate();
                line.TriggerTowers(towerLists[line.nextAxisToPlay]);
            }

            // call the RotatedFully event after full rotation
            rotationTimeCounter += Time.deltaTime;
			if (rotationTimeCounter > timePerRotation){
				RotatedFully();
                measuresPlayed += lineData[0].measurePerRotation;
                if (measuresPlayed >= measurePerSong) {
                    PlayMusic(true);
                    measuresPlayed = 0;
                }
                rotationTimeCounter -= timePerRotation;
                if (rotationTillFinish > 0) rotationTillFinish--;
			}

			// call the RotatedMeasure event after each measure rotated
			measureTimeCounter += Time.deltaTime;
			if (measureTimeCounter > timePerMeasure){
				RotatedMeasure();
                measureTimeCounter -= timePerMeasure;
			}
		}
	}


	public void SetupScanner(){
        // set up audio and do some math
        {
            
            audioClips = FindObjectOfType<MusicDatabase>().scannerClips;
            myAudio = GetComponent<AudioSource>();

            songPhasesTotal = audioClips.Length;
            songPhasesCurrent = 0;

            anglePerAxis = 360f / 16f;
            timePerSong = audioClips[0].length;
            rotationPerSong = measurePerSong / lineData[0].measurePerRotation;
            timePerRotation = timePerSong / rotationPerSong;
            timePerMeasure = timePerRotation / lineData[0].measurePerRotation;
            rotationTimeCounter = measureTimeCounter = 0;
        }

        // initialize towerLists
        {
            towerLists = new List<GameObject>[axisNumber];
            for (int i = 0; i < axisNumber; i++) {
                towerLists[i] = new List<GameObject>();
            }
        }

        // initialize scannerLines
        {
            scannerLineObj = transform.Find("ScannerLine").gameObject;
            scannerLines = new List<ScannerLine>();

            scannerLines.Add(scannerLineObj.GetComponent<ScannerLine>());
            scannerLines[0].SetupValues(lineData[0], 360f / timePerSong * rotationPerSong, anglePerAxis);

            for (int i = 1; i < lineData.Length; i++) {
                GameObject newLineObj = Instantiate(scannerLineObj, transform);
                ScannerLine newLine = newLineObj.GetComponent<ScannerLine>();
                
                rotationPerSong = measurePerSong / lineData[i].measurePerRotation;
                rotationSpeed = 360f / audioClips[i].length * rotationPerSong;
                newLine.SetupValues(lineData[i], rotationSpeed, anglePerAxis);

                scannerLines.Add(newLine);
            }
        }
    }


    // resets the rotation of all scanner lines
    public void ResetScannerLines() {
        foreach (ScannerLine line in scannerLines) {
            line.ResetValues();
        }
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


	// destory all towers. usually called during level transitions. 
	public void DestroyAllTowers(){
		for (int i = 0; i < axisNumber; i++){
            for (int j = towerLists[i].Count - 1; j >= 0; j--) {
                Destroy(towerLists[i][j]);
            }
			towerLists[i].Clear();
		}
	}


	// starts or stops rotation. 
	public void SetRotate(bool b){
		rotating = b;
        PlayMusic(b);
        ShowScannerLine(b);
	}

    
    // returns the time required for a whole rotation
    public float GetRotationTime() {
        return timePerSong;
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
        foreach (ScannerLine line in scannerLines) {
            line.gameObject.SetActive(b);
        }
    }

    private void PlayMusic(bool b) {
        if (b) {
            myAudio.PlayOneShot(audioClips[songPhasesCurrent]);
        }
        else {
            myAudio.Stop();
        }
        
    }
}
