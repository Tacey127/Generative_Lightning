using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Lightning : MonoBehaviour
{
    //**************************************************************************************Generation Type Classes
    [Serializable]
    public enum GenerationTypes
    {
        FullSphere,
        SemiSphere,
        BranchingRotation
    };

    [Serializable]
    public enum DebugNodeTypes
    {
        ShowAll,
        ShowSpawned,
        ShowStruck,
        None
    };

    [Serializable]
    public enum DrawnLineTypes
    {
        DrawAll,
        DrawSpawned,
        DrawStruck,
        None
    };

    //**************************************************************************************Editor Variables

    [SerializeField] GameObject lightningNode = null;//the node
    [SerializeField] List<ChargePoint> candidateCharges = new List<ChargePoint>();
    [SerializeField] List<ChargePoint> struckLightningPath = new List<ChargePoint>();
    [Space]

    [Tooltip("The size of the 'Grid'")]
    [SerializeField] float N = 5;//the size of the "Grid"
    [Tooltip("The physical length of a grid cell")]
    [SerializeField] float h = 1;//the physical length of a grid cell
    [Space]

    [Tooltip("How many nodes maximum do we want to spawn from the lightning before it autofails")]
    [SerializeField] int maxNumberspawned = 100;//how many nodes maximum do we want to spawn from the lightning before it autofails

    [Tooltip("How many nodes away from the struck path are rendered")]
    [SerializeField] int renderedBranchDepth = 0;

    [Tooltip("How many additional branches from each node are rendered")]
    [SerializeField] int renderedBranchesPerNode = 0;


    [Header("Lightning Node Visualisation")]
    [SerializeField] private DebugNodeTypes debugNodeType = DebugNodeTypes.None;

    [Header("Debug Lightning Draw Visualisation")]
    [SerializeField] private DrawnLineTypes debugDrawnLineType = DrawnLineTypes.None;

    [Header("Lightning Draw Visualisation")]
    [SerializeField] private DrawnLineTypes drawnLineType = DrawnLineTypes.None;

    //private, following this https://answers.unity.com/questions/1365178/serialization-of-an-enum-array.html
    [Header("Lightning Generation Varients")]
    [SerializeField] private GenerationTypes genType = GenerationTypes.SemiSphere;

    //colision and spawning
    List<Vector3> collisionPositions = new List<Vector3>();
    List<ChargePoint> spawnedCharges = new List<ChargePoint>();
    int numberspawned = 0;
    bool stricken = false;

    //the boundary constants
    float ROne = 1;
    float RTwo = 1;

    //Rendering
    LineRenderer lineRenderer;

    void Start()
    {
        //get the renderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;


        //calculate boundary constants
        ROne = h / 2;
        RTwo = (N * h) / 2;

        //spawn initial charge
        collisionPositions.Add(new Vector3(0, 0, 0));
        ChargePoint startingCharge = Instantiate(lightningNode, transform).GetComponent<ChargePoint>();


        startingCharge.potential = 26;//according to the reference material, 26 for the spawned charge, -1 for the other charges
        //changed to 17 to prevent upwards lightning

        if ((debugNodeType == DebugNodeTypes.ShowSpawned) || (debugNodeType == DebugNodeTypes.ShowAll))
        {
            startingCharge.mRenderer.enabled = true;
        }

        spawnedCharges.Add(startingCharge);
        //add surrounding candidate charges
        SpawnStartingOctantPreset(startingCharge);


    }
    void SpawnStartingOctantPreset(ChargePoint spawnCharge)
    {
        //get the recent spawned charge position
        Vector3 newPointPosition = new Vector3(0, 0, 0);

        //add charges based on a 3x3x3 octant
        List<ChargePoint> newCharges = new List<ChargePoint>();

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
                        ChargePoint newCharge = Instantiate(lightningNode, pos, Quaternion.identity, transform).GetComponent<ChargePoint>();
                        newCharge.chargePointRelativePosition = checkPos;
                        collisionPositions.Add(new Vector3(i - 1, j - 1, k - 1));
                        newCharges.Add(newCharge);
                        newCharge.potential = -1;
                        newCharge.parentCharge = spawnCharge;

                        if (genType == GenerationTypes.BranchingRotation)
                        {

                        }

                        //visualisation
                        if (debugDrawnLineType == DrawnLineTypes.DrawAll)
                        {
                            Debug.DrawLine(gameObject.transform.position, pos, Color.black, 999.0f);
                        }

                        if (debugNodeType == DebugNodeTypes.ShowAll)
                        {
                            newCharge.mRenderer.enabled = true;
                        }
                    }
                }
            }
        }
        StepFive(newCharges);
    }

    void FixedUpdate()
    {
        if (!stricken)
            stricken = runLightning();
    }

    bool runLightning()
    {
        if (spawnedCharges.Count != 0)
        {

            if (!spawnedCharges[spawnedCharges.Count - 1].hasStruck)
            {
                Laplace();
                return false;
            }
            else
            {
                Debug.Log("GOLD");
                //has struck and hit node are caused by 
                struckLightningPath = getLightningPathFromNode(spawnedCharges[spawnedCharges.Count - 1]);
                if (debugNodeType == DebugNodeTypes.ShowStruck)
                {
                    ShowLightningNodesInList();
                }
                if (debugDrawnLineType == DrawnLineTypes.DrawStruck)
                {
                    DrawDebugLightningLinesInList();
                }
                if (drawnLineType == DrawnLineTypes.DrawStruck)
                {
                    DrawLightningLines();
                }

                return true;
            }
        }
        else
        {
            return true;
        }
    }


    void Laplace()
    {
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
        if (numberspawned < maxNumberspawned)
        {
            Debug.Log(candidateCharges.Count);

            ChargePoint chosenNode = StepOne();
            StepTwo(chosenNode);
            StepThree(chosenNode);
            numberspawned++;
            Debug.Log(chosenNode.transform.localPosition);
            StepFour(chosenNode);
        }

    }

    //Step 1 - normalise and weigh all nodes, then randomly select one according to a weighting
    //http://gamma.cs.unc.edu/FRAC/laplacian_large.pdf
    ChargePoint StepOne()
    {
        //Weigh the nodes
        float minCharge = 2.0f;
        float maxCharge = -1.0f;

        foreach (ChargePoint charge in candidateCharges)
        {
            if (charge.potential < minCharge)
                minCharge = charge.potential;

            if (charge.potential > maxCharge)
                maxCharge = charge.potential;
        }

        float minMax = maxCharge - minCharge;

        foreach (ChargePoint charge in candidateCharges)
        {
            charge.weightedChance = (charge.potential - minCharge) / minMax;
        }

        //Randomly select the weighted node
        float culmativeWeight = 0;
        foreach (ChargePoint charge in candidateCharges)
        {
            culmativeWeight += charge.weightedChance;
        }

        float selection = culmativeWeight * UnityEngine.Random.value;

        foreach (ChargePoint charge in candidateCharges)
        {
            selection -= charge.weightedChance;
            if (selection <= 0)
            {
                return charge;
            }
        }

        Debug.Log("FAIL");
        return candidateCharges[0];
    }

    //add the selected node to the spawned category
    ChargePoint StepTwo(ChargePoint newNode)
    {
        candidateCharges.Remove(newNode);

        newNode.parentCharge.childSpawnedCharges.Add(newNode);

        //change the node to completed node
        newNode.mRenderer.material.color = Color.yellow;
        newNode.transform.localScale = Vector3.one;
        spawnedCharges.Add(newNode);

        //visualisation
        if (debugDrawnLineType == DrawnLineTypes.DrawSpawned)
        {
            Debug.DrawLine(newNode.transform.position, newNode.parentCharge.transform.position, Color.black, 999.0f);
        }

        if (debugNodeType == DebugNodeTypes.ShowSpawned)
        {
            newNode.mRenderer.enabled = true;
        }

        return newNode;
    }

    //Update the potential at all the candidate sites accordingto Eqn. 11.
    void StepThree(ChargePoint recentCharge)
    {
        foreach (ChargePoint i in candidateCharges)
        {
            i.potential = i.potential + (1 - (ROne / (recentCharge.potential)));///r = the potential from the point charge at step one
        }
    }

    //Step 4 - Add the new candidate sites surrounding the growth site.
    void StepFour(ChargePoint chosenNode)
    {
        //get the recent spawned charge position
        Vector3 newPointPosition = chosenNode.transform.localPosition;
        Debug.Log(newPointPosition);

        //add charges based on a 3x3x3 octant
        List<ChargePoint> newCharges = new List<ChargePoint>();

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
                        ChargePoint newCharge = Instantiate(lightningNode, pos, Quaternion.identity, transform).GetComponent<ChargePoint>();
                        newCharge.chargePointRelativePosition = checkPos;
                        collisionPositions.Add(new Vector3(i - 1, j - 1, k - 1) + newPointPosition);
                        newCharges.Add(newCharge);
                        newCharge.parentCharge = chosenNode;

                        //visualisation
                        if (debugDrawnLineType == DrawnLineTypes.DrawAll)
                        {
                            Debug.DrawLine(chosenNode.transform.position, pos, Color.black, 999.0f);
                        }
                        if (debugNodeType == DebugNodeTypes.ShowAll)
                        {
                            newCharge.mRenderer.enabled = true;
                        }
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
    void StepFive(List<ChargePoint> newCharges)
    {
        foreach (ChargePoint i in newCharges)
        {

            float n = 0;
            foreach (ChargePoint j in spawnedCharges)//all other spawned charges?
            {
                float rij = Vector3.Distance(i.chargePointRelativePosition, j.chargePointRelativePosition);
                n += (1 - (ROne / rij));
            }
            i.potential = n;
        }

        //add new candidate sites to the candidate list


        candidateCharges.AddRange(newCharges);
    }

    //returns a path from a selected node to the first lightning spawned
    List<ChargePoint> getLightningPathFromNode(ChargePoint aNode)
    {
        List<ChargePoint> lightningList = new List<ChargePoint>();
        lightningList.Add(aNode);

        ChargePoint currentNode;
        currentNode = aNode;
        while (currentNode.parentCharge != null)
        {
            currentNode = currentNode.parentCharge;
            lightningList.Add(currentNode);
        }
        return lightningList;
    }

    void ShowLightningNodesInList()
    {
        foreach (ChargePoint p in struckLightningPath)
        {
            p.mRenderer.enabled = true;
        }
    }

    void DrawDebugLightningLinesInList()
    {
        foreach (ChargePoint p in struckLightningPath)
        {
            if (p.parentCharge != null)
            {
                Debug.DrawLine(p.transform.position, p.parentCharge.transform.position, Color.black, 999.0f);
            }
        }
    }

    void DrawLightningLines()
    {
        //renderedBranchNumber
        lineRenderer.positionCount = 0;
        int x = 0;
        foreach (ChargePoint p in struckLightningPath)
        {
            if (p.parentCharge != null)
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(x, p.transform.localPosition);
                x++;
                //Debug.DrawLine(p.transform.position, p.parentCharge.transform.position, Color.black, 999.0f);
                if ((renderedBranchDepth > 0) && (renderedBranchesPerNode > 0))
                {
                    DrawBranches(p, renderedBranchDepth);
                }
            }
        }
        lineRenderer.enabled = true;
    }

    void DrawBranches(ChargePoint p, int branchDepth)
    {
        int renderNode = renderedBranchesPerNode;
        foreach (ChargePoint ps in p.childSpawnedCharges)
        {
            if (renderNode > 0)
            {
                renderNode--;
                Debug.DrawLine(p.transform.position, ps.transform.position, Color.black, 999.0f);
                if (branchDepth > 0)
                {
                    branchDepth--;
                    DrawBranches(p, branchDepth);
                }
            }
        }

    }


    /*
     * 
     * An iteration of the algorithm is as follows:
     * 1)  Randomly select a growth site according to Eqn. 12.
     * 2)  Add a new point charge at the growth site.
     * 3)  Update the potential at all the candidate sites accordingto Eqn. 11.
     * 4)  Add the new candidate sites surrounding the growth site.
     * 5)  Calculate the potential at new candidate sites using Eqn.10.

 /*
     *  
     *  With an NxNxN grid
     *  where N is the grid size?
     *  where h is the length of a grid cell
     *  
     *  
     *  
     *  located at the centre of the grid:
     *  R1 = h/2
     *  
     *  surrounded by large positively charged ring
     *  R2 = Nh/2
     *  
     *  3D Greens function:  
     *  φ = c1+(c2/r)
     *  
     *  c1 = -((R1/R2) - 1)^-1
     *  c2 = ((1/R2) - (1/R1))^-1
     *  
     *  r = the distance from the center node?
     *  
     *  As R2 approaches Infinity:
     *  φ = 1 - (R1 / r)
     *  
     *  
     *  where φt is the current potential, φt+1 is the calculated potential for the next timestep?
     *  
     *  
     *  
     *  EQN 11
     *  φt+1=φti+ (1−(R1/r,t+1)
     *  
     *  potential is ∇2φ= 0,
     *  
    */

    // about guiding the negative charge, 0 to the positive charge, 1
}
