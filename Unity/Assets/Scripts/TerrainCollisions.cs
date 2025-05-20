
using System;
using UnityEngine;

public class TerrainCollisions : MonoBehaviour
{

    // GameObject footPrint;
    ParticleSystem groundParticles;
    Texture2D texture2D;
    Material material;
    Color[] depthTex;
    public GameObject terrainBlock;

    void Start()
    {
        groundParticles = Resources.Load<ParticleSystem>("Terrain/Ground Particles");
        // footPrint = Resources.Load<GameObject>("Player/footPrintPrefab");
    }

    void OnTriggerEnter(Collider collider)
    {
        Vector3 collisionPoint = collider.ClosestPoint(gameObject.transform.position);
                
        ParticleSystem goPs = Instantiate(groundParticles, collisionPoint, Quaternion.Euler(270,0,0));
        goPs.Play();
        goPs.transform.parent = transform;
    }

    void OnTriggerStay(Collider collider)
    {
        Vector3 collisionPoint = collider.ClosestPoint(gameObject.transform.position);
                
        ParticleSystem goPs = Instantiate(groundParticles, collisionPoint, Quaternion.Euler(270,0,0));
        goPs.Play();
        goPs.transform.parent = transform;  
    }

    void OnTriggerExit(Collider collider)
    {
        Vector3 collisionPoint = collider.ClosestPoint(gameObject.transform.position);

        SetUpVars();
        Func<int, int> pointIndex = (i) =>
        {
            int val = 241 * Mathf.RoundToInt(collisionPoint.z + i - transform.position.z + 1) + Mathf.CeilToInt(collisionPoint.x + i - transform.position.x);
            print("TerrainCollisions: collisionPoint: " + collisionPoint + ", pointIndex: " + val + ", z: " + Mathf.RoundToInt(collisionPoint.z + i - transform.position.z) + ",x: " + Mathf.RoundToInt(collisionPoint.x + i - transform.position.x));
            return val;
        };

        depthTex[pointIndex(0)] = Color.red;



        print("TerrainCollisions: points: " + pointIndex(0) + ", " + pointIndex(-1) + ", " + pointIndex(-2) + ", " + pointIndex(1) + ", " + pointIndex(2));
        for (int i0 = 1; i0 < 5; i0++)
        {
            depthTex[pointIndex(-i0)] = Color.red * (1 / 4f) * (2 - i0);
            depthTex[pointIndex(i0)] = Color.red * (1 / 4f) * (2 - i0);
        }

        texture2D.SetPixels(depthTex);
        texture2D.Apply();
        material.SetTexture("_DisplacementTexture", texture2D);

        // byte[] bytes = texture2D.EncodeToPNG();
        // var dirPath = Application.dataPath + "/../SaveImages/";
        // if (!Directory.Exists(dirPath))
        // {
        //     Directory.CreateDirectory(dirPath);
        // }
        // File.WriteAllBytes(dirPath + "Image" + ".png", bytes);

        ParticleSystem goPs = Instantiate(groundParticles, collisionPoint, Quaternion.Euler(270,0,0));
        goPs.Play();
        goPs.transform.parent = transform;  
    }

    void SetUpVars()
    {
        if(depthTex == null){
            depthTex = new Color[800*241];
        }
        if(texture2D == null){
            texture2D = new Texture2D(241, 800);
        }
        if(material == null){
            material = terrainBlock.GetComponent<MeshRenderer>().material;
        }
    }

}