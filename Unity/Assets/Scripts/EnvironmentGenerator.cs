using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class EnvironmentGenerator{
    public static CosmeticPlantPositions GenerateCosmeticPlantsPositions(float[,] heightMap, int cosmeticPlantStepx, int cosmeticPlantStepz){
        
        Dictionary<Vector3, Quaternion> plantPositions = new();
        
        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);

        System.Random randomGenerator = new System.Random(10);

        int x0 = 0;
        int x1 = cosmeticPlantStepx;

        while(x1 < length-1){
            int z0 = 0;
            int z1 = cosmeticPlantStepz;
            while (z1 < width-1){
                int x = randomGenerator.Next(x0+1, x1);
                int z = randomGenerator.Next(z0+1, z1);

                Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(new Vector3(1, heightMap[x+1,z] - heightMap[x,z], 0), new Vector3(0, heightMap[x, z+1] - heightMap[x,z], 1)));
                plantPositions.Add(new Vector3(x, heightMap[x,z], z), rotation);

                z0 += cosmeticPlantStepz;
                z1 += cosmeticPlantStepz;
            }

            x0 += cosmeticPlantStepx;
            x1 += cosmeticPlantStepx;
        }

        return new CosmeticPlantPositions(plantPositions);
    }


    public static List<PlantPatch> GeneratePlantPatchData(float[,] heightMap, float minPlantPatchDensity, float maxPlantPatchDensity, int plantPatchStepZ){

        List<PlantPatch> plantPatches = new();

        int length = heightMap.GetLength(0);
        int width = heightMap.GetLength(1);

        System.Random randomGenerator = new System.Random(0);

        int x0 = 0;

        while(length-1 - x0 > 10){

            int x1 = randomGenerator.Next(x0, length-1);
            
            int z0 = randomGenerator.Next(0, plantPatchStepZ);
            int z1 = randomGenerator.Next(z0, z0+plantPatchStepZ);
            
            while(width-1 > z1) {
                int patchLength = x1-x0;
                int patchWidth = z1-z0;
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

                float patchDensity = maxPlantPatchDensity;
                Dictionary<Vector3, Quaternion> plantPositions = new();

                for(float i0=x0; i0<x1; i0+=patchDensity){
                    for(float i1=z0; i1<z1; i1+=patchDensity){
                        float distFromCenter = Vector2.Distance(new Vector2(i0,i1), new Vector2(x0 + patchLength/2, z0 + patchWidth/2));
                        patchDensity = minPlantPatchDensity + (maxPlantPatchDensity - minPlantPatchDensity)*(2*distFromCenter/Mathf.Max(patchLength, patchWidth));
                        int i0_ = Mathf.Clamp(Mathf.FloorToInt(i0), x0, x0+patchLength-1);
                        int i1_ = Mathf.Clamp(Mathf.FloorToInt(i1), z0, z0+patchWidth-1);
                        Vector3 position = new Vector3(i0, (1-i1%1)*heightMap[i0_,i1_] + (i1%1)*heightMap[i0_, i1_+1], i1);
                        Quaternion orientation = Quaternion.LookRotation(new Vector3(1, ((1-i1%1)*heightMap[i0_,i1_+1] + (i1%1)*heightMap[i0_, i1_+2])-((1-i1%1)*heightMap[i0_,i1_] + (i1%1)*heightMap[i0_, i1_+1]), 1));
                        
                        if(xPatchMap[i0_-x0,i1_-z0] && zPatchMap[i0_-x0,i1_-z0]){ 
                            plantPositions.Add(position, orientation);
                        }
                    }
                }

                PlantPatch plantPatch = new PlantPatch(plantPositions);
                    
                plantPatches.Add(plantPatch);
                z0 = z1;
                z1 = randomGenerator.Next(z0, z0+plantPatchStepZ);
            }

                x0 = randomGenerator.Next(x1, length-1);
        }

        return plantPatches;
    }

    //int difficulty -> 1 to 4 -> no. of occupied lanes
    public static ObstaclesData GenerateObstaclesData(float[,] heightMap, TrackValues trackValues, int moveSpeed, bool firstBlock, int difficulty){
        
        Dictionary<Vector3, Quaternion> positions = new Dictionary<Vector3, Quaternion>();
        int length = heightMap.GetLength(0);
        float incrementX = Mathf.Min(trackValues.minGapFactor, 1/(float)difficulty)*moveSpeed;
        float i0 = firstBlock ? 5*incrementX : incrementX;
        
        int lanesCount = (trackValues.pathEndZ - trackValues.pathStartZ )/ trackValues.laneWidth + 1;

        int emptyLane = new System.Random().Next(0,lanesCount-1);
        int emptyLanesCount = lanesCount-difficulty;

        while(i0 < length-1){
            List<int> emptyLanes = new List<int>();
            emptyLanes.Add(emptyLane);
            for(int e0=1; e0<emptyLanesCount; e0++){
                int rand = Mathf.FloorToInt((float)(new System.Random().NextDouble())*(emptyLanesCount-e0+1));
                while(emptyLanes.Contains(rand)){
                    rand++;
                }
                emptyLanes.Add(rand);
            }
            // int z = trackValues.pathStartZ + trackValues.laneWidth/2 + emptyLane*trackValues.laneWidth;
            for(int i00=0; i00<lanesCount; i00++){
                if(emptyLane != i00 && !emptyLanes.Contains(i00)){
                    int z = trackValues.pathStartZ + i00* trackValues.laneWidth;
                    int _i0 = (int)Math.Floor(i0);
                    Vector3 position = new Vector3(i0 - (float)new System.Random().NextDouble()*incrementX, heightMap[_i0,z], z);
                    Debug.Log("i0: " + i0 + ", z: " + z + ", length: " + length);
                    Quaternion rotation = Quaternion.identity;
                    
                    positions.Add(position, rotation);
                }
            }
                    Debug.Log("emptyLane: " + emptyLane);
                    emptyLane += emptyLane == 0 ? 1 : emptyLane == 4 ? -1 : new System.Random().Next(-10000, 10000) < 0 ? -1 : +1;
                    i0 += incrementX;
        }

        return new ObstaclesData(positions);
    }
}

