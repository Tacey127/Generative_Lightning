using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewChargePoint : ScriptableObject
{

    // Lightning Generation
    public float potential = -1f;//t
    public float weightedChance = 0;
    public float r;//distance from charge center
    //
    public NewChargePoint parentCharge;
    public List<NewChargePoint> childSpawnedCharges = new List<NewChargePoint>();

    public Vector3 chargePointRelativePosition;
}
