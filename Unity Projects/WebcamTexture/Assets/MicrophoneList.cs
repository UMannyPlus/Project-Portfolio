using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneList : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var device in Microphone.devices)
            Debug.Log("Name: " + device);
    }
}
