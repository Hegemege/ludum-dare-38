using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Image ChopperImage;
    public Text ChopperText;
    public Text MarkerText;

    void Awake() 
    {

    }

    void Start() 
    {
    
    }
    
    void Update()
    {
        var levelIndex = GameState.instance.EffectiveLevelIndex;
        
        // Get the texts from GameState
        var choppersDestroyed = GameState.instance.ChoppersDestroyed;
        var pickedMarkers = GameState.instance.PickedMarkers;

        var choppersMax = GameState.instance.MaxChoppers;
        var markersMax = GameState.instance.MaxMarkers;

        var markerText = pickedMarkers.ToString() + "/" + (GameState.instance.LevelRegenerateBuildings[levelIndex] ? "-" : markersMax.ToString());
        var chopperText = choppersDestroyed.ToString() + "/" + (GameState.instance.LevelRegenerateChoppers[levelIndex] ? "-" : choppersMax.ToString());

        // Chopper image disable
        ChopperImage.enabled = choppersMax != 0;
        ChopperText.enabled = choppersMax != 0;

        ChopperText.text = chopperText;
        MarkerText.text = markerText;
    }
}
