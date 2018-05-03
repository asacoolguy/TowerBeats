using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanel : MonoBehaviour {
    public Color[] towerColors;
	private List<GameObject> towerButtons;
	private List<Vector3> defaultButtonPos;
	private int highlightedButton = -1;

	public float highlightedButtonSize;
	private float defaultButtonSize;
    public List<GameObject> AOEIndicators;

	private void Awake () {
		towerButtons = new List<GameObject>();
		defaultButtonPos = new List<Vector3>();
        AOEIndicators = new List<GameObject>();

        for (int i = 0; i < towerColors.Length; i++) {
            string name = "Tower" + (i + 1);
            GameObject towerButton = transform.Find(name).gameObject;
			towerButtons.Add(towerButton);
			defaultButtonPos.Add(towerButton.transform.localPosition);

            Text towerName = towerButton.transform.Find("Text").GetComponent<Text>();
            Text towerCost = towerButton.transform.Find("Cost").GetComponent<Text>();
			towerButton.GetComponent<Image>().color = towerColors[i];
            towerButton.transform.Find("Description").GetComponent<Image>().color = towerColors[i];
            towerButton.transform.Find("Description").GetComponentInChildren<Text>().color = towerColors[i];
            towerName.color = towerColors[i];
            towerCost.color = towerColors[i];
        }

		defaultButtonSize = towerButtons[0].transform.localScale.x;
    }

	private void Update(){
		// return all non-highlighted buttons to default state 
		for (int i = 0; i < towerColors.Length; i++){
			float newSize = defaultButtonSize;
			if (i == highlightedButton){
				// highlight this button
				newSize = towerButtons[i].transform.localScale.x + 2 * Time.deltaTime;
			}
			else{
				// return this button to normal
				newSize = towerButtons[i].transform.localScale.x - 2 * Time.deltaTime;
			}
			newSize = Mathf.Clamp(newSize, defaultButtonSize, highlightedButtonSize);
			towerButtons[i].transform.localScale = new Vector3(newSize, newSize, newSize);
		}
	}


	public void HighlightButton(int index){
		highlightedButton = index;
        for (int i = 0; i < towerButtons.Count; i++) {
            bool temp = (i == index);
            towerButtons[i].transform.Find("Description").gameObject.SetActive(temp);
            AOEIndicators[i].SetActive(temp);
        }
    }


	// sets the gameobject to active and plays a little animation
	public void ActivatePanel(bool b){
        if (b) {
            gameObject.SetActive(b);
            StopCoroutine(PlayActivateAnimation());
            StartCoroutine(PlayActivateAnimation());
        }
        else {
            transform.SetParent(null, true);
            gameObject.SetActive(false);
        }
	}

	private IEnumerator PlayActivateAnimation(){
		float counter, t, limit;
		counter = t = 0;
		limit = 0.2f;
		Vector3 startPos = new Vector3(0,0, 80);

		while (counter <= limit){
			counter += Time.deltaTime;
			t = Mathf.Clamp(counter / limit, 0f, 1f);

			for (int i = 0; i < towerColors.Length; i++){
				towerButtons[i].transform.localPosition = GameManager.SmoothStep(startPos, defaultButtonPos[i], t);
			}
			yield return null;
		}
	}
}
