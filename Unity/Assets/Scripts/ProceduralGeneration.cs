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
        float[,] heightMap = MeshGenerator.GenerateHeightMap(length, width, scale, heightMultiplier, new Vector2(length-1, 0)*offsetCounter++, new AnimationCurve(heightBase.keys));
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

        requested = false;
    }

    public GameObject GenerateTerrainBlock(float[,] heightMap){
        GameObject terrainBlock = new GameObject();
        MeshFilter meshFilter = terrainBlock.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainBlock.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrainBlock.AddComponent<MeshCollider>();
        
        MeshData meshData = MeshGenerator.GenerateMeshData(heightMap);
        Mesh mesh = meshData.CreateMesh();
        
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshCollider.AddComponent<TerrainCollisions>();
        meshRenderer.sharedMaterial = meshMaterial;

        terrainBlock.tag = "Jumpable";

        GameObject environment = EnvironmentGenerator.GenerateEnvironment(heightMap, obstaclesDensity, powerUpsDensityFactor, player, jumpableObstaclePrefab, avoidableObstaclePrefab, powerUpPrefab, minObstacleGap, meshData);
        environment.transform.position = new Vector3(terrainBlock.transform.position.x, terrainBlock.transform.position.y, terrainBlock.transform.position.z);
        environment.transform.parent = terrainBlock.transform;

        return terrainBlock;
    }

}
