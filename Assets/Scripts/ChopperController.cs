using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ChopperController : MonoBehaviour 
{
    public GameObject PlanetReference;

    // Self-references
    public GameObject ModelReference;
    public float TurningSmoothing;
    public float TurningAngle;

    // Physics parameters
    public float Airspeed;
    public float TurningSpeed;
    public float VelocitySmoothing;

    public float DistanceFromGround;

    public LayerMask GroundLayerMask;

    // Physics helpers
    [HideInInspector]
    public Vector3 Velocity;

    [HideInInspector]
    public Vector3[] VelocityHistory;

    [HideInInspector]
    public bool Alive;

    // Privates
    private Quaternion modelRotationTarget;
    private Vector3 targetVelocity;

    private List<GameObject> DestructionParts;

    void Awake() 
    {
        Alive = true;
    }

    void Start() 
    {
        // Initialize velocity as forward
        Velocity = Airspeed * new Vector3(0f, 0f, 1f) * Time.fixedDeltaTime;

        // Move the chopper to be at the correct height
        var groundPoint = GroundPosition(transform.position);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;
        transform.position = groundPoint + planetToGround * DistanceFromGround;
    }
    
    void FixedUpdate() 
    {
        if (!Alive)
        {
            return;
        }

        float dt = Time.fixedDeltaTime;
        // Helper vector towards the planet from the chopper
        Vector3 towardsPlanet = PlanetReference.transform.position - transform.position;

        // Input
        var newForward = Velocity.normalized;

        // Simulate input
        var HorizontalInput = 0f;
        var VerticalInput = 0f;

        // Turning
        newForward = Quaternion.AngleAxis(HorizontalInput * TurningSpeed * dt, -towardsPlanet) * newForward;

        // Air speed calculations
        var effectiveAirspeed = Airspeed;
        //if (Boosting) effectiveAirspeed += Airspeed * BoostScale * Mathf.Abs(VerticalInput);
        //if (Braking) effectiveAirspeed += Airspeed * BrakeScale * Mathf.Abs(VerticalInput);

        var newVelocity = Velocity + newForward * effectiveAirspeed;

        // Project velocity to be at constant distance from ground
        Vector3 groundPoint = GroundPosition(transform.position + newVelocity * dt);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;

        var normalizedTarget = groundPoint + planetToGround * DistanceFromGround;
        var normalizedVelocity = normalizedTarget - transform.position;

        // TEMP
        //debugSphere.transform.position = GroundPosition(transform.position);

        // TODO: marker for chopper?
        // Move drop marker under the chopper
        //DropMarker.transform.position = GroundPosition(transform.position);

        // Apply projected velocity after smoothing
        Velocity = Vector3.Slerp(Velocity, normalizedVelocity, VelocitySmoothing);

        // Move the plane
        transform.position += Velocity * dt;

        // Rotate the player to travel forward
        transform.rotation = Quaternion.LookRotation(Velocity, -towardsPlanet);

        // Rotate the chopper model when turning or accelerating/decelerating
        var turning = TurningAngle * -HorizontalInput;
        modelRotationTarget = Quaternion.AngleAxis(turning, Vector3.forward);
        ModelReference.transform.localRotation = Quaternion.Slerp(ModelReference.transform.localRotation, modelRotationTarget, TurningSmoothing);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle") || 
            other.gameObject.CompareTag("Chopper") || 
            other.gameObject.CompareTag("Player") || 
            other.gameObject.CompareTag("Missile"))
        {
            Explode();
        }
    }


    /// <summary>
    /// Returns the distance to ground at current location
    /// </summary>
    private Vector3 GroundPosition(Vector3 from)
    {
        Vector3 towardsPlanet = PlanetReference.transform.position - from;
        Ray ray = new Ray(from, towardsPlanet);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, GroundLayerMask))
        {
            return hit.point;
        }

        Debug.LogWarning("No ground under chopper. Shouldn't happen.");
        return Vector3.zero;
    }

    /// <summary>
    /// Explodes the player model and spawns particles
    /// </summary>
    public void Explode()
    {
        // Unparent all objects in the model
        DestructionParts = new List<GameObject>();
        DestructionParts.AddRange(ModelReference.GetComponentsInChildren<MeshRenderer>().Select(t => t.gameObject));

        Alive = false;

        var allParts = ModelReference.GetComponentsInChildren<Transform>().Select(t => t.gameObject);

        foreach (var part in allParts)
        {
            part.transform.parent = GameObject.Find("Debris").transform;
            part.layer = LayerMask.NameToLayer("Debris");

            Destroy(part, Random.Range(4f, 7f));
        }

        foreach (var part in DestructionParts)
        {
            // Add rigid body and collider, add random start impulse force and torque
            var rb = part.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.drag = 0.12f;
            rb.angularDrag = 0.01f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var randomStartForce = Velocity + Random.onUnitSphere * 2f;
            var randomStartTorque = Random.onUnitSphere * 10f;

            rb.AddForce(randomStartForce, ForceMode.Impulse);
            rb.AddTorque(randomStartTorque, ForceMode.Impulse);

            part.AddComponent<BoxCollider>();

            // Add custom gravity
            var pg = part.AddComponent<PlanetGravity>();
            pg.Gravity = 20f;
            pg.PlanetReference = PlanetReference;
        }

        // TODO: spawn explosion?
    }
}
