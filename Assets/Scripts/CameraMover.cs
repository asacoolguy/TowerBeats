﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Moves the camera about using wsad 
 */
public class CameraMover : MonoBehaviour {
    public float maxVertical, maxHorizontal;
    public float moveSpeed;
    public GameObject splashScreenPos, levelMenuPos, gamePos, pausePos, gameWinPos;
    private bool playerControllable, moving;

    public AnimationCurve splashToLevelCurve, levelToGameCurve, gameToWinCurve;

    private void Start() {
        // default is splashScreenPos
        //transform.position = splashScreenPos.transform.position;
        //transform.rotation = splashScreenPos.transform.rotation;
        //playerControllable = false;
        //moving = false;

        //ToggleBlankScreen(true);
    }


    private void Update() {
        //if (playerControllable) { 
        //    Vector3 deltaPos = Vector3.zero;
        //    if (Input.GetKey(KeyCode.W) && transform.position.z < maxVertical) {
        //        deltaPos += new Vector3(0, 0, moveSpeed * Time.deltaTime);
        //    }
        //    else if (Input.GetKey(KeyCode.S) && transform.position.z > -maxVertical) {
        //        deltaPos += new Vector3(0, 0, -moveSpeed * Time.deltaTime);
        //    }
        //    if (Input.GetKey(KeyCode.A) && transform.position.x > -maxHorizontal) {
        //        deltaPos += new Vector3(-moveSpeed * Time.deltaTime, 0, 0);
        //    }
        //    else if (Input.GetKey(KeyCode.D) && transform.position.x < maxHorizontal) {
        //        deltaPos += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
        //    }

        //    transform.position = new Vector3(Mathf.Clamp(transform.position.x + deltaPos.x, -maxHorizontal, maxHorizontal),
        //                                     transform.position.y,
        //                                     Mathf.Clamp(transform.position.z + deltaPos.z, -maxVertical, maxVertical));
        //}
    }


    public void ToggleBlankScreen(bool b) {
        transform.Find("BlankScreen").gameObject.SetActive(b);
    }
}
