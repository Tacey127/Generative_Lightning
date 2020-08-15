using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LaplaceHandler))]
public class LaplaceHandlerInspector : Editor
{
    LaplaceHandler handler;


    private void OnEnable()
    {
        handler = (LaplaceHandler)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Initialise"))
        {
            handler.Initiate();
        }

        if (GUILayout.Button("Iterate Laplace Once"))
        {
            handler.IterateLaplace();
        }

        if (GUILayout.Button("Run Laplace"))
        {
            handler.RunLaplace();
        }

    }
}
