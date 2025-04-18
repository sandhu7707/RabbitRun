
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
    float hopHeight = 0.5f;
    float hopHeightDecending = 0.01f;
    float hopHeightAscending = 1.2f;
    Vector3 jumpForce;
    Vector3 hopJumpForce;
    float lastVelocityY;
    Animator animator;
    float decendDurationInitial;
    float hopDurationInitial;
    float timeToTop;
    bool insideJump;
    [Min(1)]
    float gravityFactor=2;
    float lastCollisionY;
    float slopeDeciderThreshold = 0.5f;

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
    }
    void Update(){

        powerCount.text = "" + powerUpCount;
        Rigidbody rb = GetComponent<Rigidbody>();
        

        if(Input.GetKey(KeyCode.J) || Input.touchCount > 0){
            pressInterval += Time.deltaTime;
        }
        if(Input.GetKeyUp(KeyCode.J) || pressInterval > maxPressInterval || Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Ended)){
            insideJump = false;
            float maxAdditionalForce = rb.mass * Mathf.Sqrt(2*Physics.gravity.magnitude*(maxHeight-hopHeight));
            jumpForce = hopJumpForce + new Vector3(0,maxAdditionalForce,0)*pressInterval/maxPressInterval;

            if(grounded){
                Jump(jumpForce);
                jumpForce = new Vector3(0,0,0);
            }
            pressInterval = 0;
        }
        if(lastVelocityY > 0 && rb.linearVelocity.y <= 0){
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
        animator.Play("Hop");
        
    }

    IEnumerator WaitForAnimationThenDecend()
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        
        // Wait until the current animation is done
        while (currentState.normalizedTime < 1f)
        {
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
        for(int i0=1; i0<colliders.Count(); i0++){
            float currentMinY = colliders[i0].bounds.min.y;
            leadingColliderPos += colliders[i0].bounds.center;
            if( currentMinY < minY){
                minY = currentMinY;
            }
        }

        leadingColliderPos = leadingColliderPos/colliders.Count();
        leadingColliderPos.y = minY;

        Ray ray = new Ray(leadingColliderPos, Vector3.down);
        RaycastHit raycastHit;

        if(Physics.Raycast(ray, out raycastHit)){
            speedMultiplier = decendDurationInitial/Mathf.Sqrt(Mathf.Abs(2*raycastHit.distance/Physics.gravity.y));
        }

        animator.speed = speedMultiplier;
        animator.Play("Decend and Land");
        insideJump = false;
    }

    void OnCollisionEnter(Collision collision){

        if(collision.gameObject.tag.Equals("Jumpable")){

            float currentCollisionY = collision.contacts.Average(it => it.point.y);
            grounded = true;

            if(jumpForce != new Vector3(0,0,0)){
                Jump(jumpForce);
                jumpForce = new Vector3(0,0,0);
            }
            else{
                if(lastCollisionY != null){
                    if(lastCollisionY - currentCollisionY > slopeDeciderThreshold){
                        print("descending");
                        // newHopForce = new Vector3(0, GetComponent<Rigidbody>().mass * Mathf.Sqrt(2*Physics.gravity.magnitude*hopHeightDecending), 0);
                        Collider[] colliders = GetComponentsInChildren<Collider>();
                        float minY = colliders[0].bounds.min.y;
                        Vector3 leadingColliderPos = colliders[0].bounds.center;

                        for(int i0=1; i0<colliders.Count(); i0++){
                            float currentMinY = colliders[i0].bounds.min.y;
                                leadingColliderPos += colliders[i0].bounds.center;
                                if( currentMinY < minY){
                                    minY = currentMinY;
                                }
                        }

                        leadingColliderPos = leadingColliderPos/colliders.Count();
                        leadingColliderPos.y = minY;

                        Ray ray = new Ray(leadingColliderPos, Vector3.down);
                        RaycastHit raycastHit;

                        if(Physics.Raycast(ray, out raycastHit) && raycastHit.distance > slopeDeciderThreshold){
                            insideJump = true;
                            Decend();
                        }
                        else{
                            print("but not decending");
                            Jump(hopJumpForce);
                        }
                    }
                    else if(lastCollisionY - currentCollisionY < slopeDeciderThreshold){
                        print("ascending");
                        Vector3 newHopForce = new Vector3(0, GetComponent<Rigidbody>().mass * Mathf.Sqrt(2*Physics.gravity.magnitude*hopHeightAscending), 0);
                        Jump(newHopForce);
                    }
                }
                else{
                    Jump(hopJumpForce);
                }
                lastCollisionY = currentCollisionY;
            }
        }
        if(collision.gameObject.tag.Equals("Avoidable")){
            // moveSpeed=0;
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
