
using System.Linq;
using UnityEngine;

public class PlayerConfig : MonoBehaviour
{
    public float maxPressInterval;
    public float maxHeight;
    float pressInterval = 0;
    bool grounded = false;
    int powerUpCount = 0;
    public float moveSpeed;
    public float minMoveSpeed;
    public float speedIncreaseStep;
    public float maxMoveSpeed;
    public TextMesh powerCount;

    void Start(){
        moveSpeed = minMoveSpeed;
    }
    void Update(){

        powerCount.text = "" + powerUpCount;
        if(Input.GetKeyDown(KeyCode.J)){
            pressInterval = 0;
        }

        if(Input.GetKey(KeyCode.J)){
            pressInterval += Time.deltaTime;
        }

        if(Input.GetKeyUp(KeyCode.J) || pressInterval > maxPressInterval){
            Jump(pressInterval);
            pressInterval = 0;
        }

    }

    void Jump(float forceFactor){
        if(grounded){
            Rigidbody rb = GetComponent<Rigidbody>();
            float maxForce = rb.mass * Mathf.Sqrt(2*Physics.gravity.magnitude*maxHeight);
            GetComponent<Rigidbody>().AddForce(new Vector3(0,maxForce,0)*forceFactor/maxPressInterval, ForceMode.Impulse);
        }
    }

    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.tag.Equals("Jumpable")){
            Bounds bounds = collision.gameObject.GetComponent<Renderer>().bounds;
            if(collision.contacts.Any(point => bounds.max.y-point.point.y > 0.1)){
                moveSpeed=0;
            }
            grounded = true;
        }
        if(collision.gameObject.tag.Equals("Avoidable")){
            moveSpeed=0;
        }
    }

    void OnCollisionStay(Collision collision){
        if(collision.gameObject.tag.Equals("Jumpable")){
            grounded = true;
        }
    }

    void OnCollisionExit(Collision collision){
        if(collision.gameObject.tag.Equals("Jumpable")){
            grounded = false;
        }
    }

    void OnTriggerEnter(Collider collider){
        if(collider.gameObject.tag.Equals("PowerUp")){
            Destroy(collider.gameObject);
            powerUpCount++;
            if(moveSpeed < maxMoveSpeed){
                moveSpeed+=speedIncreaseStep;
            }
        }
    }

}
