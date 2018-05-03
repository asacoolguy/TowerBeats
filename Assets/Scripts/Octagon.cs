using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for any Octagon platforms that can be highlighted and selected
// extended by CentralOctagon and BuildableOctagon

public class Octagon : MonoBehaviour {
    protected enum OctagonStatus { lowering, raising, lowered, raised };

    // variables for highlighted
    protected OctagonStatus status;
    public float raisedYPos;
    protected float loweredYPos;
    public float moveTime;
    protected float currentMoveTime;
    protected bool selected;

    protected void Awake() {
        loweredYPos = transform.localPosition.y;
        currentMoveTime = 0f;
        status = OctagonStatus.lowered;
        selected = false;
    }



    protected void Update () {
        // raise or lower the tower
        if (status == OctagonStatus.lowering) {
            currentMoveTime -= Time.deltaTime;
            if (currentMoveTime < 0) {
                currentMoveTime = 0;
                status = OctagonStatus.lowered;
            }
        }
        else if (status == OctagonStatus.raising) {
            currentMoveTime += Time.deltaTime;
            if (currentMoveTime > moveTime) {
                currentMoveTime = moveTime;
                if (status == OctagonStatus.raising) {
                    status = OctagonStatus.raised;
                }
            }
        }

        float t = currentMoveTime / moveTime;
        float yPos = GameManager.SmoothStep(loweredYPos, raisedYPos, t);
        transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);
    }


    public virtual void RaiseOctagon() {
        status = OctagonStatus.raising;
    }

    public virtual void LowerOctagon() {
        if (!selected) {
            status = OctagonStatus.lowering;
        }
    }

    public void SelectOctagon(bool b) {
        if (b) {
            selected = true;
            RaiseOctagon();
        }
        else {
            selected = false;
            LowerOctagon();
        }
    }
}
