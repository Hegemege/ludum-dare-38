using UnityEngine;
using System.Collections;
using System.Security.Cryptography;

public class PlayerController : MonoBehaviour
{
    public GameObject PlanetReference;

    // TMEP
    public GameObject debugSphere;

    // Physics parameters
    public float Airspeed;
    public float TurningSpeed;

    public float DistanceFromGround;

    public LayerMask CollidableLayerMask;
    public LayerMask GroundLayerMask;

    // Physics helpers
    [HideInInspector]
    public Vector3 Velocity;

    // Privates
    private CharacterController characterController;

    // Input
    private float HorizontalInput
    {
        get
        {
            return Input.GetAxis("Horizontal");
        }
    }

    private float VerticalInput
    {
        get
        {
            return Input.GetAxis("Vertical");
        }
    }

    void Awake() 
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        // Initialize velocity as forward
        Velocity = Airspeed * new Vector3(0f, 0f, 1f);

        // Move the player to be at the correct height
        var groundPoint = GroundPosition(transform.position);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;
        transform.position = groundPoint + planetToGround * DistanceFromGround;
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        // Helper vector towards the planet from the player
        Vector3 towardsPlanet = PlanetReference.transform.position - transform.position;

        // Input
        var newForward = Velocity.normalized;

        // Turning
        newForward = Quaternion.AngleAxis(HorizontalInput * TurningSpeed * dt, -towardsPlanet) * newForward;

        Velocity += Airspeed * newForward;

        // Project velocity to be at constant distance from ground
        Vector3 groundPoint = GroundPosition(transform.position + Velocity * dt);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;

        var normalizedTarget = groundPoint + planetToGround * DistanceFromGround;
        var normalizedVelocity = normalizedTarget - transform.position;

        debugSphere.transform.position = groundPoint;

        // Apply projected velocity
        Velocity = normalizedVelocity;

        characterController.Move(Velocity * dt);

        // Rotate the player to travel forward
        transform.rotation = Quaternion.LookRotation(Velocity, -towardsPlanet);
    }

    /// <summary>
    /// Returns the distance to ground at current location
    /// </summary>
    private Vector3 GroundPosition(Vector3 from)
    {
        Vector3 towardsPlanet = PlanetReference.transform.position - from;
        Ray ray = new Ray(from, towardsPlanet);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, GroundLayerMask))
        {
            return hit.point;
        }

        Debug.LogWarning("No ground under player. Shouldn't happen.");
        return Vector3.zero;
    }
}
