using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : BasicTower {
	private GameObject launcher;
	private LineRenderer laser;

	private AudioClip[] soundClips;
	public float damagePerSec = 0.5f;
	private float attackDuration;

	public int numSoundsUsed = 0;


	// Use this for initialization
	new void Start () {
		// set up audio clips
		soundClips = FindObjectOfType<MusicDatabase>().goldTowerClips;

		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		mRenderer = transform.Find("tower").GetComponent<MeshRenderer>();
		originalMat = mRenderer.material;

		// all towers start in the planning stage
		MakePlanning();

		// randomly pick a sound
		int maxIndex = numSoundsUsed + FindObjectOfType<GameManager>().currentLevel;
		int r = Random.Range(0, Mathf.Min(maxIndex, soundClips.Length));
		audioSource.clip = soundClips[r];
		attackDuration = audioSource.clip.length;

		// set up the laser and launcher
		laser = transform.Find("LaserBeam").GetComponent<LineRenderer>();;
		laser.gameObject.SetActive(false);
		launcher = transform.Find("Launcher").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// plays the designated sound and also does the attack
	public override void PlaySound(){
		audioSource.Play();
		anim.SetTrigger("Activate");

		// pause all other towers of this type and sound
		foreach (LaserTower tower in FindObjectsOfType<LaserTower>()){
			if (tower != this && tower.IsBuilt() && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// shoot lasers to enemies in the area
		StartCoroutine(ShootLaser());
	}


	// shoots the laser at any enemies in range
	private IEnumerator ShootLaser(){
		float t = 0f;

		while (t < attackDuration){
			List<Enemy> enemies = GetComponentInChildren<TowerAoE>().enemiesInRange;
			if (enemies.Count > 0){
				// find first enemy still alive
				Enemy target = null;
				for (int i = 0; i < enemies.Count; i++){
					if (enemies[i] != null && enemies[i].health > 0){
						target = enemies[i];
						break;
					}
				}

				if (target == null){
					// we didn't find a target, wait .2 seconds
					t += 0.2f;
					yield return new WaitForSeconds(0.2f);
				}
				else{
					// target found. keep shooting laser at it until it dies
					laser.gameObject.SetActive(true);
					while (target != null && enemies.Contains(target) && target.health > 0){
						laser.SetPosition(0, launcher.transform.position);
						laser.SetPosition(1, target.transform.position);
						target.TakeDamage(damagePerSec * Time.deltaTime * FindObjectOfType<GameManager>().attackPowerBonus);

						t += Time.deltaTime;
						yield return null;
					}
					laser.gameObject.SetActive(false);
				}

			}
			t += Time.deltaTime;
			yield return null;
		}
	}

}
