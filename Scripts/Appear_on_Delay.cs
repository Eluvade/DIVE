using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GlobalVariables{
    public static bool buttonFlag = false;
}

public class Appear_on_Delay : MonoBehaviour
{

    public GameObject startButton;

    IEnumerator TimerRoutineToZero()
    {
        float currentTime = 0;
        float step = 1f;

        while (currentTime < 4)
        {
            yield return new WaitForSeconds(step);
            currentTime += step;

        }
        GlobalVariables.buttonFlag = true;
    }
    void Start()
    {
        StartCoroutine(TimerRoutineToZero());
    }
    void Update()
    {

        if(GlobalVariables.buttonFlag == true)
        {
        startButton.SetActive(true);
        }


    }

}
