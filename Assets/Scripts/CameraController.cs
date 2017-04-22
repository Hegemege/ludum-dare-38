using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public GameObject FollowTarget;
    public GameObject PlanetReference;

    public float TargetDistance;
    public float TargetHeight;
    public float TargetAngle;

    public float PositionSmoothing;
    public float RotationSmoothing;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

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

        targetPosition = FollowTarget.transform.position - FollowTarget.transform.forward * TargetDistance;
        targetPosition += -towardsPlanet.normalized * TargetHeight;

        // Apply look rotation
        var towardsTarget = FollowTarget.transform.position - transform.position;
        var localUp = Quaternion.AngleAxis(-TargetAngle, FollowTarget.transform.right);
        targetRotation = localUp * Quaternion.LookRotation(towardsTarget, -towardsPlanet);

        // Apply + smoothing
        transform.position = Vector3.Slerp(transform.position, targetPosition, PositionSmoothing);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSmoothing);
    }
}
