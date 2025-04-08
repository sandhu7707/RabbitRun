using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration: MonoBehaviour {
    TerrainGenerator terrainGenerator;
    public TerrainData terrainData;
    public TextureData textureData;
    // GameObject oldBlock;
    // GameObject newBlock;
    Queue<GameObject> blocks;
    GameObject lastBlock;
    Vector2 offset;
    bool requested;

    void Start()
    {
        blocks = new Queue<GameObject>();
        terrainGenerator = GetComponent<TerrainGenerator>();
        offset = new Vector2(0,0);

        GenerateInitialBlocks();

    }

    void Update()
    {
        foreach (var block in blocks)
        {
            block.transform.Translate(new Vector3(-terrainData.moveSpeed*Time.deltaTime, 0, 0));
        }

        if(lastBlock != null && lastBlock.transform.position.x < terrainData.createNewThreshold){
            RequestTerrain();
        }

        // if(newBlock != null){
        //     newBlock.transform.Translate(new Vector3(-terrainData.moveSpeed*Time.deltaTime, 0, 0));
        //     if(newBlock.transform.position.x < -terrainData.height+terrainData.createNewThreshold){
        //         RequestTerrain();
        //     }
        // }

        // if(oldBlock != null){
        //     oldBlock.transform.Translate(new Vector3(-terrainData.moveSpeed*Time.deltaTime, 0, 0));
        // }

    }

    void GenerateInitialBlocks(){
        RequestTerrain();
        // RequestTerrain(true);
        // RequestTerrain(true);     
    }

    void RequestTerrain(bool initialBlocks = false){
        if(!requested || initialBlocks){
            offset += (terrainData.height-1)*(new Vector2(0,1));
            requested = true;   
            terrainGenerator.RequestTerrain(OnTerrainReceived, offset);
        }
    }

    void OnTerrainReceived(TerrainInfo terrainInfo){
        GameObject gameObject = new GameObject();
        MeshGenerator.MeshData meshData = terrainInfo.meshData;
        EnvironmentGenerator.EnvironmentInfo environmentInfo = terrainInfo.environmentInfo;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        TerrainCollisions terrainCollisions = gameObject.AddComponent<TerrainCollisions>();
        terrainCollisions.meshData = meshData;
        gameObject.tag = "Jumpable";
        Material terrainMaterial = Resources.Load<Material>("Materials/Mesh Mat");
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
        meshRenderer.material = terrainMaterial;
        textureData.ApplyToMaterial(terrainMaterial);
        Mesh mesh = meshData.CreateMesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        if(lastBlock != null) {
            // Destroy(oldBlock);
            gameObject.transform.position = lastBlock.transform.position + (terrainData.height-1)*(new Vector3(1,0,0));

            // gameObject.transform.position = newBlock.transform.position + (terrainData.height-1)*(new Vector3(1,0,0));
        } else {
            gameObject.transform.position = (terrainData.height-1)*(new Vector3(-1,0,0));
        }

        if(blocks.Count > 3){
            Destroy(blocks.Dequeue()); 
        }

        GameObject environmet = environmentInfo.CreateEnvironment(gameObject.transform.position);
        environmet.transform.parent = gameObject.transform;
        lastBlock = gameObject;
        blocks.Enqueue(gameObject);
        // oldBlock = newBlock;
        // newBlock = gameObject;
        requested = false;

    }

}