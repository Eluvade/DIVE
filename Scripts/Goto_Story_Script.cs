using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Goto_Story_Script : MonoBehaviour
{
    public void ClickButton()
    {
        SceneManager.LoadScene("Story");
    }

}