using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 offset;
    public float maxHeightGap;
    public float moveSpeed;
    void Start()
    {
        gameObject.transform.position = player.transform.position + offset;
    }

    void Update()
    {
        float heightGap = player.transform.position.y - transform.position.y;
        if(Math.Abs(heightGap) > maxHeightGap){
            float targetY = heightGap > 0 ? transform.position.y + heightGap - maxHeightGap : transform.position.y + heightGap + maxHeightGap;
            float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime/(Mathf.Abs(heightGap)/moveSpeed));
            transform.Translate(new Vector3(0, newY-transform.position.y, 0));
        }
    }
}
