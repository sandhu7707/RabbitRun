using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    public float scale;
    [Range(0,1)]
    public float persistence;
    public int octaves;
    public int seed;
    public float lacunarity;    
    [Range(1,4)]
    public int lod;
    public Vector2 offset;
}
