using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public int width;
    public int height;
    public float heightMultiplier;
    public AnimationCurve heightAcrossZ;
    public float moveSpeed;
    public float createNewThreshold;

    public float minHeight {
        get {
            return heightMultiplier*heightAcrossZ.Evaluate(0);
        }
    }

    public float maxHeight {
        get {
            return heightMultiplier*heightAcrossZ.Evaluate(width);
        }
    }
}
