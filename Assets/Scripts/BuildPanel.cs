using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanel : MonoBehaviour {
    public Color[] towerColors;
    public Color disabledColor;
	private List<GameObject> towerButtons;
	private List<Vector3> defaultButtonPos;
    private List<bool> buttonEnabled;
	private int highlightedButton = -1;

	public float highlightedButtonSize;
	private float defaultButtonSize;
    public List<GameObject> AOEIndicators;

	private void Awake () {
		towerButtons = new List<GameObject>();
		defaultButtonPos = new List<Vector3>();
        AOEIndicators = new List<GameObject>();
        buttonEnabled = new List<bool>();

        for (int i = 0; i < towerColors.Length; i++) {
            // set up tower buttons
            GameObject towerButton = transform.GetChild(i).gameObject;
            towerButtons.Add(towerButton);
            defaultButtonPos.Add(towerButton.transform.localPosition);

            // set all buttons to disabled by default
            SetButtonColor(i, disabledColor);
            buttonEnabled.Add(false);
        }

		defaultButtonSize = towerButtons[0].transform.localScale.x;
    }

	private void Update(){
		// return all non-highlighted buttons to default state 
		for (int i = 0; i < towerColors.Length; i++){
			float newSize = defaultButtonSize;
			if (i == highlightedButton && buttonEnabled[i]){
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


    private void SetButtonColor(int index, Color c) {
        Text towerName = towerButtons[index].transform.Find("Text").GetComponent<Text>();
        Text towerCost = towerButtons[index].transform.Find("Cost").GetComponent<Text>();
        towerButtons[index].GetComponent<Image>().color = c;
        towerButtons[index].transform.Find("Rhythm").GetComponent<Image>().color = c;
        towerButtons[index].transform.Find("Description").GetComponent<Image>().color = Color.Lerp(c, Color.gray, 0.7f);
        towerButtons[index].transform.Find("Description").GetComponentInChildren<Text>().color = Color.Lerp(c, Color.gray, 0.7f);
        towerName.color = c;
        towerCost.color = c;
    }

    public void SetButtonCost(int index, int cost) {
        towerButtons[index].transform.Find("Cost").GetComponent<Text>().text = "" + cost;
    }

    public void EnableButton(int index, bool b) {
        buttonEnabled[index] = b;
        if (b) {
            SetButtonColor(index, towerColors[index]);
        }
        else {
            SetButtonColor(index, disabledColor);
        }
    }

    public bool IsButtonEnabled(int index) {
        return buttonEnabled[index];
    }
}
