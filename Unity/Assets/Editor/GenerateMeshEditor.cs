using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralGeneration))]
public class GenerateMeshEditor: Editor
{
    public override void OnInspectorGUI()
    {
        ProceduralGeneration proceduralGeneration = target as ProceduralGeneration;
        if(GUILayout.Button("Generate Block")){
            proceduralGeneration.CreateTerrainBlock();
        }

        base.OnInspectorGUI();
    }
}
