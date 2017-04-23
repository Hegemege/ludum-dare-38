using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CreditsUIController : MonoBehaviour 
{
    void Awake() 
    {

    }

    void Start() 
    {
    
    }
    
    void Update() 
    {
        if (Input.GetAxis("Escape") > 0.2f)
        {
            SceneManager.LoadScene("Main");
        }
        else if (Input.anyKeyDown)
        {
            if (Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1) ||
                Input.GetMouseButtonDown(2))
            {
                return;
            }
                SceneManager.LoadScene("Title");
        }
    }
}
