using System.Collections.Generic;
using UnityEngine;

public static class EnvironmentGenerator{


    public static EnvironmentInfo GenerateEnvironmentData(MeshGenerator.MeshData meshData, EnvironmentData environmentData, int lod){
        
        int width = meshData.heightMap.GetLength(0)-2*lod;
        int length = meshData.heightMap.GetLength(1)-2*lod;

        int vertexCountZ = (width-1)/lod + 1;
        int vertexCountX = (length-1)/lod + 1;
        
        return new EnvironmentInfo(environmentData, GetGrassPositions(environmentData, meshData, vertexCountX, vertexCountZ));
        
    }

    static Dictionary<Vector3, Vector3> GetGrassPositions(EnvironmentData environmentData, MeshGenerator.MeshData meshData, int vertexCountX, int vertexCountZ){
        int grassDensity = environmentData.grassDensity;
        Dictionary<Vector3, Vector3> grassPositions = new Dictionary<Vector3, Vector3>();

        for(int i0=0; i0<vertexCountX; i0+=grassDensity){
            int i2 = new System.Random().Next(0, vertexCountZ-1);
            MonoBehaviour.print("i0: " + i0 + ",i2: " + i2);
            if(i0>=0 && i2>=0 && i0<vertexCountX && i2<vertexCountZ){
                grassPositions.Add(meshData.GetPositionAt(i0, i2), meshData.GetPerpendicularFor(i0,i2));
            }
        }

        return grassPositions;
    }

    public class EnvironmentInfo{
        Dictionary<Vector3, Vector3> grassVertices;
        EnvironmentData environmentData;
        GameObject environment;

        public EnvironmentInfo( EnvironmentData environmentData, Dictionary<Vector3, Vector3> grassVertices){
            this.environmentData = environmentData;
            this.grassVertices = grassVertices;
        }

        public GameObject CreateEnvironment(Vector3 basePosition){
            environment = new GameObject("Environment");

            if(environmentData.generateGrass){
            foreach (var item in grassVertices)
                {
                    GameObject go = GameObject.Instantiate(environmentData.grassPrefab, basePosition + item.Key, new Quaternion(item.Value.x, item.Value.y, item.Value.z, 1));
                    go.transform.parent = environment.transform;
                }
            }

            return environment;
        }
    }
}