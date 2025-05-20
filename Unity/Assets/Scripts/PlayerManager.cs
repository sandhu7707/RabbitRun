using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    Vector2 touchStartPosition;
    public float touchMovementThreshold;
    public bool isDead;
    public bool isDestructable;
    float animatorSpeed;
    Animator animator;

    public void Pause()
    {
        animator.speed = 0;
    }

    public void Play()
    {
        animator.speed = animatorSpeed;
    }

    public void StartPlayer()
    {
        animatorSpeed = 1;
        isDead = false;
        isDestructable = false;
        Play();
        startTime = Time.time;
        
    }

    public void SetAnimatorSpeed(float speed)
    {
        animatorSpeed = speed;
        animator.speed = speed;
    }

    void Start()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        animator = GetComponent<Animator>();
        animator.speed = 0;
        StartPlayer();
    }
    float startTime;
    Renderer meshRenderer;
    void Blink()
    {
        meshRenderer.enabled = !meshRenderer.enabled;
    }
    void Update()
    {
        if (Time.time - startTime > 3)
        {
            isDestructable = true;
            meshRenderer.enabled = true;
        }
        else
        {
            Blink();
        }

        if (isDead)
        {
            return;
        }

        if(Input.GetKey(KeyCode.J) || Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Began)){
            touchStartPosition = Input.touches[0].position;
        }

        if(Input.touchCount > 0 && Input.GetTouch(0).phase.Equals(TouchPhase.Ended) && Mathf.Abs(Input.GetTouch(0).position.x - touchStartPosition.x) > touchMovementThreshold){
            if(Input.GetTouch(0).position.x - touchStartPosition.x > 0){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z - 20, 350, 450));
            }
            if(Input.GetTouch(0).position.x - touchStartPosition.x < 0){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z + 20, 350, 450));
            }
        }

        if(Input.GetKeyUp(KeyCode.A)){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z - 20, 350, 450));
        }
        if(Input.GetKeyUp(KeyCode.D)){
            transform.position  = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp( transform.position.z + 20, 350, 450));
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "obstacle" && isDestructable)
        {
            isDead = true;
            isDestructable = false;
        }
        else if (collision.gameObject.tag == "PowerUp")
        {
            Destroy(collision.gameObject);
        }
    }

}
