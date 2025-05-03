using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{

    public GameObject terrain;
    public int terrainBlockLength;
    public int reqBlocks=3;
    public int chunkOverlap;
    public TerrainChunkValues pathTerrainData;
    public TerrainChunkValues leftSlopeData;
    public TerrainChunkValues rightSlopeData;
    public TrackValues trackValues;
    Queue<TerrainBlockData> terrainBlockDataQueue = new();
    List<TerrainBlock> terrainBlocks = new();
    GameObject currentTerrainBlock;
    int offset = 0;
    public int minMoveSpeed = 60;
    public int maxMoveSpeed = 120;
    public int moveSpeed = 60;
    int blocksRequestedButNotReceived = 0;
    public int difficultyChangeThreshold = 10;
    public GameObject player;

    public AnimationCurve transitionTerrainHeightCap;

    public bool triggerTransition = false;
    public int targetHeightCap = 20;

    // bool blockRequested;

    [Range(1,4)]
    public int difficulty;

    HeightMapParams leftExtensionParams = new HeightMapParams();
    HeightMapParams pathParams = new HeightMapParams();
    HeightMapParams rightExtensionParams = new HeightMapParams();
    

    void Update()
    {   

        try{
            int newDifficulty = Mathf.FloorToInt(player.GetComponent<PlayerControlsNew>().score/(difficultyChangeThreshold*((float)Math.Pow(2,difficulty-1))));
            difficulty = newDifficulty > difficulty ? newDifficulty : difficulty;
            if(!player.GetComponent<PlayerControlsNew>().isAlive){
                moveSpeed = 0;
                return;
            }   
        }
        catch(Exception ex){
            Debug.Log("couldn't get score, failed to set difficulty. " + ex.Message);
        }

        moveSpeed = minMoveSpeed + (maxMoveSpeed-minMoveSpeed)*(difficulty-1)/3;
        try{
            Debug.Log("animator speed: "  + (2 + (moveSpeed-minMoveSpeed)/(maxMoveSpeed-minMoveSpeed)));
            player.GetComponent<Animator>().speed = 2 + (moveSpeed-minMoveSpeed)/(maxMoveSpeed-minMoveSpeed);
        }
        catch(Exception ex){
            Debug.Log("couldn't change animator.speed, " + ex.Message);
        }
        terrain.transform.Translate(-moveSpeed*Time.deltaTime, 0, 0);
        if(currentTerrainBlock && currentTerrainBlock.transform.position.x < -terrainBlockLength){
            Destroy(currentTerrainBlock);
            currentTerrainBlock = terrainBlocks[0].gameObject;
            for(int i0=0; i0<terrainBlocks.Count-1; i0++){
                terrainBlocks[i0]=terrainBlocks[i0+1];
            }
            terrainBlocks.RemoveAt(terrainBlocks.Count-1);
        }

        if(blocksRequestedButNotReceived + terrainBlocks.Count + 1 < reqBlocks){
            GenerateTerrainBlockData(offset++);
            blocksRequestedButNotReceived++;
        }

        // if(terrainBlocks.Count < reqBlocks && !blockRequested){
        //     GenerateTerrainBlockData(offset++);
        //     blockRequested = true;
        // }

        Monitor.Enter(terrainBlockDataQueue);
        while(terrainBlockDataQueue.Count > 0){
            TerrainBlockData terrainBlockData = terrainBlockDataQueue.Dequeue();
            Monitor.Exit(terrainBlockDataQueue);
            terrainBlocks.Add(GenerateTerrainBlock(terrainBlockData));
            blocksRequestedButNotReceived--;
            // blockRequested = false;
            if(currentTerrainBlock == null){
                currentTerrainBlock = terrainBlocks[0].gameObject;

                if(!terrainBlocks[0].isEnvironmentInitialized){
                    terrainBlocks[0].GenerateEnvironment();
                }

                for(int i0=0; i0<terrainBlocks.Count-1; i0++){
                    terrainBlocks[i0]=terrainBlocks[i0+1];
                }
                terrainBlocks.RemoveAt(terrainBlocks.Count-1);
                
            }
            
            Monitor.Enter(terrainBlockDataQueue);
        }    
        Monitor.Exit(terrainBlockDataQueue);

        foreach (var item in terrainBlocks)
        {
            if(item.gameObject.transform.position.x < 2*terrainBlockLength && !item.isEnvironmentInitialized){
            // if(!item.isEnvironmentInitialized){
                item.GenerateEnvironment();
            }
            if(item.gameObject.transform.position.x < terrainBlockLength && !item.areCollidersSet){
                item.AddColliders();
            }
        }
    }

    public void GenerateTerrainBlockEditor()
    {

        for(int i0=0; i0<reqBlocks; i0++){
            TerrainBlock terrainBlock = GenerateTerrainBlock(ThreadLogic(offset++));
            terrainBlock.GenerateEnvironment();
        }
        offset = 0;
        // GenerateTerrain(pathTerrainData, terrain);
    }

    TerrainBlock GenerateTerrainBlock(TerrainBlockData terrainBlockData){
        GameObject terrainBlock = new GameObject();
        
        GameObject leftExtension = GenerateTerrainChunk(terrainBlockData.leftExtension, terrainBlockData.offset);
        leftExtension.transform.parent = terrainBlock.transform;
        GameObject path = GenerateTerrainChunk(terrainBlockData.path, terrainBlockData.offset);
        path.transform.parent = terrainBlock.transform;
        path.transform.position = new Vector3(0,0,leftExtension.transform.position.z + terrainBlockData.leftExtension.terrainValues.width - chunkOverlap);
        GameObject rightExtension = GenerateTerrainChunk(terrainBlockData.rightExtension, terrainBlockData.offset);
        rightExtension.transform.parent = terrainBlock.transform;
        rightExtension.transform.position = new Vector3(0,0,path.transform.position.z + terrainBlockData.path.terrainValues.width - chunkOverlap);

        terrainBlock.transform.parent = terrain.transform;
        terrainBlock.transform.localPosition = new Vector3((terrainBlockLength-1)*terrainBlockData.offset, 0, -(terrainBlockData.leftExtension.terrainValues.width + terrainBlockData.path.terrainValues.width - chunkOverlap + terrainBlockData.rightExtension.terrainValues.width - chunkOverlap)/2);
        path.tag = "Jumpable";
        return new TerrainBlock(terrainBlock, terrainBlockData, leftExtension, path, rightExtension);
    }

    GameObject GenerateTerrainChunk(TerrainChunkData terrainData, int offset){

        GameObject terrainChunk = new GameObject();
        MeshFilter meshFilter = terrainChunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainChunk.AddComponent<MeshRenderer>();
        // MeshCollider meshCollider = terrainChunk.AddComponent<MeshCollider>();


        MeshData meshData = terrainData.meshData;
        Mesh mesh = meshData.CreateMesh();

        meshFilter.sharedMesh = mesh;
        // meshCollider.sharedMesh = mesh;
        // meshCollider.GetOrAddComponent<TerrainCollisions>().offset = offset;
        meshRenderer.sharedMaterial = terrainData.terrainValues.meshMaterial;


        // foreach (var item in terrainData.plantPatchData)
        // {
        //     GameObject plantPatch = item.CreatePlantPatch();
        //     plantPatch.transform.parent = terrainChunk.transform;
        // }
        return terrainChunk;
    }

    void GenerateTerrainBlockData(int offset){
        
        ThreadStart threadStart = delegate {
            TerrainBlockData terrainBlockData = ThreadLogic(offset);
            
            lock(terrainBlockDataQueue){
                terrainBlockDataQueue.Enqueue(terrainBlockData);
            }
        };

        new Thread(threadStart).Start();
        
    }

    TerrainBlockData ThreadLogic(int offset){

        leftSlopeData.offset = new Vector2((terrainBlockLength-1)*offset, 0);
        pathTerrainData.offset = new Vector2((terrainBlockLength-1)*offset, leftSlopeData.width-chunkOverlap); 
        rightSlopeData.offset = new Vector2((terrainBlockLength-1)*offset, leftSlopeData.width-chunkOverlap + pathTerrainData.width - chunkOverlap);

        TerrainChunkData leftExtension, rightExtension, path;
        
        if(triggerTransition){
            leftExtensionParams.heightCap = targetHeightCap;
            rightExtensionParams.heightCap = targetHeightCap;
            pathParams.heightCap = targetHeightCap <= 20 ? targetHeightCap : -1;
            triggerTransition = false;
        }
            leftExtension = GenerateTerrainChunkData(leftSlopeData, leftExtensionParams);
            rightExtension = GenerateTerrainChunkData(rightSlopeData, rightExtensionParams);
            path = GenerateTerrainChunkData(pathTerrainData, pathParams, true, offset == 0);
        

        return new TerrainBlockData(offset, leftExtension, path, rightExtension);
    }

    TerrainChunkData GenerateTerrainChunkData(TerrainChunkValues terrainValues, HeightMapParams heightMapParams, bool isTrack = false, bool isFirstBlock = false){
        float[,] heightMap = MeshGenerator.GenerateHeightData(terrainBlockLength, terrainValues.width, terrainValues.scale, terrainValues.persistance, terrainValues.lacunarity, terrainValues.octaves, terrainValues.offset, new AnimationCurve(terrainValues.heightBase.keys), heightMapParams);
        // float[,] heightMap = Noise.GenerateNoiseMap(terrainBlockLength, terrainValues.width, 0, terrainValues.scale, terrainValues.octaves, terrainValues.persistance, terrainValues.lacunarity, terrainValues.offset, 30, terrainValues.heightBase);
        MeshData meshData = MeshGenerator.GenerateMeshData(heightMap);
        // List<PlantPatch> plantPatchData = EnvironmentGenerator.GeneratePlantPatchData(heightMap, terrainValues.minPlantPatchDensity, terrainValues.maxPlantPatchDensity, terrainValues.plantPatchStepZ);
        List<PlantPatch> plantPatchData = new();
        CosmeticPlantPositions cosmeticPlantPositions = EnvironmentGenerator.GenerateCosmeticPlantsPositions(heightMap, terrainValues.cosmeticPlantStepx, terrainValues.cosmeticPlantStepz);
        TerrainChunkData terrainChunkData = new TerrainChunkData(heightMap, meshData, terrainValues, plantPatchData, cosmeticPlantPositions);
        if(isTrack){
            terrainChunkData.obstaclesData = EnvironmentGenerator.GenerateObstaclesData(heightMap, trackValues, moveSpeed, isFirstBlock, difficulty);
        }
        return terrainChunkData;
    }
    
    GameObject GenerateTerrain(TerrainChunkValues terrainData, GameObject _terrainChunk){
        
        GameObject terrainChunk = _terrainChunk == null ? new GameObject() : _terrainChunk;
        float[,] heightMap = MeshGenerator.GenerateHeightData(terrainBlockLength, terrainData.width, terrainData.scale, terrainData.persistance, terrainData.lacunarity, terrainData.octaves, terrainData.offset, new AnimationCurve(terrainData.heightBase.keys), new HeightMapParams());
        MeshData meshData = MeshGenerator.GenerateMeshData(heightMap);
        
        MeshFilter meshFilter = terrainChunk.GetOrAddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainChunk.GetOrAddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrainChunk.GetOrAddComponent<MeshCollider>();
        
        Mesh mesh = meshData.CreateMesh();
        
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshCollider.GetOrAddComponent<TerrainCollisions>();
        meshRenderer.sharedMaterial = terrainData.meshMaterial;
        return terrainChunk;
    }

    class TerrainChunkData{

        float[,] heightMap;
        public MeshData meshData;
        public TerrainChunkValues terrainValues;
        public List<PlantPatch> plantPatchData;
        public CosmeticPlantPositions cosmeticPlantPositions;
        public ObstaclesData obstaclesData;

        public TerrainChunkData(float[,] heightMap, MeshData meshData, TerrainChunkValues terrainValues, List<PlantPatch> plantPatchData, CosmeticPlantPositions cosmeticPlantPositions) {
            this.heightMap = heightMap;
            this.meshData = meshData;
            this.terrainValues = terrainValues;
            this.plantPatchData = plantPatchData;
            this.cosmeticPlantPositions = cosmeticPlantPositions;
        }
    }

    class TerrainBlockData{
        public TerrainChunkData leftExtension;
        public TerrainChunkData path;
        public TerrainChunkData rightExtension;
        public int offset;
        
        public TerrainBlockData(int offset, TerrainChunkData leftExtension, TerrainChunkData path, TerrainChunkData rightExtension){
            this.offset = offset;
            this.leftExtension = leftExtension;
            this.path = path;
            this.rightExtension = rightExtension;
        }
    }

    class TerrainBlock{
        public GameObject gameObject;
        public TerrainBlockData terrainBlockData;
        public GameObject leftExtension;
        public GameObject path;
        public GameObject rightExtension;
        
        public bool isEnvironmentInitialized = false;
        public bool areCollidersSet = false;

        public TerrainBlock(GameObject gameObject, TerrainBlockData terrainBlockData, GameObject leftExtension, GameObject path, GameObject rightExtension){
            this.gameObject = gameObject;
            this.terrainBlockData = terrainBlockData;
            this.leftExtension = leftExtension;
            this.path = path;
            this.rightExtension = rightExtension;
        }

        public void GenerateEnvironment(){
            GameObject leftPlantPatch = GeneratePlantPatches(terrainBlockData.leftExtension.plantPatchData);
            GameObject pathPlantPatch = GeneratePlantPatches(terrainBlockData.path.plantPatchData);
            GameObject rightPlantPatch = GeneratePlantPatches(terrainBlockData.rightExtension.plantPatchData);

            GameObject leftCosmeticPlants = terrainBlockData.leftExtension.cosmeticPlantPositions.CreateCosmeticPlants();
            GameObject pathCosmeticPlants = terrainBlockData.path.cosmeticPlantPositions.CreateCosmeticPlants();
            GameObject rightCosmeticPlants = terrainBlockData.rightExtension.cosmeticPlantPositions.CreateCosmeticPlants();

            leftPlantPatch.transform.position = leftExtension.transform.position;
            leftPlantPatch.transform.parent = leftExtension.transform;

            leftCosmeticPlants.transform.position = leftExtension.transform.position;
            leftCosmeticPlants.transform.parent = leftExtension.transform;

            pathPlantPatch.transform.position = path.transform.position;
            pathPlantPatch.transform.parent = path.transform;

            pathCosmeticPlants.transform.position = path.transform.position;
            pathCosmeticPlants.transform.parent = path.transform;

            rightPlantPatch.transform.position = rightExtension.transform.position;
            rightPlantPatch.transform.parent = rightExtension.transform;

            rightCosmeticPlants.transform.position = rightExtension.transform.position;
            rightCosmeticPlants.transform.parent = rightExtension.transform;

            // GameObject obstalces = terrainBlockData.path.obstaclesData.CreateObstacles();

            // obstalces.transform.position = path.transform.position;
            // obstalces.transform.parent = path.transform;


            AddColliders();

            isEnvironmentInitialized = true;
        }

        GameObject GeneratePlantPatches(List<PlantPatch> plantPatchData){
            GameObject plantPatches = new GameObject();
            foreach (var item in plantPatchData)
            {
                GameObject plantPatch = item.CreatePlantPatch();
                plantPatch.transform.parent = plantPatches.transform;
            }

            return plantPatches;
        }

        public void AddColliders(){
            // AddColliderMesh(leftExtension);
            AddColliderMesh(path);
            areCollidersSet = true;
            // AddColliderMesh(rightExtension);
        }

        void AddColliderMesh(GameObject terrainChunk){
            MeshFilter meshFilter = terrainChunk.GetOrAddComponent<MeshFilter>();
            MeshCollider meshCollider = terrainChunk.GetOrAddComponent<MeshCollider>();


            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.GetOrAddComponent<TerrainCollisions>();
        
        
        }
    }
}
