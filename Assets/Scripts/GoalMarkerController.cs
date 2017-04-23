using UnityEngine;
using System.Collections;

public class GoalMarkerController : MonoBehaviour
{
    public AudioSource PickupAudio;

    private Vector3 startLocalPosition;

    public float BobbingSpeed;
    public float BobbingDistance;
    private float bobTimer;

    public GameObject ParentFarm;

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
        if (other.gameObject.CompareTag("Player"))
        {
            GameState.instance.PickedMarkers += 1;

            PickupAudio.transform.parent = null;
            PickupAudio.Play();

            Destroy(PickupAudio, 1f);

            ParentFarm.GetComponent<PlatformFarm>().IsDone = true;

            Destroy(gameObject);
        }
    }
}
