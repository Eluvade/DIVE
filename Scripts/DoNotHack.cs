using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DoNotHack : MonoBehaviour {
    public InputField nameField;

    public void CallRegister()
    {
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", nameField.text);
        WWW www = new WWW("http://dive.foundation/phpmyadmin/username.php");
        yield return www;
        if (www.text == "0");
        {
            Debug.Log("Player successfully created.");
        }


    }
  
   	
}
