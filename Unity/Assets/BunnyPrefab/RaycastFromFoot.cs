using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RaycastFromFoot : MonoBehaviour
{
    public float triggerThreshold;
    public Animator animator;
    void Update()
    {
        Ray ray = new Ray(gameObject.transform.position, -transform.up);
        
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo) && hitInfo.collider.gameObject.tag.Equals("Jumpable")){
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
            if(hitInfo.distance < 0.7){
                SetLandingTrigger();
            }
            else{
                ResetLandingTrigger();
            }
        }
        else{
            
                Debug.DrawLine(ray.origin, ray.direction*100, Color.green);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag.Equals("Jumpable")){

        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag.Equals("Jumpable")){
            
        }
    }

    void SetLandingTrigger(){
        animator.SetTrigger("Land");
    }

    void ResetLandingTrigger(){
        animator.ResetTrigger("Land");
    }
}
