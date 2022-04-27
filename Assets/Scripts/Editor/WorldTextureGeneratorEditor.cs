using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldTextureGenerator))]
public class WorldTextureGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldTextureGenerator worldTextureGenerator = (WorldTextureGenerator)target;

        if (GUILayout.Button("Generate World Texture"))
        {
            worldTextureGenerator.Generate();
        }
    }
}
