
using System.Collections;
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
    int hopHeight = 1;
    Vector3 jumpForce;
    Vector3 hopJumpForce;
    float lastVelocityY;
    Animator animator;
    float decendDurationInitial;
    float hopDurationInitial;
    float timeToTop;
    bool insideJump;
    bool wasTouchActive;
    [Min(1)]
    float gravityFactor=2;

    void Start(){
        moveSpeed = minMoveSpeed;
        hopJumpForce = new Vector3(0, GetComponent<Rigidbody>().mass * Mathf.Sqrt(2*Physics.gravity.magnitude*hopHeight), 0);
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
        GetComponent<Rigidbody>().AddForce(Physics.gravity*(gravityFactor-1), ForceMode.Acceleration);
    }
    void Update(){

        hopJumpForce = new Vector3(0, GetComponent<Rigidbody>().mass * Mathf.Sqrt(2*Physics.gravity.magnitude*hopHeight), 0);
        powerCount.text = "" + powerUpCount;
        Rigidbody rb = GetComponent<Rigidbody>();

        if(rb.GetAccumulatedForce() != new Vector3(0,0,0)){
            print(rb.GetAccumulatedForce());
        }

        // if(Input.GetKeyDown(KeyCode.J)){
        //     pressInterval = 0;
        // }

        if(Input.GetKey(KeyCode.J) || Input.touchCount > 0){
            pressInterval += Time.deltaTime;
        }

        if(Input.touchCount > 0){
            wasTouchActive = true;
        }
        else{
            wasTouchActive = false;
        }

        if(Input.GetKeyUp(KeyCode.J) || pressInterval > maxPressInterval || (wasTouchActive && Input.touchCount == 0)){
            insideJump = false;
            float maxAdditionalForce = rb.mass * Mathf.Sqrt(2*Physics.gravity.magnitude*(maxHeight-1));
            jumpForce = hopJumpForce + new Vector3(0,maxAdditionalForce,0)*pressInterval/maxPressInterval;

            if(grounded){
                Jump(jumpForce);
                jumpForce = new Vector3(0,0,0);
            }
            pressInterval = 0;
        }

        if(lastVelocityY > 0 && rb.linearVelocity.y <= 0){
            // Decend();
            StartCoroutine(WaitForAnimationThenDecend());
        }

        lastVelocityY = rb.linearVelocity.y;
    }

    void Jump(Vector3 jumpForce){
        if(insideJump){
            return;
        }
        insideJump = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        
        // rb.linearVelocity = Vector3.zero;
        // rb.angularVelocity = Vector3.zero;
        rb.AddForce(jumpForce, ForceMode.Impulse);

        timeToTop = Mathf.Abs((jumpForce.y/rb.mass)/Physics.gravity.y);

        float speedMultiplier = hopDurationInitial / timeToTop;

        animator.speed = speedMultiplier;
        // animator.Play("Land and Hop", 0, 0f);
        // animator.SetTrigger("Land");
        animator.Play("Hop");
        // StartCoroutine(WaitForAnimationToEndThenPlay("Hop"));
        grounded = false;
    }

    IEnumerator WaitForAnimationThenDecend()
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        
        // Wait until the current animation is done
        while (currentState.normalizedTime < 1f)
        {
            print(currentState.normalizedTime);
            currentState = animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }

        Decend();
    }

    void Decend(){

        float speedMultiplier = timeToTop!=0 ? decendDurationInitial / timeToTop : 1;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        float minY = colliders[0].bounds.min.y;
        Vector3 leadingColliderPos = colliders[0].bounds.center;

        foreach(Collider col in colliders){
            float currentMinY = col.bounds.min.y;
            if( currentMinY < minY){
                minY = currentMinY;
                leadingColliderPos = colliders[0].bounds.center;
                leadingColliderPos.y = colliders[0].bounds.min.y;
            }
        }

        Ray ray = new Ray(leadingColliderPos, Vector3.down);
        RaycastHit raycastHit;

        if(Physics.Raycast(ray, out raycastHit)){
            speedMultiplier = decendDurationInitial/Mathf.Sqrt(Mathf.Abs(2*raycastHit.distance/Physics.gravity.y));
        }

        animator.speed = speedMultiplier;
        animator.Play("Decend and Land");
        // StartCoroutine(WaitForAnimationToEndThenPlay("Decend and Land"));
        insideJump = false;
        // animator.SetTrigger("Decend");
    }

    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.tag.Equals("Jumpable")){
            // Bounds bounds = collision.gameObject.GetComponent<Renderer>().bounds;
            // if(collision.contacts.Any(point => bounds.max.y-point.point.y > 0.1)){
            //     moveSpeed=0;
            // }
            grounded = true;
            if(jumpForce != new Vector3(0,0,0)){
                Jump(jumpForce);
                jumpForce = new Vector3(0,0,0);
            }
            else{
                Jump(hopJumpForce);
            }
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
