using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TitleUIController : MonoBehaviour
{
    // Hacks all around

    public RectTransform Scroll;
    public RectTransform PlaneScroll;

    public float ScrollSpeed;
    public float ScrollStop;

    private bool done;
    private bool pressedAnyKey;

    public GameObject Controls;

    public GameObject AnyKey;

    void Awake() 
    {
        Controls.SetActive(false);
        AnyKey.SetActive(false);
    }

    void Start() 
    {
    
    }
    
    void Update() 
    {

        if (Input.anyKey && !pressedAnyKey)
        {
            pressedAnyKey = true;
            SceneManager.LoadScene("Main");
        }

        if (Scroll.transform.position.x < ScrollStop)
        {
            Scroll.transform.position += new Vector3(ScrollSpeed * Time.deltaTime, 0f, 0f);
        }
        else
        {
            done = true;
        }

        if (done)
        {
            Controls.SetActive(true);
            AnyKey.SetActive(true);
        }
        
        PlaneScroll.transform.position += new Vector3(ScrollSpeed * Time.deltaTime, 0f, 0f);
    }
}
