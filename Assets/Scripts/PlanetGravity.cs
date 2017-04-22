using UnityEngine;
using System.Collections;

public class PlanetGravity : MonoBehaviour
{
    [HideInInspector]
    public GameObject PlanetReference;

    [HideInInspector]
    public float DeathTimer;

    [HideInInspector]
    public float Gravity;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start() 
    {
        Destroy(gameObject, DeathTimer);
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
}
