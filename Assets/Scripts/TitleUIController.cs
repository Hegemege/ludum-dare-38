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

        if (Input.anyKeyDown && !pressedAnyKey)
        {
            if (!(Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1) ||
                Input.GetMouseButtonDown(2)))
            {
                pressedAnyKey = true;
                SceneManager.LoadScene("Main");
            }   
        }

        if (Scroll.transform.localPosition.x < ScrollStop)
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
