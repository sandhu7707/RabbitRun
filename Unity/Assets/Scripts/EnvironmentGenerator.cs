using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class EnvironmentGenerator{

    public static EnvironmentData GenerateEnvironmentData(float[,] heightMap, int obstaclesDensity, float powerUpsDensityFactor, float minObstacleGap, MeshData meshData, Vector3 playerPosition, int maxHeight){
        int avoidableObstaclesDensity = Mathf.RoundToInt(obstaclesDensity*((float)new System.Random().NextDouble())*0.75f);
        int jumpableObstaclesDensity = obstaclesDensity-avoidableObstaclesDensity;

        List<Vector3> jumpableObstaclePositions = GeneratePositions(heightMap, jumpableObstaclesDensity, new(), minObstacleGap, false, playerPosition, maxHeight, false);
        List<Vector3> avoidableObstaclePositions = avoidableObstaclesDensity == 0 ? new() : GeneratePositions(heightMap, avoidableObstaclesDensity, jumpableObstaclePositions, minObstacleGap, false, playerPosition, maxHeight, false);
        List<Vector3> powerUpPositions = GeneratePositions(heightMap, obstaclesDensity*powerUpsDensityFactor, new(), 0, true, playerPosition, maxHeight, false);
        
        int minPatchLength = 40;
        int maxPatchLength = 70;
        int minPatchWidth = 40;
        int maxPatchWidth = 70;
        int patchesPerBlock = 2;
        List<Vector3> plantPatchesPositions = GenerateSmallPlantPatchesPositions(heightMap, meshData, patchesPerBlock, minPatchLength, maxPatchLength, minPatchWidth, maxPatchWidth);
        List<Vector3> flyingObstaclesPositions = GeneratePositions(heightMap, obstaclesDensity*powerUpsDensityFactor, avoidableObstaclePositions, 0, true, playerPosition, maxHeight, true);
        List<Vector3> treesPositions = GeneratePositions(heightMap, 2, new(), 20, false, playerPosition, maxHeight, true);
        
        return new EnvironmentData(new Dictionary<EnvironmentObjectType, List<Vector3>>(){
            {EnvironmentObjectType.AVOIDABLE_OBSTACLES, avoidableObstaclePositions},
            {EnvironmentObjectType.JUMPABLE_OBSTACLES, jumpableObstaclePositions},
            {EnvironmentObjectType.POWERUPS, powerUpPositions},
            {EnvironmentObjectType.PLANT_PATCHES, plantPatchesPositions},
            {EnvironmentObjectType.FLYING_OBSTACLES, flyingObstaclesPositions},
            {EnvironmentObjectType.TREES, treesPositions}
        });
    }
    public static GameObject GenerateEnvironment(EnvironmentData environmentData, float[,] heightMap, int obstaclesDensity, float powerUpsDensityFactor, GameObject player, GameObject jumpableObstaclePrefab, GameObject avoidableObstaclePrefab, GameObject powerUpPrefab, GameObject flyingObjectPrefab, float minObstacleGap, MeshData meshData, GameObject playerPosition){
        GameObject environment = new GameObject();
        Dictionary<EnvironmentObjectType, List<Vector3>> positions = environmentData.objectPositions;

        int avoidableObstaclesDensity = Mathf.RoundToInt(obstaclesDensity*((float)new System.Random().NextDouble())*0.75f);
        int jumpableObstaclesDensity = obstaclesDensity-avoidableObstaclesDensity;

        List<Vector3> jumpableObstaclePositions = positions[EnvironmentObjectType.JUMPABLE_OBSTACLES];
        GameObject jumpableObstacles = CreateGameObjectAtPositions(jumpableObstaclePositions, jumpableObstaclePrefab, heightMap);
        jumpableObstacles.transform.parent = environment.transform;
        
        List<Vector3> avoidableObstaclePositions = positions[EnvironmentObjectType.AVOIDABLE_OBSTACLES];
        GameObject avoidableObstacles = CreateGameObjectAtPositions(avoidableObstaclePositions, avoidableObstaclePrefab, heightMap);
        avoidableObstacles.transform.parent = environment.transform;

        List<Vector3> powerUpPositions = positions[EnvironmentObjectType.POWERUPS];
        GameObject powerUps = CreateGameObjectAtPositions(powerUpPositions, powerUpPrefab, heightMap);
        powerUps.transform.parent = environment.transform;
        
        
        int minPatchLength = 40;
        int maxPatchLength = 70;
        int minPatchWidth = 40;
        int maxPatchWidth = 70;
        int patchesPerBlock = 2;
        List<Vector3> plantPatchesPositions = positions[EnvironmentObjectType.PLANT_PATCHES];
        GameObject plantPatches = GenerateSmallPlantPatchs(plantPatchesPositions, minPatchLength, maxPatchLength, minPatchWidth, maxPatchWidth, heightMap, meshData);
        plantPatches.transform.parent = environment.transform;

        List<Vector3> flyingObstaclesPositions = positions[EnvironmentObjectType.FLYING_OBSTACLES];
        GameObject flyingObstacles = CreateGameObjectAtPositions(flyingObstaclesPositions, flyingObjectPrefab, heightMap, (go) => {
            FlyingObject flyingObject = go.AddComponent<FlyingObject>();
            float speedX = -5;
            flyingObject.speedX = speedX;
            flyingObject.player = player;
            flyingObject.playerPosX = playerPosition.transform.position.x;
            flyingObject.triggerX = playerPosition.transform.position.x + 50f;
        });
        flyingObstacles.transform.parent = environment.transform;

        List<Vector3> treesPositions = positions[EnvironmentObjectType.TREES];
        GameObject treePrefab = (GameObject)Resources.Load("Terrain/Trees/tree_1");
        treePrefab.transform.Rotate(Vector3.up, new System.Random().Next(-180,180));
        // treePrefab.AddComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Terrain/Trees/Tree Mat");
        GameObject trees = CreateGameObjectAtPositions(treesPositions, treePrefab, heightMap);
        trees.transform.parent = environment.transform;

        return environment;
    }

    static List<Vector3> GenerateSmallPlantPatchesPositions(float[,] heightMap, MeshData meshData, int patchesPerBlock, int minLength = 10, int maxLength = 30, int minWidth = 10, int maxWidth = 30){
        List<Vector3> positions = new();
        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);



        int lengthIncrement = (int)Mathf.Floor((length-maxLength-1)/patchesPerBlock);


        for(int i0=0; i0+lengthIncrement<length; i0+=lengthIncrement){
            int xStart = new System.Random().Next(i0, i0+lengthIncrement);
            int zStart = new System.Random().Next(96, 190-maxWidth-1);
            // while(zStart < 153 && zStart > 117){
            //     zStart = new System.Random().Next(96, 190-maxWidth-1);
            // }
            
            positions.Add(new Vector3(xStart, 0, zStart));
        }

        return positions;
    }

    static GameObject GenerateSmallPlantPatchs(List<Vector3> positions, int minLength, int maxLength, int minWidth, int maxWidth, float[,] heightMap, MeshData meshData){
        GameObject plantPatches = new GameObject();
        foreach (var item in positions)
        {
            GameObject plantPatch = GenerateSmallPlantPatch((int)item.x, (int)item.z, minLength, maxLength, minWidth, maxWidth, heightMap, meshData);
            plantPatch.transform.parent = plantPatches.transform;
        }
        return plantPatches;
    }

    static GameObject GenerateSmallPlantPatch(int xStart, int zStart,  int minLength, int maxLength, int minWidth, int maxWidth, float[,] heightMap, MeshData meshData){
        GameObject plantPatch = new GameObject();

        int patchLength = new System.Random().Next(minLength, maxLength);
        int patchWidth = new System.Random().Next(minWidth, maxWidth);

        int variance=Mathf.Min(patchLength, patchWidth)/4;

        bool[,] xPatchMap = new bool[patchLength, patchWidth];
        bool[,] zPatchMap = new bool[patchLength, patchWidth];

        for(int i0 = 0; i0<patchLength; i0++){
            int zPatchStart = new System.Random().Next(0,variance);
            int zPatchEnd = new System.Random().Next(patchWidth-1-variance,patchWidth-1);

            for(int i1=zPatchStart; i1<zPatchEnd; i1++){
                xPatchMap[i0,i1] = true;
            }
        }

        for(int i1=0; i1<patchWidth; i1++){
            int xPatchStart = new System.Random().Next(0,variance);;
            int xPatchEnd = new System.Random().Next(patchLength-1-variance,patchLength-1);

            for(int i0=xPatchStart; i0<xPatchEnd; i0++){
                zPatchMap[i0,i1] = true;
            }
        }

        float patchDensity = 1;

        for(float i0=xStart; i0<xStart+patchLength; i0+=patchDensity){
            for(float i1=zStart; i1<zStart + patchWidth; i1+=patchDensity){
                float distFromCenter = Vector2.Distance(new Vector2(i0,i1), new Vector2(xStart + patchLength/2, zStart + patchWidth/2));
                patchDensity = 0.2f + 0.8f*(2*distFromCenter/Mathf.Max(patchLength, patchWidth));
                int i0_ = Mathf.Clamp(Mathf.FloorToInt(i0), xStart, xStart+patchLength-1);
                int i1_ = Mathf.Clamp(Mathf.FloorToInt(i1), zStart, zStart+patchWidth-1);
                Vector3 position = new Vector3(i0, (1-i1%1)*heightMap[i0_,i1_] + (i1%1)*heightMap[i0_, i1_+1], i1);
                Quaternion orientation = Quaternion.LookRotation(new Vector3(1, ((1-i1%1)*heightMap[i0_,i1_+1] + (i1%1)*heightMap[i0_, i1_+2])-((1-i1%1)*heightMap[i0_,i1_] + (i1%1)*heightMap[i0_, i1_+1]), 1));
                
                if(xPatchMap[i0_-xStart,i1_-zStart] && zPatchMap[i0_-xStart,i1_-zStart]){ 
                    GameObject smallPlant = MonoBehaviour.Instantiate((GameObject)Resources.Load("Terrain/Surface Plants/plants_" + new System.Random().Next(1,4)), position, orientation);
                    smallPlant.transform.rotation = Quaternion.Euler(0,new System.Random().Next(-180,180),0);    
                    smallPlant.transform.parent = plantPatch.transform;
                    smallPlant.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Materials/Surface Plant Mat");
                }
            }
        }

        return plantPatch;
    }

    static GameObject CreateGameObjectAtPositions(List<Vector3> positions, GameObject prefab, float[,] heightMap, Action<GameObject> additionalAction = null){
        GameObject go = new GameObject();

        foreach (var item in positions)
        {
            float i0 = item.x;
            float i1 = item.z;
            int i0_ = Mathf.FloorToInt(i0);
            int i1_ = Mathf.FloorToInt(i1);
            Quaternion orientation = Quaternion.LookRotation(new Vector3(1, ((1-i1%1)*heightMap[i0_,i1_+1] + (i1%1)*heightMap[i0_, i1_+2])-((1-i1%1)*heightMap[i0_,i1_] + (i1%1)*heightMap[i0_, i1_+1]), 1));    
            GameObject go1 = MonoBehaviour.Instantiate(prefab, item, orientation);
            if(additionalAction != null){
                additionalAction(go1);
            }
            go1.transform.parent = go.transform;
        }

        return go;
    }

    static List<Vector3> GeneratePositions(float[,] heightMap, float density, List<Vector3> takenPositions, float minGap, bool randomizeY, Vector3 playerPosition, int maxHeight, bool randomizeZ, bool flyingObject = false){
        List<Vector3> obstacles = new();
        List<Vector3> takenPositionEffective = new List<Vector3>(takenPositions);
        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);
        density = density <=0 ? 1: density;
        int increment = Mathf.RoundToInt(length/density);
        // float playerZ = player.transform.position.z;
        int playerZ = width/2; //player is at z=width/2 w.r.t block
        
        for(int i0=0; i0 < length; i0+=increment){
            int x = new System.Random().Next(i0, Mathf.Min(i0+increment, length-1));

            while(Mathf.Abs(playerPosition.x - x) < 10){
                x = new System.Random().Next(i0, Mathf.Min(i0+increment, length-1));
            }

            int z = playerZ;
            if(randomizeZ){
                z = new System.Random().Next(96, 190);
                while(z > playerZ-10 && z < playerZ+10){
                    z = new System.Random().Next(96, 190);
                }
            }

            float y = heightMap[x, z];
            
            if(randomizeY){
                float maxPowerUpsHeight = maxHeight;
                float minPowerUpsHeight = flyingObject ? maxPowerUpsHeight*0.6f : 2f;
                y += Mathf.Lerp(minPowerUpsHeight, maxPowerUpsHeight, (float)new System.Random().NextDouble());
            }



            minGap = flyingObject ? minGap + 6*10 - density*10 : minGap;

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