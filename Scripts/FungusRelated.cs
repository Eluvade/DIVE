using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public static class GlobalFungus
{
    public static bool shuttingDownFlag = false;
    public static string playerName;
}
public class FungusRelated : MonoBehaviour {
    public GameObject Texto;
    public GameObject Input;
    public Flowchart flowchart;
    private int shuttingDownChecker;
    private int playerNameChecker;
    private bool trigger = false;
    void Update() {
        shuttingDownChecker = flowchart.GetIntegerVariable("shuttingDown");
        playerNameChecker = flowchart.GetIntegerVariable("waitPlayerName");
        if (shuttingDownChecker == 1)
        {
            Texto.SetActive(true);
        }
        if (playerNameChecker == 1)
        {
            Input.SetActive(true);
            trigger = true;
        }
        if(GlobalVariables.buttonFlag == true && trigger == true){
            Input.SetActive(false);
        }
    }

}