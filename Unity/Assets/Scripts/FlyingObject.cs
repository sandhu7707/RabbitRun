using System;
using UnityEngine;

public class FlyingObject: MonoBehaviour{
    float speedY;
    public float speedX;
    public float triggerX;
    public GameObject player;
    public float playerPosX;
    void Update(){
        if(transform.position.x < triggerX && !diving){
            Dive();    
        } else if(diving){
            transform.position = new Vector3(transform.position.x + speedX*Time.deltaTime, transform.position.y + speedY*Time.deltaTime, transform.position.z);
        } else {
            transform.position = new Vector3(transform.position.x + speedX*Time.deltaTime, transform.position.y, transform.position.z);
        } 

        if(rotateToDive && Mathf.Abs(transform.rotation.eulerAngles.z) < Mathf.Abs(rotationZTarget)){
            transform.rotation = Quaternion.Euler(0,0, Time.deltaTime*rotationZTarget);
        }
    }

    bool diving = false;
    bool rotateToDive = false;
    float rotationZTarget;
    void Dive(){
        diving = true;
        float distX = transform.position.x - playerPosX;
        float distY = player.transform.position.y - transform.position.y;
        speedY = (Math.Abs(speedX) + Math.Abs(player.GetComponent<PlayerMovement>().moveSpeed))*distY/distX;

        rotationZTarget = Vector2.SignedAngle(new Vector2(speedX,0), new Vector2(speedX,speedY));
        rotateToDive = true;
    }
}