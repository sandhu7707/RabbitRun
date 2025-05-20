using System;
using UnityEngine;

public class TerrainMeshGenerator
{
    public static float[,] GenerateHeightData(int length, int width, TerrainChunkValuesDTO terrainData, Action<TerrainChunkValuesDTO, int, int> updateTerrainData, float heightMultiplier, Vector2 offset, AnimationCurve heightBase, HeightMapParams heightMapParams){
        float[,] heightMap = new float[length, width];
        

        float minValue = 0;
        float maxValue = float.MinValue;

        for(int i0=0; i0<length; i0++){

                updateTerrainData(terrainData, i0, length);
                float scale = terrainData.scale;
                
                if(terrainData.scale <=0 ){
                    scale = 0.001f;
                }
                int octaves = terrainData.octaves;
                float persistance = terrainData.persistance;
                float fakePersistance = 1;
                float lacunarity = terrainData.lacunarity;

            for(int i1=0; i1<width; i1++){  

                float amplitude = 1;
                float fakeAmplitude = 1;
                float frequency = 1;
                float height = 0;
                maxValue = 0;
                
                for(int i=0; i<octaves; i++){
                    float sampleX = (i0+offset.x)/scale*frequency ;
                    float sampleY = (i1+offset.y)/scale*frequency ;

                    float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY);
                    height += perlinNoise * amplitude;

                    maxValue += fakeAmplitude;
                    amplitude *= persistance;
                    fakeAmplitude *= fakePersistance;
                    frequency *= lacunarity;
                }

                heightMap[i0,i1] = height;
            
            }
        }

        for(int i0=0; i0<length; i0++){
            for(int i1=0; i1<width; i1++){
                
                heightMap[i0,i1] = Mathf.InverseLerp(minValue, maxValue, heightMap[i0,i1])*heightBase.Evaluate(i1);
                heightMap[i0,i1] = heightMap[i0,i1]*heightMapParams.baseMultiplier*heightMultiplier;
            }
        }

        if(heightMapParams.heightCap != -1 ){
            if(heightMapParams.resetBaseMultiplier){
                heightMapParams.resetBaseMultiplier = false;
                heightMapParams.currentMultiplier = heightMapParams.baseMultiplier;
            }
            float targetHeightMultiplier = heightMapParams.heightCap/maxValue;
            for(int i0=0; i0<length; i0++){
                for(int i1=0; i1<width; i1++){
                    heightMap[i0,i1] = heightMap[i0,i1]*heightMapParams.currentMultiplier/heightMapParams.baseMultiplier;
                }
                if(Math.Abs(heightMapParams.currentMultiplier - targetHeightMultiplier) > 1.5*heightMapParams.changeInterval){
                    heightMapParams.currentMultiplier = Mathf.Max(heightMapParams.changeInterval, heightMapParams.currentMultiplier + ((targetHeightMultiplier-heightMapParams.currentMultiplier) > 0 ? heightMapParams.changeInterval : -heightMapParams.changeInterval));
                }
            }

            if(Math.Abs(heightMapParams.currentMultiplier - targetHeightMultiplier) <= 1.5*heightMapParams.changeInterval){
                heightMapParams.baseMultiplier = heightMapParams.currentMultiplier;
                heightMapParams.resetBaseMultiplier = true;
                heightMapParams.heightCap = -1;
                heightMapParams.currentMultiplier = 1;
            }
        }
    
        return heightMap;
    }

    public static MeshData GenerateMeshData(float[,] heightMap, float startZ=0, bool isColliderMesh=false){
        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);
        
        int factor = isColliderMesh ? 2 : 1;
        length = Mathf.CeilToInt(length / factor);
        width = Mathf.CeilToInt(width / factor);

        Vector3[] vertices = new Vector3[length*width];
        Vector2[] uv = new Vector2[length*width];
        int[] triangles = new int[(length-1)*(width-1)*6];
        int triangleIndex=0;

        for (int i0 = 0; i0 < length; i0++)
        {
            for (int i1 = 0; i1 < width; i1++)
            {
                vertices[i1 + i0 * width] = new Vector3(i0*factor, heightMap[i0, i1], i1*factor + startZ);
                uv[i1 + i0 * width] = new Vector2((float)i0*factor / length, (float)i1*factor / width);

                if (i0 != length - 1 && i1 != width - 1)
                {
                    triangles[triangleIndex++] = (i0) * width + (i1);
                    triangles[triangleIndex++] = (i0) * width + (i1 + 1);
                    triangles[triangleIndex++] = (i0 + 1) * width + (i1 + 1);

                    triangles[triangleIndex++] = (i0 + 1) * width + (i1 + 1);
                    triangles[triangleIndex++] = (i0 + 1) * width + (i1);
                    triangles[triangleIndex++] = (i0) * width + (i1);
                }
            }
        }    

        return new MeshData(vertices, uv, triangles);
    }



}

public class HeightMapParams{

    public float heightCap = -1;
    public float counterPos = -1;
    public bool resetBaseMultiplier = true;
    public float currentMultiplier = 1;
    public float baseMultiplier = 5;
    public float changeInterval = 0.005f;

}

public class MeshData {
    Vector3[] vertices;
    Vector2[] uv;
    int[] triangles;
    bool isColliderMesh;

    public MeshData(Vector3[] vertices, Vector2[] uv, int[] triangles)
    {
        this.vertices = vertices;
        this.uv = uv;
        this.triangles = triangles;
    }

    public Mesh CreateMesh(){
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        // if (isColliderMesh)
        // {
        //     mesh.Optimize();
        // }

        return mesh;
    }
}