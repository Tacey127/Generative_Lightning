using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Laplace : MonoBehaviour
{
    //**************************************************************************************Generation Type Classes
    [Serializable]
    public enum GenerationTypes
    {
        FullSphere,
        SemiSphere,
        BranchingRotation
    };

    //**************************************************************************************Editor Variables

    [SerializeField] NewChargePoint lightningNode = null;//the node
    [SerializeField] List<NewChargePoint> candidateCharges = new List<NewChargePoint>();

    [Tooltip("The physical length of a grid cell")]
    [SerializeField] int maxNumberspawned = 10;
    [SerializeField] float h = 1;//the physical length of a grid cell
    [Space]

    //private, following this https://answers.unity.com/questions/1365178/serialization-of-an-enum-array.html
    [Header("Lightning Generation Varients")]
    [SerializeField] private GenerationTypes genType = GenerationTypes.SemiSphere;

    //colision and spawning
    [SerializeField]List<Vector3> collisionPositions = new List<Vector3>();
    List<NewChargePoint> spawnedCharges = new List<NewChargePoint>();

    //the boundary constants
    float ROne = -1;

    void RunLaplace()
    {
        int numberSpawned = 0;

        //Step 1
        //weigh all nodes, then randomly select one according to a weighting
        //http://gamma.cs.unc.edu/FRAC/laplacian_large.pdf
        //Step 2
        //Add a new point charge at the growth site.
        //Step 3
        //Update the potential at all the candidate sites accordingto Eqn. 11.
        //Step 4 -> 5
        //Add the new candidate sites surrounding the growth site.
        //Calculate the potential at new candidate sites  Eqn. 10
        if (numberSpawned < maxNumberspawned)
        {
            NewChargePoint chosenNode = StepOne();
            StepTwo(chosenNode);
            StepThree(chosenNode);
            numberSpawned++;
            Debug.Log(chosenNode.transform.localPosition);
            StepFour(chosenNode);
        }

    }

    void InitiateLaplace()
    {

        //calculate boundary constants
        ROne = h / 2;

        //spawn initial charge
        collisionPositions.Add(new Vector3(0, 0, 0));
        NewChargePoint startingCharge = new NewChargePoint();


        startingCharge.potential = 26;//according to the reference material, 26 for the spawned charge, -1 for the other charges
        //changed to 17 to prevent upwards lightning

        spawnedCharges.Add(startingCharge);
        //add surrounding candidate charges
        SpawnStartingOctantPreset(startingCharge);


    }
    void SpawnStartingOctantPreset(NewChargePoint spawnCharge)
    {
        //get the recent spawned charge position
        Vector3 newPointPosition = new Vector3(0, 0, 0);

        //add charges based on a 3x3x3 octant
        List<NewChargePoint> newCharges = new List<NewChargePoint>();

        int jNum = 3;
        if (genType == GenerationTypes.SemiSphere)
        {
            jNum = 2;
        }

        for (int i = 0; i < 3; i++)//x
        {
            for (int j = 0; j < jNum; j++)//y
            {
                for (int k = 0; k < 3; k++)//z
                {
                    Vector3 checkPos = newPointPosition + new Vector3(i - 1, j - 1, k - 1);
                    if (newPointPosition != checkPos)
                    {
                        Vector3 pos = checkPos + gameObject.transform.position;
                        NewChargePoint newCharge = Instantiate(lightningNode, pos, Quaternion.identity, transform).GetComponent<NewChargePoint>();
                        newCharge.chargePointRelativePosition = checkPos;
                        collisionPositions.Add(new Vector3(i - 1, j - 1, k - 1));
                        newCharges.Add(newCharge);
                        newCharge.potential = -1;
                        newCharge.parentCharge = spawnCharge;
                    }
                }
            }
        }
        StepFive(newCharges);
    }

    //Step 1 - normalise and weigh all nodes, then randomly select one according to a weighting
    //http://gamma.cs.unc.edu/FRAC/laplacian_large.pdf
    NewChargePoint StepOne()
    {
        //Weigh the nodes
        float minCharge = 2.0f;
        float maxCharge = -1.0f;

        foreach (NewChargePoint charge in candidateCharges)
        {
            if (charge.potential < minCharge)
                minCharge = charge.potential;

            if (charge.potential > maxCharge)
                maxCharge = charge.potential;
        }

        float minMax = maxCharge - minCharge;

        foreach (NewChargePoint charge in candidateCharges)
        {
            charge.weightedChance = (charge.potential - minCharge) / minMax;
        }

        //Randomly select the weighted node
        float culmativeWeight = 0;
        foreach (NewChargePoint charge in candidateCharges)
        {
            culmativeWeight += charge.weightedChance;
        }

        float selection = culmativeWeight * UnityEngine.Random.value;

        foreach (NewChargePoint charge in candidateCharges)
        {
            selection -= charge.weightedChance;
            if (selection <= 0)
            {
                return charge;
            }
        }

        return candidateCharges[0];
    }

    //add the selected node to the spawned category
    NewChargePoint StepTwo(NewChargePoint newNode)
    {
        candidateCharges.Remove(newNode);

        newNode.parentCharge.childSpawnedCharges.Add(newNode);

        //change the node to completed node
        spawnedCharges.Add(newNode);

        //visualisation


        return newNode;
    }

    //Update the potential at all the candidate sites accordingto Eqn. 11.
    void StepThree(NewChargePoint recentCharge)
    {
        foreach (NewChargePoint i in candidateCharges)
        {
            i.potential = i.potential + (1 - (ROne / (recentCharge.potential)));///r = the potential from the point charge at step one
        }
    }

    //Step 4 - Add the new candidate sites surrounding the growth site.
    void StepFour(NewChargePoint chosenNode)
    {
        //get the recent spawned charge position
        Vector3 newPointPosition = chosenNode.transform.localPosition;
        Debug.Log(newPointPosition);

        //add charges based on a 3x3x3 octant
        List<NewChargePoint> newCharges = new List<NewChargePoint>();

        int jNum = 3;
        if (genType == GenerationTypes.SemiSphere)
        {
            jNum = 2;
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < jNum; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    Vector3 checkPos = newPointPosition + new Vector3(i - 1, j - 1, k - 1);
                    if (!collisionPositions.Contains(checkPos))
                    {
                        Vector3 pos = checkPos + gameObject.transform.position;
                        NewChargePoint newCharge = new NewChargePoint();
                        newCharge.chargePointRelativePosition = checkPos;
                        collisionPositions.Add(new Vector3(i - 1, j - 1, k - 1) + newPointPosition);
                        newCharges.Add(newCharge);
                        newCharge.parentCharge = chosenNode;
                    }
                    else
                    {
                        Debug.Log("NodeOverlap");
                    }
                }
            }
        }


        StepFive(newCharges);
    }

    //Step 5 - Calculate the potential at new candidate sites  Eqn. 10
    void StepFive(List<NewChargePoint> newCharges)
    {
        foreach (NewChargePoint i in newCharges)
        {

            float n = 0;
            foreach (NewChargePoint j in spawnedCharges)//all other spawned charges?
            {
                float rij = Vector3.Distance(i.chargePointRelativePosition, j.chargePointRelativePosition);
                n += (1 - (ROne / rij));
            }
            i.potential = n;
        }

        //add new candidate sites to the candidate list


        candidateCharges.AddRange(newCharges);
    }
}
