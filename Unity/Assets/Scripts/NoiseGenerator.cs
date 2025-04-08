using UnityEngine;

public static class NoiseGenerator
{
    public static float[,] GenerateNoise(int width, int height, float scale, float persistence, int octaves, Vector2 offset, int seed, AnimationCurve animationCurve, float lacunarity, float heightMultiplier){

        float[,] noiseMap = new float[width, height];

        if(scale <= 0){
            scale = 0.001f;
        }

        float maxHeight=0;
        float amplitude = 1;
        float frequency = 1;
        Vector2[] octaveOffsets = new Vector2[octaves];

        for(int i0=0; i0<octaves; i0++){
            float offsetX = new System.Random(seed).Next(-10000, 10000);
            float offsetY = new System.Random(seed).Next(-10000, 10000);

            octaveOffsets[i0] = new Vector2(offsetX, offsetY);
        }

        for(int i1=0; i1<height; i1++){
            for(int i0=0; i0<width; i0++){
                
                float noiseHeight = 0;
                amplitude = 1;
                frequency = 1;
                maxHeight = 0;

                for(int i3=0; i3<octaves; i3++){
                    float sampleX = frequency*(i0 + offset.x + octaveOffsets[i3].x)/scale;
                    float sampleY = frequency*(i1 + offset.y + octaveOffsets[i3].y)/scale;

                    float perlinNoise = Mathf.PerlinNoise(sampleX, sampleY);

                    noiseHeight += perlinNoise*amplitude;

                    maxHeight+=amplitude;
                    amplitude*=persistence;
                    frequency*=lacunarity;
                }

                noiseMap[i0, i1] = noiseHeight;

            }
        }

        for(int i0=0; i0<width; i0++){
            for(int i1=0; i1<height; i1++){
                noiseMap[i0,i1] = animationCurve.Evaluate(i0)+heightMultiplier*Mathf.InverseLerp(0, maxHeight, noiseMap[i0,i1]);
            }
        }

        MonoBehaviour.print("NoiseGenerator: offset:" + offset);
        MonoBehaviour.print("NoiseGenerator: maxHeight:" + maxHeight);

        return noiseMap;
    }

    public static Texture2D GenerateTexture(float[,] noiseMap){
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorMap = new Color[width*height];

        for(int i1=0; i1<height; i1++){
            for(int i0=0; i0<width; i0++){

                colorMap[i0 + i1*width] = Color.Lerp(Color.black, Color.white, noiseMap[i0, i1]);
            }
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

}
