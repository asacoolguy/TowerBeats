using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueTower : BasicTower {
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
		collider = GetComponent<CapsuleCollider>();

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
		foreach (BlueTower tower in FindObjectsOfType<BlueTower>()){
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
				}while(enemies[enemyIndex] == null);

				// only shoot lasers when the enemy still exist
				GameObject bullet = Instantiate(bulletObj, Vector3.zero, Quaternion.identity);
				LineRenderer line = bullet.GetComponent<LineRenderer>();
				line.SetPosition(0, launcher.transform.position);
				line.SetPosition(1, enemies[enemyIndex].transform.position);
				enemies[enemyIndex].TakeDamage(attackPower * FindObjectOfType<GameManager>().attackPowerBonus);

				StartCoroutine(FadeBulletTrail(bullet, bulletFadeTime));

				yield return new WaitForSeconds(0.4f);
			}
		}
	}

	// helper function that slowly fades out a bullet trail before destroying it
	private IEnumerator FadeBulletTrail(GameObject trail, float time){
		Material mat = trail.GetComponent<LineRenderer>().material;
		float t = 0f;
		float decaySpeed = mat.color.a / time;
		while (t < time){
			Color c = mat.color;
			mat.color = new Color(c.r, c.g, c.b, c.a -= decaySpeed * Time.deltaTime);
			yield return null;
		}

		Destroy(trail);
		yield break;
	}

}
