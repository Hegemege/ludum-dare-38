using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MissileController : MonoBehaviour 
{
    public GameObject PlanetReference;
    public GameObject ExplosionPrefab;

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
    public bool Alive;

    // Privates
    private Vector3 targetVelocity;

    void Awake()
    {
        Alive = true;
    }

    void Start()
    {
        // Initialize velocity as forward
        // TODO: Chopper should assign velocity
        Velocity = Airspeed * transform.forward * Time.fixedDeltaTime;

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

        // Copy-paste from player - the missile does not turn
        var HorizontalInput = 0f;
        var VerticalInput = 0f;

        // Turning
        newForward = Quaternion.AngleAxis(HorizontalInput * TurningSpeed * dt, -towardsPlanet) * newForward;

        // Air speed calculations
        var effectiveAirspeed = Airspeed;

        var newVelocity = Velocity + newForward * effectiveAirspeed;

        // Project velocity to be at constant distance from ground
        Vector3 groundPoint = GroundPosition(transform.position + newVelocity * dt);
        var planetToGround = (groundPoint - PlanetReference.transform.position).normalized;

        var normalizedTarget = groundPoint + planetToGround * DistanceFromGround;
        var normalizedVelocity = normalizedTarget - transform.position;

        // Apply projected velocity after smoothing
        Velocity = Vector3.Slerp(Velocity, normalizedVelocity, VelocitySmoothing);

        // Move the missile
        transform.position += Velocity * dt;

        // Rotate the missile to travel forward
        transform.rotation = Quaternion.LookRotation(Velocity, -towardsPlanet);
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
                other.gameObject.transform.root.GetComponent<MissileController>().Explode();
            }

            if (other.gameObject.CompareTag("Player"))
            {
                other.gameObject.transform.root.GetComponent<PlayerController>().Explode();
            }

            Explode();
        }
    }

    public void Explode()
    {
        if (!Alive) return;

        Alive = false;

        var explosion = Instantiate(ExplosionPrefab);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = transform.rotation;

        Destroy(gameObject);
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

        Debug.LogWarning("No ground under missile. Shouldn't happen.");
        return Vector3.zero;
    }

}
