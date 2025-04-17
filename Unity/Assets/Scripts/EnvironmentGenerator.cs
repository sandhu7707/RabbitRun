using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Unity.Android.Gradle.Manifest;

public class EnvironmentGenerator{
    public static GameObject GenerateEnvironment(float[,] heightMap, int obstaclesDensity, float powerUpsDensityFactor, GameObject player, GameObject jumpableObstaclePrefab, GameObject avoidableObstaclePrefab, GameObject powerUpPrefab, float minObstacleGap, MeshData meshData){
        GameObject environment = new GameObject();
        
        int avoidableObstaclesDensity = Mathf.RoundToInt(obstaclesDensity*((float)new System.Random().NextDouble())*0.75f);
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
        
        GameObject plantPatches = GenerateSmallPlantPatches(heightMap, meshData);
        plantPatches.transform.parent = environment.transform;

        return environment;
    }

    static GameObject GenerateSmallPlantPatches(float[,] heightMap, MeshData meshData){
        GameObject plantPatches = new GameObject();
        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);

        int patchDensity = 1;
        int patchesPerBlock = 3;

        int minLength = 10;
        int maxLength = 30;
        int minWidth = 10;
        int maxWidth = 30;

        int lengthIncrement = (int)Mathf.Floor((length-maxLength-1)/patchesPerBlock);

        int patchLength = new System.Random().Next(minLength, maxLength);
        int patchWidth = new System.Random().Next(minWidth, maxWidth);

        for(int i0=0; i0+lengthIncrement<length; i0+=lengthIncrement){
            int xStart = new System.Random().Next(i0, i0+lengthIncrement);
            int zStart = new System.Random().Next(96, 190-maxWidth-1);
            while(zStart < 153 && zStart > 117){
                zStart = new System.Random().Next(96, 190-maxWidth-1);
            }
            GameObject plantPatch = GenerateSmallPlantPatch(xStart, zStart, patchLength, patchWidth, patchDensity, heightMap, meshData);
            plantPatch.transform.parent = plantPatches.transform;
        }

        return plantPatches;
    }

    static GameObject GenerateSmallPlantPatch(int xStart, int zStart, int patchLength, int patchWidth, int patchDensity, float[,] heightMap, MeshData meshData){
        GameObject plantPatch = new GameObject();

        int variance=4;

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


        
        for(int i0=xStart; i0<xStart+patchLength; i0+=patchDensity){
            for(int i1=zStart; i1<zStart + patchWidth; i1+=patchDensity){
                Vector3 position = new Vector3(i0, heightMap[i0,i1], i1);
                Quaternion orientation = Quaternion.LookRotation(new Vector3(i0+1, heightMap[i0+1, i1+1], i1+1) - position);
                if(xPatchMap[i0-xStart,i1-zStart] && zPatchMap[i0-xStart,i1-zStart]){
                    GameObject smallPlant = MonoBehaviour.Instantiate((GameObject)Resources.Load("Test Objects/Small Surface Plant"), new Vector3(i0, heightMap[i0,i1], i1), orientation);
                    smallPlant.transform.parent = plantPatch.transform;
                }
            }
        }

        return plantPatch;
    }

    static GameObject CreateGameObjectAtPositions(List<Vector3> positions, GameObject prefab){
        GameObject go = new GameObject();

        foreach (var item in positions)
        {
            GameObject go1 = MonoBehaviour.Instantiate(prefab, item, Quaternion.identity);
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