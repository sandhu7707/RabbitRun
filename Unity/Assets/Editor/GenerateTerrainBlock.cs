using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class GenerateTerrainBlock: Editor
{
     public override void OnInspectorGUI()
    {
        TerrainGenerator terrainBlock = target as TerrainGenerator;

        if(DrawDefaultInspector()){
            // terrainBlock.GenerateTerrainBlock();
        }
        if(GUILayout.Button("Generate Block")){
            terrainBlock.GenerateTerrainBlockEditor();
            // terrainBlock.GenerateTerrainBlock();
            // terrainBlock.GenerateTerrainBlock();
            terrainBlock.pathTerrainData.valuesUpdated.AddListener(terrainBlock.GenerateTerrainBlockEditor);
            terrainBlock.leftSlopeData.valuesUpdated.AddListener(terrainBlock.GenerateTerrainBlockEditor);
            terrainBlock.rightSlopeData.valuesUpdated.AddListener(terrainBlock.GenerateTerrainBlockEditor);
        }

    }
}
