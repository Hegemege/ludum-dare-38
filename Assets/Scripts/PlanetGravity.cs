using UnityEngine;
using System.Collections;

public class PlanetGravity : MonoBehaviour
{
    [HideInInspector]
    public GameObject PlanetReference;

    [HideInInspector]
    public float Gravity;

    private Rigidbody rb;

    private bool hitGround;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start() 
    {

    }
    
    void Update() 
    {
    
    }

    void FixedUpdate()
    {
        var towardsPlanet = PlanetReference.transform.position - transform.position;
        towardsPlanet.Normalize();
        towardsPlanet *= Gravity;

        rb.AddForce(towardsPlanet, ForceMode.Force);
    }

    void OnCollisionEnter(Collision other)
    {
        if (hitGround) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hitGround = true;
            Gravity /= 3f;
            rb.drag = 0.25f;
            rb.angularDrag = 0.5f;
        }
    }
}
