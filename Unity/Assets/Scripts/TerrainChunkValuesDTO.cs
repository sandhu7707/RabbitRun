using UnityEngine;

public class TerrainChunkValuesDTO{


    public int width;
    public int scale;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public int lacunarity;
    public Vector2 offset;
    public AnimationCurve heightBase;
    public Material meshMaterial;

    public float minPlantPatchDensity;
    public float maxPlantPatchDensity;

    public int cosmeticPlantStepx;
    public int cosmeticPlantStepz;
    public int plantPatchStepZ;

    public float heightMultiplier;
    public float targetHeightCap;
    public float acceptablePersistanceError = 0.005f;
    public bool transformingTerrains = false;
    public int maxHeight;
    public int minHeight;
    public int pathLength;

    public TerrainChunkValuesDTO(TerrainChunkValues terrainChunkValues)
    {
        this.scale = terrainChunkValues.scale;
        this.octaves = terrainChunkValues.octaves;
        this.persistance = terrainChunkValues.persistance;
        this.lacunarity = terrainChunkValues.lacunarity;
        this.meshMaterial = terrainChunkValues.meshMaterial;
        this.heightMultiplier = terrainChunkValues.heightMultiplier;

        this.width = terrainChunkValues.width;
        this.heightBase = terrainChunkValues.heightBase;
        this.minPlantPatchDensity = terrainChunkValues.minPlantPatchDensity;
        this.maxPlantPatchDensity = terrainChunkValues.maxPlantPatchDensity;
        this.cosmeticPlantStepx = terrainChunkValues.cosmeticPlantStepx;
        this.cosmeticPlantStepz = terrainChunkValues.cosmeticPlantStepz;
        this.plantPatchStepZ = terrainChunkValues.plantPatchStepZ;
        this.maxHeight = terrainChunkValues.maxHeight;
        this.minHeight = terrainChunkValues.minHeight;
        this.pathLength = terrainChunkValues.pathLength;
    }

    public TerrainChunkValuesDTO(TerrainChunkValuesDTO terrainChunkValues){
        this.scale = terrainChunkValues.scale;
        this.octaves = terrainChunkValues.octaves;
        this.persistance = terrainChunkValues.persistance;
        this.lacunarity = terrainChunkValues.lacunarity;
        this.meshMaterial = terrainChunkValues.meshMaterial;
        this.heightMultiplier = terrainChunkValues.heightMultiplier;
        this.targetHeightCap = terrainChunkValues.targetHeightCap;

        this.width = terrainChunkValues.width;
        this.offset = terrainChunkValues.offset;
        this.heightBase = terrainChunkValues.heightBase;
        this.minPlantPatchDensity = terrainChunkValues.minPlantPatchDensity;
        this.maxPlantPatchDensity = terrainChunkValues.maxPlantPatchDensity;
        this.cosmeticPlantStepx = terrainChunkValues.cosmeticPlantStepx;
        this.cosmeticPlantStepz = terrainChunkValues.cosmeticPlantStepz;
        this.plantPatchStepZ = terrainChunkValues.plantPatchStepZ;
        this.maxHeight = terrainChunkValues.maxHeight;
        this.minHeight = terrainChunkValues.minHeight;
        this.pathLength = terrainChunkValues.pathLength;
    }

    public bool LerpTerrainData(TerrainChunkValuesDTO originalValues, TerrainChunkValuesDTO targetValues,int i0, int length){

        int scaleIncrementPerBlock = 10;
        int octavesIncrementPerBlock = 1;
        float persistanceIncrementPerBlock = 1f;
        int lacunarityIncrementPerBlock = 3;
        int heightMultiplierIncrementPerBlock = 3;

        bool updated = false;

        if(scale != targetValues.scale){
            scale = originalValues.scale + Mathf.Abs(targetValues.scale-originalValues.scale)/(targetValues.scale-originalValues.scale)*Mathf.RoundToInt(scaleIncrementPerBlock*((float)i0/(length-1)));
            updated = true;
        }
        else if(Mathf.Abs(persistance - targetValues.persistance) > acceptablePersistanceError  && i0>100){
            persistance = originalValues.persistance + Mathf.Abs(targetValues.persistance-originalValues.persistance)/(targetValues.persistance-originalValues.persistance)*persistanceIncrementPerBlock*((float)i0/(length-1));
            if(Mathf.Abs(persistance - targetValues.persistance) < acceptablePersistanceError){
                persistance = targetValues.persistance;
            }
            updated = true;
        }

        if(octaves != targetValues.octaves){
            octaves = originalValues.octaves + Mathf.Abs(targetValues.octaves-originalValues.octaves)/(targetValues.octaves-originalValues.octaves)*Mathf.RoundToInt(octavesIncrementPerBlock*((float)i0/(length-1)));
            updated = true;
        }

        if(lacunarity != targetValues.lacunarity){
            lacunarity = originalValues.lacunarity + Mathf.Abs(targetValues.lacunarity-originalValues.lacunarity)/(targetValues.lacunarity-originalValues.lacunarity)*Mathf.RoundToInt(lacunarityIncrementPerBlock*((float)i0/(length-1)));
            updated = true;
        }

        if(Mathf.Abs(heightMultiplier - targetValues.heightMultiplier) > 1f){
            heightMultiplier = originalValues.heightMultiplier + Mathf.Abs(targetValues.heightMultiplier-originalValues.heightMultiplier)/(targetValues.heightMultiplier-originalValues.heightMultiplier)*Mathf.RoundToInt(heightMultiplierIncrementPerBlock*((float)i0/(length)));
            updated = true;
        }

        return updated;
    }
}