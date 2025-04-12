using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    void Update()
    {
        float moveSpeed = player.GetComponent<PlayerConfig>().moveSpeed;
        transform.Translate(new Vector3(-Time.deltaTime*moveSpeed, 0, 0));

        if(lastTerrainBlock == null){
            CreateTerrainBlock();
        }
        else if(lastTerrainBlock.transform.position.x - player.transform.position.x < createNewDistanceThreshold){
            CreateTerrainBlock();
        }
    }

    public void CreateTerrainBlock(){
        GameObject newTerrainBlock = GenerateTerrainBlock(length, width);
        if(lastTerrainBlock != null){
            newTerrainBlock.transform.position = lastTerrainBlock.transform.position + new Vector3(length,0,0);
        }
        else{
            newTerrainBlock.transform.position = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        }
        newTerrainBlock.transform.parent = transform;
        
        lastTerrainBlock = newTerrainBlock;
        terrainBlocks.Enqueue(lastTerrainBlock);
        
        if(terrainBlocks.Count > maxTerrainBlocks){
            GameObject.Destroy(terrainBlocks.Dequeue());
        }


        GameObject environment = GenerateEnvironment(length, width, obstaclesDensity, powerUpsDensityFactor, player, jumpableObstaclePrefab, avoidableObstaclePrefab, powerUpPrefab, minObstacleGap);
        environment.transform.position = new Vector3(newTerrainBlock.transform.position.x - length/2, newTerrainBlock.transform.position.y, newTerrainBlock.transform.position.z);  //adjustment for center pivot
        environment.transform.parent = newTerrainBlock.transform;
    }

    public static GameObject GenerateTerrainBlock(int length, int width){
        GameObject block  = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.transform.localScale = new Vector3(length, 1, width);
        block.tag = "Jumpable";

        return block;
    }

    public static GameObject GenerateEnvironment(int length, int width, int obstaclesDensity, float powerUpsDensityFactor, GameObject player, GameObject jumpableObstaclePrefab, GameObject avoidableObstaclePrefab, GameObject powerUpPrefab, float minObstacleGap){
        GameObject environment = new GameObject();
        
        int avoidableObstaclesDensity = Mathf.RoundToInt(obstaclesDensity*((float)new System.Random().NextDouble())*0.5f);
        int jumpableObstaclesDensity = obstaclesDensity-avoidableObstaclesDensity;

        List<Vector3> jumpableObstaclePositions = GeneratePositions(length, width, jumpableObstaclesDensity, player, new(), minObstacleGap, false);
        GameObject jumpableObstacles = CreateGameObjectAtPositions(jumpableObstaclePositions, jumpableObstaclePrefab);
        jumpableObstacles.transform.parent = environment.transform;
        
        List<Vector3> avoidableObstaclePositions = avoidableObstaclesDensity == 0 ? new() : GeneratePositions(length, width, avoidableObstaclesDensity, player, jumpableObstaclePositions, minObstacleGap, false);
        GameObject avoidableObstacles = CreateGameObjectAtPositions(avoidableObstaclePositions, avoidableObstaclePrefab);
        avoidableObstacles.transform.parent = environment.transform;

        List<Vector3> powerUpPositions = GeneratePositions(length, width, obstaclesDensity*powerUpsDensityFactor, player, jumpableObstaclePositions, 0, true);
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

    static List<Vector3> GeneratePositions(int length, int width, float density, GameObject player, List<Vector3> takenPositions, float minGap, bool randomizeY){
        List<Vector3> obstacles = new();
        List<Vector3> takenPositionEffective = new List<Vector3>(takenPositions);
        
        int increment = Mathf.RoundToInt(length/density);
        // float playerZ = player.transform.position.z;
        float playerZ = 0; //player is at z=0 w.r.t block
        
        for(int i0=10; i0 < length; i0+=increment){
            int x = new System.Random().Next(i0, i0+increment);
            float y = 1;
            float z = playerZ;

            if(randomizeY){
                float minPowerUpsHeight = 2f;
                float maxPowerUpsHeight = player.GetComponent<PlayerConfig>().maxHeight;
                y = Mathf.Lerp(minPowerUpsHeight, maxPowerUpsHeight, (float)new System.Random().NextDouble());
            }

            if(takenPositionEffective.Any(item => Math.Abs(item.x-x)<minGap )){
                i0-=increment;
                continue;
            }
            Vector3 newPosition = new Vector3(x, y, z);
            obstacles.Add(newPosition);
            takenPositionEffective.Add(newPosition);
        }

        return obstacles;
    }

    static List<Vector3> GeneratePowerUps(int length, int width, float powerUpsDensity, GameObject player, List<Vector3> obstaclePositions){
        List<Vector3> powerUps = new();
        // float playerZ = player.transform.position.z;
        float playerZ = 0; //player is at z=0 w.r.t block
        float minPowerUpsHeight = 2f;
        float maxPowerUpsHeight = player.GetComponent<PlayerConfig>().maxHeight;
        int increment = Mathf.RoundToInt(length/powerUpsDensity);

        for(int i0=10; i0 < length; i0+=increment){
            int x = new System.Random().Next(i0, i0+increment);
            float y = Mathf.Lerp(minPowerUpsHeight, maxPowerUpsHeight, (float)new System.Random().NextDouble());
            Vector3 newPosition = new Vector3(x,y,playerZ);
  
            if(obstaclePositions.Contains(newPosition)){
                i0-=increment;
                continue;
            }
            
            powerUps.Add(newPosition);
        }

        return powerUps;
    }
}
