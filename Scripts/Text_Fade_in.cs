using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Text_Fade_in : MonoBehaviour {

    public Text clickToStart;

    public IEnumerator FadeTextToFullAlpha(float t, Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / 7 * t));
            yield return null;
        }
    }

    void Start ()
    {
        StartCoroutine(FadeTextToFullAlpha(2, clickToStart));
    }
}
