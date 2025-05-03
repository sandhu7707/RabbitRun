using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TerrainData", menuName = "Scriptable Objects/TerrainData")]
public class TerrainChunkValues : ScriptableObject
{
    public int width;
    public int scale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public int lacunarity;
    public Vector2 offset;
    public AnimationCurve heightBase;
    public Material meshMaterial;

    public UnityEvent valuesUpdated;
    public float minPlantPatchDensity;
    public float maxPlantPatchDensity;

    public int cosmeticPlantStepx;
    public int cosmeticPlantStepz;
    public int plantPatchStepZ;

    // void OnValidate()
    // {
        
    //     valuesUpdated.Invoke();
    //     Debug.Log("onValidate in terrainData");
    // }
}
