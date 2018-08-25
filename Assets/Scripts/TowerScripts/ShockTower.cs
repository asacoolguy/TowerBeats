using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockTower : BasicTower {
	private TowerMusicClips[] soundClips;
	private ParticleSystem shockwave, lightning;
    private ParticleSystemRenderer lightningRenderer;
    public float[] shockWaveRadius;
    public float shockEmitDuration, shockEmitInterval;


	private void Start () {
        soundClips = GameManager.instance.GetMusicDatabase().shockTowerClips;
        SetupSound();

        // set up particle system
        shockwave = transform.Find("Shockwave").GetComponent<ParticleSystem>();
        lightning = transform.Find("Lightning").GetComponent<ParticleSystem>();
        lightningRenderer = lightning.GetComponent<ParticleSystemRenderer>();

        towerType = 1;

        StartCoroutine(EmitShock());
    }


    private void Update() {
        // slow all enemies in area
        foreach (Enemy e in area.enemiesInRange) {
            if (e != null && e.IsVulnerable()) {
                e.Slow();
            }
        }
    }


    // plays the designated sound and also does the attack
    public override void PlaySound(){
        base.PlaySound();
        audioSource.PlayOneShot(audioSource.clip);
        //anim.SetTrigger("Activate");

        shockwave.Emit(1);

		// pause all other towers of this type and sound
		foreach (ShockTower tower in FindObjectsOfType<ShockTower>()){
			if (tower != this && tower.audioSource.clip == this.audioSource.clip && tower.audioSource.isPlaying){
				tower.audioSource.Stop();
			}
		}

		// damage enemies in area
		foreach(Enemy e in area.enemiesInRange){
            if (e != null && e.IsVulnerable()) {
                e.TakeDamage(info.attackPowers[info.currentLevel]);
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


    public override void UpgradeTower() {
        if (info.currentLevel < info.maxLevel - 1) {
            info.currentLevel++;
            SetupSound();

            // change model
            foreach (Transform tran in transform.Find("Models").GetComponentInChildren<Transform>()) {
                tran.gameObject.SetActive(false);
            }
            transform.Find("Models").GetChild(info.currentLevel).gameObject.SetActive(true);

            // increase range
            transform.Find("AOEIndicator").localScale = new Vector3(info.attackRanges[info.currentLevel], 0.002f, info.attackRanges[info.currentLevel]);
            transform.Find("Area").GetComponent<CapsuleCollider>().radius = info.attackRanges[info.currentLevel];

            // increase shockwave range
            ParticleSystem.ShapeModule donut = lightning.shape;
            donut.radius = shockWaveRadius[info.currentLevel];
        }
    }


    private IEnumerator EmitShock() {
        float interval = 0f;
        float duration = 0f;

        while (true) {
            interval += Time.deltaTime;

            if (interval > shockEmitInterval) {
                interval = duration = 0;
                
                while(duration < shockEmitDuration) {
                    duration += Time.deltaTime;

                    if (duration < shockEmitDuration / 2) {
                        float t = duration / (shockEmitDuration / 2);
                        lightningRenderer.trailMaterial.SetColor("_TintColor", new Color(1, 1, 1, t));
                    }
                    else {
                        float t = (shockEmitDuration - duration) / (shockEmitDuration / 2);
                        t = Mathf.Clamp(t, 0, 1);
                        lightningRenderer.trailMaterial.SetColor("_TintColor", new Color(1, 1, 1, t));
                    }

                    yield return null;
                }
            }

            yield return null;
        }
    }
}
