using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargePoint : MonoBehaviour
{
    // Lightning Generation
    [SerializeField] public float potential = 0f;//t
    public float newPotential = 0f;//t+1
    public float weightedChance = 0;
    public float r;//distance from charge center


    //
    [SerializeField] public bool hasStruck = false;
    [SerializeField] public MeshRenderer mRenderer;
    public ChargePoint parentCharge;
    public List<ChargePoint> childSpawnedCharges = new List<ChargePoint>();

    //collision
    public Vector3 chargePointRelativePosition;

    public void Calculatedistance(Vector3 center)
    {
       r = Vector3.Distance(center, transform.localPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            hasStruck = true;
        }
    }



}
