using System;
using UnityEngine;

public class PlayerControlsNew: MonoBehaviour {

    Vector2 touchStartPosition;
    public float touchMovementThreshold;
    public bool isAlive;
    public float score;
    public TextMesh scoreCard;
    void Start()
    {
        isAlive = true;
        // groundParticles.Stop();
    }

    void Update()
    {
        if(isAlive){
            score += Time.deltaTime;
        }
        else{
            return;
        }
        scoreCard.text = "" + Mathf.FloorToInt(score);


        print("current position: " +  transform.position);
        if(Input.GetKey(KeyCode.J) || Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Began)){
            touchStartPosition = Input.touches[0].position;
        }

        if(Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Ended) && Math.Abs(Input.GetTouch(0).position.x - touchStartPosition.x) > touchMovementThreshold){
            if(Input.GetTouch(0).position.x - touchStartPosition.x > 0){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z - 15, -30, 30));
            }
            if(Input.GetTouch(0).position.x - touchStartPosition.x < 0){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z + 15, -30, 30));
            }
        }

        if(Input.GetKeyUp(KeyCode.A)){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z - 15, -30, 30));
        }
        if(Input.GetKeyUp(KeyCode.D)){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z + 15, -30, 30));
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Avoidable"){
            isAlive = false;
            // groundParticles.Play();
            // groundParticles.Emit(50);
        }
        else if(collision.gameObject.tag == "PowerUp"){
            Destroy(collision.gameObject);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Jumpable"){
            // ParticleSystem goPs = Instantiate(groundParticles, transform.position, Quaternion.Euler(90,90,0));
            // goPs.Play();
            // // goPs.Emit(50);
            // goPs.transform.parent = transform;
        }
    }
    void OnCollisionStay(Collision collision)
    {
        // groundParticles.Play();
        // groundParticles.Emit(20);
    }

    void OnCollisionExit(Collision collision)
    {  
        // groundParticles.Stop();
    }


}