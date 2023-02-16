using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleLogBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // public void PrintMessageToConsole() {
    //     Debug.Log("Doing something.");
    // }

    public void Log(string msg) {
        Debug.Log(msg);
    }
}
