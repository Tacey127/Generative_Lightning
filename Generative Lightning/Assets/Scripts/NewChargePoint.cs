using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewChargePoint : ScriptableObject
{

    // Lightning Generation
    [SerializeField] public float potential = -1f;//t
    public float newPotential = 0f;//t+1
    public float weightedChance = 0;
    public float r;//distance from charge center
    //
    [SerializeField] public bool hasStruck = false;



    public NewChargePoint parentCharge;
    public List<NewChargePoint> childSpawnedCharges = new List<NewChargePoint>();

    public Vector3 chargePointRelativePosition;
}
