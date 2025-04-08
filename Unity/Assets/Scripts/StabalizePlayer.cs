using System;
using UnityEngine;

public class StabalizePlayer: MonoBehaviour {

    // Vector3 lastCollisionPosition;
    public Animator animator;
    void Update()
    {
        // gameObject.transform.position = new Vector3(14.5f,gameObject.transform.position.y,28.14f);
        // Vector3 rotation = gameObject.transform.rotation.eulerAngles;

        // gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Clamp(rotation.z, -10, 10)));
    }

    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag.Equals("Jumpable")){
            animator.SetTrigger("Land");
        }
    }

    void OnTriggerExit(Collider other)
    {
        animator.ResetTrigger("Land");
    }
}