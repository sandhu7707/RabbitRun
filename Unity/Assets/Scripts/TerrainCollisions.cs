using System.Linq;
using UnityEngine;

public class TerrainCollisions: MonoBehaviour{

    GameObject frontFootImpact;
    GameObject backFootImpact;
    ParticleSystem groundParticles;
    public int offset;
    
    void Start()
    {
        frontFootImpact = Resources.Load<GameObject>("Terrain/FrontFootSandDisplaced");
        backFootImpact = Resources.Load<GameObject>("Terrain/BackFootSandDisplaced");
        groundParticles = Resources.Load<ParticleSystem>("Test Objects/Ground Particles");
    }
    
    void OnCollisionEnter(Collision collider){
        Vector3 collisionPoint = new Vector3(collider.contacts.Average(it => it.point.x),collider.contacts.Average(it => it.point.y),collider.contacts.Average(it => it.point.z)); 
        
        ParticleSystem goPs = Instantiate(groundParticles, collisionPoint, Quaternion.Euler(90,90,0));
        goPs.Play();
        goPs.transform.parent = transform;
        
        if(collider.collider.name.Substring(0,4).Equals("foot")){
            GameObject go = Instantiate(backFootImpact, collisionPoint, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(new Vector3(go.transform.rotation.x, go.transform.rotation.y, go.transform.rotation.z));
            go.transform.parent = transform;
        }
        if(collider.collider.name.Substring(0,4).Equals("hand")){
            GameObject go = Instantiate(frontFootImpact, collisionPoint, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(new Vector3(go.transform.rotation.x, go.transform.rotation.y, go.transform.rotation.z));
            go.transform.parent = transform;
        }
    }

    void OnCollisionStay(Collision collider){
        Vector3 collisionPoint = new Vector3(collider.contacts.Average(it => it.point.x),collider.contacts.Average(it => it.point.y),collider.contacts.Average(it => it.point.z)); 

        if(collider.collider.name.Substring(0,4).Equals("foot")){
            GameObject go = Instantiate(backFootImpact, collisionPoint, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(new Vector3(go.transform.rotation.x, go.transform.rotation.y, go.transform.rotation.z));
            go.transform.parent = transform;
        }
        if(collider.collider.name.Substring(0,4).Equals("hand")){
            GameObject go = Instantiate(frontFootImpact, collisionPoint, Quaternion.identity);
            go.transform.rotation = Quaternion.Euler(new Vector3(go.transform.rotation.x, go.transform.rotation.y, go.transform.rotation.z));
            go.transform.parent = transform;
        }
    }

}