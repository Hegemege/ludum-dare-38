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

        // Split vertices - makes a new vertex for every triangle
        // From http://answers.unity3d.com/questions/798510/flat-shading.html
        Vector3[] oldVertices = deformedVertices;
        int[] triangles = planetMesh.triangles;
        Vector3[] newVertices = new Vector3[triangles.Length];

        for (int i = 0; i < triangles.Length; i++)
        {
            newVertices[i] = oldVertices[triangles[i]];
            triangles[i] = i;
        }
        planetMesh.vertices = newVertices;
        planetMesh.triangles = triangles;

        // Calculate vertex colors - get highest and lowest first
        var highest = Mathf.NegativeInfinity;
        var lowest = Mathf.Infinity;

        for (int i = 0; i < newVertices.Length; ++i)
        {
            var distance = Vector3.Distance(PlanetReference.transform.position, newVertices[i]);
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
        Color[] colors = new Color[newVertices.Length];

        for (int i = 0; i < newVertices.Length; ++i)
        {
            var distance = Vector3.Distance(PlanetReference.transform.position, newVertices[i]);

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
            
            // For color smoothing
            var colorBalance = (distance - limits[slotIndex]) / (limits[slotIndex + 1] - limits[slotIndex]);

            colors[i] = new Color(
                Mathf.Lerp(startColor.r, endColor.r, colorBalance),
                Mathf.Lerp(startColor.g, endColor.g, colorBalance),
                Mathf.Lerp(startColor.b, endColor.b, colorBalance),
                Mathf.Lerp(startColor.a, endColor.a, colorBalance)
                );
            

            //colors[i] = startColor;
        }

        // Go through all triangles, and set the vertex colors to be the average of the three
        for (var i = 0; i < planetMesh.triangles.Length - 3; i += 3)
        {
            var color1 = colors[i];
            var color2 = colors[i + 1];
            var color3 = colors[i + 2];

            var average = (color1 + color2 + color3) / 3f;

            colors[i] = average;
            colors[i + 1] = average;
            colors[i + 2] = average;
        }

        //planetMesh.vertices = deformedVertices;
        planetMesh.colors = colors;
        planetMesh.RecalculateBounds();
        planetMesh.RecalculateNormals();

        // Update collider
        PlanetReference.GetComponentInChildren<MeshCollider>().sharedMesh = planetMesh;
        PlanetReference.GetComponent<SphereCollider>().radius = lowest * 7500;
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
