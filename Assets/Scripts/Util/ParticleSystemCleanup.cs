using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleSystemCleanup : MonoBehaviour
{
    private List<ParticleSystem> particleSystems;

    void Awake() 
    {
        particleSystems = new List<ParticleSystem>();
        particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>());
    }

    void Start() 
    {
    
    }
    
    void Update()
    {
        var stayAlive = false;

        foreach (var ps in particleSystems)
        {
            if (ps.IsAlive())
            {
                stayAlive = true;
            }
        }

        if (!stayAlive) Destroy(gameObject);
    }
}
