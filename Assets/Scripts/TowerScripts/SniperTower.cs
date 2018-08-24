using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTower : BasicTower {
	public GameObject bulletObj;
	private GameObject launcher;
	private TowerMusicClips[] soundClips;

	public float bulletFadeTime = 0.6f;


	// Use this for initialization
	private void Start () {
        // set up audio clips
        soundClips = FindObjectOfType<GameManager>().GetMusicDatabase().sniperTowerClips;
        SetupSound();
        
        // set up the muzzle for bullets
        launcher = transform.Find("Launcher").gameObject;

        towerType = 2;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	// plays the designated sound and also does the attack
	public override void PlaySound(){
        base.PlaySound();
        audioSource.PlayOneShot(audioSource.clip);
        //anim.SetTrigger("Activate");

        //pSystem.Emit(1);

        // pause all other towers of this type and sound
        foreach (SniperTower tower in FindObjectsOfType<SniperTower>()){
			if (tower != this && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// shoots 3 bullets towards 3 targets
		StartCoroutine(ShootBullets());
	}


	// shoots 3 bullets towards existing enemies. prefers hitting multiple enemies over hitting a single one
	private IEnumerator ShootBullets(){
        for (int j = 0; j < 3; j++) {
            List<Enemy> enemies = GetComponentInChildren<TowerAoE>().enemiesInRange;
            if (enemies.Count > 0) {
                // find farthest enemy still alive
                Enemy target = null;
                float currentDist = 0;
                for (int i = 0; i < enemies.Count; i++) {
                    if (enemies[i] != null && enemies[i].health > 0 && enemies[i].IsVulnerable() && enemies[i].GetTravelDist() > currentDist) {
                        target = enemies[i];
                        currentDist = enemies[i].GetTravelDist();
                    }
                }

                if (target != null) {
                    // only shoot lasers when the enemy still exist
                    GameObject bullet = Instantiate(bulletObj, Vector3.zero, Quaternion.identity);
                    LineRenderer line = bullet.GetComponent<LineRenderer>();
                    line.SetPosition(0, launcher.transform.position);
                    line.SetPosition(1, target.transform.position);
                    target.TakeDamage(info.attackPowers[info.currentLevel]);

                    yield return new WaitForSeconds(0.4f);
                }
            }
        }
	}

    public override void SetupSound() {
        // randomly pick a sound
        TowerMusicClips musicClips = soundClips[Mathf.Clamp(info.currentLevel, 0, soundClips.Length - 1)];
        if (randomClipIndex == -1) {
            randomClipIndex = Random.Range(0, musicClips.clips.Length);
        }
        audioSource.clip = musicClips.clips[randomClipIndex];
    }

}
