using System;
using System.Collections.Generic;
using UnityEngine;

public class ScannerLine : MonoBehaviour {
    // variables for tracking rotations
    public float rotationSpeed;
    public float anglePerAxis;
    public int nextAxisToPlay;
    public string targetTowers;

    private float totalRotateAmount;
    private bool readyToPlay;
    

    public void Rotate() {
        transform.Rotate(new Vector3(0, 0, -rotationSpeed * Time.deltaTime));
        // when the angle's right, play everything on the axis
        totalRotateAmount += Mathf.Abs(-rotationSpeed * Time.deltaTime); ;
        if (totalRotateAmount > anglePerAxis) {
            readyToPlay = true;
        }
    }


    public void TriggerTowers(List<GameObject> towerList) {
        if (readyToPlay) {
            readyToPlay = false;

            for (int i = 0; i < towerList.Count; i++) {
                if (towerList[i] != null && targetTowers.Contains("" + towerList[i].GetComponent<BasicTower>().towerType)) {
                    towerList[i].GetComponent<BasicTower>().PlaySound();
                }
            }

            nextAxisToPlay++;
            totalRotateAmount -= anglePerAxis;
            nextAxisToPlay %= 16;
        }
    }
    

    public void SetupValues(ScannerLineData data, float speed, float angle) {
        SetColor(data.color);
        rotationSpeed = speed;
        anglePerAxis = angle;
        targetTowers = data.targetTowers;
        ResetValues();
    }


    public void ResetValues() {
        nextAxisToPlay = 4;
        readyToPlay = false;
        totalRotateAmount = anglePerAxis;
        transform.localEulerAngles = Vector3.zero;
    }


    public void SetColor(Color c) {
        transform.Find("Line").GetComponent<MeshRenderer>().material.color = c;
        transform.Find("Spotlight").GetComponent<Light>().color = c;
    }
}



[Serializable]
public class ScannerLineData {
    public float measurePerRotation;
    public Color color;
    public String targetTowers;

    public ScannerLineData() {
        measurePerRotation = 1;
        color = Color.white;
        targetTowers = "";
    }
}