using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    void Start()
    {
        terrainsDataDTOs = new TerrainChunkValuesDTO[terrainsData.Count()];
        for (int i0 = 0; i0 < terrainsData.Count(); i0++)
        {
            terrainsDataDTOs[i0] = new TerrainChunkValuesDTO(terrainsData[i0]);
        }

        terrainData = new TerrainChunkValuesDTO(GetRandomTerrainData());

        heightMapParams = new HeightMapParams();
        offset = 0;
        blocks = new();

        surfacePlants = new GameObject[4];
        for (int i0 = 1; i0 < 4; i0++)
        {
            surfacePlants[i0 - 1] = Resources.Load<GameObject>("Terrain/Surface Plants/plants_" + i0);
        }
        surfacePlantMaterial = Resources.Load<Material>("Terrain/Surface Plants/surface plant material");
        cosmeticPlant = Resources.Load<GameObject>("Terrain/Cosmetic Plants/cosmetic plant prefab");

        obstaclePrefabs = new GameObject[2];
        obstaclePrefabs[0] = Resources.Load<GameObject>("Terrain/Obstacles/Big Bones/Big Bones Prefab");
        obstaclePrefabs[1] = Resources.Load<GameObject>("Terrain/Obstacles/Rock/Rock Prefab");
    }

    public void SetVariablesForRestart()
    {
        offset = 0;
        blocks = new();
        lastBlock = null;
    }

    void Update()
    {
        // GenerateBlocks();

        int zLanesStart = terrainData.width / 2 - 50;
        for (int i0 = 0; i0 < 6; i0++)
        {
            Debug.DrawLine(new Vector3(0, 5, zLanesStart + i0 * 20), new Vector3(1000, 5, zLanesStart + i0 * 20), Color.green);
        }
    }

    public void GenerateBlocksEditor()
    {
        Start();
        Update();
    }

    public TerrainChunkValues[] terrainsData;
    public int terrainBlockLength = 241;
    public int moveSpeed = 60;

    private TerrainChunkValuesDTO terrainData;
    private TerrainChunkValuesDTO targetData;
    private TerrainChunkValuesDTO[] terrainsDataDTOs;

    Queue<TerrainBlock> blocks = new();
    int offset = 0;
    GameObject lastBlock;
    HeightMapParams heightMapParams;
    public GameObject player;
    public int difficulty;
    TerrainChunkValuesDTO GetRandomTerrainData()
    {
        double rand = new System.Random().NextDouble();
        int idx = Mathf.RoundToInt((float)(rand * (terrainsDataDTOs.Count() - 1)));
        return terrainsDataDTOs[idx];
    }

    public void HandleBlocks()
    {

        if (blocks.Count > 0 && blocks.Peek().gameObject.transform.position.x < 0)
        {
            Destroy(blocks.Dequeue().gameObject);
        }

        if (blocks.Count < 7)
        {
            if (offset % 5 == 0)
            {
                terrainData.transformingTerrains = true;
                targetData = GetRandomTerrainData();
            }
            terrainData.offset = new Vector2((terrainBlockLength - 1) * offset, 0);

            if (heightMapParams.heightCap == -1)
            {
                int randomTargetHeight = new System.Random().Next(terrainData.minHeight, terrainData.maxHeight);
                heightMapParams.heightCap = randomTargetHeight;
            }

#if !UNITY_WEBGL
            new Thread(() =>
            {
#endif

                if (waitingForTerrain)
                {
                    return;
                }
                waitingForTerrain = true;
                offset++;
                GenerateTerrainData(terrainData, targetData, terrainBlockLength, heightMapParams, moveSpeed);

#if !UNITY_WEBGL
            }).Start();
#endif
        }

        if (terrainDataQueue.Count > 0)
        {

            TerrainBlock terrainBlock = GenerateTerrain(terrainDataQueue.Dequeue());
            GameObject go = terrainBlock.gameObject;

            if (lastBlock)
            {
                go.transform.position = lastBlock.transform.position + new Vector3(terrainBlockLength - 1, 0, 0);
            }
            else
            {
                go.transform.position = new Vector3(200, 0, 0);
            }

            lastBlock = go;
            go.transform.parent = transform;

            blocks.Enqueue(terrainBlock);
        }
    }

    public void AddColliders()
    {
        foreach (var block in blocks)
        {
            block.gameObject.transform.Translate(-moveSpeed * Time.deltaTime, 0, 0);
            if (block.gameObject.transform.position.x < player.transform.position.x + 50)
            {
                block.ActivateCollider();
            }
        }
    }

    static void GenerateTerrainData(TerrainChunkValuesDTO terrainData, TerrainChunkValuesDTO targetTerrainData, int terrainBlockLength, HeightMapParams heightMapParams, int moveSpeed)
    {


        TerrainChunkValuesDTO transformingTerrainData = new TerrainChunkValuesDTO(terrainData);

        Action<TerrainChunkValuesDTO, int, int> terrainValuesUpdater = (currentTerrainData, i0, length) =>
        {
            if (terrainData.transformingTerrains)
            {
                if (!currentTerrainData.LerpTerrainData(terrainData, targetTerrainData, i0, length))
                {
                    terrainData.transformingTerrains = false;
                }
            }
        };

        float[,] heightMap = TerrainMeshGenerator.GenerateHeightData(terrainBlockLength, terrainData.width, transformingTerrainData, terrainValuesUpdater, terrainData.heightMultiplier, terrainData.offset, new AnimationCurve(terrainData.heightBase.keys), heightMapParams);

        terrainData.scale = transformingTerrainData.scale;
        terrainData.octaves = transformingTerrainData.octaves;
        terrainData.persistance = transformingTerrainData.persistance;
        terrainData.lacunarity = transformingTerrainData.lacunarity;
        terrainData.heightMultiplier = transformingTerrainData.heightMultiplier;

        MeshData meshData = TerrainMeshGenerator.GenerateMeshData(heightMap);

        float[,] colliderHeightMap = new float[terrainBlockLength, terrainData.pathLength + 20];

        int startZ = Mathf.FloorToInt(terrainData.width / 2 - terrainData.pathLength / 2 - 10);

        for (int i0 = 0; i0 < terrainBlockLength; i0++)
        {
            for (int i1 = 0; i1 < terrainData.pathLength + 20; i1++)
            {
                colliderHeightMap[i0, i1] = heightMap[i0, startZ + i1];
            }
        }
        MeshData colliderMeshData = TerrainMeshGenerator.GenerateMeshData(colliderHeightMap, startZ, true);

        Dictionary<Vector3, Quaternion> surfacePlantPositions = EnvironmentGenerator.GeneratePlantPositions(heightMap);
        Dictionary<Vector3, Quaternion> obstaclePositions = EnvironmentGenerator.GenerateObstaclePositions(heightMap, moveSpeed, 2);

        terrainDataQueue.Enqueue(new TerrainData(meshData, terrainData.meshMaterial, colliderMeshData, surfacePlantPositions, obstaclePositions));
    }

    static GameObject[] surfacePlants;
    static Material surfacePlantMaterial;
    static GameObject cosmeticPlant;
    static GameObject[] obstaclePrefabs;

    static TerrainBlock GenerateTerrain(TerrainData terrainData)
    {
        waitingForTerrain = false;

        GameObject terrainChunk = new GameObject();
        MeshFilter meshFilter = terrainChunk.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainChunk.AddComponent<MeshRenderer>();

        Mesh mesh = terrainData.meshData.CreateMesh();

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = terrainData.meshMaterial;

        Dictionary<Vector3, Quaternion> plantPositions = terrainData.surfacePlantPositions;

        GameObject plants = new GameObject();
        foreach (var item in plantPositions)
        {
            // GameObject plant = Instantiate(surfacePlants[Mathf.RoundToInt(new System.Random().Next(0, 1000) / 1000 * 3)], item.Key, item.Value);
            GameObject plant = Instantiate(cosmeticPlant, item.Key, item.Value);
            // plant.GetComponent<MeshRenderer>().sharedMaterial = surfacePlantMaterial;
            plant.transform.parent = plants.transform;
            // plant.transform.localScale = new Vector3(20, 20, 20);
        }
        plants.transform.parent = terrainChunk.transform;

        GameObject obstacles = new GameObject();
        foreach (var item in terrainData.obstaclePositions)
        {
            int obstacleIdx = Mathf.RoundToInt((float)new System.Random().NextDouble() * (obstaclePrefabs.Count() - 1));
            Quaternion orientation = item.Value;
            if (obstacleIdx == 0) {
                orientation = Quaternion.Euler(0, orientation.eulerAngles.y, orientation.eulerAngles.z);
            }
            GameObject obstacle = Instantiate(obstaclePrefabs[obstacleIdx], item.Key, orientation);
            obstacle.transform.parent = obstacles.transform;
        }
        obstacles.transform.parent = terrainChunk.transform;

        return new TerrainBlock(terrainChunk, terrainData.colliderMeshData.CreateMesh());
    }

    static bool waitingForTerrain = false;
    static Queue<TerrainData> terrainDataQueue = new();
    struct TerrainData
    {
        public readonly MeshData meshData;
        public readonly Material meshMaterial;
        public readonly MeshData colliderMeshData;
        public readonly Dictionary<Vector3, Quaternion> surfacePlantPositions;
        public readonly Dictionary<Vector3, Quaternion> obstaclePositions;

        public TerrainData(MeshData meshData, Material meshMaterial, MeshData colliderMeshData, Dictionary<Vector3, Quaternion> surfacePlantPositions, Dictionary<Vector3, Quaternion> obstaclePositions)
        {
            this.meshData = meshData;
            this.meshMaterial = meshMaterial;
            this.colliderMeshData = colliderMeshData;
            this.surfacePlantPositions = surfacePlantPositions;
            this.obstaclePositions = obstaclePositions;
        }
    }

    class TerrainBlock
    {
        public GameObject gameObject;
        public Mesh colliderMesh;
        MeshCollider collider;
        bool colliderAdded;
        public TerrainBlock(GameObject gameObject, Mesh colliderMesh)
        {
            this.gameObject = gameObject;
            this.colliderMesh = colliderMesh;
        }

        public void ActivateCollider()
        {
            if (colliderAdded)
            {
                return;
            }
            GameObject go = new GameObject();
            go.transform.position = gameObject.transform.position;
            go.transform.parent = gameObject.transform;
            collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = colliderMesh;
            TerrainCollisions terrainCollisions = go.AddComponent<TerrainCollisions>();
            terrainCollisions.terrainBlock = gameObject;
            colliderAdded = true;
        }
    }
}
