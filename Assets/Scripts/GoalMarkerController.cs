using UnityEngine;
using System.Collections;

public class GoalMarkerController : MonoBehaviour
{
    private Vector3 startLocalPosition;

    public float BobbingSpeed;
    public float BobbingDistance;
    private float bobTimer;

    void Awake() 
    {

    }

    void Start()
    {
        startLocalPosition = transform.localPosition;
    }
    
    void Update() 
    {
    
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        bobTimer += dt;

        Vector3 offset = new Vector3(0f, 1f, 0f);

        transform.localPosition = startLocalPosition + BobbingDistance * offset * Mathf.Sin(bobTimer * 1000f / BobbingSpeed);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("HIT");
        if (other.gameObject.CompareTag("Player"))
        {
            GameState.instance.PickedFarms += 1;

            // TODO: sound effect

            Destroy(gameObject);
        }
    }
}
