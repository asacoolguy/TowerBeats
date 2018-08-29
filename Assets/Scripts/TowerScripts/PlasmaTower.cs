using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaTower : BasicTower {
	private GameObject launcher;
    public float plasmaFlyDuration, splashDamageFactor;
    private float chargeTime, currentChargeTime, targetSize, originalSize;
    private GameObject plasmaBall;
    private bool initialBall;

	private void Start () {
        // set up audio clips
        TowerMusicClips[] allClips = GameManager.instance.GetMusicDatabase().plasmaTowerClips;
        int randomIndex = Random.Range(0, allClips.Length);
        soundClips = allClips[randomIndex];
        audioSource.clip = soundClips.clips[info.currentLevel];

        // set up the laser and launcher
        launcher = transform.Find("Launcher").gameObject;

        towerType = 3;

        chargeTime = GameManager.instance.GetScanner().GetRotationTime();
        plasmaBall = Instantiate(GameManager.instance.prefabDatabase.plasmaBall, launcher.transform.position, Quaternion.identity, this.transform);
        targetSize = plasmaBall.transform.localScale.x;
        originalSize = 0.001f;
        initialBall = true;
    }


    private void Update() {        
        if (!initialBall && currentChargeTime < chargeTime) {
            if (plasmaBall == null) {
                plasmaBall = Instantiate(GameManager.instance.prefabDatabase.plasmaBall, launcher.transform.position, Quaternion.identity, this.transform);
            }

            currentChargeTime += Time.deltaTime;
            float t = currentChargeTime / chargeTime;
            float newSize = Mathf.Lerp(originalSize, targetSize, t);
            plasmaBall.transform.localScale = new Vector3(newSize, newSize, newSize);
        }
    }


    // plays the designated sound and also does the attack
    public override void PlaySound(){
        base.PlaySound();
        audioSource.PlayOneShot(audioSource.clip);
        //anim.SetTrigger("Activate");

        // pause all other towers of this type and sound
        foreach (PlasmaTower tower in FindObjectsOfType<PlasmaTower>()){
			if (tower != this && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// shoot lasers to enemies in the area
		bool attackSuccess = ShootPlasma();
        if (attackSuccess) {
            plasmaBall = null;
            currentChargeTime = 0;
            initialBall = false;
        }
    }


    private bool ShootPlasma() {
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
                plasmaBall.GetComponent<PlasmaBall>().SetTarget(target.gameObject, plasmaFlyDuration, info.attackPowers[info.currentLevel], splashDamageFactor);
                return true;
            }
        }

        return false;
    }

}
