using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Image ChopperImage;
    public Text ChopperText;
    public Text MarkerText;

    public Image LoadingImage;
    public Text LoadingText;

    public float FadeTime;
    private float fadeTimer;
    private float loadingImageAlpha;

    private bool wasPaused;

    void Awake() 
    {

    }

    void Start() 
    {
    
    }
    
    void Update()
    {
        if (GameState.instance.Paused)
        {
            LoadingImage.gameObject.SetActive(true);
            LoadingText.gameObject.SetActive(true);

            if (!wasPaused)
            {
                fadeTimer = 0f;
                wasPaused = true;
            }
        }
        else
        {
            if (wasPaused)
            {
                fadeTimer = 0f;
                wasPaused = false;
            }
            LoadingText.gameObject.SetActive(false);
        }

        fadeTimer += Time.deltaTime;

        // Set fade of the loadingImage
        if (!GameState.instance.Paused)
        {
            var fade = 1f - fadeTimer / FadeTime;
            fade = Mathf.Clamp(fade, 0f, 1f);

            if (fade < 0.01f)
            {
                LoadingImage.color = new Color(LoadingImage.color.r, LoadingImage.color.g, LoadingImage.color.b, 1f);
                LoadingImage.gameObject.SetActive(false);
            }
            else
            {
                LoadingImage.color = new Color(LoadingImage.color.r, LoadingImage.color.g, LoadingImage.color.b, fade);
            }
        }

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