public class ObstaclesData{
    Dictionary<Vector3, Quaternion> obstaclePositions;

    public ObstaclesData(Dictionary<Vector3, Quaternion> obstaclePositions){
        this.obstaclePositions = obstaclePositions;
    }

    public GameObject CreateObstacles(){
        GameObject plantPatch = new GameObject();
        foreach (var item in obstaclePositions)
        {   
            Vector3 position = item.Key;
            Quaternion orientation = item.Value;
                

            orientation = Quaternion.Euler(0, new System.Random().Next(-180,180), new System.Random().Next(-180,180));
            GameObject smallPlant = MonoBehaviour.Instantiate((GameObject)Resources.Load(new System.Random().Next(-10000, 10000) > 0 ? "Test Objects/Big Bones Prefab" : "Test Objects/Rock Prefab"), position, orientation);
            // smallPlant.transform.rotation = Quaternion.Euler(0,new System.Random().Next(-180,180),0);    
            smallPlant.transform.parent = plantPatch.transform;
            // smallPlant.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Materials/Surface Plant Mat");
        }

        return plantPatch;
        
    }
}

public class CosmeticPlantPositions{
    Dictionary<Vector3, Quaternion> plantPositions;

    public CosmeticPlantPositions(Dictionary<Vector3, Quaternion> plantPositions){
        this.plantPositions = plantPositions;
    }

    public GameObject CreateCosmeticPlants(){
        GameObject plantPatch = new GameObject();
        foreach (var item in plantPositions)
        {   
            Vector3 position = item.Key;
            Quaternion orientation = item.Value;
                

            orientation = Quaternion.Euler(orientation.eulerAngles.x, new System.Random().Next(-180,180), orientation.eulerAngles.z);
            GameObject smallPlant = MonoBehaviour.Instantiate((GameObject)Resources.Load("Test Objects/cosmetic plant prefab"), position, orientation);
            // smallPlant.transform.rotation = Quaternion.Euler(0,new System.Random().Next(-180,180),0);    
            smallPlant.transform.parent = plantPatch.transform;
            // smallPlant.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Materials/Surface Plant Mat");
        }

        return plantPatch;
    }
}

public class PlantPatch{
    Dictionary<Vector3, Quaternion> plantPositions;

    public PlantPatch(Dictionary<Vector3, Quaternion> plantPositions){
        this.plantPositions = plantPositions;
    }

    public GameObject CreatePlantPatch(){
        GameObject plantPatch = new GameObject();
        foreach (var item in plantPositions)
        {   
            Vector3 position = item.Key;
            Quaternion orientation = item.Value;

            orientation = Quaternion.Euler(orientation.eulerAngles.x, new System.Random().Next(-180,180), orientation.eulerAngles.z);
            GameObject smallPlant = MonoBehaviour.Instantiate((GameObject)Resources.Load("Terrain/Surface Plants/plants_" + new System.Random().Next(1,4)), position, orientation);
            // smallPlant.transform.rotation = Quaternion.Euler(0,,0);    
            smallPlant.transform.parent = plantPatch.transform;
            smallPlant.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Materials/Surface Plant Mat");
        }

        return plantPatch;
    }
}
