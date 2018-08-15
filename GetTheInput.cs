using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class GetTheInput : MonoBehaviour
{
    public Flowchart flowchart;
    public void GetInput(string guess)
    {
        Debug.Log("Should Work... x : " + guess);
        GlobalFungus.playerName = guess;
        flowchart.SetStringVariable("playerName", guess);
        flowchart.SetIntegerVariable("waitPlayerName", 0);
        GlobalVariables.buttonFlag = true;
    } 
    public void CallRegister()
    {
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", GlobalFungus.playerName);
        WWW www = new WWW("http://dive.foundation/username.php", form);
        yield return www;
        if (www.text == "0")
        {
            Debug.Log("Player successfully created.");
        }


    }
}
