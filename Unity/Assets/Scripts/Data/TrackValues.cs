using UnityEngine;

[CreateAssetMenu(fileName = "TrackValues", menuName = "Scriptable Objects/TrackValues")]
public class TrackValues : ScriptableObject
{
    public int pathStartZ;
    public int pathEndZ;
    public int laneWidth;
    public float minGapFactor;
}
