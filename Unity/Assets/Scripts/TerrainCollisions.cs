using System;
using System.Linq;
using UnityEngine;

public class TerrainCollisions: MonoBehaviour{

    GameObject frontFootImpact;
    GameObject backFootImpact;
    ParticleSystem groundParticles;
    public int offset;
    Texture2D texture2D;
    Material material;
    Color[] depthTex;

    void Start()
    {


        frontFootImpact = Resources.Load<GameObject>("Terrain/FrontFootSandDisplaced");
        backFootImpact = Resources.Load<GameObject>("Terrain/BackFootSandDisplaced");
        groundParticles = Resources.Load<ParticleSystem>("Test Objects/Ground Particles");
    }

    void SetUpVars()
    {
        if(depthTex == null){
            depthTex = new Color[180*241];
        }
        if(texture2D == null){
            texture2D = new Texture2D(241, 180);
        }
        if(material == null){
            material = GetComponent<MeshRenderer>().material;
        }
        
        texture2D.SetPixels(depthTex);
        texture2D.Apply();
        material.SetTexture("_Texture2D", texture2D);
        
    }

    void OnCollisionEnter(Collision collider){
        Vector3 collisionPoint = new Vector3(collider.contacts.Average(it => it.point.x),collider.contacts.Average(it => it.point.y),collider.contacts.Average(it => it.point.z)); 
        
        SetUpVars();
        ContactPoint[] contactPoints = collider.contacts;
        for(int  i =0; i < contactPoints.Count(); i++){
            int pointIndex = 241*Mathf.FloorToInt(contactPoints[i].point.z - transform.position.z) + Mathf.FloorToInt(contactPoints[i].point.x - transform.position.x);
            depthTex[ pointIndex ] = Color.red;
            // depthTex[pointIndex - 3] = Color.green;
            // depthTex[pointIndex + 3] = Color.green;
        }
        // texture2D.SetPixels(depthTex);
        // texture2D.Apply();
        // material.SetTexture("_Texture2D", texture2D);


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
        
        ContactPoint[] contactPoints = collider.contacts;
        
        SetUpVars();
        for(int  i =0; i < contactPoints.Count(); i++){
            int pointIndex = 241*Mathf.FloorToInt(contactPoints[i].point.z - transform.position.z) + Mathf.FloorToInt(contactPoints[i].point.x - transform.position.x);
            depthTex[ pointIndex ] = Color.red;
            // depthTex[pointIndex - 3] = Color.green;
            // depthTex[pointIndex + 3] = Color.green;
        }
        // texture2D.SetPixels(depthTex);
        // texture2D.Apply();
        // material.SetTexture("_Texture2D", texture2D);

        print("material" + material);

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