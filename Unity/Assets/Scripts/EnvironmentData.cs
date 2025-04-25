using System.Collections.Generic;
using UnityEngine;

public class EnvironmentData{
    public Dictionary<EnvironmentObjectType, List<Vector3>> objectPositions;
    public EnvironmentData(Dictionary<EnvironmentObjectType, List<Vector3>> objectPositions){
        this.objectPositions = objectPositions;
    }
}

public enum EnvironmentObjectType{
    AVOIDABLE_OBSTACLES,
    JUMPABLE_OBSTACLES,
    POWERUPS,
    PLANT_PATCHES,
    FLYING_OBSTACLES,
    TREES
}