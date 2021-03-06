﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPath : MonoBehaviour {
    public AudioClip turnOnPathSound;
    public float heightOffset;

	private List<LineRenderer> lineRenderers;
    private List<List<Vector3>> paths;
    private List<Vector3> spawnPoints;
    private List<GameObject> pathObjs;
    private List<GameObject> lights;
    private GameObject referencePathObj;

	private void Awake () {
        lineRenderers = new List<LineRenderer>();
        paths = new List<List<Vector3>>();
        spawnPoints = new List<Vector3>();
        lights = new List<GameObject>();
        pathObjs = new List<GameObject>();

        referencePathObj = transform.GetChild(0).gameObject;
	}
	

	private void Update () {
		foreach(LineRenderer lineRenderer in lineRenderers) {
            lineRenderer.material.mainTextureOffset = new Vector2(Time.timeSinceLevelLoad / -3, 0);
		}
	}


    public void Reset() {
        lineRenderers.Clear();
        paths.Clear();
        lights.Clear();

        for (int i = pathObjs.Count - 1; i >= 0; i--) {
            Destroy(pathObjs[i]);
        }

        pathObjs.Clear();
    }


    public void AddNewPath(EnemyPathData pathData) {
        GameObject pathObj = Instantiate(referencePathObj, transform);
        LineRenderer line = pathObj.GetComponent<LineRenderer>();
        List<Vector3> path = new List<Vector3>();
        lights.Add(pathObj.transform.Find("Light").gameObject);

        // set up the points on the line
        line.positionCount = pathData.enemyPaths.Length;
        for (int i = 0; i < pathData.enemyPaths.Length; i++) {
            line.SetPosition(i, pathData.enemyPaths[i] + new Vector3(0, heightOffset, 0));
            path.Add(pathData.enemyPaths[i]);
        }

        line.material.mainTextureScale = new Vector2(0.3f, 1);
        lineRenderers.Add(line);
        paths.Add(path);
        pathObjs.Add(pathObj);
        spawnPoints.Add(pathData.spawnPosition);
    }


    public List<Vector3> GetPath(int index) {
        return paths[index];
    }


    public Vector3 GetSpawnPoint(int index) {
        return spawnPoints[index];
    }

    public void TogglePath(int index, bool b) {
        // if we're turning on this path for the first time, play an effect
        if (lineRenderers[index].enabled == false && b) {
            GetComponent<AudioSource>().PlayOneShot(turnOnPathSound);
        }

        lineRenderers[index].enabled = b;
        lights[index].SetActive(b);
    }


    public void ToggleAllPaths(bool b) {
        for (int i = 0; i < pathObjs.Count; i++) {
            TogglePath(i, b);
        }
    }
}
