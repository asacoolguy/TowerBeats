﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperTower : BasicTower {
	public GameObject bulletObj;
	private GameObject launcher;
	private TowerMusicClips[] soundClips;

	public float attackPower = 1f;
	public float bulletFadeTime = 0.6f;


	// Use this for initialization
	new void Start () {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
		mRenderer = transform.Find("tower").GetComponent<MeshRenderer>();
		originalMat = mRenderer.material;

        // set up audio clips
        soundClips = FindObjectOfType<GameManager>().GetMusicDatabase().sniperTowerClips;
        SetupSound();

        // all towers start in the planning stage
        // MakePlanning();

        // set up the muzzle for bullets
        launcher = transform.Find("Launcher").gameObject;

        // set up the light
        spotlight = transform.Find("Spotlight").GetComponent<Light>();
        spotlight.intensity = lightMinIntensity;

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
			if (tower != this && tower.IsBuilt() && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
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
				}while(enemies[enemyIndex] == null || enemies[enemyIndex].health <= 0 || enemies[enemyIndex].ascending);

				// only shoot lasers when the enemy still exist
				GameObject bullet = Instantiate(bulletObj, Vector3.zero, Quaternion.identity);
				LineRenderer line = bullet.GetComponent<LineRenderer>();
				line.SetPosition(0, launcher.transform.position);
				line.SetPosition(1, enemies[enemyIndex].transform.position);
				enemies[enemyIndex].TakeDamage(attackPower);

				yield return new WaitForSeconds(0.4f);
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
