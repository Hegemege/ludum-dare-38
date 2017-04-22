using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlanetGenerator : MonoBehaviour
{
    public GameObject PlanetReference;
    public GameObject TerrainReference;
    public GameObject PlatformsReference;

    public GameObject PlatformPrefab;

    public LayerMask PlatformLayerMask;

    public float PlanetDeformScale;
    public int PlanetDeformRuns;

    public Color[] PlanetTerrainColors;

    public int PlatformCount;
    public float DistanceBetweenPlatforms;
    public int MaxTries;

    // Privates
    private List<GameObject> platforms;

    void Awake() 
    {
        platforms = new List<GameObject>();
    }

    void Start()
    {
        DeformPlanet();
        GenerateTerrain();
        GeneratePlatforms();
    }

    /// <summary>
    /// Deforms the planet's surface a bit
    /// </summary>
    private void DeformPlanet()
    {
        Mesh planetMesh = PlanetReference.GetComponentsInChildren<MeshFilter>().Single().mesh;
        Vector3[] originalVertices = planetMesh.vertices;
        Vector3[] deformedVertices = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; ++i)
        {
            deformedVertices[i] = originalVertices[i];
        }

        // Deform using the sphere-split-in-half method
        for (int run = 0; run < PlanetDeformRuns; ++run)
        {
            var randomPlane = Random.onUnitSphere;
            
            for (int i = 0; i < originalVertices.Length; ++i)
            {
                var side = Mathf.Ceil(Vector3.Dot(deformedVertices[i], randomPlane)) * 2 - 1;

                var towardsCenter = (PlanetReference.transform.position - originalVertices[i]).normalized;
                deformedVertices[i] += towardsCenter * side * PlanetDeformScale;
            }

        }




        // Calculate vertex colors - get highest and lowest first
        var highest = Mathf.NegativeInfinity;
        var lowest = Mathf.Infinity;

        for (int i = 0; i < originalVertices.Length; ++i)
        {
            var distance = Vector3.Distance(PlanetReference.transform.position, deformedVertices[i]);
            if (distance > highest)
            {
                highest = distance;
            }

            if (distance < lowest)
            {
                lowest = distance;
            }
        }

        var range = highest - lowest;

        // Precalculate the color limits
        float[] limits = new float[PlanetTerrainColors.Length];
        for (int i = 0; i < limits.Length; ++i)
        {
            limits[i] = lowest + i * range / (float) limits.Length;
        }

        // Blend the colors based on the highest/lowest
        Color[] colors = new Color[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; ++i)
        {
            var distance = Vector3.Distance(PlanetReference.transform.position, deformedVertices[i]);

            int slotIndex = 0;
            Color startColor = Color.white;
            Color endColor = Color.white;

            // Figure which slot it fits in
            for (int j = 0; j < limits.Length - 1; ++j)
            {
                if (limits[j] < distance && distance < limits[j + 1])
                {
                    slotIndex = j;
                    startColor = PlanetTerrainColors[j];
                    endColor = PlanetTerrainColors[j + 1];
                    break;
                }
            }

            var colorBalance = (distance - limits[slotIndex]) / (limits[slotIndex + 1] - limits[slotIndex]);

            colors[i] = new Color(
                Mathf.Lerp(startColor.r, endColor.r, colorBalance),
                Mathf.Lerp(startColor.g, endColor.g, colorBalance),
                Mathf.Lerp(startColor.b, endColor.b, colorBalance),
                Mathf.Lerp(startColor.a, endColor.a, colorBalance)
                );

            //colors[i] = startColor;
        }

        planetMesh.vertices = deformedVertices;
        planetMesh.colors = colors;
        planetMesh.RecalculateBounds();
        planetMesh.RecalculateNormals();

        // Apply mesh
        PlanetReference.GetComponentsInChildren<MeshFilter>().Single().sharedMesh = planetMesh;

    }

    /// <summary>
    /// Generates the terrain of the planet
    /// </summary>
    private void GenerateTerrain()
    {

    }

    /// <summary>
    /// Generate platforms on the surface of the planet
    /// </summary>
    private void GeneratePlatforms()
    {
        var triedToGenerate = 0;
        var platformsLeft = PlatformCount;

        while (platformsLeft > 0)
        {
            // Can't generate any more
            if (triedToGenerate >= MaxTries) break;

            // Draw random point on the sphere, cast it to the surface
            var randPoint = Random.onUnitSphere;

            randPoint *= 300f; // Far enough away from the center
            Ray ray = new Ray(randPoint, -randPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, PlatformLayerMask))
            {
                randPoint = hit.point;
            }


            // Test if there are platofrms nearby 
            var regenerate = false;
            foreach (var platform in platforms)
            {
                if (Vector3.Distance(platform.transform.position, randPoint) < DistanceBetweenPlatforms)
                {
                    regenerate = true;
                    triedToGenerate += 1;
                    break;
                }
            }

            if (regenerate) continue;

            // Create the prefab
            triedToGenerate = 0;
            platformsLeft -= 1;

            var newPlatform = Instantiate(PlatformPrefab);
            newPlatform.transform.position = randPoint;
            newPlatform.GetComponent<SurfaceAlign>().PlanetReference = PlanetReference;

            newPlatform.transform.parent = PlatformsReference.transform;

            platforms.Add(newPlatform);
        }
    }
    
    void Update() 
    {
    
    }
}
