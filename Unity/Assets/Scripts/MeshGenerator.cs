using System;
using UnityEngine;

public class HeightMapParams{

    public float heightCap = -1;
    public float counterPos = -1;
    public bool resetBaseMultiplier = true;
    public float currentMultiplier = 1;
    public float baseMultiplier = 1;
    public float changeInterval = 0.005f;
}

public class MeshGenerator{
    public static float[,] GenerateHeightData(int length, int width, float scale, float persistance, float lacunarity, int octaves, Vector2 offset, AnimationCurve heightBase, HeightMapParams heightMapParams){
        float[,] heightMap = new float[length, width];

        if(scale <=0 ){
            scale = 0.001f;
        }

        float minValue = 0;
        float maxValue = float.MinValue;

        for(int i0=0; i0<length; i0++){
            for(int i1=0; i1<width; i1++){  

                float amplitude = 1;
                float frequency = 1;
                float height = 0;
                maxValue = 0;
                
                for(int i=0; i<octaves; i++){
                    float sampleX = (i0+offset.x)/scale*frequency ;
                    float sampleY = (i1+offset.y)/scale*frequency ;

                    float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY);
                    height += perlinNoise * amplitude;

                    maxValue += amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                heightMap[i0,i1] = height;
            
            }
        }

        float newMaxValue = float.MinValue;
        for(int i0=0; i0<length; i0++){
            for(int i1=0; i1<width; i1++){
                
                heightMap[i0,i1] = Mathf.InverseLerp(minValue, maxValue, heightMap[i0,i1])*heightBase.Evaluate(i1);
                if(heightMap[i0,i1] > newMaxValue){
                    newMaxValue = heightMap[i0, i1];
                }
                heightMap[i0,i1] = heightMap[i0,i1]*heightMapParams.baseMultiplier;
            }
        }

        if(heightMapParams.heightCap != -1 ){
            if(heightMapParams.resetBaseMultiplier){
                heightMapParams.resetBaseMultiplier = false;
                heightMapParams.currentMultiplier = heightMapParams.baseMultiplier;
            }
            float targetHeightMultiplier = heightMapParams.heightCap/newMaxValue;
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

    public static float[,] GenerateHeightMap(int length, int width, float scale, float heightMultiplier, Vector2 offset, AnimationCurve heightBase){
        float[,] heightMap = new float[length, width];

        if(scale <=0 ){
            scale = 0.001f;
        }

        for(int i0=0; i0<length; i0++){
            for(int i1=0; i1<width; i1++){  
                
                float sampleX = (i0+offset.x)/scale;
                float sampleY = (i1+offset.y)/scale;

                float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY);

                heightMap[i0,i1] = perlinNoise*heightMultiplier + heightBase.Evaluate(i1);
            }
        }
    
        return heightMap;
    }

    public static MeshData GenerateMeshData(float[,] heightMap){
        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);
        
        Vector3[] vertices = new Vector3[length*width];
        Vector2[] uv = new Vector2[length*width];
        int[] triangles = new int[(length-1)*(width-1)*6];
        int triangleIndex=0;

        for(int i0=0; i0<length; i0++){
            for(int i1=0; i1<width; i1++){
                vertices[i1+i0*width] = new Vector3(i0, heightMap[i0,i1], i1);
                uv[i1+i0*width] = new Vector2(i0/length, i1/width);

                if(i0 != length-1 && i1!=width-1){
                    triangles[triangleIndex++] = (i0)*width+(i1);
                    triangles[triangleIndex++] = (i0)*width+(i1+1);
                    triangles[triangleIndex++] = (i0+1)*width+(i1+1);

                    triangles[triangleIndex++] = (i0+1)*width+(i1+1);
                    triangles[triangleIndex++] = (i0+1)*width+(i1);
                    triangles[triangleIndex++] = (i0)*width+(i1);
                }
            }
        }    

        return new MeshData(vertices, uv, triangles);
    }
}