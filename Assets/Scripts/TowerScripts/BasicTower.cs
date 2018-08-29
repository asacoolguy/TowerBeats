using System;
using System.Collections;
using UnityEngine;

public abstract class BasicTower : MonoBehaviour {
	protected AudioSource audioSource;
	protected Animator anim;
    protected TowerMusicClips soundClips;
    public int towerType = 0;
	public int axisIndex = 0;
    public TowerInfo info;

    public float popYScale, popTime, lightBoostTime;
    public float lightMinIntensity, lightMaxIntensity;
    protected Light spotlight;
    protected TowerAoE area;

    protected void Awake () {
		audioSource = GetComponent<AudioSource>();
		anim = GetComponent<Animator>();
        spotlight = transform.Find("Spotlight").GetComponent<Light>();
        spotlight.intensity = lightMinIntensity;
        area = GetComponentInChildren<TowerAoE>();
	}
	

    public virtual void PlaySound() {
        // enable the lights
        StartCoroutine(BoostLight());
        // boost the color and size of the octagon
        transform.parent.GetComponent<TowerPlatform>().BoostColor();
        transform.parent.GetComponent<TowerPlatform>().BoostSize(popYScale, popTime);
    }
    

    private IEnumerator BoostLight() {
        float currentTime = 0f;
        while (currentTime < lightBoostTime) {
            spotlight.intensity = GameManager.EaseOut(lightMinIntensity, lightMaxIntensity, currentTime / lightBoostTime);
            currentTime += Time.deltaTime;
            yield return null;
        }

        currentTime = lightBoostTime;
        while (currentTime > 0) {
            spotlight.intensity = GameManager.EaseOut(lightMinIntensity, lightMaxIntensity, currentTime / lightBoostTime);
            currentTime -= Time.deltaTime;
            yield return null;
        }
    }


    public virtual void UpgradeTower() {
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

        }   
    }
}


[Serializable]
public class TowerInfo {
    public int maxLevel, currentLevel;
    [TextArea]
    public string[] descriptions;
    public int[] costs;
    public float[] attackPowers;
    public float[] attackRanges;
}