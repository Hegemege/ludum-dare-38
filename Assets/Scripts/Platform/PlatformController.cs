using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour
{
    public PlanetGenerator GeneratorReference;
    public GameObject PlayerReference;

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
    }
}
