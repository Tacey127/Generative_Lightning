using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaplaceHandler : MonoBehaviour
{
    [SerializeField] Laplace laplace;
    bool initialised = false;

    public void Initiate()
    {
        if(!initialised)
        {
            laplace.InitiateLaplace();
            initialised = true;
        }
        else
        {
            Debug.Log("InitiateLaplace already called");
        }
    }

    public void IterateLaplace()
    {
        if(initialised)
        {
            laplace.RunLaplace();
        }
        else
        {
            Debug.Log("InitiateLaplace required for Laplace to function properly");
        }
    }

    public void RunLaplace()
    {
        if (initialised)
        {
            while(!laplace.RunLaplace());
            Debug.Log("finished");
            laplace.RenderLaplace();
        }
        else
        {
            Debug.Log("InitiateLaplace required for Laplace to function properly");
        }
    }

}
