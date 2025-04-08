using UnityEngine;

public class TerrainCollisions: MonoBehaviour{

    public MeshGenerator.MeshData meshData;
    GameObject frontFootImpact;
    GameObject backFootImpact;

    void Start()
    {
        frontFootImpact = Resources.Load<GameObject>("TerrainAssets/FrontFootSandDisplaced");
        backFootImpact = Resources.Load<GameObject>("TerrainAssets/BackFootSandDisplaced");
    }
    

    void OnCollisionEnter(Collision collision){
        Vector3 collisionPoint = collision.contacts[0].point;
        if(collision.collider.name.Substring(0,4).Equals("hand")){
            GameObject go = Instantiate(frontFootImpact, collisionPoint, GetNormalForPosition(collisionPoint));
            go.transform.rotation = Quaternion.Euler(new Vector3(go.transform.rotation.x-90, go.transform.rotation.y, go.transform.rotation.z));
            go.transform.parent = transform;
        }
        else if(collision.collider.name.Substring(0,4).Equals("foot")){
            Vector3 adjustedPosition = collisionPoint;
            // adjustedPosition.y -= -0.15f;
            GameObject go = Instantiate(backFootImpact, collisionPoint, GetNormalForPosition(collisionPoint));
            go.transform.rotation = Quaternion.Euler(new Vector3(go.transform.rotation.x-90, go.transform.rotation.y, go.transform.rotation.z));
            go.transform.parent = transform;
        }
    }

    Quaternion GetNormalForPosition(Vector3 position){
        Vector3 normal = meshData.GetNormalForPosition(position-transform.position);
        return new Quaternion(normal.x, normal.y, normal.z, normal.magnitude);
    }
}