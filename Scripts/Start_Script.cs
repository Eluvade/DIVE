using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Start_Script : MonoBehaviour
{
    public void ClickButton ()
    {
        SceneManager.LoadScene("Menu");
    }

}