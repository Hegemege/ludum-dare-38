using UnityEngine;
using System.Collections;
using System.Security.Cryptography;

public class PlayerController : MonoBehaviour
{
    public GameObject PlanetReference;
    public GameObject DropMarker;

    // TMEP
    public GameObject debugSphere;

    // Self-references
    public GameObject ModelReference;
    public float TurningSmoothing;
    public float TurningAngle;

    // Input
    public float InputDeadzone;

    // Physics parameters
    public float Airspeed;
    public float TurningSpeed;
    public float VelocitySmoothing;

    public float BoostScale;
    public float BrakeScale;

    public float DistanceFromGround;

    public LayerMask CollidableLayerMask;
    public LayerMask GroundLayerMask;

    // Physics helpers
    [HideInInspector]
    public Vector3 Velocity;

    [HideInInspector]
    public bool Boosting;
    
    [HideInInspector]
    public bool Braking;

    // Privates
    private CharacterController characterController;
    private Quaternion modelRotationTarget;
    private Vector3 targetVelocity;

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
        Velocity = Airspeed * new Vector3(0f, 0f, 1f) * Time.fixedDeltaTime;

        // Move the player to be at the correct height
        var groundPoint = GroundPosition(transform.position);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;
        transform.position = groundPoint + planetToGround * DistanceFromGround;
    }

    void Update()
    {
        // Boost/brake handling
        Boosting = false;
        Braking = false;
        if (Mathf.Abs(VerticalInput) > InputDeadzone)
        {
            if (VerticalInput < 0)
            {
                Braking = true;
            }
            else
            {
                Boosting = true;
            }
        }
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

        // Air speed calculations
        var effectiveAirspeed = Airspeed;
        if (Boosting) effectiveAirspeed += Airspeed * BoostScale * Mathf.Abs(VerticalInput);
        if (Braking) effectiveAirspeed += Airspeed * BrakeScale * Mathf.Abs(VerticalInput);

        var newVelocity = Velocity + newForward * effectiveAirspeed;

        // Project velocity to be at constant distance from ground
        Vector3 groundPoint = GroundPosition(transform.position + newVelocity * dt);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;

        var normalizedTarget = groundPoint + planetToGround * DistanceFromGround;
        var normalizedVelocity = normalizedTarget - transform.position;

        // TEMP
        debugSphere.transform.position = GroundPosition(transform.position);

        // Move drop marker under the player
        DropMarker.transform.position = GroundPosition(transform.position);

        // Apply projected velocity after smoothing
        Velocity = Vector3.Slerp(Velocity, normalizedVelocity, VelocitySmoothing);

        characterController.Move(Velocity * dt);



        // Rotate the player to travel forward
        transform.rotation = Quaternion.LookRotation(Velocity, -towardsPlanet);

        // Rotate the player model when turning
        var turning = TurningAngle * -HorizontalInput;
        modelRotationTarget = Quaternion.AngleAxis(turning, Vector3.forward);
        ModelReference.transform.localRotation = Quaternion.Slerp(ModelReference.transform.localRotation, modelRotationTarget, TurningSmoothing);
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
