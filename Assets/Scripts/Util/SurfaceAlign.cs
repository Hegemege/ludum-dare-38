using UnityEngine;
using System.Collections;

public class SurfaceAlign : MonoBehaviour
{
    public GameObject PlanetReference;

    public bool UpdateEveryFrame;

    void Start() 
    {
        AlignOnSurface();
    }

    private void AlignOnSurface()
    {
        // Project current forward onto the plane defined by the surface normal
        var normal = (transform.position - PlanetReference.transform.position).normalized;
        var projectedForward = Vector3.ProjectOnPlane(transform.forward, normal);

        transform.rotation = Quaternion.LookRotation(projectedForward, normal);
    }

    void FixedUpdate()
    {
        if (UpdateEveryFrame)
        {
            AlignOnSurface();
        }
    }
}
