// using UnityEngine;
// using UnityEditor;

// [CustomEditor(typeof(ProceduralGeneration))]
// public class GenerateMeshEditor: Editor
// {
//     public override void OnInspectorGUI()
//     {
//         ProceduralGeneration proceduralGeneration = target as ProceduralGeneration;

//         if(DrawDefaultInspector()){
//             proceduralGeneration.AddAdvancedBlockEditor();
//         }
//         if(GUILayout.Button("Generate Block")){
//             proceduralGeneration.AddAdvancedBlockEditor();
//             // proceduralGeneration.AddTerrainBlockEditor();
//             // proceduralGeneration.AddTerrainBlock();
//             // proceduralGeneration.AddTerrainBlock();
//         }

//         // base.OnInspectorGUI();
//     }
// }
