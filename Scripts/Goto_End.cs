using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Goto_End : MonoBehaviour {

	// Use this for initialization
	public void EndScene()
    {
        SceneManager.LoadScene("End");
    }
}
