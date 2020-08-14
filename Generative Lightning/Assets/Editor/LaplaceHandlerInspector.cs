using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LaplaceHandler))]
public class LaplaceHandlerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Initialise"))
        {
            Debug.Log("It's alive: " + target.name);
            LaplaceHandler handler = (LaplaceHandler)target;
            handler.Initiate();
        }
    }
}
