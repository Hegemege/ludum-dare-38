using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions.Comparers;

public class CameraController : MonoBehaviour
{
    public GameObject FollowTarget;
    public GameObject PlanetReference;

    public float NormalFOV;
    public float BoostFOV;
    public float BrakeFOV;

    public float TargetDistance;
    public float TargetHeight;
    public float TargetAngle;

    public float TargetBoostDistance;
    public float TargetBoostHeight;
    public float TargetBoostAngle;

    public float TargetBrakeDistance;
    public float TargetBrakeHeight;
    public float TargetBrakeAngle;

    public float TransitionSmoothing;

    public float PositionSmoothing;
    public float RotationSmoothing;

    // Privates
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private PlayerController playerController;

    private List<Camera> cameras;

    private float effectiveDistance;
    private float effectiveHeight;
    private float effectiveAngle;
    private float effectiveFOV;

    void Awake()
    {
        playerController = FollowTarget.GetComponent<PlayerController>();
        cameras = new List<Camera>();
        cameras.AddRange(GetComponentsInChildren<Camera>());

        effectiveDistance = TargetDistance;
        effectiveHeight = TargetHeight;
        effectiveAngle = TargetAngle;
        effectiveFOV = NormalFOV;
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

        // Make the camera follow the target at the given distance and height
        // Place us behind the player at the given distance

        // Calculate effective distances etc
        var effectiveGoalDistance = TargetDistance;
        var effectiveGoalHeight = TargetHeight;
        var effectiveGoalAngle = TargetAngle;
        var effectiveGoalFOV = NormalFOV;

        if (playerController.Boosting)
        {
            effectiveGoalDistance = TargetBoostDistance;
            effectiveGoalHeight = TargetBoostHeight;
            effectiveGoalAngle = TargetBoostAngle;
            effectiveGoalFOV = BoostFOV;
        }
        else if (playerController.Braking)
        {
            effectiveGoalDistance = TargetBrakeDistance;
            effectiveGoalHeight = TargetBrakeHeight;
            effectiveGoalAngle = TargetBrakeAngle;
            effectiveGoalFOV = BrakeFOV;
        }

        // Smoothed change of distance, height or angle
        effectiveDistance = Mathf.Lerp(effectiveDistance, effectiveGoalDistance, TransitionSmoothing);
        effectiveHeight = Mathf.Lerp(effectiveHeight, effectiveGoalHeight, TransitionSmoothing);
        effectiveAngle = Mathf.Lerp(effectiveAngle, effectiveGoalAngle, TransitionSmoothing);
        effectiveFOV = Mathf.Lerp(effectiveFOV, effectiveGoalFOV, TransitionSmoothing);

        // Calculate target position
        targetPosition = FollowTarget.transform.position - FollowTarget.transform.forward * effectiveDistance;
        targetPosition += -towardsPlanet.normalized * effectiveHeight;

        // Calculate target rotation
        var towardsTarget = FollowTarget.transform.position - transform.position;
        var localUp = Quaternion.AngleAxis(-effectiveAngle, FollowTarget.transform.right);
        targetRotation = localUp * Quaternion.LookRotation(towardsTarget, -towardsPlanet);

        // Apply position + rotation with smoothing
        transform.position = Vector3.Slerp(transform.position, targetPosition, PositionSmoothing);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSmoothing);

        // Set FOV
        foreach (var childCamera in cameras)
        {
            childCamera.fieldOfView = effectiveFOV;
        }
    }
}
