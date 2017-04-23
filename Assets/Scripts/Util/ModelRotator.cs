using UnityEngine;
using System.Collections;

public class ModelRotator : MonoBehaviour
{
    public float RotationSpeed;
    public Vector3 RotationAxis;
    
    void Update() 
    {
    
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        transform.localRotation *= Quaternion.AngleAxis(RotationSpeed * dt, RotationAxis);
    }
}
