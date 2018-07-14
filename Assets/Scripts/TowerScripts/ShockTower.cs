using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockTower : BasicTower {
	private TowerMusicClips[] soundClips;
	private ParticleSystem pSystem;
	public float attackPower = 1f;

	// Use this for initialization
	new void Start () {
		base.Start();
        towerType = 1;

        soundClips = FindObjectOfType<GameManager>().GetMusicDatabase().shockTowerClips;
        SetupSound();

		// set up particle system
		pSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
	}
	

	// plays the designated sound and also does the attack
	public override void PlaySound(){
        base.PlaySound();
        audioSource.PlayOneShot(audioSource.clip);
		//anim.SetTrigger("Activate");

		pSystem.Emit(1);

		// pause all other towers of this type and sound
		foreach (ShockTower tower in FindObjectsOfType<ShockTower>()){
			if (tower != this && tower.IsBuilt() && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// damage enemies in area
		foreach(Enemy e in GetComponentInChildren<TowerAoE>().enemiesInRange){
            if (!e.ascending) {
                e.TakeDamage(attackPower * powerFactor);
                e.Slow();
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
