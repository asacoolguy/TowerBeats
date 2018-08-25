using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : BasicTower {
	private GameObject launcher;
    public float plasmaFlyDuration;
    private TowerMusicClips[] soundClips;


	private void Start () {
        // set up audio clips
        soundClips = GameManager.instance.GetMusicDatabase().laserTowerClips;
        SetupSound();

        // set up the laser and launcher
		launcher = transform.Find("Launcher").gameObject;

        towerType = 3;
    }
	

	// plays the designated sound and also does the attack
	public override void PlaySound(){
        base.PlaySound();
        audioSource.PlayOneShot(audioSource.clip);
        //anim.SetTrigger("Activate");

        // pause all other towers of this type and sound
        foreach (LaserTower tower in FindObjectsOfType<LaserTower>()){
			if (tower != this && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// shoot lasers to enemies in the area
		ShootPlasma();
	}


    private void ShootPlasma() {
        List<Enemy> enemies = area.enemiesInRange;
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
                Vector3 lookPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
                transform.LookAt(lookPos);
                GameObject plasma = Instantiate(GameManager.instance.prefabDatabase.plasmaBall, launcher.transform.position, Quaternion.identity, this.transform);
                plasma.GetComponent<PlasmaBall>().SetTarget(target.gameObject, plasmaFlyDuration, info.attackPowers[info.currentLevel]);
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
