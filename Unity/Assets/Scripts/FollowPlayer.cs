using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    public Vector3 backOffset;
    public Vector3 sideOffset;
    public float maxHeightGap;
    public float moveSpeed;
    public CAMERA_TYPE cameraType;
    CAMERA_TYPE oldCameraType;
    [Serializable]
    public enum CAMERA_TYPE{ BACK, SIDE};
    public Vector3 playerOffset;

    void Start()
    {
        SetupCamera(true);
    }
    void Update()
    {
        SetupCamera(false);
        transform.LookAt(player.transform.position + playerOffset);
    }

    void SetupCamera(bool updateZ){
        if(cameraType == CAMERA_TYPE.BACK){
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(22,90,0));
            gameObject.transform.position = (updateZ ? player.transform.position : new Vector3(player.transform.position.x, player.transform.position.y, gameObject.transform.position.z)) + (updateZ ? backOffset : new Vector3(backOffset.x, backOffset.y, 0));
        }
        else{
            gameObject.transform.rotation = Quaternion.Euler(new Vector3(22,0,0));
            gameObject.transform.position = player.transform.position + sideOffset;
        }
        oldCameraType = cameraType;
    }
}
