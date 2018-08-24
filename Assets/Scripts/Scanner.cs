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
    private int measureTillFinish = 0;

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
	//public static event RotationCounter RotatedFully;  // scanner has made a full rotation
	public static event RotationCounter RotatedMeasure;  // scanner has rotated a single measure


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
            if (rotationTimeCounter > timePerRotation) {
                //RotatedFully();
                // the stuff below is code for if purple line rotates more than once per full playing of song
                //measuresPlayed += lineData[0].measurePerRotation;
                //if (measuresPlayed >= measurePerSong) {
                //    PlayMusic(true);
                //    measuresPlayed = 0;
                //}
                //float tempTimePerRotation = timePerRotation; // save this value in case it changes in PlayMusic
                PlayMusic(true);
                RotatedMeasure();
                if (measureTillFinish > 0) measureTillFinish--;
                //rotationTimeCounter = 0f;
            }
            else {
                // call the RotatedMeasure event after each measure rotated
                measureTimeCounter += Time.deltaTime;
                if (measureTimeCounter > timePerMeasure) {
                    RotatedMeasure();
                    measureTimeCounter -= timePerMeasure;
                    if (measureTillFinish > 0) measureTillFinish--;
                }
            }
		}
	}


	public void SetupScanner(){
        // set up audio and do some math
        {
            MusicDatabase musicData = FindObjectOfType<GameManager>().GetMusicDatabase();
            audioClips = musicData.scannerClips;
            myAudio = GetComponent<AudioSource>();

            songPhasesTotal = audioClips.Length;
            songPhasesCurrent = 0;

            anglePerAxis = 360f / 16f;
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
            
            for (int i = 1; i < lineData.Length; i++) {
                GameObject newLineObj = Instantiate(scannerLineObj, transform);
                ScannerLine newLine = newLineObj.GetComponent<ScannerLine>();
                scannerLines.Add(newLine);
            }

            ResetScannerLines();
        }
    }


	// given a point in 3D world space, find the index of the axis cloest to that point
	public int FindClosestAxisIndex(Vector3 pos){
        // first find the angle of the pos vector
        float angle = 180 + GameManager.GetAngleFromVector(-pos);
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
		measureTillFinish = r;
		while (measureTillFinish > 0){
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
            // reset music speed for the new scanner
            ResetScannerLines();        
            myAudio.PlayOneShot(audioClips[songPhasesCurrent]);
        }
        else {
            myAudio.Stop();
        }
        
    }

    
    public void ChangeSoundPhase(int i) {
        int newPhase = songPhasesCurrent + i;
        if (newPhase < songPhasesTotal && newPhase >= 0) {
            songPhasesCurrent = newPhase;
        }
    }


    public int GetSongPhase() {
        return songPhasesCurrent;
    }

    public float GetTimePerMeasure() {
        return timePerMeasure;
    }
    

    private void ResetScannerLines() {
        MusicDatabase musicData = FindObjectOfType<GameManager>().GetMusicDatabase();

        // calculate scannerline properties based on data from the current song
        // some calculations are redundant in case rotationPerSong changes later
        measurePerSong = musicData.measurePerSong[songPhasesCurrent];
        lineData[0].measurePerRotation = measurePerSong;
        timePerSong = audioClips[songPhasesCurrent].length;
        rotationPerSong = 1;
        timePerRotation = timePerSong / rotationPerSong;
        timePerMeasure = timePerRotation / measurePerSong;
        rotationTimeCounter = measureTimeCounter = 0;
        //scannerLines[0].SetupValues(lineData[0], 360f / audioClips[songPhasesCurrent].length * rotationPerSong, anglePerAxis);

        for (int i = 0; i < scannerLines.Count; i++) {
            float rotationPerSong = measurePerSong / lineData[i].measurePerRotation;
            float rotationSpeed = 360f / audioClips[songPhasesCurrent].length * rotationPerSong;
            scannerLines[i].SetupValues(lineData[i], rotationSpeed, anglePerAxis);
        }
    }
}