using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {
	private Image image;
	private int currentImageIndex = 0;
	public Sprite[] sprites;

	// Use this for initialization
	void Start () {
		if (sprites.Length == 0){
            this.gameObject.SetActive(false);
        }
		else{
			image = transform.Find("Image").GetComponent<Image>();
			image.sprite = sprites[currentImageIndex];
		}
	}

	public void ShowNextImage(){
		if (currentImageIndex == sprites.Length -1){
            this.gameObject.SetActive(false);
        }
		else{
			currentImageIndex++;
			image.sprite = sprites[currentImageIndex];

			if (currentImageIndex == sprites.Length -1){
				transform.Find("NextButton").Find("Text").GetComponent<Text>().text = "Okay";
				transform.Find("SkipButton").gameObject.SetActive(false);
			}
		}
	}
}
