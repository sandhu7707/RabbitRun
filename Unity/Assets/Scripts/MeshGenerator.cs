using UnityEngine;
public class MeshGenerator{

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

                heightMap[i0,i1] = perlinNoise*heightMultiplier+heightBase.Evaluate(i1);
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