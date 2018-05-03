using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockTower : BasicTower {
	private AudioClip[] soundClips;
	private ParticleSystem pSystem;
	public float attackPower = 1f;

	// Use this for initialization
	new void Start () {
		// set up audio clips
		soundClips = FindObjectOfType<MusicDatabase>().greenTowerClips;

		base.Start();

		// randomly pick a sound
		int r = Random.Range(0, soundClips.Length);
		audioSource.clip = soundClips[r];

		// set up particle system
		pSystem = transform.GetChild(0).GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// plays the designated sound and also does the attack
	public override void PlaySound(){
        base.PlaySound();
        audioSource.Play();
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
    
}
