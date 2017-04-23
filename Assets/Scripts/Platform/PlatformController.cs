using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour
{
    public PlanetGenerator GeneratorReference;
    public GameObject PlayerReference;

    public GameObject MarkerPrefab;

    // When the player hits the loose collider, this platform will regenerate it's contents when the player is on the opposite side of the planet
    public bool Triggered;
    public bool Regenerate;

    private GameObject content;

    void Awake()
    {

    }

    void Start() 
    {
        
    }
    
    void Update()
    {
        if (!Regenerate) return;

        Vector3 towardsPlayer = PlayerReference.transform.position - transform.position;

        if (Triggered && content.GetComponent<PlatformContent>().IsDone)
        {
            if (Vector3.Dot(towardsPlayer.normalized, -transform.up) > 0.8f)
            {
                // Re-generate the content
                GenerateContent();
            }
        }
    }

    public void GenerateContent()
    {
        var contentPrefab = GeneratorReference.GetRandomPlatform();

        if (content)
        {
            Destroy(content);
            content = null;
        }

        content = Instantiate(contentPrefab);

        content.transform.position = transform.position;
        content.transform.rotation = transform.rotation;

        content.transform.parent = transform;

        Triggered = false;

        // Generate the marker for farms
        if (content.gameObject.CompareTag("Farm"))
        {
            if (!Regenerate)
            {
                GameState.instance.MaxMarkers += 1;
            }

            var marker = Instantiate(MarkerPrefab);

            marker.transform.parent = content.transform;
            marker.transform.localRotation = Quaternion.identity;
            marker.transform.localPosition = Vector3.up * 12f;

            marker.GetComponent<GoalMarkerController>().ParentFarm = content;
        }
    }
}
