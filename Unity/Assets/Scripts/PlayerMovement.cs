using UnityEngine;
using System.Linq;
using System;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 10;
    public float maxMoveSpeed = 30;
    public float speedIncreaseStep = 1;
    float hopHeight = 1f;
    Animator animator;
    float decendDurationInitial;
    float hopDurationInitial;
    public Collider jumpTriggerCollider;
    Rigidbody rb;
    float distanceYForHop;
    float pressInterval = 0;
    public float maxHeight = 10;
    public float maxPressInterval = 0.5f;
    float inputHopHeight=0;
    public int powerUpCount=0;
    public TextMesh powerCount;

    void Start()
    {
        moveSpeed = 10;  
        animator = GetComponent<Animator>(); 
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == "Armature|decend_and_land")
            {
                decendDurationInitial = clip.length;
            }
            else if(clip.name == "Armature|hop")
            {
                hopDurationInitial = clip.length;
            }
        }
        rb = GetComponent<Rigidbody>();
    }

    float lastLinearVelocityY=-1;
    void Update()
    {
    
        powerCount.text = "" + powerUpCount;
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        if(currentState.IsName("Hop") && currentState.normalizedTime >= 1f){
            Decend();
        }
        // else if(currentState.normalizedTime >= 1.2f){ //.2 to allow for imprecision
        //     animator.Play("Idle");                                                               // issue: stops other animations
        // }
        lastLinearVelocityY = rb.linearVelocity.y;
        CastRays();

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
    }

    void CastRays(){
        float hopDuration = 2*(float)Math.Sqrt(2*hopHeight/Math.Abs(Physics.gravity.y));
        Collider[] colliders = GetComponentsInChildren<Collider>();
        float x = colliders.Max(it => it.bounds.max.x);
        float y = colliders.Min(it => it.bounds.min.y);

        Vector3 rayOrigin = new Vector3(x + moveSpeed*hopDuration, y, jumpTriggerCollider.bounds.center.z);

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
    }

    float lastTriggerTime = 0;
    bool betweenJumps = false;
    float hopDuration = 1;
    bool grounded = false;
    void OnTriggerEnter(Collider other)
    {   
        String otherTag = other.gameObject.tag;
        if(otherTag.Equals("Jumpable")){
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
            hopDuration = (float)Math.Sqrt(2*Math.Abs(baseHopHeight)/Math.Abs(Physics.gravity.y))+(float)Math.Sqrt(2*Math.Abs(effectiveHopHeight)/Math.Abs(Physics.gravity.y));
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
        else if(otherTag.Equals("PowerUp")){
            Destroy(other.gameObject);
            powerUpCount++;
            if(moveSpeed < maxMoveSpeed){
                moveSpeed+=speedIncreaseStep;
            }
        }
        else if(otherTag.Equals("Avoidable")){
            moveSpeed = 0;
            animator.StopPlayback();
        }

    }

    void OnTriggerExit(Collider other)
    {
        grounded = false;
    }

    void Decend(){
        animator.speed = Mathf.Min(decendDurationInitial/(hopDuration/2));
        animator.Play("Decend and Land");
        betweenJumps = false;
    }

}
