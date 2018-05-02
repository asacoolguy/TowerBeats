using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPath : MonoBehaviour {
	private List<LineRenderer> lines;
	private List<float> magnitudes;

	private void Start () {
		lines = new List<LineRenderer>();

		// set up the points on the line
		for(int i = 0; i < transform.childCount; i++){
			GameObject path = transform.GetChild(0).gameObject;
			LineRenderer line = path.GetComponent<LineRenderer>();
			float mag = 0f;

			line.positionCount = path.transform.childCount;
			for (int j = 0; j < path.transform.childCount; j++){
				line.SetPosition(j, path.transform.GetChild(j).position);
			}

			line.material.mainTextureScale = new Vector2(0.3f, 1);
			lines.Add(line);
		}
	}
	

	private void Update () {
		foreach(LineRenderer line in lines){
			line.material.mainTextureOffset = new Vector2(Time.timeSinceLevelLoad / -3, 0);
		}

	}
}
