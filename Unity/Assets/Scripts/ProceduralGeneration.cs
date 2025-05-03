// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using Unity.VisualScripting;
using UnityEngine;
// using UnityEngine.UIElements;

public class ProceduralGeneration : MonoBehaviour
{

//     public GameObject player;
//     public int length;
//     public int width;
//     public int createNewDistanceThreshold;
//     GameObject lastTerrainBlock;
//     Queue<GameObject> terrainBlocks = new Queue<GameObject>();
//     public int maxTerrainBlocks;
//     [Range(1, 6)]
//     public int obstaclesDensity;
//     [Min(1)]
//     public float powerUpsDensityFactor;
//     public int minObstacleGap;
//     public GameObject jumpableObstaclePrefab;
//     public GameObject powerUpPrefab;
//     public GameObject avoidableObstaclePrefab;
//     public GameObject flyingObjectPrefab;
//     public float scale;
//     public int heightMultiplier;
//     public Material meshMaterial;
//     int offsetCounter=0;
//     public AnimationCurve heightBase;
//     bool requested = false;
//     static Queue<BlockData> blockDatas = new();

//     void Update()
//     {
//         // PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
//         // float moveSpeed = playerMovement.moveSpeed;
//         float moveSpeed = 10;
//         // int powerCount = playerMovement.powerUpCount;
//         int powerCount = 0;
//         obstaclesDensity = Mathf.Clamp(powerCount/10,1,6);
//         transform.Translate(new Vector3(-Time.deltaTime*moveSpeed, 0, 0));

//         if(lastTerrainBlock == null){
//             AddTerrainBlock();
//         }
//         else if(lastTerrainBlock.transform.position.x - player.transform.position.x < createNewDistanceThreshold){
//             AddTerrainBlock();
//         }
        
//         while(blockDatas.Count>0){
//             BlockData blockData = blockDatas.Dequeue();
//             HandleNewTerrainBlock(blockData);
//         }
//     }

//     public void AddTerrainBlock(){
//         if(requested){
//             return;
//         } 
//         requested = true;
//         // float[,] heightMap = MeshGenerator.GenerateHeightMap(length, width, scale, heightMultiplier, new Vector2(length-1, 0)*offsetCounter++, new AnimationCurve(heightBase.keys));
//         // GameObject newTerrainBlock = GenerateTerrainBlock(heightMap);

//         // ThreadStart threadStart = new ThreadStart(this.RequestTerrainBlock);

        
//             Vector3 playerPosition = player.GetComponent<PlayerControls>().originalPosition;

//             RequestTerrainBlock(length, width, scale, heightMultiplier, offsetCounter++, new AnimationCurve(heightBase.keys), obstaclesDensity, powerUpsDensityFactor, minObstacleGap, playerPosition);


//         // if(lastTerrainBlock != null){
//         //     newTerrainBlock.transform.position = lastTerrainBlock.transform.position + new Vector3(length-1,0,0);
//         // }
//         // else{
//         //     newTerrainBlock.transform.position = new Vector3(player.transform.position.x-length/2, 0, player.transform.position.z-width/2);
//         // }
//         // newTerrainBlock.transform.parent = transform;
        
//         // lastTerrainBlock = newTerrainBlock;
//         // terrainBlocks.Enqueue(lastTerrainBlock);
        
//         // if(terrainBlocks.Count > maxTerrainBlocks){
//         //     GameObject.Destroy(terrainBlocks.Dequeue());
//         // }
//     }

//     private static readonly object queueLock = new object();
//     static void RequestTerrainBlock(int length, int width, float scale, float heightMultiplier, int offsetCounter, AnimationCurve heightBase, int obstaclesDensity, float powerUpsDensityFactor, float minObstacleGap, Vector3 playerPosition){

//         // int maxHeight = (int)player.GetComponent<PlayerMovement>().maxHeight;
//         int maxHeight = 20;

//         ThreadStart threadStart = delegate{
//             // try{
           
//             float[,] heightMap = MeshGenerator.GenerateHeightMap(length, width, scale, heightMultiplier, new Vector2(length-1, 0)*offsetCounter, heightBase);
            
//             MeshData meshData = MeshGenerator.GenerateMeshData(heightMap);
            
//             EnvironmentData environmentData = EnvironmentGenerator.GenerateEnvironmentData(heightMap, obstaclesDensity, powerUpsDensityFactor, minObstacleGap, meshData, playerPosition, maxHeight);
            
//             // BlockData blockData = new BlockData(meshData, environmentData, heightMap);

//             BlockData blockData = new BlockData(meshData, environmentData, heightMap);
//             // lock(queueLock){
//                 blockDatas.Enqueue(blockData);
//             // }

//             // }

//         //     catch(Exception ex){
//         //         Debug.LogError("Thread exception: " + ex);
//         //     }
        
//         };
//         Thread thread = new Thread(threadStart);

        
//         thread.Start();
        
//     }

//     public int seed;
//     public int octaves;
//     [Range(0,1)]
//     public float persistance;
//     public float lacunarity;
//     public Vector3 offset;
//     public GameObject sampler;
//     public void AddAdvancedBlockEditor(){
//         float[,] heightMap = Noise.GenerateNoiseMap(width, length, seed, scale, octaves, persistance, lacunarity, offset, heightMultiplier, heightBase);
                        
//             MeshData meshData = MeshGenerator.GenerateMeshData(heightMap);
            
//             Vector3 playerPosition = player.GetComponent<PlayerControls>().originalPosition;
//             EnvironmentData environmentData = EnvironmentGenerator.GenerateEnvironmentData(heightMap, obstaclesDensity, powerUpsDensityFactor, minObstacleGap, meshData, playerPosition, 20);
            
//             BlockData blockData = new BlockData(meshData, environmentData, heightMap);
//             // sampler = new GameObject();
//             DestroyImmediate(sampler);
//             sampler = new GameObject();
//             GenerateTerrainBlock(blockData.heightMap, blockData.meshData, blockData.environmentData, sampler);
//     }

//     // void ReceivedBlockData(MeshData meshData, EnvironmentData environmentData, float[,] heightMap){
        
//     public void AddTerrainBlockEditor(){
//             float[,] heightMap = MeshGenerator.GenerateHeightMap(length, width, scale, heightMultiplier, new Vector2(length-1, 0)*offsetCounter, heightBase);
            
//             MeshData meshData = MeshGenerator.GenerateMeshData(heightMap);
            
//             Vector3 playerPosition = player.GetComponent<PlayerControls>().originalPosition;
//             EnvironmentData environmentData = EnvironmentGenerator.GenerateEnvironmentData(heightMap, obstaclesDensity, powerUpsDensityFactor, minObstacleGap, meshData, playerPosition, 20);
            
//             // BlockData blockData = new BlockData(meshData, environmentData, heightMap);

//             BlockData blockData = new BlockData(meshData, environmentData, heightMap);
//             HandleNewTerrainBlock(blockData);
//     }
        

//     // }

//     void HandleNewTerrainBlock(BlockData blockData){
        

//         // MeshData meshData = MeshGenerator.GenerateMeshData(blockData.heightMap);
           
//         // EnvironmentData environmentData = EnvironmentGenerator.GenerateEnvironmentData(blockData.heightMap, obstaclesDensity, powerUpsDensityFactor, minObstacleGap, meshData, playerPosition.transform.position, (int)player.GetComponent<PlayerMovement>().maxHeight);
            

//         GameObject newTerrainBlock = new GameObject();
//         GenerateTerrainBlock(blockData.heightMap, blockData.meshData, blockData.environmentData, newTerrainBlock);
        
//         if(lastTerrainBlock != null){
//             newTerrainBlock.transform.position = lastTerrainBlock.transform.position + new Vector3(length-1,0,0);
//         }
//         else{
//             newTerrainBlock.transform.position = new Vector3(player.transform.position.x-length/2, 0, player.transform.position.z-width/2);
//         }
//         newTerrainBlock.transform.parent = transform;
        
//         lastTerrainBlock = newTerrainBlock;
//         terrainBlocks.Enqueue(lastTerrainBlock);
        
//         if(terrainBlocks.Count > maxTerrainBlocks){
//             GameObject.Destroy(terrainBlocks.Dequeue());
//         }
//         requested = false;
//     }

//     public GameObject GenerateTerrainBlock(float[,] heightMap, MeshData meshData, EnvironmentData environmentData, GameObject terrainBlock){
//         MeshFilter meshFilter = terrainBlock.GetOrAddComponent<MeshFilter>();
//         MeshRenderer meshRenderer = terrainBlock.GetOrAddComponent<MeshRenderer>();
//         MeshCollider meshCollider = terrainBlock.GetOrAddComponent<MeshCollider>();
        
//         Mesh mesh = meshData.CreateMesh();
        
//         meshFilter.sharedMesh = mesh;
//         meshCollider.sharedMesh = mesh;
//         meshCollider.GetOrAddComponent<TerrainCollisions>();
//         meshRenderer.sharedMaterial = meshMaterial;

//         terrainBlock.tag = "Jumpable";

//         Vector3 playerPosition = player.GetComponent<PlayerControls>().originalPosition;
//         GameObject environment = EnvironmentGenerator.GenerateEnvironment(environmentData, heightMap, obstaclesDensity, powerUpsDensityFactor, player, jumpableObstaclePrefab, avoidableObstaclePrefab, powerUpPrefab, flyingObjectPrefab, minObstacleGap, meshData, playerPosition);
        
//         environment.transform.position = new Vector3(terrainBlock.transform.position.x, terrainBlock.transform.position.y, terrainBlock.transform.position.z);
//         environment.transform.parent = terrainBlock.transform;

//         return terrainBlock;
//     }
    

//     class BlockData{
//         public MeshData meshData;
//         public EnvironmentData environmentData;
//         public float[,] heightMap;

//         public BlockData(MeshData meshData, EnvironmentData environmentData, float[,] heightMap){
//             this.meshData = meshData;
//             this.environmentData = environmentData;
//             this.heightMap = heightMap;
//         }
//     }

}
