/*
    Manager for level-specific UI such as that level's build tower options.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LevelUI : MonoBehaviour {
    public GameObject greenBuildButton, blueBuildButton, goldBuildButton;
    private GameManagerNew gameManager;

    void Start () {
	}

    // sets up the build tower buttons accordingly to availability
    public void SetupBuildTowerButtons(bool greenOkay, bool blueOkay, bool goldOkay) {
        ToggleShockTowerButton(greenOkay);
        ToggleSniperTowerButton(blueOkay);
        ToggleLaserTowerButton(goldOkay);
    }


    public void ToggleShockTowerButton(bool b) {
        Button button = greenBuildButton.GetComponent<Button>();
        Text text = greenBuildButton.GetComponentInChildren<Text>();
        greenBuildButton.GetComponent<Animator>().SetBool("Available", b);
        if (b && button.IsInteractable() == false) {
            button.interactable = true;
            text.text = "Shock";
            greenBuildButton.GetComponent<Animator>().SetTrigger("Enable");
            // TODO: find better solution than to just resort to calling the old file again
            FindObjectOfType<UIManagerNew>().PlayButtonEnableSound();
        }
        else if (!b) {
            button.interactable = false;
            text.text = "Unavailable";
        }
    }

    public void ToggleSniperTowerButton(bool b) {
        Button button = blueBuildButton.GetComponent<Button>();
        Text text = blueBuildButton.GetComponentInChildren<Text>();
        blueBuildButton.GetComponent<Animator>().SetBool("Available", b);
        if (b && button.IsInteractable() == false) {
            button.interactable = true;
            text.text = "Sniper";
            blueBuildButton.GetComponent<Animator>().SetTrigger("Enable");
            FindObjectOfType<UIManagerNew>().PlayButtonEnableSound();
        }
        else if (!b) {
            button.interactable = false;
            text.text = "Unavailable";
        }
    }

    public void ToggleLaserTowerButton(bool b) {
        Button button = goldBuildButton.GetComponent<Button>();
        Text text = goldBuildButton.GetComponentInChildren<Text>();
        goldBuildButton.GetComponent<Animator>().SetBool("Available", b);
        if (b && button.IsInteractable() == false) {
            button.interactable = true;
            text.text = "Laser";
            goldBuildButton.GetComponent<Animator>().SetTrigger("Enable");
            FindObjectOfType<UIManagerNew>().PlayButtonEnableSound();
        }
        else if (!b) {
            button.interactable = false;
            text.text = "Unavailable";
        }
    }
}
