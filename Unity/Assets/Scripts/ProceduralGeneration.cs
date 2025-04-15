using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ProceduralGeneration : MonoBehaviour
{

    public GameObject player;
    public int length;
    public int width;
    public int createNewDistanceThreshold;
    GameObject lastTerrainBlock;
    Queue<GameObject> terrainBlocks = new Queue<GameObject>();
    public int maxTerrainBlocks;
    [Range(1, 6)]
    public int obstaclesDensity;
    [Min(1)]
    public float powerUpsDensityFactor;
    public int minObstacleGap;
    public GameObject jumpableObstaclePrefab;
    public GameObject powerUpPrefab;
    public GameObject avoidableObstaclePrefab;
    public float scale;
    public int heightMultiplier;
    public Material meshMaterial;
    int offsetCounter=0;
    public AnimationCurve heightBase;
    bool requested;
    void Update()
    {
        float moveSpeed = player.GetComponent<PlayerConfig>().moveSpeed;
        transform.Translate(new Vector3(-Time.deltaTime*moveSpeed, 0, 0));

        if(lastTerrainBlock == null){
            AddTerrainBlock();
        }
        else if(lastTerrainBlock.transform.position.x - player.transform.position.x < createNewDistanceThreshold){
            AddTerrainBlock();
        }
    }

    public void AddTerrainBlock(){
        if(requested){
            return;
        }
        requested = true;
        float[,] heightMap = GenerateHeightMap(length, width, scale, heightMultiplier, new Vector2(length-1, 0)*offsetCounter++, new AnimationCurve(heightBase.keys));
        GameObject newTerrainBlock = GenerateTerrainBlock(heightMap);
        if(lastTerrainBlock != null){
            newTerrainBlock.transform.position = lastTerrainBlock.transform.position + new Vector3(length-1,0,0);
        }
        else{
            newTerrainBlock.transform.position = new Vector3(player.transform.position.x-length/2, 0, player.transform.position.z-width/2);
        }
        newTerrainBlock.transform.parent = transform;
        
        lastTerrainBlock = newTerrainBlock;
        terrainBlocks.Enqueue(lastTerrainBlock);
        
        if(terrainBlocks.Count > maxTerrainBlocks){
            GameObject.Destroy(terrainBlocks.Dequeue());
        }


        GameObject environment = GenerateEnvironment(heightMap, obstaclesDensity, powerUpsDensityFactor, player, jumpableObstaclePrefab, avoidableObstaclePrefab, powerUpPrefab, minObstacleGap);
        environment.transform.position = new Vector3(newTerrainBlock.transform.position.x, newTerrainBlock.transform.position.y, newTerrainBlock.transform.position.z);
        environment.transform.parent = newTerrainBlock.transform;

        requested = false;
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

                heightMap[i0,i1] = perlinNoise*heightMultiplier+heightBase.Evaluate(i1);
            }
        }

        // for(int i0=0; i0<length; i0++){
        //     for(int i1=0; i1<width; i1++){  
        //         heightMap[i0,i1] = Mathf.Lerp(0, heightMultiplier, heightMap[i0,i1]);
        //     }
        // }
    
        return heightMap;
    }

    public static Mesh GenerateMesh(float[,] heightMap){
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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    public GameObject GenerateTerrainBlock(float[,] heightMap){
        GameObject terrainBlock = new GameObject();
        MeshFilter meshFilter = terrainBlock.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainBlock.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrainBlock.AddComponent<MeshCollider>();
        
        Mesh mesh = GenerateMesh(heightMap);

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshCollider.AddComponent<TerrainCollisions>();
        meshRenderer.sharedMaterial = meshMaterial;

        terrainBlock.tag = "Jumpable";

        return terrainBlock;
    }


    public static GameObject GenerateEnvironment(float[,] heightMap, int obstaclesDensity, float powerUpsDensityFactor, GameObject player, GameObject jumpableObstaclePrefab, GameObject avoidableObstaclePrefab, GameObject powerUpPrefab, float minObstacleGap){
        GameObject environment = new GameObject();
        
        int avoidableObstaclesDensity = Mathf.RoundToInt(obstaclesDensity*((float)new System.Random().NextDouble())*0.5f);
        int jumpableObstaclesDensity = obstaclesDensity-avoidableObstaclesDensity;

        List<Vector3> jumpableObstaclePositions = GeneratePositions(heightMap, jumpableObstaclesDensity, player, new(), minObstacleGap, false);
        GameObject jumpableObstacles = CreateGameObjectAtPositions(jumpableObstaclePositions, jumpableObstaclePrefab);
        jumpableObstacles.transform.parent = environment.transform;
        
        List<Vector3> avoidableObstaclePositions = avoidableObstaclesDensity == 0 ? new() : GeneratePositions(heightMap, avoidableObstaclesDensity, player, jumpableObstaclePositions, minObstacleGap, false);
        GameObject avoidableObstacles = CreateGameObjectAtPositions(avoidableObstaclePositions, avoidableObstaclePrefab);
        avoidableObstacles.transform.parent = environment.transform;

        List<Vector3> powerUpPositions = GeneratePositions(heightMap, obstaclesDensity*powerUpsDensityFactor, player, jumpableObstaclePositions, 0, true);
        GameObject powerUps = CreateGameObjectAtPositions(powerUpPositions, powerUpPrefab);
        powerUps.transform.parent = environment.transform;
        
        return environment;
    }

    static GameObject CreateGameObjectAtPositions(List<Vector3> positions, GameObject prefab){
        GameObject go = new GameObject();

        foreach (var item in positions)
        {
            GameObject go1 = Instantiate(prefab, item, Quaternion.identity);
            go1.transform.parent = go.transform;
        }

        return go;

    }

    static List<Vector3> GeneratePositions(float[,] heightMap, float density, GameObject player, List<Vector3> takenPositions, float minGap, bool randomizeY){
        List<Vector3> obstacles = new();
        List<Vector3> takenPositionEffective = new List<Vector3>(takenPositions);
        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);

        int increment = Mathf.RoundToInt(length/density);
        // float playerZ = player.transform.position.z;
        int playerZ = width/2; //player is at z=width/2 w.r.t block
        
        for(int i0=10; i0 < length; i0+=increment){
            int x = new System.Random().Next(i0, Mathf.Min(i0+increment, length-1));
            int z = playerZ;
            float y = heightMap[x, z];
            
            if(randomizeY){
                float minPowerUpsHeight = 2f;
                float maxPowerUpsHeight = player.GetComponent<PlayerConfig>().maxHeight;
                y += Mathf.Lerp(minPowerUpsHeight, maxPowerUpsHeight, (float)new System.Random().NextDouble());
            }

            if(takenPositionEffective.Any(item => Math.Abs(item.x-x)<minGap )){
                if(length-1-i0 < minGap){
                    break;
                }
                i0-=increment;
                continue;
            }
            Vector3 newPosition = new Vector3(x, y, z);
            obstacles.Add(newPosition);
            takenPositionEffective.Add(newPosition);
        }

        return obstacles;
    }
}
