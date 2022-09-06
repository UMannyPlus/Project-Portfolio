using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Starts the default camera and assigns the texture to the current renderer
public class WebcamExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WebCamTexture webCamTexture = new WebCamTexture();
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = webCamTexture;
        webCamTexture.Play();
    }
}
