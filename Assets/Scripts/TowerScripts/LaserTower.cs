using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : BasicTower {
	private GameObject launchers;
    public float idleAttackInterval;
    private float idleAttackIntervalCounter;

	public float bulletFadeTime = 0.6f;


	// Use this for initialization
	private void Start () {
        idleAttackIntervalCounter = 0;
        // set up audio clips
        TowerMusicClips[] allClips = GameManager.instance.GetMusicDatabase().laserTowerClips;
        int randomIndex = Random.Range(0, allClips.Length);
        soundClips = allClips[randomIndex];
        audioSource.clip = soundClips.clips[info.currentLevel];

        // set up the muzzle for bullets
        launchers = transform.Find("Launchers").GetChild(0).gameObject;

        towerType = 2;
    }
	
	// Update is called once per frame
	void Update () {
        idleAttackIntervalCounter += Time.deltaTime;

        if (idleAttackIntervalCounter > idleAttackInterval) {
            idleAttackIntervalCounter = 0;

            ShootBullet(false);
        }
	}

	// plays the designated sound and also does the attack
	public override void PlaySound(){
        base.PlaySound();
        audioSource.PlayOneShot(audioSource.clip);
        //anim.SetTrigger("Activate");

        //pSystem.Emit(1);

        // pause all other towers of this type and sound
        foreach (LaserTower tower in FindObjectsOfType<LaserTower>()){
			if (tower != this && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// shoots 3 bullets towards 3 targets
		StartCoroutine(ShootFlurry());
	}


	private IEnumerator ShootFlurry(){
        for (int j = 0; j < 3; j++) {
            ShootBullet(true);
            yield return new WaitForSeconds(0.3f);
        }
	}


    private bool ShootBullet(bool powered) {
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

                for (int i = 0; i < launchers.transform.childCount; i++) {
                    GameObject bullet = Instantiate(GameManager.instance.prefabDatabase.laserBeam, Vector3.zero, Quaternion.identity);
                    LineRenderer line = bullet.GetComponent<LineRenderer>();
                    line.SetPosition(0, launchers.transform.GetChild(i).position);
                    line.SetPosition(1, target.transform.position);
                    target.TakeDamage(info.attackPowers[info.currentLevel] * (powered ? 1.3f : 1));
                }
                return true;
            }
        }

        return false;
    }


    public override void UpgradeTower() {
        if (info.currentLevel < info.maxLevel - 1) {
            info.currentLevel++;
            audioSource.clip = soundClips.clips[info.currentLevel];

            // change model
            foreach (Transform tran in transform.Find("Models").GetComponentInChildren<Transform>()) {
                tran.gameObject.SetActive(false);
            }
            transform.Find("Models").GetChild(info.currentLevel).gameObject.SetActive(true);

            // increase range
            transform.Find("AOEIndicator").localScale = new Vector3(info.attackRanges[info.currentLevel], 0.002f, info.attackRanges[info.currentLevel]);
            transform.Find("Area").GetComponent<CapsuleCollider>().radius = info.attackRanges[info.currentLevel];

            // change Launcher positions
            launchers = transform.Find("Launchers").GetChild(info.currentLevel).gameObject;
        }
    }
    

}
