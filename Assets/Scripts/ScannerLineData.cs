using System;
using UnityEngine;

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
