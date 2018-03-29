using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTower : BasicTower {
	public GameObject bulletObj;
	private GameObject launcher;
	private AudioClip[] soundClips;
	public int numSoundsUsed = 1;

	public float attackPower = 1f;
	public float bulletFadeTime = 0.6f;


	// Use this for initialization
	new void Start () {
		// set up audio clips
		soundClips = FindObjectOfType<MusicDatabase>().blueTowerClips;

		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		mRenderer = transform.Find("shaba").GetComponent<MeshRenderer>();
		originalMat = mRenderer.material;

		// all towers start in the planning stage
		MakePlanning();

		// randomly pick a sound
		int maxIndex = numSoundsUsed + FindObjectOfType<GameManager>().currentLevel;
		int r = Random.Range(0, Mathf.Min(maxIndex, soundClips.Length));
		audioSource.clip = soundClips[r];

		// set up the muzzle for bullets
		launcher = transform.Find("Launcher").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// plays the designated sound and also does the attack
	public override void PlaySound(){
		audioSource.Play();
		anim.SetTrigger("Activate");

		//pSystem.Emit(1);

		// pause all other towers of this type and sound
		foreach (SniperTower tower in FindObjectsOfType<SniperTower>()){
			if (tower != this && tower.IsFunctional() && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// shoots 3 bullets towards 3 targets
		StartCoroutine(ShootBullets());
	}


	// shoots 3 bullets towards existing enemies. prefers hitting multiple enemies over hitting a single one
	private IEnumerator ShootBullets(){
		List<Enemy> enemies = GetComponentInChildren<TowerAoE>().enemiesInRange;
		if (enemies.Count > 0){
			for (int i = 0; i < 3; i++){
				int enemyIndex = -1;
				do{
					enemyIndex++;
					if (enemyIndex >= enemies.Count){
						// no enemies left, just return
						yield break;
					}
				}while(enemies[enemyIndex] == null || enemies[enemyIndex].health <= 0);

				// only shoot lasers when the enemy still exist
				GameObject bullet = Instantiate(bulletObj, Vector3.zero, Quaternion.identity);
				LineRenderer line = bullet.GetComponent<LineRenderer>();
				line.SetPosition(0, launcher.transform.position);
				line.SetPosition(1, enemies[enemyIndex].transform.position);
				enemies[enemyIndex].TakeDamage(attackPower * FindObjectOfType<GameManager>().attackPowerBonus);

				yield return new WaitForSeconds(0.4f);
			}
		}
	}

}
