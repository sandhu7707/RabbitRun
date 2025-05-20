using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[CreateAssetMenu(fileName = "TerrainData", menuName = "Scriptable Objects/TerrainData")]
public class TerrainChunkValues : ScriptableObject
{
    public int width;
    public int scale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public int lacunarity;
    public AnimationCurve heightBase;
    public Material meshMaterial;

    public UnityEvent valuesUpdated;
    public float minPlantPatchDensity;
    public float maxPlantPatchDensity;

    public int cosmeticPlantStepx;
    public int cosmeticPlantStepz;
    public int plantPatchStepZ;

    public float heightMultiplier;
    public int maxHeight;
    public int minHeight;
    public int pathLength;
}