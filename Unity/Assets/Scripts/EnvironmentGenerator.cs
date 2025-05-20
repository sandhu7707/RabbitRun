using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Unity.Mathematics;
// using System.Numerics;

public class EnvironmentGenerator
{
    public static Dictionary<Vector3, Quaternion> GeneratePlantPositions(float[,] heightMap)
    {
        Dictionary<Vector3, Quaternion> positions = new Dictionary<Vector3, Quaternion>();
        float minIncrement = 10;
        float maxIncrement = 50;
        int length = heightMap.GetLength(0);
        int width = 550;

        float x0 = 0;

        float currentIncrement = minIncrement;

        while (x0 < length - 10)
        {
            float x1 = (float)new System.Random().Next((int)x0, length - 1);
            float z1 = 250;

            float z0 = (float)new System.Random().Next((int)z1, width - 1);

            while (z0 < width - 10)
            {
                z1 = (float)new System.Random().Next((int)z0, width - 1);

                for (float i0 = x0; i0 < x1; i0 = i0 + currentIncrement)
                {

                    float startZ = z0 + (z1 - z0) / 4 * (float)new System.Random().NextDouble();
                    float endZ = z1 - (z1 - z0) / 4 * (float)new System.Random().NextDouble();

                    for (float i1 = startZ; i1 < endZ; i1 = i1 + currentIncrement)
                    {
                        float distance = Vector2.Distance(new Vector2(i0, i1), new Vector2((x0 + x1) / 2, (startZ + endZ) / 2));
                        float maxDistance = Mathf.Max(
                            Vector2.Distance(new Vector2((x0 + x1) / 2, (startZ + endZ) / 2), new Vector2(x0, startZ)),
                            Vector2.Distance(new Vector2((x0 + x1) / 2, (startZ + endZ) / 2), new Vector2(x1, endZ))
                        );

                        currentIncrement = minIncrement + (maxIncrement - minIncrement) * distance / maxDistance;

                        Vector3 position = new Vector3(i0, heightMap[Mathf.RoundToInt(i0), Mathf.RoundToInt(i1)], i1);
                        Quaternion orientation = Quaternion.identity;

                        positions.Add(position, orientation);
                    }
                }
                z0 = z1;
            }
            x0 = x1;
        }

        return positions;
    }

    public static Dictionary<Vector3, Quaternion> GenerateObstaclePositions(float[,] heightMap, int moveSpeed, int difficulty)
    {
        Dictionary<Vector3, Quaternion> positions = new Dictionary<Vector3, Quaternion>();

        int lanesStart = 350;
        int laneWidth = 20;
        float gap = 1.5f*moveSpeed / difficulty;

        int emptyLanesCount = Mathf.Max(2,Mathf.Min(5 - difficulty, 3));

        int blockLength = heightMap.GetLength(0);

        int solutionLane = Mathf.RoundToInt((float)new System.Random().NextDouble() * 4);

        Debug.Log("EnvironmentGenerator: emptyLanes: " + emptyLanesCount + " ,difficulty: " + difficulty);
        for (float i0 = 0; i0 < blockLength - gap/2; i0 += gap)
        {
            bool[] laneUsed = new bool[5];
            for (int i1 = 0; i1 < 5 - emptyLanesCount;)
            {
                int randomLane = Mathf.RoundToInt((float)new System.Random().NextDouble() * 4);
                if (randomLane != solutionLane && !laneUsed[randomLane])
                {
                    laneUsed[randomLane] = true;
                    positions.Add(
                        new Vector3(i0, heightMap[Mathf.RoundToInt(i0), lanesStart + laneWidth * randomLane], lanesStart + laneWidth * randomLane),
                        Quaternion.Euler(new Vector3(360 * (float)new System.Random().NextDouble(), 360 * (float)new System.Random().NextDouble(), 360 * (float)new System.Random().NextDouble())));

                    i1++;
                }
            }

            int offsetPrimary = solutionLane == 4 ? -1 : solutionLane == 0 ? 1 : new System.Random().Next(-10000, 10000) > 0 ? 1 : -1;
            solutionLane = solutionLane + offsetPrimary;
        }

        return positions;
    }

}
