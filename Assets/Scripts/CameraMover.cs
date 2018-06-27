using System.Collections;
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
        transform.position = splashScreenPos.transform.position;
        transform.rotation = splashScreenPos.transform.rotation;
        playerControllable = false;
        moving = false;

        ToggleBlankScreen(true);
    }


    private void Update() {
        if (playerControllable) { 
            Vector3 deltaPos = Vector3.zero;
            if (Input.GetKey(KeyCode.W) && transform.position.z < maxVertical) {
                deltaPos += new Vector3(0, 0, moveSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.S) && transform.position.z > -maxVertical) {
                deltaPos += new Vector3(0, 0, -moveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A) && transform.position.x > -maxHorizontal) {
                deltaPos += new Vector3(-moveSpeed * Time.deltaTime, 0, 0);
            }
            else if (Input.GetKey(KeyCode.D) && transform.position.x < maxHorizontal) {
                deltaPos += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
            }

            transform.position = new Vector3(Mathf.Clamp(transform.position.x + deltaPos.x, -maxHorizontal, maxHorizontal),
                                             transform.position.y,
                                             Mathf.Clamp(transform.position.z + deltaPos.z, -maxVertical, maxVertical));
        }
    }


    public void MoveToGameWin(float duration) {
        // camera can only initiate a new move when the old move is finished
        if (!moving) {
            StartCoroutine(MoveToPosition(gameWinPos, duration, false, gameToWinCurve, null));
        }
    }

    public void MoveToSplashScreen(float duration) {
        // camera can only initiate a new move when the old move is finished
        if (!moving) {
            StartCoroutine(MoveToPosition(splashScreenPos, duration));
        }
    }

    public void MoveSplashToLevel(float duration) {
        // camera can only initiate a new move when the old move is finished
        if (!moving) {
            StartCoroutine(MoveToPosition(levelMenuPos, duration, false, splashToLevelCurve, null));
        }
    }

    public void MoveToGame(float duration) {
        // camera can only initiate a new move when the old move is finished
        if (!moving) {
            StartCoroutine(MoveToPosition(gamePos, duration, true, null, levelToGameCurve));
        }
    }

    public void MoveToGamePause(float duration) {
        // camera can only initiate a new move when the old move is finished
        if (!moving) {
            StartCoroutine(MoveToPosition(pausePos, duration));
        }
    }


    private IEnumerator MoveToPosition(GameObject target, float duration, bool playerControl = false, AnimationCurve moveCurve = null, AnimationCurve rotCurve = null) {
        float currentTime = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float rotationAmount = Mathf.Abs(target.transform.eulerAngles.x - transform.eulerAngles.x);
        moving = true;
        float rotationSpeed = 0;

        while (currentTime < duration) {
            currentTime = Mathf.Clamp(currentTime + Time.deltaTime, 0, duration);
            if (moveCurve == null) {
                transform.position = GameManager.SmoothStep(startPos, target.transform.position, currentTime / duration);
            }
            else {
                transform.position = GameManager.SmoothStep(startPos, target.transform.position, moveCurve.Evaluate(currentTime / duration));
            }
            if (rotCurve == null) {
                transform.rotation = Quaternion.RotateTowards(startRot, target.transform.rotation, 20f * Time.deltaTime);
            }
            else {
                rotationSpeed = rotCurve.Evaluate(currentTime / duration) - rotationSpeed;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target.transform.rotation, rotationSpeed * rotationAmount);
            }
            yield return null;
        }

        moving = false;
        playerControllable = playerControl;
    }

    public void ToggleBlankScreen(bool b) {
        transform.Find("BlankScreen").gameObject.SetActive(b);
    }
}
