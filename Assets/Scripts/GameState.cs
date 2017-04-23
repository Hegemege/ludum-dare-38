using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;

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
    public int MaxMarkers;
    public int PickedMarkers;
    public int ChoppersDestroyed;
    public int MaxChoppers;

    public bool AdvancingToNextLevel;
    public bool StartingNewLevel;

    public bool EndLevel;

    public bool Paused;

    public bool EndingLevel;

    void Awake()
    {
        // Singleton
        if (instance == null)
        {
            instance = this;
            SetupNewLevel();
        }
        else if (!instance.Equals(this))
        {
            SetupNewLevel();
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        StartingNewLevel = true;
    }
    
    void Update()
    {
        if (Input.GetAxis("Escape") > 0.1f && SceneManager.GetActiveScene().name != "Title")
        {
            SceneManager.LoadScene("Credits");
        }

        // Destroy this if we are back at title
        if (SceneManager.GetActiveScene().name == "Title")
        {
            instance = null;
            Destroy(gameObject);
        }

        if (Paused) return;

        // Advance to next level
        if (PickedMarkers == MaxMarkers && !LevelRegenerateBuildings[EffectiveLevelIndex])
        {
            AdvancingToNextLevel = true;
        }

        if (EndLevel)
        {
            NewLevel();
        }
    }

    public void SetupNewLevel()
    {
        instance.MaxMarkers = 0;
        instance.MaxChoppers = 0;
        instance.PickedMarkers = 0;
        instance.ChoppersDestroyed = 0;

        instance.AdvancingToNextLevel = false;
        instance.StartingNewLevel = true;
        instance.EndLevel = false;

        instance.EndingLevel = false;
    }

    private bool DidWin()
    {
        return PickedMarkers == MaxMarkers;
    }

    /// <summary>
    /// Starts a new level
    /// </summary>
    public void NewLevel()
    {
        if (EndingLevel) return;

        EndingLevel = true;

        if (DidWin())
        {
            LevelIndex += 1;

            // Go to credits first, then allow infinite battle
            if (LevelIndex == Levels - 1)
            {
                SceneManager.LoadScene("Credits");
                return;
            }
        }

        SceneManager.LoadSceneAsync("Main");
    }
}
