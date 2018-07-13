using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour {
    private Color towerColor;
    private List<GameObject> buttons;
    private List<Vector3> defaultButtonPos;
    private List<bool> buttonEnabled;
    private int highlightedButton = -1;

    public float highlightedButtonSize;
    private float defaultButtonSize;


    private void Awake() {
        buttons = new List<GameObject>();
        defaultButtonPos = new List<Vector3>();
        buttonEnabled = new List<bool>();

        for (int i = 0; i < 2; i++) {
            // set up tower buttons
            GameObject button = transform.GetChild(i).gameObject;
            buttons.Add(button);
            defaultButtonPos.Add(button.transform.localPosition);

            // set all buttons to disabled by default
            //SetButtonColor(i, disabledColor);
            buttonEnabled.Add(false);
        }

        defaultButtonSize = buttons[0].transform.localScale.x;
    }


    private void Update() {
        // return all non-highlighted buttons to default state 
        for (int i = 0; i < 2; i++) {
            float newSize = defaultButtonSize;
            if (i == highlightedButton && buttonEnabled[i]) {
                // highlight this button
                newSize = buttons[i].transform.localScale.x + 2 * Time.deltaTime;
            }
            else {
                // return this button to normal
                newSize = buttons[i].transform.localScale.x - 2 * Time.deltaTime;
            }
            newSize = Mathf.Clamp(newSize, defaultButtonSize, highlightedButtonSize);
            buttons[i].transform.localScale = new Vector3(newSize, newSize, newSize);
        }
    }


    public void HighlightButton(int index) {
        highlightedButton = index;
        for (int i = 0; i < 2; i++) {
            buttons[i].transform.Find("Description").gameObject.SetActive((i == index));
        }
    }

    // sets the gameobject to active and plays a little animation
    public void ActivatePanel(bool b) {
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


    private IEnumerator PlayActivateAnimation() {
        float counter, t, limit;
        counter = t = 0;
        limit = 0.2f;
        Vector3 startPos = new Vector3(0, 0, 80);

        while (counter <= limit) {
            counter += Time.deltaTime;
            t = Mathf.Clamp(counter / limit, 0f, 1f);

            for (int i = 0; i < 2; i++) {
                buttons[i].transform.localPosition = GameManager.SmoothStep(startPos, defaultButtonPos[i], t);
            }
            yield return null;
        }
    }


    private void SetButtonInfo(Color c, int upgradeLvl, string upgradeDesc, int upgradeCost) {
        //Text towerName = towerButtons[index].transform.Find("Text").GetComponent<Text>();
        //Text towerCost = towerButtons[index].transform.Find("Cost").GetComponent<Text>();
        //towerButtons[index].GetComponent<Image>().color = c;
        //towerButtons[index].transform.Find("Rhythm").GetComponent<Image>().color = c;
        //towerButtons[index].transform.Find("Description").GetComponent<Image>().color = Color.Lerp(c, Color.gray, 0.7f);
        //towerButtons[index].transform.Find("Description").GetComponentInChildren<Text>().color = Color.Lerp(c, Color.gray, 0.7f);
        //towerName.color = c;
        //towerCost.color = c;
    }


    private void SetButtonColor(int index, Color c) {
        if (index == 0) {
            buttons[index].transform.Find("Text").GetComponent<Text>().color = c;
            buttons[index].transform.Find("Cost").GetComponent<Text>().color = c;
            buttons[index].GetComponent<Image>().color = c;
            buttons[index].transform.Find("Rhythm").GetComponent<Image>().color = c;
            buttons[index].transform.Find("Description").GetComponent<Image>().color = Color.Lerp(c, Color.gray, 0.7f);
            buttons[index].transform.Find("Description").GetComponentInChildren<Text>().color = Color.Lerp(c, Color.gray, 0.7f);
        }
        else {
            buttons[index].transform.Find("Text").GetComponent<Text>().color = c;
            buttons[index].GetComponent<Image>().color = c;
            buttons[index].transform.Find("Description").GetComponent<Image>().color = Color.Lerp(c, Color.gray, 0.7f);
            buttons[index].transform.Find("Description").GetComponentInChildren<Text>().color = Color.Lerp(c, Color.gray, 0.7f);
        }
    }


    public void EnableButton(int index, bool b) {
        buttonEnabled[index] = b;
        if (b) {
            SetButtonColor(index, towerColor);
        }
        else {
            SetButtonColor(index, Color.gray);
        }
    }

    public bool IsButtonEnabled(int index) {
        return buttonEnabled[index];
    }
}
