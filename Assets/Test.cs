using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
           PlayerPrefs.SetFloat("Quan", 3.13f);
            PlayerPrefs.SetInt("Int", 6);
            PlayerPrefs.SetString("a", "áas");
        }
    }
}
