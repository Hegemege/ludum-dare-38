using UnityEngine;
using System.Collections;

public class SurfaceAlign : MonoBehaviour
{
    public GameObject PlanetReference;

    void Awake() 
    {

    }

    void Start() 
    {
        // Project current forward onto the plane defined by the surface normal
        var normal = (transform.position - PlanetReference.transform.position).normalized;
        var projectedForward = Vector3.ProjectOnPlane(transform.forward, normal);

        transform.rotation = Quaternion.LookRotation(projectedForward, normal);
    }
    
    void Update() 
    {
    
    }
}
