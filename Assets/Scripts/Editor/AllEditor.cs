using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Generator))]
public class GeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Generator g = (Generator)target;

        if (GUILayout.Button("Save"))
        {
            g.SaveList();
        }

        if (GUILayout.Button("Load"))
        {
            g.LoadList();
        }

        if (GUILayout.Button("Transform Dialogue Container To List"))
        {
            g.TransformDialogueContainerToList();
        }

        if (GUILayout.Button("Generate"))
        {
            Debug.ClearDeveloperConsole();
            g.Generate();
        }

        if (GUILayout.Button("Generate Json"))
        {
            Debug.ClearDeveloperConsole();
            g.GenerateJson();
        }
    }
}

