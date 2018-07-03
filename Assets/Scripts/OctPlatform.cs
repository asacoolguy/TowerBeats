using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for any Octagon platforms that can be highlighted and selected
// extended by CentralPlatform and TowerPlatform

public class OctPlatform : MonoBehaviour {
    protected enum PlatformStatus { lowering, raising, lowered, raised };

    // variables for highlighted
    protected PlatformStatus status;
    public float raisedYPos;
    protected float loweredYPos;
    public float moveTime;
    protected float currentMoveTime;
    protected bool selected;

    protected void Awake() {
        loweredYPos = transform.localPosition.y;
        currentMoveTime = 0f;
        status = PlatformStatus.lowered;
        selected = false;
    }



    protected void Update () {
        // raise or lower the tower
        if (status == PlatformStatus.lowering) {
            currentMoveTime -= Time.deltaTime;
            if (currentMoveTime < 0) {
                currentMoveTime = 0;
                status = PlatformStatus.lowered;
            }
        }
        else if (status == PlatformStatus.raising) {
            currentMoveTime += Time.deltaTime;
            if (currentMoveTime > moveTime) {
                currentMoveTime = moveTime;
                if (status == PlatformStatus.raising) {
                    status = PlatformStatus.raised;
                }
            }
        }

        float t = currentMoveTime / moveTime;
        float yPos = GameManager.SmoothStep(loweredYPos, raisedYPos, t);
        transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);
    }


    public virtual void RaisePlatform() {
        status = PlatformStatus.raising;
    }

    public virtual void LowerPlatform() {
        if (!selected) {
            status = PlatformStatus.lowering;
        }
    }

    public void SelectPlatform(bool b) {
        if (b) {
            selected = true;
            RaisePlatform();
        }
        else {
            selected = false;
            LowerPlatform();
        }
    }
}
