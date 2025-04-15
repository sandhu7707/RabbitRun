using UnityEngine;

public class TerrainCollisions: MonoBehaviour{

    public GameObject frontFootImpact;
    public GameObject backFootImpact;
    
    void Start()
    {
        frontFootImpact = Resources.Load<GameObject>("Terrain/FrontFootSandDisplaced");
        backFootImpact = Resources.Load<GameObject>("Terrain/BackFootSandDisplaced");
    }
    

    void OnCollisionEnter(Collision collision){
        Vector3 collisionPoint = collision.contacts[0].point;
        if(collision.collider.name.Substring(0,4).Equals("hand")){
            GameObject go1 = Instantiate(frontFootImpact, collisionPoint, Quaternion.identity);
            go1.transform.rotation = Quaternion.Euler(new Vector3(go1.transform.rotation.x-90, go1.transform.rotation.y, go1.transform.rotation.z));
            go1.transform.parent = transform;
        }
        else if(collision.collider.name.Substring(0,4).Equals("foot") || collision.collider.name.Equals("Player")){
            GameObject go = Instantiate(backFootImpact, collisionPoint, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(new Vector3(go.transform.rotation.x-90, go.transform.rotation.y, go.transform.rotation.z));
            go.transform.parent = transform;
        }
    }

}