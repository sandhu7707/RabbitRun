using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float defaultYMultiplier = 1.0f;
    float yMultiplier;
    public float maxPressInterval = 0.5f;
    public float maxYFactor = 4;
    public float minYFactorOnJump = 2;
    float pressInterval = 0;

    bool allowYReset = false;
    bool recordingMaxHeight = true;
    float hopHeight = 0;
    Animator animator;
    Vector3 touchStartPosition;
    float touchMovementThreshold = 1f;
    public float lateralMovementDampingFactor = 0.03f;
    public Vector3 originalPosition;
    public ParticleSystem groundParticles;
    void Start()
    {
        groundParticles.Stop();
        originalPosition = transform.position;
        print("original position: " + originalPosition);
        // animator = GetComponent<Animator>();
        // yMultiplier = defaultYMultiplier;
    }

    void Update()
    {
        print("current position: " +  transform.position);
        print(yMultiplier);
        if(Input.GetKey(KeyCode.J) || Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Began)){
            // if(allowYReset && yMultiplier == defaultYMultiplier){
                // if(animator.GetCurrentAnimatorStateInfo(0).IsName("Hop")){
                //     animator.Play("Decend");
                // }
                // animator.speed = 5;
            // }
            // animator.SetBool("hold", true);
            // pressInterval += Time.deltaTime;
            touchStartPosition = Input.touches[0].position;
        }
        // if(Input.GetKeyUp(KeyCode.J) || pressInterval > maxPressInterval || Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Ended) && (Mathf.Abs(Input.touches[0].position.x - touchStartPosition.x) < touchMovementThreshold)){

        //     allowYReset = false;
        //     print("y reset not allowed");
        //     yMultiplier = minYFactorOnJump + (maxYFactor - minYFactorOnJump)*pressInterval/maxPressInterval;
        //     animator.speed = 1/yMultiplier;
        //     animator.SetBool("hold", false);
        //     pressInterval = 0;
        // }

        if(Input.touchCount > 0 && Mathf.Abs(Input.GetTouch(0).position.x - touchStartPosition.x) > touchMovementThreshold){
            // pressInterval = 0;
            // animator.SetBool("hold", false);
            transform.position -= new Vector3(0,0,lateralMovementDampingFactor*(Input.GetTouch(0).position.x - touchStartPosition.x));
            transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp(transform.position.z, originalPosition.z-20, originalPosition.z+20));
        }


        // if(animator.GetCurrentAnimatorStateInfo(0).IsName("Decend")){
        //     print("y reset allowed");
        //     allowYReset = true;
        // }

        // print("hopHeight: " + hopHeight);
    }

    // void OnAnimatorMove()
    // {
    //     Animator animator = GetComponent<Animator>();
    //     if (animator != null)
    //     {
    //         Vector3 deltaPosition = animator.deltaPosition;
    //         deltaPosition.y *= yMultiplier; // Scale the Y component
    //         print(deltaPosition);
    //         print(transform.position);
    //         transform.position = new Vector3(transform.position.x, transform.position.y + deltaPosition.y, transform.position.z);
            
    //         if(deltaPosition.y < 0 && hopHeight > 0){
    //             recordingMaxHeight = false;
    //         }
    //         if(deltaPosition.y > 0 && recordingMaxHeight && yMultiplier == defaultYMultiplier){
    //             hopHeight += deltaPosition.y;
    //         }
    //     }
    // }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Jumpable"){
            // print("collision..");
            // if(allowYReset){
            //     print("y reset done");
            //     yMultiplier = defaultYMultiplier;
            //     animator.speed = 1;
            // }
            // GetComponent<Rigidbody>().linearVelocity = new Vector3(0,0,0);
            // GetComponent<Animator>().SetTrigger("Land");
            groundParticles.Play();
            groundParticles.Emit(50);
        }
        else if(collision.gameObject.tag == "PowerUp"){
            Destroy(collision.gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PowerUp"){
            Destroy(other.gameObject);
        }
    }
    void OnCollisionStay(Collision collision)
    {
        // GetComponent<Animator>().SetTrigger("Land");
        groundParticles.Play();
        groundParticles.Emit(20);
    }

    void OnCollisionExit(Collision collision)
    {  
        // if(collision.gameObject.tag == "Jumpable"){
        //     GetComponent<Animator>().ResetTrigger("Land");
        // }
        groundParticles.Stop();
    }


}
