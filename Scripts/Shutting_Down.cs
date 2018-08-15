using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Shutting_Down : MonoBehaviour {

    IEnumerator Lifetime()
    {
        float currentTime = 0;
        float step = 1f;

        while (currentTime < 10)
        {
            yield return new WaitForSeconds(step);
            currentTime += step;

        }
        GlobalFungus.shuttingDownFlag = true;
    }

    void Start()
    {
        StartCoroutine(Lifetime());
        StartCoroutine(TimeDelay());
    }

    void Update()
    {

        if (GlobalFungus.shuttingDownFlag == true)
        {
            gameObject.SetActive(false);
        }


    }
    public static bool finishedTyping = false;

    public float delay = 1f;
    private string fullText = "Shutting Down...";
    private string currentText = "";
    private int j = 0;

    IEnumerator TimeDelay()
    {
        float sec = 0;
        while (true)
        {
            yield return new WaitForSeconds(delay);
            sec += delay;
            StartCoroutine(ShowText());
            break;
        }
    }

    IEnumerator ShowText()
    {
        for (int i = 0; i <= fullText.Length - 3; i++)
        {
            currentText = fullText.Substring(0, i);
            this.GetComponent<Text>().text = currentText;
            yield return new WaitForSeconds(delay * (float)0.025);
        }
        while (j < 3)
        {
            for (int i = fullText.Length - 3; i <= fullText.Length; i++)
            {
                currentText = fullText.Substring(0, i);
                this.GetComponent<Text>().text = currentText;
                yield return new WaitForSeconds(delay * (float)0.25);

            }
            j = j + 1;
        }
        finishedTyping = true;
    }
}
