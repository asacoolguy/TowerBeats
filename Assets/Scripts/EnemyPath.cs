using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPath : MonoBehaviour {
    public float lineOffset;
	private List<LineRenderer> lineRenderers;
    private List<List<Vector3>> paths;
    private List<GameObject> lights;
	private List<float> magnitudes;

	private void Start () {
        lineRenderers = new List<LineRenderer>();
        paths = new List<List<Vector3>>();
        lights = new List<GameObject>();

		// set up the points on the line
		for(int i = 0; i < transform.childCount; i++){
			GameObject pathObj = transform.GetChild(i).gameObject;
			LineRenderer line = pathObj.GetComponent<LineRenderer>();
            List<Vector3> path = new List<Vector3>();

			line.positionCount = pathObj.transform.childCount - 2; // minus 2 because 1 is spawn point, 1 is light
			for (int j = 0; j < pathObj.transform.childCount; j++){
                GameObject pointObj = pathObj.transform.GetChild(j).gameObject;
                if (pointObj.tag == "SpawnPoint" || pointObj.tag == "Waypoint") {
                    Vector3 pos = pathObj.transform.GetChild(j).position;
                    path.Add(pos);
                    if (pointObj.tag == "Waypoint") {
                        line.SetPosition(j - 1, pos + new Vector3(0, lineOffset, 0));
                    }
                }
                else {
                    lights.Add(pointObj);
                }
			}

			line.material.mainTextureScale = new Vector2(0.3f, 1);
            lineRenderers.Add(line);
            paths.Add(path);
            //lights.Add(pathObj.transform.Find("Light").gameObject);
		}

        // turn off all lights
        for (int i = 0; i < lights.Count; i++) {
            ToggleLight(i, false);
        }
	}
	

	private void Update () {
		foreach(LineRenderer lineRenderer in lineRenderers) {
            lineRenderer.material.mainTextureOffset = new Vector2(Time.timeSinceLevelLoad / -3, 0);
		}

	}

    public List<Vector3> GetPath(int index) {
            return paths[index];
    }

    public void ToggleLight(int index, bool on) {
        lights[index].SetActive(on);
    }
}
