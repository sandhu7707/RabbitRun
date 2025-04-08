using System;
using UnityEngine;

[CreateAssetMenu()]
public class EnvironmentData : UpdatableData
{
    [Range(0,5)]
    public int grassDensity;
    public GameObject grassPrefab;
    public bool generateGrass;

}
