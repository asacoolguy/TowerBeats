using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metronome : MonoBehaviour {
	public float ringInitialSize, ringFinalSize;
	private float secondsPerMeasure;
	public float measurePerWave = 1;
	private ParticleSystem pSystem;
	private AudioSource audioSource;
	public List<GameObject> rings;
	public float[] ringSize;

	public GameObject waveSphere;

	// Use this for initialization
	void Awake () {
		pSystem = GetComponent<ParticleSystem>();
		audioSource = GetComponent<AudioSource>();
		secondsPerMeasure = audioSource.clip.length / 4f;

		// set the size of the tower build rings
		int factor = 1;
		float ringFullSize = ringFinalSize - ringInitialSize;

		for (int i = 0; i < rings.Count; i++){
			rings[i].SetActive(true);
			float scale = rings[i].transform.localScale.x;
			rings[i].transform.localScale = new Vector3(ringSize[i], ringSize[i], scale);
		}
		foreach (GameObject obj in rings){
			obj.SetActive(true);

			factor++;
		}
	}
	
	// Update is called once per frame
	void Update () {
		var emit = pSystem.emission;
		emit.rateOverTime = secondsPerMeasure;
	}

	public bool IsEmitting(){
		return pSystem.isPaused;
	}

	public void StartEmission(){
		StartCoroutine(EmitWaves());
		print("playing");
	}

	public void StopEmission(){
		StopCoroutine(EmitWaves());
	}

	public float GetWaveInterval(){
		return measurePerWave * secondsPerMeasure;
	}

	private IEnumerator EmitWaves(){
		float timer = GetWaveInterval();

		while(true){
			timer += Time.deltaTime;

			if (timer > GetWaveInterval()){
				timer = 0f;
				EmitWave();
			}

			yield return null;
		}


	}

	private void EmitWave(){
		audioSource.Play();
		pSystem.Emit(1);
		Instantiate(waveSphere, Vector3.zero, Quaternion.identity, this.transform);
		foreach(GameObject tower in GameObject.FindGameObjectsWithTag("Tower")){
			if (tower.GetComponent<BasicTower>().IsBuilt()){
				tower.GetComponent<BasicTower>().MakeFunctional();
			}
		}
	}

}
