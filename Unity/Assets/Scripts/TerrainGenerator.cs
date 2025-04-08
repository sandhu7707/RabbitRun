using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainGenerator: MonoBehaviour{

    public NoiseData noiseData;
    public TerrainData terrainData;
    public TextureData textureData;
    public GameObject testGameObject;
    public EnvironmentData environmentData;
    GameObject environment;
    Queue<ThreadInfo> generatedMeshDataList = new Queue<ThreadInfo>();

    void OnValuesUpdated(){
        if(!Application.isPlaying){
            GenerateTerrainEditor();
        }
    }

    void OnValidate()
    {
        if(terrainData != null){
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if(noiseData != null){
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if(textureData != null){
            textureData.OnValuesUpdated -= OnValuesUpdated;
            textureData.OnValuesUpdated += OnValuesUpdated;
        }
    }

    public void GenerateTerrainEditor(){
        float[,] noiseMap = NoiseGenerator.GenerateNoise(terrainData.width+2*noiseData.lod, terrainData.height+2*noiseData.lod, noiseData.scale, noiseData.persistence, noiseData.octaves, noiseData.offset, noiseData.seed, terrainData.heightAcrossZ, noiseData.lacunarity, terrainData.heightMultiplier);

        MeshFilter meshFilter = testGameObject.GetOrAddComponent<MeshFilter>();
        MeshRenderer meshRenderer = testGameObject.GetOrAddComponent<MeshRenderer>();

        Material terrainMaterial = Resources.Load<Material>("Materials/Mesh Mat");
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        meshRenderer.material = terrainMaterial;

        // Texture2D texture = NoiseGenerator.GenerateTexture(noiseMap);
        // meshRenderer.sharedMaterial.mainTexture = texture;  

        textureData.ApplyToMaterial(terrainMaterial);
        MeshGenerator.MeshData meshData = MeshGenerator.GenerateMeshData(noiseMap, noiseData.lod);
        meshFilter.mesh = meshData.CreateMesh();

        EnvironmentGenerator.EnvironmentInfo environmentInfo = EnvironmentGenerator.GenerateEnvironmentData(meshData, environmentData, noiseData.lod);
        DestroyImmediate(environment);
        environment = environmentInfo.CreateEnvironment(testGameObject.transform.position);
        environment.transform.parent = testGameObject.transform;
        
    }

    public void RequestTerrain(Action<TerrainInfo> callback, Vector2 offset){
        
        ThreadStart threadStart = delegate{
            AnimationCurve animationCurve = new AnimationCurve(terrainData.heightAcrossZ.keys);
            float[,] noiseMap = NoiseGenerator.GenerateNoise(terrainData.width+2*noiseData.lod, terrainData.height+2*noiseData.lod, noiseData.scale, noiseData.persistence, noiseData.octaves, offset, noiseData.seed, animationCurve, noiseData.lacunarity, terrainData.heightMultiplier);
            MeshGenerator.MeshData meshData = MeshGenerator.GenerateMeshData(noiseMap, noiseData.lod);
            EnvironmentGenerator.EnvironmentInfo environmentInfo = EnvironmentGenerator.GenerateEnvironmentData(meshData, environmentData, noiseData.lod);
            TerrainInfo terrainInfo = new TerrainInfo(meshData, environmentInfo);
            lock(generatedMeshDataList){
                generatedMeshDataList.Enqueue(new ThreadInfo(terrainInfo, callback));
            }
        };

        new Thread(threadStart).Start();
    }
    void Update()
    {
        while(generatedMeshDataList.Count > 0)
        {
            ThreadInfo item = generatedMeshDataList.Dequeue();
            item.callback(item.terrainInfo);
        }
    }

    public class ThreadInfo{
        public TerrainInfo terrainInfo;
        public Action<TerrainInfo> callback;
        public ThreadInfo(TerrainInfo terrainInfo, Action<TerrainInfo> callback){
            this.terrainInfo = terrainInfo;
            this.callback = callback;
        }
    }
}

public struct TerrainInfo{

    public readonly MeshGenerator.MeshData meshData;
    public readonly EnvironmentGenerator.EnvironmentInfo environmentInfo;
    public TerrainInfo(MeshGenerator.MeshData meshData, EnvironmentGenerator.EnvironmentInfo environmentInfo){
        this.meshData = meshData;
        this.environmentInfo = environmentInfo;    
    }
}