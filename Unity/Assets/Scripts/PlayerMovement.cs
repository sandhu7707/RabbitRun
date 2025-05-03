using UnityEngine;
using System.Linq;
using System;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 0;
    public float maxMoveSpeed = 30;
    public float speedIncreaseStep = 1;
    public float hopHeight = 0.5f;
    Animator animator;
    float decendDurationInitial;
    float hopDurationInitial;
    Rigidbody rb;
    float distanceYForHop;
    float pressInterval = 0;
    public float maxHeight = 10;
    public float maxPressInterval = 0.5f;
    float inputHopHeight=0;
    public int powerUpCount=0;
    public TextMesh powerCount;
    int targetedMoveSpeed;
    bool everGrounded = false;
    Quaternion targetRotation;
    public GameObject playerPositon;
    void Start()
    {
        targetedMoveSpeed = 10;  
        moveSpeed = 2;
        animator = GetComponent<Animator>(); 
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == "Armature|decend.new")
            {
                decendDurationInitial = clip.length;
            }
            else if(clip.name == "Armature|hop.new")
            {
                hopDurationInitial = clip.length;
            }
        }
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(targetedMoveSpeed > moveSpeed){
            moveSpeed += Time.deltaTime*speedIncreaseStep;
        }
       
        powerCount.text = "" + powerUpCount;
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if(currentState.IsName("Hop") && currentState.normalizedTime >= 1f){
            Decend();
        }
        if(everGrounded){
            CastRays();
        }

        if(Input.GetKey(KeyCode.J) || Input.touchCount > 0){
            pressInterval += Time.deltaTime;
        }
        if(Input.GetKeyUp(KeyCode.J) || pressInterval > maxPressInterval || Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Ended)){
            // insideJump = false;
            inputHopHeight = (maxHeight-hopHeight)*pressInterval/maxPressInterval;
            if(grounded){
                float additionalForce = rb.mass * Mathf.Sqrt(2*Physics.gravity.magnitude*(inputHopHeight));
                rb.AddForce(new Vector3(0,additionalForce,0)*pressInterval/maxPressInterval, ForceMode.Impulse);
                inputHopHeight = 0;
            }
            pressInterval = 0;
        }
        // if(targetRotation != null ){
        //     // transform.parent.rotation = targetRotation;
        //     transform.parent.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 2*Time.deltaTime/Math.Max(hopDuration,1));
        // }
        
        // transform.rotation = Quaternion.Euler(0,0,Mathf.Clamp(transform.rotation.eulerAngles.z, -30, 30));
    }

    void CastRays(){
        float hopDuration = 2*(float)Math.Sqrt(2*hopHeight/Math.Abs(Physics.gravity.y));
        Collider[] colliders = GetComponentsInChildren<Collider>();
        float x = colliders.Max(it => it.bounds.max.x);
        float y = colliders.Min(it => it.bounds.min.y);

        Vector3 rayOrigin = new Vector3(x + moveSpeed*hopDuration/2, y, transform.position.z);

        CastRay(rayOrigin, Vector3.down, Color.green);
    }

    void CastRay(Vector3 origin, Vector3 dir, Color color){

        float additionalSteps = 0;
        Vector3  stepIncrement = new Vector3(0,1,0);

        Ray ray = new Ray(origin, dir);
        RaycastHit raycastHit;

        while(!Physics.Raycast(ray, out raycastHit)){
            ray = new Ray(origin+(++additionalSteps)*stepIncrement, dir);
        }

        Debug.DrawLine(ray.origin, raycastHit.point, color);
        distanceYForHop = additionalSteps*stepIncrement.y-raycastHit.distance;
        print("additionSteps: "+additionalSteps+", rayLength: "+raycastHit.distance+", finalDistance: "+distanceYForHop);
        // targetRotation = Quaternion.LookRotation(new Vector3(moveSpeed*hopDuration/2,distanceYForHop,0));
        
    }

    float lastTriggerTime = 0;
    bool betweenJumps = false;
    float hopDuration = 1;
    bool grounded = false;
    void OnCollisionEnter(Collision other)
    {   

        String otherTag = other.gameObject.tag;
        if(otherTag.Equals("Jumpable")){
            // Editor.
            everGrounded = true;
            grounded = true;
            if(betweenJumps){
                return;
            }
            print("trigger, distanceYForHop: " + distanceYForHop);
            // Vector
            // CastRays();
            float baseHopHeight = this.hopHeight + this.inputHopHeight;
            this.inputHopHeight = 0;
            float effectiveHopHeight = baseHopHeight+distanceYForHop;
            // float currentHopHeight = this.hopHeight;
            hopDuration = (float)Math.Sqrt(2*Math.Abs(baseHopHeight)/Math.Abs(Physics.gravity.y))+(float)Math.Sqrt(2*Math.Max(baseHopHeight, effectiveHopHeight)/Math.Abs(Physics.gravity.y));
            if(lastTriggerTime!= 0){
                print("time between triggers: " + (Time.time - lastTriggerTime));
                print("hopDuration: " + hopDuration);
            }
            lastTriggerTime = Time.time;

            // float hopHeight = Math.Abs(Physics.gravity.y)*(hopDuration/2)*(hopDuration/2)/2;
            if(effectiveHopHeight > 0){
                Vector3 jumpForce = new Vector3(0,1,0)*(float)(rb.mass*(Math.Sqrt(2*Math.Abs(Physics.gravity.y)*effectiveHopHeight)));
                rb.linearVelocity = new Vector3(0,0,0);
                rb.AddForce(jumpForce, ForceMode.Impulse);
            }
            // print("applied force: "+ jumpForce + ", to produce hopheight: "+ hopHeight);

            animator.speed = Mathf.Min(hopDurationInitial/(hopDuration/2),1);
            animator.Play("Hop");
            betweenJumps = true;
        }
        else if(otherTag.Equals("Avoidable")){
            moveSpeed = 0;
            animator.StopPlayback();
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("PowerUp")){
            Destroy(other.gameObject);
            powerUpCount++;
            if(moveSpeed < maxMoveSpeed){
                moveSpeed+=speedIncreaseStep;
            }
        }
    }

    void OnCollisionExit(Collision other)
    {
        grounded = false;
    }

    void Decend(){
        // print(distanceYForHop);
        // print(hopHeight);
        // Debug.Break();
        float timeToLand = (float)Math.Sqrt(2*Math.Max(hopHeight,0-distanceYForHop)/Math.Abs(Physics.gravity.y));
        animator.speed = decendDurationInitial/timeToLand;
        animator.Play("Decend and Land");
        betweenJumps = false;
        
        targetRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Clamp(Vector3.SignedAngle(new Vector3(1,0,0), new Vector3(moveSpeed*hopDuration/2,distanceYForHop+hopHeight,0), new Vector3(0,0,1)), -30, 30)));
        print("target rotation: "+ targetRotation);

    }

}
