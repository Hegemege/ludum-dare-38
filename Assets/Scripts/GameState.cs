using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour 
{
    // Handles game progress and difficulty

    [HideInInspector]
    public static GameState instance = null; // Singleton

    public int LevelIndex;

    public int EffectiveLevelIndex
    {
        get { return Mathf.Min(LevelIndex, Levels - 1); }
    }

    public int Levels;
    public int[] LevelPlatformBuildingWeights;
    public int[] LevelPlatformFarmWeights;
    public int[] LevelPlatformFarmLandWeights;
    public int[] LevelPlatformCount;
    public int[] LevelMountainCount;
    public int[] LevelChopperCount;
    public bool[] LevelRegenerateBuildings;
    public bool[] LevelRegenerateChoppers;

    // Progress stats
    public int PickedFarms;
    public int ChoppersDestroyed;

    void Awake() 
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
        }
        else if (!instance.Equals(this))
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    
    void Start() 
    {
    
    }
    
    void Update() 
    {
    
    }
}
