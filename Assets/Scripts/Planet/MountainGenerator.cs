using UnityEngine;
using System.Collections;

public class MountainGenerator : MonoBehaviour
{
    public GameObject MountainBlockPrefab;
    public Material[] RandomMaterials;

    public int MountainBlockCount;
    public float RandomOffset;

    void Start() 
    {
        for (var i = 0; i < MountainBlockCount; ++i)
        {
            var mountain = Instantiate(MountainBlockPrefab);

            var randomMaterial = Random.Range(0, RandomMaterials.Length);
            mountain.GetComponent<MeshRenderer>().material = RandomMaterials[randomMaterial];

            mountain.transform.position = transform.position + Random.onUnitSphere * Random.Range(0f, RandomOffset);
            mountain.transform.rotation = Random.rotation;
            mountain.transform.localScale = new Vector3(
                Random.Range(0.5f, 1.2f) * mountain.transform.localScale.x,
                Random.Range(0.5f, 1.2f) * mountain.transform.localScale.y,
                Random.Range(0.5f, 1.2f) * mountain.transform.localScale.z
                );

            mountain.transform.parent = transform;
        }
    }
    
    void Update() 
    {
    
    }
}
