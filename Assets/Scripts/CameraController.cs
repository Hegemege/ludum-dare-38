using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public GameObject FollowTarget;
    public GameObject PlanetReference;

    public float TargetDistance;
    public float TargetHeight;
    public float TargetAngle;

    void Awake()
    {

    }

    void Start()
    {

    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 towardsPlanet = PlanetReference.transform.position - transform.position;

        // Make the camera follow the target at thte given distance and height
        // Place us behind the player at the given distance

        transform.position = FollowTarget.transform.position - FollowTarget.transform.forward * TargetDistance;
        transform.position += -towardsPlanet.normalized * TargetHeight;

        // TODO: Add smoothing
        // Apply look rotation
        var towardsTarget = FollowTarget.transform.position - transform.position;
        var localUp = Quaternion.AngleAxis(-TargetAngle, FollowTarget.transform.right);
        transform.rotation = localUp * Quaternion.LookRotation(towardsTarget, -towardsPlanet);
    }
}
