
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestMeshGenerator))]
public class TestMeshGeneratorEditor: Editor{
    TestMeshGenerator testMeshGenerator;
    public override void OnInspectorGUI(){

        testMeshGenerator = target as TestMeshGenerator;

        if(DrawDefaultInspector()){
            Generate();
        }

        if(GUILayout.Button("Generate")){
                Generate();
        }


    }

    void Generate(){
        for(int i0=0; i0<testMeshGenerator.transform.childCount; i0++){
            DestroyImmediate(testMeshGenerator.transform.GetChild(0).gameObject);
        }
        testMeshGenerator.GenerateBlocks();
    }
}