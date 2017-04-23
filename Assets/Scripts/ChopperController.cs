using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ChopperController : MonoBehaviour 
{
    public GameObject PlanetReference;
    public GameObject PlayerReference;
    public GameObject MissilePrefab;
    public GameObject MissileSpawnAnchor;
    public GameObject ExplosionPrefab;

    // Self-references
    public AudioSource ExplosionAudio;
    public GameObject ModelReference;
    public float TurningSmoothing;
    public float TurningAngle;
    public float PlayerFollowDistance;
    public float PlayerSeekDistance;
    public float PlayerFireDistance;
    public float FireInterval;
    public float FireConeAngle;
    public float ChopperAvoidDistance;

    public float FollowVelocityAdjust;

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
    public float[] VelocityHistory;
    private int velocityHistoryIndex;

    [HideInInspector]
    public bool Alive;

    // Privates
    private Quaternion modelRotationTarget;
    private Vector3 targetVelocity;

    private List<GameObject> DestructionParts;

    private float fireTimer;

    private GameObject avoidTarget;

    void Awake() 
    {
        Alive = true;
        VelocityHistory = new float[10];
    }

    void Start() 
    {
        // Initialize velocity as forward
        Velocity = Airspeed * transform.forward * Time.fixedDeltaTime;

        for (var i = 0; i < VelocityHistory.Length; ++i)
        {
            VelocityHistory[i] = Velocity.magnitude;
        }

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

        float playerDistance = Vector3.Distance(PlayerReference.transform.position, transform.position);

        // Input
        var newForward = Velocity.normalized;

        // Simulate input
        var HorizontalInput = 0f;
        var VerticalInput = 0f; // Not really used

        // If player is relative to the left, bank left etc
        Vector3 towardsPlayer = PlayerReference.transform.position - transform.position;
        Vector3 towardsPlayerNormalized = Vector3.ProjectOnPlane(towardsPlayer, -towardsPlanet);
        HorizontalInput = Mathf.Clamp(Vector3.Dot(towardsPlayerNormalized.normalized, transform.right) * 2, -1f, 1f);

        // If player is too close, and behind us, bank the opposite way
        if (playerDistance < PlayerFollowDistance && 
            Vector3.Dot(towardsPlayerNormalized.normalized, transform.forward) < 0)
        {
            HorizontalInput *= -1;
        }

        // Other chopper avoidance - this overrides the player following
        if (avoidTarget != null)
        {
            if (Vector3.Distance(avoidTarget.transform.position, transform.position) > ChopperAvoidDistance)
            {
                avoidTarget = null;
            }
            else
            {
                var towardsAvoid = avoidTarget.transform.position - transform.position;
                var towardsAvoidNormalized = Vector3.ProjectOnPlane(towardsAvoid, -towardsPlanet);
                HorizontalInput = -Mathf.Clamp(Vector3.Dot(towardsAvoidNormalized.normalized, transform.right) * 2, -1f, 1f);
            }
        }

        // Acceleration/deceleration
        float minVelocity = VelocityHistory.Min();
        float maxVelocity = VelocityHistory.Max();
        float averageVelocity = VelocityHistory.Average();

        if (Velocity.magnitude > averageVelocity)
        {
            VerticalInput = (Velocity.magnitude - averageVelocity) / (maxVelocity - averageVelocity);
        }
        else if (Velocity.magnitude < averageVelocity)
        {
            VerticalInput = -(1f - (averageVelocity - Velocity.magnitude) / (averageVelocity - minVelocity));
        }

        // Turning
        newForward = Quaternion.AngleAxis(HorizontalInput * TurningSpeed * dt, -towardsPlanet) * newForward;

        // Air speed calculations
        var effectiveAirspeed = Airspeed;

        // Adjust airspeed based on vertical input and if the player is too close or too far
        if (playerDistance > PlayerSeekDistance)
        {
            effectiveAirspeed += FollowVelocityAdjust;
        }

        if (playerDistance < PlayerFollowDistance)
        {
            effectiveAirspeed -= FollowVelocityAdjust;
        }

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

        // Move the chopper
        transform.position += Velocity * dt;

        // Rotate the player to travel forward
        transform.rotation = Quaternion.LookRotation(Velocity, -towardsPlanet);

        // Rotate the chopper model when turning or accelerating/decelerating
        var turning = TurningAngle * -HorizontalInput;
        modelRotationTarget = Quaternion.AngleAxis(turning, Vector3.forward);
        modelRotationTarget *= Quaternion.AngleAxis(TurningAngle * VerticalInput/3, Vector3.right);
        ModelReference.transform.localRotation = Quaternion.Slerp(ModelReference.transform.localRotation, modelRotationTarget, TurningSmoothing);

        // Update Velocity history
        VelocityHistory[velocityHistoryIndex] = Velocity.magnitude;
        velocityHistoryIndex = (velocityHistoryIndex + 1) % VelocityHistory.Length;


        // FIRING MISSILES
        fireTimer += dt;

        if (fireTimer > FireInterval && 
            playerDistance < PlayerFireDistance &&
            Vector3.Dot(towardsPlayerNormalized.normalized, transform.forward) > FireConeAngle)
        {
            fireTimer = 0f;
            FireMissile();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle") || 
            other.gameObject.CompareTag("Chopper") || 
            other.gameObject.CompareTag("Player") || 
            other.gameObject.CompareTag("Missile"))
        {
            if (other.gameObject.CompareTag("Chopper"))
            {
                other.gameObject.transform.root.GetComponent<ChopperController>().Explode();
            }

            if (other.gameObject.CompareTag("Missile"))
            {
                if (other.gameObject.transform.root.GetComponent<MissileController>().Parent == gameObject) return;

                other.gameObject.transform.root.GetComponent<MissileController>().Explode();
            }

            if (other.gameObject.CompareTag("Player"))
            {
                other.gameObject.transform.root.GetComponent<PlayerController>().Explode();
            }

            Explode();
        }

        if (other.gameObject.CompareTag("ChopperAvoid"))
        {
            // If they are the same, return
            if (other.gameObject.transform.root == gameObject.transform.root) return;

            // Set them as the avoidance target, until they go far away - only one target to avoid
            avoidTarget = other.gameObject.transform.parent.gameObject;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("ChopperAvoid"))
        {
            // If they are the same, return
            if (other.gameObject.transform.root == gameObject.transform.root) return;

            // Set them as the avoidance target, until they go far away - only one target to avoid
            avoidTarget = other.gameObject.transform.parent.gameObject;
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
        if (!Alive) return;

        // Unparent all objects in the model
        DestructionParts = new List<GameObject>();
        DestructionParts.AddRange(ModelReference.GetComponentsInChildren<MeshRenderer>().Select(t => t.gameObject));

        Alive = false;

        GameState.instance.ChoppersDestroyed += 1;

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

            var randomStartForce = Velocity/5f + Random.onUnitSphere * 5f;
            var randomStartTorque = Random.onUnitSphere * 10f;

            rb.AddForce(randomStartForce, ForceMode.Impulse);
            rb.AddTorque(randomStartTorque, ForceMode.Impulse);

            part.AddComponent<BoxCollider>();

            // Add custom gravity
            var pg = part.AddComponent<PlanetGravity>();
            pg.Gravity = 20f;
            pg.PlanetReference = PlanetReference;
        }

        var explosion = Instantiate(ExplosionPrefab);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = transform.rotation;

        ExplosionAudio.Play();
        ExplosionAudio.transform.parent = null;
        Destroy(ExplosionAudio, 3f);

        Destroy(gameObject);
    }

    private void FireMissile()
    {
        var missile = Instantiate(MissilePrefab);
        missile.GetComponent<MissileController>().PlanetReference = PlanetReference;

        missile.GetComponent<MissileController>().Velocity = Velocity;

        missile.transform.position = MissileSpawnAnchor.transform.position;
        missile.transform.rotation = MissileSpawnAnchor.transform.rotation;

        missile.GetComponent<MissileController>().Parent = gameObject;
    }
}
