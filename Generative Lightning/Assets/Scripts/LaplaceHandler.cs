using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaplaceHandler : MonoBehaviour
{
    [SerializeField] Laplace laplace;

    public void Initiate()
    {
        laplace.InitiateLaplace();

        Debug.Log("and working!");
    }

}
