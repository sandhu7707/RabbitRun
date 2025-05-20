using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestMeshGenerator : MonoBehaviour
{
    public TerrainChunkValues testDataStart;
    public TerrainChunkValues testDataEnd;
    public int terrainBlockLength;
    public int numberOfBlocks;
    public GameObject testGameObject;
    GameObject[] plants;
    Material surfacePlantMaterial;

    void Start()
    {
        plants = new GameObject[4];
        for (int i0 = 1; i0 < 4; i0++)
        {
            plants[i0 - 1] = Resources.Load<GameObject>("Terrain/Surface Plants/plants_" + i0);
        }
        surfacePlantMaterial = Resources.Load<Material>("Terrain/Surface Plants/surface plant material");
    }

    public void GenerateBlocks()
    {
        Start();

        TerrainChunkValuesDTO terrainData = new TerrainChunkValuesDTO(testDataStart);
        TerrainChunkValuesDTO targetData = new TerrainChunkValuesDTO(testDataEnd);

        HeightMapParams heightMapParams = new HeightMapParams();
        heightMapParams.heightCap = 50;

        for (int i0 = 0; i0 < numberOfBlocks; i0++)
        {
            terrainData.offset = new Vector3(terrainBlockLength - 1, 0, 0) * i0;
            GameObject go = GenerateBlock(terrainData, targetData, terrainBlockLength, heightMapParams);
            go.transform.parent = testGameObject.transform;
            go.transform.localPosition = new Vector3(terrainBlockLength - 1, 0, 0) * i0;
        }
    }

    GameObject GenerateBlock(TerrainChunkValuesDTO terrainData, TerrainChunkValuesDTO targetTerrainData, int terrainBlockLength, HeightMapParams heightMapParams){

        TerrainChunkValuesDTO transformingTerrainData = new TerrainChunkValuesDTO(terrainData);

        Action<TerrainChunkValuesDTO, int, int> terrainValuesUpdater = (currentTerrainData, i0, length) => {
            if(terrainData.transformingTerrains){
                if(!currentTerrainData.LerpTerrainData(terrainData, targetTerrainData, i0, length)){
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
    
        GameObject go = new GameObject();
        MeshFilter meshFilter = go.GetOrAddComponent<MeshFilter>();
        MeshRenderer meshRenderer = go.GetOrAddComponent<MeshRenderer>();

        Mesh mesh = meshData.CreateMesh();

        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = terrainData.meshMaterial;

        Dictionary<Vector3, Quaternion> plantPositions = EnvironmentGenerator.GeneratePlantPositions(heightMap);

        foreach (var item in plantPositions)
        {
            GameObject plant = Instantiate(plants[Mathf.RoundToInt(new System.Random().Next(0, 1000) / 1000 * 3)], item.Key, item.Value);
            plant.GetOrAddComponent<MeshRenderer>().sharedMaterial = surfacePlantMaterial;
            plant.transform.parent = go.transform;
        }

        return go;
    }
}
