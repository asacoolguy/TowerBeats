using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : BasicTower {
	private GameObject launcher;
	private LineRenderer laser;

	private TowerMusicClips[] soundClips;
	private float attackDuration;


	private void Start () {
        // set up audio clips
        soundClips = FindObjectOfType<GameManager>().GetMusicDatabase().laserTowerClips;
        SetupSound();

        // set up the laser and launcher
        laser = transform.Find("LaserBeam").GetComponent<LineRenderer>();;
		laser.gameObject.SetActive(false);
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
		StartCoroutine(ShootLaser());
	}


	// shoots the laser at any enemies in range
	private IEnumerator ShootLaser(){
		float t = 0f;

		while (t < attackDuration){
			List<Enemy> enemies = GetComponentInChildren<TowerAoE>().enemiesInRange;
			if (enemies.Count > 0){
				// find farthest enemy still alive
				Enemy target = null;
                float currentDist = 0;
				for (int i = 0; i < enemies.Count; i++){
					if (enemies[i] != null && enemies[i].health > 0 && !enemies[i].ascending && enemies[i].GetTravelDist() > currentDist){
						target = enemies[i];
                        currentDist = enemies[i].GetTravelDist();
					}
				}

				if (target == null){
					// we didn't find a target, wait .1 seconds
					t += 0.1f;
                    laser.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.1f);
				}
				else{
					// target found. keep shooting laser at it until it dies
					laser.gameObject.SetActive(true);
					if (target != null && enemies.Contains(target) && target.health > 0){
						laser.SetPosition(0, launcher.transform.position);
						laser.SetPosition(1, target.transform.position);
						target.TakeDamage(info.attackPowers[info.currentLevel] * Time.deltaTime);

						//t += Time.deltaTime;
						//yield return null;
					}
					//laser.gameObject.SetActive(false);
				}

			}
			t += Time.deltaTime;
			yield return null;
		}
        laser.gameObject.SetActive(false);
    }


    public override void SetupSound() {
        // randomly pick a sound
        TowerMusicClips musicClips = soundClips[Mathf.Clamp(info.currentLevel, 0, soundClips.Length - 1)];
        if (randomClipIndex == -1) {
            randomClipIndex = Random.Range(0, musicClips.clips.Length);
        }
        audioSource.clip = musicClips.clips[randomClipIndex];
        //attackDuration = audioSource.clip.length;
        attackDuration = FindObjectOfType<Scanner>().getTimePerMeasure() * 2;
    }

}
