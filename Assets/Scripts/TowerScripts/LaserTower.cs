using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : BasicTower {
	private GameObject launcher;
	private LineRenderer laser;

	private TowerMusicClips[] soundClips;
	public float damagePerSec = 0.5f;
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
				// find first enemy still alive
				Enemy target = null;
				for (int i = 0; i < enemies.Count; i++){
					if (enemies[i] != null && enemies[i].health > 0 && !enemies[i].ascending){
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
						target.TakeDamage(damagePerSec * Time.deltaTime);

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


    public override void SetupSound() {
        // randomly pick a sound
        TowerMusicClips musicClips = soundClips[Mathf.Clamp(info.currentLevel, 0, soundClips.Length - 1)];
        if (randomClipIndex == -1) {
            randomClipIndex = Random.Range(0, musicClips.clips.Length);
        }
        audioSource.clip = musicClips.clips[randomClipIndex];
        attackDuration = audioSource.clip.length;
    }

}
