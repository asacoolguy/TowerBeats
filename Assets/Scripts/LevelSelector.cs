using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour {
    private List<GameObject> levels;
    private int currentLevel;
    private bool canSelectLevels = false;
    private bool startStagePressed = false;

    public int numberOfLevels;
    public AnimationCurve changeLevelRotationCurve, changeLevelScaleCurve;
    public float rotationTime;
    public GameObject centralTower;

	private void Start () {
        ResetMenu();
        ShowLevelSelection(false);

        // TODO: hide earlier levels as the tower shifts
	}
	

	private void Update () {}


    public void ResetMenu() {
        // delete all previous levels but 1
        // TODO: make sure this works
        bool saved = true;
        for (int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).tag == "LevelSelector") {
                if (saved) {
                    saved = false;
                }
                else {
                    Destroy(transform.GetChild(i--));
                }
            }
        }

        currentLevel = 0;

        // add the first level to the list
        levels = new List<GameObject>();
        levels.Add(transform.GetChild(0).gameObject);

        // makes the other levels as needed
        float dist = levels[0].transform.localPosition.z;
        for (int i = 1; i < numberOfLevels; i++) {
            GameObject newMenu = Instantiate(levels[0], levels[0].transform.parent);
            float angle = -45 * i;
            newMenu.transform.localEulerAngles = new Vector3(0, angle, 0);
            newMenu.transform.localPosition = new Vector3(dist * Mathf.Sin(angle * Mathf.Deg2Rad), 0, dist * Mathf.Cos(angle * Mathf.Deg2Rad));
            newMenu.transform.Find("LevelText").GetComponent<Text>().text = "LEVEL\n" + (i + 1);

            levels.Add(newMenu);
        }

        // hide the left and right buttons accordingly
        levels[0].transform.Find("LeftButton").gameObject.SetActive(false);
        levels[numberOfLevels - 1].transform.Find("RightButton").gameObject.SetActive(false);
    }

    public void ShowNextLevel() {
        if (canSelectLevels && currentLevel < numberOfLevels - 1) {
            StartCoroutine(RotateToNewLevel(1));
        }
    }


    public void ShowPreviousLevel() {
        if (canSelectLevels && currentLevel > 0) {
            StartCoroutine(RotateToNewLevel(-1));
        }
    }


    public void EnableLevelSelection() {
        canSelectLevels = true;
        //centralTower.GetComponent<Animator>().enabled = false;
    }


    public void StartStage() {
        if (!startStagePressed) {
            FindObjectOfType<GameManager>().LoadStage(currentLevel);
            startStagePressed = true;
        }
    }


    public void ShowLevelSelection(bool b) {
        for(int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(b);
        }
    }


    private IEnumerator RotateToNewLevel(int direction) {
        float currentTime = 0;
        canSelectLevels = false;
        float startRotation = centralTower.transform.localEulerAngles.y;
        float startScale = centralTower.transform.localScale.x;
        bool finished = false;

        while (currentTime <= rotationTime) {
            float t = currentTime / rotationTime;
            float currentRotation = startRotation + changeLevelRotationCurve.Evaluate(t) * (direction * 45);
            float currentScale = startScale * changeLevelScaleCurve.Evaluate(t);

            centralTower.transform.localEulerAngles = new Vector3(0, currentRotation, 0);
            centralTower.transform.localScale = new Vector3(currentScale, currentScale, currentScale);

            currentTime += Time.deltaTime;
            if (!finished && currentTime > rotationTime) {
                currentTime = rotationTime;
                finished = true;
            }

            yield return null;
        }

        canSelectLevels = true;
        currentLevel += 1 * direction;
    }
}
