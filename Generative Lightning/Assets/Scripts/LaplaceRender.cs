using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaplaceRender : MonoBehaviour
{
    //gets the node
    //gets the node path
    //renders node path
    List<NewChargePoint> renderPath;
    [SerializeField]LineRenderer lineRenderer;

    public void RenderFromNode(NewChargePoint givenNode)
    {
        renderPath = GetLightningPathFromNode(givenNode);

        DrawFromList();
    }

    List<NewChargePoint> GetLightningPathFromNode(NewChargePoint aNode)
    {
        List<NewChargePoint> lightningList = new List<NewChargePoint>();
        lightningList.Add(aNode);

        NewChargePoint currentNode;
        currentNode = aNode;
        while (currentNode.parentCharge != null)
        {
            currentNode = currentNode.parentCharge;
            lightningList.Add(currentNode);
        }
        return lightningList;
    }

    void DrawFromList()
    {
        //renderedBranchNumber
        lineRenderer.positionCount = 0;
        int x = 0;
        foreach (NewChargePoint p in renderPath)
        {
            if (p.parentCharge != null)
            {
                lineRenderer.positionCount += 1;
                lineRenderer.SetPosition(x, p.chargePointRelativePosition + transform.position);
                x++;
                //Debug.DrawLine(p.transform.position, p.parentCharge.transform.position, Color.black, 999.0f);
            }
        }
        lineRenderer.enabled = true;
    }
}
