using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class PlayerController : MonoBehaviour
{
    public GameObject PlanetReference;
    public GameObject DropMarker;
    public GameObject ExplosionPrefab;

    // TMEP
    public GameObject debugSphere;

    // Self-references
    public GameObject ModelReference;
    public GameObject ParticleEffects;
    public float TurningSmoothing;
    public float TurningAngle;

    public AudioSource DestroyAudio;
    public AudioSource MotorAudio;
    public float StartAudioPitch;
    public float BoostAudioPitch;
    public float BrakeAudioPitch;

    // Input
    public float InputDeadzone;

    // Physics parameters
    public float Airspeed;
    public float TurningSpeed;
    public float VelocitySmoothing;

    public float BoostScale;
    public float BrakeScale;

    public float DistanceFromGround;

    public LayerMask GroundLayerMask;

    // Physics helpers
    [HideInInspector]
    public Vector3 Velocity;

    [HideInInspector]
    public bool Boosting;
    
    [HideInInspector]
    public bool Braking;

    [HideInInspector]
    public bool Alive;

    public float NextLevelAscentRate;
    public float SpawnHeight;
    public float SpawnDescentRate;
    public float EndHeight;
    private float effectiveDistanceFromGround;

    private bool endingLevel;

    // Privates
    private Quaternion modelRotationTarget;
    private Vector3 targetVelocity;

    private List<GameObject> DestructionParts;
    private Vector3 DestructionPoint;

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
        Alive = true;
        MotorAudio.pitch = StartAudioPitch;
    }

    void Start()
    {
        // Initialize velocity as forward
        Velocity = Airspeed * new Vector3(0f, 0f, 1f) * Time.fixedDeltaTime;

        // Move the player to be at the correct height
        var groundPoint = GroundPosition(transform.position);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;
        transform.position = groundPoint + planetToGround * SpawnHeight;

        effectiveDistanceFromGround = SpawnHeight;
    }

    void Update()
    {
        if (!Alive) return;

        if (Mathf.Abs(Input.GetAxis("Space")) > InputDeadzone)
        {
            // Debug purposes
            //Explode();
        }

        // Boost/brake handling
        Boosting = false;
        Braking = false;

        if (GameState.instance.AdvancingToNextLevel || GameState.instance.StartingNewLevel) return;

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
        if (GameState.instance.Paused) return;

        if (!Alive)
        {
            /*
            foreach (var part in DestructionParts)
            {
                if (!part)
                {
                    return;
                }
            }

            var positions = DestructionParts.Select(part => part.transform.position).ToList();

            Vector3 averagePosition = new Vector3();
            foreach (var position in positions)
            {
                averagePosition += position;
            }
            averagePosition /= positions.Count;

            // Sometimes the avg is NaN
            if (float.IsNaN(averagePosition.x)) averagePosition = DestructionPoint;

            transform.position = (averagePosition + DestructionPoint) / 2f;
            */
            if (!endingLevel)
            {
                endingLevel = true;
                var coroutine = EndLevelInSeconds(3f);
                StartCoroutine(coroutine);
            }
            
            return;
        }

        float dt = Time.fixedDeltaTime;
        // Helper vector towards the planet from the player
        Vector3 towardsPlanet = PlanetReference.transform.position - transform.position;

        // Input
        var newForward = Velocity.normalized;

        var horizontalInput = HorizontalInput;
        var verticalInput = VerticalInput;

        // If spawning to new level, block input
        if (GameState.instance.StartingNewLevel)
        {
            horizontalInput = 0f;
            verticalInput = 0f;

            effectiveDistanceFromGround -= dt * SpawnDescentRate;
        }

        // Enable input when player reaches altitude
        if (effectiveDistanceFromGround < DistanceFromGround)
        {
            GameState.instance.StartingNewLevel = false;
            effectiveDistanceFromGround = DistanceFromGround;
        }

        // If advancing to next level, block input
        if (GameState.instance.AdvancingToNextLevel)
        {
            horizontalInput = 0f;
            verticalInput = 0f;

            // Updates the height to ground, makes the player rise into the air
            effectiveDistanceFromGround += dt * NextLevelAscentRate;
        }

        // If reached altitude, end level
        if (GameState.instance.AdvancingToNextLevel)
        {
            if (effectiveDistanceFromGround > EndHeight)
            {
                GameState.instance.EndLevel = true;
            }
        }

        // Audio stuff
        if (Braking)
        {
            MotorAudio.pitch = Mathf.Lerp(MotorAudio.pitch, BrakeAudioPitch, 0.1f);
        }
        else if (Boosting)
        {
            MotorAudio.pitch = Mathf.Lerp(MotorAudio.pitch, BoostAudioPitch, 0.1f);
        }
        else
        {
            MotorAudio.pitch = Mathf.Lerp(MotorAudio.pitch, StartAudioPitch, 0.1f);
        }

        // Turning
        newForward = Quaternion.AngleAxis(horizontalInput * TurningSpeed * dt, -towardsPlanet) * newForward;

        // Air speed calculations
        var effectiveAirspeed = Airspeed;
        if (Boosting) effectiveAirspeed += Airspeed * BoostScale * Mathf.Abs(verticalInput);
        if (Braking) effectiveAirspeed += Airspeed * BrakeScale * Mathf.Abs(verticalInput);

        var newVelocity = Velocity + newForward * effectiveAirspeed;

        // Project velocity to be at constant distance from ground
        Vector3 groundPoint = GroundPosition(transform.position + newVelocity * dt);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;

        var normalizedTarget = groundPoint + planetToGround * effectiveDistanceFromGround;
        var normalizedVelocity = normalizedTarget - transform.position;

        // TEMP
        debugSphere.transform.position = GroundPosition(transform.position);

        // Move drop marker under the player
        DropMarker.transform.position = GroundPosition(transform.position);

        // Apply projected velocity after smoothing
        Velocity = Vector3.Slerp(Velocity, normalizedVelocity, VelocitySmoothing);

        // Move the plane
        transform.position += Velocity * dt;

        // Rotate the player to travel forward
        transform.rotation = Quaternion.LookRotation(Velocity, -towardsPlanet);

        // Rotate the player model when turning
        var turning = TurningAngle * -horizontalInput;
        modelRotationTarget = Quaternion.AngleAxis(turning, Vector3.forward);
        ModelReference.transform.localRotation = Quaternion.Slerp(ModelReference.transform.localRotation, modelRotationTarget, TurningSmoothing);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle") || 
            other.gameObject.CompareTag("Chopper") || 
            other.gameObject.CompareTag("Missile"))
        {
            if (other.gameObject.CompareTag("Chopper"))
            {
                other.gameObject.transform.root.GetComponent<ChopperController>().Explode();
            }

            if (other.gameObject.CompareTag("Missile"))
            {
                other.gameObject.transform.root.GetComponent<MissileController>().Explode();
            }

            Explode();
        }

        if (other.gameObject.CompareTag("PlatformLooseTrigger"))
        {
            other.gameObject.transform.parent.GetComponent<PlatformController>().Triggered = true;
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

        Debug.LogWarning("No ground under player. Shouldn't happen.");
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
        DestructionPoint = transform.position;

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
        
        // Spawn explosion
        var explosion = Instantiate(ExplosionPrefab);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = transform.rotation;

        // Disable colliders
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.tag = "Untagged";
            col.enabled = false;
        }

        // Disable dropmarker
        DropMarker.SetActive(false);

        // Disable particle system
        var particleSystems = ParticleEffects.GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in particleSystems)
        {
            var em = ps.emission;
            em.rateOverTime = 0f;
            Destroy(ps, ps.main.startLifetime.constant);
        }

        MotorAudio.Stop();
        DestroyAudio.Play();
    }

    IEnumerator EndLevelInSeconds(float sec)
    {
        yield return new WaitForSeconds(sec);

        GameState.instance.EndLevel = true;
    }
}
